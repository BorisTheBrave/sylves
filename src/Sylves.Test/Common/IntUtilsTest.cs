using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class IntUtilsTest
    {
        [Test]
        [TestCase((short)0x1234, (short)0x5678)]
        [TestCase((short)0x0000, unchecked((short)0xFFFF))]
        public void TestZip(short a, short b)
        {
            var i = IntUtils.Zip(a, b);
            var (a2, b2) = IntUtils.Unzip(i);
            Assert.AreEqual(a, a2);
            Assert.AreEqual(b, b2);
        }
    }
}
