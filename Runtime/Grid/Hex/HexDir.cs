using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Values for CellDir when working with PointyTopped hexes
    /// </summary>
    public enum PTHexDir
    {
        Right,
        UpRight,
        UpLeft,
        Left,
        DownLeft,
        DownRight,
    }

    /// <summary>
    /// Values for CellDir when working with FlatTopped hexes
    /// </summary>
    public enum FTHexDir
    {
        UpRight,
        Up,
        UpLeft,
        DownLeft,
        Down,
        DownRight,
    }
}
