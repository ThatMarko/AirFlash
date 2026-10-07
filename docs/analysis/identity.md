# Identity

[`ReceiverCatalog.Reconcile`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverCatalog.cs#L8) runs on every discovery list before the view model updates its known map and panel. It clones the input settings, returns rewritten records and migrations, and performs no file or network I/O. Reconciliation also runs during playback, when receiver sockets already exist. The trigger around it is in [After discovery](after-discovery.md). Credentials are not an input. See [Credentials and IPC](credentials-and-ipc.md).

## Two kinds of id

[`ReceiverIdentity`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverIdentity.cs) splits strings into broadcast identities and endpoints.

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

`NeedsBackup` is true when any previous preference key or the previous last-used id actually changes. The view model then copies `config.json` to `config.json.pre-receiver-identity.bak` once, on the first such save.

## What stays put

- A manual receiver's id is never a migration source or target.
- A `stereo:{tsid}` id is not a member canonical id. Member options stay on the member ids. The visible pair has its own options, edited separately in Settings.
- The DPAPI filename hashes the accessory `deviceID` from `GET /info` after removing colons and applying ASCII lowercase. That native normalization does not trim whitespace or remove hyphens as the catalog normalizer does. This catalog does not rename credential files.
- The catalog protects distinct live physical ids from collapsing through stale broadcast aliases: `Allowed` requires every advertised owner of the target to be this row, and `mappedOwners` restricts a mapped target to one physical owner. It does not recheck `model`, `tsid`, or `pk`, and does not undo same-identity merges already performed by `ReceiverAggregator`.

`ReceiverIdentityTests` locks the edges: transitive merge in either service order, case-sensitive `pk`, MAC-shaped settings keys, ambiguous endpoints left unmigrated, manual ids left beside an automatic row at the same address, and a failed backup leaving the original file untouched.

## Source and verification scope

| Claim group | Source anchors |
| --- | --- |
| Normalization, endpoint syntax, broadcast classification, alias walk | [`ReceiverIdentity.cs:7–46`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverIdentity.cs#L7) |
| Physical owners, clone, flattening and backup decision | [`ReceiverCatalog.cs:8–30`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverCatalog.cs#L8) |
| Member candidates, canonical priority and transient endpoints | [`ReceiverCatalog.cs:32–76`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverCatalog.cs#L32) |
| Preference merge, last id and covered members | [`ReceiverCatalog.cs:82–108`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverCatalog.cs#L82) |
| Persistent alias filtering and backup write | [`SettingsStore.cs:43–48, 80–84`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L43) |
| Separate credential key and filename | [`session.rs:231–238`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/session.rs#L231), [`credentials.rs:25–33`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/credentials.rs#L25) |

[`ReceiverIdentityTests`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/ReceiverIdentityTests.cs#L16) supplies direct tests of these pure catalog paths, including alias persistence across service loss/address change (line 119), canonical versus preference priority (lines 148–175), manual/ambiguous endpoint protection (lines 178–213), stale live-owner protection (line 224), address reuse (line 233), and both backup outcomes (lines 242–272). The tests do not establish that arbitrary malformed or hand-written settings satisfy every invariant; `Load` sanitizes aliases, but manual ids and preference keys are not all normalized on load. The catalog, not the credential store, owns these migrations.


## Implemented Group B behavior

The preceding audit remains the description of stable `41190e0`. This addendum describes local branch `codex/fix-session-lifecycle`, tested head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, based directly on that stable commit. This feature is included in local fork `main` at `72dc7c7df6649a95271e0a86ae62f371397b2ca4`; original upstream `main` remains at `41190e0` without these fixes. See the [feature acceptance checklist](../issues/GROUP-B-ACCEPTANCE.md) and [combined fork validation](../issues/FORK-INTEGRATION-ACCEPTANCE.md) for separate implementation and later integration evidence.

Manual selection preserves its exact host, port, id and independent options beside complete, incomplete or hidden discovered groups. An explicitly selected group stays selected. A discovered member resolves only through its canonical id or confirmed broadcast alias with one live physical owner and one group owner across online non-manual peers. A conflicting standalone owner also makes the selection ambiguous and rejects it before an engine command.

Address or endpoint equality is not a routing fallback; endpoint aliases alone do not prove ownership. Catalog alias migration, permitted automatic-attempt resets, independent manual persistence and schema 2 remain intact. Credentials still use the accessory `/info deviceID` and current-user DPAPI; selection creates no new credential namespace. Pure selection tests and fake command-capture cases are recorded in the checklist.
