using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // https://tilings.math.uni-bielefeld.de/substitution/sphinx/
    public class SphinxGrid : SubstitutionTilingGrid
	{
        public SphinxGrid(SubstitutionTilingBound bound = null):base(Prototiles, new[] { "Sphinx", "Sphinx2" }, bound)
        {

        }

        private static Matrix4x4 ScaleRotateAndTranslate(float scale, float angle, float x, float y)
        {
            return Matrix4x4.Translate(new Vector3(x, y, 0)) * Matrix4x4.Rotate(Quaternion.AngleAxis(angle, Vector3.forward)) * Matrix4x4.Scale(new Vector3(scale, scale, scale));
        }
		private static Vector3[] Polygon(params float[] v)
		{
			var r = new Vector3[v.Length / 2];
			for(var i=0;i<v.Length;i+=2)
			{
				r[i / 2].x = v[i];
				r[i / 2].y = v[i + 1];
            }
			return r;
        }


        // Prototiles roughly follow the scheme shown here:
        // https://tilings.math.uni-bielefeld.de/substitution/penrose-rhomb/
        // But with fewer tiles to avoid duplciates. We only keep tiles where vertex 1 is inside the bounds

        private static float Sqrt3 = Mathf.Sqrt(3);
        private static float HSqrt3 = Mathf.Sqrt(3) / 2;
        private static float Inflation = 2;
        private static float Deflation = 1 / Inflation;

        private static Prototile Sphinx = new Prototile
        {
            Name = "Sphinx",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 0, 1.5f, 0), "Sphinx2"),
                (ScaleRotateAndTranslate(Deflation, 0, 3, 0), "Sphinx2"),
                (ScaleRotateAndTranslate(Deflation, 180, 1, HSqrt3), "Sphinx2"),
                (ScaleRotateAndTranslate(Deflation, -120, 1, Sqrt3), "Sphinx"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (2, 3, 0, 4),
                    (2, 4, 0, 3),
                    (2, 5, 1, 7),
                    (2, 6, 1, 6),
                    (2, 7, 1, 5),
                    (3, 3, 0, 6),
                    (3, 4, 0, 5),
                    (3, 5, 2, 2),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 2, 0, 0),
                    (0, 1, 2, 0, 1),
                    (1, 0, 2, 0, 2),
                    (1, 1, 2, 1, 0),
                    (2, 0, 2, 1, 1),
                    (2, 1, 2, 1, 2),
                    (3, 0, 2, 1, 3),
                    (3, 1, 2, 1, 4),
                    (4, 0, 2, 2, 0),
                    (4, 1, 2, 2, 1),
                    (5, 0, 2, 3, 6),
                    (5, 1, 2, 3, 7),
                    (6, 0, 2, 3, 0),
                    (6, 1, 2, 3, 1),
                    (7, 0, 2, 3, 2),
                    (7, 1, 2, 0, 7),
                },
            ChildTiles = new[]
                {
                    Polygon(0, 0, 1, 0, 2, 0, 3, 0, 2.5f, HSqrt3, 1.5f, HSqrt3, 1, Sqrt3, 0.5f, HSqrt3),
                }
        }.HasSingleTile();

        private static Prototile Sphinx2 = Sphinx
            .HasSingleTile()
            .Mirror()
            .Rename("Sphinx2")
            .RenameChildren(new Dictionary<string, string> { { "Sphinx", "Sphinx2" }, { "Sphinx2", "Sphinx" } })
            .SwapChildren(0, 2)
            ;

        public static Prototile[] Prototiles =
        {
            Sphinx,
            Sphinx2,
		};
	}
}

