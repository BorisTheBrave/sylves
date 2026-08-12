using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Represents rotations / reflections of a rhombic dodecahedron.
    /// Delegates to <see cref="CubeRotation"/>, which shares the same octahedral group.
    /// </summary>
    public struct RhombicDodecahedronRotation
    {
        private CubeRotation cubeRotation;

        private RhombicDodecahedronRotation(CubeRotation cubeRotation)
        {
            this.cubeRotation = cubeRotation;
        }

        public static RhombicDodecahedronRotation Identity => CubeRotation.Identity;
        public static RhombicDodecahedronRotation ReflectX => CubeRotation.ReflectX;
        public static RhombicDodecahedronRotation ReflectY => CubeRotation.ReflectY;
        public static RhombicDodecahedronRotation ReflectZ => CubeRotation.ReflectZ;
        // NB: By Unity convensions:
        // Rotation around X = RotateYZ
        // Rotation around Y = RotateZX
        // Rotation around Z = RotateXY
        public static RhombicDodecahedronRotation RotateZX => CubeRotation.RotateZX;
        public static RhombicDodecahedronRotation RotateYX => CubeRotation.RotateYX;
        public static RhombicDodecahedronRotation RotateZY => CubeRotation.RotateZY;
        public static RhombicDodecahedronRotation RotateXZ => CubeRotation.RotateXZ;
        public static RhombicDodecahedronRotation RotateXY => CubeRotation.RotateXY;
        public static RhombicDodecahedronRotation RotateYZ => CubeRotation.RotateYZ;

        public static IEnumerable<RhombicDodecahedronRotation> GetRotations(bool includeReflections)
        {
            foreach (var r in CubeRotation.GetRotations(includeReflections))
            {
                yield return r;
            }
        }

        internal Matrix4x4 ToMatrix() => cubeRotation.ToMatrix();

        internal static RhombicDodecahedronRotation? FromMatrix(Matrix4x4 matrix)
        {
            var cubeRotation = CubeRotation.FromMatrix(matrix);
            if (cubeRotation == null)
            {
                return null;
            }
            return cubeRotation.Value;
        }

        public bool IsReflection => cubeRotation.IsReflection;

        public RhombicDodecahedronRotation Invert() => cubeRotation.Invert();

        public override bool Equals(object obj)
        {
            return obj is RhombicDodecahedronRotation rotation &&
                   cubeRotation == rotation.cubeRotation;
        }

        public override System.Int32 GetHashCode() => cubeRotation.GetHashCode();

        public static bool operator ==(RhombicDodecahedronRotation a, RhombicDodecahedronRotation b) => a.cubeRotation == b.cubeRotation;

        public static bool operator !=(RhombicDodecahedronRotation a, RhombicDodecahedronRotation b) => a.cubeRotation != b.cubeRotation;

        public static RhombicDodecahedronRotation operator *(RhombicDodecahedronRotation a, RhombicDodecahedronRotation b) => a.cubeRotation * b.cubeRotation;

        public static Vector3 operator *(RhombicDodecahedronRotation r, Vector3 v) => r.cubeRotation * v;

        public static Vector3Int operator *(RhombicDodecahedronRotation r, Vector3Int v) => r.cubeRotation * v;

        public static RhombicDodecahedronDir operator *(RhombicDodecahedronRotation r, RhombicDodecahedronDir dir)
        {
            return RhombicDodecahedronDirExtensions.FromForward(r.cubeRotation * dir.Forward());
        }

        public static RhombicDodecahedronCorner operator *(RhombicDodecahedronRotation r, RhombicDodecahedronCorner corner)
        {
            if((int)corner < 8)
            {
                return (RhombicDodecahedronCorner)(r.cubeRotation * (CubeCorner)corner);
            }else{
                var dir = (CubeDir)((int)corner - 8);
                dir = r.cubeRotation * dir;
                return (RhombicDodecahedronCorner)(8 + (int)dir);
            }
        }

        public override string ToString() => cubeRotation.ToString();

        public static implicit operator RhombicDodecahedronRotation(CubeRotation r) => new RhombicDodecahedronRotation(r);

        public static implicit operator CubeRotation(RhombicDodecahedronRotation r) => r.cubeRotation;

        public static implicit operator RhombicDodecahedronRotation(CellRotation r) => (CubeRotation)r;

        public static implicit operator CellRotation(RhombicDodecahedronRotation r) => r.cubeRotation;
    }
}
