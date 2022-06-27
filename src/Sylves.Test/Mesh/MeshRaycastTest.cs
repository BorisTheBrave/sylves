using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif
namespace Sylves.Test
{
    [TestFixture]
    internal class MeshRaycastTest
    {
        [Test]
        public void TestRaycast()
        {
            var plane = TestMeshes.PlaneXZ;
            QuadInterpolation.GetCorners(plane, 0, 0, false, -.5f, .5f, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
            RaycastInfo? RaycastCube(Vector3 rayOrigin, Vector3 direction) =>
                MeshRaycast.RaycastCube(rayOrigin, direction, v1, v2, v3, v4, v5, v6, v7, v8);

            void Test(Vector3 rayOrigin, CubeDir expectedCellDir)
            {
                var info = RaycastCube(rayOrigin, -rayOrigin);
                Assert.AreEqual(expectedCellDir, (CubeDir?)info?.cellDir);
            }

            Test(new Vector3(1, 0, 0), CubeDir.Right);
            Test(new Vector3(-1, 0, 0), CubeDir.Left);
            Test(new Vector3(0, 1, 0), CubeDir.Up);
            Test(new Vector3(0, -1, 0), CubeDir.Down);
            Test(new Vector3(0, 0, 1), CubeDir.Forward);
            Test(new Vector3(0, 0, -1), CubeDir.Back);
        }
    }
}
