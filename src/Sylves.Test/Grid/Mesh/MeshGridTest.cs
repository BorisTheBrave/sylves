using NUnit.Framework;
using System.Linq;
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


            g = new MeshGrid(TestMeshes.PlaneXY, new MeshGridOptions { InvertWinding = true });
            d = g.GetDeformation(new Cell(0, 0, 0));
            // Mirrored in X (see notes on InvertWinding)
            AssertAreEqual(new Vector3(0, 0, 0), d.DeformPoint(new Vector3(0, 0, 0)), 1e-6);
            AssertAreEqual(new Vector3(0, 0.5f, 0), d.DeformPoint(new Vector3(0, 0.5f, 0)), 1e-6);
            AssertAreEqual(new Vector3(-0.5f, 0, 0), d.DeformPoint(new Vector3(0.5f, 0, 0)), 1e-6);

        }

        [Test]
        public void TestCubeMove()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            // TestMeshes.Cube is arranged with this convenience for testing
            Cell ToCell(CubeDir dir) => new Cell((int)dir, 0, 0);

            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Left), (CellDir)SquareDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Right), (CellDir)SquareDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Forward), (CellDir)SquareDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Up), g.Move(ToCell(CubeDir.Back), (CellDir)SquareDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Forward), g.Move(ToCell(CubeDir.Up), (CellDir)SquareDir.Up));
            Assert.AreEqual(ToCell(CubeDir.Forward), g.Move(ToCell(CubeDir.Down), (CellDir)SquareDir.Up));
            // TODO: Also check some left right moves
        }

        [Test]
        public void TestCubeGetTRS()
        {
            var g = new MeshGrid(TestMeshes.Cube);
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


        [Test]
        public void TestCubeNoNormalGetDeform()
        {
            var meshData = new MeshData
            {
                indices = TestMeshes.Cube.indices,
                vertices = TestMeshes.Cube.vertices,
                topologies = TestMeshes.Cube.topologies,
            };
            var g = new MeshGrid(meshData);
            // TestMeshes.Cube is arranged with this convenience for testing
            Cell ToCell(CubeDir dir) => new Cell((int)dir, 0, 0);

            var deformation = g.GetDeformation(ToCell(CubeDir.Left));
            var expectedCenter = Vector3.left * 0.5f;
            // zero gives the face center
            AssertAreEqual(expectedCenter, deformation.DeformPoint(Vector3.zero), 1e-6);
            // Without a normal, moving forward does nothing for 2d mesh grid
            AssertAreEqual(expectedCenter, deformation.DeformPoint(Vector3.forward * 0.5f), 1e-6);
            // An actual deformation
            AssertAreEqual(expectedCenter + Vector3.up * 0.5f, deformation.DeformPoint(Vector3.up * 0.5f), 1e-6);

            // We can probably trust the TRS points from here
        }

        [Test]
        public void TestCubeGetDeform()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            // TestMeshes.Cube is arranged with this convenience for testing
            Cell ToCell(CubeDir dir) => new Cell((int)dir, 0, 0);

            var deformation = g.GetDeformation(ToCell(CubeDir.Left));
            var expectedCenter = Vector3.left * 0.5f;
            // zero gives the face center
            AssertAreEqual(expectedCenter, deformation.DeformPoint(Vector3.zero), 1e-6);
            // Without a normal, moving forward does nothing for 2d mesh grid
            AssertAreEqual(Vector3.left * 0.7886751f, deformation.DeformPoint(Vector3.forward * 0.5f), 1e-6);
            // An actual deformation
            AssertAreEqual(expectedCenter + Vector3.up * 0.5f, deformation.DeformPoint(Vector3.up * 0.5f), 1e-6);

            // We can probably trust the TRS points from here
        }

        [Test]
        public void TestRaycast_Planar()
        {
            var g = new MeshGrid(TestMeshes.PlaneXY);
            var results = g.Raycast(new Vector3(-0.6f, 0.1f, 0), Vector3.right);
            var raycastInfo = results.Single();
            Assert.AreEqual(new Cell(), raycastInfo.cell);
            Assert.AreEqual(SquareDir.Left, (SquareDir?)raycastInfo.cellDir);
            Assert.AreEqual(0.1f, raycastInfo.distance, 1e-6);
        }

        [Test]
        public void TestRaycast_Planar_Glancing()
        {
            var g = new MeshGrid(TestMeshes.PlaneXY);
            var results = g.Raycast(new Vector3(-0.6f, -0.1f, 0), Vector3.right);
            var raycastInfo = results.Single();
            Assert.AreEqual(new Cell(), raycastInfo.cell);
            Assert.AreEqual(SquareDir.Left, (SquareDir?)raycastInfo.cellDir);
            Assert.AreEqual(0.1f, raycastInfo.distance, 1e-6);
        }


        [Test]
        public void TestRaycast_Planar_StartsInside()
        {
            var g = new MeshGrid(ConwayOperators.Ortho(TestMeshes.PlaneXY));
            var results = g.Raycast(new Vector3(-0.25f, -0.25f, 0), Vector3.right);
            var raycastInfo = results.ToList();
            Assert.AreEqual(2, raycastInfo.Count);
            //Assert.AreEqual(new Cell(), raycastInfo[0].cell);
            //Assert.AreEqual(SquareDir.Left, (SquareDir?)raycastInfo.cellDir);
            //Assert.AreEqual(0.1f, raycastInfo.distance, 1e-6);
        }

        [Test]
        public void TestRaycast_Planar_Miss()
        {
            var g = new MeshGrid(ConwayOperators.Ortho(TestMeshes.PlaneXY));
            var results = g.Raycast(new Vector3(-0.25f, -0.25f, 1), Vector3.right);
            var raycastInfo = results.ToList();
            Assert.AreEqual(0, raycastInfo.Count);
        }

        [Test]
        public void TestRaycast_Planar_Perp()
        {
            var g = new MeshGrid(ConwayOperators.Ortho(TestMeshes.PlaneXY));
            var results = g.Raycast(new Vector3(-0.25f, -0.25f, 1), Vector3.back);
            var raycastInfo = results.ToList();
            Assert.AreEqual(1, raycastInfo.Count);
        }

        [Test]
        public void TestRaycast()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            var results = g.Raycast(new Vector3(-1.6f, -0.1f, 0), Vector3.right);
            var raycastInfo = results.First();
            Assert.AreEqual(new Cell((int)(CubeDir.Left), 0, 0), raycastInfo.cell);
            Assert.AreEqual(null, raycastInfo.cellDir);
            Assert.AreEqual(1.1f, raycastInfo.distance, 1e-6);
        }


        [Test]
        public void TestRaycast_Flat()
        {
            // XZ plane has 0 height, but it's not considered planar (XY only)
            var g = new MeshGrid(Matrix4x4.Translate(Vector3.up) * TestMeshes.PlaneXZ);
            var results = g.Raycast(new Vector3(0.1f, 2f, 0.1f), Vector3.down);
            var raycastInfo = results.First();
            Assert.AreEqual(new Cell(0, 0, 0), raycastInfo.cell);
            Assert.AreEqual(null, raycastInfo.cellDir);
            Assert.AreEqual(1.0f, raycastInfo.distance, 1e-6);
        }

        [Test]
        public void TestHex()
        {
            var h = new HexGrid(1, bound: HexBound.Hexagon(3));
            var meshData = h.ToMeshData();
            var g = new MeshGrid(meshData);

            GridTest.FindCell(g, new Cell());
        }

        // Test precision issues far from the origin
        [Test]
        public void TestFar()
        {
            var g = new UnrelaxedTownscaperGrid(4, 1e-2f).BoundBy(new SquareBound(38, -1, 39, 0));
            foreach (var c in g.GetCells())
            {
                Assert.IsFalse(float.IsNaN(g.GetCellCenter(c).x), c.ToString());
            }

        }


        [Test]
        public void TestDualMapping()
        {
            var g = new MeshGrid(TestMeshes.Cube);
            var dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(0, 0, 0), checkPositions: false);
        }
    }
}
