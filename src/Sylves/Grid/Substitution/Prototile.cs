using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    public class Prototile
	{
		public string Name { get; set; }

		public (Matrix4x4 transform, string childName)[] ChildPrototiles { get; set; }

		public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorPrototileAdjacencies { get; set; }

		public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorPrototileAdjacencies { get; set; }
		public (int fromParentSide, int fromParentSubSide, int toParentSide, int toParentSubSide)[] PassthroughPrototileAdjacencies { get; set; }


		public Vector3[][] ChildTiles { get; set; }

        public (int fromChild, int fromChildSide, int toChild, int toChildSide)[] InteriorTileAdjacencies { get; set; }

        public (int parentSide, int parentSubSide, int parentSubSideCount, int child, int childSide)[] ExteriorTileAdjacencies { get; set; }

		public Prototile CopyPrototileToTiles()
		{
			var r = Clone();
			r.InteriorTileAdjacencies = r.InteriorPrototileAdjacencies;
			r.ExteriorTileAdjacencies = r.ExteriorPrototileAdjacencies;
			return r;
		}

        public Prototile HasSingleTile()
        {
            var r = Clone();
			r.InteriorTileAdjacencies = new (int fromChild, int fromChildSide, int toChild, int toChildSide)[0];
            r.ExteriorTileAdjacencies = Enumerable.Range(0, ChildTiles[0].Length).Select(x => (x, 0, 1, 0, x)).ToArray();
            return r;
        }

        public Prototile RenameChildren(Dictionary<string, string> renames)
		{
			var r = Clone();
			r.ChildPrototiles = ChildPrototiles
				.Select(t => (t.transform, renames[t.childName]))
				.ToArray();
			return r;
		}

        public Prototile SwapChildren(int a, int b)
        {
            int Update(int c) => c == a ? b : c == b ? a : c;
            var r = Clone();
            r.ChildPrototiles = ((Matrix4x4, string)[])r.ChildPrototiles.Clone();
            r.ChildPrototiles[a] = ChildPrototiles[b];
            r.ChildPrototiles[b] = ChildPrototiles[a];
            r.InteriorPrototileAdjacencies = InteriorPrototileAdjacencies?.Select(t =>
            (Update(t.fromChild), t.fromChildSide, Update(t.toChild), t.toChildSide)).ToArray();
            r.ExteriorPrototileAdjacencies = ExteriorPrototileAdjacencies?.Select(t =>
            (t.parentSide, t.parentSubSide, t.parentSubSideCount, Update(t.child), t.childSide)).ToArray();
            return r;
        }

		public Prototile Rename(string name)
		{
			var r = Clone();
			r.Name = name;
			return r;
		}

		public Prototile Clone()
		{
			return (Prototile)MemberwiseClone();
		}

		// Mirrors the prototile in the x-axis.
		// Does not reflect the child transforms.
		// You'll want to rename things after doing this.
		// Note that this re-orders the ChildTiles to keep winding order counterclockwise.
		// But it doesn't re-order parentside.
		public Prototile Mirror()
		{
			var r = Clone();
			var m = Matrix4x4.Scale(new Vector3(-1, 1, 1));
			r.ChildTiles = ChildTiles.Select(t => t.Select(m.MultiplyVector).Reverse().ToArray()).ToArray();
			r.ChildPrototiles = ChildPrototiles.Select(t => (m * t.transform * m, t.childName)).ToArray();
			r.InteriorTileAdjacencies = InteriorTileAdjacencies.Select(t => (t.fromChild, ChildTiles[t.fromChild].Length - t.fromChildSide - 1, t.toChild, ChildTiles[t.toChild].Length - t.toChildSide - 1)).ToArray();
			r.ExteriorTileAdjacencies = ExteriorTileAdjacencies.Select(t => (t.parentSide, t.parentSubSide, t.parentSubSideCount, t.child, ChildTiles[t.child].Length - t.childSide - 1)).ToArray();
            return r;
		}

        public override string ToString() => Name;
    }
}

