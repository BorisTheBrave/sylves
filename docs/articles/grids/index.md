# Grid Bestiary

This page provides a full list of grid classes available in Sylves. You can always [create or customize your own grid](creating.md) if the ones here don't quite match your purpose.


<style>
.grid-thumb {width: 200px; min-width: 200px; height: 200px; }
</style>

## Basic Grids

These are the most common grids you are likely to want to use.


<table>
<tr>
    <td><a href="../../images/grids/square.svg"><img class="grid-thumb" src="../../images/grids/square.svg" /></img></td>
    <td><a href="squaregrid.md">Square Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/cube.cube"><img class="grid-thumb" src="../../images/grids/cube.png" /></img></td>
    <td><a href="cubegrid.md">Cube Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/hex_pt.svg"><img class="grid-thumb" src="../../images/grids/hex_pt.svg" /></img></td>
    <td><a href="hexgrid.md">Hex Grid</a><br/>Supports both pointy topped and flat topped variants.</td>
</tr>
<tr>
    <td><a href="../../images/grids/tri_fs.svg"><img class="grid-thumb" src="../../images/grids/tri_fs.svg" /></img></td>
    <td><a href="trianglegrid.md">Triangle Grid</a><br/>Supports vertical and horizontal variants.</td>
</tr>
</table>

## Periodic Grids

