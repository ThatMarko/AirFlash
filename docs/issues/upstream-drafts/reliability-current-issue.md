# Track remaining desktop and engine reliability safeguards against current Main

Reviewed source: `439de6260e01efbfca528a12cf4d4c1010b3583e`. This tracking issue covers remaining source-confirmed safeguards after the recent Main integrations. It links the existing intent issue #14 and authentication issue #24 rather than recreating them. SDK preflight, Settings scrolling, MSI payload refresh, configuration recovery, passive discovery retention, and process IPC/EQ preparation fixes already integrated into Main should remain intact.

Several independent failure paths remain:

- Delayed automatic admission/settings work can outlive current receiver or user intent. An owned active transport must keep usable Stop controls while passive discovery is absent, without permitting stale work to replace a newer session.
- A DNS resolve slot has no finite attempt deadline/retry while its advertisement remains present. Resource ownership must stay distinct from logical timeout/cancellation and cached TTL.
- Credential/password existence probing can collapse uncertain presence into absence. Pairing also needs to keep consuming terminal engine events while an input prompt is open and preserve exact accepted passwords consistently.
- A null software-group ID escapes controlled settings validation, and some direct hello/query consumers can lose the defined terminal IPC failure classification.
- Local endpoint mute state is remembered only in memory. A crash can leave an unresolved restore obligation; any recovery design must preserve exact endpoint/prior state and avoid overwriting ambiguous information.
- Monitor has no action for the existing combined diagnostics-copy command; capture underruns lack bounded output-frame padding accounting. Packaging has separate cleanup-boundary, preview-name and ordered PATH-host follow-ups.

Source entry points on the reviewed Main: [session lifecycle](https://github.com/Ding-Kyoma/AirFlash/blob/439de6260e01efbfca528a12cf4d4c1010b3583e/desktop/AirFlash.Core/SessionController.cs), [discovery](https://github.com/Ding-Kyoma/AirFlash/blob/439de6260e01efbfca528a12cf4d4c1010b3583e/desktop/AirFlash.App/Services/WindowsDiscovery.cs), [settings validation](https://github.com/Ding-Kyoma/AirFlash/blob/439de6260e01efbfca528a12cf4d4c1010b3583e/desktop/AirFlash.Core/Settings.cs), [credential loading](https://github.com/Ding-Kyoma/AirFlash/blob/439de6260e01efbfca528a12cf4d4c1010b3583e/native/airflash-engine/src/credentials.rs), and [capture pulls](https://github.com/Ding-Kyoma/AirFlash/blob/439de6260e01efbfca528a12cf4d4c1010b3583e/native/airflash-engine/src/live.rs).

These are source-derived failure paths backed by isolated synthetic regressions. Specific controls reproduced delayed ownership, the null-ID exception, lost IPC classification, input/terminal-event races, password whitespace/byte-boundary/truncation disagreement, and safe packaging failures. DNS timeout/ownership and crash recovery are tested with fake services, clocks, temporary journals and synthetic helper processes. This is not a reproduction of a particular HomePod/network incident or physical crash.

Acceptance:

- Preserve current schema-4 migration/unknown fields, software stereo, application capture/standby, selected-adapter pause, password support, language catalogs and JSONL v1.
- Serialize owner/intent validation with admission; reject stale work without undoing accepted persistence or consuming automatic candidates. Keep explicit manual endpoint/port/preferences authoritative.
- Bound DNS attempts, retries and unsettled owners. Reject old tokens within the same epoch. Once a child may have started, release process ownership only after observed exact child exit, not a timeout, response or successful-looking cancel. Proven unstarted owners may settle directly.
- Preserve credential errors and mutually exclusive authentication. Share input validation between enablement/submission and cancel/join prompts on terminal engine events without losing the single pending read.
- Preserve settings recovery bytes and terminal IPC classification through every affected consumer.
- Journal durable mute intent with exclusive ownership; retain unresolved records and restore the exact prior bit best effort during normal cleanup. Document physical durability limits.
- Expose existing diagnostics and compatible optional padding metrics before EQ, once per shared pull; do not claim network loss or acoustic measurements.
- Keep meaningful failure/cancellation regressions and exact-source CI evidence. No real configuration, credentials, receivers, endpoints or release workflow is needed for tests.

Issues #6/#7 remain incident reports. This work may improve retention and observability, but does not establish a common cause or justify closing them. Other socket/stereo/volume reports require their own evidence. A tracking issue should be completed only when its selected implementation and acceptance are integrated, not when a fork branch or draft PR exists.
