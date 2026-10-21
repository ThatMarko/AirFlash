# Identity

## Current-source boundary — 2026-10-21

Stable `439de626` adds software stereo and schema-4 receiver profiles/rates to this older map. Combined `6c1e2ef` preserves those upstream contracts and exact manual endpoints. Canonical persisted identity and an owned transport may differ after a delayed save; current session settings projection preserves owned member IDs/options unless a unique current transport binding or deliberate membership edit authorizes adoption. Accessory credential identity remains separate.

[Current source, acceptance and executed limits](../issues/PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

## Discovery preparation supplement — 2026-10-13

Current fork code `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` integrates standalone D `b8ee79511e2fa41ee02651dad83d50e008f9f680`, based on original `cf420d97700463c56adc31408510e8336e99e51c`. The helper wire preserves native OrdinalIgnoreCase TXT keys and exact values. Mixed-case deviceid/tsid/gpn/igl/cn reach aggregate identity, group name, leader and codecs; pk values remain ordinal, so Key/key conflict. Full TXT-change publication restores the original stable behavior. Distinct identities previously merged by a compatible shared endpoint split on an incompatible pk-only refresh; equal normalized device IDs still merge regardless of pk. Accessory /info credential keys and manual routing are unchanged.

[D acceptance](../issues/GROUP-D-ACCEPTANCE.md) records source, tests, reviews and limits. The earlier source map below retains its pinned historical baseline.

This page describes fork `main` at **`06237873dcb6394eb35e97a017c536843bd7743d`**, reviewed on 2026-10-12. It includes original stable `cf420d97700463c56adc31408510e8336e99e51c` and final B/C selection/load fixes. [Audit](audit.md) and [current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) distinguish the current integrated source from historical observations.

Catalog identity, explicit selection identity and native credential identity have separate ownership. The catalog's pure normalization/migration does not authenticate a receiver. A manual row preserves its own endpoint/port/options; the accessory `GET /info deviceID` remains the current-user DPAPI credential key.

[`ReceiverCatalog.Reconcile`](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverCatalog.cs#L8) runs on every discovery list before the view model updates its known map and panel. It clones the input settings, returns rewritten records and migrations, and performs no file or network I/O. Reconciliation also runs during playback, when receiver sockets already exist. The trigger around it is in [After discovery](after-discovery.md). Credentials are not an input. See [Credentials and IPC](credentials-and-ipc.md).

## Two kinds of id

[`ReceiverIdentity`](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverIdentity.cs) splits strings into broadcast identities and endpoints.

A broadcast identity is a nonempty normalized string that is not a `stereo:` id, not an `endpoint:` id, and not a parsed endpoint. A 12-digit hex MAC is stored lowercase with `:` and `-` removed. `AA:BB:CC:DD:EE:01` and `aabbccddee01` are synthetic examples that compare equal. Any other string is kept as trimmed, case-sensitive text. `IsBroadcast` is a syntactic classification; it does not authenticate the advertisement or require that the id actually appeared in DNS-SD.

An endpoint is a host followed by a final `:port`, optionally prefixed with `auto-endpoint:`. The parser trims outer brackets and accepts an IP address or DNS name; it even accepts an unbracketed IPv6 host when the text before the last colon parses as one. The port is 1–65535. `Endpoint` normalizes IP text, trims/lowercases DNS names, and prints IPv6 addresses in brackets. This shared identity parser's IPv6 support does not imply engine IPv6 support: discovery and session address resolution use IPv4.

`Resolve` normalizes the starting id and walks the alias map. A self-map returns that current id. A cycle or a non-broadcast target returns the normalized original id; an exhausted chain returns its final id. It cannot land on an endpoint or a `stereo:` row through this walk. It does not know the manual-receiver set, so rejection of a broadcast-shaped manual id is the catalog's separate responsibility.

`IsBroadcast` is how the catalog decides which strings may become the canonical device id.

## Inputs to one reconcile

| Input | Source |
| --- | --- |
| Receivers | `ReceiverAggregator` output, including stereo rows and their members |
| Settings | A clone of the in-memory settings. Manual ids are the set of `ManualReceivers` ids |
| Active id | The receiver in the session snapshot, if any |
| Last id | `settings.LastReceiverId` |

Physical devices are the peers of every non-manual row. A stereo row contributes its members, not the `stereo:` id itself. Endpoint counts are built from each physical device's aliases plus its current `address:port`. An endpoint is unique when that count is 1. A live identity maps to the physical ids that advertise it. An identity owned by two physical devices is not a safe alias target.

## One member

`ReconcileMember` runs for a standalone row and for each stereo member. Members are then ordered leader first, then by id. The group id `stereo:{tsid}` is not rewritten.

1. If the aggregator's id is already a manual id, the automatic row is renamed to `auto-endpoint:{host:port}` so the two records cannot share a key.
2. Observed ids are the row id, its aliases, and `deviceid`, dropping empties, manual ids, and endpoints that are not unique.
3. Broadcast identities in that set are the candidates that can become canonical.
4. An alias target is kept when it is not manual, every live owner of that name is this same physical row, and the resolved id is claimed by at most one physical device (`mappedOwners <= 1`). Two live speakers cannot collapse through a stale alias.
5. Saved preference keys are collected when they normalize or resolve into the candidate set and pass `Allowed`. They do not expand that set. `mappedOwners <= 1` is applied to mapped identity targets, not independently to every configured key.

The canonical id is the first match in this order:

1. The preferred mapped alias. Preference is normalized active id, then normalized last-used id, then ordinal normalized text, with ordinal original text as the final tie-breaker.
2. The active id, when it is one of the candidates.
3. The last-used id, on the same condition.
4. The preferred saved preference key that landed in the candidate set.
5. The row's own id, when that id is one of the broadcast identities. Otherwise the preferred broadcast identity.
6. The row id unchanged, when nothing above matched.

Every broadcast identity is then written in `ReceiverAliases` as pointing directly at that canonical id. Older alias keys whose resolved target is this device are rewritten the same way. The durable map is flattened at the end of the whole reconcile, so a chain `a -> b -> c` becomes `a -> c` and `b -> c`.

Advertised endpoint strings enter the returned receiver's `Aliases` only when unique. Saved preference, last-used, and active endpoint keys can join the migration map only when unique and currently advertised by that member. Nonunique endpoints are removed from the returned `Aliases` array. These endpoint associations are transient receiver aliases and migrations; current reconciliation does not insert them into `settings.ReceiverAliases`. It writes broadcast associations into that separate persistent map and flattens their targets. `Save` serializes the map; the next `Load` keeps an entry only when both key and value are broadcast identities. Normalized duplicate keys keep the first value. A `host:port`, `auto-endpoint:`, `endpoint:`, or `stereo:` entry left by an older file is dropped on load. Schema migration and the two backup files are in [Settings](settings.md).

Each accepted alias is recorded in the migration map as `alias -> canonical`. If a later member proposes a different target for an existing alias, the first target remains; the later proposal is not recorded. Manual aliases are skipped. Endpoint uniqueness and broadcast owner checks are the protections that normally prevent this ambiguity.

## Preference merge

`ApplyMigrations` groups migrations by their target. For each target it includes every saved source key and the target's own existing `ReceiverOptions`, orders those keys by normalized active id, then normalized last-used id, then ordinal normalized text and original text, and writes one record:

| Field | Rule |
| --- | --- |
| Hidden | True if any source was hidden |
| AutoConnect | `false` if any source was explicitly false. Otherwise the first non-null value in priority order |
| Volume, latency mode, custom buffer, standby seconds | The first value that is present, in the same priority order |

The target key receives that merged record. The other keys are removed from `settings.Receivers`. `LastReceiverId` is replaced when the migration map contains it and the id is not manual.

`NeedsBackup` is true when a previous preference key or last-used id actually changes. The view model requests the identity-backup save; `SettingsStore.SaveReceiverIdentityAsync` preserves `config.json.pre-receiver-identity.bak` on the first requested save when that backup is absent, then uses the shared recovery/save path.

## What stays put

- A manual receiver's id is never a migration source or target.
- A `stereo:{tsid}` id is not a member canonical id. Member options stay on the member ids. The visible pair has its own options, edited separately in Settings.
- The DPAPI filename hashes the accessory `deviceID` from `GET /info` after removing colons and applying ASCII lowercase. That native normalization does not trim whitespace or remove hyphens as the catalog normalizer does. This catalog does not rename credential files.
- The catalog protects distinct live physical ids from collapsing through stale broadcast aliases: `Allowed` requires every advertised owner of the target to be this row, and `mappedOwners` restricts a mapped target to one physical owner. It does not recheck `model`, `tsid`, or `pk`, and does not undo same-identity merges already performed by `ReceiverAggregator`.

`ReceiverIdentityTests` locks the edges: transitive merge in either service order, case-sensitive `pk`, MAC-shaped settings keys, ambiguous endpoints left unmigrated, manual ids left beside an automatic row at the same address, and a failed backup leaving the original file untouched.

## Explicit playback selection

[ReceiverSelection.ResolvePlayback](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverSelection.cs) immediately keeps explicit manual and group rows. Other selections resolve only through normalized canonical id or confirmed broadcast alias among online non-manual peers. More than one physical identity or group owner rejects. A standalone competing alias owner also makes group routing ambiguous. Address or address-plus-port equality is **not** a fallback; an endpoint-shaped canonical member id can match by that exact id, not by an inferred address association.

[AppViewModel.ToggleAsync](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L434) rechecks discovered availability/completeness and hidden options before new playback. Pair uses the same selection resolver. Both preserve manual endpoints beside complete, incomplete or hidden groups; no new manual credential namespace is created. Settings Pair takes the selected editor row but clones the application's applied settings, not the unsaved draft options. A new manual draft can therefore be paired without first persisting that row; accessory credentials and configuration have independent lifetimes.

## Identity persistence and session authority

The application captures snapshot/lifecycle owner/intent together before awaited identity saving. After a valid save, migrations remain useful even if a newer session exists, but retargeting requires the original authority after the await **and** under controller serialization. Ordinary metrics and native retries retain that owner. Same-id replacement or newer explicit intent invalidates old control work. Same-owner promotion can retain the engine when its effective signature is unchanged; promoted latency or transport changes can cause the applicable owned restart. See [After discovery](after-discovery.md) and [Session](session.md).

[SettingsStore.Load](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SettingsStore.cs) validates loaded manual structure before this consumer pipeline, while preserving raw rejected/migrated bytes and future-schema refusal. Valid nullable display names/overrides and root/equalizer extension fields remain compatible. Manual host validation does not perform DNS or impose every add-dialog syntax rule.

## Source and verification scope

Current [ReceiverIdentity](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverIdentity.cs), [ReceiverCatalog](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverCatalog.cs), [ReceiverSelectionTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/ReceiverSelectionTests.cs), [ReceiverIdentityTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/ReceiverIdentityTests.cs), [SessionLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/SessionLifecycleTests.cs) and WPF [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) cover normalization, live-owner conflicts, alias migration, backup failure, exact manual commands, delayed persistence/Pair, same-id replacement, metrics during promotion and unchanged/changed signatures. Executed results are source-bound in [B acceptance](../issues/GROUP-B-ACCEPTANCE.md) and [C acceptance](../issues/GROUP-C-ACCEPTANCE.md).

Current main's build/Core/CI qualification is complete; this documentation pass audits source and evidence. No real identities, endpoints, credentials, DNS or pairing are read/run here. Catalog tests do not establish arbitrary Windows discovery/authentication behavior.

## Implemented Group B behavior

This heading is retained for existing cross-links. The implemented selection/persistence contracts are the current narrative above. At [historical `41190e0`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292), Toggle substituted the first stereo group by shared address. Group B removed that branch and added serialized ownership; final B `442e8d8` is integrated in current main. Original stable `cf420d9`'s [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19) covers passive retention, not exact manual routing/atomic admission. Old [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) is closed without merge at its older snapshot. Earlier branch/composition results remain dated in acceptance records, not current publication or pending-reboot status.
