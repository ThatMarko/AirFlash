# Preserve authentication errors and validate pairing input consistently

Credential and stored-password loading currently probes file existence before reading. An uncertain-presence error can be treated as absence, and separate metadata/read operations do not bound the object actually read. Pairing also stops consuming engine events while waiting for PIN/password entry, so a terminal rejection or engine exit can leave the prompt open until user action or timeout.

This change opens each selected credential file once and performs a bounded read. Only initial-open NotFound establishes absence; permission, read, decryption, format and identity failures remain errors. Authentication selection is lazy and mutually exclusive: saved HAP credentials select verify, otherwise the stored password or existing transient path is selected. Failures never trigger an alternate authentication exchange. Current-user DPAPI, accessory deviceID keys, file formats and atomic writes are preserved.

Pairing retains exactly one pending event reader while the input prompt is open. Terminal IPC/scoped errors, EOF, malformed output, unexpected Stop and completed pairing cancel and join the prompt before cleanup or progression. Successful input keeps the pending read, avoiding cancellation-related reply loss. Error detail/nonretryability and raw passwords are retained. The existing accepted ASCII dashed-PIN behavior is consolidated into one enablement/submission helper; upstream already accepts dashed PINs, so that consolidation is not presented as a new PIN bug fix.

Password cancellation follows the actual engine prompt, preserving valid nonempty whitespace passwords instead of silently cancelling them. Dialog enablement and final submission share the native contract: strict UTF-8, 1–256 bytes, no CR/LF/NUL, without trimming. The existing 128-unit editing bound remains. This rejects multibyte values such as 86 CJK characters that previously enabled Pair but exceeded native's byte limit. PIN whitespace still cancels.

Both paths also compare the plaintext getter length with a secure copy, disposed after comparison. WPF can return only the prefix before NUL while retaining the complete secret internally; a mismatch is rejected instead of silently authenticating with the shortened value. The separately qualified integrated modal regressions assert both lengths and reject disabled and forced submission without sending credentials. This standalone head contains the production guard and pure regressions, but does not yet carry the new actual-modal fixtures or focused pairing/password routes. Port and qualify those fixtures before submitting this standalone candidate.

Validation:

- 535 Core tests and locked Release solution build passed with zero warnings/errors.
- Twelve whitespace-password regressions failed on unchanged Core with 29 healthy controls, then all 41 focused pairing controls passed. Pure dialog-predicate tests cover exact UTF-8 boundaries, malformed UTF-16, whitespace and forbidden characters. The integrated composition separately passes 965 Core and actual modal checks; those integration counts are not standalone branch executions.
- The final integrated [Windows desktop qualification at `0533cbf`](https://github.com/ThatMarko/AirFlash/actions/runs/39517562675) passes 18 cells / 10,914 assertions, including actual modal password/PIN, forced NUL submission rejection, clipboard and seven full regional runs. Its dialog/helper production blobs match this branch; its larger integration counts do not replace the standalone 535-test result.
- New signal-gated pairing controls cover terminal/EOF/Stop/malformed input, prompt cleanup failure, input timeout, stale near-match events, exact payloads, leader-first member pairing and one reader. A deterministic simultaneous-completion case requires the already observed terminal error to win over valid input without submission.
- The native tree is unchanged from [exact-source Windows qualification at `932f7a5`](https://github.com/ThatMarko/AirFlash/actions/runs/39480760202), which passed 97 native tests with two intentional device/soak ignores. No new native execution is implied by the later managed-only commits.
- Credential fixtures use synthetic temporary roots and injected I/O failures. No personal credential store, real pairing or receiver is used. Physical UI/DPI and real-device qualification remain outside this standalone result.

Verified source base: `439de6260e01efbfca528a12cf4d4c1010b3583e`. Proposed head: `ed0347563a9119106e6afec2e67ff13cf276eca6`.

Fixes #24
