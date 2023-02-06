from os import getcwd, chdir
import tarfile
from zipfile import ZipFile
import re
from subprocess import call
from pathlib import Path
from github import Github, Repository
from github.GitRelease import GitRelease


def get_version() -> str:
    """
    :return: The version number of Clatter.Core.
    """

    assembly_info: str = Path("../Clatter/Clatter.Core/Properties/AssemblyInfo.cs").read_text(encoding="utf-8")
    return re.search(r'^\[assembly: AssemblyVersion\("(.*?)"\)\]', assembly_info, flags=re.MULTILINE).group(1)


def get_changelog(version: str) -> str:
    """
    :param version: The version.

    :return: The changelog for this version.
    """

    s = re.search("(# " + version.replace(".", r"\.") + r"((.|\n)*?))^# ",
                  Path("../changelog.md").read_text(encoding="utf-8"),
                  flags=re.MULTILINE)
    if s is None:
        return "Initial release."
    else:
        return s.group(2).strip()


def upload_github_release() -> None:
    """
    Create a new release and upload the files.
    """

    # Get the version.
    version = get_version()
    # Load the repo.
    token_path = Path("github_auth.txt")
    assert token_path.exists(), "GitHub token not found. You must have a valid token save to github_auth.txt"
    token: str = token_path.read_text(encoding="utf-8").strip()
    repo: Repository = Github(token).get_repo("alters-mit/clatter")
    # Make sure this is a new release.
    for release in repo.get_releases():
        if release.title == version:
            raise Exception(f"Release {version} already exists.")
    release: GitRelease = repo.create_git_release(tag=version,
                                                  name=version,
                                                  message=get_changelog(version=version),
                                                  target_commitish="main")
    print(f"Created release: {version}")
    # Get the paths.
    root_src_path: Path = Path("../Clatter/").absolute().resolve()
    clatter_cli_directory: Path = root_src_path.joinpath("Clatter.CommandLine/bin/Release/net7.0")
    clatter_cli_linux_path: Path = clatter_cli_directory.joinpath("linux-x64/publish").resolve()
    clatter_cli_osx_path: Path = clatter_cli_directory.joinpath("osx-x64/publish").resolve()
    clatter_cli_win_path: Path = clatter_cli_directory.joinpath("win-x64/publish").resolve()
    cwd = getcwd()
    # Upload the UNIX CLI executables.
    for exe_path, platform in zip([clatter_cli_linux_path, clatter_cli_osx_path], ["linux", "osx"]):
        chdir(str(exe_path))
        # Make it an executable.
        call(["wsl", "chmod", "+x", "clatter"])
        tar_name = f"clatter_{platform}.tar.gz"
        # Tar.
        with tarfile.open(name=tar_name, mode="w|gz") as f:
            f.add("clatter")
        chdir(cwd)
        tar_path = exe_path.joinpath(tar_name)
        # Upload.
        release.upload_asset(path=str(tar_path),
                             name=tar_name,
                             content_type="application/gzip")
        print(f"Uploaded: {tar_name}")
    # Upload the Windows CLI executable.
    chdir(str(clatter_cli_win_path))
    with ZipFile("clatter.zip", "w") as f:
        f.write("clatter.exe", arcname="clatter.exe")
    chdir(cwd)
    zip_path = clatter_cli_directory.joinpath("clatter_windows.zip").absolute()
    release.upload_asset(path=str(zip_path),
                         name="clatter_windows.zip",
                         content_type="application/gzip")
    print(f"Uploaded: clatter_windows.zip")
    # Upload the DLLs.
    clatter_core_path: Path = root_src_path.joinpath("Clatter.Core/bin/Release/Clatter.Core.dll").resolve()
    clatter_unity_path: Path = root_src_path.joinpath("Clatter.Unity/bin/Release/Clatter.Unity.dll").resolve()
    for dll_path in [clatter_core_path, clatter_unity_path]:
        release.upload_asset(path=str(dll_path),
                             name=dll_path.name,
                             content_type="application/x-dosexec")
        print(f"Uploaded: {dll_path}")


if __name__ == "__main__":
    upload_github_release()
