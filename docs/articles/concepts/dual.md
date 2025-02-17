# Dual Grids

Each grid in Sylves can be converted into a dual grid. The dual grid of a base grid has one cell for every unique vertex in the base grid, and one vertex for every cell.

Dual grids are based on a concept from maths of [dual graphs](https://en.wikipedia.org/wiki/Dual_graph)/[dual polyhedra](https://en./.wikipedia.org/wiki/Dual_polyhedron)/[dual tilings](https://mathworld.wolfram.com/DualTessellation.html).

Some grids (black) with the dual grid overlaid in red.


<figure>
<img src="../../images/duals/square_dual.svg" width="200"/><br/>
<img src="../../images/duals/hex_dual.svg" width="200"/><br/>
<img src="../../images/duals/tri_dual.svg" width="200"/>
</figure>

The dual of the square grid is another square grid, while the dual of a hex grid is a triangle grid, and visa versa.

## Working with Dual Grids

Dual grids are useful when you need to talk about the vertices of a base grid. Sylves already has an API for working with corners, i.e. using CellCorner wth ICellType, but it's not unique. For example, in a square grid, the corner `SquareCorner.TopRight` of cell `(0, 0)` refers to the same position as `SquareCorner.BottomLeft` of cell `(1, 1)`, and there are two other cells that have corners in that position.

```csharp
IGrid squareGrid = new SquareGrid(1);
Vector3 p1 = squareGrid.GetCellCorner(new Cell(0, 0), (CellCorner)SquareCorner.TopRight);
Vector3 p2 = squareGrid.GetCellCorner(new Cell(1, 1), (CellCorner)SquareCorner.BottomLeft);
// Both p1, and p2 will have value: new Vector3(0.5, 0.5, 0)

```

In Sylves terminology, we say each cell has its own set of corners, but these corners all correspond to a single vertex. Because vertices correspond to cells in the dual grid, they're more commonly called dual cells.

The dual grid api lets you find a unique cell value for each vertex. You'll get the same value regardless of which corner you queried it about.

```csharp
IDualMapping dualMapping = squareGrid.GetDual();
Cell? dualCell = dualMapping.ToDualCell(new Cell(0, 0), (CellCorner)SquareCorner.TopRight);
Cell? dualCell2 = dualMapping.ToDualCell(new Cell(1, 1), (CellCorner)SquareCorner.BottomLeft);
// Both dualCell and dualCell2 will have the same value: new Cell(1, 1)
```

You can also get a fresh grid for querying specifics about that dual cell.

```csharp
IGrid dualGrid = dualMapping.DualGrid;
Vector3 vertexCenter = dualGrid.GetCellCenter(dualCell);
// Returns new Vector3(0.5, 0.5, 0)
```

DualGrid is a fully functional grid, where the center of each dual cell will the corner of some cell in the base grid, and the corner of each cell in the dual grid corresponds to the center of some cell in the base grid.

There are many operations in `IDualMapping` for converting between the base grid and dual grid.


### [`DualGrid`](xref:Sylves.IDualMapping.DualGrid)

The dual grid to map to.

### [`BaseGrid`](xref:Sylves.IDualMapping.BaseGrid)

The grid this mapping was constructed from.

### [`ToDualPair`](xref:Sylves.IDualMapping.ToBasePair(Sylves.Cell,Sylves.CellCorner))

Finds the corresponding dual cell to a corner of a base cell, and the corner of the dual cell that returns back.

### [`ToBasePair`](xref:Sylves.IDualMapping.ToBasePair(Sylves.Cell,Sylves.CellCorner))

Finds the corresponding base cell to a corner of a dual cell, and the corner of the base cell that returns back.

### [`ToDualCell`](xref:Sylves.DualMappingExtensions.ToDualCell(Sylves.IDualMapping,Sylves.Cell,Sylves.CellCorner))

Finds the corresponding dual cell to a corner of a base cell.

### [`ToBaseCell`](xref:Sylves.DualMappingExtensions.ToBaseCell(Sylves.IDualMapping,Sylves.Cell,Sylves.CellCorner))

Finds the corresponding base cell to a corner of a dual cell.

### [`DualNeighbours`](xref:Sylves.DualMappingExtensions.DualNeighbours(Sylves.IDualMapping,Sylves.Cell))

Finds all dual cells that correspond to some corner of the base cell, and returns the corners and pairs.

### [`BaseNeighbours`](xref:Sylves.DualMappingExtensions.BaseNeighbours(Sylves.IDualMapping,Sylves.Cell))

Finds all base cells that correspond to some corner of the dual cell, and returns the corners and pairs.

---

There's also a number of methods on `ICellType` and `IGrid` for working with `CellCorner`.

### [`ICellType.GetCellCorners`](xref:Sylves.ICellType.GetCellCorners)
### [`ICellType.Rotate`](xref:Sylves.ICellType.Rotate(Sylves.CellCorner,Sylves.CellRotation))
### [`IGrid.GetDual`](xref:Sylves.IGrid.GetDual)
### [`IGrid.GetCellCorners`](xref:Sylves.IGrid.GetCellCorners(Sylves.Cell))
### [`IGrid.GetCellCorner`](xref:Sylves.IGrid.GetCellCorner(Sylves.Cell,Sylves.CellCorner))

## Inverse Corners

A few of the methods return an "`inverseCorner`". This is similar to inverseDir in `IGrid.TryMove`. It gives the details required
to go back to the original cell you started with. It has a number of technical uses, but is rarely needed for normal usage.

## Boundaries

Dual grids become a bit more fiddly on grids with boundaries. The idea is that vertices and cells are swapped when going base <-> dual, but that idea starts to break down at borders. In these cases, Sylves typically makes the dual grid *larger* than the base grid. That means there is a cell in the dual grid for every vertex in the base grid. But there may be some vertices in the dual grid that don't have a cell in the base grid. Some of the methods return nullable types for this reason.

<figure>
<img src="../../images/duals/bounded_square_dual.svg" width="200"/>
</figure>

## Marching squares example

A tutorial showing the use of dual grids can be found in the [Marching Squares Tutorial](../tutorials/marching_squares.md).