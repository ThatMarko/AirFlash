//! Monotonic media clock and a bounded unicast PTPv2 probe master.
//! Implements the AirPlay PTP wire profile; no system clock is changed.
use crate::rtsp::Cancellation;
use anyhow::{Context, Result};
use std::{
    collections::HashMap,
    net::{IpAddr, UdpSocket},
    sync::{
        Arc, Mutex,
        atomic::{AtomicU64, Ordering},
    },
    thread::{self, JoinHandle},
    time::{Duration, Instant, SystemTime, UNIX_EPOCH},
};
#[derive(Clone)]
pub struct Clock {
    origin: Instant,
    epoch_ns: u64,
}
impl Default for Clock {
    fn default() -> Self {
        Self::new()
    }
}
impl Clock {
    pub fn new() -> Self {
        Self {
            origin: Instant::now(),
            epoch_ns: SystemTime::now()
                .duration_since(UNIX_EPOCH)
                .unwrap()
                .as_nanos() as u64,
        }
    }
    pub fn now_ns(&self) -> u64 {
        self.epoch_ns + self.origin.elapsed().as_nanos() as u64
    }
}
fn header(kind: u8, len: usize, clock: u64, seq: u16, flags: u16, interval: i8) -> Vec<u8> {
    let mut p = vec![0; len];
    p[0] = 0x10 | kind;
    p[1] = 2;
    p[2..4].copy_from_slice(&(len as u16).to_be_bytes());
    p[6..8].copy_from_slice(&flags.to_be_bytes());
    p[20..28].copy_from_slice(&clock.to_be_bytes());
    p[28..30].copy_from_slice(&0x8005u16.to_be_bytes());
    p[30..32].copy_from_slice(&seq.to_be_bytes());
    p[33] = interval as u8;
    p
}
fn timestamp(out: &mut [u8], ns: u64) {
    out[..6].copy_from_slice(&(ns / 1_000_000_000).to_be_bytes()[2..]);
    out[6..10].copy_from_slice(&((ns % 1_000_000_000) as u32).to_be_bytes());
}
fn tlv(data: &[u8]) -> Vec<u8> {
    let mut out = vec![0, 3];
    out.extend((data.len() as u16).to_be_bytes());
    out.extend(data);
    out
}
pub fn announce(clock: u64, seq: u16, ns: u64) -> Vec<u8> {
    let mut p = header(11, 76, clock, seq, 0x0408, 0);
    timestamp(&mut p[34..44], ns);
    // Local oscillator; priority is a preference, not a claim of GPS accuracy.
    p[47] = 96;
    p[48] = 248;
    p[49] = 0xfe;
    p[50..52].copy_from_slice(&0xffffu16.to_be_bytes());
    p[52] = 128;
    p[53..61].copy_from_slice(&clock.to_be_bytes());
    p[63] = 0xa0;
    p[64..68].copy_from_slice(&[0, 8, 0, 8]);
    p[68..].copy_from_slice(&clock.to_be_bytes());
    p
}
pub fn sync_pair(clock: u64, seq: u16, ns: u64) -> (Vec<u8>, Vec<u8>) {
    let mut sync = header(0, 44, clock, seq, 0x0608, -3);
    timestamp(&mut sync[34..], ns);
    let mut follow = header(8, 96, clock, seq, 0x0408, -3);
    timestamp(&mut follow[34..44], ns);
    let mut rate = vec![0; 28];
    rate[..6].copy_from_slice(&[0, 0x80, 0xc2, 0, 0, 1]);
    let mut cid = vec![0; 16];
    cid[..6].copy_from_slice(&[0, 0x0d, 0x93, 0, 0, 4]);
    cid[6..14].copy_from_slice(&clock.to_be_bytes());
    follow[44..76].copy_from_slice(&tlv(&rate));
    follow[76..].copy_from_slice(&tlv(&cid));
    (sync, follow)
}
pub struct PtpMaster {
    pub clock_id: u64,
    pub received: Arc<AtomicU64>,
    states: Arc<Mutex<HashMap<IpAddr, PeerState>>>,
    pub mmcss: Arc<Mutex<(bool, bool)>>,
    cancel: Cancellation,
    worker: Option<JoinHandle<()>>,
}
#[derive(Default)]
struct PeerState {
    follow: bool,
    exchanges: u64,
    last: Option<Instant>,
    first: Option<Instant>,
    clock_id: Option<u64>,
    sync: Option<(u16, u64)>,
    sync_pair: Option<(u64, u64)>,
    delay: Option<(u16, u64)>,
    offset: Option<f64>,
    sync_age: Option<Instant>,
    delay_age: Option<Instant>,
    sync_correction: f64,
    source_id: Option<u64>,
    source_port: Option<u16>,
    subscriptions: [Option<Subscription>; 3],
    signaling_sequence: u16,
    received_grant_sequence: Option<u16>,
    exchange_diagnostics: ExchangeDiagnostics,
    last_subscription: Option<Instant>,
    granted_until: [Option<Instant>; 3],
    received_grants: u64,
    last_request: Option<(u8, u16)>,
    grants: u64,
    round_trip_ns: Option<f64>,
    jitter_ns: Option<f64>,
}
#[derive(Clone, serde::Serialize)]
pub struct PeerSnapshot {
    pub host: IpAddr,
    pub mode: &'static str,
    pub exchanges: u64,
    pub ready: bool,
    pub last_exchange_age_ms: Option<u128>,
    pub clock_id: Option<u64>,
    pub offset_ns: Option<f64>,
    pub grants: u64,
    pub received_grants: u64,
    pub active_subscriptions: usize,
    pub round_trip_ns: Option<f64>,
    pub jitter_ns: Option<f64>,
    pub exchange_diagnostics: ExchangeDiagnostics,
}
#[derive(Clone, Copy, Default, serde::Serialize)]
pub struct ExchangeDiagnostics {
    announce_rx: u64,
    sync_rx: u64,
    follow_up_rx: u64,
    delay_resp_rx: u64,
    delay_resp_rejected: u64,
    accepted_exchanges: u64,
    signaling_rx: u64,
    source_mismatches: u64,
    grant_tlv_checked: u64,
    grant_accepted: u64,
    grant_rejected_replay: u64,
    grant_rejected_fields: u64,
    grant_rejected_no_pending_or_stale: u64,
    source_resets: u64,
    offset_resets: u64,
    rtt_rejections: u64,
    subscription_sends: u64,
    subscription_send_errors: u64,
    delay_req_sends: u64,
    delay_req_send_errors: u64,
}
fn counted(value: &mut u64) {
    *value = value.saturating_add(1);
}
#[derive(Clone, Copy)]
struct Subscription {
    interval: i8,
    sent: Instant,
}
const SUBSCRIPTION_REPLY_WINDOW: Duration = Duration::from_secs(2);
fn subscription_index(kind: u8) -> Option<usize> {
    [0, 9, 11].iter().position(|value| *value == kind)
}
impl PeerState {
    fn signaling_sequence(&mut self) -> u16 {
        let sequence = self.signaling_sequence;
        self.signaling_sequence = sequence.wrapping_add(1);
        sequence
    }
    fn delay_request_sent(&mut self, sequence: u16, sent: u64, now: Instant, success: bool) {
        if success {
            self.delay = Some((sequence, sent));
            self.delay_age = Some(now);
            counted(&mut self.exchange_diagnostics.delay_req_sends);
        } else {
            counted(&mut self.exchange_diagnostics.delay_req_send_errors);
        }
    }
    fn exchanged(&mut self, now: Instant) {
        if self
            .last
            .is_none_or(|last| now.duration_since(last) > Duration::from_secs(3))
        {
            self.exchanges = 0;
            self.first = Some(now);
        }
        self.last = Some(now);
        self.exchanges += 1;
    }
    fn ready(&self, now: Instant) -> bool {
        self.exchanges >= 3
            && self
                .first
                .is_some_and(|first| now.duration_since(first) >= Duration::from_millis(250))
            && self
                .last
                .is_some_and(|last| now.duration_since(last) < Duration::from_secs(3))
            && (!self.follow || self.offset.is_some())
    }
    fn subscribe(&mut self, clock_id: u64, now: Instant, mut send: impl FnMut(&[u8]) -> bool) {
        if !self.follow {
            return;
        }
        let healthy = self.ready(now)
            && self.subscriptions.iter().all(Option::is_none)
            && self
                .granted_until
                .iter()
                .all(|until| until.is_some_and(|until| until > now));
        // The first request can precede SETUP or the receiver's Announce.
        // Retrying an unsettled subscription must fit inside startup's 8s gate.
        let interval = Duration::from_secs(if healthy { 10 } else { 1 });
        if self
            .last_subscription
            .is_some_and(|sent| now.duration_since(sent) < interval)
        {
            return;
        }
        self.last_subscription = Some(now);
        let remote = self.source_id.zip(self.source_port);
        for kind in [0, 9, 11] {
            let index = subscription_index(kind).unwrap();
            // Signaling has its own per-destination sequence, not an echoed
            // request id. Retries retain the pending request's original age.
            let pending = self.subscriptions[index]
                .filter(|pending| now.duration_since(pending.sent) < SUBSCRIPTION_REPLY_WINDOW);
            let request = subscription(clock_id, remote, self.signaling_sequence(), kind);
            if send(&request) {
                counted(&mut self.exchange_diagnostics.subscription_sends);
                if pending.is_none() {
                    self.subscriptions[index] = Some(Subscription {
                        interval: request[49] as i8,
                        sent: now,
                    });
                }
            } else {
                counted(&mut self.exchange_diagnostics.subscription_send_errors);
            }
        }
    }
}
fn read_timestamp(data: &[u8]) -> Option<u64> {
    if data.len() < 10 {
        return None;
    }
    let mut secs = [0; 8];
    secs[2..].copy_from_slice(&data[..6]);
    let ns = u32::from_be_bytes(data[6..10].try_into().ok()?);
    if ns >= 1_000_000_000 {
        return None;
    }
    u64::from_be_bytes(secs)
        .checked_mul(1_000_000_000)?
        .checked_add(u64::from(ns))
}
fn unicast_grants(request: &[u8], clock_id: u64, seq: &mut u16) -> Vec<Vec<u8>> {
    if request.len() < 44
        || !((request[34..42] == clock_id.to_be_bytes()
            && request[42..44] == 0x8005u16.to_be_bytes())
            || request[34..44].iter().all(|b| *b == 255))
    {
        return Vec::new();
    }
    let mut grants = Vec::new();
    let mut at = 44;
    while at + 4 <= request.len() {
        let kind = u16::from_be_bytes([request[at], request[at + 1]]);
        let len = u16::from_be_bytes([request[at + 2], request[at + 3]]) as usize;
        if at + 4 + len > request.len() {
            break;
        }
        if kind == 4 && len == 6 {
            let body = &request[at + 4..at + 10];
            let msg = body[0] >> 4;
            if [0, 9, 11].contains(&msg) && u32::from_be_bytes(body[2..6].try_into().unwrap()) > 0 {
                let mut out = header(12, 56, clock_id, *seq, 0x0408, 0);
                *seq = seq.wrapping_add(1);
                out[34..44].copy_from_slice(&request[20..30]);
                out[44..48].copy_from_slice(&[0, 5, 0, 8]);
                out[48..54].copy_from_slice(body);
                let seconds = u32::from_be_bytes(body[2..6].try_into().unwrap()).min(30);
                out[50..54].copy_from_slice(&seconds.to_be_bytes());
                grants.push(out);
            }
        }
        at += 4 + len;
    }
    grants
}
fn valid_packet(data: &[u8]) -> bool {
    data.len() >= 34
        && data[0] >> 4 == 1
        && data[1] & 0xf == 2
        && data[4] == 0
        && u16::from_be_bytes([data[2], data[3]]) as usize == data.len()
}
fn correction(data: &[u8]) -> f64 {
    i64::from_be_bytes(data[8..16].try_into().unwrap()) as f64 / 65536.0
}
fn followed_packet(s: &mut PeerState, data: &[u8], local_ns: u64, now: Instant, own_id: u64) {
    if !valid_packet(data) {
        return;
    }
    let kind = data[0] & 15;
    match kind {
        0 => counted(&mut s.exchange_diagnostics.sync_rx),
        8 => counted(&mut s.exchange_diagnostics.follow_up_rx),
        9 => counted(&mut s.exchange_diagnostics.delay_resp_rx),
        11 => counted(&mut s.exchange_diagnostics.announce_rx),
        12 => counted(&mut s.exchange_diagnostics.signaling_rx),
        _ => (),
    }
    let seq = u16::from_be_bytes([data[30], data[31]]);
    let source = u64::from_be_bytes(data[20..28].try_into().unwrap());
    let source_port = u16::from_be_bytes(data[28..30].try_into().unwrap());
    if kind == 11 && data.len() >= 64 {
        let id = u64::from_be_bytes(data[53..61].try_into().unwrap());
        if s.clock_id != Some(id)
            || s.source_id != Some(source)
            || s.source_port != Some(source_port)
        {
            let subscriptions = if s.source_id.is_none() {
                s.subscriptions
            } else {
                [None; 3]
            };
            let mut exchange_diagnostics = s.exchange_diagnostics;
            if s.source_id.is_some() {
                counted(&mut exchange_diagnostics.source_resets);
            }
            *s = PeerState {
                follow: true,
                clock_id: Some(id),
                source_id: Some(source),
                source_port: Some(source_port),
                subscriptions,
                signaling_sequence: s.signaling_sequence,
                exchange_diagnostics,
                ..Default::default()
            };
        }
    }
    if s.source_id != Some(source) || s.source_port != Some(source_port) {
        counted(&mut s.exchange_diagnostics.source_mismatches);
        return;
    }
    if kind == 12 {
        receive_grants(s, data, own_id, now);
    }
    if kind == 0 && data.len() >= 44 {
        s.sync = Some((seq, local_ns));
        s.sync_age = Some(now);
        s.sync_correction = correction(data);
        if data[6] & 2 == 0 {
            // A one-step clock carries its origin directly.
            if let Some(origin) = read_timestamp(&data[34..44]) {
                s.sync_pair = Some((origin, local_ns));
            }
        }
    }
    if kind == 8
        && data.len() >= 44
        && s.sync_age
            .is_some_and(|at| now.duration_since(at) < Duration::from_secs(2))
    {
        if let (Some((sequence, received)), Some(origin)) = (s.sync, read_timestamp(&data[34..44]))
        {
            if sequence == seq {
                s.sync_pair = Some((origin, received));
                s.sync_correction += correction(data);
                s.sync = None;
            }
        }
    }
    let mut accepted_delay = false;
    if kind == 9
        && data.len() >= 54
        && data[44..52] == own_id.to_be_bytes()
        && data[52..54] == 0x8005u16.to_be_bytes()
        && s.sync_age
            .is_some_and(|at| now.duration_since(at) < Duration::from_secs(2))
        && s.delay_age
            .is_some_and(|at| now.duration_since(at) < Duration::from_secs(2))
    {
        if let (Some((sequence, sent)), Some((origin, received)), Some(remote_received)) =
            (s.delay, s.sync_pair, read_timestamp(&data[34..44]))
        {
            if sequence == seq {
                let outward = (origin as i128 - received as i128) as f64 + s.sync_correction;
                let inward = (remote_received as i128 - sent as i128) as f64 - correction(data);
                let rtt = inward - outward;
                if !(0.0..=250_000_000.0).contains(&rtt) {
                    counted(&mut s.exchange_diagnostics.rtt_rejections);
                    counted(&mut s.exchange_diagnostics.delay_resp_rejected);
                    return;
                }
                let offset = (outward + inward) / 2.0;
                s.jitter_ns = s.offset.map(|old| (old - offset).abs());
                s.round_trip_ns = Some(rtt);
                if s.offset
                    .is_some_and(|old| (old - offset).abs() > 5_000_000.0)
                {
                    counted(&mut s.exchange_diagnostics.offset_resets);
                    s.exchanges = 0;
                    s.first = None;
                    s.last = None;
                    s.offset = Some(offset);
                }
                s.offset = Some(s.offset.map_or(offset, |old| old * 0.875 + offset * 0.125));
                s.exchanged(now);
                counted(&mut s.exchange_diagnostics.accepted_exchanges);
                accepted_delay = true;
                s.delay = None;
                s.delay_age = None;
            }
        }
    }
    if kind == 9 && !accepted_delay {
        counted(&mut s.exchange_diagnostics.delay_resp_rejected);
    }
}
fn receive_grants(s: &mut PeerState, data: &[u8], own_id: u64, now: Instant) {
    if data.len() < 44 {
        return;
    }
    // Other Signaling TLVs are not rejected grants. Identify a grant candidate
    // and validate the whole packet's framing before adopting any grant.
    let mut has_grant = false;
    let mut malformed = false;
    let mut at = 44;
    while at < data.len() {
        if at + 2 <= data.len() && data[at..at + 2] == [0, 5] {
            has_grant = true;
        }
        if at + 4 > data.len() {
            malformed = true;
            break;
        }
        let length = u16::from_be_bytes([data[at + 2], data[at + 3]]) as usize;
        at += 4 + length;
        if at > data.len() {
            malformed = true;
            break;
        }
    }
    if !has_grant {
        return;
    }
    if malformed || data[34..42] != own_id.to_be_bytes() || data[42..44] != 0x8005u16.to_be_bytes()
    {
        counted(&mut s.exchange_diagnostics.grant_rejected_fields);
        return;
    }
    let sequence = u16::from_be_bytes([data[30], data[31]]);
    // Grants use the remote port's Signaling sequence, not our request's.
    // A new worker/source starts a fresh receive history; serial ordering also
    // handles 65535 -> 0 without accepting duplicate or old grant packets.
    if s.received_grant_sequence.is_some_and(|previous| {
        let advance = sequence.wrapping_sub(previous);
        advance == 0 || advance >= 0x8000
    }) {
        counted(&mut s.exchange_diagnostics.grant_rejected_replay);
        return;
    }
    let mut accepted = false;
    let mut at = 44;
    while at + 4 <= data.len() {
        let kind = u16::from_be_bytes([data[at], data[at + 1]]);
        let length = u16::from_be_bytes([data[at + 2], data[at + 3]]) as usize;
        if kind == 5 && length == 8 {
            counted(&mut s.exchange_diagnostics.grant_tlv_checked);
            let body = &data[at + 4..at + 12];
            if let Some(index) = subscription_index(body[0] >> 4) {
                if let Some(pending) = s.subscriptions[index] {
                    let interval = body[1] as i8;
                    let seconds = u32::from_be_bytes(body[2..6].try_into().unwrap());
                    if now.duration_since(pending.sent) >= SUBSCRIPTION_REPLY_WINDOW {
                        counted(&mut s.exchange_diagnostics.grant_rejected_no_pending_or_stale);
                    } else if body[0] & 15 == 0
                        && body[6] == 0
                        && body[7] & !1 == 0
                        && interval >= pending.interval
                        && interval <= 4
                        && (1..=30).contains(&seconds)
                    {
                        s.granted_until[index] =
                            Some(now + Duration::from_secs(u64::from(seconds)));
                        s.received_grants += 1;
                        s.subscriptions[index] = None;
                        counted(&mut s.exchange_diagnostics.grant_accepted);
                        accepted = true;
                    } else {
                        counted(&mut s.exchange_diagnostics.grant_rejected_fields);
                    }
                } else {
                    counted(&mut s.exchange_diagnostics.grant_rejected_no_pending_or_stale);
                }
            } else {
                counted(&mut s.exchange_diagnostics.grant_rejected_fields);
            }
        } else if kind == 5 {
            counted(&mut s.exchange_diagnostics.grant_rejected_fields);
        }
        at += 4 + length;
    }
    if accepted {
        s.received_grant_sequence = Some(sequence);
    }
}
fn subscription(clock: u64, remote: Option<(u64, u16)>, seq: u16, kind: u8) -> Vec<u8> {
    let mut request = header(12, 54, clock, seq, 0x0408, 0);
    request[34..44].fill(255);
    if let Some((id, port)) = remote {
        request[34..42].copy_from_slice(&id.to_be_bytes());
        request[42..44].copy_from_slice(&port.to_be_bytes());
    }
    request[44..48].copy_from_slice(&[0, 4, 0, 6]);
    request[48] = kind << 4;
    request[49] = if kind == 0 { (-3i8) as u8 } else { 0 };
    request[50..54].copy_from_slice(&30u32.to_be_bytes());
    request
}
impl PtpMaster {
    pub fn start(peers: Vec<IpAddr>, clock: Clock) -> Result<Self> {
        Self::start_with_followers(peers, Vec::new(), clock)
    }
    pub fn start_with_followers(
        peers: Vec<IpAddr>,
        followers: Vec<IpAddr>,
        clock: Clock,
    ) -> Result<Self> {
        let event = UdpSocket::bind("0.0.0.0:319").context("PTP event port 319 unavailable")?;
        let general = UdpSocket::bind("0.0.0.0:320").context("PTP general port 320 unavailable")?;
        Self::from_sockets(peers, followers, clock, event, general)
    }
    pub(crate) fn from_sockets(
        peers: Vec<IpAddr>,
        followers: Vec<IpAddr>,
        clock: Clock,
        event: UdpSocket,
        general: UdpSocket,
    ) -> Result<Self> {
        event.set_nonblocking(true)?;
        general.set_nonblocking(true)?;
        let clock_id = rand::random::<u64>() & 0x7fff_ffff_ffff_ffff;
        let received = Arc::new(AtomicU64::new(0));
        let rx = received.clone();
        let cancel = Cancellation::default();
        let stop = cancel.clone();
        let states = Arc::new(Mutex::new(
            peers
                .iter()
                .map(|ip| {
                    (
                        *ip,
                        PeerState {
                            follow: followers.contains(ip),
                            ..Default::default()
                        },
                    )
                })
                .collect::<HashMap<_, _>>(),
        ));
        let state = states.clone();
        let mmcss = Arc::new(Mutex::new((false, false)));
        let priority_state = mmcss.clone();
        let worker = thread::Builder::new()
            .name("airplay-ptp".into())
            .spawn(move || {
                #[cfg(windows)]
                {
                    let priority = crate::wasapi::Mmcss::clock();
                    *priority_state.lock().unwrap() =
                        (priority.registered(), priority.priority_set());
                    // The guard belongs to this worker until it exits.
                    let _priority = priority;
                    ptp_loop(
                        &event, &general, &peers, &followers, &clock, clock_id, &rx, &stop, &state,
                    );
                }
                #[cfg(not(windows))]
                {
                    let _ = priority_state;
                    ptp_loop(
                        &event, &general, &peers, &followers, &clock, clock_id, &rx, &stop, &state,
                    );
                }
            })?;
        Ok(Self {
            clock_id,
            received,
            states,
            mmcss,
            cancel,
            worker: Some(worker),
        })
    }
    pub fn snapshots(&self) -> Vec<PeerSnapshot> {
        let now = Instant::now();
        let states = self.states.lock().unwrap();
        let mut result: Vec<_> = states
            .iter()
            .map(|(ip, s)| PeerSnapshot {
                host: *ip,
                mode: if s.follow { "receiver" } else { "sender" },
                exchanges: s.exchanges,
                ready: s.ready(now),
                last_exchange_age_ms: s.last.map(|last| now.duration_since(last).as_millis()),
                clock_id: s.clock_id,
                offset_ns: s.offset,
                grants: s.grants,
                received_grants: s.received_grants,
                active_subscriptions: s
                    .granted_until
                    .iter()
                    .flatten()
                    .filter(|until| **until > now)
                    .count(),
                round_trip_ns: s.round_trip_ns,
                jitter_ns: s.jitter_ns,
                exchange_diagnostics: s.exchange_diagnostics,
            })
            .collect();
        result.sort_by_key(|s| s.host);
        result
    }
    pub fn ready(&self, host: IpAddr) -> bool {
        self.states
            .lock()
            .unwrap()
            .get(&host)
            .is_some_and(|s| s.ready(Instant::now()))
    }
    pub fn timeline(&self, host: IpAddr, local_ns: u64) -> Result<(u64, u64)> {
        let states = self.states.lock().unwrap();
        let s = states.get(&host).context("unknown timing peer")?;
        if !s.follow {
            return Ok((self.clock_id, local_ns));
        }
        anyhow::ensure!(s.ready(Instant::now()), "receiver clock is not ready");
        let mapped = local_ns as i128 + s.offset.unwrap().round() as i128;
        Ok((
            s.clock_id.context("missing receiver clock identity")?,
            u64::try_from(mapped)?,
        ))
    }
}
#[allow(clippy::too_many_arguments)]
fn ptp_loop(
    event: &UdpSocket,
    general: &UdpSocket,
    peers: &[IpAddr],
    followers: &[IpAddr],
    clock: &Clock,
    clock_id: u64,
    rx: &AtomicU64,
    stop: &Cancellation,
    state: &Mutex<HashMap<IpAddr, PeerState>>,
) {
    let mut seq = 0u16;
    let mut last_announce = Instant::now() - Duration::from_secs(2);
    let mut next_sync = Instant::now();
    let mut buf = [0u8; 2048];
    while !stop.is_cancelled() {
        let now = Instant::now();
        if now >= next_sync {
            for peer in peers {
                if followers.contains(peer) {
                    state.lock().unwrap().get_mut(peer).unwrap().subscribe(
                        clock_id,
                        now,
                        |request| general.send_to(request, (*peer, 320)).is_ok(),
                    );
                    if now.duration_since(last_announce) >= Duration::from_secs(1) {
                        let mut req = header(1, 44, clock_id, seq, 0x0408, 0);
                        let sent = clock.now_ns();
                        timestamp(&mut req[34..44], sent);
                        let success = event.send_to(&req, (*peer, 319)).is_ok();
                        state
                            .lock()
                            .unwrap()
                            .get_mut(peer)
                            .unwrap()
                            .delay_request_sent(seq, sent, now, success);
                    }
                    continue;
                }
                let (sync, follow) = sync_pair(clock_id, seq, clock.now_ns());
                let _ = event.send_to(&sync, (*peer, 319));
                let _ = general.send_to(&follow, (*peer, 320));
                if now.duration_since(last_announce) >= Duration::from_secs(1) {
                    let _ = general.send_to(&announce(clock_id, seq, clock.now_ns()), (*peer, 320));
                }
            }
            if now.duration_since(last_announce) >= Duration::from_secs(1) {
                last_announce = now;
            }
            seq = seq.wrapping_add(1);
            next_sync = now + Duration::from_millis(125);
        }
        for socket in [&event, &general] {
            let until = Instant::now() + Duration::from_millis(1);
            for _ in 0..128 {
                if Instant::now() >= until {
                    break;
                }
                let Ok((n, addr)) = socket.recv_from(&mut buf) else {
                    break;
                };
                if !peers.contains(&addr.ip())
                    || !valid_packet(&buf[..n])
                    || ![319, 320].contains(&addr.port())
                {
                    continue;
                }
                let length = u16::from_be_bytes([buf[2], buf[3]]) as usize;
                if length > n || length < 34 {
                    continue;
                }
                rx.fetch_add(1, Ordering::Relaxed);
                let kind = buf[0] & 0xf;
                let request_seq = u16::from_be_bytes([buf[30], buf[31]]);
                let local_received = clock.now_ns();
                let mut states = state.lock().unwrap();
                let s = states.get_mut(&addr.ip()).unwrap();
                if s.follow {
                    followed_packet(s, &buf[..length], local_received, now, clock_id);
                }
                if kind == 12 && length >= 44 {
                    for grant in unicast_grants(&buf[..length], clock_id, &mut s.signaling_sequence)
                    {
                        if general.send_to(&grant, (addr.ip(), 320)).is_ok() {
                            s.grants += 1;
                        }
                    }
                }
                if !s.follow
                    && (kind == 1 || kind == 2)
                    && length >= if kind == 1 { 44 } else { 54 }
                {
                    if s.last_request == Some((kind, request_seq))
                        && s.last
                            .is_some_and(|at| now.duration_since(at) < Duration::from_secs(3))
                    {
                        continue;
                    }
                    s.last_request = Some((kind, request_seq));
                    let response_kind = if kind == 1 { 9 } else { 3 };
                    let mut response = header(
                        response_kind,
                        54,
                        clock_id,
                        request_seq,
                        if kind == 1 { 0x0408 } else { 0x0608 },
                        -3,
                    );
                    timestamp(&mut response[34..44], local_received);
                    response[44..54].copy_from_slice(&buf[20..30]);
                    if kind == 1 {
                        if general.send_to(&response, (addr.ip(), 320)).is_ok() {
                            s.exchanged(now);
                        }
                    } else {
                        let _ = event.send_to(&response, (addr.ip(), 319));
                        let mut follow = header(10, 54, clock_id, request_seq, 0x0408, -3);
                        timestamp(&mut follow[34..44], clock.now_ns());
                        follow[44..54].copy_from_slice(&buf[20..30]);
                        if general.send_to(&follow, (addr.ip(), 320)).is_ok() {
                            s.exchanged(now);
                        }
                    }
                }
            }
        }
        thread::sleep(Duration::from_millis(2));
    }
}
impl Drop for PtpMaster {
    fn drop(&mut self) {
        self.cancel.cancel();
        if let Some(worker) = self.worker.take() {
            let _ = worker.join();
        }
    }
}
#[cfg(test)]
mod tests {
    use super::*;

