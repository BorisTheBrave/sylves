using System;
using System.Diagnostics;

namespace Sylves
{
#if !UNITY
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// A pure .NET implemenation of Unity's Vector4.
    /// See Unity's docs for more details.
    /// </summary>
    public struct Vector4
    {
        [DebuggerStepThrough]
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }


        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public float this[int index]
        {
            get { switch (index) { case 0: return x; case 1: return y; case 2: return z; case 3: return w; default: throw new IndexOutOfRangeException(); } }
            set { switch (index) { case 0: x = value; break; case 1: y = value; break; case 2: z = value; break; case 3: w = value; break; default: throw new IndexOutOfRangeException(); } }
        }

        public static Vector4 one => new Vector4(1, 1, 1, 1);
        public static Vector4 zero => new Vector4();
        public static Vector4 positiveInfinity => new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static Vector4 negativeInfinity => new Vector4(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public Vector4 normalized => this / magnitude;
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public float sqrMagnitude => x * x + y * y + z * z + w * w;

        public static float Distance(Vector4 a, Vector4 b) => (a - b).magnitude;
        public static float Dot(Vector4 a, Vector4 b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        public static Vector4 Lerp(Vector4 a, Vector4 b, float t) => LerpUnclamped(a, b, Mathf.Clamp01(t));
        public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, float t) => a * (1 - t) + b * t;
        public static float Magnitude(Vector4 a) => a.magnitude;
        public static Vector4 Max(Vector4 lhs, Vector4 rhs) => new Vector4(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z), Math.Max(lhs.w, rhs.w));
        public static Vector4 Min(Vector4 lhs, Vector4 rhs) => new Vector4(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z), Math.Min(lhs.w, rhs.w));
        public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
        {
            var v = (target - current);
            var d = v.magnitude;
            if(d < maxDistanceDelta)
            {
                return target;
            }
            else
            {
                return current + (maxDistanceDelta / d) * v;
            }
        }
        public static Vector4 Normalize(Vector4 a) => a.normalized;
        public static Vector4 Project(Vector4 a, Vector4 b) => b * Dot(a, b) / b.sqrMagnitude;
        public static Vector4 Scale(Vector4 a, Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static float SqrMagnitude(Vector4 a) => a.sqrMagnitude;
        public bool Equals(Vector4 other) => x == other.x && y == other.y && z == other.z && w == other.w;
        public override bool Equals(object other)
        {
            if (other is Vector4 v)
                return Equals(v);
            return false;
        }
        public override int GetHashCode() => (x, y, z, w).GetHashCode();
        public void Normalize()
        {
            var m = magnitude;
            this.x /= m;
            this.y /= m;
            this.z /= m;
            this.w /= m;
        }
        public void Scale(Vector4 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
            this.w *= scale.w;
        }
        public void Set(float newX, float newY, float newZ, float newW)
        {
            this.x = newX;
            this.y = newY;
            this.z = newZ;
            this.w = newW;
        }
        public float SqrMagnitude() => sqrMagnitude;
        public override string ToString() => $"({x}, {y}, {z}, {w})";
        public string ToString(string format) => ToString();

        public static Vector4 operator +(Vector4 a, Vector4 b) => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static Vector4 operator -(Vector4 a) => new Vector4(-a.x, -a.y, -a.z, -a.w);
        public static Vector4 operator -(Vector4 a, Vector4 b) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static Vector4 operator *(Vector4 a, Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static Vector4 operator *(float a, Vector4 b) => new Vector4(a * b.x, a * b.y, a * b.z, a * b.w);
        public static Vector4 operator *(Vector4 a, float b) => new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
        public static Vector4 operator /(Vector4 a, float b) => new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
        public static bool operator ==(Vector4 lhs, Vector4 rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
        public static bool operator !=(Vector4 lhs, Vector4 rhs) => !(lhs == rhs);

        // Ignoring this unity misfeature.
        //public static implicit operator Vector4(Vector3 v);
        //public static implicit operator Vector3(Vector4 v);
        //public static implicit operator Vector4(Vector2 v);
        //public static implicit operator Vector2(Vector4 v);
    }
#endif
}
