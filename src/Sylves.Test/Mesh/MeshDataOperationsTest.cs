using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    public class MeshDataOperationsTest
    {
        [Test]
        public void TestMaxRandomPairing()
        {
            //var md = ConwayOperators.Kis(TestMeshes.Cube);
            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(3));
            var md = triangleGrid.ToMeshData();

            var r = new Random(1);

            var result = md.MaxRandomPairing(r.NextDouble);

            var triangles = MeshUtils.GetFaces(result).Count(face => face.Count != 4);
            Assert.LessOrEqual(triangles, 1);
        }
    }
}
