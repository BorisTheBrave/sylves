using System;
using System.Collections.Generic;
using UnityEngine;


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

        /// <inheritdoc />
        public ICellType Underlying => underlying;

        /// <inheritdoc />
        public static XZCellTypeModifier Get(ICellType underlying)
        {
            if(cache.TryGetValue(underlying, out var v))
            {
                return v;
            }
            return cache[underlying] = new XZCellTypeModifier(underlying);
        }

        /// <inheritdoc />
        public CellRotation RotateCW => underlying.RotateCW;

        /// <inheritdoc />
        public CellRotation RotateCCW => underlying.RotateCCW;

        /// <inheritdoc />
        public IEnumerable<CellCorner> GetCellCorners()
        {
            return underlying.GetCellCorners();
        }

        /// <inheritdoc />
        public IEnumerable<CellDir> GetCellDirs()
        {
            return underlying.GetCellDirs();
        }

        /// <inheritdoc />
        public CellRotation GetIdentity()
        {
            return underlying.GetIdentity();
        }

        /// <inheritdoc />
        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return underlying.GetRotations(includeReflections);
        }

        /// <inheritdoc />
        public CellDir? Invert(CellDir dir)
        {
            return underlying.Invert(dir);
        }

        /// <inheritdoc />
        public CellRotation Invert(CellRotation a)
        {
            return underlying.Invert(a);
        }

        /// <inheritdoc />
        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            return underlying.Multiply(a, b);
        }

        /// <inheritdoc />
        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return underlying.Rotate(dir, rotation);
        }

        /// <inheritdoc />
        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            return underlying.Rotate(corner, rotation);
        }

        /// <inheritdoc />
        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            underlying.Rotate(dir, rotation, out resultDir, out connection);
        }

        /// <inheritdoc />
        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation cellRotation)
        {
            return underlying.TryGetRotation(fromDir, toDir, connection, out cellRotation);
        }



        // Notrivial methods at the bottom
        /// <inheritdoc />
        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return RotateZY * underlying.GetMatrix(cellRotation) * RotateYZ;
        }

        /// <inheritdoc />
        public Vector3 GetCornerPosition(CellCorner corner)
        {
            return RotateZY.MultiplyVector(underlying.GetCornerPosition(corner));
        }

        private string Reformat(string s)
        {
            return s
                .Replace("Up", "Ba!!!ck")
                .Replace("Back", "Do!!!wn")
                .Replace("Down", "Fo!!!rward")
                .Replace("Forward", "U!!!p")
                .Replace("!!!", "");
        }

        public string Format(CellRotation rotation) => Reformat(underlying.Format(rotation));
        public string Format(CellDir dir) => Reformat(underlying.Format(dir));
        public string Format(CellCorner corner) => Reformat(underlying.Format(corner));

    }
}
