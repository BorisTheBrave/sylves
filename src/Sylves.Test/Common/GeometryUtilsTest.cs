using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class GeometryUtilsTest
    {
        [Test]
        public void TestPointInTriangle()
        {
            var p = new Vector3();
            var p2 = new Vector3(10, 0, 0);
            var v1 = new Vector3(1, -1, 0);
            var v2 = new Vector3(0, 1, 0);
            var v3 = new Vector3(-1, -1, 0);
            Assert.IsTrue(GeometryUtils.IsPointInTriangle(p, v1, v2, v3));
            Assert.IsTrue(GeometryUtils.IsPointInTriangle(p, v3, v2, v1));
            Assert.IsFalse(GeometryUtils.IsPointInTriangle(p2, v1, v2, v3));
            Assert.IsFalse(GeometryUtils.IsPointInTriangle(p2, v3, v2, v1));
        }
        [Test]
        public void TestPointInTrianglePlanar()
        {
            var p = new Vector3();
            var p2 = new Vector3(10, 0, 0);
            var v1 = new Vector3(1, -1, 0);
            var v2 = new Vector3(0, 1, 0);
            var v3 = new Vector3(-1, -1, 0);
            Assert.IsTrue(GeometryUtils.IsPointInTrianglePlanar(p, v1, v2, v3));
            Assert.IsTrue(GeometryUtils.IsPointInTrianglePlanar(p, v3, v2, v1));
            Assert.IsFalse(GeometryUtils.IsPointInTrianglePlanar(p2, v1, v2, v3));
            Assert.IsFalse(GeometryUtils.IsPointInTrianglePlanar(p2, v3, v2, v1));
        }
    }
}
