# 0.1.1

- Added: `Clatter.Core.ExternalEntyPoint`. A convenient entry point for external (non-C#) applications for generating single-event Clatter audio.
- Clatter CLI: Removed a spurious `--min_speed` parameter from the help text output.
- Clatter CLI: Clamp the `--simulation_amp` to `(0-0.99)` instead of `(0-1)`.
- Clatter CLI: Refactored to use `ExternalEntryPoint`.

# 0.1.0

Initial release.
