using System;
using System.Diagnostics;

namespace Sylves
{
#if !UNITY
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// A pure .NET implemenation of Unity's Vector3Int.
    /// See Unity's docs for more details.
    /// </summary>
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        [DebuggerStepThrough]
        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

#if GODOT
        public static implicit operator Godot.Vector3I(Vector3Int v) => new Godot.Vector3I(v.x, v.y, v.z);
        public static implicit operator Vector3Int(Godot.Vector3I v) => new Vector3Int(v.X, v.Y, v.Z);

        // Also do Vector2 conversions for convenience
        public static implicit operator Godot.Vector2I(Vector3Int v) => new Godot.Vector2I(v.x, v.y);
        public static implicit operator Vector3Int(Godot.Vector2I v) => new Vector3Int(v.X, v.Y, 0);
#endif

        public static Vector3Int down => new Vector3Int(0, -1, 0);
        public static Vector3Int up => new Vector3Int(0, 1, 0);
        public static Vector3Int one => new Vector3Int(1, 1, 1);
        public static Vector3Int zero => new Vector3Int(0, 0, 0);
        public static Vector3Int left => new Vector3Int(-1, 0, 0);
        public static Vector3Int right => new Vector3Int(1, 0, 0);
        // Note: There's no forward/back, because Unity also misses them

        public int this[System.Int32 index]
        {
            get { switch(index) { case 0: return x; case 1: return y; case 2: return z; default: throw new IndexOutOfRangeException(); } }
            set { switch(index) { case 0: x = value; break; case 1: y = value; break; case 2: z = value; break; default: throw new IndexOutOfRangeException();} }
        }
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int sqrMagnitude => x * x + y * y + z * z;

        public static Vector3Int CeilToInt(Vector3 v) => new Vector3Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
        public static float Distance(Vector3Int a, Vector3Int b) => (a - b).magnitude;
        public static Vector3Int FloorToInt(Vector3 v) => new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        public static Vector3Int Max(Vector3Int lhs, Vector3Int rhs) => new Vector3Int(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        public static Vector3Int Min(Vector3Int lhs, Vector3Int rhs) => new Vector3Int(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        public static Vector3Int RoundToInt(Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        public static Vector3Int Scale(Vector3Int a, Vector3Int b) => new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
        public void Clamp(Vector3Int min, Vector3Int max) => Min(Max(this, min), max);
        public bool Equals(Vector3Int other) => x == other.x && y == other.y && z == other.z;
        public override bool Equals(object other)
        {
            if(other is Vector3Int v)
                return Equals(v);
            return false;
        }
        public override System.Int32 GetHashCode() => (x, y, z).GetHashCode();
        public void Scale(Vector3Int scale) => Scale(this, scale);
        public void Set(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString() => $"({x}, {y}, {z})";
        public string ToString(string format) => ToString();

        public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3Int operator -(Vector3Int a) => new Vector3Int(-a.x, -a.y, -a.z);
        public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3Int operator *(Vector3Int a, Vector3Int b) => new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3Int operator *(int a, Vector3Int b) => new Vector3Int(a * b.x, a * b.y, a * b.z);
        public static Vector3Int operator *(Vector3Int a, int b) => new Vector3Int(a.x * b, a.y * b, a.z * b);
        public static Vector3Int operator /(Vector3Int a, int b) => new Vector3Int(a.x / b, a.y / b, a.z / b);
        public static bool operator ==(Vector3Int lhs, Vector3Int rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        public static bool operator !=(Vector3Int lhs, Vector3Int rhs) => !(lhs == rhs);

        public static implicit operator Vector3(Vector3Int v) => new Vector3(v.x, v.y, v.z);

        // Ignoring this unity misfeature.
        //public static explicit operator Vector2Int(Vector3Int v);
    }
#endif
}
