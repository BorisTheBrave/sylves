using NUnit.Framework;
using System;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class PlanarPrismModifierTest
    {
        private PlanarPrismModifier GetGrid(int gridType)
        {
            switch (gridType)
            {
                case 0:
                    return new PlanarPrismModifier(new SquareGrid(1), new PlanarPrismOptions { }); ;
                case 1:
                    return new PlanarPrismModifier(
                        new BijectModifier(new TriangleGrid(1), TrianglePrismGrid.ToTriangleGrid, TrianglePrismGrid.FromTriangleGrid, 2),
                        new PlanarPrismOptions { });
                default:
                    throw new Exception();
            }
        }

        private static readonly int[] GridTypes = { 0, 1 };

        [Test]
        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(1, 1, 0)]
        public void TestTriangleRoundtrip(int x, int y, int z)
        {
            var c = new Cell(x, y, z);
            Assert.AreEqual(c, TrianglePrismGrid.FromTriangleGrid(TrianglePrismGrid.ToTriangleGrid(c)));
        }

        [Test]
        [TestCaseSource(nameof(GridTypes))]
        [Ignore("Never going to be supported?")]
        public void TestTryMoveByOffset(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestFindCell(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.FindCell(g, new Cell(0, 1, 10));
        }


        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestFindBasicPath(int gridType)
        {
            var g = GetGrid(gridType);
            GridTest.FindBasicPath(g, new Cell(1, 0, 0), new Cell(2, 0, -5));
        }


        [Test]
        [TestCaseSource(nameof(GridTypes))]
        public void TestDualMapping(int gridType)
        {
            var g = GetGrid(gridType);
            var dual = g.GetDual();
            GridTest.DualMapping(dual, new Cell(0, 0, 0));
        }

        [Test]
        public void TestDeform3d()
        {
            // Even if the mesh has no normals,
            // we want to do something sensible with the third dimension when extending to PlanarPrismModifier
            var normalessData = new MeshData
            {
                indices = TestMeshes.PlaneXY.indices,
                vertices = TestMeshes.PlaneXY.vertices,
                topologies = TestMeshes.PlaneXY.topologies,
            };
            var g = new PlanarPrismModifier(new RavelModifier(new MeshGrid(normalessData)), new PlanarPrismOptions { });
            var d = g.GetDeformation(new Cell());
            var j = d.GetJacobi(new Vector3());

            Assert.AreEqual(new Vector4(0, 0, 1, 0), j.column3);
        }

        [Test]
        public void TestIntersects()
        {
            var g = new PlanarPrismModifier(new SquareGrid(1), new PlanarPrismOptions { });
            CollectionAssert.AreEquivalent(new[] { new Cell() }, g.GetCellsIntersectsApprox(new Vector3(), new Vector3(.1f, .1f, .1f))); ;
        }


    }
}
