using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    public class TriangleInterpolationTest
    {
        public void Test()
        {
            var f = TriangleInterpolation.Interpolate(new Vector2(5, 0), new Vector2(10, 0), new Vector2(0, 5));
        }
    }
}
