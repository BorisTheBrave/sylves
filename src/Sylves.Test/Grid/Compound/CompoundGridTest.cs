using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class CompoundGridTest
    {
        public static MeshData Square = new MeshData
        {
            indices = new[] { new[] { 0, 1, 2, 3, } },
            vertices = new[]
        {
                    new Vector3(1, 0, 0.0f),
                    new Vector3(1, 1, 0.0f),
                    new Vector3(0, 1, 0.0f),
                    new Vector3(0, 0, 0.0f),
                },
            normals = new[]
        {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                },
            topologies = new[] { MeshTopology.Quads }
        };

        public static CompoundSection[] Squares =
        {
            new CompoundSection(
                Square,
                new Vector2(1, 0),
                new Vector2(0, 1),
                new List<HalfPlane>
                {
                    new HalfPlane {A = 1, B = 0, C = 0},
                    new HalfPlane {A = 0, B = 1, C = 0},
                }
            ),
            new CompoundSection(
                Square,
                new Vector2(1, 0),
                new Vector2(0, 1),
                new List<HalfPlane>
                {
                    new HalfPlane {A = -1, B = 0, C = -1},
                    new HalfPlane {A = 0, B = 1, C = 0},
                }
            ),
            new CompoundSection(
                Square,
                new Vector2(1, 0),
                new Vector2(0, 1),
                new List<HalfPlane>
                {
                    new HalfPlane {A = -1, B = 0, C = -1},
                    new HalfPlane {A = 0, B = -1, C = -1},
                }
            ),
            new CompoundSection(
                Square,
                new Vector2(1, 0),
                new Vector2(0, 1),
                new List<HalfPlane>
                {
                    new HalfPlane {A = 1, B = 0, C = 0},
                    new HalfPlane {A = 0, B = -1, C = -1},
                }
            ),
        };

        public static CompoundGrid SquaresGrid = new CompoundGrid(Squares.ToList());


        #region Subtract
        [Test]
        public void TestSubtract_NoOffsetEmpty()
        {
            var area = new List<HalfPlane>
            {
                new HalfPlane{ A = 1, B = 0, C = 0 },
            };
            var lines = HalfPlaneUtils.Subtract(area, new Vector2Int(0, 0));
            Assert.That(lines, Is.Empty);
        }

        [Test]
        public void TestSubtract_SingleHalfPlaneRightShift()
        {
            var area = new List<HalfPlane>
            {
                new HalfPlane{ A = 1, B = 0, C = 0 },
            };
            var lines = HalfPlaneUtils.Subtract(area, new Vector2Int(1, 0));
            Assert.That(lines.Count, Is.EqualTo(1));
            var l = lines[0];
            Assert.That(l.Origin.x, Is.EqualTo(0));
            Assert.That(l.Direction.x, Is.EqualTo(0));
            Assert.That(l.Direction.y, Is.Not.EqualTo(0));
            Assert.That(l.TMin.HasValue, Is.False);
            Assert.That(l.TMax.HasValue, Is.False);
        }

        [Test]
        public void TestSubtract_QuadrantDiagonalShiftPartition()
        {
            var area2 = new List<HalfPlane>
            {
                new HalfPlane{ A = 1, B = 0, C = 0 },
                new HalfPlane{ A = 0, B = 1, C = 0 },
            };
            var lines = HalfPlaneUtils.Subtract(area2, new Vector2Int(1, 1));
            Assert.That(lines.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(lines.Count, Is.LessThanOrEqualTo(2));

            var bbMinX = -1; var bbMaxX = 5; var bbMinY = -1; var bbMaxY = 5;
            var pts = CollectPoints(lines, bbMinX, bbMaxX, bbMinY, bbMaxY);
            Assert.That(pts.Count, Is.EqualTo(pts.Distinct().Count()));
            foreach (var p in pts)
            {
                Assert.That(p.Item1 >= 0 && p.Item2 >= 0, $"Point {p} not in original area");
                Assert.That(!(p.Item1 >= 1 && p.Item2 >= 1), $"Point {p} should be removed by shifted area");
            }
            for (int x = bbMinX; x <= bbMaxX; x++)
            for (int y = bbMinY; y <= bbMaxY; y++)
            {
                bool inOriginal = x >= 0 && y >= 0;
                bool inShifted = (x - 1) >= 0 && (y - 1) >= 0;
                if (inOriginal && !inShifted)
                {
                    Assert.That(pts.Contains((x, y)), $"Missing expected point ({x},{y})");
                }
            }
        }
        private static HashSet<(int, int)> CollectPoints(List<LatticeLine> lines, int minX, int maxX, int minY, int maxY)
        {
            var set = new HashSet<(int, int)>();
            foreach (var ln in lines)
            {
                // Compute t-range intersecting the bounding box
                int? tMin = ln.TMin;
                int? tMax = ln.TMax;

                // Intersect with box constraints for x
                if (ln.Direction.x != 0)
                {
                    var tx1 = CeilDiv(minX - ln.Origin.x, ln.Direction.x);
                    var tx2 = FloorDiv(maxX - ln.Origin.x, ln.Direction.x);
                    var lo = Math.Min(tx1, tx2);
                    var hi = Math.Max(tx1, tx2);
                    tMin = tMin.HasValue ? Math.Max(tMin.Value, lo) : lo;
                    tMax = tMax.HasValue ? Math.Min(tMax.Value, hi) : hi;
                }
                else
                {
                    if (ln.Origin.x < minX || ln.Origin.x > maxX) continue; // No intersection with box
                }

                // Intersect with box constraints for y
                if (ln.Direction.y != 0)
                {
                    var ty1 = CeilDiv(minY - ln.Origin.y, ln.Direction.y);
                    var ty2 = FloorDiv(maxY - ln.Origin.y, ln.Direction.y);
                    var lo = Math.Min(ty1, ty2);
                    var hi = Math.Max(ty1, ty2);
                    tMin = tMin.HasValue ? Math.Max(tMin.Value, lo) : lo;
                    tMax = tMax.HasValue ? Math.Min(tMax.Value, hi) : hi;
                }
                else
                {
                    if (ln.Origin.y < minY || ln.Origin.y > maxY) continue; // No intersection with box
                }

                if (tMin.HasValue && tMax.HasValue && tMin.Value > tMax.Value) continue;

                var start = tMin ?? -100; // Should be replaced by bounded values already
                var end = tMax ?? 100;
                // Clamp a bit
                start = Math.Max(start, -1000);
                end = Math.Min(end, 1000);

                for (int t = start; t <= end; t++)
                {
                    var x = ln.Origin.x + t * ln.Direction.x;
                    var y = ln.Origin.y + t * ln.Direction.y;
                    if (x < minX || x > maxX || y < minY || y > maxY) continue;
                    set.Add((x, y));
                }
            }
            return set;
        }

        private static int FloorDiv(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            int q = a / b;
            int r = a % b;
            if (r != 0 && ((r > 0) != (b > 0))) q--;
            return q;
        }

        private static int CeilDiv(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            int q = a / b;
            int r = a % b;
            if (r != 0 && ((r > 0) == (b > 0))) q++;
            return q;
        }

        #endregion


        #region TryFindMatchingRange tests
        // For TryFindMatchingRange tests, we can omit some details of LatticeLine and HalfEdge set that are not used.
        // We also omit OriginT, as it's just summed straight into v0/v1.

        [Test]
        public void TryFindMatchingRange_InfiniteParallel_SameDirection()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 0), V1 = new Vector2(0, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.True);
            Assert.That(r.SrcTMin, Is.Null);
            Assert.That(r.SrcTMax, Is.Null);
            Assert.That(r.DestTMin, Is.Null);
            Assert.That(r.DestTMax, Is.Null);
        }

        [Test]
        public void TryFindMatchingRange_InfiniteParallel_OppositeDirection()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 0), V1 = new Vector2(0, 0), StrideT = new Vector2(-1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.True);
            Assert.That(r.SrcTMin, Is.Null);
            Assert.That(r.SrcTMax, Is.Null);
            Assert.That(r.DestTMin, Is.Null);
            Assert.That(r.DestTMax, Is.Null);
        }

        [Test]
        public void TryFindMatchingRange_DisjointParallel_NoMatch()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 1), V1 = new Vector2(0, 1), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.False);
        }

        [Test]
        public void TryFindMatchingRange_NonParallel_Matches()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = null, TMax = null } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 0), V1 = new Vector2(0, 0), StrideT = new Vector2(0, 1), Line = new LatticeLine { TMin = null, TMax = null } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.True);
            Assert.That(r.SrcTMin, Is.EqualTo(0));
            Assert.That(r.SrcTMax, Is.EqualTo(0));
            Assert.That(r.DestTMin, Is.EqualTo(0));
            Assert.That(r.DestTMax, Is.EqualTo(0));
        }

        [Test]
        public void TryFindMatchingRange_SameDirection_Range()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = 0, TMax = 15 } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(11, 0), V1 = new Vector2(10, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = 0, TMax = 10 } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.True);
            Assert.That(r.SrcTMin, Is.EqualTo(10));
            Assert.That(r.SrcTMax, Is.EqualTo(15));
            Assert.That(r.DestTMin, Is.EqualTo(0));
            Assert.That(r.DestTMax, Is.EqualTo(5));
        }

        [Test]
        public void TryFindMatchingRange_OppositeDirection_Range()
        {
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { TMin = 0, TMax = 15 } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(11, 0), V1 = new Vector2(10, 0), StrideT = new Vector2(-1, 0), Line = new LatticeLine { TMin = -10, TMax = 0 } };
            Assert.That(CompoundGrid.TryFindMatchingRange(a, b, out var r), Is.True);

            Assert.That(r.SrcTMin, Is.EqualTo(10));
            Assert.That(r.SrcTMax, Is.EqualTo(15));
            Assert.That(r.DestTMin, Is.EqualTo(-5));
            Assert.That(r.DestTMax, Is.EqualTo(0));
        }

        // -------------------- PairHalfEdgesGreedy tests --------------------
        [Test]
        public void PairGreedy_SingletonOpposite_PairsOnce()
        {
            // Two single-step half-edges with reversed geometry; stride irrelevant due to IsSingle
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { Origin = new Vector2Int(0, 0), Direction = new Vector2Int(1, 0), TMin = 3, TMax = 3 } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 0), V1 = new Vector2(0, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { Origin = new Vector2Int(0, 0), Direction = new Vector2Int(1, 0), TMin = 3, TMax = 3 } };
            var res = CompoundGrid.PairHalfEdgesGreedy(new[] { a, b });
            Assert.That(res.paired.Count, Is.EqualTo(1));
            Assert.That(res.remainders.Count, Is.EqualTo(0));
        }

        [Test]
        public void PairGreedy_FiniteOpposite_SubdividesAndPairs()
        {
            // Axis aligned, opposite directions, overlapping range
            var a = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(0, 0), V1 = new Vector2(1, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { Origin = new Vector2Int(0, 0), Direction = new Vector2Int(1, 0), TMin = 0, TMax = 10 } };
            var b = new CompoundGrid.HalfEdgeSet { V0 = new Vector2(1, 0), V1 = new Vector2(0, 0), StrideT = new Vector2(1, 0), Line = new LatticeLine { Origin = new Vector2Int(5, 0), Direction = new Vector2Int(-1, 0), TMin = 0, TMax = 10 } };
            var res = CompoundGrid.PairHalfEdgesGreedy(new[] { a, b });
            Assert.That(res.paired.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(res.remainders.Count, Is.GreaterThanOrEqualTo(0));
        }

        #endregion

        [Test]
        public void TestCompoundGrid()
        {
            var g = SquaresGrid;
        }
    }
}
