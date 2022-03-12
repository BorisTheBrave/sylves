using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    
    class PlanarPrismOptions
    {
        public float LayerHeight { get; set; } = 1;
        public float LayerOffset { get; set; }
    }

    public class PlanarPrismBound : IBound
    {
        public int MinLayer { get; set; }
        public int MaxLayer { get; set; }

        public IBound PlanarBound { get; set; }

        public PlanarPrismBound Intersect(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Max(MinLayer, other.MinLayer),
                MaxLayer = Math.Min(MaxLayer, other.MaxLayer),
                PlanarBound = planarGrid.IntersectBounds(PlanarBound, other.PlanarBound),
            };
        }

        public PlanarPrismBound Union(PlanarPrismBound other, IGrid planarGrid)
        {
            return new PlanarPrismBound
            {
                MinLayer = Math.Min(MinLayer, other.MinLayer),
                MaxLayer = Math.Max(MaxLayer, other.MaxLayer),
                PlanarBound = planarGrid.UnionBounds(PlanarBound, other.PlanarBound),
            };
        }
    }

    // Doesn't use BaseModifier as so much has changed, everything needs overriding.
    internal class PlanarPrismModifier : IGrid
    {
        private readonly IGrid underlying;
        private readonly PlanarPrismOptions planarPrismOptions;
        private readonly PlanarPrismBound bound;

        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions, int minLayer, int maxLayer)
            : this(underlying, planarPrismOptions, new PlanarPrismBound { MinLayer = minLayer, MaxLayer = maxLayer, PlanarBound = null })
        {
        }


        public PlanarPrismModifier(IGrid underlying, PlanarPrismOptions planarPrismOptions = null, PlanarPrismBound bound = null)
        {
            this.underlying = underlying;
            this.planarPrismOptions = planarPrismOptions ?? new PlanarPrismOptions();
            this.bound = bound;
            if (!underlying.Is2D)
            {
                throw new Exception("Underlying should be a 2d grid");
            }
            if (!underlying.IsPlanar)
            {
                throw new Exception("Underlying should be a planar grid");
            }
        }

        private (Cell cell, int layer) Split(Cell cell)
        {
            return (new Cell(cell.x, cell.y), cell.z);
        }

        private Cell Combine(Cell cell, int layer)
        {
            return new Cell(cell.x, cell.y, layer);
        }

        private static ICellType PrismCellType(ICellType underlyingCellType)
        {
            if (underlyingCellType == SquareCellType.Instance)
            {
                return CubeCellType.Instance;
            }
            else if (underlyingCellType == HexCellType.Get(HexOrientation.FlatTopped))
            {
                return HexPrismCellType.Get(HexOrientation.FlatTopped);
            }
            else if (underlyingCellType == HexCellType.Get(HexOrientation.PointyTopped))
            {
                return HexPrismCellType.Get(HexOrientation.PointyTopped);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void GetAxialDirs(ICellType underlyingCellType, out CellDir forwardDir, out CellDir backDir)
        {

            if (underlyingCellType == SquareCellType.Instance)
            {
                forwardDir = (CellDir)CubeDir.Forward;
                backDir = (CellDir)CubeDir.Back;
            }
            else if(underlyingCellType == HexCellType.Get(HexOrientation.FlatTopped) || underlyingCellType == HexCellType.Get(HexOrientation.PointyTopped))
            {
                forwardDir = (CellDir)PTHexPrismDir.Forward;
                backDir = (CellDir)PTHexPrismDir.Back;
            }
            else
            {
                throw new NotImplementedException($"Cell type {underlyingCellType.GetType()} not implemented yet");
            }
        }

        private bool IsAxial(ICellType underlyingCellType, CellDir cellDir, out bool isForward, out CellDir inverseDir)
        {
            if (underlyingCellType == SquareCellType.Instance)
            {
                isForward = (int)cellDir == (int)CubeDir.Forward;
                inverseDir = (CellDir)((int)CubeDir.Forward + (int)CubeDir.Back - (int)cellDir);
                return (int)cellDir >= (int)CubeDir.Forward;
            }
            else if (underlyingCellType == HexCellType.Get(HexOrientation.FlatTopped) || underlyingCellType == HexCellType.Get(HexOrientation.PointyTopped))
            {
                isForward = (int)cellDir == (int)PTHexPrismDir.Forward;
                inverseDir = (CellDir)((int)PTHexPrismDir.Forward + (int)PTHexPrismDir.Back - (int)cellDir);
                return (int)cellDir >= (int)PTHexPrismDir.Forward;
            }
            else
            {
                throw new NotImplementedException($"Cell type {underlyingCellType.GetType()} not implemented yet");
            }
        }

        private Vector3 GetOffset(int layer)
        {
            return (planarPrismOptions.LayerOffset + planarPrismOptions.LayerHeight * layer) * Vector3.forward;
        }
        private int GetLayer(Vector3 position)
        {
            return Mathf.RoundToInt((position.z - planarPrismOptions.LayerOffset) / planarPrismOptions.LayerHeight);
        }

        private void CheckBounded()
        {
            if (bound == null)
            {
                throw new GridInfiniteException();
            }
        }

        #region Basics

        public virtual bool Is2D => false;

        public virtual bool Is3D => true;

        public virtual bool IsPlanar => false;

        public virtual bool IsRepeating => underlying.IsRepeating;

        public virtual bool IsOrientable => underlying.IsOrientable;

        public virtual bool IsFinite => bound != null && underlying.IsFinite;

        public virtual bool IsSingleCellType => underlying.IsSingleCellType;

        public virtual IEnumerable<ICellType> GetCellTypes() => underlying.GetCellTypes().Select(PrismCellType);

        #endregion

        #region Relatives

        public virtual IGrid Unbounded => new PlanarPrismModifier(underlying.Unbounded, new PlanarPrismOptions
        {
            LayerHeight = planarPrismOptions.LayerHeight,
            LayerOffset = planarPrismOptions.LayerOffset,
        });

        public virtual IGrid Unwrapped => underlying.Unwrapped;
        public virtual IGrid Underlying => underlying;
        #endregion

        #region Cell info

        public virtual IEnumerable<Cell> GetCells()
        {
            CheckBounded();
            foreach (var cell in underlying.GetCells())
            {
                for (var layer = bound.MinLayer; layer < bound.MaxLayer; layer++)
                {
                    yield return Combine(cell, layer);
                }
            }

        }

        public virtual ICellType GetCellType(Cell cell) => PrismCellType(underlying.GetCellType(cell));
        #endregion

        #region Topology

        public virtual bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if (IsAxial(underlying.GetCellType(cell), dir, out var isUp, out inverseDir))
            {
                var (uCell, layer) = Split(cell);
                layer += (isUp ? 1 : -1);
                dest = Combine(uCell, layer);
                connection = new Connection();
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
            }
            else
            {
                var (uCell, layer) = Split(cell);
                if (!underlying.TryMove(uCell, dir, out var destUCell, out inverseDir, out connection))
                {
                    dest = default;
                    return false;
                }
                dest = Combine(destUCell, layer);
                return true;
            }
        }

        public virtual bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            foreach (var dir in underlying.GetCellDirs(cell))
            {
                yield return dir;
            }
            var cellType = underlying.GetCellType(cell);
            GetAxialDirs(cellType, out var forwardDir, out var backDir);
            yield return forwardDir;
            yield return backDir;
        }
        public virtual IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            var (startUCell, startLayer) = Split(startCell);
            var (destUCell, destLayer) = Split(destCell);
            var path = underlying.FindBasicPath(startUCell, destUCell);
            if (path == null)
                return null;
            IEnumerable<(Cell, CellDir)> DoPath()
            {
                var cellType = underlying.GetCellType(startUCell);
                GetAxialDirs(cellType, out var forwardDir, out var backDir);
                var layer = startLayer;
                while (layer < destLayer)
                {
                    yield return (Combine(startUCell, layer), forwardDir);
                    layer += 1;
                }
                while (layer > destLayer)
                {
                    yield return (Combine(startUCell, layer), backDir);
                    layer -= 1;
                }
                foreach (var (uCell, dir) in path)
                {
                    yield return (Combine(uCell, layer), dir);
                }
            }
            return DoPath();
        }

        #endregion

        #region Index
        public virtual int IndexCount
        {
            get
            {
                CheckBounded();
                return underlying.IndexCount * (bound.MaxLayer - bound.MinLayer);
            }
        }

        public virtual int GetIndex(Cell cell) {
            CheckBounded();
            var (ucell, layer) = Split(cell);
            return underlying.GetIndex(ucell) * (bound.MaxLayer - bound.MinLayer) + (layer - bound.MinLayer);
        }

        public virtual Cell GetCellByIndex(int index)
        {
            var uindex = index / (bound.MaxLayer - bound.MinLayer);
            var layer = index % (bound.MaxLayer - bound.MinLayer) + bound.MinLayer;
            var ucell = underlying.GetCellByIndex(uindex);
            return Combine(ucell, layer);
        }
        #endregion

        #region Bounds

        public IBound GetBound() => bound;
        public virtual IBound GetBound(IEnumerable<Cell> cells) => underlying.GetBound(cells);

        public virtual IGrid BoundBy(IBound bound)
        {
            if (bound == null) return this;
            PlanarPrismBound planarPrismBound = (PlanarPrismBound)bound;
            return new PlanarPrismModifier(
                underlying.BoundBy(planarPrismBound.PlanarBound),
                planarPrismOptions,
                (PlanarPrismBound)IntersectBounds(bound, planarPrismBound)
                );
        }

        public virtual IBound IntersectBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((PlanarPrismBound)bound).Intersect((PlanarPrismBound)other, underlying);
        }

        public virtual IBound UnionBounds(IBound bound, IBound other)
        {
            if (bound == null) return other;
            if (other == null) return bound;
            return ((PlanarPrismBound)bound).Union((PlanarPrismBound)other, underlying);
        }
        public virtual IEnumerable<Cell> GetCellsInBounds(IBound bound)
        {
            var planarPrismBound = (PlanarPrismBound)bound;
            foreach (var uCell in underlying.GetCellsInBounds(planarPrismBound.PlanarBound))
            {
                for (var layer = planarPrismBound.MinLayer; layer < planarPrismBound.MaxLayer; layer++)
                {
                    yield return Combine(uCell, layer);
                }
            }
        }
        #endregion

        #region Position

        public virtual Vector3 GetCellCenter(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            return underlying.GetCellCenter(uCell) + GetOffset(layer);
        }

        public virtual TRS GetTRS(Cell cell)
        {

            var (uCell, layer) = Split(cell);
            var trs = underlying.GetTRS(uCell);
            return new TRS(trs.Position + GetOffset(layer), trs.Rotation, trs.Scale);
        }

        public virtual Deformation GetDeformation(Cell cell)
        {
            var (uCell, layer) = Split(cell);
            var deformation = underlying.GetDeformation(uCell);
            return Matrix4x4.Translate(GetOffset(layer)) * deformation;
        }
        #endregion

        #region Query
        private Vector3 GetPlanarPosition(Vector3 position) => new Vector3(position.x, position.y, 0);
        public virtual bool FindCell(Vector3 position, out Cell cell)
        {
            if (!underlying.FindCell(GetPlanarPosition(position), out var uCell))
            {
                cell = default;
                return false;
            }
            var layer = GetLayer(position);
            cell = Combine(uCell, layer);
            return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
        }

        public virtual bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var position = matrix.MultiplyPoint3x4(Vector3.zero);
            var layer = GetLayer(position);
            if (!underlying.FindCell(GetPlanarPosition(position), out var uCell))
            {
                cell = default;
                rotation = default;
                return false;
            }
            cell = Combine(uCell, layer);
            var cellType = underlying.GetCellType(uCell);
            if(cellType == SquareCellType.Instance)
            {
                var trs = underlying.GetTRS(uCell);
                var cubeRotation = CubeRotation.FromMatrix(trs.ToMatrix().inverse * matrix);
                if(cubeRotation == null)
                {
                    rotation = default;
                    return false;
                }
                rotation = cubeRotation.Value;
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
            }
            else
            {
                // All other cell types just inherit rotation from their underlying
                if(!underlying.FindCell(matrix, out var _, out rotation))
                {
                    cell = default;
                    rotation = default;
                    return false;
                }
                return bound == null ? true : bound.MinLayer <= layer && layer < bound.MaxLayer;
            }
        }

        public virtual IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var minLayer = GetLayer(min);
            var maxLayer = GetLayer(max);
            if(bound != null)
            {
                minLayer = Math.Max(minLayer, bound.MinLayer);
                maxLayer = Math.Min(maxLayer, bound.MaxLayer);
            }

            foreach (var uCell in underlying.GetCellsIntersectsApprox(GetPlanarPosition(min), GetPlanarPosition(max)))
            {
                for (var layer = minLayer; layer < maxLayer; layer++)
                {
                    yield return Combine(uCell, layer);
                }
            }
        }
        #endregion

    }
}
