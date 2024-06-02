﻿using NUnit.Framework;
using System.IO;
using System;
using System.Linq;
using System.Net;
using static Sylves.Test.GridDocsExportTest;
using static Sylves.Test.GridDocsExportTest;



#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;


namespace Sylves.Test
{
    [TestFixture]
    internal class PeriodicPlanarMeshGridTest
    {
        [Test]
        public void TestPeriodicPlanarMeshGrid()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            AssertAreEqual(Vector3.zero, g.GetCellCenter(new Cell(0, 0, 0)), 1e-6);
            AssertAreEqual(Vector3.right, g.GetCellCenter(new Cell(0, 1, 0)), 1e-6);
            AssertAreEqual(Vector3.up, g.GetCellCenter(new Cell(0, 0, 1)), 1e-6);

            Assert.AreEqual(new Cell(0, 1, 0), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Right));
            Assert.AreEqual(new Cell(0, 0, 1), g.Move(new Cell(0, 0, 0), (CellDir)SquareDir.Up));
        }

        [Test]
        [Ignore("Not supported yet")]
        public void TestTryMoveByOffset()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.TryMoveByOffset(g, new Cell());
        }

        [Test]
        public void TestFindCell()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.FindCell(g, new Cell(0, 2, 3));
        }

        [Test]
        public void TestFindCell_TriHex()
        {
            var g = new TriHexGrid();
            GridTest.FindCell(g, new Cell(0, 0, 0));
            GridTest.FindCell(g, new Cell(0, 10, 10));
            GridTest.FindCell(g, new Cell(1, 10, 10));
            GridTest.FindCell(g, new Cell(2, 10, 10));
        }
        [Test]
        public void TestFindCell_Tetrakis()
        {
            var g = new TetrakisSquareGrid();
            GridTest.FindCell(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestFindCell_Cairo()
        {
            var g = new CairoGrid();
            GridTest.FindCell(g, new Cell(0, 0, 0));
        }

        [Test]
        public void TestFindCell_MetaHexagon()
        {
            var g = new MetaHexagonGrid();
            var h = new HexGrid(1);
            System.Console.WriteLine(h.GetCellCenter(new Cell(-1, 1, 0)).ToString());
            System.Console.WriteLine(h.GetCellCenter(new Cell(-1, 0, 1)).ToString());
            System.Console.WriteLine(h.GetCellCenter(new Cell(-20, 10, 10)).ToString());
            var success = h.FindCell(g.GetCellCenter(new Cell(0, 10, 10)), out var hex);
            Assert.IsTrue(success);
            Assert.AreEqual(new Cell(-20, 10, 10), hex);

        }

        [Test]
        [Ignore("Not supported yet")]
        public void TestFindBasicPath()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            GridTest.FindBasicPath(g, new Cell(0, 0, 0), new Cell(0, 10, 10));
        }

        [Test]
        public void TestGetCellsIntersectsApprox()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            var cells = g.GetCellsIntersectsApprox(new Vector3(0.0f, 0.0f, 0), new Vector3(2.0f, 0.1f, 0));
            CollectionAssert.AreEquivalent(new[]
            {
                new Cell(0, 0, 0),
                new Cell(0, 1, 0),
                new Cell(0, 2, 0),
            },
                cells);
        }
        [Test]
        public void TestGetCellsIntersectsApprox_TriHex()
        {
            var g = new TriHexGrid();
            var cells = g.GetCellsIntersectsApprox(new Vector3(0.0f, 0.0f, 0), new Vector3(2.0f, 0.1f, 0));
            Assert.IsTrue(cells.ToList().Count > 0);
        }

        [Test]
        public void TestGetCellsIntersectsApprox_SquareSnub()
        {
            var g = new SquareSnubGrid();
            var cells = g.GetCellsIntersectsApprox(new Vector3(-10, -10, 0), new Vector3(10, 10, 0)).ToList();
            Assert.IsTrue(cells.Count > 0);
        }

        [Test]
        public void TestRaycast()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            var start = new Vector3(3.088772f, -0.9270384f, 0);
            var end = new Vector3(2.384936f, 0.2460213f, 0);
            var results = g.Raycast(start, end - start, 1);
            Assert.AreEqual(3, results.Count());
        }


        [Test]
        public void TestRaycast2()
        {
            var meshData = new MeshData
            {
                indices = new[] { new[] { 0, 1, 2, 1, 0, 3 } },
                vertices = new Vector3[]
                {
                        new Vector3(0, -0.5f, 0),
                        new Vector3(0, 0.5f, 0),
                        new Vector3(-Mathf.Sqrt(3) / 2, 0, 0),
                        new Vector3(Mathf.Sqrt(3) / 2, 0, 0),
                },
                topologies = new[] { MeshTopology.Triangles },
            };
            var g = new PeriodicPlanarMeshGrid(meshData, new Vector2(Mathf.Sqrt(3) / 2, 0.5f), new Vector2(0, 1))
            .Transformed(Matrix4x4.Scale(Vector3.one * Mathf.Sqrt(3)));


            var start = new Vector3(2.78125f, -2.640625f, 0);
            var dir = new Vector3(5, 0, 0);

            /*
            var fullPath = Path.GetFullPath("test_tri_grid.svg");
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                var g2 = g.Masked(g.GetCellsIntersectsApprox(start - Vector3.one, start + dir + Vector3.one).ToHashSet());
                WriteGrid(g2, writer, new Options());
                Console.WriteLine($"Wrote file {fullPath}");
            }
            */


            var results = g.Raycast(start, dir, 1);
            var cells = results.Select(x => x.cell).ToList();
            CollectionAssert.AreEqual(new[]
            {
                new Cell(1, 1, -2),
                new Cell(0, 2, -3),
                new Cell(1, 2, -3),
                new Cell(0, 3, -3),
                new Cell(1, 3, -3),
                new Cell(0, 4, -4),
                new Cell(1, 4, -4),
                new Cell(0, 5, -4),
                new Cell(1, 5, -4),
            }, cells);
        }

        [Test]
        public void TestDual()
        {
            var g = new PeriodicPlanarMeshGrid(TestMeshes.PlaneXY, Vector2.right, Vector2.up);
            var dm = g.GetDual();
            GridTest.DualMapping(dm, new Cell(0, 0, 0));

            Assert.AreEqual(4, dm.DualNeighbours(new Cell(0, 0, 0)).Count());
            Assert.AreEqual(4, dm.BaseNeighbours(new Cell(0, 0, 0)).Count());
        }

        [Test]
        public void TestDual2()
        {
            var meshData = new MeshData
            {
                indices = new[] { new[] { 0, 1, 2, 1, 0, 3 } },
                vertices = new Vector3[]
                {
                    new Vector3(0, -0.5f, 0),
                    new Vector3(0, 0.5f, 0),
                    new Vector3(-Mathf.Sqrt(3) / 2, 0, 0),
                    new Vector3(Mathf.Sqrt(3) / 2, 0, 0),
                },
                topologies = new[] { MeshTopology.Triangles },
            };

            var g = new PeriodicPlanarMeshGrid(meshData, new Vector2(Mathf.Sqrt(3) / 2, 0.5f), new Vector2(0, 1));
            var dm = g.GetDual();
            var dg = (PeriodicPlanarMeshGrid)(dm.DualGrid);
            Assert.AreEqual(1, dg.BoundBy(new SquareBound(0, 0, 1, 1)).GetCells().Count());
            Assert.AreEqual(6, dg.GetCellCorners(new Cell(0, 0, 0)).Count());
        }
    }
}
