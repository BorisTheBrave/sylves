using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Cell type for a regular polygon with n sides.
    /// The CellDirs are simply the numbers 0 to n-1 with dir 0 being to the right.
    /// The CellRotations are the numbers 0 to n-1 for a CCW rotation of that many sides, 
    /// plus numbers ~0 to ~(n-1) for the reflections, where rotation ~0 has dir 0 as a fix point.
    /// 
    /// The canonical shape (for use with deformations) is a regular polygon with incircle diamater 1.0 in the XY centered at the origin, with normal pointing Z-forward.
    /// </summary>
    public class NGonCellType : ICellType
    {
        private static IDictionary<int, ICellType> instances = new Dictionary<int, ICellType>
        {
            // Despite being a different class, it should be identical
            [4] = SquareCellType.Instance,
        };

        private int n;

        private CellDir[] dirs;
        private CellCorner[] corners;
        private CellRotation[] rotations;
        private CellRotation[] rotationsAndReflections;

        internal NGonCellType(int n)
        {
            this.n = n;
            dirs = Enumerable.Range(0, n).Select(x => (CellDir)x).ToArray();
            corners = Enumerable.Range(0, n).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, n).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, n).Select(x => (CellRotation)~x)).ToArray();
        }

        /// <summary>
        /// Returns the cell type corresponding to a polygon with n sides.
        /// </summary>
        public static ICellType Get(int n)
        {
            if (instances.TryGetValue(n, out var cellType))
                return cellType;
            return instances[n] = new NGonCellType(n);
        }

        /// <summary>
        /// Returns <see cref="N"/> for cellType is an NGonCellType.
        /// Also returns similar values for SquareCellType and HexCellType,
        /// as they are compatible with NGonCellType.
        /// Other celltypes return null.
        /// </summary>
        public static int? Extract(ICellType cellType)
        {
            if (cellType is NGonCellType ngct)
            {
                return ngct.N;
            }
            if (cellType is SquareCellType)
            {
                return 4;
            }
            if (cellType is HexCellType)
            {
                return 6;
            }
            return null;
        }


        public int N => n;

        public CellRotation ReflectY => (CellRotation)~0;
        public CellRotation ReflectX => (n & 1) == 0 ? (CellRotation)~(n/2) : throw new Exception("Cannot reflex x on odd-sided polygon");

        public CellRotation RotateCCW => (CellRotation)(1);

        public CellRotation RotateCW => (CellRotation)(n - 1);

        public IEnumerable<CellCorner> GetCellCorners() => corners;

        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellRotation GetIdentity()
        {
            return (CellRotation)0;
        }

        internal static CellRotation? FromMatrix(Matrix4x4 matrix, int n)
        {
            // Check that this matrix doesn't touch the z-axis
            var forward = matrix.MultiplyVector(Vector3.forward).normalized;
            if (Vector3.Distance(forward, Vector3.forward) > 1e-2f)
            {
                return null;
            }

            var right = matrix.MultiplyVector(Vector3.right);

            var up = matrix.MultiplyVector(Vector3.up);
            var isReflection = Vector3.Cross(right, up).z < 0;
            if (isReflection)
            {
                right.y = -right.y;
            }
            var angle = Mathf.Atan2(right.y, right.x);
            var angleInt = Mathf.RoundToInt(angle / (Mathf.PI * 2 / n));

            var reflectY = (CellRotation)~0;
            var identity = (CellRotation)0;
            return Multiply(isReflection ? reflectY : identity, (CellRotation)((angleInt + n) % n), n);
        }

        public CellRotation? FromMatrix(Matrix4x4 matrix)
        {
            return FromMatrix(matrix, n);
        }

        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return includeReflections ? rotationsAndReflections : rotations;
        }

        public CellDir? Invert(CellDir dir)
        {
            if((n & 1) == 0)
            {
                return (CellDir)((n / 2 + (int)dir) % n);
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

        internal static CellRotation Multiply(CellRotation a, CellRotation b, int n)
        {
            var ia = (int)a;
            var ib = (int)b;
            if(ia >= 0)
            {
                if(ib >= 0)
                {
                    return (CellRotation)((ia + ib) % n);
                }
                else
                {
                    return (CellRotation)~((n + ia + ~ib) % n);
                }
            }
            else
            {
                if (ib >= 0)
                {
                    return (CellRotation)~((n + ~ia - ib) % n);
                }
                else
                {
                    return (CellRotation)((n + ~ia - ~ib) % n);
                }
            }
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            return Multiply(a, b, n);
        }

        internal static CellDir Rotate(CellDir dir, CellRotation rotation, int n)
        {
            if ((int)rotation >= 0)
            {
                return (CellDir)(((int)dir + (int)rotation) % n);
            }
            else
            {
                return (CellDir)((n - (int)dir + ~(int)rotation) % n);
            }
        }

        internal static CellCorner Rotate(CellCorner corner, CellRotation rotation, int n)
        {
            if ((int)rotation >= 0)
            {
                return (CellCorner)(((int)corner + (int)rotation) % n);
            }
            else
            {
                return (CellCorner)((1 + n - (int)corner + ~(int)rotation) % n);
            }
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return Rotate(dir, rotation, n);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            return Rotate(corner, rotation, n);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            connection = new Connection {  Mirror = (int)rotation < 0 };
            resultDir = Rotate(dir, rotation);
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {

            if (connection.Mirror)
            {
                var delta = ((int)toDir + (int)fromDir) % n + n;
                rotation = (CellRotation)~(delta % n);
            }
            else
            {
                var delta = ((int)toDir - (int)fromDir) % n + n;
                rotation = (CellRotation)(delta % n);
            }
            return true;
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return GetMatrix(cellRotation, n);
        }

        internal static Matrix4x4 GetMatrix(CellRotation cellRotation, int n)
        {
            var i = (int)cellRotation;
            var rot = i < 0 ? ~i : i;
            var isReflection = i < 0;
            var m = isReflection ? Matrix4x4.Scale(new Vector3(1, -1, 1)) : Matrix4x4.identity;
            m = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 360.0f / n * rot)) * m;
            return m;
        }

        public Vector3 GetCornerPosition(CellCorner corner)
        {
            return GetCornerPosition(corner, n);
        }

        internal static Vector3 GetCornerPosition(CellCorner corner, int n)
        {
            var angle = (-0.5f + (int)corner) / n * Mathf.PI * 2;
            var circumradius = InradiusToCircumradius(0.5f, n);
            return new Vector3(Mathf.Cos(angle) * circumradius, Mathf.Sin(angle) * circumradius, 0);
        }

        internal static float SideLengthToInradius(float sideLength, int n) => 0.5f * sideLength * Mathf.Cos(Mathf.PI / n) / Mathf.Sin(Mathf.PI / n);

        internal static float InradiusToSideLength(float inradius, int n) => inradius / SideLengthToInradius(1, n);

        internal static float SideLengthToCircumradius(float sideLength, int n) => 0.5f * sideLength / Mathf.Sin(Mathf.PI / n);
        internal static float CircumradiusToSideLength(float circumradius, int n) => circumradius / SideLengthToCircumradius(1, n);

        internal static float InradiusToCircumradius(float inradius, int n) => inradius / Mathf.Cos(Mathf.PI / n);

        public static string Format(CellRotation rotation, int n) => ((int)rotation).ToString();

        public static string Format(CellDir dir, int n) => ((int)dir).ToString();
        public static string Format(CellCorner corner, int n) => ((int)corner).ToString();

        public string Format(CellRotation rotation) => Format(rotation, N);
        public string Format(CellDir dir) => Format(dir, N);
        public string Format(CellCorner corner) => Format(corner, N);
    }
}
