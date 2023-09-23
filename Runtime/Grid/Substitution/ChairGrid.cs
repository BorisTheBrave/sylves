using System.Collections.Generic;
using UnityEngine;

namespace Sylves
{
    public class ChairGrid : SubstitutionTilingGrid
	{
        public ChairGrid(SubstitutionTilingBound bound = null) : base(Prototiles, new[] { "1", "2" }, bound)
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

        /// <summary>
		/// shape   child index     edge number
        ///  ___                         5
        /// |  _|         3                4
        /// |_| |__         2         6      3
        /// | |__| |                            2
        /// |___|__|      0   1       7
        ///                              0   1   
        /// </summary>
		private static Prototile Chair1 = new Prototile
        {
            Name = "1",
            ChildPrototiles = new[]
                {
                    (ScaleRotateAndTranslate(0.5f, 0, -0.5f, -0.5f), "2"),
                    (ScaleRotateAndTranslate(0.5f, 90, 0.5f, -0.5f), "2"),
                    (ScaleRotateAndTranslate(0.5f, 0, 0, 0), "2"),
                    (ScaleRotateAndTranslate(0.5f, 270, -0.5f, 0.5f), "2"),
                },
            InteriorPrototileAdjacencies = new[]
                {
                    (0, 2, 1, 5),
                    (0, 3, 2, 0),
                    (0, 4, 2, 7),
                    (0, 5, 3, 2),
                    (1, 3, 2, 2),
                    (1, 4, 2, 1),
                    (1, 5, 0, 2),
                    (2, 0, 0, 3),
                    (2, 1, 1, 4),
                    (2, 2, 1, 3),
                    (2, 5, 3, 4),
                    (2, 6, 3, 3),
                    (2, 7, 0, 4),
                    (3, 2, 0, 5),
                    (3, 3, 2, 6),
                    (3, 4, 2, 5),
                },
            ExteriorPrototileAdjacencies = new[]
                {
                    (0, 0, 2, 0, 0),
                    (0, 1, 2, 0, 1),
                    (1, 0, 2, 1, 6),
                    (1, 1, 2, 1, 7),
                    (2, 0, 2, 1, 0),
                    (2, 1, 2, 1, 1),
                    (3, 0, 2, 1, 2),
                    (3, 1, 2, 2, 3),
                    (4, 0, 2, 2, 4),
                    (4, 1, 2, 3, 5),
                    (5, 0, 2, 3, 6),
                    (5, 1, 2, 3, 7),
                    (6, 0, 2, 3, 0),
                    (6, 1, 2, 3, 1),
                    (7, 0, 2, 0, 6),
                    (7, 1, 2, 0, 7),
                },
            ChildTiles = new[]
                {
                    Polygon(-1, -1, 0, -1, 1, -1, 1, 0, 0, 0, 0, 1, -1, 1, -1, 0),
                }
        }.CopyPrototileToTiles();

        public static Prototile[] Prototiles =
        {
            Chair1,
            Chair1
                .Rename("2")
                .RenameChildren(new Dictionary<string, string>{ { "2", "1" } })
                .SwapChildren(0, 2),
		};
	}
}

