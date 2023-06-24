using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Handles cell information about cubes.
    /// This is a 3d cell type, and it supports all 48 rotations / reflections of a cube.
    /// 
    /// The canonical shape (for use with deformations) is a unit cube centered at the origin.
    /// </summary>
    public class CubeCellType : ICellType
    {

        private static readonly CubeCellType instance = new CubeCellType();


        private static readonly CellCorner[] allCellCorners = new[]
        {
            (CellCorner) CubeCorner.BackDownLeft,
            (CellCorner) CubeCorner.BackDownRight,
            (CellCorner) CubeCorner.BackUpLeft,
            (CellCorner) CubeCorner.BackUpRight,
            (CellCorner) CubeCorner.ForwardDownLeft,
            (CellCorner) CubeCorner.ForwardDownRight,
            (CellCorner) CubeCorner.ForwardUpLeft,
            (CellCorner) CubeCorner.ForwardUpRight,
        };

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

        public IEnumerable<CellCorner> GetCellCorners() => allCellCorners;

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

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var cubeRotation = (CubeRotation)rotation;
            var cubeCorner = (CubeCorner)corner;
            return (CellCorner)(cubeRotation * cubeCorner);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            resultDir = Rotate(dir, rotation);

            var cubeRotation = ((CubeRotation)rotation);
            var cubeDir = (CubeDir)dir;
            var cubeResultDir = (CubeDir)resultDir;

            var up = cubeRotation * cubeDir.Up();
            var right = cubeRotation * cubeDir.Right();

            var resultUp = cubeResultDir.Up();
            var resultRight = cubeResultDir.Right();
            var resultForward = cubeResultDir.Forward();

            // Convert to 2d rotation, same as SquareRotation.FromMatrix

            var isReflection = Vector3.Dot(Vector3.Cross(right, up), resultForward)  < 0;
            var y = Vector3.Dot(right, resultUp);
            var x = Vector3.Dot(right, resultRight);
            if (isReflection)
            {
                y = -y;
            }
            var angle = Mathf.Atan2(y, x);
            var angleInt = Mathf.RoundToInt(angle / (Mathf.PI / 2));
            if(isReflection)
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

        public CellRotation RotateCW => throw new System.NotSupportedException("CubeCellType doesn't have a generic axis to rotate around");
        public CellRotation RotateCCW => throw new System.NotSupportedException("CubeCellType doesn't have a generic axis to rotate around");

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            var cubeFromDir = (CubeDir)fromDir;
            var cubeToDir = (CubeDir)toDir;
            var m1 = VectorUtils.ToMatrix(cubeFromDir.Right(), cubeFromDir.Up(), cubeFromDir.Forward());
            var m2 = VectorUtils.ToMatrix(cubeToDir.Right(), cubeToDir.Up(), cubeToDir.Forward());

            var m3 = connection.ToMatrix();

            // Transpose is equivalent to inverse in this context.
            var cubeRotation = CubeRotation.FromMatrix(m2 * m3 * m1.transpose);
            if(cubeRotation != null)
            {
                rotation = cubeRotation.Value;
                return true;
            }
            else
            {
                rotation = default;
                return false;
            }
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((CubeRotation)cellRotation).ToMatrix();
        }

        public Vector3 GetCornerPosition(CellCorner corner) => ((CubeCorner)corner).GetPosition();

        public string Format(CellRotation rotation) => ((CubeRotation)rotation).ToString();
        public string Format(CellDir dir) => ((CubeDir)dir).ToString();
        public string Format(CellCorner corner) => ((CubeCorner)corner).ToString();
    }
}
