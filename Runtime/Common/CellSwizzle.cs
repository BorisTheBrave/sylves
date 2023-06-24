using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Identifies a permutation of the 3 axes
    /// Identical to Unity's CellSwizzle.
    /// </summary>
    public enum CellSwizzle
    {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX,
    }
}
