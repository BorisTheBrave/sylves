# Installation in Unity

There's several ways to install Sylves to Unity. 

## Install from UPM

Sylves is published to [OpenUPM](https://openupm.com/packages/com.boristhebrave.sylves/). Follow the [OpenUPM instructions](https://openupm.com/packages/com.boristhebrave.sylves/#modal-manualinstallation) or use their CLI.

## Manual Installation

Download the latest Unity.zip from the [github releases](https://github.com/BorisTheBrave/sylves/releases), and drop it in your assets folder.

##  Source Installation

Source installs allow you to debug into Sylves, but they compile a bit slower.

* Copy the source code of `src/Sylves` into your Unity project.
* Add UNITY to the scripting symbols defined in Player Settings.
* Delete the UnityShim/ folder, AssemblyInfo.cs and Sylves.csproj files.