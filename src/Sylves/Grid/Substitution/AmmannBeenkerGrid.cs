using System.Collections.Generic;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    // https://tilings.math.uni-bielefeld.de/substitution/ammann-beenker/
    public class AmmannBeenkerGrid : SubstitutionTilingGrid
	{
        public AmmannBeenkerGrid(SubstitutionTilingBound bound = null):base(Prototiles, new[] { "Square" }, bound)
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

        private static float Sqrt2 = Mathf.Sqrt(2);
        private static float ISqrt2 = 1 / Mathf.Sqrt(2);
        private static float Inflation = 1 + Mathf.Sqrt(2);
        private static float Deflation = 1 / Inflation;

        private static Prototile Square = new Prototile
        {
            Name = "Square",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 180, (ISqrt2+1) * Deflation, (ISqrt2+1) * Deflation), "Square"),
                (ScaleRotateAndTranslate(Deflation, 0, 0, 0), "Rhombus"),
                (ScaleRotateAndTranslate(Deflation, 45, 0, 0), "Rhombus"),
                //(ScaleRotateAndTranslate(Deflation, 180-45, 1, 0), "Square"),
                (ScaleRotateAndTranslate(Deflation, 180+45, 0, 1), "Square"),
                (ScaleRotateAndTranslate(Deflation, 90, 1, 0), "Rhombus"),
                (ScaleRotateAndTranslate(Deflation, -45, 0, 1), "Rhombus"),
                //(ScaleRotateAndTranslate(Deflation, 180+45, 1, 1), "Square"),
                (ScaleRotateAndTranslate(Deflation, 180-45, 1, 1), "Square"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (1, 2, 0, 2),
                    (2, 0, 1, 3),
                    (2, 1, 0, 1),
                    (3, 2, 2, 2),
                    (4, 2, 0, 3),
                    (5, 0, 3, 3),
                    (5, 1, 0, 0),
                    (6, 2, 5, 2),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 3, 1, 0),
                    (0, 1, 3, 1, 1),
                    (0, 2, 3, 4, 3),
                    (1, 0, 3, 4, 0),
                    (1, 1, 3, 4, 1),
                    (1, 2, 3, 6, 3),
                    (2, 0, 3, 6, 0),
                    (2, 1, 3, 6, 1),
                    (2, 2, 3, 5, 3),
                    (3, 0, 3, 3, 0),
                    (3, 1, 3, 3, 1),
                    (3, 2, 3, 2, 3),
                },
            InteriorTileAdjacencies = new (int, int, int, int)[0],
            ExteriorTileAdjacencies = new[]
            {
                (0, 0, 1, 0, 0),
                (1, 0, 1, 0, 1),
                (2, 0, 1, 0, 2),
                (3, 0, 1, 0, 3),
            },
            ChildTiles = new[]
                {
                    Polygon(0, 0, 1, 0, 1, 1, 0, 1),
                }
        };

        private static Prototile Rhombus = new Prototile
        {
            Name = "Rhombus",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 0, 0, 0), "Rhombus"),
                //(ScaleRotateAndTranslate(Deflation, 180-45, 1, 0), "Square"),
                (ScaleRotateAndTranslate(Deflation, 180, ISqrt2, ISqrt2), "Square"),
                (ScaleRotateAndTranslate(Deflation, 90, 1, 0), "Rhombus"),
                (ScaleRotateAndTranslate(Deflation, 0, 1, 0), "Square"),
                //(ScaleRotateAndTranslate(Deflation, -45, ISqrt2, ISqrt2), "Square"),
                (ScaleRotateAndTranslate(Deflation, 180, 1 + ISqrt2, ISqrt2), "Rhombus"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (1, 2, 0, 2),
                    (2, 2, 1, 3),
                    (3, 3, 2, 0),
                    (4, 2, 3, 2),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 3, 0, 0),
                    (0, 1, 3, 0, 1),
                    (0, 2, 3, 2, 3),
                    (1, 0, 3, 3, 0),
                    (1, 1, 3, 3, 1),
                    (1, 2, 3, 4, 3),
                    (2, 0, 3, 4, 0),
                    (2, 1, 3, 4, 1),
                    (2, 2, 3, 2, 1),
                    (3, 0, 3, 1, 0),
                    (3, 1, 3, 1, 1),
                    (3, 2, 3, 0, 3),
                },
            InteriorTileAdjacencies = new (int, int, int, int)[0],
            ExteriorTileAdjacencies = new[]
            {
                (0, 0, 1, 0, 0),
                (1, 0, 1, 0, 1),
                (2, 0, 1, 0, 2),
                (3, 0, 1, 0, 3),
            },
            ChildTiles = new[]
                {
                    Polygon(0, 0, 1, 0, 1 + ISqrt2, ISqrt2, ISqrt2, ISqrt2),
                }
        };

        public static Prototile[] Prototiles =
        {
            Square,
            Rhombus,
		};
	}
}

