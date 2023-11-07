# JitteredSquareGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.JitteredSquareGrid">JitteredSquareGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a></td></tr>
<tr><td>Bound</td><td>None</td></tr>
<tr><td>Properties</td><td>2d, Planar, Infinite</td></tr>
</table>

An infinite version of the <a href="voronoigrid.md">VoronoiGrid</a> where the points are drawn from a known distribution.

To calculate the points, start with a unit square grid (i.e. `new SquareGrid(1.0f)`) and then generate a point uniformly randomly inside each cell. A Voronoi diagram is then generated for the cells. In practise, this works by generating Voronoi diagrams for chunks of cells, then stitching them together.

<img class="grid-thumb" src="../../images/grids/jitteredsquare.svg" /></img>

## Cell co-ordinates

Each cell in this grid has a point sampled from a particular cell of a `new SquareGrid(1.0f)`, and shares the same co-ordinate values as that square cell.

