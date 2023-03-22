using System;
using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Converts a ICellType based in the XY plane to one
    /// in the XZ plane. It does this by rotating Y+ to Z-  (and Z+ to Y+)
    /// </summary>
    public class XZCellTypeModifier : ICellType
    {
        private static readonly Dictionary<ICellType, XZCellTypeModifier> cache = new Dictionary<ICellType, XZCellTypeModifier>();

        private static readonly Matrix4x4 RotateYZ = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 0, 1));
        private static readonly Matrix4x4 RotateZY = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        private readonly ICellType underlying;

        private XZCellTypeModifier(ICellType underlying)
        {
            this.underlying = underlying;
        }

        public ICellType Underlying => underlying;

        public static XZCellTypeModifier Get(ICellType underlying)
        {
            if(cache.TryGetValue(underlying, out var v))
            {
                return v;
            }
            return cache[underlying] = new XZCellTypeModifier(underlying);
        }

        // GetMatrix is the only nontrivial member
        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return RotateZY * underlying.GetMatrix(cellRotation) * RotateYZ;
        }

        public CellRotation RotateCW => underlying.RotateCW;

        public CellRotation RotateCCW => underlying.RotateCCW;

        public IEnumerable<CellCorner> GetCellCorners()
        {
            return underlying.GetCellCorners();
        }

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
