from pathlib import Path

root = Path("html/highlight").resolve()
languages = ["bash", "csharp", "powershell", "python"]
for d in ["es/languages", "languages"]:
    for f in root.joinpath(d).resolve().iterdir():
        if f.stem.replace(".min", "") not in languages:
            f.unlink()