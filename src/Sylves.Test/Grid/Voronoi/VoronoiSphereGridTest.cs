using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class VoronoiSphereGridTest
    {
        [Test]
        public void TestCube()
        {
            var points = new[] {
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
            };

            var h = new VoronoiSphereGrid(points);

            for(var i=0; i<points.Length; i++)
            {
                var polygon = h.GetPolygon(new Cell(i, 0));
                Assert.AreEqual(4, polygon.Length);
                Assert.AreEqual(Mathf.Sqrt(1/3f), Vector3.Dot(polygon[0], points[i]), 1e-6);
                Assert.AreEqual(Mathf.Sqrt(1/3f), Vector3.Dot(polygon[1], points[i]), 1e-6);
                Assert.AreEqual(Mathf.Sqrt(1/3f), Vector3.Dot(polygon[2], points[i]), 1e-6);
                Assert.AreEqual(Mathf.Sqrt(1/3f), Vector3.Dot(polygon[3], points[i]), 1e-6);
            }
        }
    }
}