    fn requests(peer: &mut PeerState, now: Instant) -> Vec<Vec<u8>> {
        let mut sent = Vec::new();
        peer.subscribe(7, now, |packet| {
            sent.push(packet.to_vec());
            true
        });
        sent
    }

    fn test_grants(request: &[u8], clock_id: u64, mut sequence: u16) -> Vec<Vec<u8>> {
        unicast_grants(request, clock_id, &mut sequence)
    }
    fn sequences(packets: &[Vec<u8>]) -> Vec<u16> {
        packets
            .iter()
            .map(|packet| u16::from_be_bytes([packet[30], packet[31]]))
            .collect()
    }
    fn grants(peer: &mut PeerState, source: u64, requests: &[Vec<u8>], now: Instant) {
        for request in requests {
            let sequence = u16::from_be_bytes([request[30], request[31]]);
            for grant in test_grants(request, source, sequence) {
                followed_packet(peer, &grant, 0, now, 7);
            }
        }
    }
    fn exchange(peer: &mut PeerState, source: u64, sequence: u16, now: Instant) {
        let base = 1_000_000_000u64 + u64::from(sequence) * 1_000_000_000;
        let (sync, follow) = sync_pair(source, sequence, base + 1_000_000);
        followed_packet(peer, &sync, base + 100_000, now, 7);
        followed_packet(peer, &follow, base + 110_000, now, 7);
        peer.delay = Some((sequence, base + 200_000));
        peer.delay_age = Some(now);
        let mut response = header(9, 54, source, sequence, 0x0408, 0);
        timestamp(&mut response[34..44], base + 1_300_000);
        response[44..52].copy_from_slice(&7u64.to_be_bytes());
        response[52..54].copy_from_slice(&0x8005u16.to_be_bytes());
        followed_packet(peer, &response, base + 400_000, now, 7);
    }

