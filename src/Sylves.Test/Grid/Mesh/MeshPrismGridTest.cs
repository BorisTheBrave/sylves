using NUnit.Framework;
using System.Linq;
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

        MeshPrismGridOptions options;
        MeshPrismGridOptions xzOptions;
        public MeshPrismGridTest()
        {
            options = new MeshPrismGridOptions
            {
                LayerHeight = 1,
                MinLayer = 0,
                MaxLayer = 2,
            };
            xzOptions = new MeshPrismGridOptions
            {
                LayerHeight = 1,
                MinLayer = 0,
                MaxLayer = 2,
                UseXZPlane = true,
            };
        }
        #region PlaneXY
        [Test]
        public void TestPlaneXYFindCell()
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
            AssertAreEqual(new Vector3(0, 0, 0.5f), d.DeformPoint(new Vector3(0, 0, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);
        }

        [Test]
        public void TestPlaneXYRaycast()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXY, options);
            var hits = g.Raycast(new Vector3(0, 0, -10), Vector3.forward).ToList();
            Assert.AreEqual(2, hits.Count);
            Assert.AreEqual(new Cell(0, 0, 0), hits[0].cell);
            Assert.AreEqual(new Cell(0, 0, 1), hits[1].cell);
        }
        #endregion

        #region Cube
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

            // Because the normals point outwards, the tiles are less high than you'd naturally expect
            float f = Mathf.Sqrt(3) / 3;

            var trs = g.GetTRS(ToCell(CubeDir.Left));
            AssertAreEqual(Vector3.left * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.left * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Right));
            AssertAreEqual(Vector3.right * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.right * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Up));
            AssertAreEqual(Vector3.up * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.up * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Down));
            AssertAreEqual(Vector3.down * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.down * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Forward));
            AssertAreEqual(Vector3.forward * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.forward * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);

            trs = g.GetTRS(ToCell(CubeDir.Back));
            AssertAreEqual(Vector3.back * 0.5f, trs.Position, 1e-6);
            AssertAreEqual(Vector3.back * f, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);
        }
        #endregion

        #region UseXZPlane Option

        [Test]
        public void TestXZPlaneXZFindCell()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXZ, xzOptions);
            GridTest.FindCell(g, new Cell(0, 0, 1));
        }

        [Test]
        public void TestXZPlaneXZMove()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXZ, xzOptions);
            // Moving up should increase the layer
            Assert.AreEqual(new Cell(0, 0, 1), g.Move(new Cell(0, 0, 0), (CellDir)CubeDir.Up));
        }

        [Test]
        public void TestXZPlaneXZGetTRS()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXZ, xzOptions);

            var trs = g.GetTRS(new Cell());
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);
            AssertAreEqual(Vector3.right, trs.ToMatrix().MultiplyVector(Vector3.right), tol);
        }

        [Test]
        public void TestXZPlaneXZGetDeform()
        {
            var g = new MeshPrismGrid(TestMeshes.PlaneXZ, xzOptions);
            var d = g.GetDeformation(new Cell(0, 0, 0));
            // deform is identity
            AssertAreEqual(new Vector3(0, 0, 0), d.DeformPoint(new Vector3(0, 0, 0)), 1e-6);
            AssertAreEqual(new Vector3(0, 0, 0.5f), d.DeformPoint(new Vector3(0, 0, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);
        }
        #endregion



        #region UseXZPlane Option

        [Test]
        [Ignore("TODO")]
        public void TestEquilateralFindCell()
        {
            var g = new MeshPrismGrid(TestMeshes.Equilateral, options);
            GridTest.FindCell(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestEquilateralGetTRS()
        {
            var g = new MeshPrismGrid(TestMeshes.Equilateral, options);

            var trs = g.GetTRS(new Cell());
            AssertAreEqual(Vector3.forward, trs.ToMatrix().MultiplyVector(Vector3.forward), tol);
            AssertAreEqual(Vector3.up, trs.ToMatrix().MultiplyVector(Vector3.up), tol);
            AssertAreEqual(Vector3.right, trs.ToMatrix().MultiplyVector(Vector3.right), tol);
        }

        [Test]
        public void TestEquilateralGetDeform()
        {
            var g = new MeshPrismGrid(TestMeshes.Equilateral, options);
            var d = g.GetDeformation(new Cell(0, 0, 0));
            // deform is identity
            AssertAreEqual(new Vector3(0, 0, 0), d.DeformPoint(new Vector3(0, 0, 0)), 1e-6);
            AssertAreEqual(new Vector3(0, 0, 0.5f), d.DeformPoint(new Vector3(0, 0, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);
        }
        #endregion



        [Test]
        public void TestTriangle()
        {

            var grid = new MeshPrismGrid(TestMeshes.TrianglePlane, new Sylves.MeshPrismGridOptions
            {
                MinLayer = 0,
                MaxLayer = 1,
                LayerHeight = 1,
                LayerOffset = 0,
                //UseXZPlane = true,
            });

            Assert.IsNotNull(grid);
        }


        [Test]
        public void TestHex()
        {
            var h = new HexGrid(1, bound: HexBound.Hexagon(3));
            var meshData = h.ToMeshData();
            var g = new MeshPrismGrid(meshData, new MeshPrismGridOptions());

            GridTest.FindCell(g, new Cell());
        }
    }
}
