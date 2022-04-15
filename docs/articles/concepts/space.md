# Space

The spatial aspects of a grid are the easiest to understand. Each cell of the grid fills up some area or volume of space, usually in some sort of regular pattern.

But this is also the area of Sylves that has the most methods, which are covered below.

## Basics

* [`GetCellCenter`](xref:Sylves.IGrid.GetCellCenter(Sylves.Cell)) - Returns the center of a given cell
* [`GetTRS`](xref:Sylves.IGrid.GetTRS(Sylves.Cell)) - Returns the translation/rotation/scale of a given cell. Most basic grids have all cells at the same rotation and scale.

## Querying

Sylves contains many methods for querying how the cells are arragned in space.

* [`FindCell\](xref:Sylves.IGrid.FindCell(Sylves.Vector3,Sylves.Cell@)) - looks up the cell that contains a given point in space
* [`FindCell(Matrix4x4)`](xref:Sylves.IGrid.FindCell(Sylves.Matrix4x4,Sylves.Cell@,Sylves.CellRotation@)) - finds the cell and rotation best repsented by a given transform matrix.
* [`GetCellsIntersectsApprox`](xref:Sylves.IGrid.GetCellsIntersectsApprox(Sylves.Vector3,Sylves.Vector3)) - finds all cells that overlap a given axis aligned rectangle or cuboid. Can potentially return extra cells outside the rectangle.

## Shape

Coming soon...

## Deformation

For advanced grids, partciularly MeshGrid, each cell of the grid may be a different shape, even though they share the same cell type.

[TODO diagram]

In this case, you can call [`GetDeformation`](xref:Sylves.IGrid.GetDeformation(Sylves.Cell)) which returns a [`Deformation`](xref:Sylves.Deformation). This class assists in smoothly interpolating accross that irregular shape.

Each deformation is a continuous map, which maps a cell from its canonical shape (a regular square, triangle, etc) to the shape of a specific cell in the grid. This is done via linear/bilinear or trilinear interpolation as appropriate.