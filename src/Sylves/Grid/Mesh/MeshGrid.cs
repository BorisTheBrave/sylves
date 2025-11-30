using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;
using static Sylves.MeshGridUtils;

namespace Sylves
{
    public class MeshCellData : DataDrivenCellData
    {
        /// <summary>
        /// Details of the original face that this cell derives from 
        /// </summary>
        public MeshUtils.Face Face { get; set; }

        /// <summary>
        /// For 3d cells, information about how the 2d face was extended into 3d. 
        /// </summary>
        public PrismInfo PrismInfo { get; set; }
    }

    /// <summary>
    /// Represents a 2d grid, where each cell corresponds to a face in a given mesh.
    /// </summary>
    public class MeshGrid : DataDrivenGrid
    {
        // Epsilon for judging if something is in the plane XY
        private const float PlanarThickness = 1e-35f;

        private MeshDetails meshDetails;
        protected Vector3 min, max;
        protected readonly MeshData meshData;
        private readonly MeshGridOptions meshGridOptions;
        protected readonly bool is2d;

        // If set, is always contained by meshDetails.hashCellBounds
        protected readonly CubeBound bound;

        public MeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) :
            base(MeshGridBuilder.Build(meshData, meshGridOptions ?? new MeshGridOptions()))
        {
            this.meshData = meshData;
            this.meshGridOptions = meshGridOptions ?? new MeshGridOptions();
            BuildMeshDetails();
            is2d = true;
        }

        // Internals constructor used for other meshgrid like grids.
        // You need to call BuildMeshDetails() after calling this.
        internal MeshGrid(MeshData meshData, MeshGridOptions meshGridOptions, DataDrivenData data, bool is2d) :
            base(data)
        {
            this.meshData = meshData;
            this.meshGridOptions = meshGridOptions;
            this.is2d = is2d;
        }

        // Copy constructor
        protected MeshGrid(MeshGrid meshGrid, CubeBound bound) :
            base(new DataDrivenData { Cells = meshGrid.CellData, Moves = meshGrid.Moves})
        {
            this.meshData = meshGrid.meshData;
            this.meshGridOptions = meshGrid.meshGridOptions;
            this.is2d = meshGrid.is2d;
            this.meshDetails = meshGrid.meshDetails;
            this.bound = bound;
        }

        #region Impl

        protected virtual (Vector3, Vector3) ComputeBounds(Cell cell)
        {
            var meshCellData = (MeshCellData)CellData[cell];

            var face = meshCellData.Face;
            var cellMin = meshData.vertices[face[0]];
            var cellMax = cellMin;
            for (var i = 1; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                cellMin = Vector3.Min(cellMin, v);
                cellMax = Vector3.Max(cellMax, v);
            }
            return (cellMin, cellMax);
        }

        internal void BuildMeshDetails()
        {
            // Absolute min thickness. Start getting issues with planes at origin if smaller  this
            var hashCellSize = new Vector3(PlanarThickness, PlanarThickness, PlanarThickness);

            // Now compute hashCellSize based on largest cell
            Vector3? min = null;
            Vector3? max = null;
            foreach (var cell in GetCells())
            {
                var (cellMin, cellMax) = ComputeBounds(cell);
                var dim = cellMax - cellMin;
                hashCellSize = Vector3.Max(hashCellSize, dim);
                min = min == null ? cellMin : Vector3.Min(min.Value, cellMin);
                max = max == null ? cellMax : Vector3.Max(max.Value, cellMax);
            }
            min = this.min = min ?? new Vector3();
            max = this.max = max ?? new Vector3();

            // Also, hashCellSize must be larger than floating point precision
            // This avoids issues with non-origin planes
            hashCellSize = Vector3.Max(hashCellSize, Vector3.Max(Abs(min.Value), Abs(max.Value)) * 1e-8f);

            var meshDetails = new MeshDetails
            {
                hashCellSize = hashCellSize,
                hashCellBase = min.Value,
                hashedCells = new Dictionary<Vector3Int, List<Cell>>(),
                isPlanar = min.HasValue && min.Value.z == max.Value.z,
            };
            Vector3Int? hashCellMin = null;
            Vector3Int? hashCellMax = null;

            foreach (var cell in GetCells())
            {
                var cellTrs = GetTRS(cell);
                var hashCell = meshDetails.GetHashCell(cellTrs.Position);
                if (!meshDetails.hashedCells.TryGetValue(hashCell, out var cellList))
                {
                    cellList = meshDetails.hashedCells[hashCell] = new List<Cell>();
                }
                cellList.Add(cell);
                hashCellMin = hashCellMin == null ? hashCell : Vector3Int.Min(hashCellMin.Value, hashCell);
                hashCellMax = hashCellMax == null ? hashCell : Vector3Int.Max(hashCellMax.Value, hashCell);
            }
            if (hashCellMin == null)
            {
                hashCellMin = new Vector3Int();
                hashCellMax = -Vector3Int.one;
            }
            hashCellMin = hashCellMin ?? new Vector3Int();
            hashCellMax = hashCellMax ?? new Vector3Int();
            meshDetails.hashCellBounds = new CubeBound(hashCellMin.Value, hashCellMax.Value + Vector3Int.one);
            meshDetails.expandedHashCellBounds = new CubeBound(hashCellMin.Value - Vector3Int.one, hashCellMax.Value + Vector3Int.one + Vector3Int.one);

            this.meshDetails = meshDetails;
        }

