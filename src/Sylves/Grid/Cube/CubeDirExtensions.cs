using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public static class CubeDirExtensions
    {
        /// <returns>Returns (0, 1, 0) vector for most faces, and returns (0, 0, 1) for the top/bottom faces.</returns>
        public static Vector3Int Up(this CubeDir dir)
        {
            switch (dir)
            {
                case CubeDir.Left:
                case CubeDir.Right:
                case CubeDir.Forward:
                case CubeDir.Back:
                    return Vector3Int.up;
                case CubeDir.Up:
                case CubeDir.Down:
                    return new Vector3Int(0, 0, 1);
            }
            throw new Exception();
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3Int Forward(this CubeDir dir)
        {
            switch (dir)
            {
                case CubeDir.Left: return Vector3Int.left;
                case CubeDir.Right: return Vector3Int.right;
                case CubeDir.Up: return Vector3Int.up;
                case CubeDir.Down: return Vector3Int.down;
                case CubeDir.Forward: return new Vector3Int(0, 0, 1);
                case CubeDir.Back: return new Vector3Int(0, 0, -1);
            }
            throw new Exception();
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static CubeDir Inverted(this CubeDir dir) => (CubeDir)(1 ^ (int)dir);
    }
}
