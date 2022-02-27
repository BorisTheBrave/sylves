using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public static class VectorUtils
    {
        public static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public static Vector3 Divide(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        public static Vector4 ToVector4(Vector3 v) => new Vector4(v.x, v.y, v.z, 0);
        public static Vector3 ToVector3(Vector4 v) => new Vector3(v.x, v.y, v.z);
    }
}
