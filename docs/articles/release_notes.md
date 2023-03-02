# Unreleased

* Added [RelaxModifier](modifiers/relaxmodifier.md)
* Added [PlanarLazyGrid](grids/planarlazygrid.md)
* Added [Townscaper grid](xref:Sylves.TownscaperGrid)
* Added [Townscaper tutorial](tutorials/townscaper.md)
* Added [Concat](xref:Sylves.MeshDataOperations.Concat(System.Collections.Generic.IEnumerable{Sylves.MeshData},System.Collections.Generic.List{System.Int32[]}@)), [Relax](xref:Sylves.MeshDataOperations.Relax(Sylves.MeshData,System.Int32)), [RandomPairing](xref:Sylves.MeshDataOperations.RandomPairing(Sylves.MeshData,System.Func{System.Double})), [MaxRandomPairing](xref:Sylves.MeshDataOperations.MaxRandomPairing(Sylves.MeshData,System.Func{System.Double})) and [Weld](xref:Sylves.MeshDataOperations.Weld(Sylves.MeshData,System.Single)) mesh operations.
* Fixed bug with square and cube grid bounds.
* Fixed Ortho operator.
* Fixed floating point precision issues in MeshGrid and similar.
* Updated Deformation to use analytic rather than numerical derivatives.

# 0.1

* Initial public release