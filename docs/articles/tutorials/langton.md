# Langton's Ant

> [!Note]
> The source code for this tutorial can also be found in the <a href="https://github.com/BorisTheBrave/sylves-demos/tree/main/Assets/Langton">demo code</a>

[Langton's ant](https://en.wikipedia.org/wiki/Langton%27s_ant) is a simple mathematical game played on a square grid.

The idea is you imagine an ant on an infinite grid of white cells.

Each turn, it moves forward.
* If it lands on a white square, it turns 90 degrees clockwise, then flips the color of the square to black.
* If it lands on a black square, it turns 90 degrees counter-clockwise, then flips the color of the square to white.

Let's code this using Sylves. First, we need to define the grid we are working on. Sylves comes with [`SquareGrid`](../grids/squaregrid.md) for this purpose, which defaults to infinite size. We pass in 1f as the cell size, though the size of the grid isn't relevant to this tutorial.

```csharp
using Sylves;
var grid = new SquareGrid(1f);
```

Next, we'll store which cells are black or white. As there's an infinite amount of white cells, we'll just store the black ones, and assume everything else is white.

```csharp
var blackCells = new HashSet<Cell>();
```

Now we'll define the initial conditions. We need to know what cell the ant starts in, and what direction it is facing.

```csharp
var startCell = new Cell(0, 0);
var startDir = SquareDir.Up;
```

Next we'll definte a [`Walker`](xref:Sylves.Walker). Walkers are a utility class for easily representing an entity that moves on a grid. Note that we need to cast from `SquareDir` to `CellDir`. The latter is a [abstract direction shared between all grid types](../concepts/index.md#abstract-and-specific-types).

```csharp
var ant = new Walker(grid, startCell, (CellDir)startDir);
```

Finally, we can write a Step method that does the actual logic of the ant's movement.

```csharp
void Step()
{
    Console.WriteLine($"Ant is at {ant.Cell} and is facing {(SquareDir)ant.Dir}");
    ant.MoveForward();
    var isCurrentCellBlack = blackCells.Contains(ant.Cell);
    if (isCurrentCellBlack)
    {
        ant.TurnRight();
        blackCells.Remove(ant.Cell);
    }
    else
    {
        ant.TurnLeft();
        blackCells.Add(ant.Cell);
    }
}
```

That's it, you can now see the behaviour of the ant logged out with each step.
```
Ant is at (0, 0, 0) and is facing Up
Ant is at (0, 1, 0) and is facing Right
Ant is at (1, 1, 0) and is facing Down
Ant is at (1, 0, 0) and is facing Left
Ant is at (0, 0, 0) and is facing Up
Ant is at (0, 1, 0) and is facing Left
Ant is at (-1, 1, 0) and is facing Up
...
```

The nice thing about this code is it is very generic. The same code works with many other grids, you just need change the grid and initial conditions. Try some of the following and see how the ant behaves:


```csharp
var grid = new WrappingSquareGrid(1f, new Vector2Int(10, 10));
var startCell = new Cell(0, 0);
var startDir = SquareDir.Up;
```
```csharp
var grid = new HexGrid(1f, HexOrientation.FlatTopped);
var startCell = new Cell(0, 0, 0);
var startDir = FTHexDir.Up;
```