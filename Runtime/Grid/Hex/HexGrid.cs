using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
    /// * <see cref="HexCellType"/>
    /// * <see cref="HexBound"/>
    /// </summary>
    public class HexGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType[] ftCellTypes = {HexCellType.Get(HexOrientation.FlatTopped)};
        private static readonly ICellType[] ptCellTypes= { HexCellType.Get(HexOrientation.PointyTopped) };

        private static readonly Vector3[] ptPolygon = MeshPrimitives.ShapedPtHexPolygon(1, 1);

        private static readonly Vector3[] ftPolygon = MeshPrimitives.ShapedFtHexPolygon(1, 1);

        private readonly ICellType cellType;

        private readonly HexBound bound;

        // cellSize measures the actual bounds of a single cell.
        // Thus, to get a regular hexagon, one of the values should be sqrt(3)/2 times smaller than the other (depending on Flat/PointyTopped
        // Note: Unity basically always uses PointyTopped, as other orientations are handled by a swizzle that just affects world position, not cell indices
        private readonly Vector2 cellSize;

        private readonly HexOrientation orientation;

        private readonly TriangleGrid childTriangles;

        internal static Vector2 ComputeCellSize(float cellSize, HexOrientation orientation)
        {
            return cellSize * (orientation == HexOrientation.PointyTopped ? new Vector2(Sqrt3 / 2, 1) : new Vector2(1, Sqrt3 / 2));
        }

        /// <summary>
        /// Creates a hex grid where the side length is cellSize/2.
        /// I.e. the incircle diameter is cellSize * sqrt(3) / 2 and circumcircle diameter is cellSize
        /// 
        /// </summary>
        public HexGrid(float cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
            :this(ComputeCellSize(cellSize, orientation), orientation, bound)
        {
        }

        /// <summary>
        /// Creates a hex grid where each hex has width and height given by cellSize.x and cellSize.y
        /// </summary>
        public HexGrid(Vector2 cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            cellType = HexCellType.Get(orientation);
            childTriangles = new TriangleGrid(cellSize / 2, orientation == HexOrientation.FlatTopped ? TriangleOrientation.FlatTopped : TriangleOrientation.FlatSides);
        }

        /// <summary>
        /// Creates a hex grid where each hex has the given incircle diameter.
        /// The incircle diameter is the distance between the centers of two adjacent hexes.
        /// </summary>
        public static HexGrid WithIncircleDiameter(float diameter, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
        {
            return new HexGrid(diameter * Sqrt3, orientation, bound);
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        public HexOrientation Orientation => orientation;

        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public int CoordinateDimension => 3;

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

        public virtual IDualMapping GetDual()
        {
            // TODO: This seems right, but I haven't really validated
            var dualBound = bound == null ? null :
                new TriangleBound(bound.min, bound.max + Vector3Int.one);

            // Note hex orientation is flipped vs triangle orientation
            if (orientation == HexOrientation.FlatTopped)
            {
                var triCellSize = new Vector2(cellSize.x / (4f / 3), cellSize.y);
                return new TriangleGrid.DualMapping(new TriangleGrid(triCellSize, TriangleOrientation.FlatSides, dualBound), this).Reversed();
            }
            else
            {

                var triCellSize = new Vector2(cellSize.x, cellSize.y / (4f / 3));
                return new TriangleGrid.DualMapping(new TriangleGrid(triCellSize, TriangleOrientation.FlatTopped, dualBound), this).Reversed();
            }
        }

        public Cell[] GetChildTriangles(Cell cell)
        {
            var a = cell.x - cell.y;
            var b = cell.y - cell.z;
            var c = cell.z - cell.x;
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

        public bool IsCellInGrid(Cell cell) => IsCellInBound(cell, bound);

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
            var offset = ((HexRotation)startRotation).Multiply(destOffset - startOffset);
            destCell = startCell + offset;
            return bound == null ? true : bound.Contains(destCell);
        }

        public bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Recognize HexGrid
            return DefaultGridImpl.ParallelTransport(aGrid, aSrcCell, aDestCell, this, srcCell, startRotation, out destCell, out destRotation);
        }

        public IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            return cellType.GetCellDirs();
        }
        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            return cellType.GetCellCorners();
        }

        public IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            // Double check we really have "cube co-ordinates" otherwise this will loop for ever
            if (startCell.x + startCell.y + startCell.z != 0)
                throw new ArgumentException($"FindBasicPath passed a cell that doesn't correspond to a hex, {startCell}", nameof(startCell));
            if (destCell.x + destCell.y + destCell.z != 0)
                throw new ArgumentException($"FindBasicPath passed a cell that doesn't correspond to a hex, {destCell}", nameof(destCell));
            // FTHexDir and PTHexDir are arranged so that you don't need different code for the two orientations.
            var cell = startCell;
            while (cell.x < destCell.x && cell.z > destCell.z)
            {
                yield return (cell, (CellDir)FTHexDir.UpRight);
                cell.x += 1;
                cell.z -= 1;
            }
            while (cell.x > destCell.x && cell.z < destCell.z)
            {
                yield return (cell, (CellDir)FTHexDir.DownLeft);
                cell.x -= 1;
                cell.z += 1;
            }
            while (cell.y < destCell.y && cell.z > destCell.z)
            {
                yield return (cell, (CellDir)FTHexDir.Up);
                cell.y += 1;
                cell.z -= 1;
            }
            while (cell.y > destCell.y && cell.z < destCell.z)
            {
                yield return (cell, (CellDir)FTHexDir.Down);
                cell.y -= 1;
                cell.z += 1;
            }
            while (cell.x < destCell.x && cell.y > destCell.y)
            {
                yield return (cell, (CellDir)FTHexDir.DownRight);
                cell.x += 1;
                cell.y -= 1;
            }
            while (cell.x > destCell.x && cell.y < destCell.y)
            {
                yield return (cell, (CellDir)FTHexDir.UpLeft);
                cell.x -= 1;
                cell.y += 1;
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
                return size.x * size.y;
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            var sizeX = bound.max.x - bound.min.x;
            var dx = cell.x - bound.min.x;
            var dy = cell.y - bound.min.y;
            return dx + dy * sizeX;

        }

        public Cell GetCellByIndex(int index)
        {
            var sizeX = bound.max.x - bound.min.x;
            var x = bound.min.x + (index % sizeX);
            var y = bound.min.y + (index / sizeX);
            var z = -x - y;
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
                max = Vector3Int.Max(max, current);
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
        
        public bool IsCellInBound(Cell cell, IBound bound) => bound is HexBound hb ? hb.Contains(cell) : true;
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

        public Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            GetPolygon(cell, out var vertices, out var transform);
            return transform.MultiplyPoint3x4(vertices[(int)corner]);
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(
            GetCellCenter(cell),
            Quaternion.identity,
            // Inverse of the conversion in the constructor.
            // Should this be saved somewhere?
            new Vector3(cellSize.x / (orientation == HexOrientation.PointyTopped ? Sqrt3 / 2 : 1), cellSize.y / (orientation == HexOrientation.PointyTopped ? 1 : Sqrt3 / 2), 1));


        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            vertices = orientation == HexOrientation.PointyTopped ? ptPolygon : ftPolygon;
            transform = Matrix4x4.Translate(GetCellCenter(cell)) * Matrix4x4.Scale(new Vector3(cellSize.x, cellSize.y, 0));
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
            var success = childTriangles.FindCell(position, out var triangleCell);
            cell = GetTriangleParent(triangleCell);
            return success && IsCellInGrid(cell);
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
                    if (first_x == null) first_x = triangleCell.x;
                    var hex = GetTriangleParent(triangleCell);
                    // Tri must be in the bottom half of the hex, except the first row
                    // This stops double counting
                    if (first_x == triangleCell.x || hex.x - hex.z == triangleCell.x)
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
        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            Cell? prevHex = null;
            foreach(var triInfo in childTriangles.Raycast(origin, direction, maxDistance))
            {
                var hex = GetTriangleParent(triInfo.cell);
                if (hex == prevHex)
                    continue;

                yield return new RaycastInfo
                {
                    cell = hex,
                    cellDir = triInfo.cellDir,
                    point = triInfo.point,
                    distance = triInfo.distance,
                };

                prevHex = hex;
            }
        }
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var hexRotation = (HexRotation)cellRotation;
            var srcMin = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var srcMax = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Max);
            var r1 = hexRotation.Multiply(srcMin);
            var r2 = hexRotation.Multiply(srcMax);
            var newMin = Vector3Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var translation = destMin - newMin;
            var a = src.Select(c => (Cell)(translation + hexRotation.Multiply((Vector3Int)(c)))).ToList();
            var b = a.Select(dest.Contains).ToList();
            // Check it actually works
            if (!src.Select(c => (Cell)(translation + hexRotation.Multiply((Vector3Int)(c)))).All(dest.Contains))
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
            var cubeBound = (HexBound)srcBound;
            // TODO: Use operator*
            if (!TryApplySymmetry(s, (Cell)(cubeBound.min), out var a, out var _))
            {
                return false;
            }
            // This trick works best with *inclusive* bounds.
            if (!TryApplySymmetry(s, (Cell)(cubeBound.max - Vector3Int.one), out var b, out var _))
            {
                return false;
            }
            destBound = new HexBound(Vector3Int.Min((Vector3Int)(a), (Vector3Int)(b)), Vector3Int.Max((Vector3Int)(a), (Vector3Int)(b)) + Vector3Int.one);
            return true;
        }
        public bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            return TryMoveByOffset(s.Dest, (Vector3Int)s.Src, (Vector3Int)src, s.Rotation, out dest, out r);
        }
        #endregion
    }
}
