using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class MeshDataTest
    {
        // These have been validated against unity
        [Test]
        public void TestRecalculateNormals()
        {
            var plane = new MeshData
            {
                indices = new[] { new[] { 0, 1, 2, 3, } },
                vertices = new[]
                {
                    new Vector3(0.5f, -0.5f, 0.0f),
                    new Vector3(0.5f, 0.5f, 0.0f),
                    new Vector3(-0.5f, 0.5f, 0.0f),
                    new Vector3(-0.5f, -0.5f, 0.0f),
                },
                subMeshCount = 1,
                topologies = new[] { MeshTopology.Quads }
            };
            plane.RecalculateNormals();
            var normal = plane.normals[0];
            Assert.AreEqual(Vector3.forward, normal);

            plane = new MeshData
            {
                indices = new[] { new[] { 0, 1, 2, 3, } },
                vertices = new[]
                {
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                },
                subMeshCount = 1,
                topologies = new[] { MeshTopology.Quads }
            };
            plane.RecalculateNormals();
            normal = plane.normals[0];
            Assert.AreEqual(Vector3.down, normal);
        }
    }
}
