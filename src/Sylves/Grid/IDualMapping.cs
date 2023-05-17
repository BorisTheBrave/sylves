using System;
using System.Collections.Generic;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public interface IDualMapping
    {
        IGrid BaseGrid { get; }
        IGrid DualGrid { get; }

        (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner);

        (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner);
    }
    internal abstract class BasicDualMapping : IDualMapping
    {
        public BasicDualMapping(IGrid baseGrid, IGrid dualGrid)
        {
            BaseGrid = baseGrid;
            DualGrid = dualGrid;
        }

        public IGrid BaseGrid { get; }
        public IGrid DualGrid { get; }

        public abstract (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell dualCell, CellCorner corner);
        public abstract (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner);

    }

    public static class DualMappingExtensions
    {
        public static Cell? ToDualCell(this IDualMapping dm, Cell cell, CellCorner corner) => dm.ToDualPair(cell, corner)?.dualCell;
        public static Cell? ToBaseCell(this IDualMapping dm, Cell cell, CellCorner corner) => dm.ToBasePair(cell, corner)?.baseCell;

        public static IEnumerable<(CellCorner corner, Cell dualCell, CellCorner inverseCorner)> DualNeighbours(this IDualMapping dm, Cell cell)
        {
            // TODO: Perhaps have this overridable as many grids will have swifter methods
            var cellType = dm.BaseGrid.GetCellType(cell);
            foreach(var corner in cellType.GetCellCorners())
            {
                var t = dm.ToDualPair(cell, corner);
                if(t != null)
                {
                    yield return (corner, t.Value.dualCell, t.Value.inverseCorner);
                }
            }
        }

        // TODO: Be less lazy
        public static IEnumerable<(CellCorner corner, Cell baseCell, CellCorner inverseCorner)> BaseNeighbours(this IDualMapping dm, Cell cell) => dm.Reversed().DualNeighbours(cell);


        public static IDualMapping Reversed(this IDualMapping dualMapping)
        {
            if (dualMapping is ReversedDualMapping rdm)
            {
                return rdm.Underlying;
            }
            else
            {
                return new ReversedDualMapping(dualMapping);
            }
        }

        private class ReversedDualMapping : IDualMapping
        {
            readonly IDualMapping underlying;

            public ReversedDualMapping(IDualMapping underlying)
            {
                this.underlying = underlying;
            }

            public IDualMapping Underlying => underlying;

            public IGrid BaseGrid => underlying.DualGrid;

            public IGrid DualGrid => underlying.BaseGrid;

            public (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner) => underlying.ToDualPair(cell, corner);

            public (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner) => underlying.ToBasePair(cell, corner);
        }

    }
}