Periodic grids have a pattern that repeats via translation. These are usually called [tessellations](https://en.wikipedia.org/wiki/Tessellation). 

All the basic grids above are periodic, but some extra ones are supplied.


<table>
<tr>
    <td><a href="../../images/grids/cairo.svg"><img class="grid-thumb" src="../../images/grids/cairo.svg" /></img></td>
    <td><a href="xref:Sylves.CairoGrid">Cairo Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/trihex.svg"><img class="grid-thumb" src="../../images/grids/trihex.svg" /></img></td>
    <td><a href="xref:Sylves.TriHexGrid">TriHex Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/metahexagon.svg"><img class="grid-thumb" src="../../images/grids/metahexagon.svg" /></img></td>
    <td><a href="xref:Sylves.MetaHexagonGrid">MetaHexagon Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/squaresnub.svg"><img class="grid-thumb" src="../../images/grids/squaresnub.svg" /></img></td>
    <td><a href="xref:Sylves.SquareSnubGrid">SquareSnub Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/tetrakissquare.svg"><img class="grid-thumb" src="../../images/grids/tetrakissquare.svg" /></img></td>
    <td><a href="xref:Sylves.TetrakisSquareGrid">TetrakisSquare Grid</a></td>
</tr>
<tr>
    <td><a href="../../images/grids/rhombille.svg"><img class="grid-thumb" src="../../images/grids/rhombille.svg" /></img></td>
    <td><a href="xref:Sylves.RhombilleGrid">Rhombille Grid</a></td>
</tr>
</table>

You can create your own periodic grids with <xref:Sylves.PeriodicPlanarMeshGrid>, described below.

## Mesh Grids

Mesh grids accept a [mesh](xref:Sylves.MeshData) as the input, and base cells of the grid off faces of the mesh.

<table>
<tr>
    <td><a href="../../images/grids/meshgrid.svg"><img class="grid-thumb" src="../../images/grids/meshgrid.svg" /></img></td>
    <td><a href="xref:Sylves.MeshGrid">Mesh Grid</a><br/>Turns a mesh into a 2d grid, one cell per face. Doesn't need to be planar.</td>
</tr>
<tr>
    <td><a href="../../images/grids/meshprism.png"><img class="grid-thumb" src="../../images/grids/meshprism.png" /></img></td>
    <td><a href="xref:Sylves.MeshPrismGrid">MeshPrismGrid</a><br/>Turns a mesh into a 3d grid, one cell being one extruded face in a given layer.</td>
</tr>
<tr>
    <td><a href="../../images/grids/cairo.svg"><img class="grid-thumb" src="../../images/grids/cairo.svg" /></img></td>
    <td><a href="xref:Sylves.PeriodicPlanarMeshGrid">PeriodicPlanarMeshGrid</a><br/>Can turn any planar mesh into a periodic grid by repeating the mesh at fixed intervals.</td>
</tr>
</table>

## Prism Grids

"Prism" are when you take a 2d polygon, and extrude it into a 3d shape. 
This can convert 2d grids into 3d ones, usually with the z-cordinate being the "layer", i.e. offset from the original grid.

<table>
<tr>
    <td><a href="../../images/grids/meshprism.png"><img class="grid-thumb" src="../../images/grids/meshprism.png" /></img></td>
    <td><a href="xref:Sylves.MeshPrismGrid">MeshPrismGrid</a><br/>Turns a mesh into a 3d grid, one cell being one extruded face in a given layer.</td>
</tr>
<tr>
    <td><a href="../../images/grids/hexprism.png"><img class="grid-thumb" src="../../images/grids/hexprism.png" /></img></td>
    <td><a href="xref:Sylves.HexPrismGrid">HexPrismGrid</a><br/>A special case of PlanarPrismModifier applied to a hex grid. </td>
</tr>
<tr>
    <td><a href="../../images/grids/triangleprism.png"><img class="grid-thumb" src="../../images/grids/triangleprism.png" /></img></td>
    <td><a href="xref:Sylves.TrianglePrismGrid">TrianglePrismGrid</a><br/>A special case of PlanarPrismModifier applied to a triangle grid. </td>
</tr>
<tr>
    <td><a href="../../images/grids/planarprismmodifier.png"><img class="grid-thumb" src="../../images/grids/planarprismmodifier.png" /></img></td>
    <td><a href="xref:Sylves.PlanarPrismModifier">PlanarPrismModifier</a><br/>Takes a 2d planar grid, and extends it into multiple layers along the third the dimension. </td>
</tr>
</table>

## Modifier grids

Modifier grids let you customize an existing grid by systematically changing it in some way.

<table>
<tr>
    <td><a href="../../images/grids/biject_square.svg"><img class="grid-thumb" src="../../images/grids/biject_square.svg" /></img></td>
    <td><a href="xref:Sylves.BijectModifier">BijectModifier</a><br/>Remaps the cells of the grid by changing their co-ordinates, without touching the position, shape or topology.</td>
</tr>
<tr>
    <td><a href="../../images/grids/mask_square.svg"><img class="grid-thumb" src="../../images/grids/mask_square.svg" /></img></td>
    <td><a href="xref:Sylves.MaskModifier">MaskModifier</a><br/>Filters the cells in the the grid to a customizable subset.</td>
</tr>
<tr>
    <td><a href="../../images/grids/transform_square.svg"><img class="grid-thumb" src="../../images/grids/transform_square.svg" /></img></td>
    <td><a href="xref:Sylves.TransformModifier">TransformModifier</a><br/>Changes the world space positioning of the grid by a linear transform, leaving everything else unchanged.</td>
</tr>
<tr>
    <td><a href="../../images/grids/planarprismmodifier.png"><img class="grid-thumb" src="../../images/grids/planarprismmodifier.png" /></img></td>
    <td><a href="xref:Sylves.PlanarPrismModifier">PlanarPrismModifier</a><br/>Takes a 2d planar grid, and extends it into multiple layers along the third the dimension.</td>
</tr>
<tr>
    <td><a href="../../images/grids/wrap_square_fake.svg"><img class="grid-thumb" src="../../images/grids/wrap_square_fake.svg" /></img></td>
    <td><a href="xref:Sylves.WrapModifier">WrapModifier</a><br/>Turns any bounded grid into a grid which connects back on itself when you leave the grounds. </td>
</tr>
</table>

## Extra grids

These grids don't classify neatly and usually serve as demos for various features.

<table>
<tr>
    <td><a href="../../images/grids/mobiussquare.png"><img class="grid-thumb" src="../../images/grids/mobiussquare.png" /></img></td>
    <td><a href="xref:Sylves.MobiusSquareGrid">MobiusSquareGrid</a><br/>A grid in the shape of a <a href="https://en.wikipedia.org/wiki/M%C3%B6bius_strip">Möbius strip</a>. This is mostly to demonstrate how Sylves handles <a href="https://en.wikipedia.org/wiki/Orientability">non-orientable surfaces</a>. </td>
</tr>
<tr>
    <td><a href="../../images/grids/cubius.png"><img class="grid-thumb" src="../../images/grids/cubius.png" /></img></td>
    <td><a href="xref:Sylves.CubiusGrid">CubiusGrid</a><br/>A <a href="https://en.wikipedia.org/wiki/M%C3%B6bius_strip">Möbius strip</a> with thickness, and only a quarter turn instead of a half turn. This is mostly to demonstrate how Sylves handles advanced cases of <a href="https://en.wikipedia.org/wiki/Holonomy">holonomy</a>. </td>
</tr>
<tr>
    <td><a href="../../images/grids/wrap_square_fake.svg"><img class="grid-thumb" src="../../images/grids/wrap_square_fake.svg" /></img></td>
    <td><a href="xref:Sylves.WrappingSquareGrid">WrappingSquareGrid</a><br/>Turns any bounded grid into a grid which connects back on itself when you leave the grounds. </td>
</tr>
</table>