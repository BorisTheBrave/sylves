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

        private bool IsPointInCell(Vector3 position, Cell cell)
        {
            var cellData = CellData[cell];
            var cellLocalPoint = cellData.TRS.ToMatrix().inverse.MultiplyPoint3x4(position);
            var cellType = UnwrapXZCellModifier(cellData.CellType);
            // TODO: Dispatch to celltype method?
            // TODO: This should use actual cell shape
            if (cellType == CubeCellType.Instance || 
                cellType == SquareCellType.Instance)
            {
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
                    if (IsPointInTriangle(cellLocalPoint, v0, prev, v))
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
                else if(cellType == SquareCellType.Instance)
                {
                    var squareRotation = SquareRotation.FromMatrix(m);
                    if (squareRotation != null)
                    {
                        rotation = squareRotation.Value;
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
