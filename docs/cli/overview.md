# Clatter CLI

Use the Clatter command-line executable to create audio.

Example call to save impact audio:

```powershell
./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --speed 1 --type impact --path out.wav
```

Example call to write scrape audio:

```powershell
./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --speed 1 --type scrape --scrape_material ceramic --duration 3 --path out.wav
```

For a full list of arguments:

```powershell
./clatter.exe --help
```
