using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Cell type for a regular hexagon with 6 sides. 
    /// Supports both flat and pointy topped orientations.
    /// CellDirs are PTHexDir/FTHexDir, integers 0 to 5.
    /// CellCorners are PTHexCorner/FTHexCorner, integers 0 to 5.
    /// The CellRotations are the numbers 0 to 5 for a CCW rotation of that many sides, 
    /// plus numbers ~0 to ~5 for the reflections, where rotation ~0 has dir 0 as a fix point.
    /// 
    /// The canonical shape (for use with deformations) is a regular hexagon with incircle diamater 1.0 in the XY centered at the origin, with normal pointing Z-forward.
    /// </summary>
    public class HexCellType : ICellType
    {
        private static HexCellType ftInstance = new HexCellType(HexOrientation.FlatTopped);
        private static HexCellType ptInstance = new HexCellType(HexOrientation.PointyTopped);

        private readonly HexOrientation orientation;
        private readonly CellDir[] dirs;
        private readonly CellCorner[] corners;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private HexCellType(HexOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).ToArray();
            corners = Enumerable.Range(0, 6).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }

        public static HexCellType Get(HexOrientation orientation) => orientation == HexOrientation.FlatTopped ? ftInstance : ptInstance;

        public HexOrientation Orientation => orientation;

        public CellRotation ReflectY => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;

        public IEnumerable<CellCorner> GetCellCorners() => corners;

        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellRotation GetIdentity()
        {
            return (CellRotation)0;
        }

        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return includeReflections ? rotationsAndReflections : rotations;
        }

        public CellDir? Invert(CellDir dir)
        {
            return (CellDir)((3 + (int)dir) % 6);
        }

        public CellRotation Invert(CellRotation a)
        {
            if ((int)a < 0)
            {
                return a;
            }
            else
            {
                return (CellRotation)((6 - (int)a) % 6);
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
                    return (CellRotation)((ia + ib) % 6);
                }
                else
                {
                    return (CellRotation)~((6 + ia + ~ib) % 6);
                }
            }
            else
            {
                if (ib >= 0)
                {
                    return (CellRotation)~((6 + ~ia - ib) % 6);
                }
                else
                {
                    return (CellRotation)((6 + ~ia - ~ib) % 6);
                }
            }
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation) => (CellDir)((HexRotation)rotation * (PTHexDir)dir);

        public CellCorner Rotate(CellCorner corner, CellRotation rotation) => (CellCorner)((HexRotation)rotation * (PTHexCorner)corner);

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            connection = new Connection {  Mirror = (int)rotation < 0 };
            resultDir = Rotate(dir, rotation);
        }
        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if (connection.Mirror)
            {
                var delta = ((int)toDir + (int)fromDir) % 6 + 6;
                rotation = (CellRotation)~(delta % 6);
            }
            else
            {
                var delta = ((int)toDir - (int)fromDir) % 6 + 6;
                rotation = (CellRotation)(delta % 6);
            }
            return true;
        }
        public CellRotation RotateCW => HexRotation.RotateCW;
        public CellRotation RotateCCW => HexRotation.RotateCCW;



        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((HexRotation)cellRotation).ToMatrix(orientation);
        }

        public Vector3 GetCornerPosition(CellCorner corner) => orientation == HexOrientation.FlatTopped ? ((FTHexCorner)corner).GetPosition() : ((PTHexCorner)corner).GetPosition();

        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, 6);
        public string Format(CellDir dir) => orientation == HexOrientation.FlatTopped ? ((FTHexDir)dir).ToString() : ((PTHexDir)dir).ToString();
        public string Format(CellCorner corner) => orientation == HexOrientation.FlatTopped ? ((FTHexCorner)corner).ToString() : ((PTHexCorner)corner).ToString();
    }
}
