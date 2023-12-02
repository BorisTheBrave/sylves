# Sets up upm branch with a Unity importable version of the Sylves source
# Requires, subdir upm set up as a git clone, checked out to branch upm

import os
import shutil
import re
import subprocess
import zipfile

# Pre-process Unity.Godot
subprocess.check_call(["dotnet","run","--project", "Sylves.GodotRewriter.csproj"], cwd="src/Sylves.GodotRewriter")

# Build solution
subprocess.check_call(["dotnet","build","src/Sylves.sln","-c","Release"])
subprocess.check_call(["dotnet","build","src/Sylves.sln","-c","UnityRelease"])
subprocess.check_call(["dotnet","build","src/Sylves.sln","-c","GodotRelease"])

# Build docs
subprocess.check_call(["docfx","docs/docfx.json"])

# Build zips
with zipfile.ZipFile('release/netstandard2.0.zip', 'w') as z:
    z.write("src/Sylves/bin/Release/netstandard2.0/Sylves.dll", "Sylves.dll")
    z.write("src/Sylves/bin/Release/netstandard2.0/Sylves.xml", "Sylves.xml")
    z.write("LICENSE.txt", "LICENSE.txt")

with zipfile.ZipFile('release/Unity.zip', 'w') as z:
    z.write("src/Sylves/bin/UnityRelease/netstandard2.0/Sylves.dll", "Sylves.dll")
    z.write("src/Sylves/bin/UnityRelease/netstandard2.0/Sylves.xml", "Sylves.xml")
    z.write("LICENSE.txt", "LICENSE.txt")

with zipfile.ZipFile('release/Godot.zip', 'w') as z:
    z.write("src/Sylves.Godot/bin/Release/net6.0/Sylves.Godot.dll", "Sylves.Godot.dll")
    z.write("src/Sylves.Godot/bin/Release/net6.0/Sylves.Godot.xml", "Sylves.Godot.xml")
    z.write("LICENSE.txt", "LICENSE.txt")

