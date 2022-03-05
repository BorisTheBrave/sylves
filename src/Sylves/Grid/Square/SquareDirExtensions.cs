using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

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
    }
}
