# 0.1.2

- Resonance values are no longer clamped between 0 and 1. Now, they are clamped to be at least zero, and the documentation recommends keeping the value under 1.

# 0.1.1

- Added: `Clatter.Core.ExternalEntyPoint`. A convenient entry point for external (non-C#) applications for generating single-event Clatter audio.
- Clatter CLI: Removed a spurious `--min_speed` parameter from the help text output.
- Clatter CLI: Clamp the `--simulation_amp` to `(0-0.99)` instead of `(0-1)`.
- Clatter CLI: Refactored to use `ExternalEntryPoint`.

# 0.1.0

Initial release.
