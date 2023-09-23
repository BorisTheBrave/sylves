# Installation in Godot

## Manual Install

As Sylves is a .NET library, it automatically works in Godot, if you have the .NET module loaded. You can use it by getting the latest release from [github releases](https://github.com/BorisTheBrave/sylves/releases), and adding that dll to the csproj file Godot generates:

```xml
  <ItemGroup>
    <Reference Include="Sylves">
      <HintPath>Sylves.dll</HintPath>
    </Reference>
  </ItemGroup>
```

There is no support at present for GDScript. Please let me know if this is something that would be useful to you.

## Sylves.Godot

There is experimental version of Godot available, Sylves.Godot. This version replaces Sylves.Vector3 etc with Godot equivalents, which is marginally easier to work with. It is installed the same as a Manual Install

## Source Installation

Alternatively, you can copy the source code of `src/Sylves` into your Godot C# project project. If you do this, you can add GODOT to defines in your csproj to enable some implicit conversions to/from Godot types.

```xml
<PropertyGroup>
	<DefineConstants>GODOT</DefineConstants>
</PropertyGroup>
```