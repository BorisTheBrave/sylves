using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Values for CellDir when working with FlatSides triangles.
    /// Identical to PTHexDir
    /// </summary>
    public enum FSTriangleDir
    {
        Right,
        UpRight,
        UpLeft,
        Left,
        DownLeft,
        DownRight,
    }

    /// <summary>
    /// Values for CellDir when working with FlatTopped triangles
    /// Identical to FTHexDir
    /// </summary>
    public enum FTTriangleDir
    {
        UpRight,
        Up,
        UpLeft,
        DownLeft,
        Down,
        DownRight,
    }
}
