using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// As this supports triangles pointing both ways, there are 6 dirs, and 6 corners.
    /// So this class is really quite similar to HexCellType
    /// </summary>
    public class TriangleCellType : ICellType
    {
        private static TriangleCellType ftInstance = new TriangleCellType(TriangleOrientation.FlatTopped);
        private static TriangleCellType fsInstance = new TriangleCellType(TriangleOrientation.FlatSides);

        private readonly TriangleOrientation orientation;
        private readonly CellDir[] dirs;
        private readonly CellCorner[] corners;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private TriangleCellType(TriangleOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).ToArray();
            corners = Enumerable.Range(0, 6).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }


        public static TriangleCellType Get(TriangleOrientation orientation) => orientation == TriangleOrientation.FlatTopped ? ftInstance : fsInstance;

        public TriangleOrientation Orientation => orientation;

        public CellRotation ReflectY => orientation == TriangleOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == TriangleOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;

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
                return (CellRotation)(6 - (int)a);
            }
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            var ia = (int)a;
            var ib = (int)b;
            if (ia >= 0)
            {
                if (ib >= 0)
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

        public CellDir Rotate(CellDir dir, CellRotation rotation) => (CellDir)((HexRotation)rotation * (FSTriangleDir)dir);

        public CellCorner Rotate(CellCorner corner, CellRotation rotation) => (CellCorner)((HexRotation)rotation * (FSTriangleCorner)corner);

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            connection = new Connection { Mirror = (int)rotation < 0 };
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
            return ((HexRotation)cellRotation).ToMatrix(orientation == TriangleOrientation.FlatTopped ? HexOrientation.FlatTopped : HexOrientation.PointyTopped);
        }

        // TODO
        public Vector3 GetCornerPosition(CellCorner corner) => throw new NotImplementedException();
    }
}
