using System;
using System.Diagnostics;

namespace Sylves
{
#if !UNITY
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        [DebuggerStepThrough]
        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        // TODO
        //public float this[int index] { get; set; }

        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
        public Vector3 eulerAngles
        {
            get
            {
                // From wikipedia
                var angles = new Vector3();

                // roll (x-axis rotation)
                var sinr_cosp = 2 * (w * x + y * z);
                var cosr_cosp = 1 - 2 * (x * x + y * y);
                angles.x = Mathf.Atan2(sinr_cosp, cosr_cosp);

                // pitch (y-axis rotation)
                var sinp = 2 * (w * y - z * x);
                if (sinp >= 1)
                    angles.y = Mathf.PI; // use 90 degrees if out of range
                else if (sinp <= -1)
                    angles.y = -Mathf.PI;
                else
                    angles.y = Mathf.Asin(sinp);

                // yaw (z-axis rotation)
                var siny_cosp = 2 * (w * z + x * y);
                var cosy_cosp = 1 - 2 * (y * y + z * z);
                angles.z = Mathf.Atan2(siny_cosp, cosy_cosp);

                return angles * 180 / Mathf.PI;
            }
            set
            {
                value *= Mathf.PI / 180;
                // From wikipedia
                var cy = Mathf.Cos(value.x * 0.5f);
                var sy = Mathf.Sin(value.x * 0.5f);
                var cp = Mathf.Cos(value.y * 0.5f);
                var sp = Mathf.Sin(value.y * 0.5f);
                var cr = Mathf.Cos(value.z * 0.5f);
                var sr = Mathf.Sin(value.z * 0.5f);

                w = cr * cp * cy + sr * sp * sy;
                z = sr * cp * cy - cr * sp * sy;
                y = cr * sp * cy + sr * cp * sy;
                x = cr * cp * sy - sr * sp * cy;
            }
        }
        /*
        public Vector3 eulerAngles { get; set; }
        public Quaternion normalized { get; }

        public static float Angle(Quaternion a, Quaternion b);
        public static Quaternion AngleAxis(float angle, Vector3 axis);
        public static float Dot(Quaternion a, Quaternion b);
        */
        public static Quaternion Euler(Vector3 euler)
        {
            var q = new Quaternion();
            q.eulerAngles = euler;
            return q;
        }
        public static Quaternion Euler(float x, float y, float z) => Euler(new Vector3(x, y, z));
        /*
        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection);
        public static Quaternion Inverse(Quaternion rotation);
        public static Quaternion Lerp(Quaternion a, Quaternion b, float t);
        public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, float t);
        public static Quaternion LookRotation(Vector3 forward);
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards);
        public static Quaternion Normalize(Quaternion q);
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta);
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t);
        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t);
        */
        public bool Equals(Quaternion other) => x == other.x && y == other.y && z == other.z && w == other.w;
        public override bool Equals(object other)
        {
            if(other is Quaternion q)
            {
                return Equals(q);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode() => (x, y, z, w).GetHashCode();
        /*
        public void Normalize();
        public void Set(float newX, float newY, float newZ, float newW);
        public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection);
        public void SetLookRotation(Vector3 view, Vector3 up);
        public void SetLookRotation(Vector3 view);
        public void ToAngleAxis(out float angle, out Vector3 axis);
        */
        public string ToString(string format) => ToString();
        public override string ToString() => $"({x}, {y}, {z}, {w})";
        /*
        public static Vector3 operator *(Quaternion rotation, Vector3 point);
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs);
        public static bool operator ==(Quaternion lhs, Quaternion rhs);
        public static bool operator !=(Quaternion lhs, Quaternion rhs);
        */
    }
#endif
}
