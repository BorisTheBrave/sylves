using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public struct Matrix4x4 : IEquatable<Matrix4x4>
    {
        public float m00 { get { return column0.x; } set { column0.x = value; } }
        public float m10 { get { return column0.y; } set { column0.y = value; } }
        public float m20 { get { return column0.z; } set { column0.z = value; } }
        public float m30 { get { return column0.w; } set { column0.w = value; } }
        public float m01 { get { return column1.x; } set { column1.x = value; } }
        public float m11 { get { return column1.y; } set { column1.y = value; } }
        public float m21 { get { return column1.z; } set { column1.z = value; } }
        public float m31 { get { return column1.w; } set { column1.w = value; } }
        public float m02 { get { return column2.x; } set { column2.x = value; } }
        public float m12 { get { return column2.y; } set { column2.y = value; } }
        public float m22 { get { return column2.z; } set { column2.z = value; } }
        public float m32 { get { return column2.w; } set { column2.w = value; } }
        public float m03 { get { return column3.x; } set { column3.x = value; } }
        public float m13 { get { return column3.y; } set { column3.y = value; } }
        public float m23 { get { return column3.z; } set { column3.z = value; } }
        public float m33 { get { return column3.w; } set { column3.w = value; } }

        public Vector4 column0;
        public Vector4 column1; 
        public Vector4 column2; 
        public Vector4 column3;

        public Matrix4x4(Vector4 column0, Vector4 column1, Vector4 column2, Vector4 column3)
        {
            this.column0 = column0;
            this.column1 = column1;
            this.column2 = column2;
            this.column3 = column3;
        }

        //public float this[int index] { get; set; }
        //public float this[int row, int column] { get; set; }

        public static Matrix4x4 zero => new Matrix4x4(Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero);
        public static Matrix4x4 identity => new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
        public Matrix4x4 transpose => new Matrix4x4(GetRow(0), GetRow(1), GetRow(2), GetRow(3));
        public Quaternion rotation
        {
            get
            {
                // Orthogonalize
                var mx = MultiplyVector(Vector3.right).normalized;
                var my = Vector3.ProjectOnPlane(MultiplyVector(Vector3.up), mx).normalized;
                var mz = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(MultiplyVector(Vector3.forward), mx), my).normalized;
                var isReflection = Vector3.Dot(mx, Vector3.Cross(my, mz)) < 0;
                if (isReflection) mx *= -1;

                // https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
                // I believe this is known as shepherds method.
                float tr = mx.x + my.y+ mz.z;
                if (tr > 0)
                {
                    float S = Mathf.Sqrt(tr + 1.0f) * 2; // S=4*qw 
                    var qw = 0.25f * S;
                    var qx = (my.z - mz.y) / S;
                    var qy = (mz.x - mx.z) / S;
                    var qz = (mx.y - my.x) / S;
                    return new Quaternion(qx, qy, qz, qw);
                }
                else if ((mx.x > my.y) & (mx.x > mz.z))
                {
                    float S = Mathf.Sqrt(1.0f + mx.x - my.y - mz.z) * 2; // S=4*qx 
                    var qw = (my.z - mz.y) / S;
                    var qx = 0.25f * S;
                    var qy = (my.x + mx.y) / S;
                    var qz = (mz.x + mx.z) / S;
                    return new Quaternion(qx, qy, qz, qw);
                }
                else if (my.y > mz.z)
                {
                    float S = Mathf.Sqrt(1.0f + my.y - mx.x - mz.z) * 2; // S=4*qy
                    var qw = (mz.x - mx.z) / S;
                    var qx = (my.x + mx.y) / S;
                    var qy = 0.25f * S;
                    var qz = (mz.y + my.z) / S;
                    return new Quaternion(qx, qy, qz, qw);
                }
                else
                {
                    float S = Mathf.Sqrt(1.0f + mz.z - mx.x - my.y) * 2; // S=4*qz
                    var qw = (mx.y - my.x) / S;
                    var qx = (mz.x + mx.z) / S;
                    var qy = (mz.y + my.z) / S;
                    var qz = 0.25f * S;
                    return new Quaternion(qx, qy, qz, qw);
                }
            }
        }
        public Vector3 lossyScale
        {
            get
            {
                // Orthogonalize so this matches rotation, above
                var mx = MultiplyVector(Vector3.right);
                var my = Vector3.ProjectOnPlane(MultiplyVector(Vector3.up), mx);
                var mz = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(MultiplyVector(Vector3.forward), mx), my);
                var isReflection = Vector3.Dot(mx, Vector3.Cross(my, mz)) < 0;
                if (isReflection)
                {
                    return new Vector3(-mx.magnitude, my.magnitude, mz.magnitude);
                }
                else
                {
                    return new Vector3(mx.magnitude, my.magnitude, mz.magnitude);
                }
            }
        }
        public bool isIdentity => this == identity;
        public float determinant
        {
            get
            {
                // Copied from https://stackoverflow.com/a/44446912/14738198
                var A2323 = m22 * m33 - m23 * m32;
                var A1323 = m21 * m33 - m23 * m31;
                var A1223 = m21 * m32 - m22 * m31;
                var A0323 = m20 * m33 - m23 * m30;
                var A0223 = m20 * m32 - m22 * m30;
                var A0123 = m20 * m31 - m21 * m30;

                var det = m00 * (m11 * A2323 - m12 * A1323 + m13 * A1223)
                    - m01 * (m10 * A2323 - m12 * A0323 + m13 * A0223)
                    + m02 * (m10 * A1323 - m11 * A0323 + m13 * A0123)
                    - m03 * (m10 * A1223 - m11 * A0223 + m12 * A0123);

                return det;
            }
        }
        //public FrustumPlanes decomposeProjection { get; }
        public Matrix4x4 inverse
        {
            get
            {
                // Copied from https://stackoverflow.com/a/44446912/14738198
                var A2323 = m22 * m33 - m23 * m32;
                var A1323 = m21 * m33 - m23 * m31;
                var A1223 = m21 * m32 - m22 * m31;
                var A0323 = m20 * m33 - m23 * m30;
                var A0223 = m20 * m32 - m22 * m30;
                var A0123 = m20 * m31 - m21 * m30;
                var A2313 = m12 * m33 - m13 * m32;
                var A1313 = m11 * m33 - m13 * m31;
                var A1213 = m11 * m32 - m12 * m31;
                var A2312 = m12 * m23 - m13 * m22;
                var A1312 = m11 * m23 - m13 * m21;
                var A1212 = m11 * m22 - m12 * m21;
                var A0313 = m10 * m33 - m13 * m30;
                var A0213 = m10 * m32 - m12 * m30;
                var A0312 = m10 * m23 - m13 * m20;
                var A0212 = m10 * m22 - m12 * m20;
                var A0113 = m10 * m31 - m11 * m30;
                var A0112 = m10 * m21 - m11 * m20;

                var det = m00 * (m11 * A2323 - m12 * A1323 + m13 * A1223)
                    - m01 * (m10 * A2323 - m12 * A0323 + m13 * A0223)
                    + m02 * (m10 * A1323 - m11 * A0323 + m13 * A0123)
                    - m03 * (m10 * A1223 - m11 * A0223 + m12 * A0123);
                det = 1 / det;

                return new Matrix4x4()
                {
                    m00 = det * (m11 * A2323 - m12 * A1323 + m13 * A1223),
                    m01 = det * -(m01 * A2323 - m02 * A1323 + m03 * A1223),
                    m02 = det * (m01 * A2313 - m02 * A1313 + m03 * A1213),
                    m03 = det * -(m01 * A2312 - m02 * A1312 + m03 * A1212),
                    m10 = det * -(m10 * A2323 - m12 * A0323 + m13 * A0223),
                    m11 = det * (m00 * A2323 - m02 * A0323 + m03 * A0223),
                    m12 = det * -(m00 * A2313 - m02 * A0313 + m03 * A0213),
                    m13 = det * (m00 * A2312 - m02 * A0312 + m03 * A0212),
                    m20 = det * (m10 * A1323 - m11 * A0323 + m13 * A0123),
                    m21 = det * -(m00 * A1323 - m01 * A0323 + m03 * A0123),
                    m22 = det * (m00 * A1313 - m01 * A0313 + m03 * A0113),
                    m23 = det * -(m00 * A1312 - m01 * A0312 + m03 * A0112),
                    m30 = det * -(m10 * A1223 - m11 * A0223 + m12 * A0123),
                    m31 = det * (m00 * A1223 - m01 * A0223 + m02 * A0123),
                    m32 = det * -(m00 * A1213 - m01 * A0213 + m02 * A0113),
                    m33 = det * (m00 * A1212 - m01 * A0212 + m02 * A0112),
                };
            }
        }

        public static float Determinant(Matrix4x4 m) => m.determinant;
        //public static Matrix4x4 Frustum(float left, float right, float bottom, float top, float zNear, float zFar);
        //public static Matrix4x4 Frustum(FrustumPlanes fp);
        public static Matrix4x4 Inverse(Matrix4x4 m) => m.inverse;
        //public static bool Inverse3DAffine(Matrix4x4 input, ref Matrix4x4 result);
        //public static Matrix4x4 LookAt(Vector3 from, Vector3 to, Vector3 up);
        //public static Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar);
        //public static Matrix4x4 Perspective(float fov, float aspect, float zNear, float zFar);
        public static Matrix4x4 Rotate(Quaternion q)
        {
            var qx = q.x;
            var qy = q.y;
            var qz = q.z;
            var qw = q.w;
            return new Matrix4x4()
            {
                m00 = 1 - 2 * qy * qy - 2 * qz * qz,
                m01 = 2 * qx * qy - 2 * qz * qw,
                m02 = 2 * qx * qz + 2 * qy * qw,
                m10 = 2 * qx * qy + 2 * qz * qw,
                m11 = 1 - 2 * qx * qx - 2 * qz * qz,
                m12 = 2 * qy * qz - 2 * qx * qw,
                m20 = 2 * qx * qz - 2 * qy * qw,
                m21 = 2 * qy * qz + 2 * qx * qw,
                m22 = 1 - 2 * qx * qx - 2 * qy * qy,
                m33 = 1,
            };
        }
        public static Matrix4x4 Scale(Vector3 vector) => new Matrix4x4(new Vector4(vector.x, 0, 0, 0), new Vector4(0, vector.y, 0, 0), new Vector4(0, 0, vector.z, 0), new Vector4(0, 0, 0, 1));
        public static Matrix4x4 Translate(Vector3 vector) => new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(vector.x, vector.y, vector.z, 1));
        public static Matrix4x4 Transpose(Matrix4x4 m) => m.transpose;
        public static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s) => Translate(pos) * Rotate(q) * Scale(s);
        public override bool Equals(object other)
        {
            if(other is Matrix4x4 m)
            {
                return Equals(m);
            }
            return false;
        }
        public bool Equals(Matrix4x4 other) => column0 == other.column0 && column1 == other.column1 && column2 == other.column2 && column3 == other.column3;

        public Vector4 GetColumn(int index)
        {
            switch (index)
            {
                case 0: return column0;
                case 1: return column1;
                case 2: return column2;
                case 3: return column3;
                default: throw new IndexOutOfRangeException();
            }
        }

        public override int GetHashCode() => (column0, column1, column2, column3).GetHashCode();
        public Vector4 GetRow(int index)
        {
            switch(index)
            {
                case 0: return new Vector4(m00, m01, m02, m03);
                case 1: return new Vector4(m10, m11, m12, m13);
                case 2: return new Vector4(m20, m21, m22, m23);
                case 3: return new Vector4(m30, m31, m32, m33);
                default: throw new IndexOutOfRangeException();
            }
        }
        public Vector3 MultiplyPoint(Vector3 point) => MultiplyPoint3x4(point) /
            (point.x * m30 + point.y * m31 + point.z * m32 + m33);
        public Vector3 MultiplyPoint3x4(Vector3 point) => new Vector3(
            point.x * m00 + point.y * m01 + point.z * m02 + m03,
            point.x * m10 + point.y * m11 + point.z * m12 + m13,
            point.x * m20 + point.y * m21 + point.z * m22 + m23
            );
        public Vector3 MultiplyVector(Vector3 vector) => new Vector3(
            vector.x * m00 + vector.y * m01 + vector.z * m02,
            vector.x * m10 + vector.y * m11 + vector.z * m12,
            vector.x * m20 + vector.y * m21 + vector.z * m22
            );
        public void SetColumn(int index, Vector4 column)
        {
            switch (index)
            {
                case 0: column0 = column; return;
                case 1: column1 = column; return;
                case 2: column2 = column; return;
                case 3: column3 = column; return;
                default: throw new IndexOutOfRangeException();
            }
        }
        public void SetRow(int index, Vector4 row)
        {
            switch (index)
            {
                case 0: (m00, m01, m02, m03) = (row.x, row.y, row.z, row.w); return;
                case 1: (m10, m11, m12, m13) = (row.x, row.y, row.z, row.w); return;
                case 2: (m20, m21, m22, m23) = (row.x, row.y, row.z, row.w); return;
                case 3: (m30, m31, m32, m33) = (row.x, row.y, row.z, row.w); return;
                default: throw new IndexOutOfRangeException();
            }
        }
        public void SetTRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            var m = TRS(pos, q, s);
            column0 = m.column0;
            column1 = m.column1;
            column2 = m.column2;
            column3 = m.column3;
        }
        public override string ToString() => $"{m00}\t{m01}\t{m02}\t{m03}\n{m10}\t{m11}\t{m12}\t{m13}\n{m20}\t{m21}\t{m22}\t{m23}\n{m30}\t{m31}\t{m32}\t{m33}";
        public string ToString(string format) => ToString();
        //public Plane TransformPlane(Plane plane);
        //public bool ValidTRS();

        public static Vector4 operator *(Matrix4x4 lhs, Vector4 vector) => new Vector4(
            vector.x * lhs.m00 + vector.y * lhs.m01 + vector.z * lhs.m02 + vector.w * lhs.m03,
            vector.x * lhs.m10 + vector.y * lhs.m11 + vector.z * lhs.m12 + vector.w * lhs.m13,
            vector.x * lhs.m20 + vector.y * lhs.m21 + vector.z * lhs.m22 + vector.w * lhs.m23,
            vector.x * lhs.m30 + vector.y * lhs.m31 + vector.z * lhs.m32 + vector.w * lhs.m33
            );
        public static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs) => new Matrix4x4
        {
            m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30,
            m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30,
            m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30,
            m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30,
            m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31,
            m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31,
            m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31,
            m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31,
            m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32,
            m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32,
            m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32,
            m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32,
            m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33,
            m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33,
            m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33,
            m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33,
        };
        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs) => lhs.Equals(rhs);
        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs) => !(lhs == rhs);
    }
}
