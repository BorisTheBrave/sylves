using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    class MobiusSquareGrid : BijectModifier
    {
        public MobiusSquareGrid(int w, int h) : 
            base(
                new MeshGrid(MakeMeshData(w, h)),
                c => new Cell(c.x + w * c.y, 0, 0),
                c => new Cell(c.x % w, c.x / w, 0)
                )
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
                    vertices[x + (w + 1) * y] = new Vector3(
                        x1 * radius1 + x1 * x2 * radius2 * (y - h / 2.0f),
                        y1 * radius1 + y1 * y2 * radius2 * (y - h / 2.0f),
                        0            +      y2 * radius2 * (y - h / 2.0f)
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
            var indices = new int[4 * w * h];
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var i = 4 * (x + w * y);
                    indices[i + 0] = x + 1 + (w + 1) * (y + 0);
                    indices[i + 1] = x + 1 + (w + 1) * (y + 1);
                    indices[i + 2] = x + 0 + (w + 1) * (y + 1);
                    indices[i + 3] = x + 0 + (w + 1) * (y + 0);
                }
            }

            return new MeshData
            {
                indices = new[] { indices },
                vertices = vertices,
                topologies = new[] { MeshTopology.Quads },
                subMeshCount = 1,
            };
        }
    }
}
