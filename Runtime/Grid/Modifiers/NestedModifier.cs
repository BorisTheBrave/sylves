using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Replaces every cell in one grid (the chunk grid) with a collection of cells from lazily computed child grids.
    /// </summary>
    public abstract class NestedModifier : IGrid
    {
        private IEnumerable<ICellType> cellTypes;
        private ICachePolicy cachePolicy;
        private IGrid chunkGrid;
        private IDictionary<Cell, IGrid> childGrids;


        /// <summary>
        /// Applies NestedModifier to chunkGrid.
        /// </summary>
        /// <param name="chunkGrid">The base grid</param>
        /// <param name="cellTypes">What should the response of GetCellType </param>
        /// <param name="cachePolicy">Configures how to store the cahced meshes.</param>
        public NestedModifier(IGrid chunkGrid, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            Setup(chunkGrid, cellTypes, cachePolicy);
        }

        /// <summary>
        /// Must call Setup!
        /// </summary>
        protected NestedModifier()
        {

        }

        /// <summary>
        /// Clone constructor. Clones share the same cache!
        /// </summary>
        protected NestedModifier(NestedModifier original, IGrid chunkGrid = null)
        {
            this.cellTypes = original.cellTypes;
            this.cachePolicy = original.cachePolicy;
            this.chunkGrid = chunkGrid ?? original.chunkGrid;
            this.childGrids = original.childGrids;
        }



        protected void Setup(IGrid chunkGrid, IEnumerable<ICellType> cellTypes = null, ICachePolicy cachePolicy = null)
        {
            this.cellTypes = cellTypes;
            this.cachePolicy = cachePolicy ?? Sylves.CachePolicy.Always;
            this.chunkGrid = chunkGrid;
            childGrids = this.cachePolicy.GetDictionary<IGrid>(this);
        }

        protected IGrid ChunkGrid => chunkGrid;

        protected ICachePolicy CachePolicy => cachePolicy;

        /// <summary>
        /// The grid associated with each chunk. This grid has extra requirements:
        /// * It should only use the x-coordinate of the cell. The other two should be zero, (to be offset by the chunk position)
        /// * The dest of moves is also offset by the chunk position. Moves may be off grid (illegal in a normal grid)
        /// 
        /// As building a grid with off-grid moves is difficult, you can also just override TryMove to whatever.
        /// </summary>
        protected abstract IGrid GetChildGrid(Cell chunkCell);

        protected IGrid GetChildGridCached(Cell chunkCell)
        {
            if (childGrids.TryGetValue(chunkCell, out IGrid meshGrid))
            {
                return meshGrid;
            }
            return childGrids[chunkCell] = GetChildGrid(chunkCell);
        }

        protected virtual (Cell childCell, Cell chunkCell) Split(Cell cell)
        {
            return (new Cell(cell.x, 0, 0), new Cell(cell.y, cell.z, 0));
        }

        protected virtual Cell Combine(Cell childCell, Cell chunkCell)
        {
            // We include childCell.y/z to allow for out-of-grid moves from a child cell.
            return new Cell(childCell.x, childCell.y + chunkCell.x, childCell.z + chunkCell.y);
        }

        // TOOD: Think about this
        protected virtual Vector3 MeshTranslation(Cell chunkCell) => new Vector3();


        #region Basics

        public virtual bool Is2d => chunkGrid.Is2d;

        public virtual bool Is3d => chunkGrid.Is3d;

        public virtual bool IsPlanar => chunkGrid.IsPlanar;

        public virtual bool IsRepeating => false;

        public virtual bool IsOrientable => chunkGrid.IsOrientable;

        public virtual bool IsFinite => chunkGrid.IsFinite;

        public virtual bool IsSingleCellType => cellTypes != null && cellTypes.Count() == 1;

        public virtual int CoordinateDimension => 3;

        public virtual IEnumerable<ICellType> GetCellTypes() => cellTypes ?? throw new Exception("Unknown cell types");

        #endregion

        #region Relatives

        public abstract IGrid Unbounded { get; }

        public IGrid Unwrapped => this;


        public virtual IDualMapping GetDual()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            foreach (var chunkCell in chunkGrid.GetCells())
            {
                foreach (var childCell in GetChildGridCached(chunkCell).GetCells())
                {
                    yield return Combine(childCell, chunkCell);
                }
            }
        }

        public ICellType GetCellType(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).GetCellType(childCell);
        }

        public bool IsCellInGrid(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);

            return chunkGrid.IsCellInGrid(chunkCell) && GetChildGridCached(chunkCell).IsCellInGrid(childCell);
        }
        #endregion

        #region Topology

        public virtual bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var (childCell, chunkCell) = Split(cell);
            if (GetChildGridCached(chunkCell).TryMove(childCell, dir, out dest, out inverseDir, out connection))
            {
                dest = Combine(dest, chunkCell);
                return true;
            }
            return false;
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            throw new NotImplementedException();
        }

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).GetCellDirs(childCell);
        }
        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).GetCellCorners(childCell);
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell) => throw new NotImplementedException();

        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int GetIndex(Cell cell)
        {
            throw new NotImplementedException();
        }

        public Cell GetCellByIndex(int index)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Bounds
        public IBound GetBound() => chunkGrid.GetBound();

        public IBound GetBound(IEnumerable<Cell> cells) => chunkGrid.GetBound(cells.Select(x => Split(x).chunkCell));

        public abstract IGrid BoundBy(IBound bound);

        public IBound IntersectBounds(IBound bound, IBound other) => chunkGrid.IntersectBounds(bound, other);
        public IBound UnionBounds(IBound bound, IBound other) => chunkGrid.UnionBounds(bound, other);
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            foreach (var chunkCell in chunkGrid.GetCellsInBounds(bound))
            {
                foreach (var childCell in GetChildGridCached(chunkCell).GetCells())
                {
                    yield return Combine(childCell, chunkCell);
                }
            }
        }

        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var (childCell, chunkCell) = Split(cell);

            // No need to check childCell, this method should only be called with cells in the grid
            return chunkGrid.IsCellInBound(chunkCell, bound);
        }
        #endregion


        #region Position
        public Vector3 GetCellCenter(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).GetCellCenter(childCell) + MeshTranslation(chunkCell);
        }

        public Vector3 GetCellCorner(Cell cell, CellCorner cellCorner)
        {
            var (childCell, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).GetCellCorner(childCell, cellCorner) + MeshTranslation(chunkCell);
        }

        public TRS GetTRS(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            var trs = GetChildGridCached(chunkCell).GetTRS(childCell);
            trs = new TRS(trs.Position + MeshTranslation(chunkCell), trs.Rotation, trs.Scale);
            return trs;
        }

        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell)
        {
            var (childCell, chunkCell) = Split(cell);
            return Matrix4x4.Translate(MeshTranslation(chunkCell)) * GetChildGridCached(chunkCell).GetDeformation(childCell);
        }

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var (childCell, chunkCell) = Split(cell);
            GetChildGridCached(chunkCell).GetPolygon(childCell, out vertices, out transform);
            transform = Matrix4x4.Translate(MeshTranslation(chunkCell)) * transform;
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new NotImplementedException();
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            var (childCell, chunkCell) = Split(cell);
            GetChildGridCached(chunkCell).GetMeshData(childCell, out meshData, out transform);
            transform = Matrix4x4.Translate(MeshTranslation(chunkCell)) * transform;
        }
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            foreach (var chunkCell in chunkGrid.GetCellsIntersectsApprox(position, position))
            {
                var childGrid = GetChildGridCached(chunkCell);
                if (childGrid.FindCell(position - MeshTranslation(chunkCell), out var childCell))
                {
                    cell = Combine(childCell, chunkCell);
                    return true;
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
            var position = matrix.MultiplyPoint(Vector3.zero);
            if (!FindCell(position, out cell))
            {
                rotation = default;
                return false;
            }
            var (_, chunkCell) = Split(cell);
            return GetChildGridCached(chunkCell).FindCell(matrix * Matrix4x4.Translate(-MeshTranslation(chunkCell)), out var _, out rotation);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            foreach (var chunkCell in chunkGrid.GetCellsIntersectsApprox(min, max))
            {
                var t = MeshTranslation(chunkCell);
                foreach (var childCell in GetChildGridCached(chunkCell).GetCellsIntersectsApprox(min - t, max - t))
                {
                    yield return Combine(childCell, chunkCell);
                }
            }
        }

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            var queuedRaycastInfos = new PriorityQueue<RaycastInfo>(x => x.distance, (x, y) => -x.distance.CompareTo(y.distance));
            foreach (var chunkRaycastInfo in chunkGrid.Raycast(origin, direction, maxDistance))
            {
                foreach (var ri in queuedRaycastInfos.Drain(chunkRaycastInfo.distance))
                {
                    yield return ri;
                }

                var chunkCell = chunkRaycastInfo.cell;
                var t = MeshTranslation(chunkCell);
                foreach (var raycastInfo in GetChildGridCached(chunkCell).Raycast(origin - t, direction, maxDistance))
                {
                    queuedRaycastInfos.Add(new RaycastInfo
                    {
                        cell = Combine(chunkCell, raycastInfo.cell),
                        cellDir = raycastInfo.cellDir,
                        distance = raycastInfo.distance,
                        point = raycastInfo.point + t,
                    });
                }
            }

            // Final drain
            foreach (var ri in queuedRaycastInfos.Drain())
            {
                yield return ri;
            }
        }
        #endregion


        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            throw new NotImplementedException();
        }

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            throw new NotImplementedException();
        }

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
