# Algorithms

Sylves comes equiped with some usful algorithms for working with graphs and grids.

## Pathfinding

See [Pathfinding](pathfinding.md)

## KruskalMinimumSpanningTree

[`KruskalMinimumSpanningTree.Calculate`](xref:Sylves.KruskalMinimumSpanningTree.Calculate(Sylves.IGrid,System.Func{Sylves.Step,System.Nullable{System.Single}})) computes a [minimal spanning tree](https://en.wikipedia.org/wiki/Minimum_spanning_tree). That is, it returns a set of Step objects, such that you can use those just steps to step from any cell in the grid to any other. It returns the set of steps with the smallest summed stepLengths, which is a user customizable field.

## OutlineCells

['OutlineCells.Outline`](xref:Sylves.OutlineCells.TODO) TODO