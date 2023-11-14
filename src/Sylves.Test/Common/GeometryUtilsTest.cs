using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY
using UnityEngine;
#endif

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
            
            // collinear
            p = new Vector3(0, -1.5f, 0);
            v1 = new Vector3(0, -1f, 0);
            v2 = new Vector3(0, 0f, 0);
            v3 = new Vector3(0, 1f, 0);
            Assert.IsFalse(GeometryUtils.IsPointInTriangle(p, v1, v2, v3));

            // backfacing
            p = new Vector3(0.5f, 0, 0);
            v1 = new Vector3(-0.5f, -0.5f, 0.5f);
            v2 = new Vector3(-0.5f, 0.5f, 0.5f);
            v3 = new Vector3(-0.5f, 0.5f, -0.5f);
            Assert.IsFalse(GeometryUtils.IsPointInTriangle(p, v1, v2, v3));
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

            // collinear
            p = new Vector3(0, -1.5f, 0);
            v1 = new Vector3(0, -1f, 0);
            v2 = new Vector3(0, 0f, 0);
            v3 = new Vector3(0, 1f, 0);
            Assert.IsFalse(GeometryUtils.IsPointInTrianglePlanar(p, v1, v2, v3));
        }
    }
}
