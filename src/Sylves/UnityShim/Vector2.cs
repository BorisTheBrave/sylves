using System;
using System.Diagnostics;

namespace Sylves
{
#if !UNITY
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// A pure .NET implemenation of Unity's Vector2.
    /// See Unity's docs for more details.
    /// </summary>
    public struct Vector2
    {
        [DebuggerStepThrough]
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

#if GODOT
        public static implicit operator Godot.Vector2(Vector2 v) => new Godot.Vector2(v.x, v.y);
        public static implicit operator Vector2(Godot.Vector2 v) => new Vector2(v.X, v.Y);
#endif

        public float x { get; set; }
        public float y { get; set; }
        public float this[Int32 index]
        {
            get { switch (index) { case 0: return x; case 1: return y; ; default: throw new IndexOutOfRangeException(); } }
            set { switch (index) { case 0: x = value; break; case 1: y = value; break; default: throw new IndexOutOfRangeException(); } }
        }
        public static Vector2 right => new Vector2(1, 0);
        public static Vector2 left => new Vector2(-1, 0);
        public static Vector2 up => new Vector2(0, 1);
        public static Vector2 down => new Vector2(0, -1);
        public static Vector2 one => new Vector2(1, 1);
        public static Vector2 zero => new Vector2();
        public static Vector2 positiveInfinity => new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        public static Vector2 negativeInfinity => new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        public Vector2 normalized => this / magnitude;
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public float sqrMagnitude => x * x + y * y;

        public static float Angle(Vector2 from, Vector2 to) => (float)(180 / Math.PI * Math.Acos(Dot(from, to) / from.magnitude / to.magnitude));
        public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
        {
            var m = vector.magnitude;
            if (m < maxLength)
            {
                return vector;
            }
            else
            {
                return vector * (maxLength / m);
            }
        }
        public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;
        public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => LerpUnclamped(a, b, Mathf.Clamp01(t));
        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t) => a * (1 - t) + b * t;
        public static float Magnitude(Vector2 a) => a.magnitude;
        public static Vector2 Max(Vector2 lhs, Vector2 rhs) => new Vector2(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y));
        public static Vector2 Min(Vector2 lhs, Vector2 rhs) => new Vector2(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y));
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            var v = (target - current);
            var d = v.magnitude;
            if (d < maxDistanceDelta)
            {
                return target;
            }
            else
            {
                return current + (maxDistanceDelta / d) * v;
            }
        }
        public static Vector2 Perpendicular(Vector2 inDirection) => new Vector2(-inDirection.y, inDirection.x);
        public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal) => inDirection - 2 * inNormal * Dot(inDirection, inNormal);

        public static Vector2 Scale(Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);
        public static float SignedAngle(Vector2 from, Vector2 to)
        {
            float cross = from.x * to.y - from.y * to.x;
            float dot = from.x * to.x + from.y * to.y;
            return (180 / Mathf.PI * Mathf.Atan2(cross, dot));
        }
        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed);
        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime);
        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime);
        public static float SqrMagnitude(Vector2 a) => a.sqrMagnitude;
        public bool Equals(Vector2 other) => x == other.x && y == other.y;
        public override bool Equals(object other)
        {
            if (other is Vector2 v)
                return Equals(v);
            return false;
        }
        public override System.Int32 GetHashCode() => (x, y).GetHashCode();
        public void Normalize()
        {
            var m = magnitude;
            this.x /= m;
            this.y /= m;
        }
        public void Scale(Vector2 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
        }
        public void Set(float newX, float newY)
        {
            this.x = newX;
            this.y = newY;
        }
        public float SqrMagnitude() => sqrMagnitude;
        public override string ToString() => $"({x}, {y})";
        public string ToString(string format) => ToString();

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y);
        public static Vector2 operator *(float a, Vector2 b) => new Vector2(a * b.x, a * b.y);
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x * b, a.y * b);
        public static Vector2 operator /(Vector2 a, float b) => new Vector2(a.x / b, a.y / b);
        public static Vector2 operator /(Vector2 a, Vector2 b) => new Vector2(a.x / b.x, a.y / b.y);

        public static bool operator ==(Vector2 lhs, Vector2 rhs) => lhs.x == rhs.x && lhs.y == rhs.y;
        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

        //public static implicit operator Vector3(Vector2 v);
        //public static implicit operator Vector2(Vector3 v);
    }
#endif
}
