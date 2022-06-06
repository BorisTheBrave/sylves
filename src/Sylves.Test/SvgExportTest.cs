using NUnit.Framework;
using System.IO;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class SvgExportTest
    {
        public void Export(IGrid g, string filename)
        {
            using (var file = File.Open(filename, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                SvgExport.WriteGrid(g, writer);
            }
        }

        private IGrid TriHexGrid()
        {
            var meshData = new MeshData();
            meshData.vertices = new Vector3[]
            {
                new Vector3(0.5f, 0, 0),
                new Vector3(0.25f, 0.5f, 0),
                new Vector3(-0.25f, 0.5f, 0),
                new Vector3(-0.5f, 0, 0),
                new Vector3(-0.25f, -0.5f, 0),
                new Vector3(0.25f, -0.5f, 0),
                new Vector3(0.75f, 0.5f, 0),
                new Vector3(0.75f, -0.5f, 0),
            };
            meshData.indices = new[]{new []
            {
                0, 1, 2, 3, 4, ~5,
                6, 1, ~0,
                7, 0, ~5,
            } };
            meshData.subMeshCount = 1;
            meshData.topologies = new[] { MeshTopology.NGon };
            return new PeriodicPlanarMeshGrid(meshData, new Vector2(0.75f, 0), new Vector2(0.5f, 1.0f))
                .BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(6, 6)));
        }

        [Test]
        public void ExportGrids()
        {
            Export(
                new SquareGrid(1).BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(10, 10))),
                "square.svg");
            Export(
                new HexGrid(1, HexOrientation.PointyTopped).BoundBy(HexBound.Hexagon(10)),
                "hex_pt.svg");
            Export(
                new HexGrid(1, HexOrientation.FlatTopped).BoundBy(HexBound.Hexagon(1)),
                "hex_ft.svg");
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatSides).BoundBy(TriangleBound.Hexagon(3)),
                "tri_fs.svg");
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatTopped).BoundBy(TriangleBound.Hexagon(3)),
                "tri_ft.svg");
            Export(
                TriHexGrid(),
                "trihex.svg");
        }
    }
}
