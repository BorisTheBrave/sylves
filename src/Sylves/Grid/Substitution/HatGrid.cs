using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sylves
{
    internal class HatGrid : SubstitutionTilingGrid
    {
        // With a lot of inspiration from https://rdivyanshu.github.io/hat.html
        public const string MetaH = "(X+ 0)  (B- 1) (X- 1) (X+ 2) (B- 3) (X- 3) (X+ 4) (A+ 5) (X- 5)";
        public const string MetaT = "(A- 0) (A- 2) (B+ 4)";
        public const string MetaP = "(X+ 0) (A- 0) ( L 2) (X- 2) (X+ 3) (B+ 3) ( L 5) (X- 5)";
        public const string MetaF = "(X+ 0) (L 0) (X- 0) (F+ 1) (F- 2) (X+ 3) (B+ 3) ( L 5) (X- 5)";

        public static readonly float Phi = (float)(1 + Math.Sqrt(5)) / 2;

        public static float Len(string step)
        {

            return step[0] == 'A' || step[0] == 'B' ? 12
                : 4;
            /*
            return step[0] == 'A' || step[0] == 'B' ? 12 * Phi 
                : step[0] == 'L' ?  8 * Phi :
                : 4;
            */
        }

        public static List<Vector3> ToPoints(string s, bool skipLast = false)
        {
            var result = new List<Vector3>();
            var current = new Vector3(0, 0, 0);
            result.Add(current);
            foreach (Match match in Regex.Matches(s, @"\(\s*(\w[+-]?)\s+(-?\d*)\)"))
            {
                var step = match.Groups[1].Value;
                var turn = int.Parse(match.Groups[2].Value);
                var stepLen = Len(step);
                var dir = new Vector3(Mathf.Cos(Mathf.PI / 3 * turn), Mathf.Sin(Mathf.PI / 3 * turn), 0);
                current += dir * stepLen;
                result.Add(current);
            }
            if (skipLast)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }

        public static Vector3 ToPoint(string s) => ToPoints(s).Last();

        public static (Matrix4x4 transform, string childName) MakeChild(string tile, int turn, string offset)
        {
            var translate = ToPoint(offset);
            var transform = Matrix4x4.Translate(translate) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 60 * turn)) * Matrix4x4.Scale(Vector3.one);
            return (transform, tile);
        }
            

        public static (Matrix4x4 transform, string childName)[] HChildren = new[] {
            MakeChild("H",  0, "(F- 1) (X+ 2) (B+ 2) (X- 1)"),
            MakeChild("H", -2, "(F- 1) (X+ 2) (B+ 2) (X- 1)"),
            MakeChild("H",  0, "(F- 1) (X+ 0) (B- 1) (X- 1)"),
            MakeChild("T",  0, "(F- 1) (X+ 2) (B+ 2) (X- 1) (X+ 0)"),
            MakeChild("F", -1, "(F- 3) (X+ 2) (L 2) (X- 2)"),
            MakeChild("F",  1, "(F- 1) (X+ 0) (B- 1) (X- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("F",  3, "(F- 1) (X+ 2) (B+ 2) (X- 1) (X+ 0) (B- 1) (X- 1) (X+ 2) (L 2) (X- 2)"),
            MakeChild("P",  2, "(F- 1) (X+ 2) (B+ 2) (X- 1)"),
            MakeChild("P",  1, "(F- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("P",  3, "(F- 1) (X+ 0) (B- 1) (X- 1) (X+ 0) (B- 1) (X- 1) (X+ 2) (L 2) (X- 2)"),
        };

        public static (Matrix4x4 transform, string childName)[] TChildren = new[] {
            MakeChild("H", -1, "(X- 2)"),
        };

        public static (Matrix4x4 transform, string childName)[] PChildren = new[] {
            MakeChild("P", 1, "(F- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("H", 5, "(F- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("H", 4, "(F- 1) (X+ 2) (B+ 2) (X- 1)"),
            MakeChild("F", 5, "(F- 3) (X+ 2) (L 2) (X- 2)"),
            MakeChild("F", 2, "(F- 1) (X+ 0) (L 0) (X- 0) (X+ -1) (B- 0) (X- 0) (X+ 1) (L 1) (X- 1)))"),
        };
        public static (Matrix4x4 transform, string childName)[] FChildren = new[] {
            MakeChild("P", 1, "(F- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("H", 5, "(F- 1) (X+ 0) (L 0) (X- 0)"),
            MakeChild("H", 4, "(F- 1) (X+ 2) (B+ 2) (X- 1)"),
            MakeChild("F", 5, "(F- 3) (X+ 2) (L 2) (X- 2)"),
            MakeChild("F", 2, "(F- 1) (X+ 0) (L 0) (X- 0) (X+ -1) (B- 0) (X- 0) (X+ 1) (L 1) (X- 1)"),
            MakeChild("F", 0, "(F- 1) (X+ 0) (L 0) (X- 0) (X+ -1) (L -1) (X- -1)))"),
        };

        public static Prototile[] Prototiles =
        {
            new Prototile
            {
                Name = "H",
                ChildPrototiles = HChildren,
                ChildTiles = new[]{ ToPoints(MetaH, true).ToArray() },
            },
            new Prototile
            {
                Name = "T",
                ChildPrototiles = TChildren,
                ChildTiles = new[]{ ToPoints(MetaT, true).ToArray() },
            },
            new Prototile
            {
                Name = "P",
                ChildPrototiles = PChildren,
                ChildTiles = new[]{ ToPoints(MetaP, true).ToArray() },
            },
            new Prototile
            {
                Name = "F",
                ChildPrototiles = FChildren,
                ChildTiles = new[]{ ToPoints(MetaF, true).ToArray() },
            },
        };

        public HatGrid(SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : base(Prototiles, new[] {"H"}, bound, cachePolicy)
        {
        }
    }
}
