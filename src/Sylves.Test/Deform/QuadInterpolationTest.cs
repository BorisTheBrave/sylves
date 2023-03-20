using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;

namespace Sylves.Test
{
    [TestFixture]
    public class QuadInterpolationTest
    {
        [Test]
        public void TestQuadInterpolation1()
        {
            var v = new[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9), new Vector3(10, 11, 12) };
            var f = QuadInterpolation.Interpolate(v[0], v[1], v[2], v[3]);
            // Checks that the corners of TestMeshes.PlaneXY (which has vertices in the canconical order)
            // behaves as expected
            AssertAreEqual(v[0], f(TestMeshes.PlaneXY.vertices[0]), 1e-6);
            AssertAreEqual(v[1], f(TestMeshes.PlaneXY.vertices[1]), 1e-6);
            AssertAreEqual(v[2], f(TestMeshes.PlaneXY.vertices[2]), 1e-6);
            AssertAreEqual(v[3], f(TestMeshes.PlaneXY.vertices[3]), 1e-6);

            // Center is the average
            AssertAreEqual(new Vector3(5.5f, 6.5f, 7.5f), f(new Vector3(0, 0, 0)), 1e-6);
        }


        [Test]
        public void TestQuadInterpolation2()
        {
            // Checks that the corners of TestMeshes.PlaneXY (which has vertices in the canconical order)
            // behaves as expected
            var v = new[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9), new Vector3(10, 11, 12), new Vector3(13, 14, 15), new Vector3(16, 17, 18), new Vector3(19, 20, 21), new Vector3(22, 23, 24) };
            var f = QuadInterpolation.Interpolate(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]);

            AssertAreEqual(v[0], f(TestMeshes.PlaneXY.vertices[0] - 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[1], f(TestMeshes.PlaneXY.vertices[1] - 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[2], f(TestMeshes.PlaneXY.vertices[2] - 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[3], f(TestMeshes.PlaneXY.vertices[3] - 0.5f * Vector3.forward), 1e-6);

            AssertAreEqual(v[4], f(TestMeshes.PlaneXY.vertices[0] + 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[5], f(TestMeshes.PlaneXY.vertices[1] + 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[6], f(TestMeshes.PlaneXY.vertices[2] + 0.5f * Vector3.forward), 1e-6);
            AssertAreEqual(v[7], f(TestMeshes.PlaneXY.vertices[3] + 0.5f * Vector3.forward), 1e-6);
        }
    }
}
