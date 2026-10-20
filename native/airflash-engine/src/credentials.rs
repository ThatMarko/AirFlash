//! Versioned user-bound DPAPI storage. Legacy pyatv files are never modified.
use crate::auth::Credentials;
use anyhow::{Context, Result, ensure};
use sha2::{Digest, Sha256};
use std::{
    io::{self, Read, Write},
    path::{Path, PathBuf},
};
use windows::{
    Win32::{
        Foundation::{HLOCAL, LocalFree},
        Security::Cryptography::*,
        Storage::FileSystem::{MOVEFILE_REPLACE_EXISTING, MOVEFILE_WRITE_THROUGH, MoveFileExW},
    },
    core::{HSTRING, w},
};
use zeroize::Zeroizing;
const MAGIC: &[u8] = b"W2AP\x01";
const PASSWORD_MAGIC: &[u8] = b"W2PW\x01";
const MAX_FILE_BYTES: usize = 16384;
pub fn directory() -> PathBuf {
    std::env::var_os("APPDATA")
        .map(PathBuf::from)
        .unwrap_or_default()
        .join("AirFlash/native-credentials")
}
pub fn path(root: &Path, id: &str) -> PathBuf {
    let normalized = id.replace(':', "").to_ascii_lowercase();
    let hash = Sha256::digest(normalized.as_bytes());
    root.join(format!(
        "{}.dpapi",
        hash.iter().map(|b| format!("{b:02x}")).collect::<String>()
    ))
}
fn protect(input: &[u8], encrypt: bool) -> Result<Zeroizing<Vec<u8>>> {
    let blob = CRYPT_INTEGER_BLOB {
        cbData: input.len().try_into()?,
        pbData: input.as_ptr() as *mut u8,
    };
    let entropy = b"AirFlash HAP credentials v1";
    let entropy = CRYPT_INTEGER_BLOB {
        cbData: entropy.len() as u32,
        pbData: entropy.as_ptr() as *mut u8,
    };
    let mut output = CRYPT_INTEGER_BLOB::default();
    unsafe {
        if encrypt {
            CryptProtectData(
                &blob,
                w!("AirFlash"),
                Some(&entropy),
                None,
                None,
                CRYPTPROTECT_UI_FORBIDDEN,
                &mut output,
            )?;
        } else {
            CryptUnprotectData(
                &blob,
                None,
                Some(&entropy),
                None,
                None,
                CRYPTPROTECT_UI_FORBIDDEN,
                &mut output,
            )?;
        }
        let data = Zeroizing::new(
            std::slice::from_raw_parts(output.pbData, output.cbData as usize).to_vec(),
        );
        if !encrypt {
            std::ptr::write_bytes(output.pbData, 0, output.cbData as usize);
        }
        LocalFree(Some(HLOCAL(output.pbData.cast())));
        Ok(data)
    }
}
pub fn save(root: &Path, id: &str, credentials: &Credentials) -> Result<()> {
    std::fs::create_dir_all(root)?;
    let clear = Zeroizing::new(serde_json::to_vec(credentials)?);
    let encrypted = protect(&clear, true)?;
    let target = path(root, id);
    let temp = root.join(format!("{}.tmp", uuid::Uuid::new_v4()));
    let result = (|| -> Result<()> {
        let mut file = std::fs::OpenOptions::new()
            .write(true)
            .create_new(true)
            .open(&temp)?;
        file.write_all(MAGIC)?;
        file.write_all(&encrypted)?;
        file.sync_all()?;
        drop(file);
        unsafe {
            MoveFileExW(
                &HSTRING::from(temp.as_os_str()),
                &HSTRING::from(target.as_os_str()),
                MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH,
            )
        }?;
        Ok(())
    })();
    if result.is_err() {
        let _ = std::fs::remove_file(&temp);
    }
    result
}
pub fn load(root: &Path, id: &str) -> Result<Option<Credentials>> {
    let target = path(root, id);
    read_opened_with(
        &target,
        "native credentials",
        |target| std::fs::File::open(target),
        decode_credentials,
    )
}
fn read_opened_with<R: Read, T>(
    target: &Path,
    kind: &str,
    open: impl FnOnce(&Path) -> io::Result<R>,
    decode: impl FnOnce(&[u8]) -> Result<T>,
) -> Result<Option<T>> {
    let file = match open(target) {
        Ok(file) => file,
        Err(error) if error.kind() == io::ErrorKind::NotFound => return Ok(None),
        Err(error) => return Err(error).with_context(|| format!("cannot open {kind}")),
    };
    // Only the initial open establishes absence. Read this object once, without
    // a metadata/path reread, and bound a file that can grow after it is opened.
    let mut data = Vec::new();
    file.take((MAX_FILE_BYTES + 1) as u64)
        .read_to_end(&mut data)
        .with_context(|| format!("cannot read {kind}"))?;
    ensure!(data.len() <= MAX_FILE_BYTES, "{kind} file too large");
    decode(&data).map(Some)
}
fn decode_credentials(data: &[u8]) -> Result<Credentials> {
    ensure!(data.starts_with(MAGIC), "unsupported credential format");
    let clear = protect(&data[MAGIC.len()..], false)
        .context("cannot decrypt native credentials for this Windows user")?;
    let credentials: Credentials = serde_json::from_slice(&clear)?;
    ensure!(
        !credentials.controller_id.is_empty() && !credentials.accessory_id.is_empty(),
        "empty credential identity"
    );
    Ok(credentials)
}
pub fn save_password(root: &Path, id: &str, password: &str) -> Result<()> {
    std::fs::create_dir_all(root)?;
    let encrypted = protect(password.as_bytes(), true)?;
    let target = path(root, id).with_extension("password.dpapi");
    let temp = root.join(format!("{}.tmp", uuid::Uuid::new_v4()));
    let result = (|| -> Result<()> {
        let mut file = std::fs::OpenOptions::new()
            .write(true)
            .create_new(true)
            .open(&temp)?;
        file.write_all(PASSWORD_MAGIC)?;
        file.write_all(&encrypted)?;
        file.sync_all()?;
        drop(file);
        unsafe {
            MoveFileExW(
                &HSTRING::from(temp.as_os_str()),
                &HSTRING::from(target.as_os_str()),
                MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH,
            )
        }?;
        Ok(())
    })();
    if result.is_err() {
        let _ = std::fs::remove_file(&temp);
    }
    result
}
pub fn load_password(root: &Path, id: &str) -> Result<Option<Zeroizing<String>>> {
    let target = path(root, id).with_extension("password.dpapi");
    read_opened_with(
        &target,
        "stored password",
        |target| std::fs::File::open(target),
        decode_password,
    )
}
fn decode_password(data: &[u8]) -> Result<Zeroizing<String>> {
    ensure!(
        data.starts_with(PASSWORD_MAGIC),
        "unsupported password format"
    );
    let clear = protect(&data[PASSWORD_MAGIC.len()..], false)?;
    // Borrow while validating: FromUtf8Error would retain an unzeroized plaintext
    // Vec on its error path. Utf8Error contains only validation offsets.
    let text = std::str::from_utf8(&clear).context("invalid stored password")?;
    Ok(Zeroizing::new(text.to_owned()))
}
#[cfg(test)]
mod tests {
    use super::*;
    use std::{cell::Cell, io::Cursor, rc::Rc};

