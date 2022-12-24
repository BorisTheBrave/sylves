# Pathfinding

All grids in Sylves describe connections from cell to cell. They can therefore be considered as a [graph](https://en.wikipedia.org/wiki/Graph_(discrete_mathematics)) and regular [pathfinding](https://en.wikipedia.org/wiki/Pathfinding) algorithms can be run on them.

Sylves comes with a variety of useful, configurable functions for path finding. However, path finding is a large topic and you can likely find specialized routines elsewhere that are more appropriate.

Remember that Sylves grids only consider movement in adjacent squares, they do not allow diagonal motion. For now, there is no support for pathfinding with diagonals.

## Methods

The methods can be found in the [Pathfinding](xref:Sylves.Pathfinding) class. 

```csharp
public static CellPath FindPath(IGrid grid, Cell src, Cell dest, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
```
Finds a shortest path on grid from src to dest. A `CellPath` is returned, which is a collection of the steps needed to get from src to test.

Returns null if there is no path.

```csharp
public static float? FindDistance(IGrid grid, Cell src, Cell dest, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
```
Finds a shortest path on grid from src to dest, and returns the total length of that path.

Returns null if there is no path.

```csharp
public static Dictionary<Cell, float> FindDistances(IGrid grid, Cell src, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
```
Returns the result of FindDistance from src to every cell on the grid. This will loop indefinitely if the grid is infinite.


```csharp
public static Dictionary<Cell, float> FindRange(IGrid grid, Cell src, float maxRange, Func<Cell, bool> isAccessible = null, Func<Step, float?> stepLengths = null)
```
As FindDistances, but only returns cells within a given distance.

## isAccessible and stepLengths

Most of the pathfinding routines take some optional arguments.

### isAccessible
If isAccessible is set, the path only travels through points that this returns true for. It's equivalent to setting a MaskModifier on the grid itself.

The default is all cells are accessible.

### stepLengths

If stepLengths is set, configures the distance between adjacent cells.

The default is all steps are length 1.0.

This is commonly set to `Pathfinding.GetEuclidianDistanceMetric(grid)` which sets the step length equal to the distance between cell centers.

The stepLengths function takes a [Step](xref:Sylves.Step) and returns the length. It can return `null` to indicate that this step is not possible. Returning negative or infinite values is not allowed.

## Advanced API

[DijkstraPathfinding](xref:Sylves.DijkstraPathfinding) and [AStarPathfinding](xref:Sylves.AStarPathfinding) give direct access to the two pathfinding algorithms available. 

## Useful notes

* Internally, Sylves will usually use [Dijkstra's algorithm](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm), but it can use [A*](https://en.wikipedia.org/wiki/A*_search_algorithm) in some circumstances. A* is generally faster, but it requires more knowledge about the graph so is not always possible.
* Be careful path finding on infinite graphs! The algorithm might run forever if there's no possible route. 