        // Structure caching some additional data about the mesh
        // Every cell is assigned a unique hash cell, and the hash cells are large enough
        // that the bounds of individual cells are contained by the hash cells +-1 away from that
        // hash cell in each axis.
        private class MeshDetails
        {
            public Vector3 hashCellSize;
            public Vector3 hashCellBase;
            public CubeBound hashCellBounds;
            public CubeBound expandedHashCellBounds;
            public Dictionary<Vector3Int, List<Cell>> hashedCells;
            public bool isPlanar;

            public Vector3Int GetHashCell(Vector3 v)
            {
                // Planar meshes tend to produce overflow values here.
                // Also, many other routines add/subtract one from these values.
                // So we're careful to clip things in a way that avoids problems
                int FloorToInt(float x)
                {
                    return x < Int32.MinValue + 1 ? Int32.MinValue + 1 :
                        x> Int32.MaxValue - 1 ? Int32.MaxValue - 1 :
                        (Int32)x;
                }
                return new Vector3Int(
                    FloorToInt((v.x - hashCellBase.x) / hashCellSize.x),
                    FloorToInt((v.y - hashCellBase.y) / hashCellSize.y),
                    FloorToInt((v.z - hashCellBase.z) / hashCellSize.z)
                    );
            }
        }

        private CubeBound ExpandBound(CubeBound bound)
        {
            return new CubeBound(bound.Min - Vector3Int.one, bound.Mex + Vector3Int.one);
        }

        private Aabb ExpandAabb(Aabb aabb)
        {
            var min = meshDetails.GetHashCell(aabb.Min);
            var max = meshDetails.GetHashCell(aabb.Max);
            // Note that max + 2 is not overflow safe, so we do the operation in floating point.
            return Aabb.FromMinMax(
                Vector3.Scale(min, meshDetails.hashCellSize) - Vector3.Scale(Vector3Int.one, meshDetails.hashCellSize) + meshDetails.hashCellBase,
                Vector3.Scale(max, meshDetails.hashCellSize) + Vector3.Scale(2 * Vector3Int.one, meshDetails.hashCellSize) + meshDetails.hashCellBase
                );
        }
        #endregion


        #region Basics

        public override bool Is2d => is2d;

        public override bool Is3d => !is2d;

        public override bool IsPlanar => is2d && meshDetails.isPlanar;

        public override Int32 CoordinateDimension => is2d ? (meshData.subMeshCount == 1 ? 1 : 2) : 3;
        #endregion

        #region Relatives

        public override IGrid Unbounded => new MeshGrid(this, null);

        public override IDualMapping GetDual()
        {
            var dmb = new DualMeshBuilder(meshData);
            var dualGrid = new MeshGrid(dmb.DualMeshData, meshGridOptions);
            return new DualMapping(this, dualGrid, dmb.Mapping);
        }

        private class DualMapping : BasicDualMapping
        {
            Dictionary<(Cell, CellCorner), (Cell, CellCorner)> toDual;
            Dictionary<(Cell, CellCorner), (Cell, CellCorner)> toBase;

