using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Remaps the cells of the grid by changing their co-ordinates,
    /// without touching the position, shape or topology.
    /// </summary>
    public class BijectModifier : BaseModifier
    {
        private readonly Func<Cell, Cell> toUnderlying;
        private readonly Func<Cell, Cell> fromUnderlying;
        private readonly int coordinateDimension;

        public BijectModifier(IGrid underlying, Func<Cell, Cell> toUnderlying, Func<Cell, Cell> fromUnderlying, int coordinateDimension = 3) : base(underlying)
        {
            this.toUnderlying = toUnderlying;
            this.fromUnderlying = fromUnderlying;
            this.coordinateDimension = coordinateDimension;
        }


        private ISet<Cell> ToUnderlying(ISet<Cell> cells)
        {
            var set = new BijectSet(cells, fromUnderlying, toUnderlying);
            foreach(var cell in set)
            {
                if(!set.Contains(cell))
                {
                    throw new Exception($"Set of {string.Join(",", set)} does not contain {cell}. (originally {string.Join(",", cells)})");
                }
            }
            return set;
        }

        private GridSymmetry FromUnderlying(GridSymmetry s)
        {
            if (s == null) return null;
            return new GridSymmetry
            {
                Rotation = s.Rotation,
                Src = fromUnderlying(s.Src),
                Dest = fromUnderlying(s.Dest),
            };
        }
        private GridSymmetry ToUnderlying(GridSymmetry s)
        {
            if (s == null) return null;
            return new GridSymmetry
            {
                Rotation = s.Rotation,
                Src = toUnderlying(s.Src),
                Dest = toUnderlying(s.Dest),
            };
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new BijectModifier(underlying, toUnderlying, fromUnderlying, coordinateDimension);
        }

        #region Basics
        public override int CoordinateDimension => coordinateDimension;
        #endregion

        #region Relatives

        public override IDualMapping GetDual()
        {
            var dm = Underlying.GetDual();
            return new DualMapping(this, dm.DualGrid, dm);
        }

        private class DualMapping : BasicDualMapping
        {
            private readonly BijectModifier baseGrid;
            private readonly IDualMapping underlyingDualMapping;

            public DualMapping(BijectModifier baseGrid, IGrid dualGrid, IDualMapping underlyingDualMapping) : base(baseGrid, dualGrid)
            {
                this.baseGrid = baseGrid;
                this.underlyingDualMapping = underlyingDualMapping;
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner)
            {
                var t = underlyingDualMapping.ToBasePair(dualCell, corner);
                if (t == null)
                    return null;
                return (baseGrid.fromUnderlying(t.Value.baseCell), t.Value.inverseCorner);
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner)
            {
                return underlyingDualMapping.ToDualPair(baseGrid.toUnderlying(baseCell), corner);
            }
        }

        #endregion

        #region Cell info

        public override IEnumerable<Cell> GetCells() => Underlying.GetCells().Select(fromUnderlying);

        public override ICellType GetCellType(Cell cell) => Underlying.GetCellType(toUnderlying(cell));

        public override bool IsCellInGrid(Cell cell) => Underlying.IsCellInGrid(toUnderlying(cell));

        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if(Underlying.TryMove(toUnderlying(cell), dir, out var uDest, out inverseDir, out connection))
            {
                dest = fromUnderlying(uDest);
                return true;
            }
            else
            {
                dest = default;
                return false;
            }
        }

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if (Underlying.TryMoveByOffset(toUnderlying(startCell), startOffset, destOffset, startRotation, out var uDestCell, out destRotation))
            {
                destCell = fromUnderlying(uDestCell);
                return true;
            }
            else
            {
                destCell = default;
                return false;
            }
        }

        public override bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if(Underlying.ParallelTransport(aGrid, aSrcCell, aDestCell, toUnderlying(srcCell), startRotation, out var destUCell, out destRotation))
            {
                destCell = fromUnderlying(destUCell);
                return true;
            }
            else
            {
                destCell = default;
                return false;
            }
        }

        public override IEnumerable<CellDir> GetCellDirs(Cell cell) => Underlying.GetCellDirs(toUnderlying(cell));

        public override IEnumerable<(Cell, CellDir)> FindBasicPath(Cell startCell, Cell destCell)
        {
            return Underlying.FindBasicPath(toUnderlying(startCell), toUnderlying(destCell))
                .Select(((Cell cell, CellDir cellDir) x) => (fromUnderlying(x.cell), x.cellDir));
        }

        #endregion

        #region Index
        public override int GetIndex(Cell cell) => Underlying.GetIndex(toUnderlying(cell));

        public override Cell GetCellByIndex(int index) => fromUnderlying(Underlying.GetCellByIndex(index));
        #endregion


        #region Bounds
        public override IBound GetBound(IEnumerable<Cell> cells) => Underlying.GetBound(cells.Select(toUnderlying));

        public override bool IsCellInBound(Cell cell, IBound bound) => Underlying.IsCellInBound(toUnderlying(cell), bound);
        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell) => Underlying.GetCellCenter(toUnderlying(cell));
        public override Vector3 GetCellCorner(Cell cell, CellCorner corner) => Underlying.GetCellCorner(toUnderlying(cell), corner);

        public override TRS GetTRS(Cell cell) => Underlying.GetTRS(toUnderlying(cell));
        #endregion

        #region Shape
        public override Deformation GetDeformation(Cell cell) => Underlying.GetDeformation(toUnderlying(cell));

        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform) => Underlying.GetPolygon(toUnderlying(cell), out vertices, out transform);

        public override IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell) => Underlying.GetTriangleMesh(toUnderlying(cell));

        public override void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform) => Underlying.GetMeshData(toUnderlying(cell), out meshData, out transform);
        #endregion

        #region Query
        public override bool FindCell(Vector3 position, out Cell cell)
        {
            if(Underlying.FindCell(position, out var uCell))
            {
                cell = fromUnderlying(uCell);
                return true;
            }
            else
            {
                cell = default;
                return false;
            }
        }

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            if(Underlying.FindCell(matrix, out var uCell, out rotation))
            {
                cell = fromUnderlying(uCell);
                return true;
            }
            else
            {
                cell = default;
                return false;
            }
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max) => Underlying.GetCellsIntersectsApprox(min, max).Select(fromUnderlying);

        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            foreach(var info in Underlying.Raycast(origin, direction, maxDistance))
            {
                var info2 = info;
                info2.cell = fromUnderlying(info.cell);
                yield return info2;
            }
        }
        #endregion

        #region Symmetry
        public override GridSymmetry FindGridSymmetry(ISet<Cell> src, ISet<Cell> dest, Cell srcCell, CellRotation cellRotation) => FromUnderlying(Underlying.FindGridSymmetry(ToUnderlying(src), ToUnderlying(dest), toUnderlying(srcCell), cellRotation));

        public override bool TryApplySymmetry(GridSymmetry s, IBound srcBound, out IBound destBound) => Underlying.TryApplySymmetry(ToUnderlying(s), srcBound, out destBound);
        public override bool TryApplySymmetry(GridSymmetry s, Cell src, out Cell dest, out CellRotation r)
        {
            if(Underlying.TryApplySymmetry(ToUnderlying(s), toUnderlying(src), out dest, out r))
            {
                dest = fromUnderlying(dest);
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
