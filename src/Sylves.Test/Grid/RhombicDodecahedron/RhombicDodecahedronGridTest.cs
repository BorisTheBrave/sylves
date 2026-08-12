using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class RhombicDodecahedronGridTest
    {
        [Test]
        public void TestIsCellInGrid()
        {
            var g = new RhombicDodecahedronGrid(1);
            Assert.IsTrue(g.IsCellInGrid(new Cell(0, 0, 0)));
            Assert.IsFalse(g.IsCellInGrid(new Cell(1, 0, 0)));
            Assert.IsTrue(g.IsCellInGrid(new Cell(1, 1, 0)));
            Assert.IsTrue(g.IsCellInGrid(new Cell(1, 0, 1)));
            Assert.IsFalse(g.IsCellInGrid(new Cell(1, 1, 1)));

            var neighbours = g.GetCellDirs(new Cell()).Select(dir => new Cell() + ((RhombicDodecahedronDir)dir).Forward()).ToList();
            Assert.AreEqual(12, neighbours.Count);
            Assert.IsTrue(neighbours.All(g.IsCellInGrid));
            CollectionAssert.AllItemsAreUnique(neighbours);
        }

        [Test]
        public void TestTryMove()
        {
            var g = new RhombicDodecahedronGrid(1);
            var cell = new Cell(0, 0, 0);
            foreach (var dir in g.GetCellDirs(cell))
            {
                var rdDir = (RhombicDodecahedronDir)dir;
                Assert.IsTrue(g.TryMove(cell, dir, out var dest, out var inverseDir, out var connection));
                Assert.AreEqual(cell + rdDir.Forward(), dest);
                Assert.AreEqual((CellDir)rdDir.Inverted(), inverseDir);
                Assert.IsTrue(g.TryMove(dest, inverseDir, out var back, out var _, out var _));
                Assert.AreEqual(cell, back);
            }
        }

        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new RhombicDodecahedronGrid(1);
            GridTest.TryMoveByOffset(g, new Cell());
        }

        [Test]
        public void TestFindCell()
        {
            var g = new RhombicDodecahedronGrid(1);
            GridTest.FindCell(g, new Cell(0, 0, 0));
            GridTest.FindCell(g, new Cell(1, 1, 0));
            GridTest.FindCell(g, new Cell(1, 0, -1));
            GridTest.FindCell(g, new Cell(2, 0, 0));
        }

        [Test]
        public void TestFindCell_OddCube()
        {
            var g = new RhombicDodecahedronGrid(1);
            // Odd cube (1,0,0) occupies [1,2]x[0,1]x[0,1], center (1.5, 0.5, 0.5).
            // Points near each face belong to the adjacent even cell.
            Assert.IsTrue(g.FindCell(new Vector3(1.1f, 0.5f, 0.5f), out var cell));
            Assert.AreEqual(new Cell(0, 0, 0), cell);
            Assert.IsTrue(g.FindCell(new Vector3(1.9f, 0.5f, 0.5f), out cell));
            Assert.AreEqual(new Cell(2, 0, 0), cell);
            Assert.IsTrue(g.FindCell(new Vector3(1.5f, 0.9f, 0.5f), out cell));
            Assert.AreEqual(new Cell(1, 1, 0), cell);
            Assert.IsTrue(g.FindCell(new Vector3(1.5f, 0.1f, 0.5f), out cell));
            Assert.AreEqual(new Cell(1, -1, 0), cell);
            Assert.IsTrue(g.FindCell(new Vector3(1.5f, 0.5f, 0.9f), out cell));
            Assert.AreEqual(new Cell(1, 0, 1), cell);
            Assert.IsTrue(g.FindCell(new Vector3(1.5f, 0.5f, 0.1f), out cell));
            Assert.AreEqual(new Cell(1, 0, -1), cell);
        }

        [Test]
        public void TestFindBasicPath()
        {
            var g = new RhombicDodecahedronGrid(1);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(2, 0, 0));
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(1, 1, 0));
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(4, 2, -2));
            GridTest.FindBasicPath(g, new Cell(1, 0, -1), new Cell(10, -4, -6));
        }

        [Test]
        public void TestGetCellCenter()
        {
            var rd = new RhombicDodecahedronGrid(1);
            var cube = new CubeGrid(1);
            foreach (var cell in new[] { new Cell(0, 0, 0), new Cell(1, 1, 0), new Cell(2, -1, 1) })
            {
                TestUtils.AssertAreEqual(cube.GetCellCenter(cell), rd.GetCellCenter(cell), 1e-6);
            }
        }

        [Test]
        public void TestBounds()
        {
            var bound = new CubeBound(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2));
            var g = new RhombicDodecahedronGrid(1, bound);
            var cells = g.GetCells().ToList();
            CollectionAssert.AreEquivalent(new[]
            {
                new Cell(0, 0, 0),
                new Cell(0, 1, 1),
                new Cell(1, 0, 1),
                new Cell(1, 1, 0),
            }, cells);
            Assert.AreEqual(4, g.IndexCount);
            foreach (var cell in cells)
            {
                Assert.AreEqual(cell, g.GetCellByIndex(g.GetIndex(cell)), $"Round trip failed for {cell}");
            }

            Assert.IsTrue(g.IsCellInGrid(new Cell(0, 0, 0)));
            Assert.IsFalse(g.IsCellInGrid(new Cell(2, 0, 0)));
            Assert.IsFalse(g.IsCellInGrid(new Cell(1, 0, 0)));
        }

        [Test]
        public void TestGridSymmetry()
        {
            var g = new RhombicDodecahedronGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(0, 0, 0),
                Dest = new Cell(10, 0, 0),
                Rotation = RhombicDodecahedronRotation.RotateXY,
            };
            var success = g.TryApplySymmetry(s, new Cell(0, 0, 0), out var dest, out var r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 0, 0), dest);
            Assert.AreEqual(s.Rotation, r);

            success = g.TryApplySymmetry(s, new Cell(2, 0, 0), out dest, out r);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(10, 2, 0), dest);
            Assert.AreEqual(s.Rotation, r);
        }

        [Test]
        public void TryApplySymmetry()
        {
            var g = new RhombicDodecahedronGrid(1);
            var s = new GridSymmetry
            {
                Src = new Cell(),
                Dest = new Cell(),
                Rotation = RhombicDodecahedronRotation.Identity,
            };
            var b = new CubeBound(Vector3Int.zero, new Vector3Int(6, 1, 7));
            var success = g.TryApplySymmetry(s, b, out var b2);
            Assert.IsTrue(success);
            Assert.AreEqual(new Vector3Int(6, 1, 7), ((CubeBound)b2).Mex);
        }

        [Test]
        public void TestFindGridSymmetry()
        {
            var g = new RhombicDodecahedronGrid(1);
            GridTest.FindGridSymmetry(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestMeshData()
        {
            var g = new RhombicDodecahedronGrid(1);
            g.GetMeshData(new Cell(), out var meshData, out var transform);
            Assert.AreEqual(14, meshData.vertices.Length);
            Assert.AreEqual(12 * 4, meshData.indices[0].Length);

            var cellType = RhombicDodecahedronCellType.Instance;
            foreach (var corner in cellType.GetCellCorners())
            {
                TestUtils.AssertAreEqual(cellType.GetCornerPosition(corner), meshData.vertices[(int)corner], 1e-6, corner.ToString());
            }

            var quads = meshData.indices[0];
            foreach (var dir in cellType.GetCellDirs())
            {
                var i = (int)dir;
                var v0 = meshData.vertices[quads[i * 4 + 0]];
                var v1 = meshData.vertices[quads[i * 4 + 1]];
                var v2 = meshData.vertices[quads[i * 4 + 2]];
                var n = Vector3.Cross(v1 - v0, v2 - v1);
                var expected = (Vector3)((RhombicDodecahedronDir)dir).Forward();
                Assert.Greater(Vector3.Dot(n, expected), 0, cellType.Format(dir));
            }
        }

        [Test]
        public void TestTriangleMesh()
        {
            var g = new RhombicDodecahedronGrid(1);
            GridTest.TestTriangleMesh(g, new Cell(), dir => ((RhombicDodecahedronDir)dir).Forward(), _ => 2);
        }
    }
}
