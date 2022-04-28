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
            var f = QuadInterpolation.Interpolate(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9), new Vector3(10, 11, 12));
            AssertAreEqual(new Vector3(1, 2, 3), f(new Vector3(-0.5f, 0, -0.5f)), 1e-6);
            AssertAreEqual(new Vector3(4, 5, 6), f(new Vector3(-0.5f, 0, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(7, 8, 9), f(new Vector3(0.5f, 0, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(10, 11, 12), f(new Vector3(0.5f, 0, -0.5f)), 1e-6);

            AssertAreEqual(new Vector3(5.5f, 6.5f, 7.5f), f(new Vector3(0, 0, 0)), 1e-6);
        }


        [Test]
        public void TestQuadInterpolation2()
        {
            var f = QuadInterpolation.Interpolate(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9), new Vector3(10, 11, 12), new Vector3(13, 14, 15), new Vector3(16, 17, 18), new Vector3(19, 20, 21), new Vector3(22, 23, 24));
            AssertAreEqual(new Vector3(1, 2, 3), f(new Vector3(-0.5f, -0.5f, -0.5f)), 1e-6);
            AssertAreEqual(new Vector3(4, 5, 6), f(new Vector3(-0.5f, -0.5f, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(7, 8, 9), f(new Vector3(0.5f, -0.5f, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(10, 11, 12), f(new Vector3(0.5f, -0.5f, -0.5f)), 1e-6);
            AssertAreEqual(new Vector3(13, 14, 15), f(new Vector3(-0.5f, 0.5f, -0.5f)), 1e-6);
            AssertAreEqual(new Vector3(16, 17, 18), f(new Vector3(-0.5f, 0.5f, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(19, 20, 21), f(new Vector3(0.5f, 0.5f, 0.5f)), 1e-6);
            AssertAreEqual(new Vector3(22, 23, 24), f(new Vector3(0.5f, 0.5f, -0.5f)), 1e-6);
        }
    }
}
