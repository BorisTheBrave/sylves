using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public static class HexPrismCellDirExtensions
    {
        /// <returns>Returns (0, 0, 1) vector for most faces, and returns (0, 1, 0) for the top/bottom faces.</returns>
        public static Vector3Int Up(this PTHexPrismDir dir)
        {
            return dir.IsAxial() ? new Vector3Int(0, 1, 0) : new Vector3Int(0, 0, 1);
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3 Forward(this PTHexPrismDir dir)
        {
            switch (dir)
            {
                case PTHexPrismDir.Forward: return new Vector3Int(0, 0, 1);
                case PTHexPrismDir.Back: return new Vector3Int(0, 0, -1);
                default: return ((PTHexDir)dir).Forward();
            }
            throw new InvalidOperationException($"Cannot convert {dir} from PTHexPrismDir to Vector3");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static PTHexPrismDir Inverted(this PTHexPrismDir dir) => dir.IsAxial() ? (PTHexPrismDir)(1 ^ (int)dir) : (PTHexPrismDir)((3 + (int)dir) % 6);

        public static bool IsAxial(this PTHexPrismDir dir) => (int)dir >= 6;
    }
}
