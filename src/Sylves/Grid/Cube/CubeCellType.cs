using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public class CubeCellType : ICellType
    {

        private static readonly CubeCellType instance = new CubeCellType();

        private static readonly CellDir[] allCellDirs = new[]
        {
            (CellDir) CubeDir.Left,
            (CellDir) CubeDir.Right,
            (CellDir) CubeDir.Up,
            (CellDir) CubeDir.Down,
            (CellDir) CubeDir.Forward,
            (CellDir) CubeDir.Back,
        };

        private static readonly CellRotation[] allRotations = CubeRotation.GetRotations(false).Select(x => (CellRotation)x).ToArray();
        private static readonly CellRotation[] allRotationsAndReflections = CubeRotation.GetRotations(true).Select(x => (CellRotation)x).ToArray();

        public static CubeCellType Instance => instance;

        private CubeCellType() { }

        public IEnumerable<CellDir> GetCellDirs() => allCellDirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((CubeDir)dir).Inverted();

        // Rotations

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? allRotationsAndReflections : allRotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => (a * (CubeRotation)b);

        public CellRotation Invert(CellRotation a) => ((CubeRotation)a).Invert();

        public CellRotation GetIdentity() => CubeRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            var cubeRotation = (CubeRotation)rotation;
            var cubeDir = (CubeDir)dir;
            return (CellDir)(cubeRotation * cubeDir);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            resultDir = Rotate(dir, rotation);
            connection = new Connection { Mirror = ((CubeRotation)rotation).IsReflection };
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((CubeRotation)cellRotation).ToMatrix();
        }
    }
}
