using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves.Test
{
    [TestFixture]
    internal class SvgExportTest
    {
        public class Options
        {
            public int dim = 3;
            public double textScale = 1;
        }

        public static void WriteGrid(IGrid grid, TextWriter tw, Options options)
        {
            var b = new SvgBuilder(tw);
            b.BeginSvg();
            foreach (var cell in grid.GetCells())
            {
                b.DrawCell(grid, cell);
            }
            foreach (var cell in grid.GetCells())
            {
                b.DrawCoordinateLabel(grid, cell, options.dim, options.textScale);
            }
            b.EndSvg();
        }

        public void Export(IGrid g, string filename, Options options = null)
        {
            var fullPath = Path.GetFullPath(filename);
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                WriteGrid(g, writer, options ?? new Options());
                Console.WriteLine($"Wrote file {fullPath}");
            }
        }


        [Test]
        public void ExportGrids()
        {
            Export(
                new SquareGrid(1).BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(10, 10))),
                "square.svg",
                new Options { dim = 2});
            Export(
                new HexGrid(1, HexOrientation.PointyTopped).BoundBy(HexBound.Hexagon(10)),
                "hex_pt.svg",
                new Options { textScale = 0.7 });
            Export(
                new HexGrid(1, HexOrientation.FlatTopped).BoundBy(HexBound.Hexagon(1)),
                "hex_ft.svg",
                new Options { textScale = 0.7 });
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatSides).BoundBy(TriangleBound.Hexagon(3)),
                "tri_fs.svg",
                new Options { textScale = 0.7 });
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatTopped).BoundBy(TriangleBound.Hexagon(3)),
                "tri_ft.svg",
                new Options { textScale = 0.7 });
            Export(
                new TriHexGrid().BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(6, 6))),
                "trihex.svg",
                new Options { textScale = 0.7 });
            Export(
                new SquareSnubGrid().BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))),
                "snub.svg",
                new Options { textScale = 0.5 });
        }
    }
}
