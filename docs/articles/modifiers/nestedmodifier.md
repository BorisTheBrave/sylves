# NestedModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.NestedModifier">NestedModifier</a></td></tr>
<tr><td>CellType</td><td>*</td></tr>
<tr><td>CellDir</td><td>*</td></tr>
<tr><td>CellRotation</td><td>*</td></tr>
<tr><td>Bound</td><td>Unchanged</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>None</td></tr>
</table>

This modifier replaces every cell in the grid with a subgrid of cells. The original set of cells are called "chunk cell", and the grids replaceing them are "child grids". The cells of the child grid are "child cell".

This modifier is `abstract` - to use it, you must to inherit from it, and implement [`GetChildGrid`](xref:Sylves.NestedModifier.GetChildGrid(Sylves.Cell)). 

Using nested grid is quite tricky, consider using [`PlanarLazyMeshGrid`](../grids/planarlazymeshgrid.md) instead, which is a specialized case designed to be easier to work with.

<img width="200px" src="../../images/grids/center_square.svg" /></img> ➡ <img width="200px" src="../../images/grids/nested_square.svg" /></img>


## Cell co-ordinates

By default, each cell co-ordinate `(x, y, z)` is split in two:
* `(y, z, 0)` gives the chunk cell
* `(x, 0, 0)` gives the child cell

This behaviour can be controlled by overriding the Split/Combine methods.