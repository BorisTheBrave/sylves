using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Values for CellCorner when working with PointyTopped hexes
    /// </summary>
    public enum PTHexCorner
    {
        DownRight,
        UpRight,
        Up,
        UpLeft,
        DownLeft,
        Down,
    }

    /// <summary>
    /// Values for CellCorner when working with FlatTopped hexes
    /// </summary>
    public enum FTHexCorner
    {
        Right,
        UpRight,
        UpLeft,
        Left,
        DownLeft,
        DownRight,
    }
}
