using System;
using UnityEngine;


namespace Sylves
{
    public static class CellSwizzleExtensions
    {
        public static CubeRotation ToCubeRotation(this CellSwizzle cellSwizzle)
        {
            switch(cellSwizzle)
            {
                case CellSwizzle.XYZ: return (CellRotation)(0x012);
                case CellSwizzle.XZY: return (CellRotation)(0x021);
                case CellSwizzle.YXZ: return (CellRotation)(0x102);
                case CellSwizzle.YZX: return (CellRotation)(0x120);
                case CellSwizzle.ZXY: return (CellRotation)(0x201);
                case CellSwizzle.ZYX: return (CellRotation)(0x210);
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellSwizzle));
            }
        }

        public static Matrix4x4 ToMatrix(this CellSwizzle cellSwizzle) => cellSwizzle.ToCubeRotation().ToMatrix();
    }
}
