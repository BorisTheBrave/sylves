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
        // Neighbour diagram (for 0,0,0)
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

    public class HexGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType cellType = NGonCellType.Get(6);
        private static readonly ICellType[] cellTypes = {  cellType };

        HexBound bound;

        // cellSize measures the actual bounds of a single cell.
        // Thus, to get a regular hexagon, one of the values should be sqrt(3)/2 times smaller than the other (depending on Flat/PointyTopped
        // Note: Unity basically always uses PointyTopped, as other orientations are handled by a swizzle that just affects world position, not cell indices
        Vector2 cellSize;

        HexOrientation orientation;

        public HexGrid(float cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
            :this(cellSize *(orientation == HexOrientation.FlatTopped ? new Vector2(Sqrt3 / 2, 1) : new Vector2(1, Sqrt3 / 2)), orientation, bound)
        {
        }

        public HexGrid(Vector2 cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
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
                    return new HexGrid(cellSize, orientation, null);
                }

            }
        }
        #endregion

        #region Cell info

        public IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            return GetCellsInBounds(bound);
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
            inverseDir = (CellDir)(6 - (int)dir);
            connection = new Connection();
            switch((FTHexDir)dir)
            {
                case FTHexDir.UpRight:   dest = cell + new Vector3Int(1, 0, -1); break;
                case FTHexDir.Up:        dest = cell + new Vector3Int(0, 1, -1); break;
                case FTHexDir.UpLeft:    dest = cell + new Vector3Int(-1, 1, 0); break;
                case FTHexDir.DownLeft:  dest = cell + new Vector3Int(-1, 0, 1); break;
                case FTHexDir.Down:      dest = cell + new Vector3Int(0, -1, 1); break;
                case FTHexDir.DownRight: dest = cell + new Vector3Int(1, -1, 0); break;
                default: throw new ArgumentException($"Unknownd dir {dir}");
            }
            return true;
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
            throw new NotImplementedException();
        }

        public IGrid BoundBy(IBound bound)
        {
            throw new NotImplementedException();
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            throw new NotImplementedException();

        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            throw new NotImplementedException();

        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        #endregion
    }
}
