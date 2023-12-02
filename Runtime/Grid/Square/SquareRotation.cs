using System;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Represents rotations / reflections of a square
    /// </summary>
    public struct SquareRotation
    {
        private static readonly SquareRotation[] all =
        {
            new SquareRotation(0),
            new SquareRotation(1),
            new SquareRotation(2),
            new SquareRotation(3),
            new SquareRotation(~0),
            new SquareRotation(~1),
            new SquareRotation(~2),
            new SquareRotation(~3),
        };

        short value;

        private SquareRotation(short value)
        {
            this.value = value;
        }

        public bool IsReflection => value < 0;

        public int Rotation => value < 0 ? ~value : value;

        public static SquareRotation Identity => new SquareRotation(0);

        public static SquareRotation ReflectX => new SquareRotation(~2);

        public static SquareRotation ReflectY => new SquareRotation(~0);

        public static SquareRotation RotateCCW => new SquareRotation(1);

        public static SquareRotation RotateCW => new SquareRotation(3);

        public static SquareRotation Rotate90(int i) => new SquareRotation((short)(((i % 4) + 4) % 4));

        public static SquareRotation[] All => all;

        public SquareRotation Invert()
        {
            if (IsReflection)
            {
                return this;
            }
            else
            {
                return new SquareRotation((short)((4 - value) % 4));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is SquareRotation rotation &&
                   value == rotation.value;
        }

        public override int GetHashCode()
        {
            return 45106587 + value.GetHashCode();
        }

        public static bool operator ==(SquareRotation a, SquareRotation b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(SquareRotation a, SquareRotation b)
        {
            return a.value != b.value;
        }

        public static SquareRotation operator *(SquareRotation a, SquareRotation b)
        {
            var isReflection = a.IsReflection ^ b.IsReflection;
            var rotation = a * (b * (SquareDir)0);
            return new SquareRotation(isReflection ? (short)~rotation : (short)rotation);
        }

        public static SquareDir operator *(SquareRotation rotation, SquareDir dir)
        {
            var side = (int)(dir);
            var newSide = (rotation.IsReflection ? rotation.Rotation - side + 4 : rotation.Rotation + side) % 4;
            return (SquareDir)(newSide);
        }

        public static SquareCorner operator *(SquareRotation rotation, SquareCorner dir)
        {
            var side = (int)(dir);
            var newSide = (rotation.IsReflection ? rotation.Rotation - side + 5 : rotation.Rotation + side) % 4;
            return (SquareCorner)(newSide);
        }

        public static SquareBound operator *(SquareRotation rotation, SquareBound bound)
        {
            var a = rotation * bound.min;
            var b = rotation * (bound.max - Vector2Int.one);
            return new SquareBound(Vector2Int.Min(a, b), Vector2Int.Max(a, b) + Vector2Int.one);
        }

        public Matrix4x4 ToMatrix()
        {
            switch (value)
            {
                case 0:
                    return Matrix4x4.identity;
                case 1:
                    return new Matrix4x4(new Vector4(0, 1, 0, 0), new Vector4(-1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case 2:
                    return new Matrix4x4(new Vector4(-1, 0, 0, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case 3:
                    return new Matrix4x4(new Vector4(0, -1, 0, 0), new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case ~0:
                    return new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case ~1:
                    return new Matrix4x4(new Vector4(0, 1, 0, 0), new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case ~2:
                    return new Matrix4x4(new Vector4(-1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                case ~3:
                    return new Matrix4x4(new Vector4(0, -1, 0, 0), new Vector4(-1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
                default:
                    throw new InvalidOperationException($"Cannot convert {value} from SquareRotation to Matrix4x4");
            }
        }

        public static SquareRotation? FromMatrix(Matrix4x4 matrix)
        {
            // Check that this matrix doesn't touch the z-axis
            var forward = matrix.MultiplyVector(Vector3.forward).normalized;
            if (Vector3.Distance(forward, Vector3.forward) > 1e-2f)
            {
                return null;
            }

            var right = matrix.MultiplyVector(Vector3.right);

            var up = matrix.MultiplyVector(Vector3.up);
            var isReflection = Vector3.Cross(right, up).z < 0;
            if(isReflection)
            {
                right.y = -right.y;
            }
            var angle = Mathf.Atan2(right.y, right.x);
            var angleInt = Mathf.RoundToInt(angle / (Mathf.PI / 2));

            return (isReflection ? ReflectY : Identity) * Rotate90(angleInt);
        }

        public override string ToString() => value.ToString();

        public static Vector2Int operator *(SquareRotation r, Vector2Int v)
        {
            switch (r.value)
            {
                case 0: break;
                case 1:
                    (v.x, v.y) = (-v.y, v.x);
                    break;
                case 2:
                    (v.x, v.y) = (-v.x, -v.y);
                    break;
                case 3:
                    (v.x, v.y) = (v.y, -v.x);
                    break;
                case ~0:
                    v.y = -v.y;
                    break;
                case ~1:
                    (v.x, v.y) = (v.y, v.x);
                    break;
                case ~2:
                    v.x = -v.x;
                    break;
                case ~3:
                    (v.x, v.y) = (-v.y, -v.x);
                    break;
            }
            return v;
        }
        public static Vector3Int operator *(SquareRotation r, Vector3Int v)
        {
            switch (r.value)
            {
                case 0: break;
                case 1:
                    (v.x, v.y) = (-v.y, v.x);
                    break;
                case 2:
                    (v.x, v.y) = (-v.x, -v.y);
                    break;
                case 3:
                    (v.x, v.y) = (v.y, -v.x);
                    break;
                case ~0:
                    v.y = -v.y;
                    break;
                case ~1:
                    (v.x, v.y) = (v.y, v.x);
                    break;
                case ~2:
                    v.x = -v.x;
                    break;
                case ~3:
                    (v.x, v.y) = (-v.y, -v.x);
                    break;
            }
            return v;
        }

        public static implicit operator SquareRotation(CellRotation r) => new SquareRotation((short)r);

        public static implicit operator CellRotation(SquareRotation r) => (CellRotation)r.value;
    }
}
