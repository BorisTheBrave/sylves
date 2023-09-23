using UnityEngine;


namespace Sylves
{
    public static class VectorUtils
    {
        public static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static Vector3 Divide(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector4 ToVector4(Vector3 v) => new Vector4(v.x, v.y, v.z, 0);
        public static Vector4 ToVector4(Vector3 v, float w) => new Vector4(v.x, v.y, v.z, w);
        public static Vector3 ToVector3(Vector4 v) => new Vector3(v.x, v.y, v.z);
        public static Vector3 ToVector3(Vector2 v) => new Vector3(v.x, v.y, 0);
        public static Vector3 ToVector3(Vector2 v, float z) => new Vector3(v.x, v.y, z);
        public static Vector2 ToVector2(Vector3 v) => new Vector2(v.x, v.y);

        public static Matrix4x4 ToMatrix(Vector3 col1, Vector3 col2, Vector3 col3) => new Matrix4x4(ToVector4(col1), ToVector4(col2), ToVector4(col3), new Vector4(0, 0, 0, 1));
        public static Matrix4x4 ToMatrix(Vector3 col1, Vector3 col2, Vector3 col3, Vector4 col4) => new Matrix4x4(ToVector4(col1), ToVector4(col2), ToVector4(col3), col4);
        public static Matrix4x4 ToMatrix(Vector3 col1, Vector3 col2, Vector3 col3, Vector3 col4) => new Matrix4x4(ToVector4(col1), ToVector4(col2), ToVector4(col3), ToVector4(col4, 1));
    }
}
