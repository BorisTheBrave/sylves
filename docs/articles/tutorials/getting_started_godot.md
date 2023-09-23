# Getting Started in Godot

As Sylves is a .NET library, it automatically works in Godot, if you have the .NET module loaded. You can use it by getting the latest release from [github releases](https://github.com/BorisTheBrave/sylves/releases), and adding that dll to the csproj file Godot generates:

```xml
  <ItemGroup>
    <Reference Include="Sylves">
      <HintPath>Sylves.dll</HintPath>
    </Reference>
  </ItemGroup>
```

There is no support at present for GDScript. Please let me know if this is something that would be useful to you.

There is experimental version of Godot available, Sylves.Godot. This version replaces Sylves.Vector3 etc with Godot equivalents, which is marginally easier to work with.

Alternatively, you can copy the source code of `src/Sylves` into your Unity project. If you do this, you can add GODOT to the scripting symbols defined in Player Settings to enable some implicit conversions to/from Godot types.