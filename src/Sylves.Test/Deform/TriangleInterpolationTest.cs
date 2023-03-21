using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;


namespace Sylves.Test
{
    [TestFixture]
    public class TriangleInterpolationTest
    {
        [Test]
        public void TestTriangleInterpolation()
        {
            var v = new[] { new Vector2(5, 0), new Vector2(10, 0), new Vector2(0, 5) };
            var f = TriangleInterpolation.Interpolate(v[0], v[1], v[2]);
            // Checks that the corners of TestMeshes.PlaneXY (which has vertices in the canconical order)
            // behaves as expected
            AssertAreEqual(v[0], f(TestMeshes.Equilateral.vertices[0]), 1e-6);
            AssertAreEqual(v[1], f(TestMeshes.Equilateral.vertices[1]), 1e-6);
            AssertAreEqual(v[2], f(TestMeshes.Equilateral.vertices[2]), 1e-6);
        }
    }
}
