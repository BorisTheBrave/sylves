using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class Vector2Test
    {
        [Test]
        public void TestSignedAngle()
        {
            Assert.AreEqual(90, Vector2.SignedAngle(Vector2.right, Vector2.up));
            Assert.AreEqual(-90, Vector2.SignedAngle(Vector2.up, Vector2.right));
        }
    }
}
