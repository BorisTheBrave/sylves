# PlanarMeshGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.PlanarMeshGrid">PlanarMeshGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a>*</td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a>*</td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a>*</td></tr>
<tr><td>Bound</td><td>None</td></tr>
<tr><td>Properties</td><td>2d, Finite</td></tr>
<tr><td colspan="2"><small>*NGonCellType represents any polygon. But for 4 and 6 sided faces, the values overlap with SquareCellType and HexCellType, and the corresponding SquareDir, SquareRotation, PTHexDir, HexRotation.</small></td></tr>
</table>

Functions nearly the same as [MeshGrid](meshgrid.md). But requires all the vertices to be in the XY plane.

The main behaviour differences:
* GetCellsIntersectsApprox ignores the z-axis.
* FindCell and similar handle concave cells.
