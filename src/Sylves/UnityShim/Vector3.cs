using System;

namespace Sylves
{
    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }


        public float this[int index]
        {
            get { switch (index) { case 0: return x; case 1: return y; case 2: return z; default: throw new IndexOutOfRangeException(); } }
            set { switch (index) { case 0: x = value; break; case 1: y = value; break; case 2: z = value; break; default: throw new IndexOutOfRangeException(); } }
        }
        public static Vector3 right => new Vector3(1, 0, 0);
        public static Vector3 left => new Vector3(-1, 0, 0);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 down => new Vector3(0, -1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 back => new Vector3(0, 0, -1);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 zero => new Vector3();
        public static Vector3 positiveInfinity => new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static Vector3 negativeInfinity => new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public Vector3 normalized => this / magnitude;
        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public float sqrMagnitude => x * x + y * y + z * z;

        public static float Angle(Vector3 from, Vector3 to) => (float)(180 / Math.PI * Math.Acos(Dot(from, to) / from.magnitude / to.magnitude));
        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            var m = vector.magnitude;
            if(m < maxLength)
            {
                return vector;
            }
            else
            {
                return vector * (maxLength / m);
            }
        }
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs) => new Vector3(
            lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x
            );
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => LerpUnclamped(a, b, Mathf.Clamp01(t));
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) => a * (1 - t) + b * t;
        public static float Magnitude(Vector3 a) => a.magnitude;
        public static Vector3 Max(Vector3 lhs, Vector3 rhs) => new Vector3(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));
        public static Vector3 Min(Vector3 lhs, Vector3 rhs) => new Vector3(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
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
        public static Vector3 Normalize(Vector3 a) => a.normalized;
        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal)
        {
            normal.Normalize();
            tangent = ProjectOnPlane(tangent, normal).normalized;
            binormal = ProjectOnPlane(ProjectOnPlane(binormal, normal), tangent).normalized;
        }
        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent)
        {
            normal.Normalize();
            tangent = ProjectOnPlane(tangent, normal).normalized;
        }
        public static Vector3 Project(Vector3 a, Vector3 b) => b * Dot(a, b) / b.sqrMagnitude;
        // TODO: Check if Unity divides by sqrMagnitude
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal) => vector - planeNormal * Dot(vector, planeNormal) / planeNormal.sqrMagnitude;
        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal) => inDirection - 2 * inNormal * Dot(inDirection, inNormal) / inNormal.sqrMagnitude;
        //public static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta);

        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            from = Cross(axis, from);
            to = Cross(axis, to);
            return Angle(from, to) * (Dot(axis, Cross(from, to)) > 0 ? 1 : -1);
        }

        public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => SlerpUnclamped(a, b, Mathf.Clamp01(t));
        public static Vector3 SlerpUnclamped(Vector3 a, Vector3 b, float t)
        {
            var ma = a.magnitude;
            var mb = b.magnitude;
            a /= ma;
            b /= mb;
            var angle = Mathf.Acos(Dot(a, b));
            var s = Mathf.Sin(angle);
            var v = Mathf.Sin((1 - t) * angle) / s * a + Mathf.Sin(t * angle) / s * b;
            return v * ((1 - t) * ma + t * mb);

        }

        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime);
        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed);
        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime);
        public static float SqrMagnitude(Vector3 a) => a.sqrMagnitude;
        public bool Equals(Vector3 other) => x == other.x && y == other.y && z == other.z;
        public override bool Equals(object other)
        {
            if (other is Vector3 v)
                return Equals(v);
            return false;
        }
        public override int GetHashCode() => (x, y, z).GetHashCode();
        public void Normalize()
        {
            var m = magnitude;
            this.x *= m;
            this.y *= m;
            this.z *= m;
        }
        public void Scale(Vector3 scale)
        {
            this.x *= scale.x;
            this.y *= scale.y;
            this.z *= scale.z;
        }
        public void Set(float newX, float newY, float newZ)
        {
            this.x = newX;
            this.y = newY;
            this.z = newZ;
        }
        public float SqrMagnitude() => sqrMagnitude;
        public override string ToString() => $"({x}, {y}, {z})";
        public string ToString(string format) => ToString();

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator *(float a, Vector3 b) => new Vector3(a * b.x, a * b.y, a * b.z);
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b);
        public static Vector3 operator /(Vector3 a, float b) => new Vector3(a.x / b, a.y / b, a.z / b);
        public static bool operator ==(Vector3 lhs, Vector3 rhs) => lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);
    }
}
