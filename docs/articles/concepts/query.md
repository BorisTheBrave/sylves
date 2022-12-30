
# Querying

Sylves contains many methods for querying how the cells are arranged in space.

### [`FindCell(Vector3)`](xref:Sylves.IGrid.FindCell(Sylves.Vector3,Sylves.Cell@))

Looks up the cell that contains a given point in space

### [`FindCell(Matrix4x4)`](xref:Sylves.IGrid.FindCell(Sylves.Matrix4x4,Sylves.Cell@,Sylves.CellRotation@))

Finds the cell and rotation best repsented by a given transform matrix.

### [`GetCellsIntersectsApprox`](xref:Sylves.IGrid.GetCellsIntersectsApprox(Sylves.Vector3,Sylves.Vector3))

Finds all cells that overlap a given axis aligned rectangle or cuboid. Can potentially return extra cells outside the rectangle.

### [`Raycast`](xref:Sylves.IGrid.Raycast(Sylves.Vector3,Sylves.Vector3,System.Single))

Finds all the cells intersecting a ray extending out from a given point.
