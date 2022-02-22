using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public class HexPrismCellType : ICellType
    {
        private static HexPrismCellType ftInstance = new HexPrismCellType(HexOrientation.FlatTopped);
        private static HexPrismCellType ptInstance = new HexPrismCellType(HexOrientation.PointyTopped);

        private readonly HexOrientation orientation;
        private readonly CellDir[] dirs;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private HexPrismCellType(HexOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).Concat(new[] { (CellDir)PTHexPrismDir.Forward, (CellDir)PTHexPrismDir.Back }).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }

        public static HexPrismCellType Get(HexOrientation orientation) => orientation == HexOrientation.FlatTopped ? ftInstance : ptInstance;

        private bool IsAxial(CellDir dir) => ((PTHexPrismDir)dir).IsAxial();

        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((PTHexPrismDir)dir).Inverted();

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? rotations : rotationsAndReflections;

        public CellRotation Multiply(CellRotation a, CellRotation b) => ((HexRotation)a) * b;

        public CellRotation Invert(CellRotation a) => ((HexRotation)a).Invert();
        
        public CellRotation GetIdentity() => HexRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return IsAxial(dir) ? dir : (CellDir)((HexRotation)rotation * (PTHexDir)dir);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            if (IsAxial(dir))
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = r < 0 ? ~r : r;
                connection = new Connection { Mirror = r < 0, Rotation = rot };
            }
            else
            {
                resultDir = (CellDir)((HexRotation)rotation * (PTHexDir)dir);
                connection = new Connection { Mirror = (int)rotation < 0 };
            }
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation) => ((HexRotation)cellRotation).ToMatrix(orientation);

        public CellRotation ReflectY => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;
    }
}
