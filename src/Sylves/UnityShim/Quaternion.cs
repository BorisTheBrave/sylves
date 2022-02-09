using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

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
        /*
        public Vector3 eulerAngles { get; set; }
        public Quaternion normalized { get; }

        public static float Angle(Quaternion a, Quaternion b);
        public static Quaternion AngleAxis(float angle, Vector3 axis);
        public static float Dot(Quaternion a, Quaternion b);
        public static Quaternion Euler(Vector3 euler);
        public static Quaternion Euler(float x, float y, float z);
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
        public string ToString(string format);
        public override string ToString();

        public static Vector3 operator *(Quaternion rotation, Vector3 point);
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs);
        public static bool operator ==(Quaternion lhs, Quaternion rhs);
        public static bool operator !=(Quaternion lhs, Quaternion rhs);
        */
    }
}
