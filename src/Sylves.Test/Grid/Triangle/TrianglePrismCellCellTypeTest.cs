using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class TrianglePrismCellCellTypeTest
    {
        [Test]
        [TestCase(TriangleOrientation.FlatTopped)]
        [TestCase(TriangleOrientation.FlatSides)]
        public void TestTryGetRotation(TriangleOrientation orientation)
        {
            GridTest.TryGetRotation(TrianglePrismCellType.Get(orientation));
        }

        [Test]
        public void TestRotateFT()
        {
            var ct = TrianglePrismCellType.Get(TriangleOrientation.FlatTopped);

            Assert.AreEqual((CellDir)FTTriangleDir.UpRight, ct.Rotate((CellDir)FTTriangleDir.Up, HexRotation.RotateCW));

            Assert.AreEqual(ct.ReflectX, ct.Multiply(HexRotation.RotateCW * HexRotation.RotateCW * HexRotation.RotateCW, ct.ReflectY));

            ct.Rotate((CellDir)FTTrianglePrismDir.Forward, (CellRotation)(~0), out var resultDir, out var connection);
            Assert.AreEqual((CellDir)FTTrianglePrismDir.Forward, resultDir);
            Assert.AreEqual(true, connection.Mirror);
            Assert.AreEqual(0, connection.Rotation);
        }

        [Test]
        public void TestRotateFS()
        {
            var ct = TrianglePrismCellType.Get(TriangleOrientation.FlatSides);

            Assert.AreEqual((CellDir)FSTriangleDir.DownRight, ct.Rotate((CellDir)FSTriangleDir.Right, HexRotation.RotateCW));

            Assert.AreEqual(ct.ReflectX, ct.Multiply(HexRotation.RotateCW * HexRotation.RotateCW * HexRotation.RotateCW, ct.ReflectY));

            ct.Rotate((CellDir)FSTrianglePrismDir.Forward, (CellRotation)(~0), out var resultDir, out var connection);
            Assert.AreEqual((CellDir)FSTrianglePrismDir.Forward, resultDir);
            Assert.AreEqual(true, connection.Mirror);
            Assert.AreEqual(0, connection.Rotation);
        }
    }
}
