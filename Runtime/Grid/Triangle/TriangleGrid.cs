using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    /// <summary>
    /// A uniform tiling of triangles.
    /// TriangleOrientation.FlatSides gives columns of triangles that alternate pointing left/right.
    /// TriangleOrientation.FlatTopped gives rows of triangles that alternate pointing up/down.
    /// 
    /// In both cases, the cell type used is actually TriangleCellType. For each triangle, three of the directions
    /// point towards neighbors, and the other three will point to nothing.
    /// This is a similar setup to <see cref="MeshGridOptions.DoubleOddFaces"/>, and is usually simpler to work with
    /// as it doesn't force you to consider some cells as rotated by 180 degrees.
    /// </summary>
    public class TriangleGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType[] ftCellTypes = { TriangleCellType.Get(TriangleOrientation.FlatTopped) };
        private static readonly ICellType[] fsCellTypes = { TriangleCellType.Get(TriangleOrientation.FlatSides) };

        // Also used for FSTriangleDir Right, UpLeft, DownLeft
        private static readonly CellDir[] cellDirsA = { (CellDir)FTTriangleDir.UpRight, (CellDir)FTTriangleDir.UpLeft, (CellDir)FTTriangleDir.Down };
        // Also used for FSTriangleDir UpRight, Left, DownRight
        private static readonly CellDir[] cellDirsB = { (CellDir)FTTriangleDir.Up, (CellDir)FTTriangleDir.DownLeft, (CellDir)FTTriangleDir.DownRight };


        // Also used for FSTriangleDir Right, UpLeft, DownLeft
        private static readonly CellCorner[] cellCornersA = { (CellCorner)FTTriangleCorner.DownRight, (CellCorner)FTTriangleCorner.Up, (CellCorner)FTTriangleCorner.DownLeft };
        // Also used for FSTriangleDir UpRight, Left, DownRight
        private static readonly CellCorner[] cellCornersB = { (CellCorner)FTTriangleDir.UpRight, (CellCorner)FTTriangleDir.UpLeft, (CellCorner)FTTriangleDir.Down };

        // The triangle polygons, scaled to fit in a unit square
        private static readonly Vector3[] upPolygon =
        {
            new Vector3(0.5f, -0.3333333333f, 0),
            new Vector3(0, 0.6666666667f, 0),
            new Vector3(-0.5f, -0.3333333333f, 0),
        };

        private static readonly Vector3[] downPolygon =
        {
            new Vector3(0.5f, 0.3333333333f, 0),
            new Vector3(-0.5f, 0.3333333333f, 0),
            new Vector3(0, -0.6666666667f, 0),
        };

        private static readonly Vector3[] leftPolygon =
        {
            new Vector3(0.3333333333f, 0.5f, 0),
            new Vector3(-0.6666666667f, 0, 0),
            new Vector3(0.3333333333f, -0.5f, 0),
        };

        private static readonly Vector3[] rightPolygon =
        {
            new Vector3(0.6666666667f, 0, 0),
            new Vector3(-0.3333333333f, 0.5f, 0),
            new Vector3(-0.3333333333f, -0.5f, 0),
        };

        readonly ICellType cellType;

        private readonly TriangleBound bound;

        private readonly Vector2 cellSize;

        private readonly TriangleOrientation orientation;

        private readonly IGrid altGrid;

        internal static Vector2 ComputeCellSize(float cellSide, TriangleOrientation orientation)
        {
            return cellSide * (orientation == TriangleOrientation.FlatTopped ? new Vector2(1, Sqrt3 / 2) : new Vector2(Sqrt3 / 2, 1));
        }

        /// <summary>
        /// Creates a triangle grid where each triangle is equilateral and has each side length of cellSide.
        /// I.e. the incircle diameter is cellSide * sqrt(1/3) and circumcircle diameter is 2 * cellSide * sqrt(1/3)
        /// </summary>
        public TriangleGrid(float cellSide, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
            :this(ComputeCellSize(cellSide, orientation), orientation, bound)
        {
        }

        /// <summary>
        /// Creates a triangle grid where each triangle has width and height given by cellSize.x and cellSize.y
        /// </summary>
        public TriangleGrid(Vector2 cellSize, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            this.cellType = TriangleCellType.Get(orientation);
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

        /// <summary>
        /// Creates a triangle grid where each triangle is equilateral and the given incircle diameter.
        /// The incircle diameter is the distance between the centers of two adjacent triangles.
        /// </summary>
        public static TriangleGrid WithIncircleDiameter(float incircleDiameter, TriangleOrientation orientation = TriangleOrientation.FlatTopped, TriangleBound bound = null)
        {
            return new TriangleGrid(incircleDiameter * Sqrt3, orientation, bound);
        }

        private static Cell SwizzleXY(Cell c) => new Cell(c.y, c.x, c.z);

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        public TriangleOrientation Orientation => orientation;

        public Vector2 CellSize => cellSize;

        public bool IsUp(Cell cell) => orientation == TriangleOrientation.FlatTopped && cell.x + cell.y + cell.z == 2;
        public bool IsDown(Cell cell) => orientation == TriangleOrientation.FlatTopped && cell.x + cell.y + cell.z == 1;
        public bool IsLeft(Cell cell) => orientation == TriangleOrientation.FlatSides && cell.x + cell.y + cell.z == 1;
        public bool IsRight(Cell cell) => orientation == TriangleOrientation.FlatSides && cell.x + cell.y + cell.z == 2;
        private bool IsUpOrLeft(Cell cell) => (orientation == TriangleOrientation.FlatSides) ^ (cell.x + cell.y + cell.z == 2);

        #region Basics
        public bool Is2d => true;

        public bool Is3d => false;

        public bool IsPlanar => true;

        public bool IsRepeating => true;

        public bool IsOrientable => true;

        public bool IsFinite => bound != null;

        public bool IsSingleCellType => true;

        public int CoordinateDimension => 3;

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

        public virtual IDualMapping GetDual()
        {
            // TODO: This seems right, but I haven't really validated
            var dualBound = bound == null ? null :
                new HexBound(bound.min - Vector3Int.one, bound.max);

            // Note hex orientation is flipped vs triangle orientation
            if (orientation == TriangleOrientation.FlatTopped)
            {
                var hexCellSize = new Vector2(cellSize.x, cellSize.y * (4f / 3));
                return new DualMapping(this, new HexGrid(hexCellSize, HexOrientation.PointyTopped, dualBound));
            }
            else
            {

                var hexCellSize = new Vector2(cellSize.x * (4f / 3), cellSize.y);
                return new DualMapping(this, new HexGrid(hexCellSize, HexOrientation.FlatTopped, dualBound));
            }
        }

        internal class DualMapping : BasicDualMapping
        {
            private TriangleOrientation baseOrientation;

            public DualMapping(TriangleGrid baseGrid, HexGrid dualGrid) : base(baseGrid, dualGrid)
            {
                baseOrientation = baseGrid.orientation;
            }
            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner)
            {
                Cell dest;
                CellCorner destCorner;
                if (baseOrientation == TriangleOrientation.FlatTopped)
                {
                    switch ((FTTriangleCorner)corner)
                    {
                        // See HexGrid.TryMove
                        case FTTriangleCorner.UpRight: dest = cell + new Vector3Int(0, 0, -1); destCorner = (CellCorner)PTHexCorner.DownLeft; break;
                        case FTTriangleCorner.Up: dest = cell + new Vector3Int(-1, 0, -1); destCorner = (CellCorner)PTHexCorner.Down; break;
                        case FTTriangleCorner.UpLeft: dest = cell + new Vector3Int(-1, 0, 0); destCorner = (CellCorner)PTHexCorner.DownRight; break;
                        case FTTriangleCorner.DownLeft: dest = cell + new Vector3Int(-1, -1, 0); destCorner = (CellCorner)PTHexCorner.UpRight; break;
                        case FTTriangleCorner.Down: dest = cell + new Vector3Int(0, -1, 0); destCorner = (CellCorner)PTHexCorner.Up; break;
                        case FTTriangleCorner.DownRight: dest = cell + new Vector3Int(0, -1, -1); destCorner = (CellCorner)PTHexCorner.UpLeft; break;
                        default:
                            throw new Exception($"Unexpected corner {corner}");
                    }
                }
                else
                {
                    switch ((FSTriangleCorner)corner)
                    {
                        // See HexGrid.TryMove
                        case FSTriangleCorner.Right: dest = cell + new Vector3Int(0, -1, -1); destCorner = (CellCorner)FTHexCorner.Left; break;
                        case FSTriangleCorner.UpRight: dest = cell + new Vector3Int(0, 0, -1); destCorner = (CellCorner)FTHexCorner.DownLeft; break;
                        case FSTriangleCorner.UpLeft: dest = cell + new Vector3Int(-1, 0, -1); destCorner = (CellCorner)FTHexCorner.DownRight; break;
                        case FSTriangleCorner.Left: dest = cell + new Vector3Int(-1, 0, 0); destCorner = (CellCorner)FTHexCorner.Right; break;
                        case FSTriangleCorner.DownLeft: dest = cell + new Vector3Int(-1, -1, 0); destCorner = (CellCorner)FTHexCorner.UpRight; break;
                        case FSTriangleCorner.DownRight: dest = cell + new Vector3Int(0, -1, 0); destCorner = (CellCorner)FTHexCorner.UpLeft; break;
                        default:
                            throw new Exception($"Unexpected corner {corner}");
                    }
                }
                var s = dest.x + dest.y + dest.z;
                if (s != 0 || !DualGrid.IsCellInGrid(dest))
                {
                    return null;
                }
                return (dest, destCorner);
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner)
            {
                Cell dest;
                CellCorner destCorner;
                if (baseOrientation == TriangleOrientation.FlatTopped)
                {
                    switch ((PTHexCorner)corner)
                    {
                        // See HexGrid.TryMove
                        case PTHexCorner.DownLeft: dest = cell - new Vector3Int(0, 0, -1); destCorner = (CellCorner)FTTriangleCorner.UpRight; break;
                        case PTHexCorner.Down: dest = cell - new Vector3Int(-1, 0, -1); destCorner = (CellCorner)FTTriangleCorner.Up; break;
                        case PTHexCorner.DownRight: dest = cell - new Vector3Int(-1, 0, 0); destCorner = (CellCorner)FTTriangleCorner.UpLeft; break;
                        case PTHexCorner.UpRight: dest = cell - new Vector3Int(-1, -1, 0); destCorner = (CellCorner)FTTriangleCorner.DownLeft; break;
                        case PTHexCorner.Up: dest = cell - new Vector3Int(0, -1, 0); destCorner = (CellCorner)FTTriangleCorner.Down; break;
                        case PTHexCorner.UpLeft: dest = cell - new Vector3Int(0, -1, -1); destCorner = (CellCorner)FTTriangleCorner.DownRight; break;
                        default:
                            throw new Exception($"Unexpected corner {corner}");
                    }
                }
                else
                {
                    switch ((FTHexCorner)corner)
                    {
                        // See HexGrid.TryMove
                        case FTHexCorner.Left: dest = cell - new Vector3Int(0, -1, -1); destCorner = (CellCorner)FSTriangleCorner.Right; break;
                        case FTHexCorner.DownLeft: dest = cell - new Vector3Int(0, 0, -1); destCorner = (CellCorner)FSTriangleCorner.UpRight; break;
                        case FTHexCorner.DownRight: dest = cell - new Vector3Int(-1, 0, -1); destCorner = (CellCorner)FSTriangleCorner.UpLeft; break;
                        case FTHexCorner.Right: dest = cell - new Vector3Int(-1, 0, 0); destCorner = (CellCorner)FSTriangleCorner.Left; break;
                        case FTHexCorner.UpRight: dest = cell - new Vector3Int(-1, -1, 0); destCorner = (CellCorner)FSTriangleCorner.DownLeft; break;
                        case FTHexCorner.UpLeft: dest = cell - new Vector3Int(0, -1, 0); destCorner = (CellCorner)FSTriangleCorner.DownRight; break;
                        default:
                            throw new Exception($"Unexpected corner {corner}");
                    }
                }
                var s = dest.x + dest.y + dest.z;
                if ((s != 1 && s != 2) || !DualGrid.IsCellInGrid(dest))
                {
                    return null;
                }
                return (dest, destCorner);
            }
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

        public IEnumerable<CellCorner> GetCellCorners(Cell cell)
        {
            if (IsUpOrLeft(cell))
            {
                return cellCornersB;
            }
            else
            {
                return cellCornersA;
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
                max = Vector3Int.Max(max, current);
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

        public Vector3 GetCellCorner(Cell cell, CellCorner corner)
        {
            GetPolygon(cell, out var vertices, out var transform);
            return transform.MultiplyPoint3x4(vertices[(int)corner / 2]);
        }

        /// <summary>
        /// Returns the appropriate transform for the cell.
        /// The translation will always be to GetCellCenter.
        /// Not inclusive of cell rotation, that should be applied first.
        /// </summary>
        public TRS GetTRS(Cell cell) => new TRS(
            GetCellCenter(cell),
            Quaternion.identity,
            // Inverse of ComputeCellSize.
            // Should this be saved somewhere?
            new Vector3(cellSize.x / (orientation == TriangleOrientation.FlatTopped ? 1 : Sqrt3 / 2), cellSize.y / (orientation == TriangleOrientation.FlatTopped ? Sqrt3 / 2 : 1), 1));
        #endregion

        #region Shape
        public Deformation GetDeformation(Cell cell) => Deformation.Identity;

        public void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            if (IsUp(cell))
            {
                vertices = upPolygon;
            }
            else if (IsDown(cell))
            {
                vertices = downPolygon;
            }
            else if (IsLeft(cell))
            {
                vertices = leftPolygon;
            }
            else
            {
                vertices = rightPolygon;
            }
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
            if (orientation == TriangleOrientation.FlatSides)
            {
                var x = position.x / cellSize.x;
                var y = position.y / cellSize.y;
                cell = new Cell(
                    Mathf.FloorToInt(x) + 1,
                    Mathf.CeilToInt(y - 0.5f * x),
                    Mathf.CeilToInt(-y - 0.5f * x)
                );
                return IsCellInGrid(cell);
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
                return IsCellInGrid(cell);
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
            // TODO: Respect bounds
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

        public IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            if (orientation == TriangleOrientation.FlatSides)
            {
                foreach (var info in altGrid.Raycast(origin, direction, maxDistance))
                {
                    yield return info;
                }
                yield break;
            }

            // Normalize things
            var x1 = origin.x / cellSize.x;
            var y1 = origin.y / cellSize.y;
            var dx = direction.x / cellSize.x;
            var dy = direction.y / cellSize.y;

            // Convert from cartesian co-ordinates to the three triangle axes
            var fa = x1 - 0.5f * y1;
            var fb = y1;
            var fc = -x1 - 0.5f * y1;
            var da = dx - 0.5f * dy;
            var db = dy;
            var dc = -dx - 0.5f * dy;


            var stepa = da >= 0 ? 1 : -1;
            var stepb = db >= 0 ? 1 : -1;
            var stepc = dc >= 0 ? 1 : -1;
            var ida = Math.Abs(1 / da);
            var idb = Math.Abs(1 / db);
            var idc = Math.Abs(1 / dc);
            var cellDirX = (CellDir)(da >= 0 ? FTHexDir.UpLeft: FTHexDir.DownRight);
            var cellDirY = (CellDir)(db >= 0 ? FTHexDir.Down : FTHexDir.Up);
            var cellDirZ = (CellDir)(dc >= 0 ? FTHexDir.UpRight : FTHexDir.DownLeft);

            // -1 = in middle of cell, 0,1,2 = on x,y,z face
            int startOnBorder;
            float extraDistance;
            // Filter to bounds
            if (bound != null)
            {
                var ta1 = da == 0 ? (bound.min.x > fa ? 1 : -1) * float.PositiveInfinity : da >= 0 ? (bound.min.x - fa) / da : (bound.max.x - fa) / da;
                var ta2 = da == 0 ? (bound.max.x > fa ? 1 : -1) * float.PositiveInfinity : da >= 0 ? (bound.max.x - fa) / da : (bound.min.x - fa) / da;
                var tb1 = db == 0 ? (bound.min.y > fb ? 1 : -1) * float.PositiveInfinity : db >= 0 ? (bound.min.y - fb) / db : (bound.max.y - fb) / db;
                var tb2 = db == 0 ? (bound.max.y > fb ? 1 : -1) * float.PositiveInfinity : db >= 0 ? (bound.max.y - fb) / db : (bound.min.y - fb) / db;
                var tc1 = dc == 0 ? (bound.min.z > fc ? 1 : -1) * float.PositiveInfinity : dc >= 0 ? (bound.min.z - fc) / dc : (bound.max.z - fc) / dc;
                var tc2 = dc == 0 ? (bound.max.z > fc ? 1 : -1) * float.PositiveInfinity : dc >= 0 ? (bound.max.z - fc) / dc : (bound.min.z - fc) / dc;

                var mint = Math.Max(ta1, Math.Max(tb1, tc1));
                var maxt = Math.Min(ta2, Math.Min(tb2, tc2));
                // Don't go beyond maxt
                maxDistance = Math.Min(maxDistance, maxt);

                if (mint > 0)
                {
                    // Advance things to mint
                    fa += da * mint;
                    fb += db * mint;
                    fc += dc * mint;
                    maxDistance -= mint;
                    extraDistance = mint;
                    origin += direction * mint;
                    if (ta1 == mint)
                    {
                        startOnBorder = 0;
                        fa = 0;
                    }
                    else if (tb1 == mint)
                    {
                        startOnBorder = 1;
                        fb = 0;
                    }
                    else
                    {
                        startOnBorder = 2;
                        fc = 0;
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

            var a = startOnBorder == 0 ? Mathf.RoundToInt(fa) + (da > 0 ? -1 : 0) : Mathf.CeilToInt(fa);
            var b = startOnBorder == 1 ? Mathf.RoundToInt(fb) + (db > 0 ? -1 : 0) : Mathf.FloorToInt(fb) + 1;
            var c = startOnBorder == 2 ? Mathf.RoundToInt(fc) + (dc > 0 ? -1 : 0) : Mathf.CeilToInt(fc);

            if (startOnBorder == -1)
            {
                yield return new RaycastInfo
                {
                    cell = new Cell(a, b, c),
                    point = origin,
                    cellDir = null,
                    distance = 0,
                };
            }

            var ta = (a - 1 + (da >= 0 ? 1 : 0) - fa) / da;
            var tb = (b - 1 + (db >= 0 ? 1 : 0) - fb) / db;
            var tc = (c - 1 + (dc >= 0 ? 1 : 0) - fc) / dc;
            var isUp = a + b + c == 2;

            while (true)
            {
                // Find the next line crossed. We filter out lines that 
                // aren't bordering the current triangle, do deal with precision issues
                var ta2 = (stepa == 1) != isUp ? ta : float.PositiveInfinity;
                var tb2 = (stepb == 1) != isUp ? tb : float.PositiveInfinity;
                var tc2 = (stepc == 1) != isUp ? tc : float.PositiveInfinity;


                float t;
                CellDir cellDir;
                if (ta2 <= tb2 && ta2 <= tc2)
                {
                    if (ta > maxDistance) yield break;
                    t = ta;
                    a += stepa;
                    ta += ida;
                    cellDir = cellDirX;
                    if (bound != null && (a >= bound.max.x || a < bound.min.x)) yield break;
                }
                else if (tb2 <= ta2 && tb2 <= tc2)
                {
                    if (tb > maxDistance) yield break;
                    t = tb;
                    b += stepb;
                    tb += idb;
                    cellDir = cellDirY;
                    if (bound != null && (b >= bound.max.y || b < bound.min.y)) yield break;
                }
                else if(!float.IsInfinity(tc))
                {
                    if (tc > maxDistance) yield break;
                    t = tc;
                    c += stepc;
                    tc += idc;
                    cellDir = cellDirZ;
                    if (bound != null && (c >= bound.max.z || c < bound.min.z)) yield break;
                }
                else
                {
                    yield break;
                }
                yield return new RaycastInfo
                {
                    cell = new Cell(a, b, c),
                    point = origin + t * direction,
                    cellDir = cellDir,
                    distance = t + extraDistance,
                };
                isUp = !isUp;
            }
        }
        #endregion

        #region Symmetry

        public GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation)
        {
            var hexRotation = (HexRotation)cellRotation;
            var srcBound = GetBound(src);
            var srcMin = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var srcMax = src.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Max);
            var r1 = hexRotation.Multiply(srcMin);
            var r2 = hexRotation.Multiply(srcMax);
            var newMin = Vector3Int.Min(r1, r2);
            var destMin = dest == src ? srcMin : dest.Select(x => (Vector3Int)x).Aggregate(Vector3Int.Min);
            var translation = destMin - newMin;
            // Check partity
            if ((hexRotation.Rotation % 2 == 0) ^ ((translation.x + translation.y + translation.z) % 2 == 0))
            {
                return null;
            }

            // Check it actually works
            if (!src.Select(c => (Cell)(translation + hexRotation.Multiply((Vector3Int)c))).All(dest.Contains))
            {
                return null;
            }

            return new GridSymmetry
            {
                Src = srcCell,
                Dest = (Cell)(translation + hexRotation.Multiply((Vector3Int)srcCell)),
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
            if (!TryApplySymmetry(s, (Cell)(cubeBound.max - Vector3Int.one), out var b, out var _))
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
