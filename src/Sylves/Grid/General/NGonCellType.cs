using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Cell type for a regular polygon with n sides.
    /// The CellDirs are simply the numbers 0 to n-1 with dir 0 being to the right.
    /// The CellRotations are the numbers 0 to n-1 for a CCW rotation of that many sides, 
    /// plus numbers ~0 to ~n-1 for the reflections, where rotation ~0 has dir 0 as a fix point.
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
        private CellRotation[] rotations;
        private CellRotation[] rotationsAndReflections;

        internal NGonCellType(int n)
        {
            this.n = n;
            dirs = Enumerable.Range(0, n).Select(x => (CellDir)x).ToArray();
            rotations = Enumerable.Range(0, n).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, n).Select(x => (CellRotation)~x)).ToArray();
        }

        public static ICellType Get(int n)
        {
            if (instances.TryGetValue(n, out var cellType))
                return cellType;
            return instances[n] = new NGonCellType(n);
        }


        public int N => n;

        public CellRotation ReflectY => (CellRotation)~0;
        public CellRotation ReflectX => (n & 1) == 0 ? (CellRotation)~(n/2) : throw new Exception("Cannot reflex x on odd-sided polygon");

        public CellRotation RotateCCW => (CellRotation)(1);

        public CellRotation RotateCW => (CellRotation)(n - 1);

        public IEnumerable<CellDir> GetCellDirs()
        {
            return dirs;
        }

        public CellRotation GetIdentity()
        {
            return (CellRotation)0;
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            var i = (int)cellRotation;
            var rot = i < 0 ? ~i : i;
            var isReflection = i < 0;
            var m = isReflection ? Matrix4x4.Scale(new Vector3(1, -1, 1)) : Matrix4x4.identity;
            m = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 360.0f / n * rot)) * m;
            return m;
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
                return (CellRotation)(n - (int)a);
            }
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
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

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            if((int)rotation >= 0)
            {
                return (CellDir)(((int)dir + (int)rotation) % n);
            }
            else
            {
                return (CellDir)((n - (int)dir + ~(int)rotation) % n);
            }
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
    }
}
