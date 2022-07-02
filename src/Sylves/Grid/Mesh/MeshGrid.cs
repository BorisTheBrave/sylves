using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    public class MeshCellData : DataDrivenCellData
    {
        public MeshUtils.Face Face { get; set; }
    }

    /// <summary>
    /// Represents a 2d grid, where each cell corresponds to a face in a given mesh.
    /// </summary>
    public class MeshGrid : DataDrivenGrid
    {
        private readonly MeshDetails meshDetails;
        protected readonly MeshData meshData;
        protected bool is2d;

        public MeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) :
            base(MeshGridBuilder.Build(meshData, meshGridOptions ?? new MeshGridOptions()))
        {
            this.meshData = meshData;
            meshDetails = BuildMeshDetails();
            is2d = true;
        }

        internal MeshGrid(MeshData meshData, DataDrivenData data, bool is2d) :
            base(data)
        {
            this.meshData = meshData;
            meshDetails = BuildMeshDetails();
            this.is2d = is2d;
        }

        #region Impl

        private MeshDetails BuildMeshDetails()
        {
            var hashCellSize = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);
            Vector3? min = null;
            Vector3? max = null;
            foreach (var cell in GetCells())
            {
                if (CellData[cell] is MeshCellData meshCellData)
                {
                    var face = ((MeshCellData)CellData[cell]).Face;
                    var cellMin = meshData.vertices[face[0]];
                    var cellMax = cellMin;
                    for (var i = 1; i < face.Count; i++)
                    {
                        var v = meshData.vertices[face[i]];
                        cellMin = Vector3.Min(cellMin, v);
                        cellMax = Vector3.Max(cellMax, v);
                    }
                    var dim = cellMax - cellMin;
                    hashCellSize = Vector3.Max(hashCellSize, dim);
                    min = min == null ? cellMin : Vector3.Min(min.Value, cellMin);
                    max = max == null ? cellMax : Vector3.Max(max.Value, cellMax);
                }
                else
                {
                    // TODO: This is the wrong way to compute cell dimensions
                    var cellTrs = GetTRS(cell);
                    var dim = Abs(cellTrs.ToMatrix().MultiplyVector(Vector3.one));
                    hashCellSize = Vector3.Max(hashCellSize, dim);
                }
            }
            var meshDetails = new MeshDetails
            {
                hashCellSize = hashCellSize,
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
            if(hashCellMin == null)
            {
                hashCellMin = new Vector3Int();
                hashCellMax = -Vector3Int.one;
            }
            hashCellMin = hashCellMin ?? new Vector3Int();
            hashCellMax = hashCellMax ?? new Vector3Int();
            meshDetails.hashCellBounds = new CubeBound(hashCellMin.Value, hashCellMax.Value + Vector3Int.one);
            meshDetails.expandedHashCellBounds = new CubeBound(hashCellMin.Value - Vector3Int.one, hashCellMax.Value + Vector3Int.one + Vector3Int.one);

            return meshDetails;
        }

        // Structure caching some additional data about the mesh
        // Every cell is assigned a unique hash cell, and the hash cells are large enough
        // that the bounds of individual cells are contained by the hash cells +-1 away from that
        // hash cell in each axis.
        private class MeshDetails
        {
            public Vector3 hashCellSize;
            public CubeBound hashCellBounds;
            public CubeBound expandedHashCellBounds;
            public Dictionary<Vector3Int, List<Cell>> hashedCells;
            public bool isPlanar;

            public Vector3Int GetHashCell(Vector3 v) => Vector3Int.FloorToInt(Divide(v, hashCellSize));
        }

        ICellType UnwrapXZCellModifier(ICellType cellType)
        {
            if(cellType is XZCellModifier modifier)
            {
                return modifier.Underlying;
            }
            return cellType;
        }
        #endregion


        #region Basics

        public override bool Is2D => is2d;

        public override bool IsPlanar => meshDetails.isPlanar;
        #endregion

        #region Topology

        // TODO: Pathfind on mesh, without involving layers
        //public override IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell);
        #endregion

        #region Query

        private static bool IsPointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var s = (p0.x - p2.x) * (p.y - p2.y) - (p0.y - p2.y) * (p.x - p2.x);
            var t = (p1.x - p0.x) * (p.y - p0.y) - (p1.y - p0.y) * (p.x - p0.x);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = (p2.x - p1.x) * (p.y - p1.y) - (p2.y - p1.y) * (p.x - p1.x);
            return d == 0 || (d < 0) == (s + t <= 0);
        }

        // FindCell, applied to a single cell
        private bool IsPointInCell(Vector3 position, Cell cell)
        {
            var cellData = CellData[cell];
            var cellType = UnwrapXZCellModifier(cellData.CellType);
            // TODO: Dispatch to celltype method?
            // TODO: This should use actual cell shape
            if (cellType == CubeCellType.Instance || 
                cellType == SquareCellType.Instance)
            {
            var cellLocalPoint = cellData.TRS.ToMatrix().inverse.MultiplyPoint3x4(position);
                return Vector3Int.RoundToInt(cellLocalPoint) == Vector3Int.zero;
            }
            else
            {
                if(!IsPlanar)
                {
                    throw new NotImplementedException();
                }

                // Currently does fan detection
                // Doesn't work for convex faces
                var face = ((MeshCellData)cellData).Face;
                var v0 = meshData.vertices[face[0]];
                var prev = meshData.vertices[face[face.Count - 1]];
                for(var i=1;i<face.Count;i++)
                {
                    var v = meshData.vertices[face[i]];
                    if (IsPointInTriangle(position, v0, prev, v))
                        return true;
                    prev = v;
                }
                return false;
            }
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
            var bfRaycastInfos = CubeGrid.Raycast(origin, direction, maxDistance, meshDetails.hashCellSize, meshDetails.expandedHashCellBounds);
            Vector3Int? prevHashCell = null;
            var queuedRaycastInfos = new List<RaycastInfo>();
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

                // Re-sort queue
                queuedRaycastInfos.Sort((x, y) => -x.distance.CompareTo(y.distance));

                // Find the distance such that all raycastInfos smaller than this distance have already found,
                // meaning it is safe to stream them out of the queue without getting anything out of order
                var minDistance = bfRaycastInfo.distance - Mathf.Min(
                        Mathf.Abs(meshDetails.hashCellSize.x / direction.x),
                        Mathf.Abs(meshDetails.hashCellSize.y / direction.y),
                        Mathf.Abs(meshDetails.hashCellSize.z / direction.z)
                    );

                // Actually stream all the safe raycastInfos
                while (queuedRaycastInfos.Count > 0 && queuedRaycastInfos[queuedRaycastInfos.Count - 1].distance < minDistance)
                {
                    var ri = queuedRaycastInfos[queuedRaycastInfos.Count - 1];
                    queuedRaycastInfos.RemoveAt(queuedRaycastInfos.Count - 1);
                    if (hasOriginCell && originCell == ri.cell)
                        continue;
                    yield return ri;
                }

                prevHashCell = (Vector3Int)bfRaycastInfo.cell;
            }

            // We've found all raycast infos, stream out any that haven't already been sent
            for (var i = queuedRaycastInfos.Count - 1; i >= 0; i--)
            {
                var ri = queuedRaycastInfos[i];
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
        private RaycastInfo? RaycastCell(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            var cellData = CellData[cell];
            var cellType = UnwrapXZCellModifier(cellData.CellType);
            if (cellType == CubeCellType.Instance)
            {
                // TODO
                return null;
                /*
                GetCellVertices(cell, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8);
                var hit = MeshRaycast.RaycastCube(rayOrigin, direction, v1, v2, v3, v4, v5, v6, v7, v8);
                if (hit != null)
                {
                    var hit2 = hit.Value;
                    hit2.cell = cell;
                    return hit2;
                }
                else
                {
                    return null;
                }
                */
            } 
            else
            {
                if(!IsPlanar)
                {
                    throw new NotImplementedException();
                }

                var face = ((MeshCellData)cellData).Face;
                var prev = meshData.vertices[face[face.Count - 1]];
                var bestD = float.PositiveInfinity;
                var bestP = new Vector3();
                var bestI = 0;
                for (var i = 0; i < face.Count; i++)
                {
                    var curr = meshData.vertices[face[i]];
                    if (MeshRaycast.RaycastSegment(rayOrigin, direction, prev, curr, out var p, out var d))
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
                if(bestD == float.PositiveInfinity)
                {
                    return null;
                }
                else
                {
                    return new RaycastInfo
                    {
                        cell = cell,
                        //cellDir = null,// TODO
                        distance = bestD,
                        point = bestP,
                    };
                }
            }
        }

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            // TODO: Only supporting cube cell type
            var p = matrix.MultiplyPoint(Vector3.zero);
            if (FindCell(p, out cell))
            {
                var cellData = CellData[cell];
                var m = cellData.TRS.ToMatrix().inverse * matrix;
                var cellType = UnwrapXZCellModifier(cellData.CellType);
                // TODO: Dispatch to celltype method?
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
                else if(cellType is NGonCellType ngonCellType)
                {
                    var cellRotation = ngonCellType.FromMatrix(m);
                    if (cellRotation != null)
                    {
                        rotation = cellRotation.Value;
                        return true;
                    }
                }
                else
                {
                    throw new NotImplementedException($"Unsupported cellType {cellType}");

                }
            }

            rotation = default;
            return false;
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var minHashCell = Vector3Int.Max(meshDetails.hashCellBounds.min, meshDetails.GetHashCell(min) - Vector3Int.one);
            var maxHashCell = Vector3Int.Min(meshDetails.hashCellBounds.max - Vector3Int.one, meshDetails.GetHashCell(max) + Vector3Int.one);

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
        protected IReadOnlyList<int> GetBaseFaceIndices(Cell cell)
        {
            var (face, submesh, layer) = (cell.x, cell.y, cell.z);
            var topology = meshData.GetTopology(submesh);
            if (topology == MeshTopology.Triangles)
            {
                return new ArraySegment<int>(meshData.indices[submesh], face * 3, 3);
            }
            else if (topology == MeshTopology.Quads)
            {
                return new ArraySegment<int>(meshData.indices[submesh], face * 4, 4);

            }
            else
            {
                throw new Exception($"Unsupported topology {topology}");
            }
        }

        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var face = ((MeshCellData)CellData[cell]).Face;
            transform = Matrix4x4.identity;
            vertices = new Vector3[face.Length];
            var i = 0;
            foreach (var index in face)
            {
                vertices[i++] = meshData.vertices[index];
            }
        }
        #endregion
    }
}
