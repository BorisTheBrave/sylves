using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A regular square 2d grid.
    /// Covers both the infinite grid, and bounded versions.
    /// Cell (0, 0) has center at (0.5, 0.5).
    /// Related classes:
    /// * <see cref="FTHexDir"/>/<see cref="PTHexDir"/>
    /// * <see cref="NGonCellType"/> (with n = 6)
    /// * <see cref="HexBound"/>
    /// </summary>
    public class SquareGrid : IGrid
    {
        private static readonly ICellType[] cellTypes = { SquareCellType.Instance };

        private static readonly Vector3[] polygon =
        {
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(-0.5f, -0.5f, 0),
        };

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
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public int CoordinateDimension => 2;

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

        public IDualMapping GetDual()
        {
            var dualBound = bound == null ? null :
                new SquareBound(bound.min, bound.max + Vector2Int.one);

            var translation = Matrix4x4.Translate(new Vector3(-cellSize.x / 2, -cellSize.y / 2, 0));

            return new DualMapping(this, new SquareGrid(cellSize, dualBound).Transformed(translation));
        }

        private class DualMapping : BasicDualMapping
        {
            public DualMapping(SquareGrid baseGrid, IGrid dualGrid):base(baseGrid, dualGrid)
            {

            }
            private (Cell cell, CellCorner inverseCorner)? ToPair(Cell cell, CellCorner corner, IGrid outGrid)
            {
                var squareCorner = (SquareCorner)corner;
                switch (squareCorner)
                {
                    case SquareCorner.DownRight:
                        cell.x += 1;
                        break;
                    case SquareCorner.UpRight:
                        cell.x += 1;
                        cell.y += 1;
                        break;
                    case SquareCorner.UpLeft:
                        cell.y += 1;
                        break;
                    case SquareCorner.DownLeft:
                        break;
                    default:
                        throw new Exception($"Unexpected corner {corner}");
                }
                if (!outGrid.IsCellInGrid(cell))
                {
                    return null;
                }
                return (cell, (CellCorner)(((int)corner + 2) % 4));
            }
            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner) => ToPair(baseCell, corner, DualGrid);

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner) => ToPair(dualCell - new Vector3Int(1, 1, 0), corner, BaseGrid);
        }
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

        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            return SquareCellType.Instance.GetCellCorners();
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
                max = Vector2Int.Max(max, current);
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
            if (bound == null) throw new GridInfiniteException();
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

        public Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            return Vector3.Scale(new Vector3(cellSize.x, cellSize.y, 0), (Vector3Int)(cell) + new Vector3(0.5f, 0.5f, 0) + ((SquareCorner)corner).GetPosition());
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(GetCellCenter(cell), Quaternion.identity, new Vector3(cellSize.x, cellSize.y, 1));
        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            vertices = polygon;
            transform = Matrix4x4.Translate(GetCellCenter(cell)) * Matrix4x4.Scale(new Vector3(cellSize.x, cellSize.y, 1));
        }

        public IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            throw new Grid2dException();
        }

        public void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            DefaultGridImpl.GetMeshDataFromPolygon(this, cell, out meshData, out transform);
        }
        #endregion

        #region Query
        public bool FindCell(Vector3 position, out Cell cell)
        {
            var x = Mathf.FloorToInt(position.x / cellSize.x);
            var y = Mathf.FloorToInt(position.y / cellSize.y);
            var z = 0;
            cell = new Cell(x, y, z);
            return IsCellInGrid(cell);
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var squareRotation = SquareRotation.FromMatrix(matrix);

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
                    maxCell.x = Math.Min(maxCell.x, bound.max.x - 1);
                    maxCell.y = Math.Min(maxCell.y, bound.max.y - 1);
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
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            return Raycast(origin, direction, maxDistance, cellSize, bound);
        }

        // TOOD: Move somewhere more appropriate?
        public static IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance, Vector2 cellSize, SquareBound bound)
        {
            // Normalize things into a space where each cell
            // occupies a unit cube.
            var x1 = origin.x / cellSize.x;
            var y1 = origin.y / cellSize.y;
            var dx = direction.x / cellSize.x;
            var dy = direction.y / cellSize.y;

            var stepx = Math.Sign(dx);
            var stepy = Math.Sign(dy);
            var idx = Math.Abs(1 / dx);
            var idy = Math.Abs(1 / dy);
            var cellDirX = (CellDir)(dx >= 0 ? SquareDir.Left : SquareDir.Right);
            var cellDirY = (CellDir)(dy >= 0 ? SquareDir.Down : SquareDir.Up);

            // -1 = in middle of cell, 0,1,2 = on x,y,z face
            int startOnBorder;
            float extraDistance;
            // Filter to bounds
            if (bound != null)
            {
                // Find the start and end values of t that the ray crosses each axis.
                var tx1 = dx == 0 ? (bound.min.x > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (bound.min.x - x1) / dx : (bound.max.x - x1) / dx;
                var tx2 = dx == 0 ? (bound.max.x > x1 ? 1 : -1) * float.PositiveInfinity : dx >= 0 ? (bound.max.x - x1) / dx : (bound.min.x - x1) / dx;
                var ty1 = dy == 0 ? (bound.min.y > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (bound.min.y - y1) / dy : (bound.max.y - y1) / dy;
                var ty2 = dy == 0 ? (bound.max.y > y1 ? 1 : -1) * float.PositiveInfinity : dy >= 0 ? (bound.max.y - y1) / dy : (bound.min.y - y1) / dy;

                var mint = Math.Max(tx1, ty1);
                var maxt = Math.Min(tx2, ty2);
                // Don't go beyond maxt
                maxDistance = Math.Min(maxDistance, maxt);

                if (mint > 0)
                {
                    // Advance things to mint
                    x1 += dx * mint;
                    y1 += dy * mint;
                    maxDistance -= mint;
                    extraDistance = mint;
                    origin += direction * mint;
                    if (tx1 == mint)
                    {
                        startOnBorder = 0;
                        x1 = 0;
                    }
                    else
                    {
                        startOnBorder = 1;
                        y1 = 0;
                    }
                }
                else
                {
                    startOnBorder = -1;
                    extraDistance = 0;
                }

                if (maxDistance < 0)
                    yield break;
                if (mint == float.PositiveInfinity)
                    yield break;
            }
            else
            {
                startOnBorder = -1;
                extraDistance = 0;
            }

            var x = startOnBorder == 0 ? Mathf.RoundToInt(x1) + (dx > 0 ? -1 : 0) : Mathf.FloorToInt(x1);
            var y = startOnBorder == 1 ? Mathf.RoundToInt(y1) + (dy > 0 ? -1 : 0) : Mathf.FloorToInt(y1);

            if (startOnBorder == -1)
            {
                yield return new RaycastInfo
                {
                    cell = new Cell(x, y),
                    point = origin,
                    cellDir = null,
                    distance = 0,
                };
            }

            var tx = (x + (dx >= 0 ? 1 : 0) - x1) / dx;
            var ty = (y + (dy >= 0 ? 1 : 0) - y1) / dy;

            while (true)
            {
                float t;
                CellDir cellDir;
                if (tx < ty)
                {
                    if (tx > maxDistance) yield break;
                    t = tx;
                    x += stepx;
                    tx += idx;
                    cellDir = cellDirX;
                    if (bound != null && (x >= bound.max.x || x < bound.min.x)) yield break;
                }
                else
                {
                    if (ty > maxDistance) yield break;
                    t = ty;
                    y += stepy;
                    ty += idy;
                    cellDir = cellDirY;
                    if (bound != null && (y >= bound.max.y || x < bound.min.y)) yield break;
                }
                yield return new RaycastInfo
                {
                    cell = new Cell(x, y),
                    point = origin + t * direction,
                    cellDir = cellDir,
                    distance = t + extraDistance,
                };
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
            if (!TryApplySymmetry(s, FromVector2Int(squareBound.max - Vector2Int.one), out var b, out var _))
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
