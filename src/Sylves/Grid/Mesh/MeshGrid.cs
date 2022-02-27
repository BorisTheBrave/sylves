using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Sylves.VectorUtils;

namespace Sylves
{
    class MeshGrid : DataDrivenGrid
    {
        private readonly MeshDetails meshDetails;

        public MeshGrid(MeshData meshData):
            base(MeshGridBuilder.Build(meshData))
        {
            meshDetails = BuildMeshDetails();
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
            hashCellMin = hashCellMin ?? new Vector3Int();
            hashCellMax = hashCellMax ?? new Vector3Int();
            meshDetails.hashCellBounds = new CubeBound(hashCellMin.Value, hashCellMax.Value);

            return meshDetails;
        }

        // Structure caching some additional data about the mesh
        private class MeshDetails
        {
            public Vector3 hashCellSize;
            public CubeBound hashCellBounds;
            public Dictionary<Vector3Int, List<Cell>> hashedCells;

            public Vector3Int GetHashCell(Vector3 v) => Vector3Int.FloorToInt(Divide(v, hashCellSize));
        }
        #endregion


        #region Basics

        public override bool Is2D => true;
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell)
        {
            foreach (var c in GetCellsIntersectsApprox(position, position))
            {
                var trs = GetTRS(c);
                var m = trs.ToMatrix().inverse;
                // TODO: Only supporint cube cell type
                var x = Vector3Int.FloorToInt(m.MultiplyPoint3x4(position));
                if(x == Vector3Int.zero)
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
                var cubeRotation = CubeRotation.FromMatrix(GetTRS(cell).ToMatrix().inverse * matrix);
                if (cubeRotation != null)
                {
                    rotation = cubeRotation.Value;
                    return true;
                }
            }

            rotation = default;
            return false;
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var minHashCell = Vector3Int.Max(meshDetails.hashCellBounds.min, meshDetails.GetHashCell(min) - Vector3Int.one);
            var maxHashCell = Vector3Int.Min(meshDetails.hashCellBounds.max, meshDetails.GetHashCell(max) + Vector3Int.one);

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

        // TODO: FindCells, Raycast, GetPath
        #endregion
    }
}
