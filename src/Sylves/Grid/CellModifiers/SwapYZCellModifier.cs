using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    // Perhaps cell types should have no spatial component?
    // But we use them to know the domain from deformations, so needed anyway?
    /// <summary>
    /// Swaps Y and Z axes.
    /// This only affects GetMatrix as all other methods don't refer to space.
    /// </summary>
    public class SwapYZCellModifier : ICellType
    {
        private static readonly Dictionary<ICellType, SwapYZCellModifier> cache = new Dictionary<ICellType, SwapYZCellModifier>();

        private static readonly Matrix4x4 SwapYZ = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        public readonly ICellType underlying;

        private SwapYZCellModifier(ICellType underlying)
        {
            this.underlying = underlying;
        }

        public static SwapYZCellModifier Get(ICellType underlying)
        {
            if(cache.TryGetValue(underlying, out var v))
            {
                return v;
            }
            return cache[underlying] = new SwapYZCellModifier(underlying);
        }

        // GetMatrix is the only nontrivial member
        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return SwapYZ * underlying.GetMatrix(cellRotation) * SwapYZ;
        }

        public CellRotation RotateCW => underlying.RotateCW;

        public CellRotation RotateCCW => underlying.RotateCCW;

        public IEnumerable<CellDir> GetCellDirs()
        {
            return underlying.GetCellDirs();
        }

        public CellRotation GetIdentity()
        {
            return underlying.GetIdentity();
        }

        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return underlying.GetRotations(includeReflections);
        }

        public CellDir? Invert(CellDir dir)
        {
            return underlying.Invert(dir);
        }

        public CellRotation Invert(CellRotation a)
        {
            return underlying.Invert(a);
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            return underlying.Multiply(a, b);
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return underlying.Rotate(dir, rotation);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            underlying.Rotate(dir, rotation, out resultDir, out connection);
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation cellRotation)
        {
            return underlying.TryGetRotation(fromDir, toDir, connection, out cellRotation);
        }
    }
}
