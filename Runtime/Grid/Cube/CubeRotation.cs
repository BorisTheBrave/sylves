using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public struct CubeRotation
    {
        short value;

        private CubeRotation(short value)
        {
            this.value = value;
        }

        public static CubeRotation Identity => new CubeRotation(0x012);
        public static CubeRotation ReflectX => new CubeRotation(0x812);
        public static CubeRotation ReflectY => new CubeRotation(0x092);
        public static CubeRotation ReflectZ => new CubeRotation(0x01A);
        // NB: By Unity convensions:
        // Rotation around X = RotateYZ
        // Rotation around Y = RotateZX
        // Rotation around Z = RotateXY
        public static CubeRotation RotateZX => new CubeRotation(0xA10);
        public static CubeRotation RotateYX => new CubeRotation(0x902);
        public static CubeRotation RotateZY => new CubeRotation(0x0A1);
        public static CubeRotation RotateXZ => new CubeRotation(0x218);
        public static CubeRotation RotateXY => new CubeRotation(0x182);
        public static CubeRotation RotateYZ => new CubeRotation(0x029);

        // Ordered by all rotations, then all refelctions

        public static IEnumerable<CubeRotation> GetRotations(bool includeReflections)
        {
            var evenPermutations = new short[]
            {
                    0x012,
                    0x120,
                    0x201,
            };
            var oddPermutations = new short[]
            {
                    0x021,
                    0x102,
                    0x210,
            };
            var evenReflections = new short[]
            {
                    0x000,
                    0x088,
                    0x808,
                    0x880,
            };
            var oddReflections = new short[]
            {
                    0x008,
                    0x080,
                    0x800,
                    0x888,
            };

            foreach (var r in evenReflections)
            {
                foreach (var p in evenPermutations)
                {
                    yield return new CubeRotation((short)(p + r));
                }
            }
            foreach (var r in oddReflections)
            {
                foreach (var p in oddPermutations)
                {
                    yield return new CubeRotation((short)(p + r));
                }
            }
            if (includeReflections)
            {

                foreach (var r in evenReflections)
                {
                    foreach (var p in oddPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }
                foreach (var r in oddReflections)
                {
                    foreach (var p in evenPermutations)
                    {
                        yield return new CubeRotation((short)(p + r));
                    }
                }
            }
        }

        internal Matrix4x4 ToMatrix()
        {
            Vector4 GetCol(int i)
            {
                var sign = (i & 8) == 0 ? 1 : -1;
                var row = i & 3;
                return new Vector4(
                    sign * (row == 0 ? 1 : 0),
                    sign * (row == 1 ? 1 : 0),
                    sign * (row == 2 ? 1 : 0),
                    0
                    );
            }
            return new Matrix4x4(
                GetCol(value >> 8),
                GetCol(value >> 4),
                GetCol(value >> 0),
                new Vector4(0, 0, 0, 1)
            );
        }

        // Converts form matrix when the matrix is guarnateed to be in a good format.
        private static CubeRotation FromMatrixSimple(Matrix4x4 matrix)
        {
            int GetCol(Vector4 c)
            {
                if (c.x != 0)
                {
                    return c.x > 0 ? 0 : 8;
                }
                else if (c.y != 0)
                {
                    return c.y > 0 ? 1 : 9;
                }
                else
                {
                    return c.z > 0 ? 2 : 10;
                }
            }
            return new CubeRotation((short)(
                (GetCol(matrix.GetColumn(0)) << 8) |
                (GetCol(matrix.GetColumn(1)) << 4) |
                (GetCol(matrix.GetColumn(2)) << 0)
                ));
        }

        internal static CubeRotation? FromMatrix(Matrix4x4 matrix)
        {
            Vector4 Rotate(Vector3Int v)
            {
                var v1 = matrix.MultiplyVector(v).normalized;
                var v2 = new Vector4(Mathf.RoundToInt(v1.x), Mathf.RoundToInt(v1.y), Mathf.RoundToInt(v1.z), 0);

                return v2;
            }

            // True if v is a unit vector along an axis
            bool Ok(Vector4 v)
            {
                return Math.Abs(v.x) + Math.Abs(v.y) + Math.Abs(v.z) == 1;
            }

            var rotatedRight = Rotate(Vector3Int.right);
            var rotatedUp = Rotate(Vector3Int.up);
            var rotatedForward = Rotate(new Vector3Int(0, 0, 1));

            // All three are unity vectors
            if (Ok(rotatedRight) && Ok(rotatedUp) && Ok(rotatedForward))
            {
                var sum = rotatedRight + rotatedForward + rotatedUp;
                // All three are orthogonal
                if (Math.Abs(sum.x) == 1 && Math.Abs(sum.y) == 1 && Math.Abs(sum.z) == 1)
                {
                    return FromMatrixSimple(new Matrix4x4(
                        rotatedRight,
                        rotatedUp,
                        rotatedForward,
                        new Vector4(0, 0, 0, 1)
                    ));
                }
            }
            return null;
        }

        public bool IsReflection
        {
            get
            {
                var isOddPermutation = ((value & 3) + ((value & (3 << 4)) >> 3)) % 3 != 1;
                return isOddPermutation ^ ((value & (1 << 3)) != 0) ^ ((value & (1 << 7)) != 0) ^ ((value & (1 << 11)) != 0);
            }
        }

        public CubeRotation Invert()
        {
            // TODO: Do fancy bitwise formula
            return FromMatrixSimple(ToMatrix().transpose);
        }

        public override bool Equals(object obj)
        {
            return obj is CubeRotation rotation &&
                   value == rotation.value;
        }

        public override int GetHashCode()
        {
            return -1584136870 + value.GetHashCode();
        }

        public static bool operator ==(CubeRotation a, CubeRotation b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(CubeRotation a, CubeRotation b)
        {
            return a.value != b.value;
        }

        public static CubeRotation operator *(CubeRotation a, CubeRotation b)
        {
            // bit fiddling version of FromMatrix(a.ToMatrix() * b.ToMatrix())

            // Which column in a to read, for a given axis
            var xOffset = (2 - ((b.value >> 8) & 3)) << 2;
            var yOffset = (2 - ((b.value >> 4) & 3)) << 2;
            var zOffset = (2 - ((b.value >> 0) & 3)) << 2;

            var col1 = ((a.value >> xOffset) & 15) ^ ((b.value >> 8) & (1 << 3));
            var col2 = ((a.value >> yOffset) & 15) ^ ((b.value >> 4) & (1 << 3));
            var col3 = ((a.value >> zOffset) & 15) ^ ((b.value >> 0) & (1 << 3));

            return new CubeRotation((short)((col1 << 8) + (col2 << 4) + (col3 << 0)));
        }

        public static Vector3 operator *(CubeRotation r, Vector3 v)
        {
            // bit fiddling version of r.ToMatrix().Multiply(v);

            var o = new Vector3();
            o[(r.value >> 8) & 3] = v.x * ((r.value & (1 << 11)) != 0 ? -1 : 1);
            o[(r.value >> 4) & 3] = v.y * ((r.value & (1 << 7)) != 0 ? -1 : 1);
            o[(r.value >> 0) & 3] = v.z * ((r.value & (1 << 3)) != 0 ? -1 : 1);
            return o;
        }

        public static Vector3Int operator *(CubeRotation r, Vector3Int v)
        {
            // bit fiddling version of r.ToMatrix().Multiply(v);

            var o = new Vector3Int();
            o[(r.value >> 8) & 3] = v.x * ((r.value & (1 << 11)) != 0 ? -1 : 1);
            o[(r.value >> 4) & 3] = v.y * ((r.value & (1 << 7)) != 0 ? -1 : 1);
            o[(r.value >> 0) & 3] = v.z * ((r.value & (1 << 3)) != 0 ? -1 : 1);
            return o;
        }

        public static CubeDir operator *(CubeRotation r, CubeDir dir)
        {
            return ToDir(r * dir.Forward());
        }

        public static CubeCorner operator *(CubeRotation r, CubeCorner corner)
        {
            var c = (int)corner;
            var x = (c >> 0) & 1;
            var y = (c >> 1) & 1;
            var z = (c >> 2) & 1;
            // Invert
            x = x ^ ((r.value >> 11) & 1);
            y = y ^ ((r.value >> 7) & 1);
            z = z ^ ((r.value >> 3) & 1);
            // Permute
            var o = (x << ((r.value >> 8) & 3)) |
                (y << ((r.value >> 4) & 3)) |
                (z << ((r.value >> 0) & 3));
            return (CubeCorner)o;
        }

        public static CubeBound operator *(CubeRotation rotation, CubeBound bound)
        {
            var a = rotation * bound.min;
            var b = rotation * (bound.max - Vector3Int.one);
            return new CubeBound(Vector3Int.Min(a, b), Vector3Int.Max(a, b) + Vector3Int.one);
        }

        private static CubeDir ToDir(Vector3Int v)
        {
            if (v.x == 1)
                return CubeDir.Right;
            if (v.x == -1)
                return CubeDir.Left;

            if (v.y == 1)
                return CubeDir.Up;
            if (v.y == -1)
                return CubeDir.Down;

            if (v.z == 1)
                return CubeDir.Forward;
            if (v.z == -1)
                return CubeDir.Back;

            throw new InvalidOperationException($"Cannot convert {v} to CubeDir");
        }

        public override string ToString()
        {
            switch(value)
            {
                case 0x12: return "Identity";
                case 0x812: return "ReflectX";
                case 0x092: return "ReflectY";
                case 0x01A: return "ReflectZ";
                case 0xA10: return "RotateZX";
                case 0x902: return "RotateYX";
                case 0x0A1: return "RotateZY";
                case 0x218: return "RotateXZ";
                case 0x182: return "RotateXY";
                case 0x029: return "RotateYZ";
                default: return value.ToString();
            }
        }

        public static implicit operator CubeRotation(CellRotation r) => new CubeRotation((short)r);

        public static implicit operator CellRotation(CubeRotation r) => (CellRotation)r.value;
    }
}
