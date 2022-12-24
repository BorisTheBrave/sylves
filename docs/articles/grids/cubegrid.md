# CubeGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.CubeGrid">CubeGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.CubeCellType">CubeCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CubeDir">CubeDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CubeRotation">CubeRotation</a></td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.CubeBound">CubeBound</a></td></tr>
<tr><td>Properties</td><td>3d, Planar, Repeating, Infinite</td></tr>
</table>

CubeGrid is a 3d grid.

<img width="200px" src="../../images/grids/cube.png" /></img>

Increasing the x co-ordinate of a cell moves right, and the y co-ordinate up and z-cordinate forward. (though these are just labelling conventions in Sylves, you are free to use the co-ordinates differently).

The cell (0, 0, 0) has bounds from 0 to 1 in each axis (for a grid with cell size of 1). That means that the cell center of cell (0, 0, 0) is at `new Vector2(0.5f, 0.5f, 0.5f)`. This is the usual convention for square grids, but can be a bit surprising if youa r enot ready for it.

`CubeRotation` supports rotations on all three axes, so there are 48 possible values (of which 24 are various reflections).

CubeGrid can also represent cuboids, just use the constructor which accepts a Vector3 cellSize.