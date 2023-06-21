using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    public class HexRotationTest
    {
        [Test]
        public void TestHexRotation_PT()
        {
            Assert.AreEqual(PTHexDir.UpRight, HexRotation.RotateCCW * PTHexDir.Right);
            Assert.AreEqual(PTHexCorner.UpRight, HexRotation.RotateCCW * PTHexCorner.DownRight);


            Assert.AreEqual(PTHexDir.Left, HexRotation.PTReflectX * PTHexDir.Right);
            Assert.AreEqual(PTHexCorner.DownLeft, HexRotation.PTReflectX * PTHexCorner.DownRight);

            Assert.AreEqual(PTHexDir.Right, HexRotation.PTReflectY * PTHexDir.Right);
            Assert.AreEqual(PTHexCorner.UpRight, HexRotation.PTReflectY * PTHexCorner.DownRight);
        }

        [Test]
        public void TestHexRotation_FT()
        {
            Assert.AreEqual(FTHexDir.UpRight, HexRotation.RotateCCW * FTHexDir.DownRight);
            Assert.AreEqual(FTHexCorner.UpRight, HexRotation.RotateCCW * FTHexCorner.Right);


            Assert.AreEqual(FTHexDir.DownLeft, HexRotation.FTReflectX * FTHexDir.DownRight);
            Assert.AreEqual(FTHexCorner.Left, HexRotation.FTReflectX * FTHexCorner.Right);

            Assert.AreEqual(FTHexDir.UpRight, HexRotation.FTReflectY * FTHexDir.DownRight);
            Assert.AreEqual(FTHexCorner.Right, HexRotation.FTReflectY * FTHexCorner.Right);
        }

        [Test]
        public void TestFromMatrix()
        {
            Assert.AreEqual(HexRotation.Identity, HexRotation.FromMatrix(Matrix4x4.Scale(new Vector3(10, 10, 10)), HexOrientation.FlatTopped));
        }
    }
}