            public DualMapping(MeshGrid baseGrid, MeshGrid dualGrid, List<(int primalFace, int primalVert, int dualFace, int dualVert)> rawMapping) : base(baseGrid, dualGrid)
            {
                toDual = rawMapping.ToDictionary(
                    x => (new Cell(x.primalFace, 0, 0), (CellCorner)x.primalVert),
                    x => (new Cell(x.dualFace, 0, 0), (CellCorner)x.dualVert));
                toBase = toDual.ToDictionary(kv => kv.Value, kv => kv.Key);
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner)
            {
                if (toDual.TryGetValue((cell, corner), out var r))
                {
                    return r;
                }
                else
                {
                    return null;
                }
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner)
            {

                if (toBase.TryGetValue((cell, corner), out var r))
                {
                    return r;
                }
                else
                {
                    return null;
                }
            }
        }

        override public IGrid GetDiagonalGrid()
        {
            if (!Is2d) throw new NotImplementedException();

            var diagCellData = new Dictionary<Cell, DataDrivenCellData>();
            var diagMovesByPair = new Dictionary<(Cell, Cell), CellDir>();
            var dualMapping = GetDual();
            var dualGrid = (MeshGrid)dualMapping.DualGrid;
            foreach (var cell in GetCells())
            {
                var cellData = (MeshCellData)CellData[cell];
                var n = cellData.Face.Count;
                var diagCount = 0;
                for (var i = 0; i < n; i++)
                {
                    // Start from corner 1 as the second loop effectively steps back by one
                    var dualPair = dualMapping.ToDualPair(cell, (CellCorner)((i + 1) % n));
                    if (dualPair == null)
                        continue;
                    var (dualCell, inverseCorner) = dualPair.Value;
                    var m = ((MeshCellData)dualGrid.CellData[dualCell]).Face.Count;
                    // Find all cells adjacent to this dual cell, starting from the original cell,
                    // and skipping first (the original cell) and last (will be covered by next iteration of i)
                    for (var j = 1; j < m - 1; j++)
                    {
                        var basePair = dualMapping.ToBasePair(dualCell, (CellCorner)(((int)inverseCorner + j) % m));
                        if (basePair == null)
                            continue;
                        var (baseCell, _) = basePair.Value;

                        diagMovesByPair[(cell, baseCell)] = (CellDir)(diagCount++);

                    }
                }
                diagCellData[cell] = new MeshCellData
                {
                    CellType = NoRotationCellType.Get(NGonCellType.Get(diagCount)),
                    Deformation = cellData.Deformation,
                    Face = cellData.Face,
                    TRS = cellData.TRS,
                };
            }

            // Fill in inverse dirs
            var diagMoves = new Dictionary<(Cell, CellDir), (Cell, CellDir, Connection)>();
            foreach (var kv in diagMovesByPair)
            {
                if(diagMovesByPair.TryGetValue((kv.Key.Item2, kv.Key.Item1), out var other))
                {
                    diagMoves[(kv.Key.Item1, kv.Value)] = (kv.Key.Item2, other, new Connection());
                    diagMoves[(kv.Key.Item2, other)] = (kv.Key.Item1, kv.Value, new Connection());
                }
            }

            return new MeshGrid(meshData, meshGridOptions, new DataDrivenData { Cells = diagCellData, Moves = diagMoves }, is2d);
        }

        public override IGrid GetCompactGrid() => CoordinateDimension <= 2 ? this : throw new NotImplementedException();

        #endregion

        #region Cell info

