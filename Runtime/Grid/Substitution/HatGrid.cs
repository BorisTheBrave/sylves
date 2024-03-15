using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


namespace Sylves
{
    // 
    /// <summary>
    /// Implementation of the Hat grid, following https://rdivyanshu.github.io/hat.html and https://www.chiark.greenend.org.uk/~sgtatham/quasiblog/aperiodic-tilings/
    /// It's broken, and fundamentally will never work as i wanted it to.
    /// 
    /// most substitution tilings work by taking a finite set of tilings, and subdividing each tile according to a specific rule for that tile. 
    /// Then you get a set of smaller tiles that are each a scaled copy of one of the original tiles. so you can apply the subdivision again and again
    /// The hat tiling rules fundamentally do not work this way.
    /// If you follow the correct subdivisions, you get a set of tiles that are not similar to the original. The aspect ratio changes every iteration. 
    /// You are meant to do something smart to deal with this.
    /// 
    /// As an aside, the paper authors note that the aspect ratio eventually converges, so after enough iteratios, you start to approximate a normal substitution tiling. 
    /// That's what I implemented. But it only describes the long term behaviour of the actual scheme - it'll never help you place specific hat tiles in the correct place.
    /// </summary>
    internal class HatGrid : SubstitutionTilingGrid
    {
        // With a lot of inspiration from https://rdivyanshu.github.io/hat.html
        // These strings encode a series of relatives steps. The letter gives the edge type, and the number the rotation counter clockwise from right.
        public const string Hat = "(T1 -1) (T1 1) (T2 4) (T2 6) (T1 3) (T1 5) (T2 8) (T2 6) (T1 9) (T1 7) (T2 10) (T3 12) (T2 14)";
        public const string FlippedHat = "(T2 -2) (T3 0) (T2 2) (T1 5) (T1 3) (T2 6) (T2 4) (T1 7) (T1 9) (T2 6) (T2 8) (T1 11) (T1 1)";

        public static float HatLen(string step)
        {

            return step[1] == '1' ? Mathf.Sqrt(3) / 2
                : step[1] == '2' ? 0.5f
                : step[1] == '3' ? 1f
                : throw new Exception();
        }

        public static List<Vector3> HatToPoints(string s, bool skipLast = false)
        {
            var result = new List<Vector3>();
            var current = new Vector3(0, 0, 0);
            result.Add(current);
            foreach (Match match in Regex.Matches(s, @"\(\s*(\w[1-3]?)\s+(-?\d*)\)"))
            {
                var step = match.Groups[1].Value;
                var turn = int.Parse(match.Groups[2].Value);
                var stepLen = HatLen(step);
                var angle = Mathf.PI / 6 * turn;
                var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                current += dir * stepLen;
                result.Add(current);
            }
            if (skipLast)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }

        public static readonly List<Vector3> HatPoints = HatToPoints(Hat);
        public static readonly List<Vector3> FlippedHatPoints = HatToPoints(FlippedHat);

        public const string MetaH = "(X+ 0)  (B- 1) (X- 1) (X+ 2) (B- 3) (X- 3) (X+ 4) (A+ 5) (X- 5)";
        public const string MetaT = "(A- 0) (A- 2) (B+ 4)";
        public const string MetaP = "(X+ 0) (A- 0) ( L 2) (X- 2) (X+ 3) (B+ 3) ( L 5) (X- 5)";
        public const string MetaF = "(X+ 0) (L 0) (X- 0) (F+ 1) (F- 2) (X+ 3) (B+ 3) ( L 5) (X- 5)";

        public static readonly float Phi = (float)(1 + Math.Sqrt(5)) / 2;

        public static float Len(string step)
        {

            return step[0] == 'A' || step[0] == 'B' ? 3 * Phi 
                : step[0] == 'L' ?  2 * Phi
                : step[0] == 'X' ? 1
                : step[0] == 'F' ? 2.28824935959322f // algebraic value?
                : throw new Exception();
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
                var angle = Mathf.PI / 3 * turn;
                if(step == "F+" || step == "F-")
                {
                    angle += 0.388140787472124f;
                }
                var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
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
            var transform = Matrix4x4.Scale(Vector3.one / Phi / Phi) * Matrix4x4.Translate(translate) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, 60 * turn));
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
            MakeChild("H", -1, "(X+ 3) (F+ 2) (X+ 0) (L 0) (X+ 0) (X+ 5) (X+ 1) (X+ 2)" /*"(X- 2)"*/),
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

        public static Vector3[] ChildHat(string offset, int turn, bool flipped, int shift)
        {
            var offsetVector = ToPoint(offset);
            var rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 30 * turn));
            var basePts = (flipped ? FlippedHatPoints : HatPoints);
            shift = (shift % basePts.Count);
            var pts = basePts.Select(x => offsetVector + /*1.45f **/ rot.MultiplyVector(x - basePts[shift]));
            return pts.ToArray();
        }

        public static Prototile[] Prototiles2 =
        {
            new Prototile
            {
                Name = "H",
                ChildPrototiles = HChildren,
                ChildTiles = new[]{ 
                    ChildHat("", -2, false, 1),
                    ChildHat("(X+ 0) (B- 1)", 2, false, 12),
                    ChildHat("(X+ 2) (A- 2)", 2, false, 7),
                    ChildHat("(X+ 2) (L 2) (L 1)", 4, true, 6),
                },
            },
            new Prototile
            {
                Name = "T",
                ChildPrototiles = TChildren,
                ChildTiles = new[]{ 
                    ChildHat("", 0, false, 11),
                },
            },
            new Prototile
            {
                Name = "P",
                ChildPrototiles = PChildren,
                ChildTiles = new[]{ 
                    ChildHat("", -2, false, 1),
                    ChildHat("(X- 0)", 0, false, 11),
                },
            },
            new Prototile
            {
                Name = "F",
                ChildPrototiles = FChildren,
                ChildTiles = new[]{ 
                    ChildHat("", -2, false, 1),
                    ChildHat("(X- 0)", 0, false, 11),
                },
            },
        };

        public HatGrid(SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : base(Prototiles, new[] {"H"}, bound, cachePolicy)
        {
        }
    }
}
