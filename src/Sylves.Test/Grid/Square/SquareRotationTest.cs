using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
