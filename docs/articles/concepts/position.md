# Position

These grid methods return the position of specific cells.

### [`GetCellCenter`](xref:Sylves.IGrid.GetCellCenter(Sylves.Cell)) 

Returns the center of a given cell. The center is usually the polygon centroid, but it doesn't have to be.

### [`GetTRS`](xref:Sylves.IGrid.GetTRS(Sylves.Cell))

Returns the translation/rotation/scale of a given cell.

The translation will match GetCellCenter.

The rotation is aims to make it so the adjcent cell in a cell direction actually lies roughly where that cell direction implies. E.g. 

```csharp
var cell2 = grid.Move(cell1, SquareDir.Right);
var trs1 = grid.GetTRS(cell1);
var estimatedDirection = trs.Rotation * Vector3.Right;
var actualDirection = grid.GetCellCenter(cell2) - grid.GetCellCenter(cell1);
// Estimated direction will be roughly parallel to actualDirection.
```

The scale usually matches the cell size, if grids support it.

