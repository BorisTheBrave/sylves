using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class Vector2Test
    {
        [Test]
        public void TestSignedAngle()
        {
            Assert.AreEqual(90, Vector2.SignedAngle(Vector2.right, Vector2.up));
        }
    }
}
