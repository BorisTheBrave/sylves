using NUnit.Framework;
using System;
using System.Collections.Generic;
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

            Assert.AreEqual(new Vector4(1, 0, 0, 0), j.column0);
            Assert.AreEqual(new Vector4(0, 1, 0, 0), j.column1);
            Assert.AreEqual(new Vector4(0, 0, 1, 0), j.column2);
            Assert.AreEqual(new Vector4(0, 0, 0, 1), j.column3);
        }

        [Test]
        public void TestIntersects()
        {
            var g = new PlanarPrismModifier(new SquareGrid(1), new PlanarPrismOptions { });
            CollectionAssert.AreEquivalent(new[] { new Cell() }, g.GetCellsIntersectsApprox(new Vector3(), new Vector3(.1f, .1f, .1f))); ;
        }

        [Test]
        public void TestTriangleMesh()
        {
            var g = new PlanarPrismModifier(new SquareGrid(1));
            GridTest.TestTriangleMesh(g, new Cell(), dir => ((CubeDir)dir).Forward(), _ => 2);
        }

        [Test]
        public void TestRaycast()
        {
            // LayerOffset=0.5 aligns PlanarPrismModifier layers with CubeGrid z-cells.
            var cube = new CubeGrid(1);
            var prism = new PlanarPrismModifier(
                new SquareGrid(1),
                new PlanarPrismOptions { LayerHeight = 1, LayerOffset = 0.5f });

            var origin = new Vector3(0.5f, 0.5f, 0.5f);

            // Lateral ray (no layer crossings)
            CompareRaycast(cube, prism, origin, new Vector3(2, 1, 0), 1);

            // Pure z ray (only layer crossings)
            CompareRaycast(cube, prism, origin, new Vector3(0, 0, 1), 5);
            CompareRaycast(cube, prism, origin, new Vector3(0, 0, -1), 5);

            // Diagonal rays (both 2D and layer crossings)
            CompareRaycast(cube, prism, origin, new Vector3(1.1f, 0.7f, 1.3f), 3);
            CompareRaycast(cube, prism, origin, new Vector3(1.1f, 0.7f, -1.3f), 3);
            CompareRaycast(cube, prism, origin, new Vector3(3, 1, 2), 2);

            // Corner hits, where a planar face and a layer face are crossed together
            CompareRaycast(cube, prism, origin, new Vector3(1, 1, 1), 3);
            CompareRaycast(cube, prism, origin, new Vector3(1, 0, 1), 3);
            CompareRaycast(cube, prism, origin, new Vector3(1, -1, -1), 3);

            // Ray starting outside the origin cell, or exactly on a face
            CompareRaycast(cube, prism, new Vector3(-0.3f, 0.5f, 0.5f), new Vector3(1, 0, 0), 5);
            CompareRaycast(cube, prism, new Vector3(-0.3f, 0.5f, 0.5f), new Vector3(1, 0.3f, 0.7f), 5);
            CompareRaycast(cube, prism, new Vector3(1f, 0.5f, 0.5f), new Vector3(-1, 0, 0), 3);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 1f), new Vector3(0, 0, -1), 3);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 1f), new Vector3(0, 0, 1), 3);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 1f), new Vector3(1, 0, -1), 3);

            // Non-uniform cell size. LayerOffset = height/2 keeps layers on the same slabs as CubeGrid.
            var scaledCube = new CubeGrid(new Vector3(2, 3, 4));
            var scaledPrism = new PlanarPrismModifier(
                new SquareGrid(new Vector2(2, 3)),
                new PlanarPrismOptions { LayerHeight = 4, LayerOffset = 2 });
            CompareRaycast(scaledCube, scaledPrism, new Vector3(1, 1.5f, 2), new Vector3(3, -1, 2), 4);
            CompareRaycast(scaledCube, scaledPrism, new Vector3(-1, 4, -3), new Vector3(2, -0.5f, 1.5f), 6);
        }

        [Test]
        public void TestRaycast_Bounds()
        {
            var options = new PlanarPrismOptions { LayerHeight = 1, LayerOffset = 0.5f };
            var cube = new CubeGrid(1, new CubeBound(new Vector3Int(0, 0, 0), new Vector3Int(3, 2, 4)));
            var prism = new PlanarPrismModifier(new SquareGrid(1), options)
                .BoundBy(new PlanarPrismBound
                {
                    MinLayer = 0,
                    MexLayer = 4,
                    PlanarBound = new SquareBound(0, 0, 3, 2),
                });

            // Stay inside, then leave through a side and through the top
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0.2f, 0.5f), 10);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 3.5f), new Vector3(0.2f, 0, 1), 10);

            // Enter from outside: below, above, the side, and a corner
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, -2), new Vector3(0, 0, 1), 10);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 6), new Vector3(0, 0, -1), 10);
            CompareRaycast(cube, prism, new Vector3(-2, 0.5f, 1.5f), new Vector3(1, 0, 0), 10);
            CompareRaycast(cube, prism, new Vector3(-1, 0.5f, -1.1f), new Vector3(1, 0.1f, 1), 10);
            CompareRaycast(cube, prism, new Vector3(-1, 0.5f, -0.9f), new Vector3(1, 0.1f, 1), 10);

            // Exactly on a corner, a side face and the layer face are reached together.
            // CubeGrid picks the side (x before z); the prism enters through the layer face.
            var cornerHits = prism.Raycast(new Vector3(-1, 0.5f, -1), new Vector3(1, 0.1f, 1), 10).ToList();
            Assert.AreEqual(new Cell(0, 0, 0), cornerHits[0].cell);
            Assert.AreEqual((CellDir)CubeDir.Back, cornerHits[0].cellDir);
            Assert.That(cornerHits[0].distance, Is.EqualTo(1f).Within(0.001f));
            CollectionAssert.AreEqual(
                cube.Raycast(new Vector3(-1, 0.5f, -1), new Vector3(1, 0.1f, 1), 10).Select(h => h.cell),
                cornerHits.Select(h => h.cell));

            // Miss the volume, including rays that run along an outer face
            CompareRaycast(cube, prism, new Vector3(-2, 0.5f, 1.5f), new Vector3(-1, 0, 0), 10);
            CompareRaycast(cube, prism, new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(1, 0, 0), 10);
            CompareRaycast(cube, prism, new Vector3(-0.5f, 0f, 0.5f), new Vector3(1, 0, 0), 10);
            CompareRaycast(cube, prism, new Vector3(-0.5f, 2f, 0.5f), new Vector3(1, 0, 0), 10);

            // maxDistance ends the ray inside the volume
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0, 1), 0.5f);

            // Layer bounds only. XY is wide enough that this ray does not meet the cube's side faces.
            var layerCube = new CubeGrid(1, new CubeBound(new Vector3Int(-20, -20, 1), new Vector3Int(20, 20, 4)));
            var layerPrism = new PlanarPrismModifier(
                new SquareGrid(1),
                options,
                new PlanarPrismBound { MinLayer = 1, MexLayer = 4 });
            CompareRaycast(layerCube, layerPrism, new Vector3(0.5f, 0.5f, -1), new Vector3(0.4f, -0.15f, 0.7f), 8);
            CompareRaycast(layerCube, layerPrism, new Vector3(0.5f, 0.5f, 2.5f), new Vector3(0, 0, 1), 10);

            // Outside the layer slab and moving away from it: no hits, and the query terminates.
            CollectionAssert.IsEmpty(layerPrism.Raycast(new Vector3(0.5f, 0.5f, 10f), new Vector3(1, 0, 0)).Take(5));
            CollectionAssert.IsEmpty(layerPrism.Raycast(new Vector3(0.5f, 0.5f, -2f), new Vector3(0, 0, -1)).Take(5));

            // A diagonal ray through a finite prism must stop, even with no maxDistance.
            var hits = prism.Raycast(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 0, 1)).Take(100).ToList();
            Assert.Less(hits.Count, 100);
            Assert.IsNotEmpty(hits);
            Assert.IsTrue(hits.All(h => prism.IsCellInGrid(h.cell)));
        }

        [Test]
        public void TestRaycast_Gap()
        {
            // A masked-out column leaves a gap in the planar raycast between a cell's exit
            // and the next cell's entry. Layer faces crossed in that gap must still count.
            var options = new PlanarPrismOptions { LayerHeight = 1, LayerOffset = 0.5f };
            Func<Cell, bool> noColumn1 = c => c.x != 1;
            var cube = new MaskModifier(new CubeGrid(1), noColumn1);
            var prism = new PlanarPrismModifier(new MaskModifier(new SquareGrid(1), noColumn1), options);

            var origin = new Vector3(0.5f, 0.5f, 0.5f);
            // Two layer faces crossed inside the gap, going up and going down
            CompareRaycast(cube, prism, origin, new Vector3(1, 0, 2), 2.5f);
            CompareRaycast(cube, prism, new Vector3(0.5f, 0.5f, 4.5f), new Vector3(1, 0, -2), 2.5f);
            // No layer face in the gap: the carried layer is still right
            CompareRaycast(cube, prism, origin, new Vector3(1, 0, 0.1f), 2.5f);
            // Layer face exactly at the exit, and another inside the gap
            CompareRaycast(cube, prism, origin, new Vector3(1, 0, 1), 2.5f);
            // Lateral through the gap
            CompareRaycast(cube, prism, origin, new Vector3(1, 0.2f, 0), 2.5f);
        }

        [Test]
        public void TestRaycast_MeshGap()
        {
            // Two unit quads in the XY plane, with a unit gap between them.
            var meshData = new MeshData
            {
                vertices = new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                    new Vector3(2, 0, 0), new Vector3(3, 0, 0), new Vector3(3, 1, 0), new Vector3(2, 1, 0),
                },
                normals = Enumerable.Repeat(Vector3.forward, 8).ToArray(),
                indices = new[] { new[] { 0, 1, 2, 3, 4, 5, 6, 7 } },
                topologies = new[] { MeshTopology.Quads },
            };
            var g = new PlanarPrismModifier(new MeshGrid(meshData), new PlanarPrismOptions { LayerHeight = 1, LayerOffset = 0.5f });

            var origin = new Vector3(0.5f, 0.5f, 0.5f);
            var direction = new Vector3(1, 0, 2);
            var maxDistance = 2.5f;

            var hits = g.Raycast(origin, direction, maxDistance).ToList();
            // First quad: layers 0 and 1. Gap over x in [1, 2) climbs through layers 2 and 3.
            // Second quad: layers 3, 4 and 5, then the ray leaves at x = 3 (t = 2.5).
            CollectionAssert.AreEqual(new[] { 0, 1, 3, 4, 5 }, hits.Select(h => h.cell.z));
            Assert.AreEqual(hits[0].cell.x, hits[1].cell.x);
            Assert.AreNotEqual(hits[1].cell.x, hits[2].cell.x);
            Assert.That(hits[2].distance, Is.EqualTo(1.5f).Within(0.001f));

            // Every hit should be in the cell that actually contains the ray just after it.
            // (A midpoint would land in the gap for the hit before it.)
            foreach (var hit in hits)
            {
                var p = origin + direction * (hit.distance + 0.01f);
                Assert.IsTrue(g.FindCell(p, out var cell), $"No cell at {p}");
                Assert.AreEqual(hit.cell, cell);
            }

            // With exit info, the first quad's exit is at the gap, and nothing is reported in it.
            var all = g.Raycast(origin, direction, maxDistance, exitInfo: true).ToList();
            var exits = all.Where(h => h.isExit).ToList();
            Assert.AreEqual(hits.Count, all.Count - exits.Count);
            Assert.IsFalse(all.Any(h => h.distance > 0.501f && h.distance < 1.499f), "No hits inside the gap");
            var gapExit = exits.Single(h => Math.Abs(h.distance - 0.5f) < 0.001f);
            Assert.AreEqual(new Cell(hits[1].cell.x, hits[1].cell.y, 1), gapExit.cell);
        }

        [Test]
        public void TestRaycast_Triangle()
        {
            var g = new TrianglePrismGrid(1, 1, TriangleOrientation.FlatTopped);
            var origin = g.GetCellCenter(new Cell(0, 0, 0));
            var direction = new Vector3(0.2f, 0.1f, 0.7f);
            var maxDistance = 4f;
            var hits = g.Raycast(origin, direction, maxDistance).Take(50).ToList();

            Assert.IsNotEmpty(hits);
            Assert.Less(hits.Count, 50);
            Assert.AreEqual(new Cell(0, 0, 0), hits[0].cell);
            Assert.IsNull(hits[0].cellDir);
            Assert.AreEqual(0, hits[0].distance);

            for (var i = 0; i < hits.Count; i++)
            {
                Assert.LessOrEqual(hits[i].distance, maxDistance);
                if (i > 0)
                {
                    Assert.GreaterOrEqual(hits[i].distance, hits[i - 1].distance);
                    if (hits[i].distance > hits[i - 1].distance)
                    {
                        var mid = origin + direction * ((hits[i - 1].distance + hits[i].distance) * 0.5f);
                        Assert.IsTrue(g.FindCell(mid, out var cell), $"No cell at {mid}");
                        Assert.AreEqual(hits[i - 1].cell, cell);
                    }
                }
            }

            // Straight up the layers of one triangle.
            var up = g.Raycast(origin, new Vector3(0, 0, 1), 5).ToList();
            CollectionAssert.AreEqual(
                Enumerable.Range(0, 6).Select(layer => new Cell(0, 0, layer)),
                up.Select(h => h.cell));
            Assert.AreEqual(0f, up[0].distance);
            for (var layer = 1; layer <= 5; layer++)
            {
                Assert.That(up[layer].distance, Is.EqualTo(layer - 0.5f).Within(0.001f));
                Assert.IsNotNull(up[layer].cellDir);
            }
        }

        private void CompareRaycast(IGrid cube, IGrid prism, Vector3 origin, Vector3 direction, float maxDistance)
        {
            CompareRaycast(cube, prism, origin, direction, maxDistance, false);
            CompareRaycast(cube, prism, origin, direction, maxDistance, true);
        }

        private void CompareRaycast(IGrid cube, IGrid prism, Vector3 origin, Vector3 direction, float maxDistance, bool exitInfo)
        {
            var cubeResults = cube.Raycast(origin, direction, maxDistance, exitInfo).ToList();
            var prismResults = prism.Raycast(origin, direction, maxDistance, exitInfo).ToList();
            var label = $"Ray ({origin}, {direction}, {maxDistance}, exitInfo={exitInfo})";

            Assert.AreEqual(cubeResults.Count, prismResults.Count,
                $"{label}: count mismatch.\n" +
                $"  Cube:  [{string.Join(", ", cubeResults.Select(FormatHit))}]\n" +
                $"  Prism: [{string.Join(", ", prismResults.Select(FormatHit))}]");

            for (int i = 0; i < cubeResults.Count; i++)
            {
                Assert.AreEqual(cubeResults[i].cell, prismResults[i].cell, $"{label}: cell mismatch at [{i}]");
                Assert.AreEqual(cubeResults[i].cellDir, prismResults[i].cellDir, $"{label}: cellDir mismatch at [{i}]");
                Assert.AreEqual(cubeResults[i].isExit, prismResults[i].isExit, $"{label}: isExit mismatch at [{i}]");
                Assert.That(prismResults[i].distance, Is.EqualTo(cubeResults[i].distance).Within(0.001f),
                    $"{label}: distance mismatch at [{i}]");
                TestUtils.AssertAreEqual(cubeResults[i].point, prismResults[i].point, 0.001f,
                    $"{label}: point mismatch at [{i}]");
            }
        }

        private static string FormatHit(RaycastInfo hit)
        {
            return $"{hit.cell} dir={hit.cellDir} t={hit.distance} exit={hit.isExit}";
        }
    }
}
