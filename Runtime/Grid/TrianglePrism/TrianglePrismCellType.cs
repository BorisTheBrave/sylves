using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    public class TrianglePrismCellType : ICellType
    {
        private static TrianglePrismCellType ftInstance = new TrianglePrismCellType(TriangleOrientation.FlatTopped);
        private static TrianglePrismCellType fsInstance = new TrianglePrismCellType(TriangleOrientation.FlatSides);

        private readonly TriangleOrientation orientation;
        private readonly CellCorner[] corners;
        private readonly CellDir[] dirs;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private TrianglePrismCellType(TriangleOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).Concat(new[] { (CellDir)PTHexPrismDir.Forward, (CellDir)PTHexPrismDir.Back }).ToArray();
            corners = Enumerable.Range(0, 12).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }

        public static TrianglePrismCellType Get(TriangleOrientation orientation) => orientation == TriangleOrientation.FlatTopped ? ftInstance : fsInstance;

        private bool IsAxial(CellDir dir) => ((FSTrianglePrismDir)dir).IsAxial();

        public IEnumerable<CellCorner> GetCellCorners() => corners;
        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((FSTrianglePrismDir)dir).Inverted();

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? rotationsAndReflections: rotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => ((HexRotation)a) * b;

        public CellRotation Invert(CellRotation a) => ((HexRotation)a).Invert();
        
        public CellRotation GetIdentity() => HexRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return IsAxial(dir) ? dir : (CellDir)((HexRotation)rotation * (PTHexDir)dir);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var hexCorner = (PTHexCorner)((int)corner % 6);
            hexCorner = (HexRotation)rotation * hexCorner;
            return (CellCorner)(hexCorner + ((int)corner >= 6 ? 6 : 0));
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            if (dir == (CellDir)PTHexPrismDir.Forward)
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = r < 0 ? ~r : r;
                connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = 6 };
            }
            else if(dir == (CellDir)PTHexPrismDir.Back)
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = (6 - (r < 0 ? ~r : r)) % 6;
                connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = 6 };
            }
            else
            {
                resultDir = (CellDir)((HexRotation)rotation * (PTHexDir)dir);
                var isMirror = (int)rotation < 0;
                connection = new Connection { Mirror = isMirror, Rotation = isMirror ? 2 : 0, Sides = 4 };
            }
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if (fromDir == (CellDir)PTHexPrismDir.Forward)
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                rotation = (CellRotation)(connection.Mirror ? ~connection.Rotation : connection.Rotation);
                return true;
            }
            else if (fromDir == (CellDir)PTHexPrismDir.Back)
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                var ir = (6 - connection.Rotation) % 6;
                rotation = (CellRotation)(connection.Mirror ?  ~ir : ir);
                return true;
            }
            else
            {
                if(IsAxial(toDir))
                {
                    rotation = default;
                    return false;
                }
                if (connection.Mirror && connection.Rotation == 2)
                {
                    var delta = ((int)toDir + (int)fromDir) % 6 + 6;
                    rotation = (CellRotation)~(delta % 6);
                    return true;
                }
                if (!connection.Mirror && connection.Rotation == 0)
                {
                    var delta = ((int)toDir - (int)fromDir) % 6 + 6;
                    rotation = (CellRotation)(delta % 6);
                    return true;
                }
                rotation = default;
                return false;
            }
        }

        public CellRotation ReflectY => orientation == TriangleOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == TriangleOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;
        public CellRotation RotateCW => HexRotation.RotateCW;
        public CellRotation RotateCCW => HexRotation.RotateCCW;

        public Matrix4x4 GetMatrix(CellRotation cellRotation) => ((HexRotation)cellRotation).ToMatrix(orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);

        // TODO
        public Vector3 GetCornerPosition(CellCorner corner) => throw new System.NotImplementedException();


        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, 6);
        public string Format(CellDir dir) => orientation == TriangleOrientation.FlatSides ? ((FSTrianglePrismDir)dir).ToString() : ((FTTrianglePrismDir)dir).ToString();
        public string Format(CellCorner corner)
        {
            var flatCorner = (int)corner % 6;
            return ((int)corner >= 6 ? "Forward" : "Back") +
                (orientation == TriangleOrientation.FlatSides ? ((FSTriangleCorner)flatCorner).ToString() : ((FTTriangleCorner)flatCorner).ToString());
        }


    }
}
