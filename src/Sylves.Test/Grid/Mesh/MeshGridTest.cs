using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class MeshGridTest
    {
        MeshData meshData;
        public MeshGridTest()
        {
            meshData = TestMeshes.Cube;
        }

        [Ignore("Never going to be supported?")]
        [Test]
        public void TestTryMoveByOffset()
        {
            var g = new MeshGrid(meshData);
            GridTest.TryMoveByOffset(g, new Cell());
        }


        [Test]
        public void TestFindCell()
        {
            var g = new MeshGrid(meshData);
            GridTest.FindCell(g, new Cell());
        }


        [Test]
        public void TestGetTRS()
        {
            // quad 0 points z-
            var g = new MeshGrid(meshData);
            var trs = g.GetTRS(new Cell());
            var v = trs.ToMatrix().MultiplyVector(Vector3.forward);
            TestUtils.AssertAreEqual(Vector3.back, v, 1e-6);
        }
    }
}
