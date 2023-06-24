using System;
using UnityEngine;

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
            throw new Exception($"Unrecognized dir {dir}");
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
            throw new Exception($"Unrecognized dir {dir}");
        }

        /// <returns>Cross product of Up() and Forward().</returns>
        /// I.e. what would be on your right if you where facing in this cell dir.
        public static Vector3Int Right(this CubeDir dir)
        {
            switch (dir)
            {
                case CubeDir.Left: return new Vector3Int(0, 0, 1);
                case CubeDir.Right: return new Vector3Int(0, 0, -1);
                case CubeDir.Up: return Vector3Int.left;
                case CubeDir.Down: return Vector3Int.right;
                case CubeDir.Forward: return Vector3Int.right;
                case CubeDir.Back: return Vector3Int.left;
            }
            throw new Exception($"Unrecognized dir {dir}");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static CubeDir Inverted(this CubeDir dir) => (CubeDir)(1 ^ (int)dir);



        /// <returns>The position of a corner in a unit cube centered on the origin.</returns>
        public static Vector3 GetPosition(this CubeCorner corner)
        {
            switch (corner)
            {
                case CubeCorner.BackDownRight: return new Vector3(0.5f, -0.5f, -0.5f);
                case CubeCorner.BackUpRight: return new Vector3(0.5f, 0.5f, -0.5f);
                case CubeCorner.BackUpLeft: return new Vector3(-0.5f, 0.5f, -0.5f);
                case CubeCorner.BackDownLeft: return new Vector3(-0.5f, -0.5f, -0.5f);
                case CubeCorner.ForwardDownRight: return new Vector3(0.5f, -0.5f, 0.5f);
                case CubeCorner.ForwardUpRight: return new Vector3(0.5f, 0.5f, 0.5f);
                case CubeCorner.ForwardUpLeft: return new Vector3(-0.5f, 0.5f, 0.5f);
                case CubeCorner.ForwardDownLeft: return new Vector3(-0.5f, -0.5f, 0.5f);
            }
            throw new Exception($"{corner} is not a valid value for CubeCorner");
        }
    }
}
