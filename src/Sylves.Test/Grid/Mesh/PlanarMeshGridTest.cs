using NUnit.Framework;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class PlanarMeshGridTest
    {
        // Tolerance is kinda bad due to use of numerical differentiation
        // Should really fix that at some point
        private const float tol = 1e-3f;


        [Test]
        public void TestFindCell()
        {
            var g = new PlanarMeshGrid(TestMeshes.PlaneXY);
            GridTest.FindCell(g, new Cell());
        }

        [Test]
        public void TestFindCell_Concave()
        {
            // Dart pointing upwards.
            var dartMesh = new MeshData
            {
                vertices = new[]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0.5f, 0.5f, 0f),
                    new Vector3(1f, 0f, 0f),
                    new Vector3(0.5f, 1f, 0f),
                },
                normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward },
                indices = new[] { new[] { 0, 1, 2, 3 } },
                topologies = new[] { MeshTopology.Quads }
            };
            var g = new PlanarMeshGrid(dartMesh);
            var dartCell = new Cell(0, 0, 0);

            // Point inside the dart (e.g. left of the notch) should find the cell
            Assert.IsTrue(g.FindCell(new Vector3(0.5f, 0.75f, 0f), out var found), "Point inside dart should find cell");
            Assert.AreEqual(dartCell, found);

            // Point in the notch (in hull but not in polygon) must NOT find the cell
            var pointInHullNotInPolygon = new Vector3(0.5f, 0.25f, 0f);
            bool foundInNotch = g.FindCell(pointInHullNotInPolygon, out _);
            Assert.IsFalse(foundInNotch, "Point in convex hull but outside concave polygon (dart notch) must not be found by FindCell");
        }

        
        [Test]
        public void TestGetCellsIntersectsApprox()
        {
            var g = new PlanarMeshGrid(TestMeshes.PlaneXY);
            var cells = g.GetCellsIntersectsApprox(new Vector3(0.0f, 0.0f, 1), new Vector3(2.0f, 0.1f, 1));
            CollectionAssert.AreEquivalent(new[]
            {
                new Cell(0, 0, 0),
            },
                cells);
        }


    }
}
