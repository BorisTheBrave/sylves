using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
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
            public int? dim = null;
            public double? textScale = 1;
            public float strokeWidth = 0.1f;
            public Vector2 min = new Vector2(-2, -2);
            public Vector2 max = new Vector2(2, 2);
            public bool includeDual = false;
            public bool trim = false;
            public Func<Cell, string> fillFunc = null;
            public Action<SvgBuilder> postProcess = null;
        }

        private const bool IsSeed = false;

        public static void WriteCells(IGrid grid, IEnumerable<Cell> cells, TextWriter tw, SvgBuilder b, Options options)
        {
            foreach (var cell in cells)
            {
                b.DrawCell(grid, cell, options.fillFunc?.Invoke(cell));
            }
            if (options.textScale != null)
            {
                foreach (var cell in cells)
                {
                    b.DrawCoordinateLabel(grid, cell, options.dim ?? grid.CoordinateDimension, options.textScale.Value);
                }
            }
        }

        public static void BeginSvg(SvgBuilder b, Options options)
        {

            var min = options.min;
            var max = options.max;
            b.BeginSvg($"{min.x} {-max.y} {max.x - min.x} {max.y - min.y}", options.strokeWidth);
        }

        public static void WriteGrid(IGrid grid, TextWriter tw, Options options)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (IsSeed)
            {
                if (grid.Unwrapped is PeriodicPlanarMeshGrid)
                {
                    grid = grid.BoundBy(new SquareBound(0, 0, 1, 1));
                }
                options.textScale = null;
            }
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
            BeginSvg(b, options);
            var cells = grid.GetCells();
            if(options.trim)
            {
                cells = cells.Where(c =>
                {
                    var polygon = grid.GetPolygon(c);
                    return !(polygon.All(v => v.x < min.x) ||
                        polygon.All(v => v.y < min.y) ||
                        polygon.All(v => v.x > max.x) ||
                        polygon.All(v => v.y > max.y));
                });
            }
            WriteCells(grid, cells, tw, b, options);

            if(options.includeDual)
            {
                var dualGrid = grid.GetDual().DualGrid;
                tw.WriteLine("<g class=\"dual\">");
                WriteCells(dualGrid, dualGrid.GetCells(), tw, b, options);
                tw.WriteLine("</g>");
            }
            if(options.postProcess != null)
            {
                options.postProcess(b);
            }

            b.EndSvg();
        }
        public static void Export(IGrid g, string filename, Options? options = null)
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

        private static void ExportPrototiles(string filename, Prototile[] prototiles)
        {

            Matrix4x4 globalTransform = Matrix4x4.Scale(new Vector3(1, -1, 1));
            const float strideY = 10;
            const string viewBox = "0 -50 25 50";

            var byName = prototiles.ToDictionary(x => x.Name);

            var fullPath = Path.GetFullPath(filename);
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var tw = new StreamWriter(file))
            {
                var svg = new SvgBuilder(tw);

                void DrawPoly(Vector3[] vertices, Matrix4x4 transform, bool isParent)
                {
                    tw.Write($@"<path class=""cell-path"" style=""{(isParent ? "stroke: grey; stroke-width:  0.2" : "fill: none")}"" d=""");
                    SvgExport.WritePathCommands(vertices, globalTransform * transform, tw);
                    tw.WriteLine("\"/>");
                }


                svg.BeginSvg(viewBox);
                var y = 0f;
                foreach (var prototile in prototiles)
                {
                    var transform = Matrix4x4.Translate(new Vector3(0, y, 0));
                    tw.WriteLine($"<!-- {prototile.Name} -->");
                    foreach (var tile in prototile.ChildTiles)
                        DrawPoly(tile, transform, true);
                    foreach (var child in prototile.ChildPrototiles)
                    {
                        var childPrototile = byName[child.childName];
                        foreach (var tile in childPrototile.ChildTiles)
                        {
                            DrawPoly(tile, transform * child.transform, false);
                        }
                    }
                    y += strideY;
                }
                svg.EndSvg();
            }
        }

        private static void ExportTreeView(string filename, SubstitutionTilingGrid grid, Options options)
        {
            var h = (grid.GetBound() as SubstitutionTilingBound).Height;
            Matrix4x4 globalTransform = Matrix4x4.Scale(new Vector3(1, -1, 1));


            var fullPath = Path.GetFullPath(filename);
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var tw = new StreamWriter(file))
            {
                var svg = new SvgBuilder(tw);

                BeginSvg(svg, options);
                var y = 0;
                foreach (var cell in grid.GetCells())
                {
                    svg.DrawCell(grid, cell);
                }

                var currentCells = grid.GetCells().ToHashSet();
                var currentGrid = grid;
                for (var i = 1; i <= h; i++)
                {
                    var parentGrid = grid.ParentGrid(i);
                    var parentCells = new HashSet<Cell>();
                    foreach (var cell in currentCells)
                    {
                        var parentCell = currentGrid.CellToParentGrid(cell);
                        parentCells.Add(parentCell);
                        var fromP = currentGrid.GetCellCenter(cell);
                        var toP = parentGrid.GetCellCenter(parentCell);
                        tw.WriteLine($"<!-- From {cell} at height {i - 1} to {parentCell} at height {i} -->");
                        tw.Write($@"<path style=""stroke: rgb(255, 255, 255); stroke-width: 0.11"" d=""");
                        SvgExport.WritePathCommands(new[] { fromP, toP }, globalTransform, tw, close: false);
                        tw.WriteLine("\"/>");
                        tw.Write($@"<path style=""stroke: rgb(51, 51, 51); stroke-width: 0.1"" d=""");
                        SvgExport.WritePathCommands(new[] { fromP, toP }, globalTransform, tw, close: false);
                        tw.WriteLine("\"/>");
                    }
                    foreach (var parentCell in parentCells)
                    {
                        var toP = parentGrid.GetCellCenter(parentCell);
                        tw.WriteLine($@"<circle cx=""{toP.x}"" cy=""{-toP.y}"" r=""0.2""/>");
                    }

                    currentGrid = parentGrid;
                    currentCells = parentCells;
                }

                svg.EndSvg();
            }
        }

        private static void ExportSpigotView(string filename, SubstitutionTilingGrid grid, Options options)
        {

            var h = (grid.GetBound() as SubstitutionTilingBound).Height;
            Matrix4x4 globalTransform = Matrix4x4.Scale(new Vector3(1, -1, 1));


            var fullPath = Path.GetFullPath(filename);
            using (var file = File.Open(fullPath, FileMode.Create))
            using (var tw = new StreamWriter(file))
            {
                var svg = new SvgBuilder(tw);

                BeginSvg(svg, options);

                for (var i = 0; i < h; i++)
                {
                    var parentGrid = grid.ParentGrid(i);

                    var cells = parentGrid.GetCellsInBounds(new SubstitutionTilingBound { Height = 1 })
                        .Where(x => i == 0 || x != new Cell());
                    foreach(var cell in cells)
                    {
                        svg.DrawCell(parentGrid, cell);
                    }
                }

                svg.EndSvg();
            }
        }


        [Test]
        public void ExportSubstitutionGrids()
        {
            // Aperiodic grids
            Export(
                new DominoGrid().Transformed(Matrix4x4.Translate(new Vector3(0, 3, 0))).BoundBy(new SubstitutionTilingBound { Height = 4 }),
                "domino.svg",
                new Options { textScale = 0.5, min = new Vector2(-3, -3), max = new Vector2(3, 3), trim = true });

            var chairOptions = new Options
            {
                //textScale = null,
                min = new Vector2(-5, -5),
                max = new Vector2(5, 5),
                trim = true,
                //fillFunc = c => (c.x % 4) switch { 2 => "#e6ed69", 1 or 3 => "#9def94", 0 => "#f0905e" },
            };
            var chairGrid = new ChairGrid(new SubstitutionTilingBound { Height = 6 });
            Export(
                chairGrid,
                "chair.svg",
                chairOptions);

            var penroseOptions = new Options { textScale = 1, min = new Vector2(-15, -15), max = new Vector2(15, 15), trim = true,
                fillFunc = (c) => c.x % 2 == 0 ? "mediumpurple" : "greenyellow" };
            var penroseGrid = new PenroseRhombGrid(new SubstitutionTilingBound { Height = 10 });
            Export(
                penroseGrid,
                "penrose_rhomb.svg",
                penroseOptions
                );

            var ammannBeenkerGrid = new AmmannBeenkerGrid(new SubstitutionTilingBound { Height = 1 });
            var ammannBeenkerOptions = new Options
            {
                textScale = null,
                min = new Vector2(-80, -80),
                max = new Vector2(80, 80),
                trim = true,
                fillFunc = (c) => ammannBeenkerGrid.GetPrototileName(c) == "Square" ? "#ffcc66" : "#000060"
            };
            Export(
                ammannBeenkerGrid,
                "ammann_beenker.svg",
                ammannBeenkerOptions
                );



            var sphinxGrid = new SphinxGrid(new SubstitutionTilingBound { Height = 8 });
            var sphinxGridOptions = new Options
            {
                textScale = null,
                min = new Vector2(-30, -30),
                max = new Vector2(30, 30),
                trim = true,
                fillFunc = (c) => sphinxGrid.GetPrototileName(c) == "Sphinx" ? "#cecbe3" : "#f96610x"
            };
            Export(
                sphinxGrid,
                "sphinx.svg",
                sphinxGridOptions
                );


            // Export prototiles (i.e. substitution rules)
            ExportPrototiles("chair_prototiles.svg", ChairGrid.Prototiles);
            ExportPrototiles("penrose_rhomb_prototiles.svg", PenroseRhombGrid.Prototiles);
            ExportPrototiles("ammann_beenker_prototiles.svg", AmmannBeenkerGrid.Prototiles);

            // Tree view
            ExportTreeView("chair_tree.svg", chairGrid, chairOptions);
            ExportTreeView("penrose_rhomb_tree.svg", penroseGrid, penroseOptions);
            ExportTreeView("ammann_beenker_tree.svg", penroseGrid, penroseOptions);

            // Spigot view
            ExportSpigotView("chair_spigot.svg", chairGrid, chairOptions);
            ExportSpigotView("penrose_rhomb_spigot.svg", penroseGrid, penroseOptions);
            var hat = new HatGrid(bound: new SubstitutionTilingBound { Height = 3 });
            var positions = hat.GetCells().Select(hat.GetCellCenter).ToList();
            Export(
                hat,
                "hat.svg",
                new Options { textScale=3f, min=new Vector2(-50, -50), max = new Vector2(50, 50)}
                );

            ExportPrototiles("hat_prototiles.svg", HatGrid.Prototiles);
            //ExportPrototiles("hat_metaprototiles.svg", HatGrid.MetaPrototiles);

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
            var r = new System.Random(0);
            var points = Enumerable.Range(0, 100).Select(x => new Vector2((float)r.NextDouble(), (float)r.NextDouble()) * 10).ToList();
            Export(
                new VoronoiGrid(points),
                "voronoi.svg",
                new Options { min = new Vector2(0, 0), max = new Vector2(10, 10), textScale = 0.5 });
            Export(
                new JitteredSquareGrid().BoundBy(new SquareBound(-1, -1, 2, 2)),
                "jitteredsquare.svg",
                new Options { min = new Vector2(5, 5), max = new Vector2(15, 15), textScale = 0.5 });

            // Mesh grids
            Export(new MeshGrid(TestMeshes.Lion).Transformed(Matrix4x4.Scale(4f * Vector3.one)), "meshgrid.svg", new Options { textScale = null});

            // Extras grids
            Export(new TownscaperGrid(4).BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(3, 3))).Transformed(Matrix4x4.Scale(Vector3.one * 0.3f)), "townscaper.svg", new Options { textScale = null, strokeWidth = 0.01f, });
            Export(new TownscaperGrid(4, 0).BoundBy(new SquareBound(new Vector2Int(-2, -2), new Vector2Int(3, 3))).Transformed(Matrix4x4.Scale(Vector3.one * 0.3f)), "unrelaxedtownscaper.svg", new Options { textScale = null, strokeWidth = 0.01f, });
            Export(new OffGrid(0.2f, new SquareBound(-4, -4, 5, 5)), "off.svg", new Options { textScale = 0.5f, min = new Vector2(-3, -3), max = new Vector2(3, 3)});

            // Modifier grids
            var centerSquare = new SquareGrid(1).BoundBy(new SquareBound(new Vector2Int(-3, -3), new Vector2Int(4, 4))).Transformed(Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0)));

            Export(
                centerSquare,
                "center_square.svg",
                new Options { dim = 2 });
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
                new WrapModifier(centerSquare.BoundBy(new SquareBound(new Vector2Int(0, 0), new Vector2Int(2, 2))), x => new Cell(x.x % 2, x.y % 2)),
                "wrap_square.svg",
                new Options { dim = 2 });
            Export(
                new RavelModifier(centerSquare.BoundBy(new SquareBound(new Vector2Int(-1, -1), new Vector2Int(2, 2)))),
                "ravel_square.svg",
                new Options { dim = 2 });
            Export(
                new RelaxModifier(centerSquare, relaxIterations: 10),
                "relax_square.svg",
                new Options { dim = 2, textScale = null });
            Export(
                new TestNestedModifier(centerSquare),
                "nested_square.svg",
                new Options { dim = 2, textScale = null });
        }

        private class TestNestedModifier : NestedModifier
        {
            public TestNestedModifier(IGrid grid):
                base(grid)
            {

            }

            public override IGrid Unbounded => throw new NotImplementedException();

            public override IGrid BoundBy(IBound bound)
            {
                throw new NotImplementedException();
            }

            protected override IGrid GetChildGrid(Cell chunkCell)
            {
                var r = (float)new System.Random(HashUtils.Hash(chunkCell.x, chunkCell.y)).NextDouble();
                var i = Mathf.FloorToInt(r * 6);

                var tg = new TriangleGrid(0.3f, bound: TriangleBound.Hexagon(1));
                var mg = new MeshGrid(tg.ToMeshData());
                return mg
                    .Masked(x=>x.x != i)
                    .Transformed(Matrix4x4.Translate(new Vector3(chunkCell.x, chunkCell.y, 0)));
            }
        }

        [Test]
        [Ignore("slow!")]
        public void ExportTutorialImages()
        {
            var triangleGrid = new TriangleGrid(0.5f, TriangleOrientation.FlatSides, bound: TriangleBound.Hexagon(4));
            var meshData = triangleGrid.ToMeshData();

            {
                var options = new Options { textScale = null, strokeWidth = 0.05f, };

                Export(new MeshGrid(meshData), "townscaper_tutorial_1.svg", options);
                meshData = meshData.RandomPairing(new System.Random(1).NextDouble);
                Export(new MeshGrid(meshData), "townscaper_tutorial_2.svg", options);
                meshData = ConwayOperators.Ortho(meshData);
                Export(new MeshGrid(meshData), "townscaper_tutorial_3.svg", options);
                meshData = meshData.Weld();
                meshData = meshData.Relax();
                Export(new MeshGrid(meshData), "townscaper_tutorial_4.svg", options);
            }

            // TODO: This doesn't work as PlanarPrismModifier needs the 3rd coordinate for itself.
            if(false)
            {
                var height = 100;
                var options = new PlanarPrismOptions { };
                var townscaperGrid = new TownscaperGrid(4).BoundBy(new SquareBound(-1, -1, 0, 0));
                var townscaper3dGrid = new PlanarPrismModifier(townscaperGrid, options, 0, height);
                ExportObj(townscaper3dGrid, "townscaper_tutorial_5.obj");
            }
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
            ExportObj(new PlanarPrismModifier(new TownscaperGrid(4).GetCompactGrid()).BoundBy(new PlanarPrismBound { MinLayer=0, MaxLayer = 2, PlanarBound=new SquareBound(0, 0, 3, 3)}), "townscaper_layers.obj");

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

        [Test]
        public void ExportDualGrid()
        {
            Export(new SquareGrid(1, new SquareBound(new Vector2Int(-3, -3), new Vector2Int(3, 3))), "square_dual.svg", new Options { textScale = null, includeDual = true,});
            Export(new HexGrid(1, bound: HexBound.Hexagon(3)), "hex_dual.svg", new Options { textScale = null, includeDual = true,});
            Export(new TriangleGrid(1, bound: TriangleBound.Hexagon(3)), "tri_dual.svg", new Options { textScale = null, includeDual = true,});
            Export(new SquareGrid(1, new SquareBound(new Vector2Int(-0, -0), new Vector2Int(1, 1))), "bounded_square_dual.svg", new Options { textScale = null, includeDual = true, min=new Vector2(-1.1f, -1.1f), max = new Vector2(1.1f, 1.1f) });
        }

        [Test]
        [Ignore("Needs inspection to tell if there's a problem")]
        public void TestPrecision()
        {
            var n = 4;
            var ug = new UnrelaxedTownscaperGrid(n, 0, 1e-2f).BoundBy(new SquareBound(100, 100, 105, 105));
            var g = new RelaxModifier(ug, 4, relaxIterations: 1, weldTolerance: 1e-6f);
            var min = g.GetCells().Select(g.GetCellCenter).Aggregate(Vector3.Min);
            var max = g.GetCells().Select(g.GetCellCenter).Aggregate(Vector3.Max);
            Export(g, "precision.svg", new Options() { textScale = null, strokeWidth=0.05f, min = new Vector2(min.x, min.y), max = new Vector2(max.x, max.y) });
        }

        [Test]
        public void ExportKruskal()
        {
            var g = new TownscaperGrid(4, 99).BoundBy(new SquareBound(0, 0, 1, 1));
            //var g = new UnrelaxedTownscaperGrid(4, 99, 1e-6f).BoundBy(new SquareBound(0, 0, 1, 1));

            var tree = KruskalMinimumSpanningTree.Calculate(g, StepLengths.Euclidian(g));

            void PostProcess(SvgBuilder b)
            {
                var tw = b.TextWriter;
                tw.Write($@"<path style=""stroke-width: 0.05;stroke: red; stroke-linecap: round; fill: none"" d=""");
                foreach (var step in tree)
                {
                    var vertices = new[] { g.GetCellCenter(step.Src), g.GetCellCenter(step.Dest) };
                    SvgExport.WritePathCommands(vertices, SvgBuilder.FlipY, tw, false);
                }
                tw.WriteLine("\"/>");
            }

            Export(g, "kruskal.svg", new Options
            {
                postProcess = PostProcess,
                textScale = null,
            });
        }

        [Test]
        public void ExportOutline()
        {
            var g = new TownscaperGrid(4, 99).BoundBy(new SquareBound(-1,-1, 2, 2));
            var cells = g
                .GetCellsIntersectsApprox(new Vector3(-2, -2, -2), new Vector3(2, 2, 2))
                .Where(c =>
                {
                    var v = g.GetCellCenter(c);
                    return v.magnitude <= 1.8 &&
                        Vector3.Angle(v, Vector3.right) > 30 &&
                        (v - new Vector3(0.1f, 1, 0)).magnitude > 0.2;
                })
                .ToHashSet();

            var outlines = OutlineCells.Outline(g, cells);

            void PostProcess(SvgBuilder b)
            {
                var tw = b.TextWriter;
                foreach (var outline in outlines) 
                {
                    var vertices = outline.Edges.Select(t => g.GetCellCorner(t.Cell, (CellCorner)t.CellDir)).ToArray();
                    tw.Write($@"<path style=""stroke-width: 0.05;stroke: red; fill: none"" d=""");
                    SvgExport.WritePathCommands(vertices, SvgBuilder.FlipY, tw, outline.IsLoop);
                    tw.WriteLine("\"/>");

                }
            }

            Export(g, "outline.svg", new Options
            {
                postProcess = PostProcess,
                textScale = null,
            });
        }
    }
}
