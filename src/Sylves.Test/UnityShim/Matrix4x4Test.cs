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
    }
}
