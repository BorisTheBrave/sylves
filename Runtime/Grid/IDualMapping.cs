using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Sylves
{
    public interface IDualMapping
    {
        /// <summary>
        /// The grid this mapping was constructed from.
        /// </summary>
        IGrid BaseGrid { get; }

        /// <summary>
        /// The dual grid to map to.
        /// </summary>
        IGrid DualGrid { get; }

        /// <summary>
        /// Finds the corresponding dual cell to a corner of a base cell, and the corner of the dual cell that returns back.
        /// </summary>
        (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell baseCell, CellCorner corner);


        /// <summary>
        /// Finds the corresponding base cell to a corner of a dual cell, and the corner of the base cell that returns back.
        /// </summary>
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
        /// <summary>
        /// Finds the corresponding dual cell to a corner of a base cell.
        /// </summary>
        public static Cell? ToDualCell(this IDualMapping dm, Cell baseCell, CellCorner corner) => dm.ToDualPair(baseCell, corner)?.dualCell;

        /// <summary>
        /// Finds the corresponding base cell to a corner of a dual cell.
        /// </summary>
        public static Cell? ToBaseCell(this IDualMapping dm, Cell dualCell, CellCorner corner) => dm.ToBasePair(dualCell, corner)?.baseCell;

        /// <summary>
        /// Finds all dual cells that correspond to some corner of the base cell, and returns the corners and pairs.
        /// </summary>
        public static IEnumerable<(CellCorner corner, Cell dualCell, CellCorner inverseCorner)> DualNeighbours(this IDualMapping dm, Cell baseCell)
        {
            // TODO: Perhaps have this overridable as many grids will have swifter methods
            var cellType = dm.BaseGrid.GetCellType(baseCell);
            foreach(var corner in cellType.GetCellCorners())
            {
                var t = dm.ToDualPair(baseCell, corner);
                if(t != null)
                {
                    yield return (corner, t.Value.dualCell, t.Value.inverseCorner);
                }
            }
        }

        /// <summary>
        /// Finds all base cells that correspond to some corner of the dual cell, and returns the corners and pairs.
        /// </summary>
        // TODO: Be less lazy
        public static IEnumerable<(CellCorner corner, Cell baseCell, CellCorner inverseCorner)> BaseNeighbours(this IDualMapping dm, Cell dualCell) => dm.Reversed().DualNeighbours(dualCell);


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
