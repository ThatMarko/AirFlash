# Equalizer

One equalizer is applied to the PCM of every member, before master gain. It is saved in `config.json` and can change during a live session. It does not restart the engine process. The send loop that consumes it is in [Engine](engine.md). Settings Apply, which commits or clears a preview, is in [Settings](settings.md).

## The filter

[`equalizer.rs`](../../native/airflash-engine/src/equalizer.rs) and [`EqualizerResponse`](../../desktop/AirFlash.Core/EqualizerSettings.cs) use the same ten peaking bands:

`31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000` Hz.

The editor labels the second band "63 Hz". The coefficient uses 62.5.

Each band is an RBJ peaking section. A gain of 0 is a wire (`b = 1, 0, 0`). Any other gain uses

```text
A = 10^(gain / 40)
ω = 2π f / rate
α = sin(ω) / (2 × 1.4)
```

`1.4` is the Q. The desktop headroom math uses that same alpha. Gains and the preamp must be finite and inside −12 to +12 dB on both sides. The sample rate must be 44100 or 48000.

The chain is bypassed when the equalizer is disabled, or when it is enabled and every band and the preamp are zero. Otherwise each channel runs the ten sections in order and then multiplies by the effective preamp.

## Headroom

When the equalizer is enabled, both implementations measure the peak of the summed band response at DC, at Nyquist, at each band center, and on a 4096-point log grid from 10 Hz up to `rate / 2`. Automatic attenuation is `−max(0, preamp + peak)`. The effective preamp is `preamp + attenuation`, so a boost that would peak above 0 dB is pulled back and a cut is left alone. Disabled or invalid settings report 0 dB of attenuation and 0 dB of effective preamp on the desktop. The engine still stores those two numbers on a successful `Prepared`.

The editor shows "Automatic attenuation" and "Effective preamp". It recomputes them on a background task after 50 ms of quiet, using the active stream rate when the session is active at 44100 or 48000, and otherwise the draft sample rate.

Five presets replace the bands and set the preamp to 0: Flat, Bass boost, Vocals, Pop, and Rock. Any other curve is Custom. Reset is the flat curve.

## Preview and the live command

Dragging a band updates the Settings draft immediately and calls `PreviewEqualizerAsync`. The session keeps that preview as the effective equalizer for the next `start` and for `set_equalizer`. The send loop waits 50 ms, then sends the latest sequence. Further edits during that wait collapse into one send. A reply whose sequence is not the latest edit is ignored. Apply and disposing Settings call `ClearEqualizerPreviewAsync`, which drops the preview and sends the saved equalizer.

`set_equalizer` is accepted only when all of these hold:

- The session id matches the running worker.
- The command's sequence is greater than 0, and greater than the sequence already applied.
- The worker was started with `start`. `probe` and `pair` leave the equalizer slot empty, so the command returns `equalizer_error`.
- The payload validates. Preparation runs on the command thread.

Success emits `equalizer_changed` with `auto_attenuation_db` and `effective_preamp_db`. Failure emits `equalizer_error` and does not stop playback. The desktop shows that message on the equalizer page.

The capture path reads the mailbox with `try_lock`. A contended lock skips the update for that packet instead of waiting. A newer curve crossfades over `rate / 50` frames, about 20 ms, and a curve that arrives during a fade is held until the fade finishes.

## Where it is illegal

`validate_start` checks the real equalizer, then validates a clone whose equalizer is the default flat disabled curve. A live `start` may therefore enable the filter. `validate`, which is what `probe` calls, rejects `enabled: true` with "finite probes require the equalizer to be disabled." The desktop start path never sends a probe. [`scripts/native_probe.py`](../../scripts/native_probe.py) is the finite harness, and it does not send an enabled equalizer.
