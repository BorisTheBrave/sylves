# BijectModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.BijectModifier">BijectModifier</a></td></tr>
<tr><td>CellType</td><td>Unchanged</td></tr>
<tr><td>CellDir</td><td>Unchanged</td></tr>
<tr><td>CellRotation</td><td>Unchanged</td></tr>
<tr><td>Bound</td><td>Unchanged</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>None</td></tr>
</table>

This modifier re-labels the cell co-ordinates of the underlying grid. You must pass a pair of functions, one which to the underlying grid from the new convention, and one that converts from the new convention to the underlying grid.

For example,

```csharp
new BijectModifier(squareGrid, x => new Cell((x.x + x.y) / 2, (x.x - x.y) / 2, x.z), x => new Cell(x.x - x.y, x.x + x.y, x.z))
```

<img width="200px" src="../../images/grids/center_square.svg" /></img> ➡ <img width="200px" src="../../images/grids/biject_square.svg" /></img>

## Cell co-ordinates

Cell co-ordinates are changed according to user specified functions.