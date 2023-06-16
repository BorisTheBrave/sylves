using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class SquareRotationTest
    {
        [Test]
        public void TestToFromMatrix()
        {
            foreach(var r in SquareRotation.All)
            {
                var m = r.ToMatrix();
                var r2 = SquareRotation.FromMatrix(m);
                Assert.AreEqual(r, r2);
            }

            Assert.AreEqual(SquareRotation.Identity, SquareRotation.FromMatrix(Matrix4x4.Scale(new Vector3(10, 10, 10))));
        }

        [Test]
        public void TestToMatrix()
        {
            var m1 = new Matrix4x4(new Vector4(0, 1, 0, 0), new Vector4(-1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
            var m2 = SquareRotation.RotateCCW.ToMatrix();
            TestUtils.AssertAreEqual(m1, m2, 1e-6);
        }

        // Because I'm constantly getting this confused, serves as a reminder:
        // ~x is a reflection in the y-axis, followed by a rotation of x.
        [Test]
        public void TestCompostion()
        {
            var r = (SquareRotation)(CellRotation)~3;
            var ra = (SquareRotation)(CellRotation)~0;
            var rb = (SquareRotation)(CellRotation)3;

            Assert.AreEqual(r, rb * ra);

            TestUtils.AssertAreEqual(r.ToMatrix(), rb.ToMatrix() * ra.ToMatrix(), 1e-6);
        }

        [Test]
        public void TestRotate()
        {
            Assert.AreEqual(SquareDir.Up, SquareRotation.RotateCCW * SquareDir.Right);
            Assert.AreEqual(SquareCorner.UpRight, SquareRotation.RotateCCW * SquareCorner.DownRight);


            Assert.AreEqual(SquareDir.Right, SquareRotation.ReflectY * SquareDir.Right);
            Assert.AreEqual(SquareCorner.UpRight, SquareRotation.ReflectY * SquareCorner.DownRight);


            Assert.AreEqual(SquareDir.Left, SquareRotation.ReflectX * SquareDir.Right);
            Assert.AreEqual(SquareCorner.DownLeft, SquareRotation.ReflectX * SquareCorner.DownRight);
        }
    }
}
