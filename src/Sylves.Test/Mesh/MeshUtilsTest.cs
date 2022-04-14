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

            QuadInterpolation.GetCorners(plane, 0, 0, -.5f, .5f, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
            bool IsPointInCube(Vector3 p) => MeshUtils.IsPointInCube(p, v1, v2, v3, v4, v5, v6, v7, v8);

            Assert.IsTrue(IsPointInCube(new Vector3(0, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(1, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(-1, 0, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 1, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, -1, 0)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 0, 1)));
            Assert.IsFalse(IsPointInCube(new Vector3(0, 0, -1)));
        }
    }
}
