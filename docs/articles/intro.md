# Introduction

Sylves is a C# library for handling maths relating to grids, for games and procedural generation.

**Q**: Isn't working with grids easy? It's basically a 2d array.

**A**: Sure, if you only care about *square* grids. Sylves defines a "grid" much more broadly than that. Grids can be 2d or 3d, built out of varying or irregular shapes, and can be infinite.

Furthermore:

* Sylves supports a [wide range of different grids](grids/index.md)
* All grids in Sylves shares a common interface, [IGrid](concepts/intro.md), so algorithms can be written once and work on any grid. 
* Sylves handles many of the fiddlier grid operations, such as raycasts and pathfinding.
* Sylves comes with a sophisticated notion of direction and [rotation](concepts/rotation.md)

Read on to learn about the **[basic concepts of Sylves](concepts/intro.md)**, or work through some [**tutorials**](tutorials/index.md).