# Extras

Sylves comes with lots of other goodies.

## Walker

[`Walker`](xref:Sylves.Walker) is a utility for entities that move around on a grid. The [Langton's Ant tutorial](tutorials/langton.md) has an example of how to use it.

## UnityShim

Sylves contains many classes that mirror math and vector objects from the Unity game engine. This allows the library to work the same in both Unity and non-Unity contexts.

When compiling for Unity, these classes are disabled.

## Mesh operations

Many of the mesh operations that Sylves uses for mesh grids, or deformations, can be used without reference to grids at all. See the classes in the Mesh subfolder.
