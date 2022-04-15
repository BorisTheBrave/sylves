# Topology

Topology is a fancy word for considering how the cells of a grid connect together, with no regard for their actual size or position in space.

In other words, it is treating a grid like it is a [graph](https://en.wikipedia.org/wiki/Graph_(abstract_data_type)), 
where nodes of the graphs are cells of the grid, and there are edges for each pair of adjacent cells. 
The edges of the graph are labelled with [rotation data](rotation.md), and the cells are labelled with their celltype.

Treating a grid as a graph is handy for many common algorithms, such as path finding and distance fields.

## TryMove

There's only one key method relating to the topology of a graphy.

```csharp
bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection);
```

`TryMove` attempts to move from `cell`, along the edge labelled with `dir`. If it is sucessfully, it returns the cell along the edge as `dest`,
and gives some information on the edge traversed, `inverseDir` and `connection`. See [rotations](rotation.md) for how to interpret these.

With `TryMove` defined, an algorithm can explore the entire grid, one cell at a time.

To make `TryMove` a bit easier to use, there is also `Move` which has a simpler signature, and the [`Walker`](xref:Sylves.Walker) class, which represents a object standing on a given cell facing a given direction.