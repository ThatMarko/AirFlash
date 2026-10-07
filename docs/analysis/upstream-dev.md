# Upstream dev is not this tree

These notes describe `main` at `41190e0`. That commit is `upstream/main` and `origin/main`. The v0.3.3-rc.1 pre-release is `upstream/dev` at `67435a4`, one commit ahead, published as a draft pre-release. It is not merged here. The stable update check reads GitHub's latest release and skips pre-releases, so a 0.3.0 install does not offer that build.

`67435a4` does not edit `WindowsDiscovery`. Browse, resolve, the 90-second cache, the hung `_pending` entry, adapter filtering, and the discovery stop are the behavior in [Discovery](discovery.md) and [Playback coupling](playback-coupling.md). Identity reconciliation is the same walk. It also keeps two new per-receiver fields, transport mode and timing mode.

The commit changes the session that starts after discovery:

- `start` sends `timing: auto`, `transport: auto`, and `compatibility_buffer_ms`.
- A model named `AudioAccessory…` or `HomePod…`, and any stereo pair, still uses the 120 / 200 / 500 ms presets and prefers PTP. One other receiver uses a 3-second buffer unless a per-receiver latency, or a global custom or realtime latency, is set. A receiver custom latency may be as long as 10 seconds.
- The transport signature includes advertised capability fields. A change in those fields restarts a live session.
- The engine can try each advertised address and can choose UDP type 96, TCP type 103, PTP, NTP, PCM, or ALAC. The monitor stores that result. Feedback or volume answers of 404, 405, or 501 are treated as unsupported. Status 470, or a refused transient pairing, is `pairing_required` and is not retried.
- A negotiated sample-rate change rebuilds the equalizer at that rate. The ten bands and Q of 1.4 stay.

Upstream marks the third-party path as experimental and unqualified. HomePod stereo on that commit still needs a real-device pass. Moving this checkout onto `dev` would leave [the workflow notes](README.md) stale for the handshake, the latency ceiling, and the session-restart rule. It would not by itself change the discovery pages.
