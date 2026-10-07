# Identity

[`ReceiverCatalog.Reconcile`](../../desktop/AirFlash.Core/ReceiverCatalog.cs) runs on every discovery list, before the panel redraws and before any HomePod socket opens. It is a pure function of that list, the current settings, and the id that is playing. The trigger around it is in [After discovery](after-discovery.md). Credentials are not an input. See [Credentials and IPC](credentials-and-ipc.md).

## Two kinds of id

[`ReceiverIdentity`](../../desktop/AirFlash.Core/ReceiverIdentity.cs) splits strings into broadcast identities and endpoints.

A broadcast identity is anything that is not a `stereo:` id, not an `endpoint:` id, and not `host:port`. A 12-digit hex MAC is stored lowercase with `:` and `-` removed. `AA:BB:CC:DD:EE:01` and `aabbccddee01` compare equal. Any other non-endpoint string is kept as trimmed text.

An endpoint is `host:port` or `[ipv6]:port`, optionally prefixed with `auto-endpoint:`. The host must parse as an IP address or a DNS name. The port is 1–65535. `Endpoint` reprints an IPv6 address in brackets.

`Resolve` walks the alias map. It stops on a cycle, on a repeated id, or when the next id is not a broadcast identity. A non-broadcast target returns the original id, so an alias cannot land on a manual endpoint or a `stereo:` row through this walk.

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
5. Saved preference keys join the candidate set when they normalize or resolve into it, with the same owner checks.

The canonical id is the first match in this order:

1. The preferred mapped alias. Preference is the active id, then the last-used id, then ordinal normalized text.
2. The active id, when it is one of the candidates.
3. The last-used id, on the same condition.
4. The preferred saved preference key that landed in the candidate set.
5. The row's own id, when that id is one of the broadcast identities. Otherwise the preferred broadcast identity.
6. The row id unchanged, when nothing above matched.

Every broadcast identity is then written in `ReceiverAliases` as pointing directly at that canonical id. Older alias keys whose resolved target is this device are rewritten the same way. The durable map is flattened at the end of the whole reconcile, so a chain `a -> b -> c` becomes `a -> c` and `b -> c`.

Endpoint strings are added to the alias set only when they are unique and this member advertises them. They are removed from the returned `Aliases` array when they are not unique. `Reconcile` writes the flattened broadcast map into `settings.ReceiverAliases`, and that object is what `Save` serializes. The next `Load` keeps an entry only when both the key and the value are broadcast identities. Normalized duplicate keys keep the first value. A `host:port` or `endpoint:` alias that an older file still contains is dropped on that load, so it cannot become canonical after a restart. Until that restart, the in-memory map can still carry a unique endpoint for the current process. Schema migration and the two backup files are in [Settings](settings.md).

Each accepted alias is recorded in the migration map as `alias -> canonical`. A second canonical for the same alias is kept only when it equals the first. Manual aliases are skipped.

## Preference merge

`ApplyMigrations` groups migrations by their target. For each target it loads every saved `ReceiverOptions` whose key moved, orders those keys by active id, then last-used id, then ordinal text, and writes one record:

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
- The DPAPI filename is the hash of `deviceID` from `GET /info`. This catalog does not rename those files.
- A conflict that `ReceiverAggregator` refused to merge stays two physical devices here as well. `Allowed` rejects an identity whose live owners are not exactly this row.

`ReceiverIdentityTests` locks the edges: transitive merge in either service order, case-sensitive `pk`, MAC-shaped settings keys, ambiguous endpoints left unmigrated, manual ids left beside an automatic row at the same address, and a failed backup leaving the original file untouched.
