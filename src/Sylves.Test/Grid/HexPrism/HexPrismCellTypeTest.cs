using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    public class HexPrismCellTypeTest
    {
        [Test]
        [TestCase(HexOrientation.PointyTopped)]
        [TestCase(HexOrientation.FlatTopped)]
        public void TestTryGetRotation(HexOrientation orientation)
        {
            GridTest.TryGetRotation(HexPrismCellType.Get(orientation));
        }

        [Test]
        public void TestConnectionSense()
        {
            var cellType = HexPrismCellType.Get(HexOrientation.PointyTopped);

            Matrix4x4 GetMatrix(PTHexPrismDir dir) => VectorUtils.ToMatrix(Vector3.Cross(dir.Up(), dir.Forward()), dir.Up(), dir.Forward());


            foreach (var dir in cellType.GetCellDirs())
            {
                foreach (var rotation in cellType.GetRotations(true))
                {
                    cellType.Rotate(dir, rotation, out var dir2, out var connection);

                    var rotationMatrix = cellType.GetMatrix(rotation);

                    var connectionMatrix = connection.ToMatrix();

                    var m1 = GetMatrix((PTHexPrismDir)dir);
                    var m2 = GetMatrix((PTHexPrismDir)dir2);

                    // Check that we get equivalent results for going via rotationMatrix or connection matrix

                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{cellType.Format(dir)} {cellType.Format(rotation)}");
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{cellType.Format(dir)} {cellType.Format(rotation)}");
                    TestUtils.AssertAreEqual((m2 * connectionMatrix).MultiplyVector(Vector3.right), (rotationMatrix * m1).MultiplyVector(Vector3.right), 1e-6, $"{cellType.Format(dir)} {cellType.Format(rotation)}");
                }
            }
        }

    }
}
