# Clatter CLI

Use the Clatter command-line executable to generate audio and save it as a .wav file.

[URLS]

Example call to write impact audio:

```powershell
./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --speed 1 --type impact --path out.wav
```

Example call to write scrape audio:

```powershell
./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --speed 1 --type scrape --scrape_material ceramic --duration 3 --path out.wav
```

Not all arguments are shown in these examples. For a full list of arguments:

```powershell
./clatter.exe --help
```
