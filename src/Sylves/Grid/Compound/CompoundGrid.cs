using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

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

        public CompoundSection(MeshData meshData, Vector2 strideX, Vector2 strideY, List<HalfPlane> halfPlanes)
        {
            MeshData = meshData;
            StrideX = strideX;
            StrideY = strideY;
            HalfPlanes = halfPlanes;
        }

        public CompoundSection(MeshData meshData)
        {
            MeshData = meshData;
            StrideX = new Vector2(1, 0);
            StrideY = new Vector2(0, 1);
            // Use 4 half plans to restrict to just (0, 0)
            HalfPlanes = new List<HalfPlane>
            {
                new HalfPlane { A = 1, B = 0, C = 0 },
                new HalfPlane { A = 0, B = 1, C = 0 },
                new HalfPlane { A = -1, B = 0, C = 0 },
                new HalfPlane { A = 0, B = -1, C = 0 },
            };
        }

        public bool Test(Cell cell)
        {
            foreach (var hp in HalfPlanes)
            {
                if (!hp.Test(cell.y, cell.z))
                    return false;
            }
            return true;
        }

        public static CompoundSection operator *(Matrix4x4 matrix, CompoundSection cs)
        {
            return new CompoundSection(matrix * cs.MeshData,
                VectorUtils.ToVector2(matrix.MultiplyVector(VectorUtils.ToVector3(cs.StrideX))),
                VectorUtils.ToVector2(matrix.MultiplyVector(VectorUtils.ToVector3(cs.StrideY))),
                cs.HalfPlanes);
        }
    }

    public class CompoundGrid : IGrid
    {
        public class CompoundBound : IBound
        {
            public SquareBound[] SectionBounds;
        }

        internal class HalfEdgeSet
        {
            public Int32 SectionIndex {get;set;}
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
            public Int32 SrcSectionIndex {get;set;}
            public Int32 DestSectionIndex {get;set;}
            public Cell SrcCell {get;set;}
            public CellDir SrcDir {get;set;}
            public Cell DestCell {get;set;}
            public CellDir DestDir {get;set;}
            public Vector2 V0 {get;set;}
            public Vector2 V1 {get;set;}
            
            public Vector2 SrcStrideT { get; set; }
            public Vector2 DestStrideT { get; set; }

            // Affine parameter mappings within the matched interval
            // DestT = DestFromSrcAlpha + DestFromSrcBeta * SrcT
            // SrcT  = SrcFromDestAlpha + SrcFromDestBeta * DestT
            public int DestFromSrcAlpha { get; set; }
            public int DestFromSrcBeta { get; set; }
            public int SrcFromDestAlpha { get; set; }
            public int SrcFromDestBeta { get; set; }

            public LatticeLine SrcLine {get;set;}
            public LatticeLine DestLine {get;set;}
        }

        private readonly List<CompoundSection> sections;
        private readonly List<HalfEdgeSet> halfEdges = new List<HalfEdgeSet>();
        private readonly List<ICellType> cellTypes;
        private readonly Int32 cellsPerSection;
        private readonly List<PeriodicPlanarMeshGrid> grids = new List<PeriodicPlanarMeshGrid>();
        private readonly List<PairedHalfEdgeSet> pairedHalfEdges;
        private readonly CompoundBound bound;
        // Applies bound to grids.
        private readonly List<PeriodicPlanarMeshGrid> boundedGrids;

        public CompoundGrid(List<CompoundSection> sections)
        {
            if(sections == null) throw new ArgumentNullException(nameof(sections));

            this.sections = sections;
            for (var sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
            {
                var section = sections[sectionIndex];
                var grid = new PeriodicPlanarMeshGrid(section.MeshData, section.StrideX, section.StrideY);
                grids.Add(grid);
                halfEdges.AddRange(GetHalfEdges(sectionIndex, section, grid));
            }

            var pr = PairHalfEdgesGreedy(halfEdges);
            pairedHalfEdges = pr.paired;
            halfEdges = pr.remainders;

            cellTypes = grids.SelectMany(x=>x.GetCellTypes()).Distinct().ToList();
            cellsPerSection = grids.Select(x=>x.BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(1, 1))).GetCells().Count()).Max();

            boundedGrids = grids;
        }

        private CompoundGrid(CompoundGrid other, CompoundBound bound)
        {
            this.sections = other.sections;
            this.halfEdges = other.halfEdges;
            this.cellTypes = other.cellTypes;
            this.cellsPerSection = other.cellsPerSection;
            this.grids = other.grids;
            this.pairedHalfEdges = other.pairedHalfEdges;
            this.bound = bound;
            if (bound == null)
            {
                boundedGrids = grids;
            }
            else
            {
                if (bound.SectionBounds.Length != sections.Count)
                    throw new ArgumentException($"Expected {sections.Count} section bounds, got {bound.SectionBounds.Length}");
                boundedGrids = new List<PeriodicPlanarMeshGrid>();
                for (var i = 0; i < sections.Count; i++)
                {
                    var grid = grids[i];
                    var b = bound.SectionBounds[i];
                    if (b != null)
                        grid = (PeriodicPlanarMeshGrid)grid.BoundBy(b);
                    boundedGrids.Add(grid);
                }
            }
        }

        #region Construction helpers

        private static List<HalfEdgeSet> GetHalfEdges(Int32 sectionIndex, CompoundSection section, PeriodicPlanarMeshGrid grid)
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
                    var chunkDelta = srcChunk - destChunk;

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

        private static (LatticeLine left, LatticeLine middle, LatticeLine right) Subdivide(LatticeLine line, int? matchMin, int? matchMax)
        {
            var leftMin = line.TMin;
            var leftMax = matchMin - 1;
            var rightMin = matchMax + 1;
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
            // DestT = DestFromSrcAlpha + DestFromSrcBeta * SrcT
            // SrcT  = SrcFromDestAlpha + SrcFromDestBeta * DestT
            public int DestFromSrcAlpha;
            public int DestFromSrcBeta;
            public int SrcFromDestAlpha;
            public int SrcFromDestBeta;
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
                range.DestFromSrcAlpha = uInt - tInt; range.DestFromSrcBeta = 1;
                range.SrcFromDestAlpha = tInt - uInt; range.SrcFromDestBeta = 1;
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
                range.DestTMin = tMin - k;
                range.DestTMax = tMax - k;
                range.DestFromSrcAlpha = -k; range.DestFromSrcBeta = 1;
                range.SrcFromDestAlpha = k; range.SrcFromDestBeta = 1;
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
                range.DestTMin = k - tMax;
                range.DestTMax = k - tMin;
                range.DestFromSrcAlpha = k; range.DestFromSrcBeta = -1;
                range.SrcFromDestAlpha = k; range.SrcFromDestBeta = -1;
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
                for (var i = 0; i < pool.Count; i++)
                {
                    var a = pool[i];
                    for (var j = i + 1; j < pool.Count; j++)
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
                    DestFromSrcAlpha = mr.DestFromSrcAlpha,
                    DestFromSrcBeta = mr.DestFromSrcBeta,
                    SrcFromDestAlpha = mr.SrcFromDestAlpha,
                    SrcFromDestBeta = mr.SrcFromDestBeta,
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
        #endregion

        private (Int32, Cell) Split(Cell cell)
        {
            var (sectionIndex, localX) = Encoding.RavelDecode(cell.x, cellsPerSection);
            return (
                (Int32)sectionIndex,
                new Cell(localX, cell.y, cell.z)
                );
        }

        private Cell Combine(int sectionIndex, Cell cell)
        {
            return new Cell(Encoding.RavelEncode(sectionIndex, cell.x, cellsPerSection), cell.y, cell.z);
        }


        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => false;

        public bool IsSingleCellType => cellTypes.Count == 1;

        public Int32 CoordinateDimension => 3;

        public IEnumerable<ICellType> GetCellTypes() => cellTypes;
        #endregion

        #region Relatives
        public IGrid Unbounded => throw new NotImplementedException();

        public IGrid Unwrapped => throw new NotImplementedException();

        public IDualMapping GetDual() => throw new NotImplementedException();

        public IGrid GetDiagonalGrid() => throw new NotImplementedException();

        public IGrid GetCompactGrid() => throw new NotImplementedException();

        public IGrid Recenter(Cell cell) => throw new NotImplementedException();

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => boundedGrids.SelectMany((x, i) => x.GetCells().Select(c => Combine(i, c)));

        public ICellType GetCellType(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            return grids[sectionIndex].GetCellType(localCell);
        }

        public bool IsCellInGrid(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            if (sectionIndex < 0 || sectionIndex >= grids.Count) return false;
            return sections[sectionIndex].Test(localCell) && grids[sectionIndex].IsCellInGrid(localCell);
        }
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (sectionIndex, localCell) = Split(cell);
            // First try and move within grid
            var grid = boundedGrids[sectionIndex];
            if (grid.TryMove(localCell, dir, out var localDest, out inverseDir, out connection))
            {
                if (sections[sectionIndex].Test(localDest))
                {
                    dest = Combine(sectionIndex, localDest);
                    return true;
                }
            }
            // Check for paired half-edges
            foreach (var pe in pairedHalfEdges)
            {
                int t;
                if (pe.SrcSectionIndex == sectionIndex && pe.SrcDir == dir && pe.SrcCell.x == localCell.x && pe.SrcLine.TryGet(new Vector2Int(localCell.y, localCell.z), out t))
                {
                    // Map t to dest t
                    var destT = pe.DestFromSrcAlpha + pe.DestFromSrcBeta * t;
                    var b = pe.DestLine.TryGet(destT, out var destPoint);
                    if(!b) throw new Exception("Mapping failure");
                    var destCell = new Cell(pe.DestCell.x, destPoint.x, destPoint.y);
                    dest = Combine(pe.DestSectionIndex, destCell);
                    inverseDir = pe.DestDir;
                    connection = new Connection();
                    return sections[pe.DestSectionIndex].Test(destCell);
                }
                if (pe.DestSectionIndex == sectionIndex && pe.DestDir == dir && pe.DestCell.x == localCell.x && pe.DestLine.TryGet(new Vector2Int(localCell.y, localCell.z), out t))
                {
                    // Map t to src t
                    var srcT = pe.SrcFromDestAlpha + pe.SrcFromDestBeta * t;
                    var b = pe.SrcLine.TryGet(srcT, out var srcPoint);
                    if (!b) throw new Exception("Mapping failure");
                    var destCell = new Cell(pe.SrcCell.x, srcPoint.x, srcPoint.y);
                    dest = Combine(pe.SrcSectionIndex, destCell);
                    inverseDir = pe.SrcDir;
                    connection = new Connection();
                    return sections[pe.SrcSectionIndex].Test(destCell);
                }
            }
            dest = default;
            inverseDir = default;
            connection = default;
            return false;
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation) => throw new NotImplementedException();


        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            return boundedGrids[sectionIndex].GetCellDirs(localCell);
        }

        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            return boundedGrids[sectionIndex].GetCellCorners(localCell);
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount => throw new NotImplementedException();

        public int GetIndex(Cell cell) => throw new NotImplementedException();

        public Cell GetCellByIndex(int index) => throw new NotImplementedException();
        #endregion

        #region Bounds
        public IBound GetBound() => bound;

        public IBound GetBound(IEnumerable<Cell> cells)
        {
            return new CompoundBound
            {
                SectionBounds = cells
                    .Select(Split)
                    .GroupBy(x => x.Item1, (k, g) => g.Count() == 0 ? new SquareBound(0, 0, -1, -1) : (SquareBound)grids[k].GetBound(g.Select(x => x.Item2)))
                    .ToArray()
            };
            
        }

        public IGrid BoundBy(IBound bound)
        {
            if (this.bound != null)
                bound = IntersectBounds(this.bound, bound);

            return new CompoundGrid(this, (CompoundBound)bound);
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return new CompoundBound
            {
                SectionBounds = ((CompoundBound)bound).SectionBounds.Zip(((CompoundBound)other).SectionBounds, (a, b) => a.Intersect(b)).ToArray()
            };
        }

        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null || other == null)
            {
                return null;
            }
            return new CompoundBound
            {
                SectionBounds = ((CompoundBound)bound).SectionBounds.Zip(((CompoundBound)other).SectionBounds, (a, b) => a.Union(b)).ToArray()
            };
        }

        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            var sectionBounds = ((CompoundBound)bound).SectionBounds;
            for (var i = 0; i < sectionBounds.Length; i++)
            {
                var grid = boundedGrids[i];
                var sectionBound = sectionBounds[i];
                foreach (var cell in grid.GetCellsInBounds(bound))
                {
                    if (sections[i].Test(cell))
                        yield return Combine(i, cell);
                }
            }
        }

        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            var sectionBound = ((CompoundBound)bound).SectionBounds[sectionIndex];
            return grid.IsCellInBound(localCell, sectionBound);
        }

        public Aabb? GetBoundAabb(IBound bound)
        {
            var sectionBounds = ((CompoundBound)bound).SectionBounds;
            return Aabb.Union(sectionBounds.Select((b, i) => boundedGrids[i].GetBoundAabb(b)));
        }

        #endregion

        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetCellCenter(localCell);
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetCellCorner(localCell, cellCorner);
        }

        public TRS GetTRS(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetTRS(localCell);
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetDeformation(localCell);
        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            grid.GetPolygon(localCell, out vertices, out transform);
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetTriangleMesh(localCell);
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            grid.GetMeshData(localCell, out meshData, out transform);
        }

        public Aabb GetAabb(Cell cell)
        {
            var (sectionIndex, localCell) = Split(cell);
            var grid = boundedGrids[sectionIndex];
            return grid.GetAabb(localCell);
        }

        public Aabb GetAabb(IEnumerable<Cell> cells) => DefaultGridImpl.GetAabb(this, cells);

        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            for (var i = 0; i < boundedGrids.Count; i++)
            {
                var grid = boundedGrids[i];
                if (grid.FindCell(position, out var localCell))
                {
                    cell = Combine(i, localCell);
                    return sections[i].Test(localCell);
                }
            }
            cell = default;
            return false;
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            for (var i = 0; i < boundedGrids.Count; i++)
            {
                var grid = boundedGrids[i];
                if (grid.FindCell(matrix, out var localCell, out var localRotation))
                {
                    cell = Combine(i, localCell);
                    rotation = localRotation;
                    return sections[i].Test(localCell);
                }
            }
            cell = default;
            rotation = default;
            return false;

        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            for (var i = 0; i < boundedGrids.Count; i++)
            {
                var grid = boundedGrids[i];
                var section = sections[i];
                foreach (var localCell in grid.GetCellsIntersectsApprox(min, max))
                {
                    if (section.Test(localCell))
                        yield return Combine(i, localCell);
                }
            }
        }

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            // Run raycast on each bounded grid and collect results
            var raycastResults = new List<IEnumerable<RaycastInfo>>();
            
            for (var i = 0; i < boundedGrids.Count; i++)
            {
                var grid = boundedGrids[i];
                var section = sections[i];
                
                // Get raycast results from this grid
                var gridResults = grid.Raycast(origin, direction, maxDistance)
                    .Where(ri => 
                    {
                        return section.Test(ri.cell);
                    })
                    .Select(ri => new RaycastInfo
                    {
                        cell = Combine(i, ri.cell),
                        point = ri.point,
                        distance = ri.distance,
                        cellDir = ri.cellDir
                    });
                
                raycastResults.Add(gridResults);
            }
            
            return MergeRaycastResults(raycastResults);
        }
        
        private IEnumerable<RaycastInfo> MergeRaycastResults(List<IEnumerable<RaycastInfo>> raycastResults)
        {
            // Lazily merge multiple (potentially infinite) sorted enumerables by distance.
            // Assumes each input enumerable yields RaycastInfo in non-decreasing distance order.
            // No heap used; sections list is small.
            var enumerators = new List<IEnumerator<RaycastInfo>>(raycastResults.Count);
            var hasCurrent = new List<bool>(raycastResults.Count);
            var currents = new List<RaycastInfo>(raycastResults.Count);

            // Initialize enumerators and advance once to get the current heads
            foreach (var seq in raycastResults)
            {
                var e = seq.GetEnumerator();
                enumerators.Add(e);
                var moved = e.MoveNext();
                hasCurrent.Add(moved);
                currents.Add(moved ? e.Current : default);
            }

            while (true)
            {
                // Find the index of the smallest current distance among active enumerators
                var minIndex = -1;
                var minDistance = float.PositiveInfinity;
                for (var i = 0; i < enumerators.Count; i++)
                {
                    if (!hasCurrent[i]) continue;
                    var d = currents[i].distance;
                    if (d < minDistance)
                    {
                        minDistance = d;
                        minIndex = i;
                    }
                }

                if (minIndex == -1)
                {
                    // All enumerators exhausted
                    yield break;
                }

                // Yield the min current
                var result = currents[minIndex];
                yield return result;

                // Advance that enumerator
                var movedNext = enumerators[minIndex].MoveNext();
                hasCurrent[minIndex] = movedNext;
                if (movedNext)
                {
                    currents[minIndex] = enumerators[minIndex].Current;
                }
                else
                {
                    currents[minIndex] = default;
                }
            }
        }
        #endregion

        #region Symmetry
        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => throw new NotImplementedException();

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => throw new NotImplementedException();
        #endregion
    }
}
