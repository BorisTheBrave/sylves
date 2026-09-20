using System.Collections.Generic;
using System.Linq;
using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Handles cell information about rhombic dodecahedra.
    /// This is a 3d cell type, and it supports all 48 rotations / reflections of a cube
    /// (the octahedral group, which is also the symmetry group of the rhombic dodecahedron).
    /// </summary>
    public class RhombicDodecahedronCellType : ICellType
    {
        private static readonly RhombicDodecahedronCellType instance = new RhombicDodecahedronCellType();

        private static readonly CellCorner[] allCellCorners = new[]
        {
            (CellCorner) RhombicDodecahedronCorner.BackDownLeft,
            (CellCorner) RhombicDodecahedronCorner.BackDownRight,
            (CellCorner) RhombicDodecahedronCorner.BackUpLeft,
            (CellCorner) RhombicDodecahedronCorner.BackUpRight,
            (CellCorner) RhombicDodecahedronCorner.ForwardDownLeft,
            (CellCorner) RhombicDodecahedronCorner.ForwardDownRight,
            (CellCorner) RhombicDodecahedronCorner.ForwardUpLeft,
            (CellCorner) RhombicDodecahedronCorner.ForwardUpRight,
            (CellCorner) RhombicDodecahedronCorner.Right,
            (CellCorner) RhombicDodecahedronCorner.Left,
            (CellCorner) RhombicDodecahedronCorner.Up,
            (CellCorner) RhombicDodecahedronCorner.Down,
            (CellCorner) RhombicDodecahedronCorner.Forward,
            (CellCorner) RhombicDodecahedronCorner.Back,
        };

        private static readonly CellDir[] allCellDirs = new[]
        {
            (CellDir) RhombicDodecahedronDir.RightUp,
            (CellDir) RhombicDodecahedronDir.LeftDown,
            (CellDir) RhombicDodecahedronDir.RightDown,
            (CellDir) RhombicDodecahedronDir.LeftUp,
            (CellDir) RhombicDodecahedronDir.RightForward,
            (CellDir) RhombicDodecahedronDir.LeftBack,
            (CellDir) RhombicDodecahedronDir.RightBack,
            (CellDir) RhombicDodecahedronDir.LeftForward,
            (CellDir) RhombicDodecahedronDir.UpForward,
            (CellDir) RhombicDodecahedronDir.DownBack,
            (CellDir) RhombicDodecahedronDir.UpBack,
            (CellDir) RhombicDodecahedronDir.DownForward,
        };

        private static readonly CellRotation[] allRotations = RhombicDodecahedronRotation.GetRotations(false).Select(x => (CellRotation)x).ToArray();
        private static readonly CellRotation[] allRotationsAndReflections = RhombicDodecahedronRotation.GetRotations(true).Select(x => (CellRotation)x).ToArray();

        public static RhombicDodecahedronCellType Instance => instance;

        private RhombicDodecahedronCellType() { }

        public IEnumerable<CellCorner> GetCellCorners() => allCellCorners;

        public Int32 N => 12;
        public IEnumerable<CellDir> GetCellDirs() => allCellDirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((RhombicDodecahedronDir)dir).Inverted();

        // Rotations

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? allRotationsAndReflections : allRotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => (a * (RhombicDodecahedronRotation)b);

        public CellRotation Invert(CellRotation a) => ((RhombicDodecahedronRotation)a).Invert();

        public CellRotation GetIdentity() => RhombicDodecahedronRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            var rhombicDodecahedronRotation = (RhombicDodecahedronRotation)rotation;
            var rhombicDodecahedronDir = (RhombicDodecahedronDir)dir;
            return (CellDir)(rhombicDodecahedronRotation * rhombicDodecahedronDir);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var rhombicDodecahedronRotation = (RhombicDodecahedronRotation)rotation;
            var rhombicDodecahedronCorner = (RhombicDodecahedronCorner)corner;
            return (CellCorner)(rhombicDodecahedronRotation * rhombicDodecahedronCorner);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            resultDir = Rotate(dir, rotation);

            var rdRotation = (RhombicDodecahedronRotation)rotation;
            var rdDir = (RhombicDodecahedronDir)dir;
            var rdResultDir = (RhombicDodecahedronDir)resultDir;

            var up = rdRotation * rdDir.Up();
            var right = rdRotation * rdDir.Right();

            var resultUp = rdResultDir.Up();
            var resultRight = rdResultDir.Right();
            var resultForward = rdResultDir.Forward();

            // Convert to 2d rotation, same as SquareRotation.FromMatrix
            var isReflection = Vector3.Dot(Vector3.Cross(right, up), resultForward) < 0;
            var y = Vector3.Dot(right, resultUp);
            var x = Vector3.Dot(right, resultRight);
            if (isReflection)
            {
                y = -y;
            }
            var angle = Mathf.Atan2(y, x);
            var angleInt = MathUtils.RoundToInt32(angle / (Mathf.PI / 2));
            if (isReflection)
            {
                angleInt = (-angleInt + 4) % 4;
            }
            else
            {
                angleInt = (angleInt + 4) % 4;
            }

            connection = new Connection
            {
                Mirror = isReflection,
                Rotation = angleInt,
                Sides = 4,
            };
        }

        public CellRotation RotateCW => throw new System.NotSupportedException("RhombicDodecahedronCellType doesn't have a generic axis to rotate around");
        public CellRotation RotateCCW => throw new System.NotSupportedException("RhombicDodecahedronCellType doesn't have a generic axis to rotate around");

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            var rdFromDir = (RhombicDodecahedronDir)fromDir;
            var rdToDir = (RhombicDodecahedronDir)toDir;
            var m1 = FaceMatrix(rdFromDir);
            var m2 = FaceMatrix(rdToDir);
            var m3 = connection.ToMatrix();

            // Transpose is equivalent to inverse because FaceMatrix is orthonormal.
            var rdRotation = RhombicDodecahedronRotation.FromMatrix(m2 * m3 * m1.transpose);
            if (rdRotation != null)
            {
                rotation = rdRotation.Value;
                return true;
            }
            else
            {
                rotation = default;
                return false;
            }
        }

        private static Matrix4x4 FaceMatrix(RhombicDodecahedronDir dir)
        {
            return VectorUtils.ToMatrix(
                ((Vector3)dir.Right()).normalized,
                ((Vector3)dir.Up()).normalized,
                ((Vector3)dir.Forward()).normalized);
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((RhombicDodecahedronRotation)cellRotation).ToMatrix();
        }

        public bool GetRotationFromMatrix(Matrix4x4 cellTransform, Matrix4x4 matrix, out CellRotation rotation)
        {
            var m = cellTransform.inverse * matrix;
            var rhombicDodecahedronRotation = RhombicDodecahedronRotation.FromMatrix(m);
            if (rhombicDodecahedronRotation != null)
            {
                rotation = rhombicDodecahedronRotation.Value;
                return true;
            }
            rotation = default;
            return false;
        }

        public Vector3 GetCornerPosition(CellCorner corner) => ((RhombicDodecahedronCorner)corner).GetPosition();

        public string Format(CellRotation rotation) => ((RhombicDodecahedronRotation)rotation).ToString();
        public string Format(CellDir dir) => ((RhombicDodecahedronDir)dir).ToString();
        public string Format(CellCorner corner) => ((RhombicDodecahedronCorner)corner).ToString();
    }
}
