# SubstitutionTilingGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.SubstitutionTilingGrid">SubstitutionTilingGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a>*</td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a>*</td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a>*</td></tr>
<tr><td>Bound</td><td><a href="xref:SubstitutionTilingBound">SubstitutionTilingBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Infinite</td></tr>
<tr><td colspan="2"><small>*NGonCellType represents any polygon. But for 4 and 6 sided faces, the values overlap with SquareCellType and HexCellType, and the corresponding SquareDir, SquareRotation, PTHexDir, HexRotation.</small></td></tr>
</table>

SubstitutionTilingGrid implements configurable [subtitution tilings](https://en.wikipedia.org/wiki/Substitution_tiling), also known as inflation/deflation tilings. This is a common way of creating aperiodic tilings, that is, grids made of cells in a small variety of shapes, but the overall patern of the grid does not repeat.

The substitution tilings are configured via a set of [prototiles](xref:Sylves.Prototile), which at present are not documented further.

<style>
.grid-thumb {width: 200px; min-width: 200px; height: 200px; }
</style>

<table>
<tr>
    <th>Result</th>
    <th>Class</th>
</tr>
<tr>
    <td><a href="../../images/grids/domino.svg"><img class="grid-thumb" src="../../images/grids/domino.svg" /></img></td>
    <td><a href="xref:Sylves.DominoGrid">Domino Grid</a><br/>A grid of dominos (6 sided cells the shape of a rectangle) tiled in an aperiodic pattern.</td>
</tr>
<tr>
    <td><a href="../../images/grids/penrose_rhomb.svg"><img class="grid-thumb" src="../../images/grids/penrose_rhomb.svg" /></img></td>
    <td><a href="xref:Sylves.PenroseRhombGrid">Penrose Rhomb Grid</a><br/>Also known as the <a href="https://en.wikipedia.org/wiki/Penrose_tiling#Rhombus_tiling_(P3)">penrose P3 tiling.</a></td>
</tr>
</table>


> [!Note]
> Unlike other Sylves grids, SubstitutionTilingGrid is not constant time. Tiles further from the origin take logarithmic time to evaluate. If this is a serious problem, consider fronting the grid with a PlanarLazyMeshGrid, which can cache the output in chunks.

## Cell co-ordinates

The x, y, z values are concatenated to form a 24 byte string that encodes the path to a specific cell. The cells nearest the origin will have y=z=0, which is usually sufficient.