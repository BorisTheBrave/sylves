using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // https://tilings.math.uni-bielefeld.de/substitution/domino/
    public class PenroseRhombGrid : SubstitutionTilingGrid
	{
        public PenroseRhombGrid(SubstitutionTilingBound bound = null):base(Prototiles, new[] { "Fat" }, bound)
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

        private static float Sin72 = Mathf.Sin(72 / 360f * Mathf.PI * 2);
        private static float Cos72 = Mathf.Cos(72 / 360f * Mathf.PI * 2);
        private static float Sin144 = Mathf.Sin(144 / 360f * Mathf.PI * 2);
        private static float Cos144 = Mathf.Cos(144 / 360f * Mathf.PI * 2);
        private static float Inflation = Mathf.Sqrt(5) / 2 + 0.5f;
        private static float Deflation = 1 / Inflation;

        // Fat Rhomb has angles 72 and 108
        // The distinguished corner is 0
        private static Prototile Fat = new Prototile
        {
            Name = "Fat",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 180, (1 + Cos72) * Deflation, Sin72 * Deflation), "Fat"),
                (ScaleRotateAndTranslate(Deflation, 0, Cos72 * Deflation, Sin72 * Deflation), "Thin"),
                (ScaleRotateAndTranslate(Deflation, 180 + 36, 1 + Cos72, Sin72), "Fat"),
                //(ScaleRotateAndTranslate(Deflation, -72, Deflation, 0), "Thin"),
                //(ScaleRotateAndTranslate(Deflation, 180 - 36, 1 + Cos72, Sin72), "Fat"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                (0, 0, 1, 0),
                (1, 0, 0, 0),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                (0, 0, 3, 0, 2),
                (0, 1, 3, 0, 3),
                (0, 2, 3, 2, 1),
                (1, 0, 2, 2, 2),
                (1, 1, 2, 2, 3),
                (2, 0, 2, 2, 0),
                (2, 1, 2, 1, 1),
                (3, 0, 3, 1, 2),
                (3, 1, 3, 1, 3),
                (3, 2, 3, 0, 1),
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
                    Polygon(0, 0, 1, 0, 1 + Cos72, Sin72, Cos72, Sin72),
                }
        };

        // Thin Rhomb has angles 36 and 144
        // The distinguished corner is 0
        private static Prototile Thin = new Prototile
        {
            Name = "Thin",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 180+36+36+36,  1 + Cos144, Sin144), "Fat"),
                (ScaleRotateAndTranslate(Deflation, 36*3,  1 - Deflation, 0), "Thin"),
                //(ScaleRotateAndTranslate(Deflation, 180-36,  1 + Cos144, Sin144), "Fat"),
                //(ScaleRotateAndTranslate(Deflation, -36*3,  Cos144 * (1 - Deflation), Sin144 * (1 - Deflation)), "Thin"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                (0, 0, 1, 0),
                (1, 0, 0, 0),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                (0, 0, 3, 1, 2),
                (0, 1, 3, 1, 3),
                (0, 2, 3, 0, 1),
                (1, 0, 2, 0, 2),
                (1, 1, 2, 0, 3),
                (3, 2, 3, 1, 1),
                },
            PassthroughPrototileAdjacencies = new[]
            {
                (2, 0, 3, 1),
                (2, 1, 3, 2),
                (3, 0, 2, 0),
                (3, 1, 2, 1),
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
                    Polygon(0, 0, 1, 0, 1 + Cos144, Sin144, Cos144, Sin144),
                }
        };

        public static Prototile[] Prototiles =
        {
            Fat,
            Thin,
		};
	}
}

