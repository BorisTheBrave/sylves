using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class TriangleCellCellTypeTest
    {
        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(TriangleCellType.Get(TriangleOrientation.FlatTopped));
        }

        [Test]
        public void TestRotate()
        {
            var ct = TriangleCellType.Get(TriangleOrientation.FlatTopped);

            Assert.AreEqual((CellDir)FTTriangleDir.UpRight, ct.Rotate((CellDir)FTTriangleDir.Up, HexRotation.RotateCW));

            Assert.AreEqual(ct.ReflectX, ct.Multiply(HexRotation.RotateCW * HexRotation.RotateCW * HexRotation.RotateCW, ct.ReflectY));
        }
    }
}
