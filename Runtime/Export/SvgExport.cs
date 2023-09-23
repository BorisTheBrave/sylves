using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Sylves
{
    public static class SvgExport
    {
        public static void WritePathCommands(Vector3[] vertices, Matrix4x4 transform, TextWriter tw, bool close = true)
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
            if (close)
            {
                tw.Write('Z');
            }
        }
    }

    public class SvgBuilder
    {
        TextWriter tw;

        // By Default, flip everything as Sylves uses y-up and svg uses y-down
        Matrix4x4 globalTransform = Matrix4x4.Scale(new Vector3(1, -1, 1));

        public SvgBuilder(TextWriter tw)
        {
            this.tw = tw;
        }

        public TextWriter TextWriter => tw;

        public void BeginSvg(string viewBox= "-5 -5 10 10", float strokeWidth=0.1f)
        {
            tw.WriteLine($"<svg viewBox=\"{viewBox}\" xmlns=\"http://www.w3.org/2000/svg\">");
            tw.WriteLine("<style>");
            tw.WriteLine($@".cell-path {{
                stroke-linejoin: round;
                fill: rgb(244, 244, 241);
                stroke: rgb(51, 51, 51);
                stroke-width: {strokeWidth}
            }}");
            tw.WriteLine($@".dual .cell-path {{
                fill: none;
                stroke: rgb(255, 0, 0);
                stroke-opacity: 0.5;
                stroke-width: {strokeWidth / 3}
            }}");
            tw.WriteLine("</style>");
        }

        public void EndSvg()
        {
            tw.WriteLine("</svg>");
        }

        public void DrawCell(IGrid grid, Cell cell, string fill = null)
        {
            var styleString = "";
            if(fill != null)
            {
                styleString = $@" style=""fill: {fill}""";
            }
            grid.GetPolygon(cell, out var vertices, out var transform);
            tw.WriteLine($"<!-- {cell} -->");
            tw.Write($@"<path class=""cell-path""{styleString} d=""");
            SvgExport.WritePathCommands(vertices, globalTransform * transform, tw);
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

            var cellCenter = globalTransform.MultiplyPoint3x4(grid.GetCellCenter(cell));
            tw.WriteLine($@"<g transform=""translate({ cellCenter.x},{ cellCenter.y + 0.08}) scale({textScale})"">");
            foreach (var textStyle in new[] { stroke_text_style, text_style })
            {
                tw.Write($@"<text text-anchor=""middle"" alignment-baseline=""middle"" style=""{ textStyle}"">");
                tw.Write($@"<tspan {xs}>{cell.x}</tspan>");
                if (dim >= 2)
                {
                    tw.Write($@", <tspan {ys}>{cell.y}</tspan>");
                }
                if (dim >= 3) {
                    tw.Write($@", <tspan {zs}>{cell.z}</tspan>");
                }
                tw.WriteLine($@"</text>");
            }
            tw.WriteLine("</g>");
        }
    }
}
