using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

using static Sylves.VectorUtils;

namespace Sylves
{
    /// <summary>
    /// Represents a 2d grid, where each cell corresponds to a face in a given mesh.
    /// </summary>
    public class MeshGrid : DataDrivenGrid
    {
        private readonly MeshDetails meshDetails;
        private bool is2d;

        public MeshGrid(MeshData meshData) :
            base(MeshGridBuilder.Build(meshData))
        {
            meshDetails = BuildMeshDetails();
            is2d = true;
        }

        internal MeshGrid(DataDrivenData data, bool is2d) :
            base(data)
        {
            meshDetails = BuildMeshDetails();
            this.is2d = is2d;
        }

        #region Impl

        private MeshDetails BuildMeshDetails()
        {
            var hashCellSize = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);
            foreach (var cell in GetCells())
            {
                var cellTrs = GetTRS(cell);
                // TODO: This is the wrong way to compute cell dimensions
                var dim = Abs(cellTrs.ToMatrix().MultiplyVector(Vector3.one));
                hashCellSize = Vector3.Max(hashCellSize, dim);
            }
            var meshDetails = new MeshDetails
            {
                hashCellSize = hashCellSize,
                hashedCells = new Dictionary<Vector3Int, List<Cell>>(),
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

            public Vector3Int GetHashCell(Vector3 v) => Vector3Int.FloorToInt(Divide(v, hashCellSize));
        }
        #endregion


        #region Basics

        public override bool Is2D => is2d;
        #endregion

        #region Topology

        // TODO: Pathfind on mesh, without involving layers
        //public override IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell);
        #endregion

        #region Query

        private bool IsPointInCell(Vector3 position, Cell cell)
        {
            var cellData = CellData[cell];
            var cellLocalPoint = cellData.TRS.ToMatrix().inverse.MultiplyPoint3x4(position);
            // TODO: Dispatch to celltype method?
            if(cellData.CellType == CubeCellType.Instance || cellData.CellType == SquareCellType.Instance)
            {
                return Vector3Int.FloorToInt(cellLocalPoint) == Vector3Int.zero;
            }
            else
            {
                throw new NotImplementedException();
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
                // TODO: Dispatch to celltype method?
                if (cellData.CellType == CubeCellType.Instance)
                {
                    var cubeRotation = CubeRotation.FromMatrix(m);
                    if (cubeRotation != null)
                    {
                        rotation = cubeRotation.Value;
                        return true;
                    }
                } 
                else if(cellData.CellType == SquareCellType.Instance)
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
                    throw new NotImplementedException();

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
    }
}
