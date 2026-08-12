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
    }
}
