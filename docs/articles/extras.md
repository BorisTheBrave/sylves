# Extras

Sylves comes with lots of other goodies.

## Walker

[`Walker`](xref:Sylves.Walker) is a utility for entities that move around on a grid. The [Langton's Ant tutorial](tutorials/langton.md) has an example of how to use it.

## Alternative Builds: Unity, Godot

The Unity and Godot builds of Sylves are self explanatory. They replace all of Sylves' vector and math objects with the equivalents from those libraries. This makes interoperation much easier.

They also add additional methods for working with engine objects, such as reading from Mesh objects.



## UnityShim

Sylves contains many classes that mirror math and vector objects from the Unity game engine. This allows the library to work the same in both Unity and non-Unity contexts.

When compiling for Unity, these classes are disabled. 

This can be useful if you are writing unity code, and also want to compile it standalone, e.g. for testing or external tools.

## Alternative Build: BigInt

This build replaces most the integers in the library with C#'s BigInteger type. This allows grids to scale to far larger sizes than the normal implementation. You can read more details on the [infinity page](concepts/infinity.md).

## Mesh operations

Many of the mesh operations that Sylves uses for mesh grids, or deformations, can be used without reference to grids at all. See the classes in the [Mesh subfolder](https://github.com/BorisTheBrave/sylves/tree/main/src/Sylves/Mesh).
