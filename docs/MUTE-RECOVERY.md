# Local mute recovery

AirFlash retains the exact mute setting observed before changing an output,
including an output that was already muted. Normal cleanup attempts to restore
that setting after the attempt ends. Recovery adds a separate record for an
interrupted desktop process; it does not alter playback credentials, JSONL v1,
or the preferences schema.

## Ownership and storage

The machine-local, per-user directory is
`%LOCALAPPDATA%\AirFlash\mute-recovery`. Diagnostic data overrides use their
isolated output directory instead. Constructing the audio service, enumerating
outputs, or running `--audio-check` does not initialize or read recovery.
Ordinary application startup explicitly initializes it on the audio MTA worker.

A constant `mute-recovery.lease` file remains exclusively open for the service's
lifetime. Cooperating instances cannot acquire it simultaneously, including
instances in separate Windows logon sessions. A process GUID and positive
acquisition generation identify each record; a PID is not ownership evidence.
The actual endpoint identity is resolved before acquisition, even when capture
uses the current default output. Returning a different endpoint is rejected.
The opened endpoint's direction is authoritative, independent of catalog labels
or friendly names. Recording inputs are never read or changed through the mute
interface and never acquire a new recovery record. Recovery also refuses mute
access if a saved output identity now opens a recording input or its direction
cannot be determined; the original obligation remains pending until a playback
output is available. Default capture continues to resolve the render endpoint.

The version-1 journal uses `Intent`, `Owned`, `Restoring`, `Resolved`, and
`Abandoned` states. A durable Intent retains the exact original boolean before
the endpoint setter runs. In-memory ownership is also established before that
setter, because a failing setter may already have changed the output. A failed
acquisition attempts tracked exact-bit rollback and retains unresolved data.
Repeating an owned mute does not recapture its original value.

Writes use a unique temporary file in the same directory, `Flush(true)`,
replacement, and a flush of the committed file. [The .NET flush contract](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.flush?view=net-10.0)
requests flushing intermediate file buffers. It does not establish physical
power-loss durability of filesystem metadata or storage hardware. A reported
final-flush failure can follow replacement and remains a failure.

## Recovery choices

An active lease, unreadable file, invalid structure, unsupported version, or
unavailable endpoint blocks a new attempt that would mute local audio. The
record is never silently replaced with defaults. Parsing bounds are 64 KiB,
32 ownership entries and 1,024 characters per endpoint identity; duplicates and
unknown fields are rejected conservatively.

On a later launch, a saved bit already matching the endpoint can be resolved
without an endpoint write. A differing bit remains pending: a user can restore
the saved setting, keep the current setting, or leave the decision for later.
Each decision revalidates the record revision, owner, generation, endpoint and
observed bit. The modeless review window keeps Stop accessible and runs outside
audio, settings and lifecycle locks. Its notice is independent of session notices.
An unsupported readable record can be explicitly archived: exact original bytes
are backed up and flushed before the original is consumed. Failure preserves a
blocking obligation; continued uncertainty does not become absence.

Normal cleanup still attempts the exact live-owned bit when journal writes fail.
If restoration succeeds but saving the resolved record fails, the living process
knows restoration completed. Further attempts retry persistence only; they do not
overwrite a subsequent user change. The review window offers a saving retry for
that state. After a restart this in-memory distinction is lost, so an old
Restoring/Intent record again requires conservative inspection. Ordinary Restore
and disposal never apply a stale record's endpoint choice.

Playback with local muting disabled can continue while recovery is unresolved.
While the local-muting option is enabled, admission conservatively requires a
resolved recovery journal even for a selected recording source. Turning local
muting off permits that source without discarding saved recovery information.
Muting admission is checked before/after old-attempt cleanup and before a retry
opens another engine; the audio service also checks before any endpoint mutation.

## Verification limits

The state machine, file lease, interruption windows, MTA queue, dispatcher,
controls, and failures are tested with synthetic endpoints and temporary stores.
These fixtures do not prove Windows endpoint mute persistence after a physical
reboot or power loss. Recovery cannot reconstruct a bit lost before a journal
existed. The lease coordinates cooperating AirFlash instances; checking the
expected journal bytes is not a universal lock on arbitrary external writers.
Normal restoration remains best effort rather than an immediate-completion
deadline.
