# How to Build Clatter

**Requirements:**

- Admin access to the clatter repo
- Windows 10
- WSL
- Jetbrains Rider

**First time only:**

In the terminal:

1. `wsl apt install update`
2. `wsl apt install python3.8 python3.8-dev libffi`
3. `wsl python3.8 -m pip install cffi`
4. `wsl python3.8 -m pip install PyGitHub`

**In Rider:**

1. Make sure you've updated the version in `Clatter.Core/Properties/AssemblyInfo.cs` and `Clatter.Unity/Properties/AssemblyInfo.cs`. If not, do so now and push a new commit to main.
2. Set the solution configuration to "Release".
3. Run the configuration "build_all".
