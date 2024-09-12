using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class EncodingTest
    {
        [Test]
        public void TestZigZag()
        {
            Assert.AreEqual(0, Encoding.ZigZagEncode(0));
            Assert.AreEqual(1, Encoding.ZigZagEncode(-1));
            Assert.AreEqual(2, Encoding.ZigZagEncode(1));
            Assert.AreEqual(-2, Encoding.ZigZagEncode(int.MaxValue));
            Assert.AreEqual(-1, Encoding.ZigZagEncode(int.MinValue));

            Assert.AreEqual(0, Encoding.ZigZagDecode(0));
            Assert.AreEqual(-1, Encoding.ZigZagDecode(1));
            Assert.AreEqual(1, Encoding.ZigZagDecode(2));
            Assert.AreEqual(int.MaxValue, Encoding.ZigZagDecode(-2));
            Assert.AreEqual(int.MinValue, Encoding.ZigZagDecode(-1));
        }
        [Test]
        public void TestZOrder()
        {
            Assert.AreEqual(0, Encoding.ZOrderEncode(0, 0));
            Assert.AreEqual(1, Encoding.ZOrderEncode(1, 0));
            Assert.AreEqual(2, Encoding.ZOrderEncode(0, 1));
            Assert.AreEqual(3, Encoding.ZOrderEncode(1, 1));
            Assert.AreEqual(4, Encoding.ZOrderEncode(2, 0));


            Assert.AreEqual((0, 0), Encoding.ZOrderDecode(0));
            Assert.AreEqual((1, 0), Encoding.ZOrderDecode(1));
            Assert.AreEqual((0, 1), Encoding.ZOrderDecode(2));
            Assert.AreEqual((1, 1), Encoding.ZOrderDecode(3));
            Assert.AreEqual((2, 0), Encoding.ZOrderDecode(4));
        }
    }
}
