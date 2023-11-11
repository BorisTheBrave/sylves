# Relax Modifier

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Class</td><td><a href="xref:Sylves.RelaxModifier">RelaxModifier</a></td></tr>
<tr><td>CellType</td><td>Unchanged</td></tr>
<tr><td>CellDir</td><td>Unchanged</td></tr>
<tr><td>CellRotation</td><td>Unchanged</td></tr>
<tr><td>Bound</td><td>SquareBound</td></tr>
<tr><td>Properties</td><td>Unchanged</td></tr>
<tr><td>Requirements</td><td>Base grid must be planar.</td></tr>
</table>

This modifier performs [mesh relaxation](https://en.wikipedia.org/wiki/Laplacian_smoothing) on the input grid. This tends to make things rounder and more evenly sized.

RelaxModifier is designed to work with infinite grids, if you have a finite grid, you'll find it much more efficient to use [MeshDataOperations.Relax](xref:Sylves.MeshDataOperations.Relax(Sylves.MeshData,System.Int32)):

```csharp
var relaxedMesh = new MeshGrid(originalGrid.ToMeshData().Relax());
```

Internally, RelaxModifier takes inspiration from [Townscaper](https://twitter.com/OskSta/status/1151154492102123521). The input grid is subdivided into hexagon chunks. A relaxation is done separately for every hexagon plus its neighbors to get a bunch of relaxed patches. Finally the modifier smoothly blends between relaxed patches.

There's several configuration settings documented on [the constructor](xref:Sylves.MeshDataOperations.Relax(Sylves.MeshData,System.Int32)).

<img width="200px" src="../../images/grids/unrelaxedtownscaper.svg" /></img> ➡ <img width="200px" src="../../images/grids/townscaper.svg" /></img>

## Cell co-ordinates

Uses the same co-ordinates as underlying.