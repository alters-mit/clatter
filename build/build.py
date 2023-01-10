from pathlib import Path

root_src_path: Path = Path("../Clatter/").absolute().resolve()
clatter_core_path: Path = root_src_path.joinpath("Clatter.Core/bin/Release/Clatter.Core.dll").resolve()
clatter_unity_path: Path = root_src_path.joinpath("Clatter.Unity/bin/Release/Clatter.Unity.dll").resolve()
clatter_cli_directory: Path = root_src_path.joinpath("Clatter.CommandLine/bin/Release/net7.0")
clatter_cli_linux_path: Path = clatter_cli_directory.joinpath("linux-x64/publish/clatter").resolve()
clatter_cli_osx_path: Path = clatter_cli_directory.joinpath("osx-x64/publish/clatter").resolve()
clatter_cli_win_path: Path = clatter_cli_directory.joinpath("win-x64/publish/clatter.exe").resolve()
