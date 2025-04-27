using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class MeshUtilsTest
    {
        [Test]
        public void TestIsPointInCube()
        {
            var plane = TestMeshes.PlaneXY;

            QuadInterpolation.GetCorners(plane, 0, 0, false, -.5f, .5f, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
            bool IsPointInCube(Vector3 p) => MeshUtils.IsPointInCube(p, v1, v2, v3, v4, v5, v6, v7, v8);

            Assert.IsTrue(IsPointInCube(new Vector3(0, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(1, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(-1, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 1, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, -1, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 0, 1)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 0, -1)));
        }

        [Test]
        public void TestGetNormalDirection()
        {
            var v0 = new Vector3(0.0f, 0.0f, 0.0f);
            var v1 = new Vector3(1.0f, 0.0f, 0.0f);
            var v2 = new Vector3(0.0f, 1.0f, 0.0f);

            var normal = MeshUtils.GetNormalDirection(v0, v1, v2);

            Assert.AreEqual(0.0f, normal.x, 1e-6f);
            Assert.AreEqual(0.0f, normal.y, 1e-6f);
            Assert.AreEqual(1.0f, normal.z, 1e-6f);

            v0 = new Vector3(0.0f, 0.0f, 0.0f);
            v1 = new Vector3(1.0f, 0.0f, 0.0f);
            v2 = new Vector3(0.0f, 0.0f, 1.0f);

            normal = MeshUtils.GetNormalDirection(v0, v1, v2);

            Assert.AreEqual(0.0f, normal.x, 1e-6f);
            Assert.AreEqual(-1.0f, normal.y, 1e-6f);
            Assert.AreEqual(0.0f, normal.z, 1e-6f);
        }
    }
}
