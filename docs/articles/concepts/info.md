# Info

Every grid exports some basic details about what sort of grid it is.

### [`Is2d`](xref:Sylves.IGrid.Is2d)

Are the cells flat surfaces. Note: Each cell may be flat but they can be placed at different angles leading to a grid that is not flat. E.g. the surface of a cube makes 2d grid, as are the majority of grids in Sylves. IsPlanar is used for grids that are completely flat.

### [`Is3d`](xref:Sylves.IGrid.Is3d)

Are the cells 3d shapes like [polyhedra](https://en.wikipedia.org/wiki/Honeycomb_(geometry)).

### [`IsPlanar`](xref:Sylves.IGrid.IsPlanar)

Are the cells confined to the XY plane. Implies Is2d.

### [`IsRepeating`](xref:Sylves.IGrid.IsRepeating)

Does this grid use the same cell shapes over and over.

### [`IsOrientable`](xref:Sylves.IGrid.IsOrientable)

A [non-orientable](https://en.wikipedia.org/wiki/Orientability) grid some paths on the grid end up mirroring your as you travel them. See [Connections](rotation.html#connection)

### [`IsFinite`](xref:Sylves.IGrid.IsFinite)

Is there a finite number of cells in the grid. Infinite grids don't support some operations. You can often limit an infinite grid to a finite one with [bounds](bounds.md) or the [mask modifier](../modifiers/maskmodifier.md).

### [`IsSingleCellType`](xref:Sylves.IGrid.IsSingleCellType)

Do all cells of the grid share a single [cell type](index.md#what-is-a-cell).

### [`GetCellTypes`](xref:Sylves.IGrid.GetCellTypes)

All the [cell types](index.md#what-is-a-cell) used by this grid.
