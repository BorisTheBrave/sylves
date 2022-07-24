# Meshes

A [polygon mesh](https://en.wikipedia.org/wiki/Polygon_mesh) is a data tructure commonly used in computer graphics. Roughly speaking they are a collection of vertices, edges and vertices.

Sylves mostly deals with meshs using the [MeshData](xref:Sylves.MeshData) class which closely mirrors how Unity stores them internally, but with extra support for faces with more than 4 vertices.

Meshes are closely related to grids and there are numerous methods in Sylves for dealing with them.

## Grid Conversion

<xref:Sylves.MeshGrid>
<xref:Sylves.MeshPrismGrid>
<xref:Sylves.PeriodicPlanarMeshGrid>

<xref:Sylves.MeshUtils.ToMesh(Sylves.IGrid)>

## Deformation

The [grid deformation functionality](space.md#deformation) can be used from any mesh with [`Sylves.MeshUtils.GetDeformation`](xref:Sylves.MeshUtils.GetDeformation(Sylves.MeshData,System.Single,System.Single,System.Boolean,System.Int32,System.Int32,System.Int32,System.Boolean)).



