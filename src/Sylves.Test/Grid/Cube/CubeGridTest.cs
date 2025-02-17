using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class CubeGridTest
    {
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new CubeGrid(1);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = new CubeGrid(1);
            GridTest.FindCell(g, new Cell(1, 0, -1));
        }

        [Test]
        public void TestFindBasicPath()
        {
            var g = new CubeGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(10, 10, 10));
        }

        [Test]
        public void TestRaycast()
        {
            var g = new CubeGrid(1);
            var start = g.GetCellCenter(new Cell(0, 0, 0));
            var end = g.GetCellCenter(new Cell(2, 1, 0));
            var infos = g.Raycast(start, end-start, 1).ToList();
            Assert.AreEqual(new Cell(0, 0, 0), infos[0].cell);
            Assert.AreEqual(null, infos[0].cellDir);
            Assert.AreEqual(new Cell(1, 0, 0), infos[1].cell);
            Assert.AreEqual(CubeDir.Left, (CubeDir?)infos[1].cellDir);
            Assert.AreEqual(new Cell(1, 1, 0), infos[2].cell);
            Assert.AreEqual(CubeDir.Down, (CubeDir?)infos[2].cellDir);
            Assert.AreEqual(new Cell(2, 1, 0), infos[3].cell);
            Assert.AreEqual(CubeDir.Left, (CubeDir?)infos[3].cellDir);
            Assert.AreEqual(4, infos.Count);


            // Test bad direction doesn't break things
            g.Raycast(new Vector3(1.23f, 4.56f, 0), new Vector3(), 1).ToList();

            var bound = new CubeBound(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1));
            g = new CubeGrid(1, bound);

            // Test distance
            Assert.AreEqual(0.4f, g.Raycast(new Vector3(-0.4f, 0.5f, 0), Vector3.right).First().distance);

            // Test degenerate axes work ok
            // Particularly fiddly case is when the ray is co-incident witht he cube side
            Assert.AreEqual(0, g.Raycast(new Vector3(-0.5f, -0.5f, 0), Vector3.right).Count());
            Assert.AreEqual(1, g.Raycast(new Vector3(-0.5f, 0f, 0), Vector3.right).Count());
            Assert.AreEqual(1, g.Raycast(new Vector3(-0.5f, 0.5f, 0), Vector3.right).Count());
            Assert.AreEqual(0, g.Raycast(new Vector3(-0.5f, 1f, 0), Vector3.right).Count());
            Assert.AreEqual(0, g.Raycast(new Vector3(-0.5f, 1.5f, 0), Vector3.right).Count());
        }

        [Test]
        public void TestRaycast_Bounds()
        {
            var hits = CubeGrid.Raycast(new Vector3(0, 10, 0), new Vector3(0, -1, 0), float.PositiveInfinity, new Vector3(1, 1, 1), new CubeBound(new Vector3Int(-1, -1, -1), new Vector3Int(2, 2, 2))).ToList();
            CollectionAssert.AreEqual(new[] { new Cell(0, 1, 0), new Cell(0, 0, 0), new Cell(0, -1, 0) }, hits.Select(x => x.cell));

            hits = CubeGrid.Raycast(new Vector3(0, -10, 0), new Vector3(0, 1, 0), float.PositiveInfinity, new Vector3(1, 1, 1), new CubeBound(new Vector3Int(-1, -1, -1), new Vector3Int(2, 2, 2))).ToList();
            CollectionAssert.AreEqual(new[] { new Cell(0, -1, 0), new Cell(0, 0, 0), new Cell(0, 1, 0) }, hits.Select(x => x.cell));
        }

        [Test]
        public void TestGridSymmetry()
        {
            var g = new CubeGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0, 0),
                Dest = new Cell(10, 0, 0),
                Rotation = CubeRotation.RotateXY,
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0, 0), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 0, 0), dest);
            Assert.AreEqual(s.Rotation, r);

            success = g.TryApplySymmetry(s, new Cell(3, 0, 0), out dest, out r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 3, 0), dest);
            Assert.AreEqual(s.Rotation, r);
        }

        [Test]
        public void TryApplySymmetry()
        {
            var g = new CubeGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(),
                Dest = new Cell(),
                Rotation = CubeRotation.Identity,
            };
            var b = new CubeBound(Vector3Int.zero, new Vector3Int(6, 1, 7));
            var success = g.TryApplySymmetry(s, b, out var b2);
            Assert.IsTrue(success);
            Assert.AreEqual(new Vector3Int(6, 1, 7), ((CubeBound)b2).Mex);
        }

        [Test]
        public void TestFindGridSymmetry()
        {
            var g = new CubeGrid(1);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestDualMapping()
        {
            var g = new CubeGrid(1);
            var dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(0, 0, 0));
            GridTest.DualMapping(dual, new Cell(8, -4, 0));
        }

        [Test]
        public void TestTriangleMesh()
        {
            var g = new CubeGrid(1);
            GridTest.TestTriangleMesh(g, new Cell(), dir => ((CubeDir)dir).Forward(), _ => 2);
        }
    }
}
