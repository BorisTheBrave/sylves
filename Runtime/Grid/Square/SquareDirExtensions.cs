using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public static class SquareDirExtensions
    {
        /// <returns>The normal vector for a given face.</returns>
        public static Vector3Int Forward(this SquareDir dir)
        {
            switch (dir)
            {
                case SquareDir.Left: return Vector3Int.left;
                case SquareDir.Right: return Vector3Int.right;
                case SquareDir.Up: return Vector3Int.up;
                case SquareDir.Down: return Vector3Int.down;
            }
            throw new Exception($"{dir} is not a valid value for SquareDir");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static SquareDir Inverted(this SquareDir dir)
        {
            return (SquareDir)((((int)dir) + 2) % 4);
        }

        /// <returns>The position of a corner in a unit square centered on the origin.</returns>
        public static Vector3 GetPosition(this SquareCorner corner)
        {
            switch (corner)
            {
                case SquareCorner.DownRight: return new Vector3(0.5f, -0.5f, 0);
                case SquareCorner.UpRight: return new Vector3(0.5f, 0.5f, 0);
                case SquareCorner.UpLeft: return new Vector3(-0.5f, 0.5f, 0);
                case SquareCorner.DownLeft: return new Vector3(-0.5f, -0.5f, 0);
            }
            throw new Exception($"{corner} is not a valid value for SquareCorner");
        }
    }
}
