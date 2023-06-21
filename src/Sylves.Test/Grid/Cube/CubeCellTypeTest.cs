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
        public void TestConnectionSense()
        {
            Matrix4x4 GetMatrix(CubeDir dir) => VectorUtils.ToMatrix(dir.Right(), dir.Up(), dir.Forward());

            foreach(var dir in CubeCellType.Instance.GetCellDirs())
            {
                foreach(var rotation in CubeCellType.Instance.GetRotations(true))
                {
                    CubeCellType.Instance.Rotate(dir, rotation, out var dir2, out var connection);

                    var rotationMatrix = CubeCellType.Instance.GetMatrix(rotation);

                    var connectionMatrix = connection.ToMatrix();

                    var cubeDir1 = (CubeDir)dir;
                    var cubeDir2 = (CubeDir)dir2;

                    var m1 = GetMatrix(cubeDir1);
                    var m2 = GetMatrix(cubeDir2);

                    // Check that we get equivalent results for going via rotationMatrix or connection matrix

                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{(CubeDir)dir} {(CubeRotation)rotation}");
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{(CubeDir)dir} {(CubeRotation)rotation}");
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{(CubeDir)dir} {(CubeRotation)rotation}");
                }
            }
        }

        [Test]
        public void TestUpRight()
        {
            Assert.AreEqual(Vector3.right, Vector3.Cross(Vector3.up, Vector3.forward));
            foreach(CubeDir dir in CubeCellType.Instance.GetCellDirs())
            {
                TestUtils.AssertAreEqual(dir.Right(), Vector3.Cross(dir.Up(), dir.Forward()), 1e-6);
            }
        }
    }
}
