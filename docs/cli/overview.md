# Clatter CLI

Use the Clatter command-line executable to create audio.

Example call:

```powershell
./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --type impact --path out.wav
```

For a full list of arguments:

```powershell
./clatter.exe --help
```

For a list of impact materials, see API documentation for `Clatter.Core.ImpactMaterial`.

To generate a scrape sound, add `--scrape_material [STRING]` and `--duration [FLOAT]`. For a list of scrape materials, see API documentation for `Clatter.Core.ScrapeMaterial`.

You can omit `--path [STRING]` to send the output to stdout.
