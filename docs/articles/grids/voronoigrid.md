# VoronoiGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.VoronoiGrid">VoronoiGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a></td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a></td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a></td></tr>
<tr><td>Bound</td><td>None</td></tr>
<tr><td>Properties</td><td>2d, Planar, Finite</td></tr>
</table>

Creates a grid from a <a href="https://en.wikipedia.org/wiki/Voronoi_diagram">Voronoi diagram</a> based on a set of input points. <a href="http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/">RedBlobGames has a good description of using a Voronoi grid in games</a>.

<img class="grid-thumb" src="../../images/grids/voronoi.svg" /></img>

## VoronoiGridOptions

**ClipMin/ClipMax**

Limits the area in which cells are considered to be. Defaults to "large enough".

This ensures that all cells are finite, which makes a lot of Voronoi algorithms simpler.

**LloydRelaxationIterations**

Smooths the positions of the points with [Lloyd's Algorithm](https://en.wikipedia.org/wiki/Lloyd's_algorithm). Larger values gives a smoother result.

**BorderRelaxation**

Affects behaviour during Lloyd relaxation.

`BorderRelaxation.Pin` keeps outermost points fixed, while `BorderRelaxation.Relax` allows them to move, causing the points to contract inwards with each iteration.


## Cell co-ordinates

The x value is corresponds to the index of the input point that this cell is based on. y and z values are unused.

