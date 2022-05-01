using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class MeshGridTest
    {
        // Tolerance is kinda bad due to use of numerical differentiation
        // Should really fix that at some point
        private const float tol = 1e-3f;

        public MeshGridTest()
        {
        }

        [Ignore("Never going to be supported?")]
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            GridTest.FindCell(g, new Cell());
        }

        [Test]
        public void TestPlaneXYGetTRS()
        {
            var g = new MeshGrid(TestMeshes.PlaneXY);
            var trs = g.GetTRS(new Cell(0, 0, 0));
            // should be identity
            Assert.AreEqual(Vector3.zero, trs.Position);
            AssertAreEqual(Quaternion.identity, trs.Rotation, 1e-6);
            AssertAreEqual(Vector3.one, trs.Scale, tol);
        }

        [Test]
        public void TestPlaneXYGetDeform()
        {
            var g = new MeshGrid(TestMeshes.PlaneXY);
            var d = g.GetDeformation(new Cell(0, 0, 0));
            // deform is identity
            AssertAreEqual(new Vector3(0, 0, 0), d.DeformPoint(new Vector3(0, 0, 0)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);

        }


        [Test]
        public void TestCubeGetTRS()
        {
            // quad 0 points z-
            var g = new MeshGrid(TestMeshes.Cube);
            var trs = g.GetTRS(new Cell());
            var v = trs.ToMatrix().MultiplyVector(Vector3.forward);
            AssertAreEqual(Vector3.back, v, 1e-6);
        }


        [Test]
        public void TestCubeGetDeform()
        {
            // quad 0 points z-
            var g = new MeshGrid(TestMeshes.Cube);
            var deform = g.GetDeformation(new Cell());
        }
    }
}
