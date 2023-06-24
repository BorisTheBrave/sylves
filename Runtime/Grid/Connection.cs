using System;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Represents how the edges (2d) or faces (3d) of cells can connect to each other.
    /// In 2d, rotation/sides are unused, as two edges can only connect together normally or reflected.
    /// In 3d, this represents both a rotation and reflection, similar to NGonCellType rotations.
    /// As there, Mirror inverts the y-axis, and is applied before rotation, which is counter clockwise.
    /// </summary>
    public struct Connection : IEquatable<Connection>
    {
        public bool Mirror { get; set; }
        public int Rotation { get; set; }
        public int Sides { get; set; }

        public static Connection operator*(Connection a, Connection b)
        {
            var n = a.Sides != 0 ? a.Sides : b.Sides;
            int rotation;

            if (n == 0)
            {
                rotation = 0;
            }
            else
            {
                // Same logic as NGonCellType
                if (!a.Mirror)
                {
                    rotation = ((a.Rotation + b.Rotation) % n);
                }
                else
                {
                    rotation = ((n + a.Rotation - b.Rotation) % n);
                }
            }

            return new Connection
            {
                Rotation = rotation,
                Mirror = a.Mirror ^ b.Mirror,
                Sides = n,
            };
        }

        /// <summary>
        /// Give an equivalent rotation in the XY plane.
        /// </summary>
        public Matrix4x4 ToMatrix()
        {
            // Same logic as NGonCellType
            var rot = Rotation;
            var n = Sides;
            var isReflection = Mirror;
            var m = isReflection ? Matrix4x4.Scale(new Vector3(1, -1, 1)) : Matrix4x4.identity;
            m = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 360.0f / n * rot)) * m;
            return m;
            /*
            var m3 = Matrix4x4.Rotate(Quaternion.Euler(0, 0, Rotation * 90));
            if (Mirror)
            {
                m3 = m3 * Matrix4x4.Scale(new Vector3(1, -1, 1));
            }
            return m3;
            */
        }

        public Connection GetInverse()
        {
            if (Rotation != 0)
                throw new NotImplementedException();
            return new Connection
            {
                Mirror = Mirror,
            };
        }

        public bool Equals(Connection other) => Mirror == other.Mirror && Rotation == other.Rotation && Sides == other.Sides;
        public override bool Equals(object other)
        {
            if (other is Connection v)
                return Equals(v);
            return false;
        }
        public override int GetHashCode() => (Mirror, Rotation, Sides).GetHashCode();

        public override string ToString()
        {
            if (Sides > 0)
            {
                return $"(Mirror={Mirror}, Rotation={Rotation}/{Sides})";
            }
            else
            {
                return $"(Mirror={Mirror})";
            }
        }

        public static bool operator ==(Connection lhs, Connection rhs) => lhs.Equals(rhs);
        public static bool operator !=(Connection lhs, Connection rhs) => !(lhs == rhs);

    }
}
