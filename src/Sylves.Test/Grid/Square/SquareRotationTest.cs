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
    }
}
