using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class ConwayOperatorsTest
    {
        [Test]
        public void TestDual()
        {
            var r = ConwayOperators.Dual(ConwayOperators.Kis(TestMeshes.PlaneXY));
        }
    }
}
