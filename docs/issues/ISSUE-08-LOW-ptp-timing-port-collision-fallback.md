# [LOW] Hardcoded PTP Ports 319/320 and Remote RTSP Port 7000 Socket Conflict Handling

- **Issue ID**: ISSUE-08
- **Severity**: **LOW**
- **Subsystem**: Native Audio Engine & Timing Stack (`airflash-engine`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`native/airflash-engine/src/clock.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/clock.rs#L95-L105)
  - [`native/airflash-engine/src/session.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L30-L35)
  - [`native/airflash-engine/src/session.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L525-L530)

---

## 1. Summary
In [`clock.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/clock.rs), AirFlash acts as a PTPv2 (IEEE 1588-2008) Grandmaster and attempts to bind directly to standard PTP UDP ports `319` (Event) and `320` (General). If another PTP daemon, Windows Time synchronization service, or media software is already using ports 319 or 320, socket binding fails unconditionally with `WSAEADDRINUSE`.

Because [`session.rs:L527`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L527) propagates this error via `?`, a port collision immediately aborts the entire audio session startup without falling back to alternative timing modes (such as NTP).

---

## 2. Technical Root Cause Analysis
In [`native/airflash-engine/src/clock.rs:L95-L105`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/clock.rs#L95-L105):

```rust
impl PtpMaster {
    pub fn start(peers: Vec<IpAddr>, clock: Clock) -> Result<Self> {
        let event = UdpSocket::bind("0.0.0.0:319").context("PTP event port 319 unavailable")?;
        let general = UdpSocket::bind("0.0.0.0:320").context("PTP general port 320 unavailable")?;
```

In [`native/airflash-engine/src/session.rs:L525-L530`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L525-L530):
```rust
ptp: if options.timing == "ptp" {
    Some(PtpMaster::start(peers, clock.clone())?) // <-- HARD CRASH ON BIND FAILURE
} else {
    None
},
```

1. **Strict Port Collision Failure**: Ports 319 and 320 are well-known IANA-assigned ports. If bound by another process or restricted by corporate network policies, session startup fails.
2. **Missing NTP Graceful Fallback**: AirFlash supports NTP synchronization (`NtpServer`), but `SessionController.cs` hardcodes `timing = "ptp"` without negotiation or dynamic fallback.

---

## 3. Reproduction Steps
1. Run a utility or service bound to UDP port 319 (e.g. `ncat -u -l 319`).
2. Attempt to start streaming in AirFlash.
3. **Observed Result**: Connection immediately faults with `PTP event port 319 unavailable`.
4. **Expected Result**: Clear diagnostic logging and graceful fallback to NTP timing mode (`timing: "ntp"`).

---

## 4. Proposed Solution
1. In `session.rs`, catch bind errors on `PtpMaster::start` and gracefully fall back to `NtpServer::start` on an ephemeral UDP port.
2. Provide specific diagnostic error messaging when remote receiver port 7000 or local PTP ports collide.
