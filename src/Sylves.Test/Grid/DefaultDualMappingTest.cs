using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    internal class DefaultDualMappingTest
    {
        [Test]
        public void TestArc()
        {
            var meshData = new MeshData
            {
                vertices = new [] { 
                    new Vector3(100, 100, 0), new Vector3(100, 101, 0), new Vector3(101,101, 0), new Vector3(101, 100, 0), 
                    new Vector3(101, 99, 0), new Vector3(100, 99, 0),
                    new Vector3(99, 100, 0), new Vector3(99, 101, 0),
                    new Vector3(100, 102, 0), new Vector3(101, 102, 0),
                    new Vector3(102, 100, 0), new Vector3(102, 101, 0),
                },
                indices = new[] { new int[] { 
                    0, 1, 2, ~3, 
                    5, 0, 3, ~4,
                    6, 7, 1, ~0,
                    1, 8, 9, ~2,
                    3, 2, 11, ~10,
                } },
                topologies = new[] { MeshTopology.NGon },
            };
            var grid = new MeshGrid(meshData);
            var dual = new DefaultDualMapping(grid, 1000, CachePolicy.Always);
            Assert.AreEqual(12, dual.DualGrid.GetCells().Count());
            GridTest.DualMapping(dual, new Cell());
        }
    }
}
