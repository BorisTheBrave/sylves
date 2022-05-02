using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class MeshPrismGridTest
    {
        // Tolerance is kinda bad due to use of numerical differentiation
        // Should really fix that at some point
        private const float tol = 1e-3f;

        MeshPrismOptions options;
        public MeshPrismGridTest()
        {
            options = new MeshPrismOptions
            {
                LayerHeight = 1,
                MinLayer = 0,
                MaxLayer = 2,
            };
        }

        [Test]
        public void TestFindCell()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXY, options);
            GridTest.FindCell(g, new Cell(0, 0, 1));
        }

        [Test]
        public void TestPlaneXYGetTRS()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXY, options);

            var trs = g.GetTRS(new Cell());
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);
            AssertAreEqual(Vector3.right, trs.ToMatrix().MultiplyVector(Vector3.right), tol);
        }

        [Test]
        public void TestPlaneXYGetDeform()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXY, options);
            var d = g.GetDeformation(new Cell(0, 0, 0));
            // deform is identity
            AssertAreEqual(new Vector3(0, 0, 0), d.DeformPoint(new Vector3(0, 0, 0)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);
        }

        [Test]
        public void TestCubeMove()
        {
            var g = new MeshPrismGrid(TestMeshes.Cube, options);
            // TestMeshes.Cube is arranged with this convenience for testing
            Cell ToCell(CubeDir dir, int layer = 0) => new Cell((int)dir, 0, layer);

            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Left), (CellDir)CubeDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Right), (CellDir)CubeDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Forward), (CellDir)CubeDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Back), (CellDir)CubeDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Forward), g.Move(ToCell(CubeDir.Up), (CellDir)CubeDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Forward), g.Move(ToCell(CubeDir.Down), (CellDir)CubeDir.Up));
            // TODO: Also check some left right moves
        }



        [Test]
        public void TestCubeGetTRS()
        {
            var g = new MeshPrismGrid(TestMeshes.Cube, options);
            // TestMeshes.Cube is arranged with this convenience for testing
            Cell ToCell(CubeDir dir) => new Cell((int)dir, 0, 0);

            var trs = g.GetTRS(ToCell(CubeDir.Left));
            AssertAreEqual(Vector3.left * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.left, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Right));
            AssertAreEqual(Vector3.right * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.right, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Up));
            AssertAreEqual(Vector3.up * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Down));
            AssertAreEqual(Vector3.down * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.down, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Forward));
            AssertAreEqual(Vector3.forward * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Back));
            AssertAreEqual(Vector3.back * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.back, trs.ToMatrix().MultiplyVector(Vector3.forward), 1e-6);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);
        }
    }
}
