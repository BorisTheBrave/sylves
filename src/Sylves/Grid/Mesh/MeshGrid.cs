﻿using System;
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
        private MeshDetails meshDetails;
        protected readonly MeshData meshData;
        private readonly MeshGridOptions meshGridOptions;
        protected bool is2d;

        public MeshGrid(MeshData meshData, MeshGridOptions meshGridOptions = null) :
            base(MeshGridBuilder.Build(meshData, meshGridOptions ?? new MeshGridOptions()))
        {
            this.meshData = meshData;
            this.meshGridOptions = meshGridOptions ?? new MeshGridOptions();
            BuildMeshDetails();
            is2d = true;
        }

        // Internals constructor used for other meshgrid like grids.
        internal MeshGrid(MeshData meshData, MeshGridOptions meshGridOptions, DataDrivenData data, bool is2d) :
            base(data)
        {
            this.meshData = meshData;
            this.meshGridOptions = meshGridOptions;
            this.is2d = is2d;
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
            var hashCellSize = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);
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

            this.meshDetails = meshDetails;
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
            if(cellType is XZCellTypeModifier modifier)
            {
                return modifier.Underlying;
            }
            return cellType;
        }
        #endregion


        #region Basics

        public override bool Is2d => is2d;

        public override bool Is3d => !is2d;

        public override bool IsPlanar => is2d && meshDetails.isPlanar;
        #endregion

        #region Topology

        // TODO: Pathfind on mesh, without involving layers
        //public override IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell);
        #endregion

        #region Query



        /// <summary>
        /// Returns true if p is in the triangle po, p1, p2
        /// </summary>
        private static bool IsPointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var n = Vector3.Cross(p1 - p0, p2 - p0);

            var o = Vector3.Dot(p - p2, n);
            const float epsilon = 1e-6f;
            if (o < -epsilon || o > epsilon)
                return false;

            var s = Vector3.Dot(n, Vector3.Cross(p0 - p2, p - p2));
            var t = Vector3.Dot(n, Vector3.Cross(p1 - p0, p - p0));

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = Vector3.Dot(n, Vector3.Cross(p2 - p1, p - p1));
            return d == 0 || (d < 0) == (s + t <= 0);
        }

        // FindCell, applied to a single cell
        protected virtual bool IsPointInCell(Vector3 position, Cell cell)
        {
            // Currently does fan detection
            // Doesn't work for convex faces
            var cellData = (MeshCellData)CellData[cell];
            var face = cellData.Face;
            var v0 = meshData.vertices[face[0]];
            var prev = meshData.vertices[face[face.Count - 1]];
            for (var i = 1; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                if (IsPointInTriangle(position, v0, prev, v))
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
        // Unlike raycast, this doesn't catch rays that start in the cell.
        protected virtual RaycastInfo? RaycastCell(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            var cellData = CellData[cell] as MeshCellData;
            if (IsPlanar)
            {
                var face = cellData.Face;
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
            else
            {
                // Currently does fan detection
                // Doesn't work for convex faces
                var face = ((MeshCellData)cellData).Face;
                var v0 = meshData.vertices[face[0]];
                var prev = meshData.vertices[face[face.Count - 1]];
                for (var i = 1; i < face.Count; i++)
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
                else if(cellType is NGonPrismCellType ngonPrismCellType)
                {
                    var cellRotation = ngonPrismCellType.FromMatrix(m);
                    if (cellRotation != null)
                    {
                        rotation = cellRotation.Value;
                        return true;
                    }
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
        public IReadOnlyList<int> GetFaceIndices(Cell cell)
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
            throw new Grid2dException();
        }
        #endregion
    }
}
