# Creating a grid

Sylves comes with a [large array of built in grids](grids/index.md), but sometimes you need something more custom. This page describes the various ways to get even more range from your grids.

## Use MeshGrid

[`MeshGrid`](xref:Sylves.MeshGrid) creates a grid which has one cell for every face in an input Mesh. Meshes are accepted in Unity's standard format, and can be created from a wide array of software and tools.

This is by far the easiest way to create practically useful grids as the tooling is so plentiful.

There's also [`MeshPrsimGrid`](xref:Sylves.MeshPrismGrid) which creates multiple cells per face, using extrusion to arrange the cells into layers, and [`PeriodicPlanarMeshGrid`](xref:Sylves.PeriodicPlanarMeshGrid) which repeats a mesh at fixed intervals across an infinite 2d plane.

## Use modifiers

If you just want to customize an existing grid, there are various modifiers you can apply that change an aspect of their behaviour.

* <xref:Sylves.BijectModifier> - changes the cell co-ordinate system
* <xref:Sylves.RavelModifier> - changes the cell co-ordinate system to lie along the x-axis
* <xref:Sylves.MaskModifier> - restricts the grid to certain cells, similar to bounds
* <xref:Sylves.TransformModifier> - applies a linear transformation to the physical space of the cells
* <xref:Sylves.PlanarPrismModifier> - converts a flat 2d grid into a 3d grid by making multiple layers
* <xref:Sylves.WrapModifier> - causes a grid to wrap at the edges

## Write your own grid

`IGrid` is of course an interface, and ultimately you can create your own implementations. Many of the more complex methods can be forwarded to methods in `DefaultGridImpl` which provides basic implementations.