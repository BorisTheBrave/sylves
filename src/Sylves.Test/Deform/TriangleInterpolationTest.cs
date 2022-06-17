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
            var f = TriangleInterpolation.Interpolate(new Vector2(5, 0), new Vector2(10, 0), new Vector2(0, 5));

            AssertAreEqual(new Vector2(5, 0), f(new Vector3(0, 0, 1)), 1e-6);
            AssertAreEqual(new Vector2(10, 0), f(new Vector3(Mathf.Sqrt(3) / 2, 0, -.5f)), 1e-6);
            AssertAreEqual(new Vector2(0, 5), f(new Vector3(-Mathf.Sqrt(3) / 2, 0, -.5f)), 1e-6);
        }
    }
}
