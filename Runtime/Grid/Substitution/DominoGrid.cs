using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    // https://tilings.math.uni-bielefeld.de/substitution/domino/
    public class DominoGrid : SubstitutionTilingGrid
	{
        public DominoGrid(SubstitutionTilingBound bound = null):base(Prototiles, new[] { "L", "R" }, bound)
        {

        }

		private static Matrix4x4 ScaleAndTranslate(float scale, float x, float y)
		{
			return Matrix4x4.Translate(new Vector3(x, y, 0)) * Matrix4x4.Scale(new Vector3(scale, scale, scale));
		}
        private static Matrix4x4 ScaleRotateAndTranslate(float scale, float angle, float x, float y)
        {
            return Matrix4x4.Translate(new Vector3(x, y, 0)) * Matrix4x4.Rotate(Quaternion.AngleAxis(angle, Vector3.forward)) * Matrix4x4.Scale(new Vector3(scale, scale, scale));
        }
        private static float R90 = 90;
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

        /// <summary>
		/// shape   child index     edge number
        ///  ___                         2
        /// |___|       3
        /// | | |      0 1            3     1
        /// |_|_|                     4     0
		/// |___|       2
		///                              5
		/// width 2, height 4
        /// </summary>
		private static Prototile L = new Prototile
        {
            Name = "L",
            ChildPrototiles = new[]
                {
                    (ScaleRotateAndTranslate(0.5f, 0, -0.5f, 0), "R"),
                    (ScaleRotateAndTranslate(0.5f, 0, 0.5f, 0), "R"),
                    (ScaleRotateAndTranslate(0.5f, R90, 0, -1.5f), "R"),
                    (ScaleRotateAndTranslate(0.5f, R90, 0, 1.5f), "R"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 1, 4),
                    (0, 1, 1, 3),
                    (0, 2, 3, 0),
                    (0, 5, 2, 4),
                    (1, 2, 3, 1),
                    (1, 3, 0, 1),
                    (1, 4, 0, 0),
                    (1, 5, 2, 3),
                    (2, 3, 1, 5),
                    (2, 4, 0, 5),
                    (3, 0, 0, 2),
                    (3, 1, 1, 2),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 2, 2, 2),
                    (0, 1, 2, 1, 0),
                    (1, 0, 2, 1, 1),
                    (1, 1, 2, 3, 2),
                    (2, 0, 2, 3, 3),
                    (2, 1, 2, 3, 4),
                    (3, 0, 2, 3, 5),
                    (3, 1, 2, 0, 3),
                    (4, 0, 2, 0, 4),
                    (4, 1, 2, 2, 5),
                    (5, 0, 2, 2, 0),
                    (5, 1, 2, 2, 1),
                },
            ChildTiles = new[]
                {
                    Polygon(0, -1, 0, 0, 0, 1, -1, 1, -1, 0, -1, -1),
                    Polygon(1, -1, 1, 0, 1, 1, 0, 1, 0, 0, 0, -1),
                    Polygon(-1, -2, 0, -2, 1, -2, 1, -1, 0, -1, -1, -1),
                    Polygon(-1, 1, 0, 1, 1, 1, 1, 2, 0, 2, -1, 2),
                }
        }.CopyPrototileToTiles();

        public static Prototile[] Prototiles =
        {
            L,
            L
                .Rename("R")
                .RenameChildren(new Dictionary<string, string>{ { "R", "L" } })
                .SwapChildren(0, 1),
		};
	}
}

