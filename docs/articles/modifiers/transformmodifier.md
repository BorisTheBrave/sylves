# TransformModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.TransformModifier">TransformModifier</a></td></tr>
<tr><td>CellType</td><td>Unchanged</td></tr>
<tr><td>CellDir</td><td>Unchanged</td></tr>
<tr><td>CellRotation</td><td>Unchanged</td></tr>
<tr><td>Bound</td><td>Unchanged</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>None</td></tr>
</table>

This modifier applies a configurable linear transform to a grid. You can use it to rotate and resize grids.

For example, here it rotates the square grid into diamonds.

```csharp
squareGrid.Transformed(Matrix4x4.Rotate(Quaternion.Euler(0, 0, 45)))
```

<img width="200px" src="../../images/grids/center_square.svg" /></img> ➡ <img width="200px" src="../../images/grids/transform_square.svg" /></img>

This modifier can also be applied by extension method [Transformed](xref:Sylves.GridExtensions.Transformed(Sylves.IGrid,Sylves.Matrix4x4)).

## Cell co-ordinates

This modifier does not alter cell co-ordinates.