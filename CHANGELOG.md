# 1.0.0
* **Breaking**: Swapped CubeDir.Left and CubeDir.Right
* Added [OffGrid](grids/offgrid.md)
* Added [ChairGrid](grids/substitutiontilinggrid.md)
* Added [VoronoiGrid](grids/voronoigrid.md)
* Added [JitteredSquareGrid](grids/jitteredsquaregrid.md)
* Added [Voronator](xref:Sylves.Voronator) utiliyt class
* Added [NestedModifier](xref:Sylves.NestedModifier) (for internal use)
* Added [AabbGrid](xref:Sylves.AabbGrid) (for internal use)
* 2d grids now support GetMeshData.
* Fixes for RelaxModifier. It now uses the same coordinates as underlying.
* Fix IsPointInTriangle.
* Fix MeshPrism.IsPointInTri to be more robust.
* Fix issue in substitution tiling.
* Fix various grid operations to respect grid bounds
* Fix transform of dual of transformed grid


# 0.4.0
* Added [Substitution tiling grid](grids/substitutiontilinggrid.md)
* Added [experimental Godot support](tutorials/getting_started_godot.md)
* Added [UPM Published package](tutorials/getting_started_unity.md#install-from-upm)
* Add Format methods to ICellType
* Fix hang in PeriodicPlanarMeshGrid
* Fix planar prism modifier layer scaling.
* Fix vertex fanning.
* Fix TransformModifier.FindCell.
* Fixed BijectModifier.TryMoveByOffset
* Fixed raycast on some bounded grids
* Fixed orientation of Connections
* Improve precision of some floating point calculations, particularly for Townscaper grid
* Fix FromMatrix to ignore (uniform) scaling
* Fix issues with 360 degree rotation

# 0.3.0
* Changed Quad/TriangleInterpolation to use same conventions as rest of Sylves
* Introduce [CellCorner](xref:Sylves.CellCorner) and related operations.
* Added [dual grids](concepts/dual.md).
* Added [ChisledPathfinding](xref:Sylves.ChisledPathfinding).
* Added [FaceRelax](xref:Sylves.MeshDataOperations.FaceRelax(Sylves.MeshData,System.Int32)) and [FaceFilter](xref:Sylves.MeshDataOperations.FaceFilter(Sylves.MeshData,System.Func{Sylves.MeshUtils.Face,System.Int32,System.Boolean})) operations
* Improved mesh smoothing option for mesh deformation.
* Fixed SquareGrid.FindCell for non unit sized grids
* Fix precision issues in triangle/hex raycasts.
* Fix square/cube raycasts for non-unit cell sizes.
* Fix Vector.Normalized.
* Fix Parallel transport.
* Performance / accuracy improvements for RelaxModifier / Townscaper Grid

# 0.2

* Added [RelaxModifier](modifiers/relaxmodifier.md)
* Added [PlanarLazyMeshGrid](grids/planarlazymeshgrid.md)
* Added [Townscaper grid](xref:Sylves.TownscaperGrid)
* Added [Townscaper tutorial](tutorials/townscaper.md)
* Added [Concat](xref:Sylves.MeshDataOperations.Concat(System.Collections.Generic.IEnumerable{Sylves.MeshData},System.Collections.Generic.List{System.Int32[]}@)), [Relax](xref:Sylves.MeshDataOperations.Relax(Sylves.MeshData,System.Int32)), [RandomPairing](xref:Sylves.MeshDataOperations.RandomPairing(Sylves.MeshData,System.Func{System.Double})), [MaxRandomPairing](xref:Sylves.MeshDataOperations.MaxRandomPairing(Sylves.MeshData,System.Func{System.Double})) and [Weld](xref:Sylves.MeshDataOperations.Weld(Sylves.MeshData,System.Single)) mesh operations.
* Fixed bug with square and cube grid bounds.
* Fixed Ortho operator.
* Fixed floating point precision issues in MeshGrid and similar.
* Updated Deformation to use analytic rather than numerical derivatives.

# 0.1

* Initial public release