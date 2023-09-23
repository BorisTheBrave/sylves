using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Sylves
{
    /// <summary>
    /// Cell type for a regular polygon with n sides extended in the z-axis to a prism.
    /// 
    /// The canonical shape (for use with deformations) is the shape for the corresponding NGonCellType, extended to +-0.5 along the z-axis.
    /// </summary>
    public class NGonPrismCellType : ICellType
    {
        private static IDictionary<int, ICellType> instances = new Dictionary<int, ICellType>
        {
        };

        private int n;

        private CellDir[] dirs;
        private CellCorner[] corners;
        private CellRotation[] rotations;
        private CellRotation[] rotationsAndReflections;

        internal NGonPrismCellType(int n)
        {
            this.n = n;
            dirs = Enumerable.Range(0, n).Select(x => (CellDir)x).Concat(new[] { (CellDir)n, (CellDir)(n +1) }).ToArray();
            corners = Enumerable.Range(0, 2 * n).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, n).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, n).Select(x => (CellRotation)~x)).ToArray();
        }

        public static ICellType Get(int n)
        {
            if (instances.TryGetValue(n, out var cellType))
                return cellType;
            return instances[n] = new NGonPrismCellType(n);
        }


        public int N => n;

        private bool IsAxial(CellDir dir) => ((int)dir) >= n;

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
            if(IsAxial(dir))
            {
                return (CellDir)(n + n + 1 - (int)dir);
            }
            else if((n & 1) == 0)
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

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            return NGonCellType.Multiply(a, b, n);
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return IsAxial(dir) ? dir : NGonCellType.Rotate(dir, rotation, n);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var ngonCorner = (CellCorner)((int)corner % n);
            ngonCorner = NGonCellType.Rotate(ngonCorner, rotation, n);
            return (CellCorner)(ngonCorner + ((int)corner >= n ? n : 0));
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {

            if (dir == (CellDir)n)
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = r < 0 ? ~r : r;
                connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = n };
            }
            else if (dir == (CellDir)(n+1))
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = (n - (r < 0 ? ~r : r)) % n;
                connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = n };
            }
            else
            {
                var isMirror = (int)rotation < 0;
                connection = new Connection { Mirror = isMirror, Rotation = isMirror ? 2 : 0, Sides = 4 };
                resultDir = Rotate(dir, rotation);
            }
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if (fromDir == (CellDir)n)
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                rotation = (CellRotation)(connection.Mirror ? ~connection.Rotation : connection.Rotation);
                return true;
            }
            else if (fromDir == (CellDir)(n+1))
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                var ir = (n - connection.Rotation) % n;
                rotation = (CellRotation)(connection.Mirror ? ~ir : ir);
                return true;
            }
            else
            {
                if (IsAxial(toDir))
                {
                    rotation = default;
                    return false;
                }
                if (connection.Mirror && connection.Rotation == 2)
                {
                    var delta = ((int)toDir + (int)fromDir) % n + n;
                    rotation = (CellRotation)~(delta % n);
                    return true;
                }
                if (!connection.Mirror && connection.Rotation == 0)
                {
                    var delta = ((int)toDir - (int)fromDir) % n + n;
                    rotation = (CellRotation)(delta % n);
                    return true;
                }
                rotation = default;
                return false;
            }
        }

        // Utility methods. 

        public float CircumcircleToIncircle(float diameter) => diameter * Mathf.Cos(Mathf.PI / n);
        public float IncircleToCircumcircle(float diameter) => diameter / Mathf.Cos(Mathf.PI / n);

        public float CircumcircleDiameterToSideLength(float diameter) => diameter * Mathf.Sin(Mathf.PI / n);
        public float SideLengthToCircumcircleDiameter(float sideLength) => sideLength / Mathf.Sin(Mathf.PI / n);

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return NGonCellType.GetMatrix(cellRotation, n);
        }

        public Vector3 GetCornerPosition(CellCorner corner)
        {
            var flatCorner = (int)corner % n;
            var flatPosition = NGonCellType.GetCornerPosition(corner, n);
            return flatPosition + (((int)corner) >= n ? new Vector3(0, 0, 0.5f) : new Vector3(0, 0, -0.5f));
        }

        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, N);
        public string Format(CellDir dir) => dir == (CellDir)n ? "Forward" : dir == (CellDir)(n + 1) ? "Back" : NGonCellType.Format(dir, N);
        public string Format(CellCorner corner)
        {
            var flatCorner = (int)corner % N;
            return ((int)corner >= 6 ? "Forward" : "Back") + NGonCellType.Format(corner, N);
                
        }
    }
}
