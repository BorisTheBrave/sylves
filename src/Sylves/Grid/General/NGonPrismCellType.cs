﻿using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Cell type for a regular polygon with n sides extended in the z-axis to a prism.
    /// </summary>
    public class NGonPrismCellType : ICellType
    {
        private static IDictionary<int, ICellType> instances = new Dictionary<int, ICellType>
        {
        };

        private int n;

        private CellDir[] dirs;
        private CellRotation[] rotations;
        private CellRotation[] rotationsAndReflections;

        internal NGonPrismCellType(int n)
        {
            this.n = n;
            dirs = Enumerable.Range(0, n).Select(x => (CellDir)x).Concat(new[] { (CellDir)n, (CellDir)(n +1) }).ToArray();
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
            return NGonCellType.GetMatrix(cellRotation, n);
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
                return (CellRotation)(n - (int)a);
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
                connection = new Connection { Mirror = (int)rotation < 0 };
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
}