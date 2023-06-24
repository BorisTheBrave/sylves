using System.Linq;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// A torus with a quarter turn. Demonstrates how Sylves handles non-orientability on 3d surfaces.
    /// </summary>
    public class CubiusGrid : MeshPrismGrid
    { 
        public CubiusGrid(int width, int height, float outerRadius = 10, float innerRadius = 3)
            :base(MakeMeshData(width, height, outerRadius, innerRadius), Options(width, height, outerRadius, innerRadius), MakeData(width, height, outerRadius, innerRadius), false)
        {
        }

        private static MeshPrismGridOptions Options(int w, int h, float outerRadius, float innerRadius) => new MeshPrismGridOptions
        {
            LayerHeight = 2 * innerRadius / h,
            LayerOffset = -2 * innerRadius / h * (h - 1) / 2,
            MinLayer = 0,
            MaxLayer = h,
        };

        private static DataDrivenData MakeData(int w, int h, float outerRadius, float innerRadius)
        {
            var meshData = MakeMeshData(w, h, outerRadius, innerRadius);
            var meshPrismGridOptions = Options(w, h, outerRadius, innerRadius);
            var data = MeshGridBuilder.Build(meshData, meshPrismGridOptions);
            // Add connection back to start
            for (var y = 0; y < h; y++)
            {
                for (var z = 0; z < h; z++)
                {
                    data.Moves[(new Cell(w - 1, y, z), (CellDir)CubeDir.Right)] = (new Cell(0, z, h - 1 - y), (CellDir)CubeDir.Left, new Connection { Rotation = 1, Sides = 4 });
                    data.Moves[(new Cell(    0, y, z), (CellDir)CubeDir.Left)]  = (new Cell(0, h - 1 - z, y), (CellDir)CubeDir.Right, new Connection { Rotation = 3, Sides = 4 });
                }
            }
            return data;
        }

        private static MeshData MakeMeshData(int w, int h, float outerRadius, float innerRadius)
        {
            var radius1 = outerRadius;
            var radius2 = innerRadius;
            var vertices = new Vector3[(w + 1) * (h + 1)];
            var normals = new Vector3[(w + 1) * (h + 1)];
            for (var x = 0; x <= w; x++)
            {
                for(var y = 0; y <= h; y++)
                {
                    var theta1 = (x * Mathf.PI * 2 / w);
                    var theta2 = (x * Mathf.PI / 2 / w);// Only makes a quarter turn
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
                    normals[x + (w + 1) * y] = new Vector3(
                        x1 * y2,
                        y1 * y2,
                             -x2
                        );

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

            var meshData = new MeshData
            {
                indices = indices,
                vertices = vertices,
                normals = normals,
                topologies = Enumerable.Range(0, h).Select(_ => MeshTopology.Quads).ToArray(),
            };
            return meshData;
        }
    }
}
