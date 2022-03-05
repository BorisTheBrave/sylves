using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    internal class Matrix4x4Test
    {
        [Test]
        public void TestRotate()
        {
            {
                var q = Quaternion.Euler(0, 0, 90);
                var m = Matrix4x4.Rotate(q);
                Assert.AreEqual(0, m.m00, 1e-6);
                Assert.AreEqual(1, m.m10, 1e-6);
                Assert.AreEqual(-1, m.m01, 1e-6);
                Assert.AreEqual(0, m.m11, 1e-6);
            }
            {
                var q = Quaternion.Euler(0, 90, 0);
                /*
                 * 0.00000	0.00000	1.00000	0.00000
0.00000	1.00000	0.00000	0.00000
-1.00000	0.00000	0.00000	0.00000
0.00000	0.00000	0.00000	1.00000*/

                var m = Matrix4x4.Rotate(q);
                var e = new Matrix4x4(
                    new Vector4(0, 0, 1, 0),
                    new Vector4(0, 1, 0, 0),
                    new Vector4(-1, 0, 0, 0),
                    new Vector4(0, 0, 0, 1)
                    ).transpose;
                AssertAreEqual(e, m, 1e-6);
            }
        }

        [Test]
        [TestCase(0f, 10f, 0f)]
        [TestCase(10f, 0f, 0f)]
        [TestCase(0f, 0f, 10f)]
        [TestCase(10f, 10f, 10f)]
        public void TestRotation(float rx, float ry, float rz)
        {
            var m = Matrix4x4.Rotate(Quaternion.Euler(rx, ry, rz));
            var m2 = Matrix4x4.Rotate(m.rotation);
            AssertAreEqual(m, m2, 1e-6);
        }


        [Test]
        [TestCase(2f, 3f, 4f, 0f, 0f, 0f)]
        [TestCase(2f, 1f, 1f, 0f, 10f, 0f)]
        [TestCase(2f, 1f, 1f, 10f, 0f, 0f)]
        [TestCase(2f, 1f, 1f, 0f, 0f, 10f)]
        [TestCase(3f, 3f, 5f, 10f, 10f, 10f)]
        public void TestLossyScale(float sx, float sy, float sz, float rx, float ry, float rz)
        {
            var m = Matrix4x4.Rotate(Quaternion.Euler(rx, ry, rz)) * Matrix4x4.Scale(new Vector3(sx, sy, sz));
            var scale = m.lossyScale;
            AssertAreEqual(new Vector3(sx, sy, sz), scale, 1e-6);
        }

        [Test]
        public void TestLossyScaleNegative()
        {
            var m = Matrix4x4.Scale(new Vector3(-1, -2, -3));
            Assert.AreEqual(new Vector3(-1, 2, 3), m.lossyScale);
            AssertAreEqual(Matrix4x4.Rotate(Quaternion.Euler(0, 180, 180)), Matrix4x4.Rotate(m.rotation), 1e-6f);
        }
    }
}
