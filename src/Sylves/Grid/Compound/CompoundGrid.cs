using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Sylves
{
    public class HalfPlane
    {
        public int A { get; set; }
        public int B { get; set; }

        public int C { get; set; }

        public bool Test(int x, int y)
        {
            return A * x + B * y + C >= 0;
        }
    }

    public class LatticeLine
    {
        public int? TMin { get; set; }
        public int? TMax { get; set; }

        public Vector2Int Origin { get; set; }
        public Vector2Int Direction { get; set; }

        public bool IsSingle => TMin.HasValue && TMax.HasValue && TMin.Value == TMax.Value;

        public bool IsFinite => TMin.HasValue && TMax.HasValue;

        public bool TryGet(int t, out Vector2Int point)
        {
            point = new Vector2Int(Origin.x + Direction.x * t, Origin.y + Direction.y * t);
            return (TMin == null || t >= TMin.Value) && (TMax == null || t <= TMax.Value);
        }

        public bool TryGet(Vector2Int point, out int t)
        {
			t = 0;
			var dx = Direction.x;
			var dy = Direction.y;
			var deltaX = point.x - Origin.x;
			var deltaY = point.y - Origin.y;
			// Handle zero direction (degenerate)
			if (dx == 0 && dy == 0)
			{
				// Only matches the origin at t = 0
				if (deltaX == 0 && deltaY == 0)
				{
					t = 0;
					return (TMin == null || t >= TMin.Value) && (TMax == null || t <= TMax.Value);
				}
				return false;
			}
			if (dx != 0)
			{
				if (deltaX % dx != 0) return false;
				var k = deltaX / dx;
				if (Origin.y + k * dy != point.y) return false;
				t = k;
			}
			else // dx == 0, so dy != 0
			{
				if (deltaY % dy != 0) return false;
				var k = deltaY / dy;
				if (Origin.x + k * dx != point.x) return false;
				t = k;
			}
			return (TMin == null || t >= TMin.Value) && (TMax == null || t <= TMax.Value);
        }
    }

    public class CompoundSection
    {
        public MeshData MeshData { get; set; }
        public Vector2 StrideX { get; set; }
        public Vector2 StrideY { get; set; }

        public List<HalfPlane> HalfPlanes { get; set; }
    }

    internal class CompoundGrid
    {

        internal class HalfEdgeSet
        {
            public int SectionIndex {get;set;}
            public Cell Cell {get;set;}
            public CellDir Dir {get;set;}
            public Vector2 V0 {get;set;}
            public Vector2 V1 {get;set;}
            
            public Vector2 OriginT { get; set; }
            public Vector2 StrideT { get; set; }

            public LatticeLine Line {get;set;}
        }

        internal class PairedHalfEdgeSet
        {
            public int SrcSectionIndex {get;set;}
            public int DestSectionIndex {get;set;}
            public Cell SrcCell {get;set;}
            public CellDir SrcDir {get;set;}
            public Cell DestCell {get;set;}
            public CellDir DestDir {get;set;}
            public Vector2 V0 {get;set;}
            public Vector2 V1 {get;set;}
            
            public Vector2 SrcStrideT { get; set; }
            public Vector2 DestStrideT { get; set; }

            public LatticeLine SrcLine {get;set;}
            public LatticeLine DestLine {get;set;}
        }

        private readonly List<HalfEdgeSet> halfEdges = new List<HalfEdgeSet>();
        private readonly List<PairedHalfEdgeSet> pairedHalfEdges;
        

        public CompoundGrid(List<CompoundSection> sections)
        {
            if(sections == null) throw new ArgumentNullException(nameof(sections));

            for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
            {
                var section = sections[sectionIndex];
                var grid = new PeriodicPlanarMeshGrid(section.MeshData, section.StrideX, section.StrideY);
                halfEdges.AddRange(GetHalfEdges(sectionIndex, section, grid));
            }

            var pr = PairHalfEdgesGreedy(halfEdges);
            pairedHalfEdges = pr.paired;
            halfEdges = pr.remainders;
        }

        private static List<HalfEdgeSet> GetHalfEdges(int sectionIndex, CompoundSection section, PeriodicPlanarMeshGrid grid)
        {
            var halfEdges = new List<HalfEdgeSet>();

            // Restrict to a single chunk (0,0) so GetCells() is finite
            var centerGrid = (PeriodicPlanarMeshGrid)grid.BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(1, 1)));

            foreach (var cell in centerGrid.GetCells())
            {
                foreach (var dir in grid.GetCellDirs(cell))
                {
                    if (!grid.TryMove(cell, dir, out var dest, out var inverseDir, out var connection))
                        continue;

                    // Compute chunk delta using PeriodicPlanarMeshGrid's Combine/Split layout:
                    // Combine(centerCell, chunk) => new Cell(centerCell.x, chunk.x, chunk.y)
                    var srcChunk = new Vector2Int(cell.y, cell.z);
                    var destChunk = new Vector2Int(dest.y, dest.z);
                    var chunkDelta = new Vector2Int(destChunk.x - srcChunk.x, destChunk.y - srcChunk.y);

                    // Generate lattice lines for this delta
                    var lines = HalfPlaneUtils.Subtract(section.HalfPlanes, chunkDelta);
                    if (lines == null || lines.Count == 0)
                        continue;

                    // Get polygon edge endpoints for this cell/dir
                    grid.GetPolygon(cell, out var vertices, out var transform);
                    var n = vertices.Length;
                    var i0 = ((int)dir) % n;
                    if (i0 < 0) i0 += n;
                    var i1 = (i0 + 1) % n;

                    var v0 = transform.MultiplyPoint3x4(vertices[i0]);
                    var v1 = transform.MultiplyPoint3x4(vertices[i1]);

                    foreach (var line in lines)
                    {
                        halfEdges.Add(new HalfEdgeSet
                        {
                            SectionIndex = sectionIndex,
                            Cell = cell,
                            Dir = dir,
                            V0 = new Vector2(v0.x, v0.y),
                            V1 = new Vector2(v1.x, v1.y),
                            // Flattens mapping from T to chunk space to real space
                            OriginT = section.StrideX * line.Origin.x + section.StrideY * line.Origin.y,
                            StrideT = section.StrideX * line.Direction.x + section.StrideY * line.Direction.y,
                            Line = line,
                        });
                    }
                }
            }
            return halfEdges;
        }

        private static bool Approximately(Vector2 a, Vector2 b, float eps = 1e-5f)
        {
            return Math.Abs(a.x - b.x) <= eps && Math.Abs(a.y - b.y) <= eps;
        }

        private static bool IsSingle(LatticeLine line)
        {
            return line.TMin.HasValue && line.TMax.HasValue && line.TMin.Value == line.TMax.Value;
        }

        private static bool IsFinite(LatticeLine line)
        {
            return line.TMin.HasValue && line.TMax.HasValue;
        }

        private static int? DecrementNullable(int? a)
        {
            if (!a.HasValue) return null;
            return a.Value - 1;
        }

        private static int? IncrementNullable(int? a)
        {
            if (!a.HasValue) return null;
            return a.Value + 1;
        }

        private static bool IsEmptyInterval(int? min, int? max)
        {
            if (min.HasValue && max.HasValue && min.Value > max.Value) return true;
            return false;
        }

        private static int? MaxNullable(int? a, int? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return Math.Max(a.Value, b.Value);
        }

        private static int? MinNullable(int? a, int? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return Math.Min(a.Value, b.Value);
        }

        private static int? AddNullable(int? a, int b)
        {
            return a.HasValue ? a.Value + b : (int?)null;
        }

        private static int? NegateNullable(int? a)
        {
            return a.HasValue ? -a.Value : (int?)null;
        }

        private static (LatticeLine left, LatticeLine middle, LatticeLine right) Subdivide(LatticeLine line, int? matchMin, int? matchMax)
        {
            var leftMin = line.TMin;
            var leftMax = DecrementNullable(matchMin);
            var rightMin = IncrementNullable(matchMax);
            var rightMax = line.TMax;

            LatticeLine left = null, middle = null, right = null;
            if (!IsEmptyInterval(leftMin, leftMax) && matchMin != null)
            {
                left = new LatticeLine { Origin = line.Origin, Direction = line.Direction, TMin = leftMin, TMax = leftMax };
            }
            if (!IsEmptyInterval(matchMin, matchMax))
            {
                middle = new LatticeLine { Origin = line.Origin, Direction = line.Direction, TMin = matchMin, TMax = matchMax };
            }
            if (!IsEmptyInterval(rightMin, rightMax) && matchMax != null)
            {
                right = new LatticeLine { Origin = line.Origin, Direction = line.Direction, TMin = rightMin, TMax = rightMax };
            }
            return (left, middle, right);
        }

        internal struct MatchRange
        {
            public int? SrcTMin;
            public int? SrcTMax;
            public int? DestTMin;
            public int? DestTMax;
            public bool? IsForward;
        }

        internal static bool TryFindMatchingRange(HalfEdgeSet src, HalfEdgeSet dest, out MatchRange range)
        {
            range = new MatchRange();

            // Orientation check: src.V0->V1 reversed equals dest.V0->V1
            var srcEdge = new Vector2(src.V0.x - src.V1.x, src.V0.y - src.V1.y);
            var destEdge = new Vector2(dest.V1.x - dest.V0.x, dest.V1.y - dest.V0.y);
            if (!Approximately(srcEdge, destEdge))
            {
                return false;
            }

            var srcV = new Vector2(src.V0.x + src.OriginT.x, src.V0.y + src.OriginT.y);
            var destV = new Vector2(dest.V1.x + dest.OriginT.x, dest.V1.y + dest.OriginT.y);
            var a = src.StrideT;
            var b = dest.StrideT;

            if (Approximately(a, new Vector2(0, 0)) || Approximately(b, new Vector2(0, 0)))
            {
                throw new Exception("Degenerate stride");
            }

            float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;
            bool Approx(float x, float y, float eps = 1e-5f) => Math.Abs(x - y) <= eps;

            var delta = new Vector2(destV.x - srcV.x, destV.y - srcV.y);
            var cross = Cross(a, b);

            // Non-parallel: unique integer solution required
            if (!Approx(cross, 0))
            {
                var tReal = (delta.x * b.y - delta.y * b.x) / cross;
                var uReal = (a.x * delta.y - a.y * delta.x) / cross;
                var tInt = (int)Math.Round(tReal);
                var uInt = (int)Math.Round(uReal);
                if (!Approx(tReal, tInt) || !Approx(uReal, uInt)) return false;
                if ((src.Line.TMin.HasValue && tInt < src.Line.TMin.Value) || (src.Line.TMax.HasValue && tInt > src.Line.TMax.Value)) return false;
                if ((dest.Line.TMin.HasValue && uInt < dest.Line.TMin.Value) || (dest.Line.TMax.HasValue && uInt > dest.Line.TMax.Value)) return false;
                range.SrcTMin = tInt; range.SrcTMax = tInt;
                range.DestTMin = uInt; range.DestTMax = uInt;
                return true;
            }

            // Parallel cases
            var crossDeltaA = Cross(a, delta);
            if (!Approx(crossDeltaA, 0))
            {
                return false;
            }

            if (Approximately(a, b))
            {
                // u = t - k where a * k = delta
                float kReal;
                if (!Approx(a.x, 0)) kReal = delta.x / a.x; else kReal = delta.y / a.y;
                var k = (int)Math.Round(kReal);
                var aK = new Vector2(a.x * k, a.y * k);
                if (!Approximately(new Vector2(delta.x, delta.y), aK)) return false;
                var mapMin = dest.Line.TMin.HasValue ? dest.Line.TMin.Value + k : (int?)null;
                var mapMax = dest.Line.TMax.HasValue ? dest.Line.TMax.Value + k : (int?)null;
                var tMin = MaxNullable(src.Line.TMin, mapMin);
                var tMax = MinNullable(src.Line.TMax, mapMax);
                if (IsEmptyInterval(tMin, tMax)) return false;
                range.SrcTMin = tMin; range.SrcTMax = tMax;
                range.DestTMin = AddNullable(tMin, -k);
                range.DestTMax = AddNullable(tMax, -k);
                range.IsForward = true;
                return true;
            }

            var negB = new Vector2(-b.x, -b.y);
            if (Approximately(a, negB))
            {
                // u = -t + k where a * k = delta
                float kReal;
                if (!Approx(a.x, 0)) kReal = delta.x / a.x; else kReal = delta.y / a.y;
                var k = (int)Math.Round(kReal);
                var aK = new Vector2(a.x * k, a.y * k);
                if (!Approximately(new Vector2(delta.x, delta.y), aK)) return false;
                var mapMin = dest.Line.TMax.HasValue ? (-dest.Line.TMax.Value + k) : (int?)null;
                var mapMax = dest.Line.TMin.HasValue ? (-dest.Line.TMin.Value + k) : (int?)null;
                var tMin = MaxNullable(src.Line.TMin, mapMin);
                var tMax = MinNullable(src.Line.TMax, mapMax);
                if (IsEmptyInterval(tMin, tMax)) return false;
                range.SrcTMin = tMin; range.SrcTMax = tMax;
                range.DestTMin = AddNullable(NegateNullable(tMax), k);
                range.DestTMax = AddNullable(NegateNullable(tMin), k);
                range.IsForward = false;
                return true;
            }

            throw new Exception("Parallel non-opposite unequal stride not supported");
        }

        internal static (List<PairedHalfEdgeSet> paired, List<HalfEdgeSet> remainders) PairHalfEdgesGreedy(IEnumerable<HalfEdgeSet> input)
        {
            var paired = new List<PairedHalfEdgeSet>();
            var pool = new List<HalfEdgeSet>(input);
            var progress = true;
            while (progress)
            {
                progress = false;
                for (int i = 0; i < pool.Count; i++)
                {
                    var a = pool[i];
                    for (int j = i + 1; j < pool.Count; j++)
                    {
                        var b = pool[j];
                        if (!TryMatch(a, b, out var matched, out var remaindersA, out var remaindersB))
                            continue;

                        paired.AddRange(matched);

                        // Remove originals
                        pool.RemoveAt(j);
                        pool.RemoveAt(i);

                        // Add remainders back to pool
                        if (remaindersA != null)
                            pool.AddRange(remaindersA);
                        if (remaindersB != null)
                            pool.AddRange(remaindersB);

                        progress = true;
                        break;
                    }
                    if (progress) break;
                }
            }
            return (paired, pool);
        }

        internal static bool TryMatch(HalfEdgeSet src, HalfEdgeSet dest, out List<PairedHalfEdgeSet> paired, out List<HalfEdgeSet> srcRemainders, out List<HalfEdgeSet> destRemainders)
        {
            paired = null;
            srcRemainders = null;
            destRemainders = null;

            MatchRange mr;
            try
            {
                if (!TryFindMatchingRange(src, dest, out mr))
                {
                    return false;
                }
            }
            catch (NotImplementedException)
            {
                return false;
            }

            var (srcLeft, srcMid, srcRight) = Subdivide(src.Line, mr.SrcTMin, mr.SrcTMax);
            var (destLeft, destMid, destRight) = Subdivide(dest.Line, mr.DestTMin, mr.DestTMax);

            paired = new List<PairedHalfEdgeSet>();
            srcRemainders = new List<HalfEdgeSet>();
            destRemainders = new List<HalfEdgeSet>();

            if (srcMid != null && destMid != null)
            {
                
                paired.Add(new PairedHalfEdgeSet
                {
                    SrcSectionIndex = src.SectionIndex,
                    DestSectionIndex = dest.SectionIndex,
                    SrcCell = src.Cell,
                    DestCell = dest.Cell,
                    SrcDir = src.Dir,
                    DestDir = dest.Dir,
                    V0 = src.V0,
                    V1 = src.V1,
                    SrcStrideT = src.StrideT,
                    DestStrideT = dest.StrideT,
                    SrcLine = srcMid,
                    DestLine = destMid,
                });
            }

            void AddIf(LatticeLine part, HalfEdgeSet template, List<HalfEdgeSet> outList)
            {
                if (part != null)
                {
                    outList.Add(new HalfEdgeSet
                    {
                        SectionIndex = template.SectionIndex,
                        Cell = template.Cell,
                        Dir = template.Dir,
                        V0 = template.V0,
                        V1 = template.V1,
                        StrideT = template.StrideT,
                        Line = part,
                    });
                }
            }

            AddIf(srcLeft, src, srcRemainders);
            AddIf(srcRight, src, srcRemainders);
            AddIf(destLeft, dest, destRemainders);
            AddIf(destRight, dest, destRemainders);

            return paired.Count > 0;
        }
    }
}
