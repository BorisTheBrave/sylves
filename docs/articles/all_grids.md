# Grid Bestiary

This page provides a full list of grid classes available in Syves.

## Regular Grids

Regular grids are those with a periodic repeating pattern. These are the most common and practical grids available.

### Square grid

<xref:Sylves.SquareGrid>

## Cube grid

<xref:Sylves.CubeGrid>

### Hex grid

<xref:Sylves.HexGrid>

### Triangle gird

<xref:Sylves.TriangleGrid>

## Mesh Grids

Mesh grids accept a [mesh](xref:Sylves.MeshData) as the input, and base cells of the grid off faces of the mesh.

<xref:Sylves.MeshGrid>
<xref:Sylves.MeshPrismGrid>

## Prism Grids

"Prism" are when you take a 2d polygon, and extrude it into a 3d shape. 
This can convert 2d grids into 3d ones, usually with the z-cordinate being the "layer", i.e. offset
from the original grid.

<xref:Sylves.MeshPrismGrid>
<xref:Sylves.HexPrismGrid>
<xref:Sylves.TrianglePrismGrid>
<xref:Sylves.PlanarPrismModifier>

## Modifier grids

Modifier grids let you customize an existing grid by systematically changing it in some way.

### Biject modifier

<xref:Sylves.BijectModifier>

### Mask modifier

<xref:Sylves.MaskModifier>

### Transform modifier

<xref:Sylves.TransformModifier>

### Planar prism modifier

<xref:Sylves.PlanarPrismModifier>
### Wrapping modifier

<xref:Sylves.WrapModifier>

## Extra grids

These grids don't classify neatly and usually serve as demos for various features.

<xref:Sylves.MobiusSquareGrid>
<xref:Sylves.CubiusGrid>
<xref:Sylves.WrappingSquareGrid>