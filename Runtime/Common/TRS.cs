using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{

    /// <summary>
    /// Represents a position / rotation and scale.
    /// Much like a Unity Transform, but without the association with a Unity object.
    /// </summary>
    public class TRS
    {
        public TRS()
        {
            Position = Vector3.zero;
            Rotation = Quaternion.identity;
            Scale = Vector3.one;
        }

        public TRS(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public TRS(Vector3 position)
        {
            Position = position;
            Rotation = Quaternion.identity;
            Scale = Vector3.one;
        }

        public TRS(Matrix4x4 m)
        {
            // Unity's primitives don't seem too robust.
            // lossyScale is not really designed for this.
            // So isntead, we use a hand rolled solution, cribbed from UnityShim.Matrix4x4
            /*
            var scale = m.lossyScale;
            m = m * Matrix4x4.Scale(new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
            Position = m.MultiplyPoint(Vector3.zero);
            Rotation = m.rotation;
            Scale = scale;
            */

            /*
            var scale = m.lossyScale;
            Debug.Log($"{scale}");
            m = m * Matrix4x4.Scale(new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
            Debug.Log($"{m} {m.determinant}");
            Position = m.MultiplyPoint(Vector3.zero);
            Rotation = m.rotation;
            Scale = scale;
            */
            Position = m.MultiplyPoint(Vector3.zero);

            var mx = m.MultiplyVector(Vector3.right);
            var my = Vector3.ProjectOnPlane(m.MultiplyVector(Vector3.up), mx);
            var mz = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(m.MultiplyVector(Vector3.forward), mx), my);
            var isReflection = Vector3.Dot(mx, Vector3.Cross(my, mz)) < 0;
            Scale = new Vector3((isReflection ? -1 : 1) * mx.magnitude, my.magnitude, mz.magnitude);
            mx /= Scale.x;
            my /= Scale.y;
            mz /= Scale.z;

            // https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
            // I believe this is known as shepherds method.
            float tr = mx.x + my.y + mz.z;
            if (tr > 0)
            {
                float S = Mathf.Sqrt(tr + 1.0f) * 2; // S=4*qw 
                var qw = 0.25f * S;
                var qx = (my.z - mz.y) / S;
                var qy = (mz.x - mx.z) / S;
                var qz = (mx.y - my.x) / S;
                Rotation = new Quaternion(qx, qy, qz, qw);
            }
            else if ((mx.x > my.y) & (mx.x > mz.z))
            {
                float S = Mathf.Sqrt(1.0f + mx.x - my.y - mz.z) * 2; // S=4*qx 
                var qw = (my.z - mz.y) / S;
                var qx = 0.25f * S;
                var qy = (my.x + mx.y) / S;
                var qz = (mz.x + mx.z) / S;
                Rotation = new Quaternion(qx, qy, qz, qw);
            }
            else if (my.y > mz.z)
            {
                float S = Mathf.Sqrt(1.0f + my.y - mx.x - mz.z) * 2; // S=4*qy
                var qw = (mz.x - mx.z) / S;
                var qx = (my.x + mx.y) / S;
                var qy = 0.25f * S;
                var qz = (mz.y + my.z) / S;
                Rotation = new Quaternion(qx, qy, qz, qw);
            }
            else
            {
                float S = Mathf.Sqrt(1.0f + mz.z - mx.x - my.y) * 2; // S=4*qz
                var qw = (mx.y - my.x) / S;
                var qx = (mz.x + mx.z) / S;
                var qy = (mz.y + my.z) / S;
                var qz = 0.25f * S;
                Rotation = new Quaternion(qx, qy, qz, qw);
            }
        }

        public static TRS Local(Transform t)
        {
            return new TRS(t.localPosition, t.localRotation, t.localScale);
        }

        public static TRS World(Transform t)
        {
            return new TRS(t.position, t.rotation, t.lossyScale);
        }

        public Matrix4x4 ToMatrix()
        {
            return Matrix4x4.TRS(Position, Rotation, Scale);
        }

        public static TRS operator *(TRS a, TRS b)
        {
            // TOOD: More efficient
            return new TRS(a.ToMatrix() * b.ToMatrix());
        }

        public Vector3 Position { get; internal set; }
        public Quaternion Rotation { get; internal set; }
        public Vector3 Scale { get; internal set; }
    }
}
