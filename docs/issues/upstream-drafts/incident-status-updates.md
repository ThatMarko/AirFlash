# Manual incident status updates — unsent

These are prepared status comments for the existing upstream reports. The user selected the closing relationships **Fixes #14**, **Fixes #24**, **Closes #6** and **Closes #7** in the consolidated PR draft. The first two are supported by source and regression evidence; the incident closures reflect the reporter's decision based on the older fork's field result. Posting, closing issues, publishing the PR or changing a release still requires explicit authorization.

## Issue #6 — receiver disappearance and disconnects

I've been using [my fork's AirFlash 0.4.1](https://github.com/ThatMarko/AirFlash/releases/tag/v0.4.1), built from `fb6721d70da072c289fa36e7d729cc743f53f505`, for a few days now. My follow-up was “No dissconects”. I consider this report resolved for my use and have selected its closure through the consolidated PR.

That release contains the earlier session-retention and DNS-lifetime improvements. The refreshed PR carries those safeguards forward against current upstream and preserves its newer features. My observation belongs to the older fork release; it does not identify a specific root cause or manually qualify the refreshed `bbe283c` candidate or current upstream.

## Issue #7 — crackles or brief silence while connected

I have also selected closure of this report after using [my fork's AirFlash 0.4.1](https://github.com/ThatMarko/AirFlash/releases/tag/v0.4.1), built from `fb6721d70da072c289fa36e7d729cc743f53f505`, for a few days. The concrete field result I supplied was “No dissconects”.

I am not adding a new crackle-specific or acoustic observation, and I have not isolated a root cause. This is my decision to close the report based on that older-fork experience. The refreshed PR's automated checks remain separate from this observation and do not manually qualify its `bbe283c` head on my receivers.

## Publication boundary

The four closing references are prepared locally. No status comment, upstream issue closure or PR publication is authorized by this preparation alone. Keep #25 open. When publication is separately authorized, preserve both the positive older-fork report and its evidence boundary; do not turn it into a causal claim or a new-head device test.
