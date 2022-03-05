using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class CubeCellTypeTest
    {
        [Test]
        public void TestUpRightForward()
        {
            foreach(var dir in CubeCellType.Instance.GetCellDirs())
            {
                var cubeDir = (CubeDir)dir;
                Assert.AreEqual(Vector3.Cross(cubeDir.Up(), cubeDir.Forward()), (Vector3)cubeDir.Right(), $"{cubeDir}");
            }
        }

        [Test]
        public void TestTryGetRotation()
        {
            GridTest.TryGetRotation(CubeCellType.Instance);
        }
    }
}