    struct TemporaryRoot(PathBuf);
    impl TemporaryRoot {
        fn new() -> Self {
            let root = std::env::temp_dir()
                .join(format!("airflash-storage-test-{}", uuid::Uuid::new_v4()));
            std::fs::create_dir(&root).unwrap();
            Self(root)
        }
    }
    impl Drop for TemporaryRoot {
        fn drop(&mut self) {
            let _ = std::fs::remove_dir_all(&self.0);
        }
    }
    fn synthetic_credentials() -> Credentials {
        Credentials {
            accessory_id: b"synthetic-accessory".to_vec(),
            accessory_public: [11; 32],
            controller_id: b"synthetic-controller".to_vec(),
            controller_secret: [22; 32],
        }
    }
    fn expect_error<T>(result: Result<T>) -> anyhow::Error {
        match result {
            Err(error) => error,
            Ok(_) => panic!("storage unexpectedly succeeded"),
        }
    }
    fn write_payload(target: &Path, magic: &[u8], clear: &[u8]) {
        let encrypted = protect(clear, true).unwrap();
        let mut bytes = magic.to_vec();
        bytes.extend_from_slice(&encrypted);
        std::fs::write(target, bytes).unwrap();
    }
    fn assert_io(error: &anyhow::Error, kind: io::ErrorKind, context: &str) {
        assert_eq!(error.downcast_ref::<io::Error>().unwrap().kind(), kind);
        let fault = crate::transport::Fault::from_error("127.0.0.1", "setup", error);
        let wire = serde_json::to_string(&fault.event()).unwrap();
        let event: serde_json::Value = serde_json::from_str(&wire).unwrap();
        assert_eq!(event["channel"], "setup");
        assert_eq!(event["code"], "socket_error");
        assert_eq!(event["retryable"], true);
        assert!(event["message"].as_str().unwrap().contains(context));
        assert!(event["message"].as_str().unwrap().contains("synthetic"));
    }
    #[test]
    fn only_initial_open_not_found_is_absence_for_both_stores() {
        for label in ["native credentials", "stored password"] {
            for kind in [
                io::ErrorKind::NotFound,
                io::ErrorKind::PermissionDenied,
                io::ErrorKind::Other,
            ] {
                let opens = Cell::new(0);
                let decoded = Cell::new(false);
                let result = read_opened_with(
                    Path::new("synthetic.dpapi"),
                    label,
                    |_| -> io::Result<Cursor<Vec<u8>>> {
                        opens.set(opens.get() + 1);
                        Err(io::Error::new(kind, "synthetic open error"))
                    },
                    |_| {
                        decoded.set(true);
                        Ok(())
                    },
                );
                assert_eq!(opens.get(), 1);
                assert!(!decoded.get());
                if kind == io::ErrorKind::NotFound {
                    assert!(result.unwrap().is_none());
                } else {
                    assert_io(&expect_error(result), kind, &format!("cannot open {label}"));
                }
            }
        }
    }
    struct ReadFailure(io::ErrorKind);
    impl Read for ReadFailure {
        fn read(&mut self, _: &mut [u8]) -> io::Result<usize> {
            Err(io::Error::new(self.0, "synthetic read error"))
        }
    }
    #[test]
    fn opened_read_errors_including_not_found_never_become_absence() {
        for label in ["native credentials", "stored password"] {
            for kind in [
                io::ErrorKind::NotFound,
                io::ErrorKind::PermissionDenied,
                io::ErrorKind::Other,
            ] {
                let opens = Cell::new(0);
                let error = expect_error(read_opened_with(
                    Path::new("synthetic.dpapi"),
                    label,
                    |_| {
                        opens.set(opens.get() + 1);
                        Ok(ReadFailure(kind))
                    },
                    |_| -> Result<()> { panic!("failed read must not decode") },
                ));
                assert_eq!(opens.get(), 1);
                assert_io(&error, kind, &format!("cannot read {label}"));
            }
        }
    }
    struct GrowingReader(Rc<Cell<usize>>);
    impl Read for GrowingReader {
        fn read(&mut self, bytes: &mut [u8]) -> io::Result<usize> {
            bytes.fill(0);
            self.0.set(self.0.get() + bytes.len());
            Ok(bytes.len())
        }
    }
    #[test]
    fn read_bound_accepts_the_limit_and_rejects_growth_before_decoding() {
        for label in ["native credentials", "stored password"] {
            let accepted = read_opened_with(
                Path::new("synthetic.dpapi"),
                label,
                |_| Ok(Cursor::new(vec![0; MAX_FILE_BYTES])),
                |bytes| Ok(bytes.len()),
            )
            .unwrap();
            assert_eq!(accepted, Some(MAX_FILE_BYTES));
            let read = Rc::new(Cell::new(0));
            let decoded = Cell::new(false);
            let error = expect_error(read_opened_with(
                Path::new("synthetic.dpapi"),
                label,
                |_| Ok(GrowingReader(read.clone())),
                |_| {
                    decoded.set(true);
                    Ok(())
                },
            ));
            assert_eq!(read.get(), MAX_FILE_BYTES + 1);
            assert!(!decoded.get());
            assert!(error.to_string().contains("file too large"));
        }
    }
    #[test]
    fn opened_object_survives_path_replacement_for_both_decoders() {
        let root = TemporaryRoot::new();
        let hap = path(&root.0, "synthetic-info-id");
        let password = hap.with_extension("password.dpapi");
        save(&root.0, "synthetic-info-id", &synthetic_credentials()).unwrap();
        save_password(&root.0, "synthetic-info-id", " raw-päss-12-34 ").unwrap();
        let open = |target: &Path| -> io::Result<std::fs::File> {
            let file = std::fs::File::open(target)?;
            std::fs::rename(target, target.with_extension("opened-original"))?;
            std::fs::write(target, b"replacement must not be read")?;
            Ok(file)
        };
        let credentials = read_opened_with(&hap, "native credentials", open, decode_credentials)
            .unwrap()
            .unwrap();
        assert_eq!(credentials.accessory_id, b"synthetic-accessory");
        let loaded = read_opened_with(&password, "stored password", open, decode_password)
            .unwrap()
            .unwrap();
        assert_eq!(&**loaded, " raw-päss-12-34 ");
        assert!(load(&root.0, "synthetic-info-id").is_err());
        assert!(load_password(&root.0, "synthetic-info-id").is_err());
    }
    #[test]
    fn public_loaders_preserve_absence_directory_and_format_failures() {
        let root = TemporaryRoot::new();
        assert!(load(&root.0, "synthetic").unwrap().is_none());
        assert!(load_password(&root.0, "synthetic").unwrap().is_none());
        let hap = path(&root.0, "synthetic");
        let password = hap.with_extension("password.dpapi");
        for target in [&hap, &password] {
            std::fs::create_dir(target).unwrap();
        }
        assert!(load(&root.0, "synthetic").is_err());
        assert!(load_password(&root.0, "synthetic").is_err());
        for target in [&hap, &password] {
            std::fs::remove_dir(target).unwrap();
            std::fs::write(target, b"unsupported").unwrap();
        }
        assert!(
            expect_error(load(&root.0, "synthetic"))
                .to_string()
                .contains("credential format")
        );
        assert!(
            expect_error(load_password(&root.0, "synthetic"))
                .to_string()
                .contains("password format")
        );
    }
    #[test]
    fn encrypted_payload_parse_identity_tamper_and_utf8_failures_are_errors() {
        let root = TemporaryRoot::new();
        let target = path(&root.0, "synthetic");
        write_payload(&target, MAGIC, b"not-json");
        assert!(expect_error(load(&root.0, "synthetic")).is::<serde_json::Error>());
        for accessory in [false, true] {
            let mut credentials = synthetic_credentials();
            if accessory {
                credentials.accessory_id.clear();
            } else {
                credentials.controller_id.clear();
            }
            save(&root.0, "synthetic", &credentials).unwrap();
            assert!(
                expect_error(load(&root.0, "synthetic"))
                    .to_string()
                    .contains("empty credential identity")
            );
        }
        let password = target.with_extension("password.dpapi");
        write_payload(&password, PASSWORD_MAGIC, b"synthetic-secret\xff");
        let error = expect_error(load_password(&root.0, "synthetic"));
        assert!(error.is::<std::str::Utf8Error>());
        assert!(!format!("{error:?}").contains("synthetic-secret"));
        for (target, magic) in [(&target, MAGIC), (&password, PASSWORD_MAGIC)] {
            write_payload(target, magic, b"synthetic");
            let mut bytes = std::fs::read(target).unwrap();
            *bytes.last_mut().unwrap() ^= 1;
            std::fs::write(target, bytes).unwrap();
        }
        assert!(load(&root.0, "synthetic").is_err());
        assert!(load_password(&root.0, "synthetic").is_err());
    }
    #[test]
    fn normalized_accessory_keys_atomic_replacement_and_separate_passwords_survive() {
        let root = TemporaryRoot::new();
        let mut credentials = synthetic_credentials();
        save(&root.0, "AA:BB:CC:DD:EE:01", &credentials).unwrap();
        credentials.controller_id = b"replacement-controller".to_vec();
        save(&root.0, "aabbccddee01", &credentials).unwrap();
        save_password(&root.0, "AA:BB:CC:DD:EE:01", " exact-raw-pässword ").unwrap();
        assert_eq!(
            load(&root.0, "aabbccddee01")
                .unwrap()
                .unwrap()
                .controller_id,
            b"replacement-controller"
        );
        assert_eq!(
            &**load_password(&root.0, "aabbccddee01").unwrap().unwrap(),
            " exact-raw-pässword "
        );
        assert_eq!(std::fs::read_dir(&root.0).unwrap().count(), 2);
    }
    #[test]
    fn passwords_are_user_bound_encrypted_and_separate_from_pairing_credentials() {
        let root =
            std::env::temp_dir().join(format!("airflash-secret-test-{}", uuid::Uuid::new_v4()));
        save_password(&root, "AA:BB:CC:DD:EE:01", "synthetic-password").unwrap();
        let file = path(&root, "aabbccddee01").with_extension("password.dpapi");
        let bytes = std::fs::read(file).unwrap();
        assert!(!bytes.windows(18).any(|b| b == b"synthetic-password"));
        assert_eq!(
            &**load_password(&root, "aabbccddee01").unwrap().unwrap(),
            "synthetic-password"
        );
        assert!(load(&root, "aabbccddee01").unwrap().is_none());
        std::fs::remove_dir_all(root).unwrap();
    }
    #[test]
    fn dpapi_roundtrip_and_tamper() {
        let original = b"synthetic-test-key";
        let protected = protect(original, true).unwrap();
        assert_eq!(protect(&protected, false).unwrap().as_slice(), original);
        let mut damaged = protected.to_vec();
        let end = damaged.len() - 1;
        damaged[end] ^= 1;
        assert!(protect(&damaged, false).is_err());
    }
}
