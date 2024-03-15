using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // Experimental
    internal class OffsetDiagonalsModifier : BaseModifier
    {
        private readonly Vector3Int[] offsets;
        private readonly int[] inverses;
        private readonly ICellType cellType;
        private readonly ICellType[] cellTypes;

        public OffsetDiagonalsModifier(IGrid underlying, Vector3Int[] offsets):base(underlying)
        {
            this.offsets = offsets;
            inverses = new int[offsets.Length];
            for(var i=0; i<offsets.Length; i++)
            {
                var inv = offsets.Select((x, j) => x == -offsets[i] ? j : (int?)null).Where(x => x != null).SingleOrDefault();
                if(inv == null)
                    throw new Exception($"No inverse found for offset {offsets[i]}");
                inverses[i] = inv.Value;
            }

            var uCellTypes = underlying.GetCellTypes().ToList();
            if (uCellTypes.Count != 1)
                throw new Exception("OffsetDiagonalsGrid only works on grids with a single cell type");

            cellType = new OffsetDiagonalsCellType(offsets.Length, uCellTypes.Single());

            cellTypes = new[] { cellType }; 
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new OffsetDiagonalsModifier(underlying, offsets);
        }

        public override IEnumerable<ICellType> GetCellTypes()
        {
            return cellTypes;
        }

        public override ICellType GetCellType(Cell cell)
        {
            return cellType;
        }

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            dest = cell + offsets[(int)dir];
            inverseDir = (CellDir)inverses[(int)dir];
            connection = default;
            return Underlying.IsCellInGrid(dest);

        }

        class OffsetDiagonalsCellType : ICellType
        {
            private readonly ICellType underlying;
            int dirCount;

            public OffsetDiagonalsCellType(int dirCount, ICellType underlying)
            {
                this.dirCount = dirCount;
                this.underlying = underlying;
            }

            public CellRotation RotateCW => underlying.RotateCW;

            public CellRotation RotateCCW => underlying.RotateCCW;

            public string Format(CellRotation rotation) => underlying.Format(rotation);

            public string Format(CellDir dir)
            {
                return dir.ToString();
            }

            public string Format(CellCorner corner) => underlying.Format(corner);

            public IEnumerable<CellCorner> GetCellCorners() => underlying.GetCellCorners();

            public IEnumerable<CellDir> GetCellDirs()
            {
                return Enumerable.Range(0, dirCount).Select(x => (CellDir)x);
            }

            public Vector3 GetCornerPosition(CellCorner corner) => underlying.GetCornerPosition(corner);

            public CellRotation GetIdentity() => underlying.GetIdentity();

            public Matrix4x4 GetMatrix(CellRotation cellRotation) => underlying.GetMatrix(cellRotation);

            public IList<CellRotation> GetRotations(bool includeReflections = false) => underlying.GetRotations(includeReflections);

            public CellDir? Invert(CellDir dir)
            {
                throw new NotImplementedException();
            }

            public CellRotation Invert(CellRotation a) => underlying.Invert(a);

            public CellRotation Multiply(CellRotation a, CellRotation b) => underlying.Multiply(a, b);

            public CellDir Rotate(CellDir dir, CellRotation rotation)
            {
                throw new NotImplementedException();
            }

            public CellCorner Rotate(CellCorner corner, CellRotation rotation) => underlying.Rotate(corner, rotation);

            public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
            {
                throw new NotImplementedException();
            }

            public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation cellRotation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
