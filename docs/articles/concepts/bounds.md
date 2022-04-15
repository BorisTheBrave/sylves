# Bounds

[`IBound`](xref:Sylves.IBound) is a generalization of the idea of a [bounding box](https://en.wikipedia.org/wiki/Minimum_bounding_box) to different sorts of grids.

A bound represents a collection of cells using a small class, which typically stores the min and max extends of the cells.
Bounds cannot represent *any* collection of cells, for that, you should use a `HashSet<Cell>`.
So bounds are best used in cases where it doesn't matter if a few too many cells are included, such as in many chunking and culling algorithms.

Not all grids support bounds. The ones that don't will simply return `null`, which represents bounds that cover the entire grid. When grids do support bounds, 
the specific class that grid uses is documented with the grid. The classes are usually named similarly: `SquareGrid` uses `SquareBound`, `HexGrid` uses `HexBound` and so on.

## Working with bounds

`IGrid` has many methods for working with bounds. Probably the most important are

* [`GetBound`](xref:Sylves.IGrid.GetBound(System.Collections.Generic.IEnumerable{Sylves.Cell})) - finds the smallest bound that contains all the given cells
* [`GetCellsInBounds`](xref:Sylves.IGrid.GetCellsInBounds(Sylves.IBound)) - finds all the cells inside a bound
* [`IsCellInBound`](xref:Sylves.IGrid.IsCellInBound(Sylves.Cell,Sylves.IBound)) - tests if a given cell is inside a bound.
* [`IntersectBounds`](xref:Sylves.IGrid.IntersectBounds(Sylves.IBound,Sylves.IBound))/[`UnionBounds`](xref:Sylves.IGrid.UnionBounds(Sylves.IBound,Sylves.IBound)) - combines two bounds into one.

Most of these methods will also accept a `null` bound, meaning the entire grid.

If you know the exact class of a bound, you can use that type instead of `IBound`. Most bounds have useful methods and constructors on them.

## Bounded grids

Grids themselves often are associated with a specific bound. For example, `new SquareGrid(1)` gives an *infinite* grid of squares. But you restrict the grid:

```csharp
var infiniteGrid = new SquareGrid(1);
var bound = new SquareBound(new Vector2Int(0,0), new Vector2Int(8,8));
var grid = infiniteGrid.BoundBy(bound);
```

The new grid returned only contains cells in the range `0 <= x < 8` and `0 <= y < 8`, like the 64 cells of a chess board. All grid methods, like `TryMove` or `GetCells` will respect this bound
and not return cells outside the bound.

To remove the bound from a grid, you can access [`grid.Unbounded`](xref:Sylves.IGrid.Unbounded).

Note that you can use [`MaskModifier`](xref:Sylves.MaskModifier) like `BoundBy`, except `MaskModifier` accepts an arbitrary set of cells.