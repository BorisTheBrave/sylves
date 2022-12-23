
## Storage

In Sylves, a grid does not store any information at all, it just has information about the shape and arrangement of the cells.

In fact, Sylves doesn't come with code for storing data in a grid at all. If you want to create a storage layer, often called a Tilemap in games, then try one of the following.

### Use a `IDictionary<Cell, Tile>`

`Cell` is a lightweight class, and implements `IEquatable`, so it is perfect for use as a dictionary key. This is the most convient method for working in memory.

### Use an array

You can also store the data in a 1d array. This is the most convenient method for serialization.

`IGrid` comes with methods [`GetIndex`](xref:Sylves.IGrid.GetIndex(Sylves.Cell)) and [`GetCellByIndex`](xref:Sylves.IGrid.GetCellByIndex(System.Int32)) that convert a `Cell` object to a `int` suitable for use in a compact array. Property `IndexCount` gives the size of the array needed to fully contain the grid.

> [!Note]
> These index methods only work on finite grids. You may need to apply a [bound](bounds.md) to a infinite grid to restrict it to a finite set of cells.
