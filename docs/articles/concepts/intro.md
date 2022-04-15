# Key Concepts

## Grids and cells

The central interface of Sylves is [IGrid](xref:Sylves.IGrid). Each grid type supported by Sylves has a different implementation of this class, such as [`CubeGrid`](xref:Sylves.CubeGrid) or [`HexGrid`](xref:Sylves.HexGrid).

`IGrid` is essentially stateless. It doesn't store any data, it just has a great deal of methods for querying every aspect of the grid.

For example,

```csharp
IEnumerable<Cell> GetCells();
```

GetCells returns all the cells of a grid. Each cell are individual locations in a grid. In a square grid, each square is one cell. In a hexagon grid, each cell is one hexagon, etc.

Cells are represented by the [`Cell`](xref:Sylves.Cell) class. A `Cell` is just (x,y,z) co-ordinate, it contains no other data. If you want to know something specific about a cell, you must query the grid that it came from (or in some cases, query the `ICellType` interface described below).

The methods of `IGrid` can be rougly split into several categories. The most important being:

* Basics - Factual info about the grid, such as if is 2d, infinite, etc
* [Topology](topology.md) - Information about moving about the grid. This generally treats the grid as a network of cells, with links between adjacent cells.
* [Position](space.md) - Describes how the cells are laid out in 3d space, and their shape.

Other categories include:

* Relatives - Provides other grids related to the current grid.
* Index - Converts between `Cell` and `int`, if you want to use an array for storage
* Bounds - Methods of dealing with bounding boxes on the grid.
* Query - Methods for finding cells in 3d space, such as point queries, raycasts, etc.

## Grid Abstraction

Because IGrid can represent such a diverse range of possible grids, it necessarily talks about objects in a very abstract way. Sylves handles that abstractions in two ways.

1) Interfaces, such as `IGrid` and `ICellType`. These which have concrete implementations that vary per grid, such as `SquareGrid` and `SquareCellType`.

2) Empty enumerations, such as `CellDir` and `CellRotation`. The values of those enums can be found in correspsponding classes, like `SquareDir` and `SquareRotation`. 

You, the user, may prefer to work entirely with concrete classes. They give better autocomplete and formatting, and in many cases offer additional properties not present on the interface. However, due to C#'s type system, it's often necessary to manually cast these interfaces and enums back and forth to their concerete equivalents.

This table gives a summary of some of the concrete classes backing each grid. The docs for each grid class list the same details.

|IGrid|ICellType|CellDir|CellRotation|IBound|
|-----|---------|-------|------------|------|
|`SquareGrid`|`SquareCellType`|`SquareDir`|`SquareRotation`|`SquareBound`|
|`HexGrid`|`HexCellType`|`HexDir`|`HexRotation`|`HexBound`|
|`TriangleGrid`|`HexCellType`|`PTHexDir` *or* `FTHexDir`|`HexRotation`|`HexBound`|
|`HexPrismGrid`|`HexPrismCellType`|`HexPrismDir`|`HexRotation`|`HexPrismBound`|
|`MeshGrid`|* |* |* | `null`|
|`MeshPrismGrid`|* |* |* | `null`|

\* = Varies

It's also worth noting that `SquareCellType` and `HexCellType` can be used interchangably with `NGonCellType`, allowing you to treat all polygon cells similarly.

## Cell types

While an `IGrid` contains the majority of Sylves methods, some more can be found on `ICellType`. ICellType considers just a single cell in isolation, and talks about how...
