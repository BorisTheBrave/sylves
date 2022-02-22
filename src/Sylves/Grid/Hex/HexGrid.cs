using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    public enum HexOrientation
    {
        // Neighbour diagram (for 0,0,0)
        //   -1,1,0 /\ 0,1,-1       
        //  -1,0,1 |  | 1,0,-1    
        //   0,-1,1 \/ 1,-1,0     
        // Axes diagram
        //           y
        //           |
        //          _*_
        //         ╱   ╲
        //        z     x
        PointyTopped,
        // Neighbour diagram
        //        0,1,-1
        //  -1,1,0  __  1,0,-1
        //         /  \ 
        //  -1,0,1 \__/ 1,-1,0
        //        0,-1,1
        // Axes diagram
        //         y
        //          \
        //           *-x
        //          /
        //         z
        FlatTopped,
    }

    /// <summary>
    /// A regular 2d grid of hexagons.
    /// The co-ordinate system used is "Cube-cordinates described here: https://www.redblobgames.com/grids/hexagons/
    /// However, it'll usually be fairly forgiving if you just use x,y and don't fill the z value.
    /// See HexOrientation for more details.
    /// Covers both the infinite grid, and bounded versions.
    /// Related classes:
    /// * <see cref="FTHexDir"/>/<see cref="PTHexDir"/>
    /// * <see cref="NGonCellType"/> (with n = 6)
    /// * <see cref="HexBound"/>
    /// </summary>
    public class HexGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType[] ftCellTypes = {HexCellType.Get(HexOrientation.FlatTopped)};
        private static readonly ICellType[] ptCellTypes= { HexCellType.Get(HexOrientation.PointyTopped) };

        private readonly ICellType cellType;

        private readonly HexBound bound;

        // cellSize measures the actual bounds of a single cell.
        // Thus, to get a regular hexagon, one of the values should be sqrt(3)/2 times smaller than the other (depending on Flat/PointyTopped
        // Note: Unity basically always uses PointyTopped, as other orientations are handled by a swizzle that just affects world position, not cell indices
        private readonly Vector2 cellSize;

        private readonly HexOrientation orientation;

        TriangleGrid childTriangles;

        public HexGrid(float cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
            :this(cellSize *(orientation == HexOrientation.PointyTopped ? new Vector2(Sqrt3 / 2, 1) : new Vector2(1, Sqrt3 / 2)), orientation, bound)
        {
        }

        public HexGrid(Vector2 cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            cellType = HexCellType.Get(orientation);
            childTriangles = new TriangleGrid(cellSize / 2, orientation == HexOrientation.FlatTopped ? TriangleOrientation.FlatTopped : TriangleOrientation.FlatSides);
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

        /// <summary>
        /// Returns the full list of cell types that can be returned by <see cref="GetCellType(Cell)"/>
        /// </summary>
        public IEnumerable<ICellType> GetCellTypes()
        {
            return orientation == HexOrientation.FlatTopped ? ftCellTypes : ptCellTypes;
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
                    return new HexGrid(cellSize, orientation, null);
                }

            }
        }

        public IGrid Unwrapped => this;

        public Cell[] GetChildTriangles(Cell cell)
        {
            var (a, b,c) = (cell.x, cell.y, cell.z);
            return new[] {
                new Cell(a + 1, b, c),
                new Cell(a + 1, b + 1, c),
                new Cell(a, b + 1, c),
                new Cell(a, b + 1, c + 1),
                new Cell(a, b, c + 1),
                new Cell(a + 1, b, c + 1),
            };
        }

        public Cell GetTriangleParent(Cell triangleCell)
        {
            // Rotate the co-ordinate system by 30 degrees, and discretize.
            // I'm not totally sure why this works.
            // Thanks to https://justinpombrio.net/programming/2020/04/28/pixel-to-hex.html
            var (x, y, z) = (triangleCell.x, triangleCell.y, triangleCell.z);
            if (orientation == HexOrientation.FlatTopped)
            {
                return new Cell(
                    Mathf.RoundToInt((x - z) / 3f),
                    Mathf.RoundToInt((y - x) / 3f),
                    Mathf.RoundToInt((z - y) / 3f)
                );
            }
            else
            {
                return new Cell(
                    Mathf.RoundToInt((x - y) / 3f),
                    Mathf.RoundToInt((y - z) / 3f),
                    Mathf.RoundToInt((z - x) / 3f)
                );
            }
        }

        public TriangleGrid GetChildTriangleGrid() => childTriangles;

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
            // FTHexDir and PTHexDir are arranged so that you don't need different code for the two orientations.
            inverseDir = (CellDir)((3 + (int)dir) % 6);
            connection = new Connection();
            switch((FTHexDir)dir)
            {
                case FTHexDir.UpRight:   dest = cell + new Vector3Int(1, 0, -1); break;
                case FTHexDir.Up:        dest = cell + new Vector3Int(0, 1, -1); break;
                case FTHexDir.UpLeft:    dest = cell + new Vector3Int(-1, 1, 0); break;
                case FTHexDir.DownLeft:  dest = cell + new Vector3Int(-1, 0, 1); break;
                case FTHexDir.Down:      dest = cell + new Vector3Int(0, -1, 1); break;
                case FTHexDir.DownRight: dest = cell + new Vector3Int(1, -1, 0); break;
                default: throw new ArgumentException($"Unknown dir {dir}");
            }
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
                offset = new Vector3Int(-offset.z, -offset.y, -offset.x);
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
            return cellType.GetCellDirs();
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
            return new HexBound(min, max + Vector3Int.one);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new HexGrid(cellSize, orientation, (HexBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((HexBound)bound).Intersect((HexBound)other);

        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((HexBound)bound).Union((HexBound)other);

        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (HexBound)bound;
        }
        #endregion
        
        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            if (orientation == HexOrientation.FlatTopped)
            {
                return new Vector3(
                    (0.5f * cell.x - 0.25f * cell.y - 0.25f * cell.z) * cellSize.x,
                    (                0.5f  * cell.y - 0.5f  * cell.z) * cellSize.y,
                    0);
            }
            else
            {
                return new Vector3(
                    (                0.5f  * cell.x - 0.5f  * cell.z) * cellSize.x,
                    (0.5f * cell.y - 0.25f * cell.x - 0.25f * cell.z) * cellSize.y,
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
            var success = childTriangles.FindCell(position, out var triangleCell);
            cell = GetTriangleParent(triangleCell);
            return success;
        }


        public bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var r = HexRotation.FromMatrix(matrix, orientation);
            if (r == null)
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
            if (orientation == HexOrientation.FlatTopped)
            {
                Cell? prev = null;
                int? first_y = null;
                foreach (var triangleCell in childTriangles.GetCellsIntersectsApprox(min, max))
                {
                    if (first_y == null) first_y = triangleCell.y;
                    var hex = GetTriangleParent(triangleCell);
                    // Tri must be in the bottom half of the hex, except the first row
                    // This stops double counting
                    if (first_y == triangleCell.y || hex.y - hex.z == triangleCell.y)
                    {
                        if (hex != prev)
                        {
                            yield return hex;
                            prev = hex;
                        }
                    }
                }
            }
            else
            {
                Cell? prev = null;
                int? first_x = null;
                foreach (var triangleCell in childTriangles.GetCellsIntersectsApprox(min, max))
                {
                    if (first_x == null) first_x = triangleCell.y;
                    var hex = GetTriangleParent(triangleCell);
                    // Tri must be in the bottom half of the hex, except the first row
                    // This stops double counting
                    if (first_x == triangleCell.y || hex.x - hex.z == triangleCell.x)
                    {
                        if (hex != prev)
                        {
                            yield return hex;
                            prev = hex;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
