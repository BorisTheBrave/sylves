using System;
using System.Collections.Generic;
using UnityEngine;


namespace Sylves
{
    public static class HexDirExtensions
    {
        private const float Sqrt3 = 1.73205080756888f;

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3 Forward(this PTHexDir dir)
        {
            switch (dir)
            {
                case PTHexDir.Right: return new Vector3(1, 0, 0);
                case PTHexDir.UpRight: return new Vector3(0.5f, Sqrt3 / 2, 0);
                case PTHexDir.UpLeft: return new Vector3(-0.5f, Sqrt3 / 2, 0);
                case PTHexDir.Left: return new Vector3(-1, 0, 0);
                case PTHexDir.DownLeft: return new Vector3(-0.5f, -Sqrt3 / 2, 0);
                case PTHexDir.DownRight: return new Vector3(0.5f, -Sqrt3 / 2, 0);
            }
            throw new Exception($"{dir} is not a valid value for PTHexDir");
        }

        /// <returns>The normal vector for a given face.</returns>
        public static Vector3 Forward(this FTHexDir dir)
        {
            switch (dir)
            {
                case FTHexDir.UpRight: return new Vector3(Sqrt3 / 2, 0.5f, 0);
                case FTHexDir.Up: return new Vector3(0, 1, 0);
                case FTHexDir.UpLeft: return new Vector3(Sqrt3 / 2, 0.5f, 0);
                case FTHexDir.DownLeft: return new Vector3(-Sqrt3 / 2, -0.5f, 0);
                case FTHexDir.Down: return new Vector3(0, -1, 0);
                case FTHexDir.DownRight: return new Vector3(-Sqrt3 / 2, 0.5f, 0);
            }
            throw new Exception($"{dir} is not a valid value for FTHexDir");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static PTHexDir Inverted(this PTHexDir dir)
        {
            return (PTHexDir)((((int)dir) + 3) % 6);
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static FTHexDir Inverted(this FTHexDir dir)
        {
            return (FTHexDir)((((int)dir) + 3) % 6);
        }

        // TODO: Avoid allocation here
        /// <returns>The position of a corner in a unit hexcentered on the origin.</returns>
        public static Vector3 GetPosition(this FTHexCorner corner)
        {
            return MeshPrimitives.FtHexPolygon[(int)corner];
        }

        // TODO: Avoid allocation here
        /// <returns>The position of a corner in a unit hexcentered on the origin.</returns>
        public static Vector3 GetPosition(this PTHexCorner corner)
        {
            return MeshPrimitives.PtHexPolygon[(int)corner];
        }
    }
}
