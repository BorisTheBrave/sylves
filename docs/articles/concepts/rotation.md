# Rotation

For many games and other usages of grids, you never need to worry about rotating things. In chess, for example, the units move in fixed patterns. Or for a height map, you just need a single number per cell.

But in other usages, the facing direction of objects matters, or you wish to re-use the same tile image in a different orientation. For these, sooner or later, you'll need to worry about rotations.

Rotations are one of the most complicated parts of Sylves' API and you are recommended to avoid them until you are comfortable with cells, celltypes and grids.

## What is a rotation?

In Sylves, a rotation is a mapping of a cell onto itself. For example, for a square tile, there are 4 rotations, corresponding to rotating by 0, 90, 180 or 270 degrees.
Rotating by, say, 45 degrees is not a rotation, as that would transform a square to a diamond, not back to the original square.

There's also 4 reflections that map a square onto itself. Reflections are treated the same as rotations in Sylves, and usually we'll use rotation to refer to both of them.

Transformations that map something back onto itself are known symmetries in mathematics.

---

Rotations are represented by the enum `CellRotation`. This is an empty enumeration - the actual values are specific to the cell type in question.

So for `SquareCellType`, you can use `SquareRotation` which contains the 8 rotations/reflections of a square.

(recall, SquareCellType is used to describe cells shaped like squares, which are then used by several different grids).

Rotations only consider a single cell at a time. For rotating an entire grid, see [Grid Symmetry](grid_symmetry.md) or [TransformModifier](xref:Sylves.TransformModifier).

### 2d/3d rotations

2d rotations are the easiest to consider. If a polygon has $n$ sides, then there are $n$ rotations, each a multiple of $360 / n$ degrees, which are stored as positive integers in counterclockwise order.
There's also $n$ possible reflections, which are stored as negative numbers.

So a 2d rotation will have `-n <= (int)rotation < n`.

SquareCellType, HexCellType and NGonCellType [all work this way](https://en.wikipedia.org/wiki/Dihedral_group).

Sylves also supports CubeCellType, which supports all 48 rotations / reflections of a cube. 

## Basic usage

Most of the key methods of rotations are on ICellType, as rotations only consider a single cell.

* [`GetIdentity`](xref:Sylves.ICellType.GetIdentity) - gets the rotation that does nothing.
* [`GetRotations`](xref:Sylves.ICellType.GetRotations(System.Boolean)) - gets all rotations (and reflections) of a cell
* [`Invert`](xref:Sylves.ICellType.Invert(Sylves.CellRotation)) - finds the rotation that undoes the given rotation
* [`RotateCCW`](xref:Sylves.ICellType.RotateCCW)/[`RotateCW`](xref:Sylves.ICellType.RotateCW) - rotate left/right (2d cell types only)

The main thing you can do with a rotation, is rotate things by it.

* ['Rotate'](xref:Sylves.ICellType.Rotate(Sylves.CellDir,Sylves.CellRotation)) - rotate a `CellDir`.
* ['Multiply'](xref:Sylves.ICellType.Multiply(Sylves.CellRotation,Sylves.CellRotation)) - compose two rotations together into a single one
* ['GetMatrix'](xref:Sylves.ICellType.GetMatrix(Sylves.CellRotation)) - get a rotation as a matrix, so you can transform vectors.

If you use concrete classes like `SquareRotation`, they usually have the `*` operator overloaded as a shorthand for applying rotations, and many other convenience methods.


## TryMove and Rotation

When you call [`TryMove`](xref:Sylves.IGrid.TryMove(Sylves.Cell,Sylves.CellDir,Sylves.Cell@,Sylves.CellDir@,Sylves.Connection@)), in addition to returning the tile you move to, the grid returns `inverseDir` and `connection`.

Let's ignore connection for now, as it is irrelevant to the majority of grids. `inverseDir` returns the `CellDir` needed to move *back* to the original cell. Why is `inverseDir` so important?

Well, for the basic grids, like SquareGrid, HexGrid, CubeGrid, `inverseDir` is always the obvious choice. If you move left, then inverseDir will be right. Same for up/down, etc.

But there are more complex grids, where that isn't the case. Consider a grid which has 6 square cells, arranged as the faces of a 3d cube.

[TODO: Diagram]

It's not possible to arrange the cells so that inverseDir is always the obvious choice. You can see above, that if you move **up** from the cell A, you end up on the top of the cube, cell B. But you need to go **left** to get back.

Another way of saying the same thing is that if you are standing on the surface of this cube on cell A, facing towards the **upper** edge. If you walk straight, you'll cross over the edge, and now you'll find you are facing towards B's **right** edge.

Crossing the edge from A to B moves you from A's local coordinates to B's. The transition from one to the other is a *rotation*.

### Non-orientability


### Non-trivial connections