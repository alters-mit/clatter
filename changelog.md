# 0.1.4

- `ClatterManager.auto` and `ClatterManager.adjustAudioSettings` are now static fields.
- Added: `Sound.spatialize`

# 0.1.3

- Added: `ScrapeMaterialData.roughnessRatioExponent` An exponent for each scrape material's roughness ratio. A lower value will cause all scrape audio to be louder relative to impact audio.
- Fixed: Impact and scrape audio calculated speed in different ways. Now, scrapes calculate speed the same way that impacts have been calculated.
- Fixed: Crash in `ExternalEntryPoint.GetAudio()` when trying to load a scrape material.
- Clatter CLI: Added `--roughness_ratio_exponent`
- Adjusted the resonance values of some code examples.

# 0.1.2

- Resonance values are no longer clamped between 0 and 1. Now, they are clamped to be at least zero, and the documentation recommends keeping the value under 1.

# 0.1.1

- Added: `Clatter.Core.ExternalEntyPoint`. A convenient entry point for external (non-C#) applications for generating single-event Clatter audio.
- Clatter CLI: Removed a spurious `--min_speed` parameter from the help text output.
- Clatter CLI: Clamp the `--simulation_amp` to `(0-0.99)` instead of `(0-1)`.
- Clatter CLI: Refactored to use `ExternalEntryPoint`.

# 0.1.0

Initial release.
