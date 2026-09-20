using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class RhombicDodecahedronCellTypeTest
    {
        [Test]
        public void TestCornerPosition()
        {
            var ct = RhombicDodecahedronCellType.Instance;
            TestUtils.AssertAreEqual(new Vector3(-0.5f, -0.5f, -0.5f), ct.GetCornerPosition((CellCorner)RhombicDodecahedronCorner.BackDownLeft), 1e-6);
            TestUtils.AssertAreEqual(new Vector3(1, 0, 0), ct.GetCornerPosition((CellCorner)RhombicDodecahedronCorner.Right), 1e-6);
            TestUtils.AssertAreEqual(new Vector3(0, 0, -1), ct.GetCornerPosition((CellCorner)RhombicDodecahedronCorner.Back), 1e-6);
        }

        [Test]
        public void TestRotateCorner()
        {
            var ct = RhombicDodecahedronCellType.Instance;
            Assert.AreEqual(
                (CellCorner)RhombicDodecahedronCorner.Left,
                ct.Rotate((CellCorner)RhombicDodecahedronCorner.Right, RhombicDodecahedronRotation.ReflectX));
            Assert.AreEqual(
                (CellCorner)RhombicDodecahedronCorner.BackDownRight,
                ct.Rotate((CellCorner)RhombicDodecahedronCorner.BackDownLeft, RhombicDodecahedronRotation.ReflectX));
        }

        [Test]
        public void TestRotateDir()
        {
            var ct = RhombicDodecahedronCellType.Instance;
            foreach (var dir in ct.GetCellDirs())
            {
                foreach (var rotation in ct.GetRotations(true))
                {
                    var rotated = (RhombicDodecahedronDir)ct.Rotate(dir, rotation);
                    var expected = ((RhombicDodecahedronRotation)rotation) * ((RhombicDodecahedronDir)dir).Forward();
                    Assert.AreEqual(expected, rotated.Forward(), $"{(RhombicDodecahedronDir)dir} {rotation}");
                }
            }
        }

        [Test]
        public void TestUpRightForward()
        {
            foreach (var dir in RhombicDodecahedronCellType.Instance.GetCellDirs())
            {
                var rdDir = (RhombicDodecahedronDir)dir;
                TestUtils.AssertAreEqual(Vector3.Cross(rdDir.Up(), rdDir.Forward()), (Vector3)rdDir.Right(), 1e-6, $"{rdDir}");
                Assert.AreEqual(0, Vector3.Dot(rdDir.Up(), rdDir.Forward()), $"{rdDir}");
            }
        }

        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(RhombicDodecahedronCellType.Instance);
        }

        [Test]
        public void TestRotate()
        {
            var ct = RhombicDodecahedronCellType.Instance;
            var mirrorYConnection = new Connection { Mirror = true, Rotation = 0, Sides = 4 };
            var mirrorXConnection = new Connection { Mirror = true, Rotation = 2, Sides = 4 };

            ct.Rotate((CellDir)RhombicDodecahedronDir.RightUp, RhombicDodecahedronRotation.ReflectX, out var resultDir, out var connection);
            Assert.AreEqual((CellDir)RhombicDodecahedronDir.LeftUp, resultDir);
            Assert.AreEqual(mirrorXConnection, connection);

            ct.Rotate((CellDir)RhombicDodecahedronDir.RightUp, RhombicDodecahedronRotation.ReflectY, out resultDir, out connection);
            Assert.AreEqual((CellDir)RhombicDodecahedronDir.RightDown, resultDir);
            Assert.AreEqual(mirrorXConnection, connection);

            ct.Rotate((CellDir)RhombicDodecahedronDir.RightUp, RhombicDodecahedronRotation.ReflectZ, out resultDir, out connection);
            Assert.AreEqual((CellDir)RhombicDodecahedronDir.RightUp, resultDir);
            Assert.AreEqual(mirrorYConnection, connection);

            ct.Rotate((CellDir)RhombicDodecahedronDir.RightUp, RhombicDodecahedronRotation.Identity, out resultDir, out connection);
            Assert.AreEqual((CellDir)RhombicDodecahedronDir.RightUp, resultDir);
            Assert.AreEqual(new Connection { Mirror = false, Rotation = 0, Sides = 4 }, connection);
        }

        [Test]
        public void TestConnectionSense()
        {
            Matrix4x4 GetMatrix(RhombicDodecahedronDir dir) => VectorUtils.ToMatrix(
                ((Vector3)dir.Right()).normalized,
                ((Vector3)dir.Up()).normalized,
                ((Vector3)dir.Forward()).normalized);

            var ct = RhombicDodecahedronCellType.Instance;
            foreach (var dir in ct.GetCellDirs())
            {
                foreach (var rotation in ct.GetRotations(true))
                {
                    ct.Rotate(dir, rotation, out var dir2, out var connection);

                    var rotationMatrix = ct.GetMatrix(rotation);
                    var connectionMatrix = connection.ToMatrix();

                    var m1 = GetMatrix((RhombicDodecahedronDir)dir);
                    var m2 = GetMatrix((RhombicDodecahedronDir)dir2);

                    var msg = $"{(RhombicDodecahedronDir)dir} {(RhombicDodecahedronRotation)rotation}";
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-5, msg);
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.up), (rotationMatrix * m1).MultiplyVector(Vector3.up), 1e-5, msg);
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.forward), (rotationMatrix * m1).MultiplyVector(Vector3.forward), 1e-5, msg);
                }
            }
        }
    }
}
