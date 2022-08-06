# Grid Bestiary

This page provides a full list of grid classes available in Sylves.

## Basic Grids

These are the most common grids you are likely to want to use.

* <xref:Sylves.SquareGrid>
* <xref:Sylves.CubeGrid>
* <xref:Sylves.HexGrid>
* <xref:Sylves.TriangleGrid>

## Periodic Grids

Periodic grids have a pattern that repeats via translation. These are usually called [tessellations](https://en.wikipedia.org/wiki/Tessellation).

All the basic grids above are periodic, plus and some extra ones are supplied:
* <xref:Sylves.CairoGrid>
* <xref:Sylves.TriHexGrid>
* <xref:Sylves.MetaHexagonGrid>
* <xref:Sylves.SquareSnubGrid>
* <xref:Sylves.TetrakisSquareGrid>
* <xref:Sylves.RhombilleGrid>

## Mesh Grids

Mesh grids accept a [mesh](xref:Sylves.MeshData) as the input, and base cells of the grid off faces of the mesh.

* <xref:Sylves.MeshGrid> - Turns a mesh into a 2d grid, one cell per face.
* <xref:Sylves.MeshPrismGrid> - Turns a mesh into a 3d grid, one cell being one extruded face in a given layer.
* <xref:Sylves.PeriodicPlanarMeshGrid> - Can turn any planar mesh into a periodic grid by repeating the mesh at fixed intervals.

## Prism Grids

"Prism" are when you take a 2d polygon, and extrude it into a 3d shape. 
This can convert 2d grids into 3d ones, usually with the z-cordinate being the "layer", i.e. offset from the original grid.

* <xref:Sylves.MeshPrismGrid>
* <xref:Sylves.HexPrismGrid>
* <xref:Sylves.TrianglePrismGrid>
* <xref:Sylves.PlanarPrismModifier>

## Modifier grids

Modifier grids let you customize an existing grid by systematically changing it in some way.

* <xref:Sylves.BijectModifier> - Remaps the cells of the grid by changing their co-ordinates, without touching the position, shape or topology.
* <xref:Sylves.MaskModifier> - Filters the cells in the the grid to a customizable subset.
* <xref:Sylves.TransformModifier> - Changes the world space positioning of the grid by a linear transform, leaving everything else unchanged.
* <xref:Sylves.PlanarPrismModifier> - Takes a 2d planar grid, and extends it into multiple layers along the third the dimension.
* <xref:Sylves.WrapModifier> - Turns any bounded grid into a grid which connects back on itself when you leave the grounds. 

## Extra grids

These grids don't classify neatly and usually serve as demos for various features.

* <xref:Sylves.MobiusSquareGrid> - a square grid on a [Möbius strip](https://en.wikipedia.org/wiki/M%C3%B6bius_strip). Demonstrates how Sylves handles non-orientability on 2d surfaces.
* <xref:Sylves.CubiusGrid> - A torus with a quarter turn. Demonstrates how Sylves handles non-orientability on 3d surfaces.
* <xref:Sylves.WrappingSquareGrid> - WrapModifier applied to SquareGrid. This is a very common grid in games. 