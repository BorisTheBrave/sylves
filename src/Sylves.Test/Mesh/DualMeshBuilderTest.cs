using NUnit.Framework;

#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class DualMeshBuilderTest
    {
        [Test]
        public void TestDual()
        {
            // Primal
            //  ____         
            // |\1 /|               
            // |0\/2|               
            // | /\ |               
            // |/3_\|               
            //                      
            // Dual
            //   |     
            // 1 /\ 2  
            //__/4 \__ 
            //  \  /   
            // 0 \/ 3  
            //   |    


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
            Assert.AreEqual(new Vector3(-1E+10f, 0, 0), dualMesh.vertices[4]);
            Assert.AreEqual(new Vector3(0, -1E+10f, 0), dualMesh.vertices[5]);
            Assert.AreEqual(new Vector3(0, 1E+10f, 0), dualMesh.vertices[6]);
            Assert.AreEqual(new Vector3(-1E+10f, 0, 0), dualMesh.vertices[7]);
            Assert.AreEqual(new Vector3(1E+10f, 0, 0), dualMesh.vertices[8]);
            Assert.AreEqual(new Vector3(0, 1E+10f, 0), dualMesh.vertices[9]);
            Assert.AreEqual(new Vector3(0, -1E+10f, 0), dualMesh.vertices[10]);
            Assert.AreEqual(new Vector3(1E+10f, 0, 0), dualMesh.vertices[11]);

            CollectionAssert.AreEqual(new []
            {
                4,
                0,
                3,
                ~5,
                6,
                1,
                0,
                ~7,
                8,
                2,
                1,
                ~9,
                10,
                3,
                2,
                ~11,
                0,
                1,
                2,
                ~3,
            }, dualMesh.indices[0]);

            CollectionAssert.AreEquivalent(new[] {
                (0, 1, 0, 1),
                (3, 2, 0, 2),
                (1, 1, 1, 1),
                (0, 2, 1, 2),
                (2, 1, 2, 1),
                (1, 2, 2, 2),
                (3, 1, 3, 1),
                (2, 2, 3, 2),
                (0, 0, 4, 0),
                (1, 0, 4, 1),
                (2, 0, 4, 2),
                (3, 0, 4, 3),
            }, dmb.Mapping);


        }
    }
}
