using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif


using static Sylves.VectorUtils;

namespace Sylves
{
    public class CubeGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { CubeCellType.Instance };

        CubeBound bound;

        Vector3 cellSize;

        public CubeGrid(float cellSize, CubeBound bound = null)
            :this(new Vector3(cellSize, cellSize, cellSize), bound)
        {

        }

        public CubeGrid(Vector3 cellSize, CubeBound bound = null)
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

        #region Basics
        public bool Is2D => false;

        public bool Is3D => true;

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
                    return new CubeGrid(cellSize, null);
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
            return CubeCellType.Instance;
        }
        #endregion
        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            switch ((CubeDir)dir)
            {
                case CubeDir.Right:
                    dest = cell + Vector3Int.right;
                    inverseDir = (CellDir)CubeDir.Left;
                    break;
                case CubeDir.Left:
                    dest = cell + Vector3Int.left;
                    inverseDir = (CellDir)CubeDir.Right;
                    break;
                case CubeDir.Up:
                    dest = cell + Vector3Int.up;
                    inverseDir = (CellDir)CubeDir.Down;
                    break;
                case CubeDir.Down:
                    dest = cell + Vector3Int.down;
                    inverseDir = (CellDir)CubeDir.Up;
                    break;
                case CubeDir.Forward:
                    dest = cell + new Vector3Int(0, 0, 1);
                    inverseDir = (CellDir)CubeDir.Back;
                    break;
                case CubeDir.Back:
                    dest = cell + new Vector3Int(0, 0, -1);
                    inverseDir = (CellDir)CubeDir.Forward;
                    break;
                default:
                    throw new Exception($"Invalid dir {dir}");
            }
            connection = new Connection();
            return bound == null ? true : bound.Contains(dest);
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            var squareRotation = (CubeRotation)startRotation;
            destCell = startCell + squareRotation * (destOffset - startOffset);
            destRotation = squareRotation;
            return true;
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return CubeCellType.Instance.GetCellDirs();
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var cell = startCell;
            while (cell.x < destCell.x)
            {
                yield return (cell, (CellDir)CubeDir.Right);
                cell.x += 1;
            }
            while (cell.x > destCell.x)
            {
                yield return (cell, (CellDir)CubeDir.Left);
                cell.x -= 1;
            }
            while (cell.y < destCell.y)
            {
                yield return (cell, (CellDir)CubeDir.Up);
                cell.y += 1;
            }
            while (cell.y > destCell.y)
            {
                yield return (cell, (CellDir)CubeDir.Down);
                cell.y -= 1;
            }
            while (cell.z < destCell.z)
            {
                yield return (cell, (CellDir)CubeDir.Forward);
                cell.z += 1;
            }
            while (cell.z > destCell.z)
            {
                yield return (cell, (CellDir)CubeDir.Back);
                cell.z -= 1;
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
            var min = (Vector3Int)(enumerator.Current);
            var max = min;
            while (enumerator.MoveNext())
            {
                var current = (Vector3Int)(enumerator.Current);
                min = Vector3Int.Min(min, current);
                max = Vector3Int.Min(max, current);
            }
            return new CubeBound(min, max + Vector3Int.one);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new CubeGrid(cellSize, (CubeBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((CubeBound)bound).Intersect((CubeBound)other);
        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((CubeBound)bound).Union((CubeBound)other);
        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (CubeBound)bound;
        }
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            return Vector3.Scale(cellSize, (Vector3Int)(cell) + new Vector3(0.5f, 0.5f, 0.5f));
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
            cell = (Cell)Vector3Int.FloorToInt(Divide(position, cellSize));
            return true;
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var cubeRotation = CubeRotation.FromMatrix(matrix);

            if(cubeRotation != null)
            {
                rotation = cubeRotation.Value;
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
                    minCell.z = Math.Max(minCell.z, bound.min.z);
                    maxCell.x = Math.Min(minCell.x, bound.max.x - 1);
                    maxCell.y = Math.Min(minCell.y, bound.max.y - 1);
                    maxCell.z = Math.Min(minCell.z, bound.max.z - 1);
                }

                // Loop over cels
                for (var x = minCell.x; x <= maxCell.x; x++)
                {
                    for (var y = minCell.y; y <= maxCell.y; y++)
                    {
                        for (var z = minCell.z; y <= maxCell.z; z++)
                        {
                            yield return new Cell(x, y, z);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
