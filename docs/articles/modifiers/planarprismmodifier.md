# PlanarPrismModifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.PlanarPrismModifier">PlanarPrismModifier</a></td></tr>
<tr><td>CellType</td><td>See below</td></tr>
<tr><td>CellDir</td><td>See below</td></tr>
<tr><td>CellRotation</td><td>See below</td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.PlanarPrismBound">PlanarPrismBound</a></td></tr>
<tr><td>Properties</td><td>3d</td></tr>
<tr><td>Requirements</td><td>Underlying grid must be 2d, planar</td></tr>
</table>

PlanarPrismModifier takes a 2d, planar grid, and extrudes it along the z-axis into the third dimension, optionally making multiple layers of grid.

<img width="200px" src="../../images/grids/cairo.svg" /></img> ➡ <img width="200px" src="../../images/grids/planarprismmodifier.png" /></img>


The cell types are mapped from the underlying corresponding 3d versions:

|2d|3d|
|--|--|
|SquareCellType|CubeCellType|
|HexCellType|HexPrismCellType|
|NGonCellType|NGonPrismCellType|

There are similar mappings for dirs and rotations.

# PlanarPrismOptions

The only settings are LayerHeight and LayerOffset, that function the same as the ones documented for [MeshPrismGrid](../grids/meshprismgrid.md).

## Cell co-ordinates

This modifier passes the x and y values to the underlying grid, and uses the z value for layers. So this modifier will break if the underlying grid also uses the z value. You can use the [RavelModifier](ravelmodifier.md) to convert grids to only use the x value.