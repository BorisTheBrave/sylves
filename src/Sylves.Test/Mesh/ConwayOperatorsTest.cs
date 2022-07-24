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
            Assert.AreEqual(5, MeshUtils.GetFaces(r, 0).Count());
            var r2 = ConwayOperators.Dual(r);
            Assert.AreEqual(1, MeshUtils.GetFaces(r2, 0).Count());
        }
    }
}
