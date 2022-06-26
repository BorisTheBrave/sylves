using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public static class SvgExport
    {
        public static void WritePathCommands(Vector3[] vertices, Matrix4x4 transform, TextWriter tw)
        {
            var first = true;
            foreach(var v in vertices)
            {
                var v2 = transform.MultiplyPoint3x4(v);
                    tw.Write(first ? 'M' : 'L');
                first = false;
                tw.Write(v2.x);
                tw.Write(' ');
                tw.Write(v2.y);
            }
            tw.Write('Z');
        }
    }

    public class SvgBuilder
    {
        TextWriter tw;

        public SvgBuilder(TextWriter tw)
        {
            this.tw = tw;
        }

        public TextWriter TextWriter => tw;

        public void BeginSvg(string viewBox= "-5 -5 10 10")
        {
            tw.Write($"<svg viewBox=\"{viewBox}\" xmlns=\"http://www.w3.org/2000/svg\">");
        }

        public void EndSvg()
        {
            tw.Write("</svg>");
        }

        public void DrawCell(IGrid grid, Cell cell)
        {
            var cellPolyStyle = "fill: rgb(244, 244, 241); stroke: rgb(51, 51, 51); stroke-width: 0.1";

            grid.GetPolygon(cell, out var vertices, out var transform);
            tw.WriteLine($"<!-- {cell} -->");
            tw.Write($@"<path style=""{cellPolyStyle}"" d=""");
            SvgExport.WritePathCommands(vertices, transform, tw);
            tw.WriteLine("\"/>");
        }

        public void DrawCoordinateLabel(IGrid grid, Cell cell, int dim = 3, double textScale = 1.0)
        {
            // Style is hard coded for now
            var stroke_text_style = "fill: rgb(51, 51, 51); font-size: 0.3px;stroke: white; stroke-width: 0.05";
            var text_style = @"fill: rgb(51, 51, 51); font-size: 0.3px;";
            var xs = @"style=""fill: hsl(90, 100%, 35%); font-weight: bold"" ";
            var ys = @"style=""fill: hsl(300, 80%, 50%); font-weight: bold"" ";
            var zs = @"style=""fill: hsl(200, 100%, 45%); font-weight: bold"" ";

            var cellCenter = grid.GetCellCenter(cell);
            tw.WriteLine($@"<g transform=""translate({ cellCenter.x},{ cellCenter.y + 0.08}) scale({textScale})"">");
            foreach (var textStyle in new[] { stroke_text_style, text_style })
            {
                tw.Write($@"<text text-anchor=""middle"" alignment-baseline=""middle"" style=""{ textStyle}"">");
                tw.Write($@"<tspan {xs}>{cell.x}</tspan>, <tspan {ys}>{cell.y}</tspan>");
                if (dim >= 3) {
                    tw.Write($@", <tspan {zs}>{cell.z}</tspan>");
                }
                tw.WriteLine($@"</text>");
            }
            tw.WriteLine("</g>");
        }
    }
}
