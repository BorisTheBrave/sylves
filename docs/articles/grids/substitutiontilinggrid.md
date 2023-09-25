# SubstitutionTilingGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.SubstitutionTilingGrid">SubstitutionTilingGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a>*</td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a>*</td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a>*</td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.SubstitutionTilingBound">SubstitutionTilingBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Infinite</td></tr>
<tr><td colspan="2"><small>*NGonCellType represents any polygon. But for 4 and 6 sided faces, the values overlap with SquareCellType and HexCellType, and the corresponding SquareDir, SquareRotation, PTHexDir, HexRotation.</small></td></tr>
</table>

SubstitutionTilingGrid implements configurable [subtitution tilings](https://en.wikipedia.org/wiki/Substitution_tiling), also known as inflation/deflation tilings. This is a common way of creating aperiodic tilings, that is, grids made of cells in a small variety of shapes, but the overall patern of the grid does not repeat.

The substitution tilings are configured via a set of [prototiles](xref:Sylves.Prototile), which at present are not documented further.


Sylves comes with several built-in substitution tilings.

<style>
.grid-thumb {width: 200px; min-width: 200px; height: 200px; }
</style>

[!include[](_substitution_table.md)]


## Cell co-ordinates

The x, y, z values are concatenated to form a 24 byte string that encodes the path to a specific cell. The cells nearest the origin will have y=z=0, which is usually sufficient.