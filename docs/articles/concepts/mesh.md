# Meshes

A [polygon mesh](https://en.wikipedia.org/wiki/Polygon_mesh) is a data tructure commonly used in computer graphics. Roughly speaking they are a collection of vertices, edges and vertices.

Sylves mostly deals with meshs using the [MeshData](xref:Sylves.MeshData) class which closely mirrors how Unity stores them internally, but with [extra support](xref:Sylves.MeshTopology.NGon) for faces with more than 4 vertices.

Meshes are closely related to grids and there are numerous methods in Sylves for dealing with them.

## Grid Conversion

There are several classes for converting to/from meshes.

* [MeshGrid](../grids/meshgrid.md)
* [MeshPrismGrid](../grids/meshprismgrid.md)
* [PeriodicPlanarMeshGrid](../grids/periodicplanarmeshgrid.md)
* [MeshUtils.ToMesh](xref:Sylves.MeshUtils.ToMesh(Sylves.IGrid))

## Deformation

The [grid deformation functionality](shape.md#deformation) can be used from any mesh with [`Sylves.DeformationUtils.GetDeformation`](xref:Sylves.DeformationUtils.GetDeformation(Sylves.MeshData,System.Single,System.Single,System.Boolean,System.Int32,System.Int32,System.Int32,System.Boolean)).



