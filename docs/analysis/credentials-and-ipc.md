# Credentials and the process boundary

The WPF app and `airflash-engine.exe` share no memory. Pairing state crosses the boundary as JSONL commands in one direction and as DPAPI files on disk in the other. The session commands themselves are in [Session](session.md). The handshake that consumes a credential file is in [Engine](engine.md).

## Process and pipes

[`AppPaths.ExtractEngine`](../../desktop/AirFlash.App/Services/AppPaths.cs) reads the embedded resource `AirFlash.Engine.exe`, hashes it with SHA-256, and writes `%LOCALAPPDATA%\AirFlash\engine\<hash>\airflash-engine.exe` when that hash is not already on disk. The write goes to a temporary file in the same directory and then `File.Move` with overwrite.

[`ProcessEngineConnection`](../../desktop/AirFlash.Core/EngineConnection.cs) starts that executable with stdin, stdout, and stderr redirected and with no window. Stdin and stdout use UTF-8 without a BOM. Each command is one JSON line. A `SemaphoreSlim` allows one `SendAsync` at a time. `ReadAsync` takes the next stdout line.

[`main.rs`](../../native/airflash-engine/src/main.rs) reads until a newline and rejects a line longer than 64 KiB before parsing it. The event is `error` with the message `IPC command exceeds 64KiB`. `send` then stamps any error that has no `code` yet: `code` becomes `command_error`, `channel` becomes `ipc`, and `retryable` becomes false. A JSON value that fails `deny_unknown_fields`, a `version` other than 1 (`unsupported IPC version`), and an unknown command (`unknown command`) take that same stamp. There is no `ipc_error` or `line_too_long` code. Events always include `version`, `id`, and `session_id`. The session reader returns a line only when `version` is 1 and `session_id` matches the attempt. Other lines stay in the stdout loop until one matches or the read budget expires.

Stdout is reserved for these events. Diagnostics that must not contain keys or PINs go to stderr, and the desktop appends stderr to the WPF log.

Shutdown closes stdin, waits two seconds, and calls `Process.Kill(true)` if the process is still running. The engine treats EOF as cancellation of the running worker. `stop` cancels only when the session id matches, then replies `stopped`. A new `start` or `pair` drops the previous worker first. The desktop still uses a new process per attempt, so a retry does not reuse sockets.

The `hello` command is used once at startup, before discovery `Start`. The desktop requires `event` equal to `hello` and `version` equal to 1, then reads `engine_version`. The engine also reports `mode: native`, `qualification: partial`, `live_loopback: true`, `production_ready: false`, and the command list `hello`, `start`, `probe`, `stop`, `set_gain`, `set_equalizer`, `set_device_volume`, `pair`, `pair_pin`. That process exits before any receiver is contacted. `production_ready: false` is the engine's own label. The desktop does not branch on it.

## What a pairing file contains

[`credentials.rs`](../../native/airflash-engine/src/credentials.rs) stores one file per accessory id:

```text
%APPDATA%\AirFlash\native-credentials\<sha256>.dpapi
```

The hash input is the id with `:` removed and forced to lowercase. The file begins with the five bytes `W2AP\x01`. The remainder is `CryptProtectData` output for the current user, with the entropy bytes `AirFlash HAP credentials v1`. `load` rejects a file larger than 16384 bytes, a bad magic prefix, a decrypt failure, or empty controller or accessory ids. The decrypted body is JSON for `Credentials`: a 32-byte controller secret, a 32-byte accessory public key, and the two id strings.

`save` writes `.<uuid>.tmp` in the same directory, flushes, `sync_all`s, then `MoveFileExW` with `MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH`. On failure it deletes the temp file. The destination is replaced by that move. It is not opened and truncated in place. A crash before the move leaves the previous file in place and may leave a temp file if the error path did not run. The code does not read or modify legacy pyatv credential files.

The id in the filename is `deviceID` from `GET /info` during `pair`. The desktop catalog id, including a migrated alias, is not an input. A speaker whose `/info` identity changes does not find the old file.

`pair` writes the file and does not turn on the playback cipher. The following `start` calls `credentials::load` and, when the file exists, runs pair-verify. When it does not, `start` runs transient pair-setup with PIN `3939` and does not write a file. See [Engine](engine.md).

## Commands that do not open a HomePod socket

| Command | Effect |
| --- | --- |
| `hello` | Returns the self-description above and exits the command. No sockets |
| `set_gain` | Stores master PCM gain for the matching live session |
| `set_equalizer` | Replaces equalizer coefficients for the matching live session, or returns `equalizer_error` |
| `set_device_volume` | Queues a percent for the volume thread. Ignored when the session id does not match |
| `stop` | Cancels the matching session |
| `pair` / `pair_pin` | One pairing connection, then the process has no playback session |
| `start` | The playback handshake in [Engine](engine.md) |

`set_gain` and `set_equalizer` are rejected when no live session matches. They do not open a process by themselves. The desktop opens the process with `start` or `pair` and sends the others on the same stdin.
