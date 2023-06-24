using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A square grid on a Möbius strip.
    /// Demonstrates how Sylves handles non-orientability on 2d surfaces.
    /// </summary>
    public class MobiusSquareGrid : MeshGrid
    { 
        public MobiusSquareGrid(int width, int height)
            :base(MakeMeshData(width, height))
        {
        }

        private static MeshData MakeMeshData(int w, int h)
        {
            var radius1 = 10;
            var radius2 = 3;
            var vertices = new Vector3[(w + 1) * (h + 1)];
            for(var x = 0; x < w; x++)
            {
                for(var y = 0; y <= h; y++)
                {
                    var theta1 = (x * Mathf.PI * 2 / w);
                    var theta2 = (x * Mathf.PI / w);
                    var x1 = Mathf.Cos(theta1);
                    var y1 = Mathf.Sin(theta1);
                    var x2 = Mathf.Cos(theta2);
                    var y2 = Mathf.Sin(theta2);
                    var yy = y * 2.0f / h - 1;
                    vertices[x + (w + 1) * y] = new Vector3(
                        x1 * radius1 + x1 * x2 * radius2 * yy,
                        y1 * radius1 + y1 * x2 * radius2 * yy,
                        0            +      y2 * radius2 * yy
                        );

                }
            }
            for (var x = w; x <= w; x++)
            {
                for (var y = 0; y <= h; y++)
                {
                    vertices[x + (w + 1) * y] = vertices[0 + (w + 1) * (h - y)];

                }
            }
            var indices = new int[h][];
            for (var y = 0; y < h; y++)
            {
                indices[y] = new int[4 * w];
                for (var x = 0; x < w; x++)
                {
                    indices[y][4 * x + 0] = x + 1 + (w + 1) * (y + 0);
                    indices[y][4 * x + 1] = x + 1 + (w + 1) * (y + 1);
                    indices[y][4 * x + 2] = x + 0 + (w + 1) * (y + 1);
                    indices[y][4 * x + 3] = x + 0 + (w + 1) * (y + 0);
                }
            }

            return new MeshData
            {
                indices = indices,
                vertices = vertices,
                topologies = Enumerable.Range(0, h).Select(_ => MeshTopology.Quads).ToArray(),
            };
        }
    }
}
