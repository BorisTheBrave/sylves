using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Values for CellCorner when working with FlatTopped triangles.
    /// Identical to PTHexCorner
    /// </summary>
    public enum FTTriangleCorner
    {
        DownRight,
        UpRight,
        Up,
        UpLeft,
        DownLeft,
        Down,
    }

    /// <summary>
    /// Values for CellCorner when working with FlatSides triangles
    /// Identical to FTHexCorner
    /// </summary>
    public enum FSTriangleCorner
    {
        Right,
        UpRight,
        UpLeft,
        Left,
        DownLeft,
        DownRight,
    }
}
