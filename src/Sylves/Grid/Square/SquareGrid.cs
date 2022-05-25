using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

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


        public SquareGrid(float cellSize, SquareBound bound = null)
            :this(new Vector2(cellSize, cellSize), bound)
        {
        }

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
            if (cell.z != 0)
            {
                throw new Exception("SquareGrid only has cells with z = 0");
            }
            return new Vector2Int(cell.x, cell.y);
        }
        private static Cell FromVector2Int(Vector2Int v)
        {
            return new Cell(v.x, v.y);
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
        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
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
            return bound == null ? true : bound.Contains(destCell);
        }

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Recognize SquareGrids
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return SquareCellType.Instance.GetCellDirs();
        }
        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var cell = startCell;
            while (cell.x < destCell.x)
            {
                yield return (cell, (CellDir)SquareDir.Right);
                cell.x += 1;
            }
            while (cell.x > destCell.x)
            {
                yield return (cell, (CellDir)SquareDir.Left);
                cell.x -= 1;
            }
            while (cell.y < destCell.y)
            {
                yield return (cell, (CellDir)SquareDir.Up);
                cell.y += 1;
            }
            while (cell.y > destCell.y)
            {
                yield return (cell, (CellDir)SquareDir.Down);
                cell.y -= 1;
            }
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
        public IBound GetBound() => bound;

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
            return new SquareBound(min, max + Vector2Int.one);
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
        
        public bool IsCellInBound(Cell cell, IBound bound) => bound is SquareBound sb ? sb.Contains(cell) : true;
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            var s = Vector2.Scale(cellSize, ToVector2Int(cell) + new Vector2(0.5f, 0.5f));
            return new Vector3(s.x, s.y, 0);
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(GetCellCenter(cell));
        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => throw new NotImplementedException();
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
            var squareRotation= SquareRotation.FromMatrix(matrix);

            if (squareRotation != null)
            {
                rotation = squareRotation.Value;
                return FindCell(matrix.MultiplyPoint3x4(Vector3.zero), out cell);
            }
            else
            {
                cell = default;
                rotation = default;
                return false;
            }
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


        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var squareRotation = (SquareRotation)cellRotation;
            var srcBound = GetBound(src);
            var srcMin = src.Select(ToVector2Int).Aggregate(Vector2Int.Min);
            var srcMax = src.Select(ToVector2Int).Aggregate(Vector2Int.Max);
            var r1 = squareRotation * srcMin;
            var r2 = squareRotation * srcMax;
            var newMin = Vector2Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(ToVector2Int).Aggregate(Vector2Int.Min);
            var translation = destMin - newMin;
            // Check it actually works
            if(!src.Select(c => FromVector2Int(translation + squareRotation * ToVector2Int(c))).All(dest.Contains))
            {
                return null;
            }
            return new GridSymmetry
            {
                Src = new Cell(),
                Dest = FromVector2Int(translation),
                Rotation = cellRotation,
            };
        }

        public bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound)
        {
            destBound = null;
            if (srcBound == null)
            {
                return true;
            }
            var squareBound = (SquareBound)srcBound;
            // TODO: Use operator*
            if(!TryApplySymmetry(s, FromVector2Int(squareBound.min), out var a, out var _))
            {
                return false;
            }
            // This trick works best with *inclusive* bounds.
            if (!TryApplySymmetry(s, FromVector2Int(squareBound.min - Vector2Int.one), out var b, out var _))
            {
                return false;
            }
            destBound = new SquareBound(Vector2Int.Min(ToVector2Int(a), ToVector2Int(b)), Vector2Int.Max(ToVector2Int(a), ToVector2Int(b)) + Vector2Int.one);
            return true;
        }

        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            return TryMoveByOffset(s.Dest, (Vector3Int)s.Src, (Vector3Int)src, s.Rotation, out dest, out r);
        }
        #endregion
    }
}
