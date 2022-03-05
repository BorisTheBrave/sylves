using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    public static class TestUtils
    {
        public static void AssertAreEqual(Quaternion q1, Quaternion q2, double delta)
        {
            Assert.AreEqual(q1.x, q2.x, delta, $"Comparing {q1} to {q2}");
            Assert.AreEqual(q1.y, q2.y, delta, $"Comparing {q1} to {q2}");
            Assert.AreEqual(q1.z, q2.z, delta, $"Comparing {q1} to {q2}");
            Assert.AreEqual(q1.w, q2.w, delta, $"Comparing {q1} to {q2}");
        }

        public static void AssertAreEqual(Vector3 v1, Vector3 v2, double delta)
        {
            Assert.AreEqual(v1.x, v2.x, delta, $"Comparing {v1} to {v2}");
            Assert.AreEqual(v1.y, v2.y, delta, $"Comparing {v1} to {v2}");
            Assert.AreEqual(v1.z, v2.z, delta, $"Comparing {v1} to {v2}");
        }

        public static void AssertAreEqual(Matrix4x4 m1, Matrix4x4 m2, double delta, string message = "")
        {
            Assert.AreEqual(m1.m00, m2.m00, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m01, m2.m01, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m02, m2.m02, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m03, m2.m03, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m10, m2.m10, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m11, m2.m11, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m12, m2.m12, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m13, m2.m13, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m20, m2.m20, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m21, m2.m21, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m22, m2.m22, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m23, m2.m23, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m30, m2.m30, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m31, m2.m31, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m32, m2.m32, delta, $"Comparing {m1} to {m2} {message}");
            Assert.AreEqual(m1.m33, m2.m33, delta, $"Comparing {m1} to {m2} {message}");
        }
    }
}
