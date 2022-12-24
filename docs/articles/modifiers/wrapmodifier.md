# WrapModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.WrapModifier">WrapModifier</a></td></tr>
<tr><td>CellType</td><td>Unchanged</td></tr>
<tr><td>CellDir</td><td>Unchanged</td></tr>
<tr><td>CellRotation</td><td>Unchanged</td></tr>
<tr><td>Bound</td><td>Unchanged</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>Base grid must have <a href="../concepts/bounds.md">bounds applied</a></td></tr>
</table>

This modifier converts a grid into a wrapping grid, i.e. if you leave one side of the grid, you enter on the other side. It's a bit complex to use, you are recommended to use pre-made wrapping grids like [WrappingSquareGrid](xref:Sylves.WrappingSquareGrid) when possible.

The WrapModifier modifier works as follows. To take a step in the wrapped grid, first take a step in the unbounded grid, then jump from that cell to a "canonical" cell found inside the bounds of the grid.

So it's necessary to supply both a bounded grid, and a canonicalize function. The function is responsible for moving back inside.

In this example, we start with an infinite square grid. We then make a bounded grid that only contains 4 cells: (0, 0), (0, 1), (1, 0) and (1, 1). Finally, we make a `Canonicalize` function that replaces any cell in the infinte grid with the corresponding one of the 4 cells inside the bounds.

```csharp
var squareGrid = new SquareGrid(1);
var boundedSquareGrid = squareGrid.BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(2, 2)));
Cell? Canonicalize(Cell x)
{
    return new Cell(x.x % 2, x.y % 2);
}
var wrappedSquareGrid = new WrapModifier(boundedSquareGrid, Canonicalize);
```

The result is a grid with 4 cells. Moving both Up and Down from (0, 0) moves to (0, 1). 

<img width="200px" src="../../images/grids/center_square.svg" /></img> ➡ <img width="200px" src="../../images/grids/wrap_square_fake.svg" /></img>

In the diagram, I've used faded colors to indicate how the wrapping works - the grid will report there are only 4 actual cells.

## Cell co-ordinates

This modifier does not alter cell co-ordinates.