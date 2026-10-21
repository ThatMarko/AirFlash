# Guard delayed receiver work with session ownership and user intent

Delayed discovery saves and automatic connection work can reach the controller after Play, Pair, Stop or shutdown has changed the user's intent. Ranking an already attempted preferred receiver can also hide another candidate, and address-only routing can substitute a stereo group for an explicitly selected manual endpoint.

This change captures lifecycle ownership and automatic intent atomically, revalidates delayed work under controller serialization and after cleanup, and separates automatic admission from explicit Toggle. Eligibility changes arriving during persistence also invalidate queued admission. Only admitted attempts affect attempted/last-used state; manual selections keep their exact host, port, identity and preferences. Existing current-stable transport refresh and retry snapshots are preserved, while unavailable or hidden active rows retain Stop presentation.

It also publishes the selected stream-rate snapshot. Previously that silent replacement could make the dispatcher discard every Connecting notification and leave an offline row inactive while its engine was still connecting. A controlled dispatcher run reproduced controller Connecting/application Idle/disabled Stop before the fix; the exact-current-snapshot regression now passes. Shutdown closes admission before awaiting settings or engine cleanup and still attempts later cleanup after earlier failures.

Validation:

- 521 Core tests passed with locked restore and a zero-warning Release solution build. The final culture-only harness update preserves the tested Core/test trees.
- Three isolated dispatcher runs passed 115 assertions each, observing formatting/UI cultures en-US/en-US, zh-TW/zh-CN and en-US/de-DE. Invalid culture produces a controlled isolated failure.
- Fixtures cover gated saves/controller cleanup, same-id replacement, queued eligibility changes, failed saves, candidate fallback/aliases, manual routing, empty/partial/reappearing discovery, both prior mute bits and shutdown ordering. Pair Stop availability fixtures hold before a PIN prompt opens.
- Full local raster, physical DPI/multimonitor and real receiver/audio behavior remain unqualified; synthetic services and temporary stores are used throughout these checks.

Verified source base: `439de6260e01efbfca528a12cf4d4c1010b3583e`. Proposed head: `5749b74a2cf10c613f47f9a9ba4d9d7fb663f650`.

Fixes #14