    #[test]
    fn follower_retries_a_lost_initial_request_inside_the_startup_budget() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        // The synthetic receiver drops the first batch. No socket is opened.
        assert_eq!(requests(&mut peer, now).len(), 3);
        assert!(requests(&mut peer, now + Duration::from_millis(999)).is_empty());
        let retry_at = now + Duration::from_secs(1);
        let retry = requests(&mut peer, retry_at);
        assert_eq!(retry.len(), 3);
        followed_packet(&mut peer, &announce(8, 0, 0), 0, retry_at, 7);
        let matched = requests(&mut peer, retry_at + Duration::from_millis(125));
        assert_eq!(matched.len(), 3);
        assert!(
            matched
                .iter()
                .all(|packet| packet[34..42] == 8u64.to_be_bytes())
        );
        grants(
            &mut peer,
            8,
            &matched,
            retry_at + Duration::from_millis(150),
        );
        assert_eq!(peer.received_grants, 3);
        for sequence in 0..3 {
            exchange(
                &mut peer,
                8,
                sequence,
                retry_at + Duration::from_millis(250 + u64::from(sequence) * 1000),
            );
        }
        assert!(peer.ready(now + Duration::from_millis(3250)));
    }

    #[test]
    fn follower_retry_preserves_the_pending_reply_lifetime_with_new_wire_sequences() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        let first = requests(&mut peer, now);
        let retry = requests(&mut peer, now + Duration::from_secs(1));
        assert_eq!(sequences(&first), [0, 1, 2]);
        assert_eq!(sequences(&retry), [3, 4, 5]);
        assert!(
            peer.subscriptions
                .iter()
                .flatten()
                .all(|pending| pending.sent == now)
        );
        // A 1.5s reply remains valid across the 1s retry, including a lost first grant.
        grants(&mut peer, 8, &first, now + Duration::from_millis(1500));
        assert_eq!(peer.received_grants, 3);
        assert!(peer.subscriptions.iter().all(Option::is_none));
        assert!(!peer.ready(now + Duration::from_millis(1500)));

        let mut lost = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut lost, &announce(8, 0, 0), 0, now, 7);
        let stale = requests(&mut lost, now);
        requests(&mut lost, now + Duration::from_secs(1));
        grants(&mut lost, 8, &stale, now + SUBSCRIPTION_REPLY_WINDOW);
        assert_eq!(lost.received_grants, 0);
        let fresh = requests(&mut lost, now + Duration::from_millis(2100));
        assert_eq!(fresh.len(), 3);
        assert_eq!(sequences(&fresh), [6, 7, 8]);
        grants(&mut lost, 8, &fresh, now + Duration::from_millis(2200));
        assert_eq!(lost.received_grants, 3);
    }

    #[test]
    fn follower_recovers_grants_before_announce_without_accepting_unknown_sources() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        let initial = requests(&mut peer, now);
        grants(&mut peer, 8, &initial, now + Duration::from_millis(10));
        assert_eq!(peer.received_grants, 0);
        followed_packet(
            &mut peer,
            &announce(8, 0, 0),
            0,
            now + Duration::from_millis(20),
            7,
        );
        let retry = requests(&mut peer, now + Duration::from_millis(125));
        assert_eq!(retry.len(), 3);
        // First Announce preserves the valid initial pending lifetime while
        // making a request to the now-confirmed source immediately possible.
        assert_eq!(sequences(&retry), [3, 4, 5]);
        grants(&mut peer, 9, &initial, now + Duration::from_millis(150));
        assert_eq!(peer.received_grants, 0);
        grants(&mut peer, 8, &retry, now + Duration::from_millis(200));
        assert_eq!(peer.received_grants, 3);
        assert!(!peer.ready(now + Duration::from_millis(200)));
        for sequence in 0..3 {
            exchange(
                &mut peer,
                8,
                sequence,
                now + Duration::from_millis(250 + u64::from(sequence) * 1000),
            );
        }
        assert!(peer.ready(now + Duration::from_millis(2250)));
    }

    #[test]
    fn healthy_follower_renews_at_ten_seconds_independently_of_unready_peers() {
        let now = Instant::now();
        let mut healthy = PeerState {
            follow: true,
            ..Default::default()
        };
        let mut unsettled = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut healthy, &announce(8, 0, 0), 0, now, 7);
        let initial = requests(&mut healthy, now);
        grants(&mut healthy, 8, &initial, now);
        requests(&mut unsettled, now);
        exchange(&mut healthy, 8, 0, now);
        exchange(&mut healthy, 8, 1, now + Duration::from_millis(125));
        exchange(&mut healthy, 8, 2, now + Duration::from_millis(250));
        for second in 1..10 {
            let at = now + Duration::from_secs(second);
            exchange(&mut healthy, 8, second as u16 + 2, at);
            assert!(healthy.ready(at));
            assert!(requests(&mut healthy, at).is_empty());
            assert_eq!(requests(&mut unsettled, at).len(), 3);
        }
        assert!(requests(&mut healthy, now + Duration::from_millis(9999)).is_empty());
        assert_eq!(
            requests(&mut healthy, now + Duration::from_secs(10)).len(),
            3
        );
    }

    #[test]
    fn expired_leases_and_changed_sources_resume_bounded_follower_retries() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        let initial = requests(&mut peer, now);
        for (sequence, request) in initial.iter().enumerate() {
            let mut grant = test_grants(request, 8, sequence as u16).remove(0);
            grant[50..54].copy_from_slice(&1u32.to_be_bytes());
            followed_packet(&mut peer, &grant, 0, now, 7);
        }
        exchange(&mut peer, 8, 0, now);
        exchange(&mut peer, 8, 1, now + Duration::from_millis(125));
        exchange(&mut peer, 8, 2, now + Duration::from_millis(250));
        assert!(peer.ready(now + Duration::from_secs(1)));
        let expired = requests(&mut peer, now + Duration::from_secs(1));
        assert_eq!(expired.len(), 3);
        followed_packet(
            &mut peer,
            &announce(9, 0, 0),
            0,
            now + Duration::from_millis(1100),
            7,
        );
        let changed = requests(&mut peer, now + Duration::from_millis(1125));
        assert_eq!(changed.len(), 3);
        assert!(
            changed
                .iter()
                .all(|packet| packet[34..42] == 9u64.to_be_bytes())
        );
        grants(&mut peer, 8, &expired, now + Duration::from_millis(1150));
        assert_eq!(peer.received_grants, 0);
        grants(&mut peer, 9, &changed, now + Duration::from_millis(1200));
        assert_eq!(peer.received_grants, 3);
        assert!(!peer.ready(now + Duration::from_millis(1200)));
        let mut changed_port = announce(9, 0, 0);
        changed_port[28..30].copy_from_slice(&0x8006u16.to_be_bytes());
        followed_packet(
            &mut peer,
            &changed_port,
            0,
            now + Duration::from_millis(1250),
            7,
        );
        let port_retry = requests(&mut peer, now + Duration::from_millis(1375));
        assert_eq!(port_retry.len(), 3);
        assert!(
            port_retry
                .iter()
                .all(|packet| packet[42..44] == 0x8006u16.to_be_bytes())
        );
        grants(&mut peer, 9, &changed, now + Duration::from_millis(1400));
        assert_eq!(peer.received_grants, 0);
    }

    #[test]
    fn failed_subscription_sends_are_bounded_and_sender_role_is_unchanged() {
        let now = Instant::now();
        assert!(requests(&mut PeerState::default(), now).is_empty());
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        let mut attempts = 0;
        peer.subscribe(7, now, |_| {
            attempts += 1;
            false
        });
        assert_eq!(attempts, 3);
        assert!(peer.subscriptions.iter().all(Option::is_none));
        assert!(requests(&mut peer, now + Duration::from_millis(125)).is_empty());
        assert_eq!(requests(&mut peer, now + Duration::from_secs(1)).len(), 3);
    }

    #[test]
    fn readiness_requires_exchange_streak_and_expires() {
        let now = Instant::now();
        let mut peer = PeerState::default();
        assert!(!peer.ready(now));
        peer.exchanged(now);
        peer.exchanged(now + Duration::from_millis(125));
        assert!(!peer.ready(now + Duration::from_millis(250)));
        peer.exchanged(now + Duration::from_millis(250));
        assert!(peer.ready(now + Duration::from_millis(250)));
        assert!(!peer.ready(now + Duration::from_secs(4)));
        peer.exchanged(now + Duration::from_secs(4));
        assert_eq!(peer.exchanges, 1);
        peer.follow = true;
        peer.exchanged(now + Duration::from_millis(4125));
        peer.exchanged(now + Duration::from_millis(4250));
        assert!(!peer.ready(now + Duration::from_millis(4250)));
    }
    #[test]
    fn unicast_grants_validate_target_length_type_and_duration() {
        let request = subscription(7, Some((8, 0x8005)), 3, 0);
        let grants = test_grants(&request, 8, 3);
        assert_eq!(grants.len(), 1);
        assert_eq!(&grants[0][34..42], &7u64.to_be_bytes());
        assert_eq!(&grants[0][44..48], &[0, 5, 0, 8]);
        assert!(test_grants(&request, 9, 3).is_empty());
        assert!(test_grants(&request[..50], 8, 3).is_empty());
        let mut invalid = request.clone();
        invalid[48] = 7 << 4;
        assert!(test_grants(&invalid, 8, 3).is_empty());
        let mut invalid = request;
        invalid[50..54].fill(0);
        assert!(test_grants(&invalid, 8, 3).is_empty());
    }
    #[test]
    fn follower_pairs_sequences_and_rejects_wrong_or_stale_responses() {
        let now = Instant::now();
        let mut s = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut s, &announce(8, 0, 0), 0, now, 7);
        for seq in 0..3u16 {
            let at = now + Duration::from_millis(u64::from(seq) * 1000);
            let base = 1_000_000_000u64 + u64::from(seq) * 1_000_000_000;
            let (sync, follow) = sync_pair(8, seq, base + 1_000_000);
            followed_packet(&mut s, &sync, base + 100_000, at, 7);
            followed_packet(&mut s, &follow, base + 110_000, at, 7);
            s.delay = Some((seq, base + 200_000));
            s.delay_age = Some(at);
            let mut response = header(9, 54, 8, seq, 0x0408, 0);
            timestamp(&mut response[34..44], base + 1_300_000);
            response[44..52].copy_from_slice(&7u64.to_be_bytes());
            response[52..54].copy_from_slice(&0x8005u16.to_be_bytes());
            let mut bad = response.clone();
            bad[44] = 1;
            followed_packet(&mut s, &bad, base + 400_000, at, 7);
            assert!(s.delay.is_some());
            followed_packet(&mut s, &response, base + 400_000, at, 7);
            followed_packet(&mut s, &response, base + 400_000, at, 7);
            assert_eq!(s.exchanges, u64::from(seq) + 1);
        }
        assert!(s.ready(now + Duration::from_secs(2)));
        assert_eq!(s.offset, Some(1_000_000.0));
        assert_eq!(s.round_trip_ns, Some(200_000.0));
        let mut bad = announce(8, 0, 0);
        bad[4] = 1;
        assert!(!valid_packet(&bad));
        assert!(read_timestamp(&[255; 10]).is_none());
    }
    #[test]
    fn follower_grants_require_pending_source_target_and_bounded_lease() {
        let now = Instant::now();
        let mut s = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut s, &announce(8, 0, 0), 0, now, 7);
        s.subscriptions[0] = Some(Subscription {
            interval: -3,
            sent: now,
        });
        let request = subscription(7, Some((8, 0x8005)), 3, 0);
        let grant = test_grants(&request, 8, 3).remove(0);
        for (index, value) in [
            (20, 1),
            (29, 1),
            (41, 1),
            (43, 1),
            (47, 7),
            (48, 0x90),
            (49, (-4i8) as u8),
            (54, 1),
            (55, 2),
        ] {
            let mut bad = grant.clone();
            bad[index] = value;
            followed_packet(&mut s, &bad, 0, now, 7);
            assert_eq!(s.received_grants, 0);
        }
        let mut bad = grant.clone();
        bad[50..54].copy_from_slice(&31u32.to_be_bytes());
        followed_packet(&mut s, &bad, 0, now, 7);
        followed_packet(&mut s, &grant, 0, now + Duration::from_secs(3), 7);
        assert_eq!(s.received_grants, 0);
        followed_packet(&mut s, &grant, 0, now, 7);
        followed_packet(&mut s, &grant, 0, now, 7);
        assert_eq!(s.received_grants, 1);
        assert_eq!(s.granted_until[0], Some(now + Duration::from_secs(30)));
        assert!(!s.ready(now)); // A lease is not a clock offset exchange.
        let mut bad = request;
        bad[43] = 1;
        assert!(test_grants(&bad, 8, 3).is_empty());
    }

    // Encode this grant independently of the production grant writer: the
    // eight-byte GRANT_UNICAST_TRANSMISSION body ends in reserved, then flags.
    fn independent_grant() -> [u8; 56] {
        [
            0x1c, 2, 0, 56, 0, 0, 4, 8, // Signaling header, domain zero.
            0, 0, 0, 0, 0, 0, 0, 0, // Correction.
            0, 0, 0, 0, // Reserved.
            0, 0, 0, 0, 0, 0, 0, 8, 0x80, 5, // Remote source identity and port.
            0, 3, 5, 0x7f, // Sequence, control, interval.
            0, 0, 0, 0, 0, 0, 0, 7, 0x80, 5, // Local target identity and port.
            0, 5, 0, 8, // Grant TLV type and body length.
            0, 0xfd, 0, 0, 0, 30, 0, 1, // Sync, -3, 30 seconds, reserved, R.
        ]
    }

    #[test]
    fn follower_accepts_independent_grants_with_either_renewal_flag() {
        let now = Instant::now();
        for renewal in [0, 1] {
            let mut peer = PeerState {
                follow: true,
                ..Default::default()
            };
            followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
            assert_eq!(requests(&mut peer, now).len(), 3);
            for (index, kind) in [0, 9, 11].into_iter().enumerate() {
                let mut grant = independent_grant();
                grant[30..32].copy_from_slice(&(700 + index as u16).to_be_bytes());
                grant[48] = kind << 4;
                grant[49] = if kind == 0 { (-3i8) as u8 } else { 0 };
                grant[55] = renewal;
                followed_packet(&mut peer, &grant, 0, now + Duration::from_millis(10), 7);
            }
            assert_eq!(peer.received_grants, 3);
            assert_eq!(
                peer.granted_until,
                [Some(now + Duration::from_millis(30010)); 3]
            );
            assert!(peer.subscriptions.iter().all(Option::is_none));
            assert!(!peer.ready(now)); // A grant still cannot replace an exchange.
            for sequence in 0..3 {
                exchange(
                    &mut peer,
                    8,
                    sequence,
                    now + Duration::from_millis(20 + u64::from(sequence) * 125),
                );
            }
            assert!(peer.ready(now + Duration::from_millis(270)));
            assert!(requests(&mut peer, now + Duration::from_secs(1)).is_empty());
            exchange(&mut peer, 8, 4, now + Duration::from_secs(10));
            exchange(&mut peer, 8, 5, now + Duration::from_millis(10125));
            exchange(&mut peer, 8, 6, now + Duration::from_millis(10250));
            assert_eq!(
                requests(&mut peer, now + Duration::from_millis(10250)).len(),
                3
            );
        }
    }

    #[test]
    fn follower_independent_grants_retain_reserved_flag_and_correlation_guards() {
        let now = Instant::now();
        for (index, value) in [
            (20, 1),            // Source identity.
            (29, 6),            // Source port.
            (41, 1),            // Target identity.
            (43, 6),            // Target port.
            (47, 7),            // TLV body length.
            (48, 1),            // Reserved message-type bits.
            (49, (-4i8) as u8), // Faster interval than requested.
            (54, 1),            // Reserved octet, including the previously accepted bit zero.
            (55, 2),            // Unknown flag bit, not Renewal Invited.
        ] {
            let mut peer = PeerState {
                follow: true,
                ..Default::default()
            };
            followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
            requests(&mut peer, now);
            let mut invalid = independent_grant();
            invalid[index] = value;
            followed_packet(&mut peer, &invalid, 0, now, 7);
            assert_eq!(peer.received_grants, 0, "accepted byte {index}={value}");
            assert!(peer.granted_until.iter().all(Option::is_none));
        }
        for seconds in [0u32, 31] {
            let mut peer = PeerState {
                follow: true,
                ..Default::default()
            };
            followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
            requests(&mut peer, now);
            let mut invalid = independent_grant();
            invalid[50..54].copy_from_slice(&seconds.to_be_bytes());
            followed_packet(&mut peer, &invalid, 0, now, 7);
            assert_eq!(peer.received_grants, 0);
        }
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        requests(&mut peer, now);
        followed_packet(
            &mut peer,
            &independent_grant(),
            0,
            now + SUBSCRIPTION_REPLY_WINDOW,
            7,
        );
        assert_eq!(peer.received_grants, 0);
        assert!(peer.granted_until.iter().all(Option::is_none));
    }

    #[test]
    fn signaling_uses_per_peer_transmit_sequences_across_requests_replies_and_wrap() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            signaling_sequence: 65534,
            ..Default::default()
        };
        let mut other = PeerState {
            follow: true,
            ..Default::default()
        };
        assert_eq!(sequences(&requests(&mut peer, now)), [65534, 65535, 0]);
        assert_eq!(sequences(&requests(&mut other, now)), [0, 1, 2]);
        let request = subscription(8, Some((7, 0x8005)), 4321, 0);
        let reply = unicast_grants(&request, 7, &mut peer.signaling_sequence);
        assert_eq!(sequences(&reply), [1]); // A grant never echoes the request.
        assert_eq!(
            sequences(&requests(&mut peer, now + Duration::from_secs(1))),
            [2, 3, 4]
        );
        followed_packet(
            &mut peer,
            &announce(8, 0, 0),
            0,
            now + Duration::from_millis(1100),
            7,
        );
        assert_eq!(
            sequences(&requests(&mut peer, now + Duration::from_millis(1200))),
            [5, 6, 7]
        );
    }

    #[test]
    fn independent_grants_share_packet_sequence_and_reject_replays_after_renewal() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        requests(&mut peer, now);
        let mut packet = independent_grant().to_vec();
        packet[30..32].copy_from_slice(&400u16.to_be_bytes());
        for kind in [9, 11] {
            let mut tlv = independent_grant()[44..].to_vec();
            tlv[4] = kind << 4;
            tlv[5] = 0;
            packet.extend(tlv);
        }
        packet[2..4].copy_from_slice(&80u16.to_be_bytes());
        followed_packet(&mut peer, &packet, 0, now + Duration::from_millis(100), 7);
        assert_eq!(peer.received_grants, 3);
        assert_eq!(peer.received_grant_sequence, Some(400));
        assert!(peer.subscriptions.iter().all(Option::is_none));
        let leases = peer.granted_until;
        requests(&mut peer, now + Duration::from_secs(1));
        followed_packet(&mut peer, &packet, 0, now + Duration::from_millis(1100), 7);
        packet[30..32].copy_from_slice(&399u16.to_be_bytes());
        followed_packet(&mut peer, &packet, 0, now + Duration::from_millis(1200), 7);
        assert_eq!(peer.received_grants, 3);
        assert_eq!(peer.granted_until, leases);
        assert!(peer.subscriptions.iter().all(Option::is_some));
        packet[30..32].copy_from_slice(&401u16.to_be_bytes());
        followed_packet(&mut peer, &packet, 0, now + Duration::from_millis(1300), 7);
        assert_eq!(peer.received_grants, 6);
        assert_eq!(
            peer.granted_until,
            [Some(now + Duration::from_millis(31300)); 3]
        );
        assert_eq!(peer.exchange_diagnostics.grant_rejected_replay, 2);
    }

    #[test]
    fn other_signaling_tlvs_do_not_count_as_rejected_grants_or_advance_grant_history() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        requests(&mut peer, now);
        // These independently encoded packets contain Request or unknown TLVs,
        // including wildcard targets and an incomplete non-grant TLV body.
        let mut request = independent_grant()[..54].to_vec();
        request[2..4].copy_from_slice(&54u16.to_be_bytes());
        request[34..44].fill(255);
        request[44..48].copy_from_slice(&[0, 4, 0, 6]);
        request[30..32].copy_from_slice(&400u16.to_be_bytes());
        let mut unknown = independent_grant()[..48].to_vec();
        unknown[2..4].copy_from_slice(&48u16.to_be_bytes());
        unknown[34..44].fill(255);
        unknown[44..48].copy_from_slice(&[0x12, 0x34, 0, 0]);
        unknown[30..32].copy_from_slice(&399u16.to_be_bytes());
        let mut malformed_other = unknown.clone();
        malformed_other[46..48].copy_from_slice(&8u16.to_be_bytes());
        for phase in 0..2 {
            let before = serde_json::to_value(peer.exchange_diagnostics).unwrap();
            for packet in [&request, &unknown, &malformed_other] {
                followed_packet(&mut peer, packet, 0, now, 7);
            }
            let mut after = serde_json::to_value(peer.exchange_diagnostics).unwrap();
            assert_eq!(
                after["signaling_rx"].as_u64(),
                Some(before["signaling_rx"].as_u64().unwrap() + 3)
            );
            after["signaling_rx"] = before["signaling_rx"].clone();
            assert_eq!(after, before);
            assert_eq!(
                peer.received_grant_sequence,
                if phase == 0 { None } else { Some(400) }
            );
            let mut grant = independent_grant();
            grant[30..32].copy_from_slice(&(400 + phase as u16).to_be_bytes());
            if phase == 1 {
                grant[48] = 9 << 4;
                grant[49] = 0;
            }
            followed_packet(&mut peer, &grant, 0, now, 7);
            assert_eq!(peer.received_grants, phase + 1);
        }
        assert_eq!(peer.received_grant_sequence, Some(401));
        assert_eq!(peer.exchange_diagnostics.grant_rejected_fields, 0);
        assert_eq!(peer.exchange_diagnostics.grant_rejected_replay, 0);
    }

    #[test]
    fn grant_receive_sequence_wraps_and_malformed_or_unrequested_packets_do_not_poison_it() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        let mut grant = independent_grant();
        grant[30..32].copy_from_slice(&12345u16.to_be_bytes());
        followed_packet(&mut peer, &grant, 0, now, 7);
        assert_eq!(peer.received_grant_sequence, None);
        requests(&mut peer, now);
        let mut malformed = grant.to_vec();
        malformed.extend([0, 5, 0, 8, 0]);
        malformed[2..4].copy_from_slice(&61u16.to_be_bytes());
        followed_packet(&mut peer, &malformed, 0, now, 7);
        assert_eq!(peer.received_grant_sequence, None);
        assert_eq!(peer.received_grants, 0);
        for (kind, sequence) in [(0, 65535u16), (9, 0), (11, 1)] {
            grant[48] = kind << 4;
            grant[49] = if kind == 0 { (-3i8) as u8 } else { 0 };
            grant[30..32].copy_from_slice(&sequence.to_be_bytes());
            followed_packet(&mut peer, &grant, 0, now, 7);
        }
        assert_eq!(peer.received_grants, 3);
        assert_eq!(peer.received_grant_sequence, Some(1));
        let previous = peer.granted_until;
        requests(&mut peer, now + Duration::from_secs(1));
        grant[30..32].copy_from_slice(&65535u16.to_be_bytes());
        followed_packet(&mut peer, &grant, 0, now + Duration::from_secs(1), 7);
        assert_eq!(peer.received_grants, 3);
        assert_eq!(peer.granted_until, previous);
    }

    #[test]
    fn attempt_clock_diagnostics_survive_source_changes_and_saturate() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        let sent = requests(&mut peer, now);
        grants(&mut peer, 8, &sent, now);
        exchange(&mut peer, 8, 0, now);
        peer.offset = Some(-10_000_000.0);
        exchange(&mut peer, 8, 1, now + Duration::from_secs(1));
        assert_eq!(peer.exchange_diagnostics.offset_resets, 1);
        assert_eq!(peer.exchanges, 1);
        followed_packet(
            &mut peer,
            &announce(9, 0, 0),
            0,
            now + Duration::from_secs(2),
            7,
        );
        assert_eq!(peer.received_grant_sequence, None);
        assert_eq!(peer.received_grants, 0);
        assert_eq!(peer.exchange_diagnostics.grant_accepted, 3);
        assert_eq!(peer.exchange_diagnostics.sync_rx, 2);
        assert_eq!(peer.exchange_diagnostics.follow_up_rx, 2);
        assert_eq!(peer.exchange_diagnostics.delay_resp_rx, 2);
        assert_eq!(peer.exchange_diagnostics.source_resets, 1);
        assert_eq!(peer.exchange_diagnostics.announce_rx, 2);
        requests(&mut peer, now + Duration::from_secs(2));
        let mut new_source = independent_grant();
        new_source[27] = 9;
        new_source[30..32].copy_from_slice(&0u16.to_be_bytes());
        followed_packet(&mut peer, &new_source, 0, now + Duration::from_secs(2), 7);
        assert_eq!(peer.received_grant_sequence, Some(0));
        assert_eq!(peer.received_grants, 1);
        assert_eq!(peer.exchange_diagnostics.grant_accepted, 4);
        peer.subscribe(7, now + Duration::from_secs(3), |_| false);
        assert_eq!(peer.exchange_diagnostics.subscription_send_errors, 3);
        assert_eq!(peer.exchange_diagnostics.subscription_sends, 6);
        peer.exchange_diagnostics.subscription_send_errors = u64::MAX;
        peer.subscribe(7, now + Duration::from_secs(4), |_| false);
        assert_eq!(peer.exchange_diagnostics.subscription_send_errors, u64::MAX);
        let serialized = serde_json::to_value(peer.exchange_diagnostics).unwrap();
        assert_eq!(serialized["offset_resets"], 1);
        assert_eq!(serialized["source_resets"], 1);
        assert_eq!(serialized["grant_accepted"], 4);
        assert_eq!(
            serialized["subscription_send_errors"].as_u64(),
            Some(u64::MAX)
        );
    }
    #[test]
    fn delay_diagnostics_distinguish_send_failures_stale_responses_and_invalid_round_trips() {
        let now = Instant::now();
        let mut peer = PeerState {
            follow: true,
            ..Default::default()
        };
        followed_packet(&mut peer, &announce(8, 0, 0), 0, now, 7);
        let (sync, follow) = sync_pair(8, 3, 1_001_000_000);
        followed_packet(&mut peer, &sync, 1_000_100_000, now, 7);
        followed_packet(&mut peer, &follow, 1_000_100_000, now, 7);
        peer.delay_request_sent(5, 1_000_200_000, now, true);
        peer.delay_request_sent(6, 1_000_300_000, now, false);
        assert_eq!(peer.delay, Some((5, 1_000_200_000)));
        let mut response = header(9, 54, 8, 4, 0x0408, 0);
        timestamp(&mut response[34..44], 1_300_000_000);
        response[44..52].copy_from_slice(&7u64.to_be_bytes());
        response[52..54].copy_from_slice(&0x8005u16.to_be_bytes());
        followed_packet(&mut peer, &response, 1_000_400_000, now, 7);
        assert_eq!(peer.exchange_diagnostics.delay_resp_rejected, 1);
        response[30..32].copy_from_slice(&5u16.to_be_bytes());
        followed_packet(&mut peer, &response, 1_000_400_000, now, 7);
        assert_eq!(peer.exchange_diagnostics.rtt_rejections, 1);
        assert_eq!(peer.exchange_diagnostics.delay_resp_rejected, 2);
        assert_eq!(peer.exchange_diagnostics.accepted_exchanges, 0);
        timestamp(&mut response[34..44], 1_001_300_000);
        followed_packet(&mut peer, &response, 1_000_400_000, now, 7);
        assert_eq!(peer.exchange_diagnostics.accepted_exchanges, 1);
        assert_eq!(peer.exchanges, 1);
        assert!(!peer.ready(now));
        assert_eq!(peer.exchange_diagnostics.delay_req_sends, 1);
        assert_eq!(peer.exchange_diagnostics.delay_req_send_errors, 1);
    }

    #[test]
    fn wire_clock_and_lengths() {
        let id = 0x1234567812345678;
        let (s, f) = sync_pair(id, 65535, 1234567890);
        assert_eq!(&s[20..28], &id.to_be_bytes());
        assert_eq!(s.len(), 44);
        assert_eq!(f.len(), 96);
        assert_eq!(&f[34..44], &[0, 0, 0, 0, 0, 1, 13, 251, 56, 210]);
        assert_eq!(announce(id, 0, 0).len(), 76);
    }
}
