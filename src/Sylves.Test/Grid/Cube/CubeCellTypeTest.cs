using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class CubeCellTypeTest
    {
        [Test]
        public void TestUpRightForward()
        {
            foreach(var dir in CubeCellType.Instance.GetCellDirs())
            {
                var cubeDir = (CubeDir)dir;
                Assert.AreEqual(Vector3.Cross(cubeDir.Up(), cubeDir.Forward()), (Vector3)cubeDir.Right(), $"{cubeDir}");
            }
        }

        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(CubeCellType.Instance);
        }

        [Test]
        public void TestRotate()
        {
            var mirrorYConnection = new Connection { Mirror = true, Rotation = 0, Sides = 4 };
            var mirrorXConnection = new Connection { Mirror = true, Rotation = 2, Sides = 4 };

            CubeCellType.Instance.Rotate((CellDir)CubeDir.Right, CubeRotation.ReflectX, out var resultDir, out var connection);
            Assert.AreEqual((CellDir)CubeDir.Left, resultDir);
            Assert.AreEqual(mirrorXConnection, connection);
            CubeCellType.Instance.Rotate((CellDir)CubeDir.Right, CubeRotation.ReflectY, out resultDir, out connection);
            Assert.AreEqual((CellDir)CubeDir.Right, resultDir);
            Assert.AreEqual(mirrorYConnection, connection);
            CubeCellType.Instance.Rotate((CellDir)CubeDir.Right, CubeRotation.ReflectZ, out resultDir, out connection);
            Assert.AreEqual((CellDir)CubeDir.Right, resultDir);
            Assert.AreEqual(mirrorXConnection, connection);
        }

        [Test]
        [Ignore("Which way actually should we define this?")]
        public void TestConnectionSense()
        {
            // Rotates X -> Y
            var r = CubeRotation.RotateXY;
            CubeCellType.Instance.Rotate((CellDir)CubeDir.Up, r, out var dir, out var connection);

            Assert.AreEqual((CellDir)CubeDir.Up, dir);
            Assert.AreEqual(true, connection.Mirror);
            Assert.AreEqual(4, connection.Sides);
            Assert.AreEqual(1, connection.Rotation);

            // Swaps X and Z
            r = (CellRotation)528;
            CubeCellType.Instance.Rotate((CellDir)CubeDir.Up, r, out dir, out connection);

            Assert.AreEqual((CellDir)CubeDir.Up, dir);
            Assert.AreEqual(true, connection.Mirror);
            Assert.AreEqual(4, connection.Sides);
            Assert.AreEqual(1, connection.Rotation);
        }
    }
}
