# CellType Canonical shape

A lot of methods about a cell can be found in ICellType.

But crucially, an instance of ICellType (e.g. SquareCellType.Instance) is shared by many cells from many different grids. Those cells might not be the same size, or even the same shape - Sylves will allow any 4 sided polygon to have SquareCellType if the grid declares it that way.

Most of the methods of ICellType work the same regardless of the shape of the cell, so are perfectly safe to be shared. But there's a small number of methods of that assume a specific shape, such as [`GetMatrix`](xref:Sylves.ICellType.GetMatrix(Sylves.CellRotation)) or [`GetCornerPosition`](xref:Sylves.ICellType.GetCornerPosition(Sylves.CellCorner)).

These methods assume a fixed size shape. These are documented on the specific implementation, but are listed here for convenience:

|CellType|Shape|[Inradius](https://mathworld.wolfram.com/Inradius.html)|[Circumradius](https://mathworld.wolfram.com/Circumradius.html)|
|--------|-----|-------------------------------------------------------|---------------------------------------------------------------|
|SquareCellType|Square of width/height 1 and centered at (0, 0).|0.5|√2 / 2
|CubeCellType|Cube of width/height/depth 1 and centered at (0, 0).|0.5|√3 / 2
|TriangleCellType (FlatTopped)|A upwards pointing equilateral triangle|0.5|√3 / 3
|TriangleCellType (FlatSides)|A left pointing equilateral triangle|0.5|√3 / 3
|HexCellType (PointyTopped)|A regular hexagon with a side normal to (0, +1).|0.5|
|HexCellType (FlatTopped)|A regular hexagon of with a side normal to (+1, 0).|0.5|
|NGonCellType|A regular n-gon with a side normal to (+1, 0).|0.5| 0.5 / cos(π / n)|
|NGonPrismCellType|As NGonCellType, but extruded ±0.5 along the z-axis.|

The inradius is set to 0.5, so that the distance from one cell center to a neighbours center is always 1.0.


## Deformations

[Deformations](shape.md#deformation) also uses canonical shapes. It's the area that every deformation maps *from*. If you want to deform tiles that aren't the canonical shape, you need to scale and translate them to that, first.