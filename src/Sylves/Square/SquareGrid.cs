using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public class SquareGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { SquareCellType.Instance };

        SquareBound bound;

        Vector2 cellSize;

        public SquareGrid(Vector2 cellSize, SquareBound bound = null)
        {
            this.cellSize = cellSize;
            this.bound = bound;
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        private static Vector2Int ToVector2Int(Cell cell)
        {
            if(cell.z != 0)
            {
                throw new Exception("SquareGrid only has cells with z = 0");
            }
            return new Vector2Int(cell.x, cell.y);
        }

        #region Basics
        public bool Is2D => true;

        public bool Is3D => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        /// <summary>
        /// Returns the full list of cell types that can be returned by <see cref="GetCellType(Cell)"/>
        /// </summary>
        public IEnumerable<ICellType> GetCellTypes(Cell cell)
        {
            return cellTypes;
        }
        #endregion

        #region Relatives
        public IGrid Unbounded
        {
            get
            {
                if (bound == null)
                {
                    return this;
                }
                else
                {
                    return new SquareGrid(cellSize, null);
                }
            }
        }
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            for (var x = bound.min.x; x < bound.max.x; x++)
            {
                for (var y = bound.min.y; y < bound.max.y; y++)
                {
                    yield return new Cell(x, y);
                }
            }
        }

        public ICellType GetCellType(Cell cell)
        {
            return SquareCellType.Instance;
        }
        #endregion
        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            switch((SquareDir)dir)
            {
                case SquareDir.Right:
                    dest = cell + Vector3Int.right;
                    inverseDir = (CellDir)SquareDir.Left;
                    break;
                case SquareDir.Left:
                    dest = cell + Vector3Int.left;
                    inverseDir = (CellDir)SquareDir.Right;
                    break;
                case SquareDir.Up:
                    dest = cell + Vector3Int.up;
                    inverseDir = (CellDir)SquareDir.Down;
                    break;
                case SquareDir.Down:
                    dest = cell + Vector3Int.down;
                    inverseDir = (CellDir)SquareDir.Up;
                    break;
                default:
                    throw new Exception($"Invalid dir {dir}");
            }
            connection = new Connection();
            return bound == null ? true : bound.Contains(new Vector2Int(dest.x, dest.y));
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            var squareRotation = (SquareRotation)startRotation;
            destCell = startCell + squareRotation * (destOffset - startOffset);
            destRotation = squareRotation;
            return true;
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return SquareCellType.Instance.GetCellDirs();
        }

        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                CheckBounded();
                return bound.size.x * bound.size.y;
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            return (cell.x - bound.min.x) + (cell.y - bound.min.y) * bound.size.x;
        }

        public Cell GetCellByIndex(int index)
        {
            var x = index % bound.size.x;
            var y = index / bound.size.x;
            return new Cell(x + bound.min.x, y + bound.min.y, 0);
        }
        #endregion

        #region Bounds
        public IBound GetBound(IEnumerable<Cell> cells)
        {
            var enumerator = cells.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                throw new Exception($"Enumerator empty");
            }
            var min = ToVector2Int(enumerator.Current);
            var max = min;
            while(enumerator.MoveNext())
            {
                var current = ToVector2Int(enumerator.Current);
                min = Vector2Int.Min(min, current);
                max = Vector2Int.Min(max, current);
            }
            return new SquareBound(min, max - Vector2Int.one);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new SquareGrid(cellSize, (SquareBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((SquareBound)bound).Intersect((SquareBound)other);
        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((SquareBound)bound).Union((SquareBound)other);
        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            var sBound = (SquareBound)bound;
            for (var x = sBound.min.x; x < sBound.max.x; x++)
            {
                for (var y = sBound.min.y; y < sBound.max.y; y++)
                {
                    yield return new Cell(x, y);
                }
            }
        }
        #endregion

        #region Position
        #endregion

        #region Query
        #endregion

    }
}
