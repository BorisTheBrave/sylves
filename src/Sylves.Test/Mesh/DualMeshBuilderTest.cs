using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class DualMeshBuilderTest
    {
        [Test]
        public void Asdf()
        {
            var mesh = new MeshData
            {
                vertices = new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(-1, -1, 0),
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                },
                indices = new int[][]
                {
                    new int[]
                    {
                        0, 1, 2, 
                        0, 2, 3,
                        0, 3, 4,
                        0, 4, 1,
                    }
                },
                subMeshCount = 1,
                topologies = new[] { MeshTopology.Triangles },
            };

            var dmb = new DualMeshBuilder(mesh);
            var dualMesh = dmb.DualMeshData;
            Assert.AreEqual(new Vector3(-0.6666667f, 0, 0), dualMesh.vertices[0]);
            Assert.AreEqual(new Vector3(0, 0.6666667f, 0), dualMesh.vertices[1]);
            Assert.AreEqual(new Vector3(0.6666667f, 0, 0), dualMesh.vertices[2]);
            Assert.AreEqual(new Vector3(0, -0.6666667f, 0), dualMesh.vertices[3]);
            Assert.AreEqual(new Vector3(0, -1E+10f, 0), dualMesh.vertices[4]);
            Assert.AreEqual(new Vector3(-1E+10f, 0, 0), dualMesh.vertices[5]);
            Assert.AreEqual(new Vector3(-1E+10f, 0, 0), dualMesh.vertices[6]);
            Assert.AreEqual(new Vector3(0, 1E+10f, 0), dualMesh.vertices[7]);
            Assert.AreEqual(new Vector3(0, 1E+10f, 0), dualMesh.vertices[8]);
            Assert.AreEqual(new Vector3(1E+10f, 0, 0), dualMesh.vertices[9]);
            Assert.AreEqual(new Vector3(1E+10f, 0, 0), dualMesh.vertices[10]);
            Assert.AreEqual(new Vector3(0, -1E+10f, 0), dualMesh.vertices[11]);
            CollectionAssert.AreEquivalent(new[]
            {
                0,
                3,
                2,
                ~1,
                4,
                3,
                0,
                ~5,
                6,
                0,
                1,
                ~7,
                8,
                1,
                2,
                ~9,
                10,
                2,
                3,
                ~11,
            }, dualMesh.indices[0]);

        }
    }
}
