using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public static class TrianglePrismCellDirExtensions
    {
        /// <returns>Returns (0, 0, 1) vector for most faces, and returns (0, 1, 0) for the top/bottom faces.</returns>
        public static Vector3Int Up(this FSTrianglePrismDir dir)
        {
            return dir.IsAxial() ? new Vector3Int(0, 1, 0) : new Vector3Int(0, 0, 1);
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3 Forward(this FSTrianglePrismDir dir)
        {
            switch (dir)
            {
                case FSTrianglePrismDir.Forward: return new Vector3Int(0, 0, 1);
                case FSTrianglePrismDir.Back: return new Vector3Int(0, 0, -1);
                default: return ((FSTrianglePrismDir)dir).Forward();
            }
            throw new InvalidOperationException($"Cannot convert {dir} from FSTrianglePrismDir to Vector3"); 
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static FSTrianglePrismDir Inverted(this FSTrianglePrismDir dir) => dir.IsAxial() ? (FSTrianglePrismDir)(1 ^ (int)dir) : (FSTrianglePrismDir)((3 + (int)dir) % 6);

        public static bool IsAxial(this FSTrianglePrismDir dir) => (int)dir >= 6;
    }
}
