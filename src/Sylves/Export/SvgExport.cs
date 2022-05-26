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
        private static void WritePathCommands(Vector3[] vertices, Matrix4x4 transform, TextWriter tw)
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

        public static void WriteGrid(IGrid grid, TextWriter tw)
        {
            tw.Write("<svg viewBox=\"-10 -10 20 20\" xmlns=\"http://www.w3.org/2000/svg\">");
            foreach (var cell in grid.GetCells())
            {
                grid.GetPolygon(cell, out var vertices, out var transform);
                tw.WriteLine($"<!-- {cell} -->");
                tw.Write("<path fill=\"none\" stroke=\"black\" stroke-width=\"0.1\" d=\"");
                WritePathCommands(vertices, transform, tw);
                tw.WriteLine("\"/>");
            }
            tw.Write("</svg>");
        }
    }
}
