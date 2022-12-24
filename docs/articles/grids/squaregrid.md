# SquareGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.SquareGrid">SquareGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.SquareCellType">SquareCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.SquareDir">SquareDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.SquareRotation">SquareRotation</a></td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.SquareBound">SquareBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Repeating, Infinite</td></tr>
</table>

SquareGrid is the most bog standard of all grids.

<img width="200px" src="../../images/grids/square.svg" /></img>

Increasing the x co-ordinate of a cell moves right, and the y co-ordinate up. The cell (0, 0) has bounds from 0 to 1 in each axis (for a grid with cell size of 1). That means that the cell center of cell (0, 0) is at `new Vector2(0.5f, 0.5f)`. This is the usual convention for square grids, but can be a bit surprising if you are not ready for it.


SquareGrid can also represent rectangles, just use the constructor which accepts a Vector2 cellSize.