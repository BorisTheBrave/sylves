using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
