using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public static class RhombicDodecahedronDirExtensions
    {
        /// <returns>The neighbor offset for a given face.</returns>
        public static Vector3Int Forward(this RhombicDodecahedronDir dir)
        {
            switch (dir)
            {
                case RhombicDodecahedronDir.RightUp: return new Vector3Int(1, 1, 0);
                case RhombicDodecahedronDir.LeftDown: return new Vector3Int(-1, -1, 0);
                case RhombicDodecahedronDir.RightDown: return new Vector3Int(1, -1, 0);
                case RhombicDodecahedronDir.LeftUp: return new Vector3Int(-1, 1, 0);
                case RhombicDodecahedronDir.RightForward: return new Vector3Int(1, 0, 1);
                case RhombicDodecahedronDir.LeftBack: return new Vector3Int(-1, 0, -1);
                case RhombicDodecahedronDir.RightBack: return new Vector3Int(1, 0, -1);
                case RhombicDodecahedronDir.LeftForward: return new Vector3Int(-1, 0, 1);
                case RhombicDodecahedronDir.UpForward: return new Vector3Int(0, 1, 1);
                case RhombicDodecahedronDir.DownBack: return new Vector3Int(0, -1, -1);
                case RhombicDodecahedronDir.UpBack: return new Vector3Int(0, 1, -1);
                case RhombicDodecahedronDir.DownForward: return new Vector3Int(0, -1, 1);
            }
            throw new Exception($"Unrecognized dir {dir}");
        }

        /// <returns>Returns the face dir with the opposite normal vector.</returns>
        public static RhombicDodecahedronDir Inverted(this RhombicDodecahedronDir dir) => (RhombicDodecahedronDir)(1 ^ (int)dir);

        public static RhombicDodecahedronDir FromForward(Vector3Int v)
        {
            if (v.x == 1 && v.y == 1 && v.z == 0)
                return RhombicDodecahedronDir.RightUp;
            if (v.x == -1 && v.y == -1 && v.z == 0)
                return RhombicDodecahedronDir.LeftDown;
            if (v.x == 1 && v.y == -1 && v.z == 0)
                return RhombicDodecahedronDir.RightDown;
            if (v.x == -1 && v.y == 1 && v.z == 0)
                return RhombicDodecahedronDir.LeftUp;

            if (v.x == 1 && v.y == 0 && v.z == 1)
                return RhombicDodecahedronDir.RightForward;
            if (v.x == -1 && v.y == 0 && v.z == -1)
                return RhombicDodecahedronDir.LeftBack;
            if (v.x == 1 && v.y == 0 && v.z == -1)
                return RhombicDodecahedronDir.RightBack;
            if (v.x == -1 && v.y == 0 && v.z == 1)
                return RhombicDodecahedronDir.LeftForward;

            if (v.x == 0 && v.y == 1 && v.z == 1)
                return RhombicDodecahedronDir.UpForward;
            if (v.x == 0 && v.y == -1 && v.z == -1)
                return RhombicDodecahedronDir.DownBack;
            if (v.x == 0 && v.y == 1 && v.z == -1)
                return RhombicDodecahedronDir.UpBack;
            if (v.x == 0 && v.y == -1 && v.z == 1)
                return RhombicDodecahedronDir.DownForward;

            throw new InvalidOperationException($"Cannot convert {v} to RhombicDodecahedronDir");
        }

        /// <returns>
        /// The position of a corner in a unit rhombic dodecahedron centered on the origin.
        /// The 8 cube vertices sit at (±0.5, ±0.5, ±0.5).
        /// The 6 octahedron vertices sit at the adjacent odd-cube centers, (±1, 0, 0) and permutations.
        /// </returns>
        public static Vector3 GetPosition(this RhombicDodecahedronCorner corner)
        {
            switch (corner)
            {
                case RhombicDodecahedronCorner.BackDownLeft: return new Vector3(-0.5f, -0.5f, -0.5f);
                case RhombicDodecahedronCorner.BackDownRight: return new Vector3(+0.5f, -0.5f, -0.5f);
                case RhombicDodecahedronCorner.BackUpLeft: return new Vector3(-0.5f, +0.5f, -0.5f);
                case RhombicDodecahedronCorner.BackUpRight: return new Vector3(+0.5f, +0.5f, -0.5f);
                case RhombicDodecahedronCorner.ForwardDownLeft: return new Vector3(-0.5f, -0.5f, +0.5f);
                case RhombicDodecahedronCorner.ForwardDownRight: return new Vector3(+0.5f, -0.5f, +0.5f);
                case RhombicDodecahedronCorner.ForwardUpLeft: return new Vector3(-0.5f, +0.5f, +0.5f);
                case RhombicDodecahedronCorner.ForwardUpRight: return new Vector3(+0.5f, +0.5f, +0.5f);
                case RhombicDodecahedronCorner.Right: return new Vector3(+1.0f, 0.0f, 0.0f);
                case RhombicDodecahedronCorner.Left: return new Vector3(-1.0f, 0.0f, 0.0f);
                case RhombicDodecahedronCorner.Up: return new Vector3(0.0f, +1.0f, 0.0f);
                case RhombicDodecahedronCorner.Down: return new Vector3(0.0f, -1.0f, 0.0f);
                case RhombicDodecahedronCorner.Forward: return new Vector3(0.0f, 0.0f, +1.0f);
                case RhombicDodecahedronCorner.Back: return new Vector3(0.0f, 0.0f, -1.0f);
            }
            throw new Exception($"{corner} is not a valid value for RhombicDodecahedronCorner");
        }
    }
}
