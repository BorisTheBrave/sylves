using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// A regular square 2d grid.
    /// Covers both the infinite grid, and bounded versions.
    /// Related classes:
    /// * <see cref="FTHexDir"/>/<see cref="PTHexDir"/>
    /// * <see cref="NGonCellType"/> (with n = 6)
    /// * <see cref="HexBound"/>
    /// </summary>
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

        public IEnumerable<ICellType> GetCellTypes()
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

        public IGrid Unwrapped => this;
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return bound;
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
            return bound == null ? true : bound.Contains(dest);
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
            return (SquareBound)bound;
        }
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            var s = Vector2.Scale(cellSize, ToVector2Int(cell) + new Vector2(0.5f, 0.5f));
            return new Vector3(0.5f + s.x, 0.5f + s.y, 0);
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(GetCellCenter(cell));

        public Deformation GetDeformation(Cell cell) => Deformation.Identity;
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            position -= new Vector3(.5f, .5f, 0);
            var x = Mathf.RoundToInt(position.x / cellSize.x);
            var y = Mathf.RoundToInt(position.y / cellSize.y);
            var z = 0;
            cell = new Cell(x, y, z);
            return true;
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            const float eps = 1e-6f;
            var m = matrix;

            var localPos = m.MultiplyPoint3x4(Vector3.zero);

            var forward = m.MultiplyVector(Vector3.forward);
            if (Vector3.Distance(forward, Vector3.forward) > eps)
            {
                cell = default;
                rotation = default;
                return false;
            }

            var right = m.MultiplyVector(Vector3.right);

            var scale = m.lossyScale;
            var isReflection = false;
            if (scale.x * scale.y * scale.z < 0)
            {
                isReflection = true;
                right.x = -right.x;
            }
            var angle = Mathf.Atan2(right.y, right.x);
            var angleInt = Mathf.RoundToInt(angle / (Mathf.PI / 2));

            rotation = (isReflection ? SquareRotation.ReflectX : SquareRotation.Identity) * SquareRotation.Rotate90(angleInt);
            return FindCell(localPos, out cell);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {

            if (FindCell(min, out var minCell) &&
                FindCell(max, out var maxCell))
            {
                // Filter to in bounds
                if (bound != null)
                {
                    minCell.x = Math.Max(minCell.x, bound.min.x);
                    minCell.y = Math.Max(minCell.y, bound.min.y);
                    maxCell.x = Math.Min(minCell.x, bound.max.x - 1);
                    maxCell.y = Math.Min(minCell.y, bound.max.y - 1);
                }

                // Loop over cels
                for (var x = minCell.x; x <= maxCell.x; x++)
                {
                    for (var y = minCell.y; y <= maxCell.y; y++)
                    {
                        yield return new Cell(x, y, 0);
                    }
                }
            }
        }
        #endregion

    }
}
