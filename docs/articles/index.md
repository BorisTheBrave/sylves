# Introduction

Sylves is a C# library for handling maths relating to grids, for games and procedural generation.

It can be downloaded from <a href="https://github.com/BorisTheBrave/sylves/releases" role="button">here</a>.

**Q**: Isn't working with grids easy? It's basically a 2d array.

**A**: Sure, if you only care about *square* grids. Sylves defines a "grid" much more broadly than that. Grids can be 2d or 3d, built out of varying or irregular shapes, and can be infinite.

Furthermore:

* Sylves supports a [wide range of different grids](grids/index.md) and you can [create](creating.md) even more.
* All grids in Sylves shares a common interface, [IGrid](concepts/index.md), so algorithms can be written once and work on any grid. 
* Sylves handles many of the fiddlier grid operations, such as [raycasts](concepts/query.md) and [pathfinding](concepts/pathfinding.md).
* Sylves comes with a sophisticated notion of direction and [rotation](concepts/rotation.md)
* Sylves supports [mesh deformation](concepts/shape.md#deformation) to squeeze meshes to fit irregular polygons.

Read on to learn about the **[basic concepts of Sylves](concepts/index.md)**, work through some [**tutorials**](tutorials/index.md), or see an **[online demo](https://boristhebrave.itch.io/sylves-demos)**.