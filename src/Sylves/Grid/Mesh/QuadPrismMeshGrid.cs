using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /*
    class QuadPrismMeshGrid : IGrid
    {
        private readonly MeshData surfaceMesh;
        private readonly int layerCount;
        private readonly int[] faceCounts;
        private readonly int maxFaceCount;
        private readonly int subMeshCount;
        private readonly int indexCount;
        private readonly float tileHeight;
        private readonly float surfaceOffset;
        private readonly IDictionary<(Vector3Int, CellDir), (Vector3Int, CellDir, Connection)> moves;
        private readonly bool surfaceSmoothNormals;
        private readonly MeshDetails meshDetails;

        public QuadPrismMeshGrid(MeshData surfaceMesh, int minLayer, int maxLayer, float tileHeight, float surfaceOffset, bool surfaceSmoothNormals)
        {
            this.surfaceMesh = surfaceMesh;
            this.layerCount = layerCount;
            this.tileHeight = tileHeight;
            this.surfaceOffset = surfaceOffset;
            this.surfaceSmoothNormals = surfaceSmoothNormals;
            this.moves = moves;
            this.faceCounts = Enumerable.Range(0, surfaceMesh.subMeshCount).Select(i => surfaceMesh.indices[i].Length / 4).ToArray();
            this.maxFaceCount = faceCounts.Max();
            this.subMeshCount = surfaceMesh.subMeshCount;
            this.indexCount = maxFaceCount * layerCount * subMeshCount;
            moves = BuildMoves();
            meshDetails = BuildMeshDetails();
        }

        #region Impl

        private MeshDetails BuildMeshDetails()
        {
            var trs = new Dictionary<Vector3Int, TRS>();
            var hashCellSize = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);
            foreach (var cell in GetCells())
            {
                var cellTrs = trs[cell] = GetTRSInner(cell);
                var dim = GeometryUtils.Abs(cellTrs.ToMatrix().MultiplyVector(Vector3.one));
                hashCellSize = Vector3.Max(hashCellSize, dim);
            }
            var meshDetails = new MeshDetails
            {
                trs = trs,
                hashCellSize = hashCellSize,
                hashedCells = new Dictionary<Vector3Int, List<Vector3Int>>(),
            };
            Vector3Int? hashCellMin = null;
            Vector3Int? hashCellMax = null;

            foreach (var cell in GetCells())
            {
                var cellTrs = trs[cell];
                var hashCell = meshDetails.GetHashCell(cellTrs.Position);
                if (!meshDetails.hashedCells.TryGetValue(hashCell, out var cellList))
                {
                    cellList = meshDetails.hashedCells[hashCell] = new List<Vector3Int>();
                }
                cellList.Add(cell);
                hashCellMin = hashCellMin == null ? hashCell : Vector3Int.Min(hashCellMin.Value, hashCell);
                hashCellMax = hashCellMax == null ? hashCell : Vector3Int.Max(hashCellMax.Value, hashCell);
            }
            meshDetails.hashCellBounds = hashCellMin == null ? new BoundsInt() : new BoundsInt(hashCellMin.Value, hashCellMax.Value - hashCellMin.Value);

            return meshDetails;
        }

        // Structure caching some additional data about the mesh
        private class MeshDetails
        {
            public Dictionary<Vector3Int, TRS> trs;
            public Vector3 hashCellSize;
            public CubeBound hashCellBounds;
            public Dictionary<Vector3Int, List<Vector3Int>> hashedCells;

            public Vector3Int GetHashCell(Vector3 v) => Vector3Int.FloorToInt(new Vector3(v.x / hashCellSize.x, v.y / hashCellSize.y, v.z / hashCellSize.z));
        }
        #endregion


        #region Basics

        public bool Is2D => false;
        public bool Is3D => true;
        public bool IsPlanar => false;
        public bool IsRepeating => false;
        public bool IsOrientable => false;
        public bool IsFinite => true;
        public bool IsSingleCellType => true;
        public IEnumerable<ICellType> GetCellTypes()
        {
            yield return CubeCellType.Instance;
        }
        #endregion

        #region Relatives

        public IGrid Unbounded => this;

        public IGrid Unwrapped => this;

        #endregion

        #region Cell info

        IEnumerable<Cell> GetCells();

        ICellType GetCellType(Cell cell);
        #endregion

        #region Topology

        bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection);

        bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation);

        IEnumerable<CellDir> GetCellDirs(Cell cell);

        #endregion

        #region Index
        int IndexCount { get; }

        int GetIndex(Cell cell);

        Cell GetCellByIndex(int index);
        #endregion

        #region Bounds
        IBound GetBound(IEnumerable<Cell> cells);

        IGrid BoundBy(IBound bound);

        IBound IntersectBounds(IBound bound, IBound other);
        IBound UnionBounds(IBound bound, IBound other);
        IEnumerable<Cell> GetCellsInBounds(IBound bound);
        #endregion

        #region Position
        Vector3 GetCellCenter(Cell cell);

        TRS GetTRS(Cell cell);

        Deformation GetDeformation(Cell cell);

        // TODO: Also shape
        #endregion

        #region Query
        bool FindCell(Vector3 position, out Cell cell);

        bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation);

        IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max);

        // TODO: FindCells, Raycast, GetPath
        #endregion
    }
    */
}
