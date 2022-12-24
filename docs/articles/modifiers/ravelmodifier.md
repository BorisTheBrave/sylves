# RavelModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.RavelModifier">RavelModifier</a></td></tr>
<tr><td>CellType</td><td>Unchanged</td></tr>
<tr><td>CellDir</td><td>Unchanged</td></tr>
<tr><td>CellRotation</td><td>Unchanged</td></tr>
<tr><td>Bound</td><td>Unchanged</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>Base grid must be finite</td></tr>
</table>

This is a specific version of [biject modifier](bijectmodifier.md) that re-labels the cells so that the cell co-ordinates always have zero for the y value and z value, and the x value simply counts upwards from zero.

It does this by calling [GetIndex](xref:Sylves.IGrid.GetIndex(Sylves.Cell)) on the underlying grid.

For example, we could bound the square grid, then apply

```csharp
new RavelModifier(squareGrid.BoundBy(new SquareBound(new Vector2Int(-1, -1), new Vector2Int(2, 2)))),

```

<img width="200px" src="../../images/grids/center_square.svg" /></img> ➡ <img width="200px" src="../../images/grids/ravel_square.svg" /></img>

## Cell co-ordinates

Cell co-ordinates always have zero for the y value and z value, and the x value counts upwards from zero.