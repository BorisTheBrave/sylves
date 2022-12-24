# TriangleGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.TriangleGrid">TriangleGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.HexCellType">HexCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.PTHexDir">PTHexDir</a> / <a href="xref:Sylves.PTHexDir">FTHexDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.HexRotation">HexRotation</a></td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.TriangleBound">TriangleBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Repeating, Infinite</td></tr>
</table>

TriangleGrid is a tiling of the plane with triangles. It supports both flat-topped and flat-sides varieties.

<img width="200px" src="../../images/grids/tri_fs.svg" /></img>
<img width="200px" src="../../images/grids/tri_ft.svg" /></img>

## Cell co-ordinates

The co-ordinate systems used are shown above - it uses three co-ordinates that always sum to 1 (for left and down pointing triangles) or 2 (for right or up pointing triangles). This scheme is explained more on [my blog](https://www.boristhebrave.com/2021/05/23/triangle-grids/).

TriangleGrid comes with methods [IsUp](xref:Sylves.TriangleGrid.IsUp(Sylves.Cell))/[IsDown](xref:Sylves.TriangleGrid.IsDown(Sylves.Cell))/[IsLeft](xref:Sylves.TriangleGrid.IsLeft(Sylves.Cell))/[IsRight](xref:Sylves.TriangleGrid.IsRight(Sylves.Cell)) to determine which direction a given cell is pointing.

## Hexagonal basis

Note that Triangle grid shares several classes in common with hexagons. E.g. A flat-topped triangle grid uses `FTHexDir` to get the list of directions, and flat-sides triangle grid uses `PTHexDir`.

That means, for an up-pointing triangle, it has adjacent triangles `FTHexDir.Down`, `FTHexDir.UpLeft` and `FTHexDir.UpRight`. The other three directions, `FTHexDir.Up`, `FTHexDir.DownRight` and `FTHexDir.DownLeft` are not used for an up-pointing triangle. It's the other way around for a downward pointing triangle, and similar for left/right.

Hexes are used in these cases so that triangles pointing in opposite directions do not re-use the same `CellDir` values to mean different things. Sylves supports that, but it would effectively mean you'd have to deal with some triangles being rotated 180 degress compared to others. [Rotation](../concepts/rotation.md) is a concept best avoided where possible.

If you *do* want a triangle grid with only 3 directions, using `NGonCellType.Get(3)`, and so on, then you can construct one using PeriodicPlanarMeshGrid.