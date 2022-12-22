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
    internal class GridDocsExportTest
    {
        public class Options
        {
            public int dim = 3;
            public double? textScale = 1;
            public Vector2 min = new Vector2(-2, -2);
            public Vector2 max = new Vector2(2, 2);
        }

        public static void WriteGrid(IGrid grid, TextWriter tw, Options options)
        {
            var b = new SvgBuilder(tw);
            /*
            Vector3 min = Vector3.zero, max = Vector3.zero;
            foreach (var cell in grid.GetCells())
            {
                foreach(var v in grid.GetPolygon(cell))
                {
                    min = Vector3.Min(min, v);
                    max = Vector3.Max(max, v);
                }
            }*/
            var min = options.min;
            var max = options.max;
            b.BeginSvg($"{min.x} {min.y} {max.x - min.x} {max.y-min.y}");
            foreach (var cell in grid.GetCells())
            {
                b.DrawCell(grid, cell);
            }
            if (options.textScale != null)
            {
                foreach (var cell in grid.GetCells())
                {
                    b.DrawCoordinateLabel(grid, cell, options.dim, options.textScale.Value);
                }
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

        public void ExportObj(IGrid g, string filename)
        {
            var fullPath = Path.GetFullPath(filename);
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                var mesh = g.ToMeshData();
                foreach(var v in mesh.vertices)
                {
                    writer.WriteLine($"v {v.x} {v.y} {v.z}");
                }
                foreach (var face in MeshUtils.GetFaces(mesh))
                {
                    //writer.WriteLine($"l {string.Join(" ", face.Select(x => x + 1))} {face[0] + 1}");
                    writer.WriteLine($"f {string.Join(" ", face.Select(x => x + 1))}");
                }
                Console.WriteLine($"Wrote file {fullPath}");
            }
        }


        [Test]
        public void ExportSvgGrids()
        {
            // Basic grids
            Export(
                new SquareGrid(1).BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(5, 5))),
                "square.svg",
                new Options { dim = 2});
            Export(
                new HexGrid(1, HexOrientation.PointyTopped).BoundBy(HexBound.Hexagon(5)),
                "hex_pt.svg",
                new Options { textScale = 0.7 });
            Export(
                new HexGrid(1, HexOrientation.FlatTopped).BoundBy(HexBound.Hexagon(5)),
                "hex_ft.svg",
                new Options { textScale = 0.7 });
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatSides).BoundBy(TriangleBound.Hexagon(3)),
                "tri_fs.svg",
                new Options { textScale = 0.5 });
            Export(
                new TriangleGrid(1, TriangleOrientation.FlatTopped).BoundBy(TriangleBound.Hexagon(3)),
                "tri_ft.svg",
                new Options { textScale = 0.5 });

            // Periodic grids
            Export(
                new CairoGrid().BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(6, 6))),
                "cairo.svg",
                new Options { textScale = 0.7 });
            Export(
                new TriHexGrid().BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(6, 6))).Transformed(Matrix4x4.Scale(2f * Vector3.one)),
                "trihex.svg",
                new Options { textScale = 0.7 });
            Export(
                new MetaHexagonGrid().BoundBy(new SquareBound(new Vector2Int(-5, -5), new Vector2Int(6, 6))).Transformed(Matrix4x4.Scale(2f * Vector3.one)),
                "metahexagon.svg",
                new Options { textScale = null });
            Export(
                new SquareSnubGrid().BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))),
                "squaresnub.svg",
                new Options { textScale = 0.5 });
            Export(
                new TetrakisSquareGrid().BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))).Transformed(Matrix4x4.Scale(2f * Vector3.one)),
                "tetrakissquare.svg",
                new Options { textScale = 0.5 });
            Export(
                new RhombilleGrid().BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))).Transformed(Matrix4x4.Scale(2f * Vector3.one)),
                "rhombille.svg",
                new Options { textScale = 0.5 });

            // Mesh grids
            Export(new MeshGrid(TestMeshes.Lion).Transformed(Matrix4x4.Scale(4f * Vector3.one)), "meshgrid.svg", new Options { textScale = null});

            // Modifier grids
            var centerSquare = new SquareGrid(1).BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))).Transformed(Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0)));


            Export(
                new BijectModifier(centerSquare, x => new Cell((x.x + x.y) / 2, (x.x - x.y) / 2, x.z), x => new Cell(x.x - x.y, x.x + x.y, x.z)),
                "biject_square.svg",
                new Options { dim = 2 });
            Export(
                centerSquare.Masked(cell => cell.x == 0 || cell.y == 0),
                "mask_square.svg",
                new Options { dim = 2 });
            Export(
                centerSquare.Transformed(Matrix4x4.Rotate(Quaternion.Euler(0, 0, 45))),
                "transform_square.svg",
                new Options { dim = 2 });
            Export(
                new WrapModifier(centerSquare.BoundBy(new SquareBound(new Vector2Int(-1, -1), new Vector2Int(2, 2))), x => new Cell((x.x + 1) % 3 - 1, (x.y + 1) % 3 - 1)),
                "wrap_square.svg",
                new Options { dim = 2 });
        }

        [Test]
        public void ExportObjGrids()
        {
            ExportObj(new CubeGrid(1, new CubeBound(new Vector3Int(-2, -2, -2), new Vector3Int(3, 3, 3))), "cube.obj");
            ExportObj(new HexPrismGrid(1, 1, bound: new HexPrismBound(HexBound.Hexagon(2), 0, 2)), "hexprism.obj");
            ExportObj(new TrianglePrismGrid(1, 1, bound: new TrianglePrismBound(TriangleBound.Hexagon(2), 0, 2)), "triangleprism.obj");
            ExportObj(new MobiusSquareGrid(10, 10), "mobiussquare.obj");
            ExportObj(new CubiusGrid(8, 2), "cubius.obj");
            var cairoBound = new SquareBound(-3, -3, 3, 3);
            ExportObj(new PlanarPrismModifier(new RavelModifier(new CairoGrid().BoundBy(cairoBound)), new PlanarPrismOptions { LayerHeight = 0.24f, }).BoundBy(new PlanarPrismBound { PlanarBound = cairoBound, MinLayer = 0, MaxLayer = 1}), "planarprismmodifier.obj");
            var ico = TestMeshes.Icosahedron;
            ico.RecalculateNormals();
            ExportObj(new MeshPrismGrid(ico, new MeshPrismGridOptions { LayerHeight = 0.25f }), "meshprism.obj");

            /* Handy blender script for tweaking these
import bpy

# Set up the imported object properly
bpy.ops.object.modifier_add(type='WELD')
bpy.ops.object.modifier_add(type='WIREFRAME')
bpy.context.object.modifiers["Wireframe"].thickness = 0.1

bpy.context.object.active_material.node_tree.nodes["Principled BSDF"].inputs[0].default_value = (0.000973191, 0, 0.8, 1)

# Set up rendering
bpy.context.scene.render.resolution_y = 1080
bpy.context.scene.render.resolution_x = 1080


bpy.context.scene.view_layers["ViewLayer"].use_pass_mist = True

scene = bpy.context.scene
node_tree = scene.node_tree
scene.use_nodes = True
rlayers = node_tree.nodes.new("CompositorNodeRLayers")
mix = node_tree.nodes.new("CompositorNodeMixRGB")
composite = node_tree.nodes.new("CompositorNodeComposite")
node_tree.links.new(rlayers.outputs["Image"], mix.inputs[1])
node_tree.links.new(rlayers.outputs["Mist"], mix.inputs[2])
node_tree.links.new(mix.outputs[0], composite.inputs[0])

mix.inputs[0].default_value = 0.433333

            */
        }
    }
}
