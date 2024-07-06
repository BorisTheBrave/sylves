using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // Experimental
    // Various utilities for implementing GetDiagonalsGrid()

    /// <summary>
    /// A collection of offsets and inverses, often useful with creating diagonal grids.
    /// </summary>
    internal class OffsetCollection
    {
        private readonly Vector3Int[] offsets;
        private readonly int[] inverses;

        // self inverses
        public OffsetCollection(Vector3Int[] offsets)
        {
            this.offsets = offsets;
            // Compute inverses
            inverses = new int[offsets.Length];
            for (var i = 0; i < offsets.Length; i++)
            {
                var inv = offsets.Select((x, j) => x == -offsets[i] ? j : (int?)null).Where(x => x != null).SingleOrDefault();
                if (inv == null)
                    throw new Exception($"No inverse found for offset {offsets[i]}");
                inverses[i] = inv.Value;
            }
        }

        public bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            dest = cell + offsets[(int)dir];
            inverseDir = (CellDir)inverses[(int)dir];
            connection = default;
            return true;
        }
    }

    // This doesn't have a great deal of implementation,
    // but the abstract methods remind you what needs implementing.
    internal abstract class BaseOffsetDiagonalsModifier : BaseModifier
    {
        public BaseOffsetDiagonalsModifier(IGrid underlying):base(underlying)
        {
        }

        public override IEnumerable<CellDir> GetCellDirs(Cell cell) => GetCellType(cell).GetCellDirs();

        public override abstract IEnumerable<ICellType> GetCellTypes();

        public override abstract ICellType GetCellType(Cell cell);

        public abstract OffsetCollection GetOffsetCollection(Cell cell);

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            return GetOffsetCollection(cell).TryMove(cell, dir, out dest, out inverseDir, out connection) && Underlying.IsCellInGrid(dest);
        }
    }

    /// <summary>
    /// CellType with d directions and n corners and non-reflective rotations.
    /// d should be a multiple of n, and
    /// (CellDir)i in NGonCellType corresponds to (CellDir)(i * d / n) in this cell type.
    /// </summary>
    internal class NGonDiagonalsCellType : ICellType
    {
        private static IDictionary<(int, int), ICellType> instances = new Dictionary<(int, int), ICellType>();

        private int n;
        private int d;

        private CellDir[] dirs;
        private CellCorner[] corners;
        private CellRotation[] rotations;
        private CellRotation[] rotationsAndReflections;

        internal NGonDiagonalsCellType(int n, int d)
        {
            this.n = n;
            this.d = d;
            dirs = Enumerable.Range(0, d).Select(x => (CellDir)x).ToArray();
            corners = Enumerable.Range(0, n).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, n).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, n).Select(x => (CellRotation)~x)).ToArray();
        }

        /// <summary>
        /// Returns the cell type corresponding to a polygon with n sides.
        /// </summary>
        public static ICellType Get(int n, int d)
        {
            if (instances.TryGetValue((n, d), out var cellType))
                return cellType;
            return instances[(n, d)] = new NGonDiagonalsCellType(n, d);
        }

        public int N => n;
        public int D => d;

        public CellRotation ReflectY => (CellRotation)~0;
        public CellRotation ReflectX => (n & 1) == 0 ? (CellRotation)~(n / 2) : throw new Exception("Cannot reflex x on odd-sided polygon");

        public CellRotation RotateCCW => (CellRotation)(1);

        public CellRotation RotateCW => (CellRotation)(n - 1);

        public IEnumerable<CellCorner> GetCellCorners() => corners;

        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellRotation GetIdentity()
        {
            return (CellRotation)0;
        }

        public CellRotation? FromMatrix(Matrix4x4 matrix)
        {
            return NGonCellType.FromMatrix(matrix, n);
        }

        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return includeReflections ? rotationsAndReflections : rotations;
        }

        public CellDir? Invert(CellDir dir)
        {
            if ((d & 1) == 0)
            {
                return (CellDir)((d / 2 + (int)dir) % d);
            }
            else
            {
                // Odd sided polygon's have no inverse for a dir.
                return null;
            }
        }

        public CellRotation Invert(CellRotation a)
        {
            if ((int)a < 0)
            {
                return a;
            }
            else
            {
                return (CellRotation)((n - (int)a) % n);
            }
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            return NGonCellType.Multiply(a, b, n);
        }

        internal static CellDir Rotate(CellDir dir, CellRotation rotation, int n, int d)
        {
            var r = (int)rotation;
            r *= (d / n);
            if (r >= 0)
            {
                return (CellDir)(((int)dir + r) % d);
            }
            else
            {
                return (CellDir)((n - (int)dir + ~r) % d);
            }
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return Rotate(dir, rotation, n, d);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            return NGonCellType.Rotate(corner, rotation, n);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            connection = new Connection { Mirror = (int)rotation < 0 };
            resultDir = Rotate(dir, rotation);
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if (connection.Mirror)
            {
                var delta = ((int)toDir + (int)fromDir) % d + d;
                if (delta % n != 0)
                {
                    rotation = default;
                    return false;
                }
                rotation = (CellRotation)~((delta * n / d) % n);
            }
            else
            {
                var delta = ((int)toDir - (int)fromDir) % d + d;
                rotation = (CellRotation)((delta * n / d) % n);
            }
            return true;
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return NGonCellType.GetMatrix(cellRotation, n);
        }

        public Vector3 GetCornerPosition(CellCorner corner)
        {
            return NGonCellType.GetCornerPosition(corner, n);
        }

        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, N);
        public string Format(CellDir dir) => NGonCellType.Format(dir, N);
        public string Format(CellCorner corner) => NGonCellType.Format(corner, N);
    }

    internal class NoRotationCellType : ICellType
    {

        private static IDictionary<ICellType, ICellType> instances = new Dictionary<ICellType, ICellType>();

        private ICellType underlying;

        internal NoRotationCellType(ICellType underlying)
        {
            this.underlying = underlying;
        }

        /// <summary>
        /// Returns the cell type corresponding to a polygon with n sides.
        /// </summary>
        public static ICellType Get(ICellType cellType)
        {
            if (instances.TryGetValue(cellType, out var cellType2))
                return cellType2;
            return instances[cellType] = new NoRotationCellType(cellType);
        }

        private void AssertNoRotation(CellRotation r)
        {
            if (r != 0)
                throw new ArgumentException("Only identity rotation supported");
        }

        public CellRotation ReflectY => throw new InvalidOperationException($"No rotations supported");
        public CellRotation ReflectX => throw new InvalidOperationException($"No rotations supported");

        public CellRotation RotateCCW => throw new InvalidOperationException($"No rotations supported");

        public CellRotation RotateCW => throw new InvalidOperationException($"No rotations supported");

        public ICellType Underlying => underlying;

        public IEnumerable<CellCorner> GetCellCorners() => Underlying.GetCellCorners();

        public IEnumerable<CellDir> GetCellDirs() => Underlying.GetCellDirs();

        public CellRotation GetIdentity() => (CellRotation)0;

        public CellRotation? FromMatrix(Matrix4x4 matrix) => throw new InvalidOperationException($"No rotations supported");

        public IList<CellRotation> GetRotations(bool includeReflections = false) => new[] { GetIdentity() };

        public CellDir? Invert(CellDir dir) => Underlying.Invert(dir);
        public CellRotation Invert(CellRotation a)
        {
            AssertNoRotation(a);
            return GetIdentity();
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            AssertNoRotation(a);
            AssertNoRotation(b);

            return GetIdentity();
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            AssertNoRotation(rotation);
            return dir;
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            AssertNoRotation(rotation);
            return corner;
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            AssertNoRotation(rotation);
            resultDir = dir;
            connection = new Connection();
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            rotation = GetIdentity();
            return fromDir == toDir && connection.Rotation == 0 && connection.Mirror == false;
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            AssertNoRotation(cellRotation);
            return Matrix4x4.identity;
        }

        public Vector3 GetCornerPosition(CellCorner corner) => underlying.GetCornerPosition(corner);
        public string Format(CellRotation rotation) => underlying.Format(rotation);
        public string Format(CellDir dir) => underlying.Format(dir);
        public string Format(CellCorner corner) => underlying.Format(corner);
    }

    // Unused
    /*
    internal class OffsetDiagonalsCellType : ICellType
    {
        private readonly ICellType underlying;
        int dirCount;
        private readonly int[] inverses;

        public OffsetDiagonalsCellType(ICellType underlying, int dirCount, int[] inverses)
        {
            this.dirCount = dirCount;
            this.inverses = inverses;
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
            return (CellDir)inverses[(int)dir];
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
    */

}
