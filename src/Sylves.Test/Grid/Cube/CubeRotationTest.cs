using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class CubeRotationTest
    {
        [Test]
        public void TestToFromMatrix()
        {
            foreach(var r in CubeRotation.GetRotations(true))
            {
                var m = r.ToMatrix();
                var r2 = CubeRotation.FromMatrix(m);
                Assert.AreEqual(r, r2);
            }
        }

        [Test]
        public void TestVectorRotation()
        {
            Assert.AreEqual(new Vector3Int(0, 10, 0), CubeRotation.RotateXY * new Vector3Int(10, 0, 0));
            Assert.AreEqual(new Vector3Int(0, 0, 10), CubeRotation.RotateXZ * new Vector3Int(10, 0, 0));
            Assert.AreEqual(new Vector3Int(0, 0, 10), CubeRotation.RotateYZ * new Vector3Int(0, 10, 0));
        }
    }
}
