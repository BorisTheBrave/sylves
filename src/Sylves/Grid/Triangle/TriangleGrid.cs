using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public enum TriangleOrientation
    {
        // Neighbour diagram
        //
        // 0,1,0 /|
        //      / | 1,0,0
        //      \ |
        // 0,0,1 \|
        //
        //        |\ 0,0,-1
        // -1,0,0 | \ 
        //        | /
        //        |/ 0,-1,0
        //
        // Axes diagram
        //         y
        //          \
        //           *-x
        //          /
        //         z
        FlatSides,

        // Neighbour diagram
        //      0,1,0
        //        __
        //       \  /
        //  0,0,1 \/ 1,0,0
        //
        // -1,0,0 /\  0,0,-1
        //       /__\
        //      0,-1,0  
        // Axes diagram
        //         y
        //         |
        //        _*_
        //       ╱   ╲
        //      z     x
        FlatTopped,
    }

    public class TriangleGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType[] ftCellTypes = { HexCellType.Get(HexOrientation.FlatTopped) };
        private static readonly ICellType[] fsCellTypes = { HexCellType.Get(HexOrientation.PointyTopped) };

        // Also used for PTHexDir Right, UpLeft, DownLeft
        private static readonly CellDir[] cellDirsA = { (CellDir)FTHexDir.UpRight, (CellDir)FTHexDir.UpLeft, (CellDir)FTHexDir.Down };
        // Also used for PTHexDir UpRight, Left, DownRight
        private static readonly CellDir[] cellDirsB = { (CellDir)FTHexDir.Up, (CellDir)FTHexDir.DownLeft, (CellDir)FTHexDir.DownRight };

        readonly ICellType cellType;

        private readonly TriangleBound bound;

        private readonly Vector2 cellSize;

        private readonly TriangleOrientation orientation;

        private readonly IGrid altGrid;

        internal static Vector2 ComputeCellSize(float cellSize, TriangleOrientation orientation)
        {
            return cellSize * (orientation == TriangleOrientation.FlatTopped ? new Vector2(1, Sqrt3 / 2) : new Vector2(Sqrt3 / 2, 1));
        }

        public TriangleGrid(float cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
            :this(ComputeCellSize(cellSize, orientation), orientation, bound)
        {
        }

        public TriangleGrid(Vector2 cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            this.cellType = HexCellType.Get(orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);
            if(orientation == TriangleOrientation.FlatSides)
            {
                // altGrid has the same topology and cell centers
                // as this grid, but it uses the other orientation.
                // This is a lazy hack to avoid coding things twice.
                altGrid = 
                    new BijectModifier(
                    new TransformModifier(new TriangleGrid(
                    new Vector2(cellSize.y, cellSize.x),
                    TriangleOrientation.FlatTopped,
                    bound
                    ),
                    new Matrix4x4(new Vector4(0, 1, 0, 0), new Vector4( 1, 0, 0, 0), new Vector4(0, 0,1, 0), new Vector4(0, 0, 0, 1))
                    ),
                    SwizzleXY,
                    SwizzleXY);
                /*altGrid = new TransformWrapper(new TriangleGrid(
                    new Vector2(cellSize.y, cellSize.x),
                    TriangleOrientation.FlatTopped,
                    bound
                    ),
                    Matrix4x4.Rotate(Quaternion.Euler(0, 0, 30))
                    );
                */
            }
        }

        private static Cell SwizzleXY(Cell c) => new Cell(c.y, c.x, c.z);

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        public bool IsUp(Cell cell) => orientation == TriangleOrientation.FlatTopped && cell.x + cell.y + cell.z == 2;
        public bool IsDown(Cell cell) => orientation == TriangleOrientation.FlatTopped && cell.x + cell.y + cell.z == 1;
        public bool IsLeft(Cell cell) => orientation == TriangleOrientation.FlatSides && cell.x + cell.y + cell.z == 1;
        public bool IsRight(Cell cell) => orientation == TriangleOrientation.FlatSides && cell.x + cell.y + cell.z == 2;
        private bool IsUpOrLeft(Cell cell) => (orientation == TriangleOrientation.FlatSides) ^ (cell.x + cell.y + cell.z == 2);

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
            return orientation == TriangleOrientation.FlatTopped ? ftCellTypes : fsCellTypes;
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
                    return new TriangleGrid(cellSize, orientation, null);
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
            return cellType;
        }
        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);
        #endregion
        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            inverseDir = (CellDir)((3 + (int)dir) % 6);
            if (orientation == TriangleOrientation.FlatTopped)
            {
                switch ((FTHexDir)dir)
                {
                    case FTHexDir.UpRight: dest = cell + new Vector3Int(0, 0, -1); break;
                    case FTHexDir.Up: dest = cell + new Vector3Int(0, 1, 0); break;
                    case FTHexDir.UpLeft: dest = cell + new Vector3Int(-1, 0, 0); break;
                    case FTHexDir.DownLeft: dest = cell + new Vector3Int(0, 0, 1); break;
                    case FTHexDir.Down: dest = cell + new Vector3Int(0, -1, 0); break;
                    case FTHexDir.DownRight: dest = cell + new Vector3Int(1, 0, 0); break;
                    default: throw new ArgumentException($"Unknown dir {dir}");
                }
            }
            else
            {
                switch ((PTHexDir)dir)
                {
                    case PTHexDir.Right: dest = cell + new Vector3Int(1, 0, 0); break;
                    case PTHexDir.UpRight: dest = cell + new Vector3Int(0, 0, -1); break;
                    case PTHexDir.UpLeft: dest = cell + new Vector3Int(0, 1, 0); break;
                    case PTHexDir.Left: dest = cell + new Vector3Int(-1, 0, 0); break;
                    case PTHexDir.DownLeft: dest = cell + new Vector3Int(0, 0, 1); break;
                    case PTHexDir.DownRight: dest = cell + new Vector3Int(0, -1, 0); break;
                    default: throw new ArgumentException($"Unknown dir {dir}");
                }
            }
            connection = new Connection();
            return IsCellInGrid(dest);
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // FTHexDir and PTHexDir are arranged so that you don't need different code for the two orientations.
            destRotation = startRotation;
            var offset = destOffset - startOffset;
            var ir = (int)startRotation;
            if (ir < 0)
            {
                ir = ~ir;
                if (orientation == TriangleOrientation.FlatSides)
                {
                    offset = new Vector3Int(offset.x, offset.z, offset.y);
                }
                else
                {
                    offset = new Vector3Int(offset.y, offset.x, offset.z);
                }
            }
            switch (ir)
            {
                case 0: break;
                case 1: offset = new Vector3Int(-offset.y, -offset.z, -offset.x); break;
                case 2: offset = new Vector3Int(offset.z, offset.x, offset.y); break;
                case 3: offset = new Vector3Int(-offset.x, -offset.y, -offset.z); break;
                case 4: offset = new Vector3Int(offset.y, offset.z, offset.x); break;
                case 5: offset = new Vector3Int(-offset.z, -offset.x, -offset.y); break;
            }
            destCell = startCell + offset;
            return IsCellInGrid(destCell);
        }

        public virtual bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Recognize TriangleGrid
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            if (IsUpOrLeft(cell))
            {
                return cellDirsA;
            }
            else
            {
                return cellDirsB;
            }
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var cell = startCell;
            var isMin = cell.x + cell.y + cell.z == 1;
            if (orientation == TriangleOrientation.FlatTopped)
            {
                while (true)
                {
                    if (isMin)
                    {
                        if (cell.x < destCell.x)
                        {
                            yield return (cell, (CellDir)FTHexDir.DownRight);
                            cell.x += 1;
                            isMin = false;
                            continue;
                        }
                        if (cell.y < destCell.y)
                        {
                            yield return (cell, (CellDir)FTHexDir.Up);
                            cell.y += 1;
                            isMin = false;
                            continue;

                        }
                        if (cell.z < destCell.z)
                        {
                            yield return (cell, (CellDir)FTHexDir.DownLeft);
                            cell.z += 1;
                            isMin = false;
                            continue;
                        }
                    }
                    else
                    {
                        if (cell.x > destCell.x)
                        {
                            yield return (cell, (CellDir)FTHexDir.UpLeft);
                            cell.x -= 1;
                            isMin = true;
                            continue;
                        }
                        if (cell.y > destCell.y)
                        {
                            yield return (cell, (CellDir)FTHexDir.Down);
                            cell.y -= 1;
                            isMin = true;
                            continue;
                        }
                        if (cell.z > destCell.z)
                        {
                            yield return (cell, (CellDir)FTHexDir.UpRight);
                            cell.z -= 1;
                            isMin = true;
                            continue;
                        }
                    }
                    break;
                }
            }
            else
            {
                while (true)
                {
                    if (isMin)
                    {
                        if (cell.x < destCell.x)
                        {
                            yield return (cell, (CellDir)PTHexDir.Right);
                            cell.x += 1;
                            isMin = false;
                            continue;
                        }
                        if (cell.y < destCell.y)
                        {
                            yield return (cell, (CellDir)PTHexDir.UpLeft);
                            cell.y += 1;
                            isMin = false;
                            continue;
                        }
                        if (cell.z < destCell.z)
                        {
                            yield return (cell, (CellDir)PTHexDir.DownLeft);
                            cell.z += 1;
                            isMin = false;
                            continue;
                        }
                    }
                    else
                    {
                        if (cell.x > destCell.x)
                        {
                            yield return (cell, (CellDir)PTHexDir.Left);
                            cell.x -= 1;
                            isMin = true;
                            continue;
                        }
                        if (cell.y > destCell.y)
                        {
                            yield return (cell, (CellDir)PTHexDir.DownRight);
                            cell.y -= 1;
                            isMin = true;
                            continue;
                        }
                        if (cell.z > destCell.z)
                        {
                            yield return (cell, (CellDir)PTHexDir.UpRight);
                            cell.z -= 1;
                            isMin = true;
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        #endregion

        #region Index
        public int IndexCount
        {
            get
            {
                CheckBounded();
                var size = bound.max - bound.min;
                return 2 * size.x * size.y;
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            var sizeX = bound.max.x - bound.min.x;
            var dx = cell.x - bound.min.x;
            var dy = cell.y - bound.min.y;
            var dz = cell.x + cell.y + cell.z - 1;
            return dz + (dx + dy * sizeX) * 2;
        }

        public Cell GetCellByIndex(int index)
        {
            var sizeX = bound.max.x - bound.min.x;
            var dz = index % 2;
            index /= 2;
            var x = bound.min.x + (index % sizeX);
            var y = bound.min.y + (index / sizeX);
            var z = dz - x - y + 1;
            return new Cell(x, y, z);
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
            return new TriangleBound(min, max + Vector3Int.one);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new TriangleGrid(cellSize, orientation, (TriangleBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((TriangleBound)bound).Intersect((TriangleBound)other);
        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((TriangleBound)bound).Union((TriangleBound)other);
        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (TriangleBound)bound;
        }
        public bool IsCellInBound(Cell cell, IBound bound)
        {
            var sum = cell.x + cell.y + cell.z;
            return 1 <= sum && sum <= 2 && (bound is TriangleBound tb ? tb.Contains(cell) : true);
        }
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            if(orientation == TriangleOrientation.FlatTopped)
            {
                return new Vector3(
                    (0.5f * cell.x                       + -0.5f * cell.z) * cellSize.x,
                    (-1 / 3f * cell.x + 2 / 3f * cell.y - 1 / 3f * cell.z) * cellSize.y,
                    0);
            }
            else
            {
                return new Vector3(
                    (-1 / 3f * cell.y + 2 / 3f * cell.x - 1 / 3f * cell.z) * cellSize.x,
                    (0.5f * cell.y                       + -0.5f * cell.z) * cellSize.y,
                    0);
            }
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
            if (orientation == TriangleOrientation.FlatSides)
            {
                var x = position.x / cellSize.x;
                var y = position.y / cellSize.y;
                cell = new Cell(
                    Mathf.FloorToInt(x) + 1,
                    Mathf.CeilToInt(y - 0.5f * x),
                    Mathf.CeilToInt(-y - 0.5f * x)
                );
                return true;
            }
            else
            {
                // Using dot product, measures which row and diagonals a given point occupies.
                // Or equivalently, multiply by the inverse matrix to tri_center
                // Note we have to break symmetry, using floor(...)+1 instead of ceil, in order
                // to deal with corner vertices like (0, 0) correctly.
                var x = position.x / cellSize.x;
                var y = position.y / cellSize.y;
                cell = new Cell(
                    Mathf.CeilToInt(x - 0.5f * y),
                    Mathf.FloorToInt(y) + 1,
                    Mathf.CeilToInt(-x - 0.5f * y)
                );
                return true;
            }
        }

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var r = HexRotation.FromMatrix(matrix, orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);
            if(r == null)
            {
                cell = default;
                rotation = default;
                return false;
            }
            rotation = r.Value;
            var localPos = matrix.MultiplyPoint3x4(Vector3.zero);
            return FindCell(localPos, out cell);
        }

        public IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            if (orientation == TriangleOrientation.FlatSides)
            {
                foreach (var cell in altGrid.GetCellsIntersectsApprox(min, max))
                {
                    yield return cell;
                }
                yield break;
            }
            var x = min.x;
            var y = min.y;
            var width = max.x - min.x;
            var height = max.y - min.y;
            if (width < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException($"Rectangle should have non-negative area");
            }
            //assert width >= 0, "Rectangle should have non-negative width"
            //assert height >= 0, "Rectangle should have non-negative height"
            // For consistency, we treat the triangles as exclusive of their border, and the rect as inclusive
            x /= cellSize.x;
            y /= cellSize.y;
            width /= cellSize.x;
            height /= cellSize.y;
            // Lower and upper bound by row
            var fl = y;
            var fu = (y + height);
            // Loop over all rows that the rectangle is in
            for (var b = Mathf.FloorToInt(fl) + 1; b < Mathf.CeilToInt(fu) + 1; b++)
            {
                // Consider each row vs a trimmed rect
                var minb = Math.Max(b - 1, fl);
                var maxb = Math.Min(b, fu);
                // The smallest / largest values for the diagonals
                // can be read from the trimmed rect corners
                var mina = Mathf.FloorToInt(x - maxb / 2) + 1;
                var maxa = Mathf.CeilToInt(x + width - minb / 2);
                var minc = Mathf.FloorToInt(-x - width - maxb / 2) + 1;
                var maxc = Mathf.CeilToInt(-x - minb / 2);
                // Walk along the row left to right
                var a = mina;
                var c = maxc;
                //assert a + b + c == 1 or a + b + c == 2
                while (a <= maxa && c >= minc)
                {
                    yield return new Cell(a, b, c);
                    if (a + b + c == 1) {
                        a += 1;
                    }
                    else {
                        c -= 1;
                    }
                }
            }
        }
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var cubeRotation = (HexRotation)cellRotation;
            var srcBound = GetBound(src);
            var srcMin = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var srcMax = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Max);
            var r1 = cubeRotation.Multiply(srcMin);
            var r2 = cubeRotation.Multiply(srcMax);
            var newMin = Vector3Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var translation = destMin - newMin;
            // Check it actually works
            if (!src.Select(c => (Cell)(translation + cubeRotation.Multiply((Vector3Int)(c)))).All(dest.Contains))
            {
                return null;
            }
            return new GridSymmetry
            {
                Src = new Cell(),
                Dest = (Cell)(translation),
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
            var cubeBound = (TriangleBound)srcBound;
            // TODO: Use operator*
            if (!TryApplySymmetry(s, (Cell)(cubeBound.min), out var a, out var _))
            {
                return false;
            }
            // This trick works best with *inclusive* bounds.
            if (!TryApplySymmetry(s, (Cell)(cubeBound.min - Vector3Int.one), out var b, out var _))
            {
                return false;
            }
            destBound = new TriangleBound(Vector3Int.Min((Vector3Int)(a), (Vector3Int)(b)), Vector3Int.Max((Vector3Int)(a), (Vector3Int)(b)) + Vector3Int.one);
            return true;
        }
        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            return TryMoveByOffset(s.Dest, (Vector3Int)s.Src, (Vector3Int)src, s.Rotation, out dest, out r);
        }
        #endregion
    }
}
