# Index

Sylves identifies each cell with up to three co-ordinates, x, y, z.

But for some purposes, it's helpful to associate each cell with a single tightly integer instead, for example so you can [store data in an array](storage.md).

Sylves comes with methods to convert to such an integer, called an index. Indices will always be non-negative, and usually count upwards from zero skipping as few spots as possible. The conversion routines are usually fast.

* [`GetIndex`](xref:Sylves.IGrid.GetIndex(Sylves.Cell)) - Converts a cell to an index
* [`GetCellByIndex`](xref:Sylves.IGrid.GetCellByIndex(System.Int32))  - Converts an index back to a cell.

Indices are only available on finite grids.

> [!Note]
> When you change the [bounds](bounds.md) of a grid, the indices will change too. Use `Cell` to ensure stability.
