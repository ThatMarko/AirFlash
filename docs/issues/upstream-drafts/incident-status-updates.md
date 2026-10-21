# Manual incident status updates — unsent

These are proposed status comments for the existing upstream reports. Posting them, closing an issue or changing the release still requires explicit authorization. The user identified the tested fork release and duration; this is not a new-head device qualification. Keep the consolidated PR's source-confirmed closing references to #14/#24.

## Issue #6 — receiver disappearance and disconnects

I've been using [my fork's AirFlash 0.4.1](https://github.com/ThatMarko/AirFlash/releases/tag/v0.4.1), built from `fb6721d70da072c289fa36e7d729cc743f53f505`, for a few days now without disconnects. The issue currently appears fixed in that tested fork build.

That release already contains the earlier session-retention and DNS-lifetime improvements. The upcoming consolidated PR refreshes those changes against current upstream while preserving its newer features. This observation is for the older fork release. Those observations do not cover the refreshed head or establish current upstream behavior or a specific root cause.

## Issue #7 — crackles or brief silence while connected

This issue currently appears fixed in [my fork's AirFlash 0.4.1](https://github.com/ThatMarko/AirFlash/releases/tag/v0.4.1), built from `fb6721d70da072c289fa36e7d729cc743f53f505`. My concrete follow-up observation is a few days of use without disconnects.

I have not isolated which change caused the improvement. This report does not establish a crackle-specific cause or manually qualify the newer upstream/rebased code. The refreshed PR preserves upstream's later audio-continuity work and carries forward the fork's reliability safeguards; its automated tests remain separate from this manual observation.

## Publication boundary

The issue author can choose to close the incidents as resolved/not reproduced in the tested fork. That decision is distinct from claiming the refreshed PR fixes their causes or that stable upstream already includes every fork safeguard. These drafts preserve the positive report without making either claim. Do not post or close anything during local preparation.
