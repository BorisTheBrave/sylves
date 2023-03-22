using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

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
            Assert.AreEqual(new Vector3Int(-10, 0, 0), CubeRotation.RotateXY * new Vector3Int(0, 10, 0));
            Assert.AreEqual(new Vector3Int(0, 0, 10), CubeRotation.RotateXZ * new Vector3Int(10, 0, 0));
            Assert.AreEqual(new Vector3Int(-10, 0, 0), CubeRotation.RotateXZ * new Vector3Int(0, 0, 10));
            Assert.AreEqual(new Vector3Int(0, 0, 10), CubeRotation.RotateYZ * new Vector3Int(0, 10, 0));
            Assert.AreEqual(new Vector3Int(0, -10, 0), CubeRotation.RotateYZ * new Vector3Int(0, 0, 10));
        }

        [Test]
        public void TestRotateCorner()
        {
            var d = new Dictionary<CubeCorner, Vector3Int>();
            d[CubeCorner.BackDownLeft] =     new Vector3Int(-1, -1, -1);
            d[CubeCorner.BackDownRight] =    new Vector3Int( 1, -1, -1);
            d[CubeCorner.BackUpLeft] =       new Vector3Int(-1,  1, -1);
            d[CubeCorner.BackUpRight] =      new Vector3Int( 1,  1, -1);
            d[CubeCorner.ForwardDownLeft] =  new Vector3Int(-1, -1,  1);
            d[CubeCorner.ForwardDownRight] = new Vector3Int( 1, -1,  1);
            d[CubeCorner.ForwardUpLeft] =    new Vector3Int(-1,  1,  1);
            d[CubeCorner.ForwardUpRight] =   new Vector3Int( 1,  1,  1);

            // Checks corner rotation matches the equivalent vector rotation.
            foreach(var rotation in CubeCellType.Instance.GetRotations(true))
            {
                var cubeRotation = (CubeRotation)rotation;
                foreach(var corner in CubeCellType.Instance.GetCellCorners())
                {
                    var cubeCorner = (CubeCorner)corner;
                    var expected = d.Where(kv => kv.Value == cubeRotation * d[cubeCorner]).First().Key;
                    Assert.AreEqual(expected, cubeRotation * cubeCorner, $"Failed at {cubeRotation} {cubeCorner}");
                }
            }
        }
    }
}
