using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Handles cell information about squares.
    /// This is a a customized version of NGonCellType and behaves virtually identically.
    /// 
    /// The canonical shape (for use with deformations) is a unit square in the XY centered at the origin, with normal pointing Z-forward.
    /// </summary>
    public class SquareCellType : ICellType
    {
        private static readonly SquareCellType instance = new SquareCellType();

        private static readonly CellDir[] allCellDirs = new[]
        {
            (CellDir) SquareDir.Right,
            (CellDir) SquareDir.Up,
            (CellDir) SquareDir.Left,
            (CellDir) SquareDir.Down,
        };

        private static readonly CellCorner[] allCellCorners = new[]
        {
            (CellCorner) SquareCorner.DownRight,
            (CellCorner) SquareCorner.UpRight,
            (CellCorner) SquareCorner.UpLeft,
            (CellCorner) SquareCorner.DownLeft,
        };

        private static readonly CellRotation[] allRotations = new[]
        {
            (CellRotation) 0,
            (CellRotation) 1,
            (CellRotation) 2,
            (CellRotation) 3,
        };

        private static readonly CellRotation[] allRotationsAndReflections = new[]
        {
            (CellRotation) 0,
            (CellRotation) 1,
            (CellRotation) 2,
            (CellRotation) 3,
            (CellRotation) ~0,
            (CellRotation) ~1,
            (CellRotation) ~2,
            (CellRotation) ~3,
        };

        public static SquareCellType Instance => instance;

        private SquareCellType(){}

        public IEnumerable<CellDir> GetCellDirs() => allCellDirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((SquareDir)dir).Inverted();

        public IEnumerable<CellCorner> GetCellCorners() => allCellCorners;

        // Rotations

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? allRotationsAndReflections : allRotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => (a * (SquareRotation)b);

        public CellRotation Invert(CellRotation a) => ((SquareRotation)a).Invert();

        public CellRotation GetIdentity() => SquareRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            var squareRotation = (SquareRotation)rotation;
            var squareDir = (SquareDir)dir;
            return (CellDir)(squareRotation * squareDir);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var squareRotation = (SquareRotation)rotation;
            var squareCorner= (SquareCorner)corner;
            return (CellCorner)(squareRotation * squareCorner);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            resultDir = Rotate(dir, rotation);
            connection = new Connection { Mirror = ((SquareRotation)rotation).IsReflection };
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if(connection.Mirror)
            {
                var delta = ((int)toDir + (int)fromDir) % 4 + 4;
                rotation = (CellRotation)~(delta % 4);
            }
            else
            {
                var delta = ((int)toDir - (int)fromDir) % 4 + 4;
                rotation = (CellRotation)(delta % 4);
            }
            return true;
        }


        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((SquareRotation)cellRotation).ToMatrix();
        }

        public Vector3 GetCornerPosition(CellCorner corner) => ((SquareCorner)corner).GetPosition();

        public CellRotation RotateCW => SquareRotation.RotateCW;
        public CellRotation RotateCCW => SquareRotation.RotateCCW;


        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, 4);
        public string Format(CellDir dir) => ((SquareDir)dir).ToString();
        public string Format(CellCorner corner) => ((SquareCorner)corner).ToString();
    }
}
