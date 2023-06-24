using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    public class DataDrivenCellData
    {
        public TRS TRS { get; set; }
        public Deformation Deformation { get; set; }
        public ICellType CellType { get; set; }

        public DataDrivenCellData Clone()
        {
            return (DataDrivenCellData)MemberwiseClone();
        }
    }

    public class DataDrivenData
    {
        public IDictionary<Cell, DataDrivenCellData> Cells { get; set; }
        public IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> Moves { get; set; }
    }

    // A finite grid specified by a bunch of cell data passed in.
    public abstract class DataDrivenGrid : IGrid
    {
        private ICellType[] cellTypes;
        private readonly IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves;
        private readonly IDictionary<Cell, DataDrivenCellData> cellData;
        private BiMap<Cell, int> indices;

        protected DataDrivenGrid(DataDrivenData data)
        {
            this.cellData = data.Cells;
            this.moves = data.Moves;
            cellTypes = cellData.Select(x => x.Value.CellType).Distinct().ToArray();
            indices = new BiMap<Cell, int>(cellData.Keys.Select((x, i) => (x, i)));
        }


        protected IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> Moves => moves;
        protected IDictionary<Cell, DataDrivenCellData> CellData => cellData;

        #region Basics

        public virtual bool Is2d => false;
        public virtual bool Is3d => false;
        public virtual bool IsPlanar => false;
        public bool IsRepeating => false;
        public bool IsOrientable => false;
        public bool IsFinite => true;
        public bool IsSingleCellType => cellTypes.Length == 1;

        public virtual int CoordinateDimension => 3;
        public IEnumerable<ICellType> GetCellTypes()
        {
            return cellTypes;
        }
        #endregion

        #region Relatives

        public IGrid Unbounded => this;

        public IGrid Unwrapped => this;
        public virtual IDualMapping GetDual() => throw new System.NotSupportedException();

        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells() => cellData.Keys;

        public ICellType GetCellType(Cell cell) => cellData[cell].CellType;

        public bool IsCellInGrid(Cell cell) => cellData.ContainsKey(cell);
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if(moves.TryGetValue((cell, dir), out var t))
            {
                (dest, inverseDir, connection) = t;
                return true;
            }
            else
            {
                dest = default;
                inverseDir = default;
                connection = default;
                return false;
            }
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.TryMoveByOffset(this, startCell, startOffset, destOffset, startRotation, out destCell, out destRotation);
        }

        public virtual bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        // TODO: Do we want to enhance this?
        public IEnumerable<CellDir> GetCellDirs(Cell cell) => GetCellType(cell).GetCellDirs();
        public IEnumerable<CellCorner> GetCellCorners(Cell cell) => GetCellType(cell).GetCellCorners();

        public virtual IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => DefaultGridImpl.FindBasicPath(this, startCell, destCell);

        #endregion

        #region Index
        public int IndexCount => indices.Count;

        public int GetIndex(Cell cell) => indices[cell];

        public Cell GetCellByIndex(int index) => indices[index];
        #endregion

        #region Bounds
        public IBound GetBound() => DefaultGridImpl.GetBound(this);

        public IBound GetBound(IEnumerable<Cell> cells) => DefaultGridImpl.GetBound(this, cells);

        public IGrid BoundBy(IBound bound) => DefaultGridImpl.BoundBy(this, bound);

        public IBound IntersectBounds(IBound bound, IBound other) => DefaultGridImpl.IntersectBounds(this, bound, other);
        public IBound UnionBounds(IBound bound, IBound other) => DefaultGridImpl.UnionBounds(this, bound, other);
        public IEnumerable<Cell> GetCellsInBounds(IBound bound) => DefaultGridImpl.GetCellsInBounds(this, bound);
        public virtual bool IsCellInBound(Cell cell, IBound bound) => DefaultGridImpl.IsCellInBound(this, cell, bound);

        #endregion

        #region Position
        public Vector3 GetCellCenter(Cell cell) => GetTRS(cell).Position;

        public abstract Vector3 GetCellCorner(Cell cell, CellCorner cellCorner);

        public TRS GetTRS(Cell cell) => cellData[cell].TRS;

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => cellData[cell].Deformation;

        public virtual void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new System.NotImplementedException();

        public virtual IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell) => throw new System.NotImplementedException();

        public virtual void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform) => throw new System.NotImplementedException();

        #endregion

        #region Query
        public abstract bool FindCell(Vector3 position, out Cell cell);

        public abstract bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation);

        public abstract IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max);

        public abstract IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity);
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => DefaultGridImpl.FindGridSymmetry(this, src, dest, srcCell, cellRotation);

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => DefaultGridImpl.TryApplySymmetry(this, s, srcBound, out destBound);
        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r) => DefaultGridImpl.TryApplySymmetry(this, s, src, out dest, out r);
        #endregion
    }
}
