using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    internal class DefaultDiagonalGrid : BaseModifier
    {
        private readonly int m;
        private readonly IDualMapping dualMapping;

        public DefaultDiagonalGrid(IGrid underlying, int m = 8) : base(underlying)
        {
            if (!underlying.Is2d)
                throw new Grid3dException();
            this.m = m;
            dualMapping = underlying.GetDual();
        }

        private ICellType GetDiagonalCellType(ICellType cellType)
        {
            var n = NGonCellType.Extract(cellType).Value;
            return new NGonDiagonalsCellType(n, n * m);
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new DefaultDiagonalGrid(underlying, m);
        }

        public override IEnumerable<CellDir> GetCellDirs(Cell cell)
        {
            // TODO: We can do better by checking the dual sizes.
            // This is important to allow efficient iteration over dirs when m is sized unreasonably large.
            return base.GetCellDirs(cell);
        }

        public override ICellType GetCellType(Cell cell) => GetDiagonalCellType(Underlying.GetCellType(cell));

        public override IEnumerable<ICellType> GetCellTypes() => Underlying.GetCellTypes().Select(GetDiagonalCellType);

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var n = NGonCellType.Extract(Underlying.GetCellType(cell)).Value;
            var i = ((int)dir) % m;
            var j = ((int)dir) / m;

            if(i == 0)
            {
                // Adjacent move
                var b = Underlying.TryMove(cell, (CellDir)j, out dest, out var iDir, out connection);
                inverseDir = (CellDir)(((int)iDir) * m);
                return b;
            }

            var corner1 = (CellCorner)((j + 1) % n);
            var dualPair = dualMapping.ToDualPair(cell, corner1);
            if (dualPair == null)
                goto fail;

            var (dualCell, inverseCorner) = dualPair.Value;
            var dn = NGonCellType.Extract(dualMapping.DualGrid.GetCellType(dualCell)).Value;

            // Stops short of a full rotation so we don't double count
            if(i >= dn - 2) 
                goto fail;

            var corner2 = (CellCorner)(((int)inverseCorner + 1 + i) % dn);
            var basePair = dualMapping.ToBasePair(dualCell, corner2);
            if (basePair == null)
                goto fail;

            var (baseCell, inverseCorner2) = basePair.Value;

            // I think if done right this should never come up.
            // Perhaps we should allow, in case of tiny wrapping grids?
            if (baseCell == cell)
                throw new Exception();

            var ddn = NGonCellType.Extract(Underlying.GetCellType(baseCell)).Value;

            dest = baseCell;
            // Need to pick inverseJ such that corner2 of the step back resolves to inverseCorner1 of this method
            // Expression is (inverseCorner + 1 + i) % dn) in the step back
            // which is equivalent to (corner2 + 1 + inverseI) %dn in this method
            var inverseI = ((int)inverseCorner + dn - 1 - (int)corner2) % dn;
            // Need to pick inverseJ such that corner1 of the step back resolves to inverseCorner2 of this method
            var inverseJ = ((int)inverseCorner2 + ddn - 1) % ddn;
            inverseDir = (CellDir)(inverseI + m * inverseJ);
            connection = default;
            return true;
            
            

            fail:
            {
                dest = default;
                inverseDir = default;
                connection = default;
                return false;
            }
        }
    }
}
