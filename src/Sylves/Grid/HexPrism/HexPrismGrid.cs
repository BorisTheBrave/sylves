using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    internal class HexPrismGrid : IGrid
    {
        private const float Sqrt3 = 1.73205080756888f;

        private static readonly ICellType[] ftCellTypes = { HexPrismCellType.Get(HexOrientation.FlatTopped) };
        private static readonly ICellType[] ptCellTypes = { HexPrismCellType.Get(HexOrientation.PointyTopped) };

        private readonly ICellType cellType;

        private readonly HexPrismBound bound;

        private readonly Vector3 cellSize;

        private readonly HexOrientation orientation;

        private readonly HexGrid hexGrid;
        private readonly int hexGridIndexCount;

        public HexPrismGrid(float cellSize, float layerHeight, HexOrientation orientation = HexOrientation.PointyTopped, HexPrismBound bound = null)
            : this(cellSize * (orientation == HexOrientation.PointyTopped ? new Vector3(Sqrt3 / 2, 1, layerHeight) : new Vector3(1, Sqrt3 / 2, layerHeight)), orientation, bound)
        {
        }

        public HexPrismGrid(Vector3 cellSize, HexOrientation orientation = HexOrientation.PointyTopped, HexPrismBound bound = null)
        {
            this.cellSize = cellSize;
            this.orientation = orientation;
            this.bound = bound;
            cellType = HexPrismCellType.Get(orientation);
            hexGrid = new HexGrid(new Vector2(cellSize.x, cellSize.y), orientation, bound?.hexBound);
            hexGridIndexCount = bound != null ? hexGrid.IndexCount : 0;
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

        public bool IsPlanar => false;

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
                    return new HexPrismGrid(cellSize, orientation, null);
                }

            }
        }

        public IGrid Unwrapped => this;

        public HexGrid HexGrid => hexGrid;

        public static Cell GetHexCell(Cell hexPrismCell)
        {
            return new Cell(hexPrismCell.x, hexPrismCell.y, -hexPrismCell.x - hexPrismCell.y);
        }

        // TODO Get rid of this
        private static Vector3Int GetHexCell(Vector3Int hexPrismCell)
        {
            return new Vector3Int(hexPrismCell.x, hexPrismCell.y, -hexPrismCell.x - hexPrismCell.y);
        }

        public static Cell GetHexPrismCell(Cell hexCell, int layer)
        {
            return new Cell(hexCell.x, hexCell.y, layer);
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
        #endregion

        #region Topology

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            // FTHexDir and PTHexDir are arranged so that you don't need different code for the two orientations.
            connection = new Connection();
            switch ((PTHexPrismDir)dir)
            {
                case PTHexPrismDir.Forward:
                    dest = cell + new Vector3Int(0, 0, 1);
                    inverseDir = (CellDir)PTHexPrismDir.Back;
                    return bound == null ? true : bound.Contains(dest);
                case PTHexPrismDir.Back:
                    dest = cell + new Vector3Int(0, 0, -1);
                    inverseDir = (CellDir)PTHexPrismDir.Forward;
                    return bound == null ? true : bound.Contains(dest);
                case PTHexPrismDir.Right: dest = cell + new Vector3Int(1, 0, 0); break;
                case PTHexPrismDir.UpRight: dest = cell + new Vector3Int(0, 1, 0); break;
                case PTHexPrismDir.UpLeft: dest = cell + new Vector3Int(-1, 1, 0); break;
                case PTHexPrismDir.Left: dest = cell + new Vector3Int(-1, 0, 0); break;
                case PTHexPrismDir.DownLeft: dest = cell + new Vector3Int(0, -1, 0); break;
                case PTHexPrismDir.DownRight: dest = cell + new Vector3Int(1, -1, 0); break;
                default: throw new ArgumentException($"Unknown dir {dir}");
            }
            inverseDir = (CellDir)((3 + (int)dir) % 6);
            return bound == null ? true : bound.Contains(dest);
        }

        public bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if(!hexGrid.TryMoveByOffset(GetHexCell(startCell), GetHexCell(startOffset), GetHexCell(destOffset), startRotation, out var destHex, out destRotation))
            {
                destCell = default;
                return false;
            }
            destCell = GetHexPrismCell(destHex, startCell.z + (destOffset.z - startOffset.z));
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
                return hexGridIndexCount * (bound.layerMax - bound.layerMin);
            }
        }

        public int GetIndex(Cell cell)
        {
            CheckBounded();
            return hexGrid.GetIndex(GetHexCell(cell)) + cell.z * hexGridIndexCount;
        }

        public Cell GetCellByIndex(int index)
        {
            var layerCount = hexGridIndexCount;
            var layer = index / layerCount;
            var hexIndex = index % layerCount;
            return GetHexPrismCell(hexGrid.GetCellByIndex(hexIndex), layer);
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
            var hexBound = new HexBound(new Vector3Int(min.x, min.y, -min.x - min.y), new Vector3Int(max.x, max.y, -max.x - max.y) + Vector3Int.one);
            return new HexPrismBound(hexBound, min.z, max.z + 1);
        }

        public IGrid BoundBy(IBound bound)
        {
            return new HexPrismGrid(cellSize, orientation, (HexPrismBound)IntersectBounds(this.bound, bound));
        }

        public IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((HexPrismBound)bound).Intersect((HexPrismBound)other);

        }
        public IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return null;
            if (other == null) return null;
            return ((HexPrismBound)bound).Union((HexPrismBound)other);

        }
        public IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            if (bound == null) throw new Exception("Cannot get cells in null bound as it is infinite");
            return (HexPrismBound)bound;
        }
        #endregion

        #region Position
        /// <summary>
        /// Returns the center of the cell in local space
        /// </summary>
        public Vector3 GetCellCenter(Cell cell)
        {
            return hexGrid.GetCellCenter(GetHexCell(cell)) + cellSize.z * cell.z * Vector3.forward;
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
            if (!hexGrid.FindCell(position, out var hex))
            {
                cell = default;
                return false;
            }

            var z = Mathf.RoundToInt(position.z / cellSize.z);
            cell = GetHexPrismCell(hex, z);
            return true;
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
            var minZ = Mathf.RoundToInt(min.z / cellSize.z);
            var maxZ = Mathf.RoundToInt(max.z / cellSize.z);
            foreach (var hex in hexGrid.GetCellsIntersectsApprox(min, max))
            {
                for (var z = minZ; z <= maxZ; z++)
                {
                    yield return GetHexPrismCell(hex, z);
                }
            }
        }
        #endregion
    }
}
