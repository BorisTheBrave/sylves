# Sets up upm branch with a Unity importable version of the Sylves source
# Requires, subdir upm set up as a git clone, checked out to branch upm

import os
import shutil
import re
from typing import List, IO, Tuple

UPM_DIR="upm/"

KEEP="KEEP"

RUNTIME_ASMDEF="""{
    "name": "Sylves",
    "references": [
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
"""

# This is the one file that needs a consistent meta file
RUNTIME_ASMDEF_META="""fileFormatVersion: 2
guid: eeb0c2632dc942b4a851016966687b2f
AssemblyDefinitionImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

PACKAGE_JSON="""{
  "name": "com.boristhebrave.sylves",
  "version": "VERSION",
  "displayName": "Sylves",
  "description": "Sylves is a pure C# utility for working with 2d and 3d grids",
  "unity": "2019.1",
  "unityRelease": "0b5",
  "documentationUrl": "https://www.boristhebrave.com/docs/sylves/1",
  "dependencies": {
  },
  "keywords": [
    "grid"
  ],
  "author": {
    "name": "BorisTheBrave",
    "email": "boris@boristhebrave.com",
    "url": "https://www.boristhebrave.com"
  }
}

"""

def parse_if(line) -> Tuple[str, bool]:
    expr = line.strip()
    invert = False
    if expr.startswith("!"):
        invert = True
        expr = expr[1:]
    return (expr, invert)

def preprocess_cs_file(lines: List[str], defines: List[str], keep_defines: List[str]):
    if_state = []
    for line in lines:
        stripped_line = line
        if stripped_line.startswith("#if"):
            (expr, invert) = parse_if(stripped_line[3:])
            if expr in keep_defines:
                if_state.append(KEEP)
            else:
                if_state.append(invert ^ (expr in defines))
                continue
        elif stripped_line.startswith("#else"):
            if if_state[-1] is KEEP:
                pass
            else:
                if_state[-1] = not if_state[-1]
                continue
        elif stripped_line.startswith("#endif"):
            if if_state[-1] is KEEP:
                if_state.pop()
            else:
                if_state.pop()
                continue
        elif stripped_line.startswith("#pragma") or stripped_line.startswith("#region") or stripped_line.startswith("#endregion"):
            pass
        elif stripped_line.startswith("#"):
            raise Exception("Unknown directive: "+ line)
        if False not in if_state:
            yield line

def preprocess_file(filename: str, defines: List[str], keep_defines: List[str]):
    if filename.endswith(".cs"):
        with open(filename, 'r', encoding="utf_8_sig") as infile:
            lines = infile.readlines()
            lines = preprocess_cs_file(lines, defines, keep_defines)
        with open(filename, 'w+', encoding="utf_8_sig") as outfile:
            outfile.writelines(lines)

def preprocess_dir(dir):
    for (dirpath, _, filenames) in os.walk(dir):
        for filename in filenames:
            infilename = os.path.join(dirpath, filename)

            # Filter out files that aren't wanted
            is_cs_file = filename.endswith(".cs")
            if not is_cs_file: continue

            # Copy file
            preprocess_file(infilename, ["UNITY"], [])

def get_version():
    is_preview = False
    for line in open("docs/articles/release_notes.md", "r").readlines():
        if line.startswith("#"):
            version = line[1:].strip().lower().lstrip("v")
            if version == "unreleased":
                is_preview = True
                continue
            if not re.fullmatch("\d+\.\d+.\d", version):
                raise Exception(f"Version doesn't appear to be semver: {version}")
            return version + ("-preview" if is_preview else "")

    raise Exception("Couldn't find version")

def build_upm_release():
    # Copy source data
    shutil.rmtree(UPM_DIR + "Runtime/", ignore_errors=True)
    ignored = shutil.ignore_patterns("bin", "obj", "UnityShim", "AssemblyInfo.cs", "Sylves.csproj")
    shutil.copytree("src/Sylves/", UPM_DIR + "Runtime/", ignore=ignored)
    preprocess_dir(UPM_DIR + "Runtime/")

    # Copy other files
    shutil.copy("LICENSE.txt", UPM_DIR + "LICENSE.md")
    shutil.copy("README.md", UPM_DIR)
    shutil.copy("docs/articles/release_notes.md", UPM_DIR + "CHANGELOG.md")

    shutil.rmtree(UPM_DIR + "Documentation~/", ignore_errors=True)
    shutil.copytree("docs/_site/", UPM_DIR + "Documentation~/")

    # Copy files unique to UPM branch
    package_json = PACKAGE_JSON.replace("VERSION", get_version())
    open(UPM_DIR + "package.json", "w").write(package_json)
    open(UPM_DIR + "Runtime/Sylves.asmdef", "w").write(RUNTIME_ASMDEF)
    open(UPM_DIR + "Runtime/Sylves.asmdef.meta", "w").write(RUNTIME_ASMDEF_META)


if __name__ == "__main__":
    build_upm_release()