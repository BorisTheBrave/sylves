using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // https://tilings.math.uni-bielefeld.de/substitution/pinwheel/
    // https://pages.vassar.edu/nafrank/files/2012/08/substitutions.pdf has a better picture of the substitution rules
    public class PinwheelGrid : SubstitutionTilingGrid
	{
        public PinwheelGrid(SubstitutionTilingBound bound = null):base(Prototiles, new[] { "Pinwheel", "Pinwheel2" }, bound)
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


        private static float Sqrt5 = Mathf.Sqrt(5);
        private static float Angle1 = 180 / Mathf.PI * Mathf.Atan2(1, 2);
        private static float Inflation = Sqrt5;
        private static float Deflation = 1 / Inflation;

        private static Prototile Pinwheel = new Prototile
        {
            Name = "Pinwheel",
            ChildPrototiles = new[]
                {
                (ScaleRotateAndTranslate(Deflation, 180 + Angle1, 0, 0), "Pinwheel2"),
                (ScaleRotateAndTranslate(Deflation, 180 + Angle1, 1, 0), "Pinwheel2"),
                (ScaleRotateAndTranslate(Deflation, -90+Angle1, 2, 0), "Pinwheel2"),
                (ScaleRotateAndTranslate(Deflation, Angle1, 1, 0), "Pinwheel"),
                (ScaleRotateAndTranslate(Deflation, 180 + Angle1, 1.6f, 0.8f), "Pinwheel"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (2, 6, 1, 4),
                    (3, 0, 1, 6),
                    (3, 1, 1, 5),
                    (3, 2, 2, 5),
                    (4, 2, 0, 4),
                    (4, 3, 3, 7),
                    (4, 4, 3, 6),
                    (4, 5, 3, 5),
                    (4, 6, 3, 4),
                    (4, 7, 3, 3),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 5, 0, 7),
                    (0, 1, 5, 0, 0),
                    (0, 2, 5, 0, 1),
                    (0, 3, 5, 0, 2),
                    (0, 4, 5, 0, 3),
                    (1, 0, 5, 1, 7),
                    (1, 1, 5, 1, 0),
                    (1, 2, 5, 1, 1),
                    (1, 3, 5, 1, 2),
                    (1, 4, 5, 1, 3),
                    (2, 0, 5, 2, 7),
                    (2, 1, 5, 2, 0),
                    (2, 2, 5, 2, 1),
                    (2, 3, 5, 2, 2),
                    (2, 4, 5, 2, 3),
                    (3, 0, 1, 2, 4),
                    (4, 0, 1, 4, 0),
                    (5, 0, 1, 4, 1),
                    (6, 0, 1, 0, 5),
                    (7, 0, 1, 0, 6),
                },
            ChildTiles = new[]
                {
                    // Have to add a lot of sides to get edge-to-edge to work.
                    //Polygon(0, 0, 2, 0, 2, 1),
                    Polygon(0, 0, 1, 0, 2, 0, 2, 1, 1.6f, 0.8f, 1.2f, 0.6f, 0.8f, 0.4f, 0.4f, 0.2f),
                }
        };

        private static Prototile Pinwheel2 = Pinwheel
            .HasSingleTile()
            .Mirror()
            .Rename("Pinwheel2")
            .RenameChildren(new Dictionary<string, string> { { "Pinwheel", "Pinwheel2"}, { "Pinwheel2", "Pinwheel"} })
            .SwapChildren(0, 2);

        public static Prototile[] Prototiles =
        {
            Pinwheel,
            Pinwheel2,
		};
	}
}

