using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Represents rotations / reflections of a hex
    /// </summary>
    public struct HexRotation
    {
        private static readonly HexRotation[] all =
        {
            new HexRotation(0),
            new HexRotation(1),
            new HexRotation(2),
            new HexRotation(3),
            new HexRotation(4),
            new HexRotation(5),
            new HexRotation(~0),
            new HexRotation(~1),
            new HexRotation(~2),
            new HexRotation(~3),
            new HexRotation(~4),
            new HexRotation(~5),
        };

        short value;

        private HexRotation(short value)
        {
            this.value = value;
        }

        public bool IsReflection => value < 0;

        public int Rotation => value < 0 ? ~value : value;

        public static HexRotation Identity => new HexRotation(0);

        public static HexRotation PTReflectX => new HexRotation(~3);

        public static HexRotation PTReflectY => new HexRotation(~0);

        public static HexRotation FTReflectX => new HexRotation(~2);

        public static HexRotation FTReflectY => new HexRotation(~5);

        public static HexRotation RotateCCW => new HexRotation(1);

        public static HexRotation RotateCW => new HexRotation(-1);

        public static HexRotation Rotate60(int i) => new HexRotation((short)(((i % 6) + 6) % 6));

        public static HexRotation[] All => all;

        public HexRotation Invert()
        {
            if (IsReflection)
            {
                return this;
            }
            else
            {
                return new HexRotation((short)((6 - value) % 6));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is HexRotation rotation &&
                   value == rotation.value;
        }

        public override int GetHashCode()
        {
            return 45106587 + value.GetHashCode();
        }

        public static bool operator ==(HexRotation a, HexRotation b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(HexRotation a, HexRotation b)
        {
            return a.value != b.value;
        }

        public static HexRotation operator *(HexRotation a, HexRotation b)
        {
            var isReflection = a.IsReflection ^ b.IsReflection;
            var rotation = a * (b * 0);
            return new HexRotation(isReflection ? (short)~rotation : (short)rotation);
        }

        public static SquareDir operator *(HexRotation rotation, SquareDir dir)
        {
            var side = (int)(dir);
            var newSide = (rotation.IsReflection ? rotation.Rotation - side + 6 : rotation.Rotation + side) % 6;
            return (SquareDir)(newSide);
        }

        public Matrix4x4 ToMatrix(HexOrientation orientation)
        {
            var i = value;
            var rot = i < 0 ? ~i : i;
            var isReflection = i < 0;
            var rotM = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 360.0f / 6 * rot));
            if(isReflection)
            {
                if(orientation == HexOrientation.PointyTopped)
                {
                    // TODO: To Constant?
                    return rotM * Matrix4x4.Scale(new Vector3(1, -1, 1));
                }
                else
                {
                    // TODO: To Constant?
                    return rotM * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 30)) * Matrix4x4.Scale(new Vector3(1, -1, 1)) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, -30));
                }
            }
            else
            {
                return rotM;
            }
        }

        public static HexRotation? FromMatrix(Matrix4x4 m, HexOrientation orientation)
        {
            const float eps = 1e-6f;

            // Check it's a rotation in the XY plane
            var forward = m.MultiplyVector(Vector3.forward);
            if (Vector3.Distance(forward, Vector3.forward) > eps)
            {
                return null;
            }

            var right = m.MultiplyVector(Vector3.right);

            var isReflection = false;
            if (m.determinant < 0)
            {
                isReflection = true;
                right.x = -right.x;
            }

            var angle = Mathf.Atan2(right.y, right.x);
            var angleInt = Mathf.RoundToInt(angle / (Mathf.PI / 3));

            if (orientation == HexOrientation.FlatTopped)
            {
                return (isReflection ? FTReflectX : Identity) * Rotate60(angleInt);
            }
            else
            {
                return (isReflection ? PTReflectX : Identity) * Rotate60(angleInt);
            }
        }

        public static implicit operator HexRotation(CellRotation r) => new HexRotation((short)r);

        public static implicit operator CellRotation(HexRotation r) => (CellRotation)r.value;
    }
}
