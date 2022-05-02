using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    public class SwapYZModifier : TransformModifier
    {
        private static readonly Matrix4x4 SwapYZ = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        private readonly ICellType cellType;
        private readonly ICellType[] cellTypes;

        public SwapYZModifier(IGrid underlying) : base(underlying, SwapYZ)
        {
            if(underlying.IsSingleCellType)
            {
                cellType = SwapYZCellModifier.Get(underlying.GetCellTypes().Single());
                cellTypes = new[] { cellType };

            }
            else
            {
                cellType = null;
                cellTypes = underlying.GetCellTypes().Select(SwapYZCellModifier.Get).ToArray();
            }
        }

        public override TRS GetTRS(Cell cell) => new TRS(this.GetCellCenter(cell));

        public override IEnumerable<ICellType> GetCellTypes()
        {
            return cellTypes;
        }

        public override ICellType GetCellType(Cell cell)
        {
            return cellType ?? SwapYZCellModifier.Get(Underlying.GetCellType(cell));
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new SwapYZModifier(underlying);
        }

        public override bool FindCell(Matrix4x4 matrix, out Cell cell, out CellRotation rotation)
        {
            return Underlying.FindCell(SwapYZ * matrix * SwapYZ, out cell, out rotation);
        }

        public override bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Need more permanent solution for this.
            // The default behaviour doesn't work as it uses one underlying, one not, but they have different cell types.
            if(aGrid is SwapYZModifier other)
            {
                return Underlying.ParallelTransport(other.Underlying, aSrcCell, aDestCell, srcCell, startRotation, out destCell, out destRotation);
            }
            else
            {
                return base.ParallelTransport(aGrid, aSrcCell, aDestCell, srcCell, startRotation, out destCell, out destRotation);
            }
        }
    }
}
