using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class Vector3Test
    {
        [Test]
        public void TestCross()
        {
            Assert.AreEqual(new Vector3(0, 0, 1), Vector3.Cross(Vector3.right, Vector3.up));
        }

        [Test]
        public void TestSignedAgnle()
        {
            Assert.AreEqual(90.0f, Vector3.SignedAngle(Vector3.right, Vector3.up, Vector3.forward));
            Assert.AreEqual(-90.0f, Vector3.SignedAngle(Vector3.up, Vector3.right, Vector3.forward));
            Assert.AreEqual(-90.0f, Vector3.SignedAngle(Vector3.right, Vector3.up, Vector3.back));
        }

        [Test]
        public void TestSlerp()
        {
            var expected = new Vector3(1.1547004f, 0.6666666f, 0);
            var actual = Vector3.Slerp(Vector3.right, 2 * Vector3.up, 1f / 3);
            Assert.AreEqual(expected, actual);

        }
    }
}
