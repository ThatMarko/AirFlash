# Credentials and the process boundary

Source reviewed on 2026-10-12 at own-fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, containing original stable `cf420d97700463c56adc31408510e8336e99e51c`. This page describes that integrated fork source, not an unmerged feature checkout or original upstream runtime.

WPF and Rust share no memory and expose no C-ABI. They communicate over redirected stdin/stdout JSONL v1. Rust owns its DPAPI credential files; WPF does not deserialize private keys. Groups E/F are integrated here; neither changes preferences schema 2 or creates a symmetric pipe-size contract.

## Process and pipes

[ExtractEngine](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/AppPaths.cs#L22) hashes embedded `AirFlash.Engine.exe` with SHA-256 and extracts it beneath the per-user local engine/hash directory. An existing target is reused only after matching its actual hash; replacement uses a unique temporary file, overwrite move and finally cleanup.

[ProcessEngineConnection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineConnection.cs#L26) starts a windowless child with redirected streams, UTF-8 stdin without BOM and UTF-8 output decoding. Its writer semaphore serializes complete command lines; native [send](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L34) holds stdout while writing/flushing an event. Request ids are generated per desktop command; session ids belong to the attempt.

| Direction | Actual guard |
| --- | --- |
| Desktop → native command | Native input reader bounds accumulated bytes at 65,536 **including LF and any CR**. It drains an oversized line through newline, then reports rejection. A final unterminated JSON line is parsed on EOF. Desktop sends have no separate size guard. |
| Native → desktop event | Native output is not size-capped. Desktop checks a completed stdout line against 65,536 **UTF-16 characters**, after `ReadLineAsync` has allocated it. The line terminator is removed by the reader. |

[native byte reader](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L111) and [desktop line reader](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineConnection.cs#L68) therefore do not enforce one universal 64 KiB limit. Bounded desktop allocation/general size hardening is outside the implemented error-routing fix.

Native envelope and typed peer/probe/EQ structures deny unknown fields. Commands with arbitrary `params`, such as gain/stop/PIN, are not uniformly schema-checked. Malformed/oversized envelopes use explicitly empty request/session strings. Parsed unsupported-version, unknown-command or invalid-start errors normally echo supplied ids.

## Process-owned rejection and correlation

[envelope](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L22) stamps an `error` without a code as version 1, `command_error`, null host, `ipc` channel and `retryable: false`. There is no invented `ipc_error` or `line_too_long` code.

[IsProcessRejection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineEvents.cs#L8) recognizes only the exact process-owned envelope: object, version 1, event `error`, code `command_error`, channel `ipc`, explicit false retryability, explicit null host and explicit empty string id/session. Startup [HelloAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineHandshake.cs#L6) and session [ReadEventAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L625) handle it before ordinary session filtering, preserving its detail and final classification. Each attempt owns its child, so an empty correlation is attributed to that process, without attributing a receiver fault.

Ordinary reads accept version 1 and matching session id, not a request-id match. Other-session/version events remain filtered within the original read budget. Non-object/malformed JSON child output becomes a controlled final `protocol_error` on `ipc`. Near-match envelopes do not become global failures. Current nonfatal `equalizer_error` is still an editor result, not a terminal process rejection.

[stderr drainage](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineConnection.cs#L45) retains only the first 4,096 decoded characters, applies [DiagnosticText.Protect](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/DiagnosticText.cs#L6) to known per-user paths and labelled secret/PIN/key values, and logs the protected text. Exit observation logs numeric/hex status, and startup failure text can include it. This is bounded pattern protection, not a guarantee to sanitize every possible unlabelled secret. Stdout remains reserved for events.

## Process and worker lifetime

Matching `stop` cancels/joins the worker; nonmatching stop still replies `stopped`. Neither worker completion nor stop/hello exits the native command loop. Valid replacement start/probe or parsed Pair drops/joins the prior worker; rejected replacement parameters leave it intact. EOF drops the worker and exits.

Desktop disposal closes stdin, waits two seconds, requests a process-tree kill if necessary, then awaits exit/stderr drainage. The two-second wait is one budget, not a hard total cleanup deadline. Playback retry creates a new process, JSONL session UUID, native clock and RTP identities. It is not RTP resume.

Normal startup now constructs a usable shell and uses [EngineInitialization](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineInitialization.cs#L14) for an asynchronous bounded hello probe with visible Ready/Failed/retry state. Hello opens no receiver/capture sockets. [hello](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L169) still reports version/mode/commands, `qualification: partial` and `production_ready: false`; the latter is informational, not a desktop behavior switch.

## What a pairing file contains

[credentials storage](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L21) hashes the `/info` id after removing `:` and ASCII-lowercasing; other separators and whitespace remain. The directory is per-user roaming `AirFlash/native-credentials`, or relative to the current directory if APPDATA is absent. Files start with `W2AP\x01` and current-user `CryptProtectData`, entropy `AirFlash HAP credentials v1`, UI forbidden and no machine scope.

[load_with](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L106) opens exactly once. Only initial `NotFound` means no saved file. An opened handle is read with a 16 KiB+1 bound; oversize, later read-stage `NotFound`, permission/I/O, magic, DPAPI, JSON and empty-identity failures remain errors. A path disappearing after open can still yield the valid opened object. Credentials contain byte-array ids and fixed 32-byte public/secret arrays; unknown payload fields are not explicitly denied.

[save](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L76) uses create-new unique temp, magic/encrypted bytes, `sync_all`, close and `MoveFileExW` replace/write-through. It attempts temp cleanup after failure. The previous destination is not truncated in place. Cleartext/DPAPI buffers and credential drop zeroization remain. Synthetic tests verify replacement; physical power-loss durability is unqualified. Legacy pyatv files are untouched.

The key remains accessory `/info deviceID`, not catalog id or manual endpoint. The authenticated HAP accessory id is retained inside the payload and verified later; it is not compared with the filename input at Pair save. Missing/empty/non-string playback `deviceID` still skips lookup and chooses transient. Failed lookup or pair-verify never switches to transient. See [Authentication](auth.md).

## Command handling

| Command | Effect |
| --- | --- |
| `hello` | Informational response; no receiver sockets; command loop remains alive. |
| `set_gain` | Matching stored worker validates/stores gain and replies; invalid gain is terminal command error. No match is ignored; Pair permits only zero. |
| `set_equalizer` | Matching stored start slot/positive sequence prepares a curve. Newer/full-identical replay succeeds with retained metadata; stale/conflict is nonfatal error. This is mailbox adoption, not DSP completion or worker liveness. |
| `set_device_volume` | Matching non-Pair worker queues valid percent/sequence; invalid payload gives nonfatal volume error; other targets ignored. |
| `stop` | Cancel/join matching worker, reply in all cases; process stays alive. |
| `pair` / `pair_pin` | Persistent pairing worker and its PIN channel; Pair parses a Peer but does not apply start's IPv4/nonzero-port validation. |
| `start` | Validated unbounded capture/transport path in [Engine](engine.md). |
| `probe` | Finite 1–5,000 ms, gain 0–0.1, EQ disabled, one/two distinct IPv4 peers and 44,100/48,000 Hz. |

The actual dispatcher is [main](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L175); a stored `Running` can outlive its worker. Process existence or matched control slot does not establish healthy audio.

## Source verification and coverage

[IPC failure fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/IpcFailureTests.cs#L21) assert exact envelope/detail, startup/playback/Pair/final retryability, near matches, malformed output, stale events, disposal and cancellation. Native [JSONL child tests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/tests/equalizer_ipc.rs#L9) and [retained-state serialized reply tests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L426) are receiver-free. [temporary-root loader tests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L203) and [method-selection tests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs#L793) cover genuine absence/error classifications and no verify fallback. [E](../issues/GROUP-E-ACCEPTANCE.md) and [F](../issues/GROUP-F-ACCEPTANCE.md) retain failing controls and exact executed sources.

The [continuation acceptance](../issues/CONTINUATION-ACCEPTANCE.md) separates executed verification from source inspection: exact-source main CI passed 667 Core, 124 Python and 69 native tests, with recording-device qualification and the thirty-minute localhost soak ignored. Complete packaged integration at `4ae1db4` passed six portable UI runs (1,059 assertions each), three actual 125% DPI runs (40 each) and disposable-runner installer checks; the runtime/package inputs at `8989b36` are identical and its final commit changes documentation only. This documentation refresh did not rerun those suites or use real endpoints, credentials, capture or receivers.

## Historical audit

Original `41190e0` used `Path::exists`, could hide lookup failure, discarded empty-correlated process errors in the desktop session filter and logged unbounded stderr lines without the current protection. E/F fix the first two; current stable-derived process diagnostics supply the bounded stderr/exit observations. The [previous pinned page](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/credentials-and-ipc.md) preserves the dated audit. Its “not integrated into fork main”, “proposal only” and pending reboot labels do not describe current main.