        public virtual bool IsCellInGrid(Cell cell) => base.IsCellInGrid(cell) && IsCellInBound(cell, bound);
        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            // NB: We test IsCellInBound and not IsCellInGrid as this permits "improper" grids with moves outside
            // the grid, a feature some other grid impls rely on.
            return base.TryMove(cell, dir, out dest, out inverseDir, out connection) && IsCellInBound(dest, bound);
        }


        // TODO: Pathfind on mesh, without involving layers
        //public override IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell);
        #endregion

        #region Bounds
        public override IBound GetBound() => bound;
        public override IBound GetBound(IEnumerable<Cell> cells)
        {
            var hashCells = cells.Select(c => meshDetails.GetHashCell(GetCellCenter(c)));
            return CubeBound.FromVectors(hashCells);
        }
        public override IGrid BoundBy(IBound bound)
        {
            return new MeshGrid(this, (CubeBound)IntersectBounds(this.bound, bound));
        }
        public override IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((CubeBound)bound).Intersect((CubeBound)other);
        }
        public override IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((CubeBound)bound).Union((CubeBound)other);
        }
        public override IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (CubeBound)bound;
        }
        public override bool IsCellInBound(Cell cell, IBound bound)
        {
            if (bound == null) return true;
            var cb = (CubeBound)bound;
            return cb.Contains((Cell)meshDetails.GetHashCell(GetCellCenter(cell)));
        }
        public override Aabb? GetBoundAabb(IBound bound)
        {
            if (bound is CubeBound cb)
            {
                return Aabb.FromMinMax(
                    Vector3.Scale((Vector3)(cb.Min - Vector3Int.one), meshDetails.hashCellSize) + meshDetails.hashCellBase,
                    Vector3.Scale((Vector3)(cb.Mex + Vector3Int.one), meshDetails.hashCellSize) + meshDetails.hashCellBase);
            }
            else
            {
                return Aabb.FromMinMax(
                    Vector3.Scale((Vector3)meshDetails.expandedHashCellBounds.Min, meshDetails.hashCellSize) + meshDetails.hashCellBase,
                    Vector3.Scale((Vector3)meshDetails.expandedHashCellBounds.Mex, meshDetails.hashCellSize) + meshDetails.hashCellBase);

            }
        }
        #endregion

        #region Position
        public override Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            GetPolygon(cell, out var vertices, out var transform);
            var i = MeshGridBuilder.CellCornerToVertexIndex(corner, ((MeshCellData)CellData[cell]).Face.Count, meshGridOptions.DoubleOddFaces);
            return transform.MultiplyPoint3x4(vertices[i]);

        }

        #endregion

        #region Query


        // FindCell, applied to a single cell
        protected virtual bool IsPointInCell(Vector3 position, Cell cell)
        {
            // Currently does fan detection
            // Doesn't work for convex faces
            var cellData = (MeshCellData)CellData[cell];
            var face = cellData.Face;
            var v0 = meshData.vertices[face[0]];
            var prev = meshData.vertices[face[1]];
            for (var i = 2; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                if (GeometryUtils.IsPointInTriangle(position, v0, prev, v, PlanarThickness))
                    return true;
                prev = v;
            }
            return false;
        }

        public override bool FindCell(Vector3 position, out Cell cell)
        {
            // TODO: Maybe search the central hashcell first?
            foreach (var c in GetCellsIntersectsApprox(position, position))
            {
                if(IsPointInCell(position, c))
                {
                    cell = c;
                    return true;
                }
            }
            cell = default;
            return false;
        }

        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            // Report the cell the origin starts in.
            // This is a special case as RaycastCell is dumb and doesn't always report it.
            bool hasOriginCell = FindCell(origin, out var originCell);
            if (hasOriginCell)
            {
                yield return new RaycastInfo
                {
                    cell = originCell,
                    distance = 0,
                    point = origin,
                };
            }

            // Broadphase - walk through the hashCells looking for cells to check.
            var bfByBound = bound == null ? meshDetails.expandedHashCellBounds : ExpandBound(bound);
            var bfRaycastInfos = CubeGrid.Raycast(origin - meshDetails.hashCellBase, direction, maxDistance, meshDetails.hashCellSize, bfByBound);
            Vector3Int? prevHashCell = null;
            var queuedRaycastInfos = new PriorityQueue<RaycastInfo>(x=>x.distance, (x, y) => -x.distance.CompareTo(y.distance));
            foreach (var bfRaycastInfo in bfRaycastInfos)
            {
                // Find check every hashCell within one of the cast to cell
                // Excluding ones we've already checked
                // So this is a bit like a simplified shapecast, rather than a raycast.
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        for (var z = -1; z <= 1; z++)
                        {
                            var hashCell = ((Vector3Int)bfRaycastInfo.cell) + new Vector3Int(x, y, z);
                            if (prevHashCell != null)
                            {
                                var diffX = hashCell.x - prevHashCell.Value.x;
                                var diffY = hashCell.y - prevHashCell.Value.y;
                                var diffZ = hashCell.z - prevHashCell.Value.z;
                                if (Math.Abs(diffX) <= 1 && Math.Abs(diffY) <= 1 && Math.Abs(diffZ) <= 1)
                                {
                                    // We've already checked this cell, skip
                                    continue;
                                }
                            }

                            queuedRaycastInfos.AddRange(RaycastHashCell(hashCell, origin, direction, maxDistance));
                        }
                    }
                }

                // Find the distance such that all raycastInfos smaller than this distance have already found,
                // meaning it is safe to stream them out of the queue without getting anything out of order
                var minDistance = bfRaycastInfo.distance - Mathf.Min(
                        Mathf.Abs(meshDetails.hashCellSize.x / direction.x),
                        Mathf.Abs(meshDetails.hashCellSize.y / direction.y),
                        Mathf.Abs(meshDetails.hashCellSize.z / direction.z)
                    );

                // Actually stream all the safe raycastInfos
                foreach(var ri in  queuedRaycastInfos.Drain(minDistance))
                {
                    if (hasOriginCell && originCell == ri.cell)
                        continue;
                    yield return ri;
                }

                prevHashCell = (Vector3Int)bfRaycastInfo.cell;
            }

            // We've found all raycast infos, stream out any that haven't already been sent
            foreach (var ri in queuedRaycastInfos.Drain())
            {
                if (hasOriginCell && originCell == ri.cell)
                    continue;
                yield return ri;
            }
        }

        private IEnumerable<RaycastInfo> RaycastHashCell(Vector3Int hashCell, Vector3 rayOrigin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            if (meshDetails.hashedCells.TryGetValue(hashCell, out var cells))
            {
                foreach (var cell in cells)
                {
                    var raycastInfo = RaycastCell(cell, rayOrigin, direction);
                    if (raycastInfo != null && raycastInfo.Value.distance <= maxDistance)
                    {
                        yield return raycastInfo.Value;
                    }
                }
            }
        }

        // Narrow phase
        // I.e. raycast, applied to a single cell
        // Unlike raycast, this doesn't catch rays that start in the cell.
        protected virtual RaycastInfo? RaycastCell(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            // Detect special planar case
            if (IsPlanar && direction.z == 0)
            {
                return RaycastCell2D(cell, rayOrigin, direction);
            }
            // Normal 3d raycast
            // Currently does fan detection
            // Doesn't work for convex faces
            var cellData = CellData[cell] as MeshCellData;
            var face = cellData.Face;
            var v0 = meshData.vertices[face[0]];
            var prev = meshData.vertices[face[1]];
            for (var i = 2; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                if (MeshRaycast.RaycastTri(rayOrigin, direction, v0, prev, v, out var point, out var distance))
                {
                    return new RaycastInfo
                    {
                        cell = cell,
                        cellDir = null,
                        distance = distance,
                        point = point,
                    };
                }
                prev = v;
            }
            return null;
        }

        private RaycastInfo? RaycastCell2D(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            var cellData = CellData[cell] as MeshCellData;
            var face = cellData.Face;
            var prev = meshData.vertices[face[face.Count - 1]];
            var bestD = float.PositiveInfinity;
            var bestP = new Vector3();
            var bestI = 0;
            for (var i = 0; i < face.Count; i++)
            {
                var curr = meshData.vertices[face[i]];
                if (MeshRaycast.RaycastSegmentPlanar(rayOrigin, direction, prev, curr, out var p, out var d, out var _))
                {
                    if (d < bestD)
                    {
                        bestD = d;
                        bestP = p;
                        bestI = i;
                    }
                }
                prev = curr;
            }
            if (bestD == float.PositiveInfinity)
            {
                return null;
            }
            else
            {
                var cellDir = MeshGridBuilder.EdgeIndexToCellDir((bestI + face.Count - 1) % face.Count, face.Count, meshGridOptions.DoubleOddFaces);
                return new RaycastInfo
                {
                    cell = cell,
                    cellDir = cellDir,
                    distance = bestD,
                    point = bestP,
                };
            }
        }


        private static readonly Matrix4x4 RotateYZ = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 0, 1));
        private static readonly Matrix4x4 RotateZY = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        internal static bool GetRotationFromMatrix(ICellType cellType, Matrix4x4 cellTransform, Matrix4x4 matrix, out CellRotation rotation)
        {
            var m = cellTransform.inverse * matrix;
            // TODO: Dispatch to celltype method?
            if (cellType is XZCellTypeModifier modifier)
            {
                cellType = modifier.Underlying;
                if (cellType != CubeCellType.Instance)
                {
                    m = RotateYZ * m * RotateZY;
                }
            }
            if (cellType == CubeCellType.Instance)
            {
                var cubeRotation = CubeRotation.FromMatrix(m);
                if (cubeRotation != null)
                {
                    rotation = cubeRotation.Value;
                    return true;
                }
            }
            else if (cellType == SquareCellType.Instance)
            {
                var squareRotation = SquareRotation.FromMatrix(m);
                if (squareRotation != null)
                {
                    rotation = squareRotation.Value;
                    return true;
                }
            }
            else if (cellType is HexCellType hexCellType)
            {
                var hexRotation = HexRotation.FromMatrix(m, hexCellType.Orientation);
                if (hexRotation != null)
                {
                    rotation = hexRotation.Value;
                    return true;
                }
            }
            else if (cellType is TriangleCellType triangleCellType)
            {
                var hexRotation = HexRotation.FromMatrix(m, triangleCellType.Orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);
                if (hexRotation != null)
                {
                    rotation = hexRotation.Value;
                    return true;
                }
            }
            else if (cellType is HexPrismCellType hexPrismCellType)
            {
                var hexRotation = HexRotation.FromMatrix(m, hexPrismCellType.Orientation);
                if (hexRotation != null)
                {
                    rotation = hexRotation.Value;
                    return true;
                }
            }
            else if (cellType is TrianglePrismCellType trianglePrismCellType)
            {
                var hexRotation = HexRotation.FromMatrix(m, trianglePrismCellType.Orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);
                if (hexRotation != null)
                {
                    rotation = hexRotation.Value;
                    return true;
                }
            }
            else if (cellType is NGonCellType ngonCellType)
            {
                var cellRotation = ngonCellType.FromMatrix(m);
                if (cellRotation != null)
                {
                    rotation = cellRotation.Value;
                    return true;
                }
            }
            else if (cellType is NGonPrismCellType ngonPrismCellType)
            {
                var cellRotation = ngonPrismCellType.FromMatrix(m);
                if (cellRotation != null)
                {
                    rotation = cellRotation.Value;
                    return true;
                }
            }
            rotation = default;
            return false;
        }

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var p = matrix.MultiplyPoint(Vector3.zero);
            if (FindCell(p, out cell))
            {
                var cellData = CellData[cell];
                return GetRotationFromMatrix(cellData.CellType, cellData.TRS.ToMatrix(), matrix, out rotation);
            }

            rotation = default;
            return false;
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var boundBy = bound ?? meshDetails.hashCellBounds;
            var minHashCell = Vector3Int.Max(boundBy.Min, meshDetails.GetHashCell(min) - Vector3Int.one);
            var maxHashCell = Vector3Int.Min(boundBy.Mex - Vector3Int.one, meshDetails.GetHashCell(max) + Vector3Int.one);

            // Use a spatial hash to locate cells near the tile, and test each one.
            for (var x = minHashCell.x; x <= maxHashCell.x; x++)
            {
                for (var y = minHashCell.y; y <= maxHashCell.y; y++)
                {
                    for (var z = minHashCell.z; z <= maxHashCell.z; z++)
                    {
                        var h = new Vector3Int(x, y, z);
                        if (meshDetails.hashedCells.TryGetValue(h, out var cells))
                        {
                            foreach (var c in cells)
                            {
                                yield return c;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Shape
        public IReadOnlyList<Int32> GetFaceIndices(Cell cell)
        {
            var (face, submesh, layer) = Unpack(cell);
            var topology = meshData.GetTopology(submesh);
            if (topology == MeshTopology.Triangles)
            {
                return new ArraySegment<Int32>(meshData.indices[submesh], face * 3, 3);
            }
            else if (topology == MeshTopology.Quads)
            {
                return new ArraySegment<Int32>(meshData.indices[submesh], face * 4, 4);
            }
            else
            {
                return ((MeshCellData)CellData[cell]).Face;
            }
        }

        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            // TODO: This data should be cached
            var face = ((MeshCellData)CellData[cell]).Face;
            transform = Matrix4x4.identity;
            vertices = new Vector3[face.Length];
            var i = 0;
            foreach (var index in face)
            {
                vertices[i++] = meshData.vertices[index];
            }
        }

        public override IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        public override void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            DefaultGridImpl.GetMeshDataFromPolygon(this, cell, out meshData, out transform);
        }

        public override Aabb GetAabb(Cell cell)
        {
            var center = GetCellCenter(cell);
            var aabb = Aabb.FromMinMax(center, center);
            return ExpandAabb(aabb);
        }

        public override Aabb GetAabb(IEnumerable<Cell> cells)
        {
            var aabb = Aabb.FromVectors(cells.Select(GetCellCenter));
            return ExpandAabb(aabb);
        }

        #endregion
    }
}
