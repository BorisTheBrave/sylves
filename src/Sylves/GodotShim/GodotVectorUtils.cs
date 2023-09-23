using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sylves
{
#if GODOT
    public static class GodotVectorUtils
    {
        // Quaternion

        // TODO: I suspect this doesn't match Unity's conventions
        public static Godot.Quaternion AngleAxis(float angle, Godot.Vector3 axis)
        {
            angle *= Mathf.PI / 180;
            axis.Normalize();
            var c = Mathf.Cos(angle / 2);
            var s = Mathf.Sin(angle / 2);
            return new Godot.Quaternion(axis.X * s, axis.Y * s, axis.Z * s, c);
        }

        // TODO: I suspect this doesn't match Unity's conventions
        public static Godot.Quaternion Euler(float x, float y, float z)
        {
            const float deg2rad = (float)Math.PI / 180;
            return Godot.Quaternion.FromEuler(new Godot.Vector3(x * deg2rad, y * deg2rad, z * deg2rad));
        }

        // Vector2
        public static Godot.Vector2 Min(Godot.Vector2 a, Godot.Vector2 b) => new Godot.Vector2(Godot.Mathf.Min(a.X, b.X), Godot.Mathf.Min(a.Y, b.Y));
        public static Godot.Vector2 Max(Godot.Vector2 a, Godot.Vector2 b) => new Godot.Vector2(Godot.Mathf.Max(a.X, b.X), Godot.Mathf.Max(a.Y, b.Y));

        public static float Distance(Godot.Vector2 a, Godot.Vector2 b) => (a - b).Length();
        public static void Normalize(this Godot.Vector2 a)
        {
            var l = a.Length();
            a.X /= l;
            a.Y /= l;
        }
        public static Godot.Vector2 Scale(Godot.Vector2 a, Godot.Vector2 b) => new Godot.Vector2(a.X * b.X, a.Y * b.Y);
        public static float Dot(Godot.Vector2 a, Godot.Vector2 b) => a.X * b.X + a.Y * b.Y;
        public static Godot.Vector2 ProjectOntoPlane(Godot.Vector2 vector, Godot.Vector2 planeNormal) => vector - planeNormal * vector.Dot(planeNormal) / planeNormal.LengthSquared();

        // Vector2I
        public static Godot.Vector2I Min(Godot.Vector2I a, Godot.Vector2I b) => new Godot.Vector2I(Godot.Mathf.Min(a.X, b.X), Godot.Mathf.Min(a.Y, b.Y));
        public static Godot.Vector2I Max(Godot.Vector2I a, Godot.Vector2I b) => new Godot.Vector2I(Godot.Mathf.Max(a.X, b.X), Godot.Mathf.Max(a.Y, b.Y));
        public static Godot.Vector2I FloorToInt(Godot.Vector2 a) => new Godot.Vector2I(Mathf.FloorToInt(a.X), Mathf.FloorToInt(a.Y));


        // Vector3
        public static Godot.Vector3 Min(Godot.Vector3 a, Godot.Vector3 b) => new Godot.Vector3(Godot.Mathf.Min(a.X, b.X), Godot.Mathf.Min(a.Y, b.Y), Godot.Mathf.Min(a.Z, b.Z));
        public static Godot.Vector3 Max(Godot.Vector3 a, Godot.Vector3 b) => new Godot.Vector3(Godot.Mathf.Max(a.X, b.X), Godot.Mathf.Max(a.Y, b.Y), Godot.Mathf.Max(a.Z, b.Z));

        public static float Distance(Godot.Vector3 a, Godot.Vector3 b) => (a - b).Length();
        public static Godot.Vector3 Cross(Godot.Vector3 a, Godot.Vector3 b) => a.Cross(b);
        public static void Normalize(this Godot.Vector3 a)
        {
            var l = a.Length();
            a.X /= l;
            a.Y /= l;
            a.Z /= l;
        }
        public static Godot.Vector3 Scale(Godot.Vector3 a, Godot.Vector3 b) => new Godot.Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static float Dot(Godot.Vector3 a, Godot.Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Godot.Vector3 ProjectOntoPlane(Godot.Vector3 vector, Godot.Vector3 planeNormal) => vector - planeNormal * vector.Dot(planeNormal) / planeNormal.LengthSquared();

        // Vector3I
        public static Godot.Vector3I Min(Godot.Vector3I a, Godot.Vector3I b) => new Godot.Vector3I(Godot.Mathf.Min(a.X, b.X), Godot.Mathf.Min(a.Y, b.Y), Godot.Mathf.Min(a.Z, b.Z));
        public static Godot.Vector3I Max(Godot.Vector3I a, Godot.Vector3I b) => new Godot.Vector3I(Godot.Mathf.Max(a.X, b.X), Godot.Mathf.Max(a.Y, b.Y), Godot.Mathf.Max(a.Z, b.Z));
        public static Godot.Vector3I FloorToInt(Godot.Vector3 a) => new Godot.Vector3I(Mathf.FloorToInt(a.X), Mathf.FloorToInt(a.Y), Mathf.FloorToInt(a.Z));
    }
#endif
}
