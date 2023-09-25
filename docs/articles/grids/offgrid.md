# OffGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.OffGrid">OffGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.SquareCellType">SquareCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.SquareDir">SquareDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.SquareRotation">SquareRotation</a></td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.SquareBound">SquareBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Infinite</td></tr>
</table>

OffGrid is a [packed grid of irregular rectangles invented by Chris Cox](https://gitlab.com/chriscox/offgrid/-/wikis/home). It resembles rough masony.


<img width="200px" src="../../images/grids/off.svg" /></img>

Unlike most Sylves grids, this grid is not edge to edge. Every rectangle is considered a square with exactly 4 neighbours.

The size of cells can be controlled with the `minSize` parameter, which limits the minimum width and height of each rectangle. It also limits the max width/height to `2-minSize`. To get rectangles with a different distribution of size and aspect ratio, use [TransformModifier](../modifiers/transformmodifier.md).


## Cell co-ordinates

As with a square grid, increasing the x co-ordinate of a cell moves right, and the y co-ordinate up. 

The cell (0, 0) always includes the point (0, 0, 0), and and has maximum extents of ±(1-minSize/2).

