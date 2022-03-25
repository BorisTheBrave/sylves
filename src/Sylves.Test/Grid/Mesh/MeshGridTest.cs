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
            meshData = new MeshData();
            Vector3[] vertices = {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, -0.5f, +0.5f),
                new Vector3 (-0.5f, -0.5f, +0.5f),
            };
            int[] quads = {
                0, 3, 2, 1,
                2, 3, 4, 5,
                1, 2, 5, 6,
                0, 7, 4, 3,
                5, 4, 7, 6,
                0, 1, 6, 7,
            };

            meshData.subMeshCount = 1;
            meshData.vertices = vertices;
            meshData.indices = new[] { quads };
            meshData.topologies = new[] { MeshTopology.Quads };
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
    }
}
