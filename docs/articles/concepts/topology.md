# Topology

Topology is a fancy word for considering how the cells of a grid connect together, with no regard for their actual size or position in space.

In other words, it is treating a grid like it is a [graph](https://en.wikipedia.org/wiki/Graph_(abstract_data_type)), 
where nodes of the graphs are cells of the grid, and there are edges for each pair of adjacent cells. 
The edges of the graph are labelled with [cell dirs](xref:Sylves.CellDir) and [rotation data](rotation.md), and the cells are labelled with their celltype.

Treating a grid as a graph is handy for many common algorithms, such as path finding and distance fields.

## TryMove

There's only one key method relating to the topology of a graph.

```csharp
bool IGrid.TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection);
```

`TryMove` attempts to move from `cell`, along the edge labelled with `dir`. If it is sucessful, it returns the cell along the edge as `dest`,
and gives some information on the edge traversed, `inverseDir` and `connection`. See [rotations](rotation.md#trymove-and-rotation) for how to interpret these.

With `TryMove` defined, an algorithm can explore the entire grid, one cell at a time.

If you don't care about rotation information, you can use the following simpler methods:

```csharp
Cell? IGrid.Move(Cell cell, CellDir dir);
IEnumerable<Cell> IGrid.GetNeighbours(Cell cell)
```

There is the [`Walker`](xref:Sylves.Walker) class, which represents a object standing on a given cell facing a given direction, and is a mutable interface to the same logic.

## ParallelTransport

In Sylves, all movement is one-cell-at-a-time using the TryMove method above. But in some cases, that's not really useful.

For example, you might be working with a square grid, and want to move 100 units right. Calling TryMove one hundred times is not really great.

If you are always working with a square grid, then you can simply add 100 to the x co-ordinate of the `Cell`, and you don't need this section.

But if you need something that works generally across grids, you need ParallelTransport.

```csharp
bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation);
```

`ParallelTransport` imagines you have a path from `aSrcCell` to `aDestCell` on `aGrid`, and then repeats that same path on the current grid, with a fresh starting point and rotation. It then tells you where the path finishes. Sylves will detect if the grids are sufficiently similar, and employ co-ordinate arithmetic if it can, otherwise it falls back to walking the two paths cell-by-cell.

For example:

```csharp
myGrid.ParallelTransport(new SquareGrid(1), new Cell(0, 0, 0), new Cell(100, 0, 0), new Cell(1, 2, 3), CubeRotation.Identity, out var destCell, out var destRotation)
```

Describes a path which moves 100 units to the right, and asks that path to rebased to start at (1,2,3). If myGrid is also a square grid, then this will result in `destCell = (101, 2, 3)`, just as if we'd done the straightforward arithmetic.

NB: [Grid symmetries](grid_symmetry.md) offers a similar feature to ParallelTransport, but only uses a single grid.