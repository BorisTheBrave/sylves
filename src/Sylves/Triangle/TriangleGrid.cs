using System;
using System.Collections.Generic;
using System.Text;

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

        private static readonly ICellType cellType = NGonCellType.Get(6);
        private static readonly ICellType[] cellTypes = { cellType };

        TriangleBound bound;

        Vector2 cellSize;

        TriangleOrientation orientation;

        IGrid altGrid;

        public TriangleGrid(float cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
            :this(cellSize * (orientation == TriangleOrientation.FlatTopped ? new Vector2(1, Sqrt3 / 2) : new Vector2(Sqrt3 / 2, 1)), orientation, bound)
        {
        }

        public TriangleGrid(Vector2 cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            if(orientation == TriangleOrientation.FlatSides)
            {
                // altGrid has the same topology and cell centers
                // as this grid, but it uses the other orientation.
                // This is a lazy hack to avoid coding things twice.
                altGrid = new TransformWrapper(new TriangleGrid(
                    new Vector2(cellSize.y, cellSize.x),
                    TriangleOrientation.FlatTopped,
                    bound
                    ),
                    Matrix4x4.Rotate(Quaternion.Euler(0, 0, 30))
                    );
            }
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
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
        #endregion
        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            inverseDir = (CellDir)((3 + (int)dir) % 6);
            connection = new Connection();
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
            return bound == null ? true : bound.Contains(dest);
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
                throw new NotImplementedException();
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            throw new NotImplementedException();
        }

        public Cell GetCellByIndex(int index)
        {
            throw new NotImplementedException();
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
            var min = (Vector3Int)(enumerator.Current);
            var max = min;
            while (enumerator.MoveNext())
            {
                var current = (Vector3Int)(enumerator.Current);
                min = Vector3Int.Min(min, current);
                max = Vector3Int.Min(max, current);
            }
            return new TriangleBound(min, max - Vector3Int.one);
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
                    (0.5f * cell.y + -0.5f * cell.z) * cellSize.y,
                    0);
            }
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
            if (orientation == TriangleOrientation.FlatSides)
            {
                return altGrid.FindCell(position, out cell);
            }

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

        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            throw new NotImplementedException();

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
            for (var b = Mathf.FloorToInt(fl); b < Mathf.CeilToInt(fu); b++)
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
    }
}
