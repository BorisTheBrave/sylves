# PlanarLazyGrid

<table>
<tr><th colspan="2">Quick facts</th></tr>
<tr><td>Grid</td><td><a href="xref:Sylves.PlanarLazyGrid">PlanarLazyGrid</a></td></tr>
<tr><td>CellType</td><td><a href="xref:Sylves.NGonCellType">NGonCellType</a>*</td></tr>
<tr><td>CellDir</td><td><a href="xref:Sylves.CellDir">CellDir</a>*</td></tr>
<tr><td>CellRotation</td><td><a href="xref:Sylves.CellRotation">CellRotation</a>*</td></tr>
<tr><td>Bound</td><td><a href="xref:Sylves.SquareBound">SquareBound</a></td></tr>
<tr><td>Properties</td><td>2d, Planar, Infinite</td></tr>
<tr><td colspan="2"><small>*NGonCellType represents any polygon. But for 4 and 6 sided faces, the values overlap with SquareCellType and HexCellType, and the corresponding SquareDir, SquareRotation, PTHexDir, HexRotation.</small></td></tr>
</table>

PlanarLazyGrid is a variant of [PeriodicPlanarMeshGrid](periodicplanarmeshgrid.md). While PeriodicPlanarMeshGrid takes a single mesh, and repeats it over the plane, instead PlanarLazyGrid uses a *different* mesh at each position, called a **chunk**, stitching them all together.

To do so, you must supply a function that builds each of the meshes for the chunk, and give some information about the placement of each mesh, which must be periodic. PlanarLazyGrid will invoke the function as necessary (i.e. *lazily*) and store the results, stitching together the meshes into a single grid.

## Constructor

The constructor for PlanarLazyGrid is somewhat complicated, you may wish to use the alternative constructors further below.

```csharp
public PlanarLazyGrid(Func<Vector2Int, MeshData> getMeshData, Vector2 strideX, Vector2 strideY, Vector2 aabbBottomLeft, Vector2 aabbSize, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
```

The arguments are 
* `getMeshData` - Gets `MeshData` for a given chunk.
* `strideX` / `strideY` - Function as in [PeriodicPlanarMeshGrid](periodicplanarmeshgrid.md), these describe the distance between meshes.
* `aabbBottomLeft`, `aabbSize` - These give constant bounds on the output of `getMeshData(new Vector2Int(0, 0))`, and similarly for other chunks, translated by strideX/strideY. The [aabb](https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection)'s of chunks *may* overlap.
* `MeshGridOptions` - as documented in [MeshGrid](meshgrid.md#meshgridoptions)
* `bound` - the [bounds](../concepts/bounds.md) to restrict this grid to. This restricts which chunks are generated, there's no bounds within-chunks.
* `cellTypes` - the value to output for [`IGrid.GetCellTypes()`](xref:Sylves.IGrid.GetCellTypes())
* `cachePolicy` - see [caching](../concepts/caching.md).

### Alternative constructor

To make it easier to work with, there are alternative constructors for common periodic spacing:

```csharp
public PlanarLazyGrid(Func<Cell, MeshData> getMeshData, HexGrid chunkGrid, MeshGridOptions meshGridOptions = null, SquareBound bound = null, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
```

Here, each mesh is expected to fill a single cell of `chunkGrid`. `getMeshData` takes a `Cell` argument naming the cell to be filled. Other arguments work the same.

## Cell co-ordinates

The x value indicates the face in the mesh, and the y and z values indicate the multiple of stride moved in the two directions.