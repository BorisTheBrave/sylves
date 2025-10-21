using System;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    internal static class HalfPlaneUtils
    {
        /// <summary>
        /// Finds all the points that are in area, but are not in area translated by offset.
        /// Returns as a set of lines along the lattice.
        /// </summary>
        public static List<LatticeLine> Subtract(List<HalfPlane> area, Vector2Int offset)
        {
            var result = new List<LatticeLine>();
            var seen = new HashSet<(int a, int b, int s)>();

            for (var i = 0; i < area.Count; i++)
            {
                var hp = area[i];
                var a = hp.A;
                var b = hp.B;
                var c = hp.C;

                // Parallel shift amount along the normal
                var delta = a * offset.x + b * offset.y;
                if (delta <= 0)
                    continue; // No strip contributed by this half-plane

                // For points in Original \ Shifted, along this constraint we need:
                // a x + b y >= -c (inside original)
                // a x + b y < -(c - delta)  => a x + b y <= -(c - delta) - 1
                // Let s = a x + b y. Then s ranges over integers in [-c, -(c - delta) - 1] inclusive
                var sMin = -c;
                var sMax = -(c - delta) - 1;

                // Direction vector along the lattice for lines a x + b y = s
                var g = Gcd(Math.Abs(a), Math.Abs(b));
                if (g == 0)
                    continue; // Degenerate constraint, skip
                var dir = new Vector2Int(b / g, -a / g);

                for (int s = sMin; s <= sMax; s++)
                {
                    // Only lines with integer solutions are relevant: require g | s
                    if (s % g != 0)
                        continue;

                    // Normalize key by primitive (a', b', s') to dedupe lines coming from duplicate/scaled planes
                    var an = a / g;
                    var bn = b / g;
                    var sn = s / g;
                    var key = (an, bn, sn);
                    if (seen.Contains(key))
                        continue;

                    if (!TrySolveLinearDiophantine(a, b, s, out var x0, out var y0))
                        continue;

                    int? tMin = null;
                    int? tMax = null;

                    // Clip along direction by all original half-planes
                    for (var j = 0; j < area.Count; j++)
                    {
                        ClipByHalfPlane(area[j], new Vector2Int(x0, y0), dir, ref tMin, ref tMax);
                        if (tMin.HasValue && tMax.HasValue && tMin.Value > tMax.Value)
                            break;
                    }

                    // If empty after clipping, skip
                    if (tMin.HasValue && tMax.HasValue && tMin.Value > tMax.Value)
                        continue;


					// Ensure partition: subtract overlaps with all previously added lines, possibly splitting
					var pending = new List<LatticeLine>
					{
						new LatticeLine{ Origin = new Vector2Int(x0, y0), Direction = dir, TMin = tMin, TMax = tMax }
					};
					for (int r = 0; r < result.Count && pending.Count > 0; r++)
					{
						var existing = result[r];
						var nextPending = new List<LatticeLine>();
						foreach (var seg in pending)
						{
							SplitSubtractOverlap(seg, existing, nextPending);
						}
						pending = nextPending;
					}

					result.AddRange(pending);

                    seen.Add(key);
                }
            }

            return result;
        }

		private static void SplitSubtractOverlap(LatticeLine candidate, LatticeLine existing, List<LatticeLine> output)
		{
			// If colinear, subtract interval overlap
			if (TryColinearShift(candidate.Origin, candidate.Direction, existing.Origin, out var shift))
			{
				var overlapMin = AddNullable(existing.TMin, shift);
				var overlapMax = AddNullable(existing.TMax, shift);
				SubtractInterval(candidate.TMin, candidate.TMax, overlapMin, overlapMax, candidate.Origin, candidate.Direction, output);
				return;
			}

			// Otherwise, at most a single lattice point intersection
			if (TrySolveIntersection(candidate.Origin, candidate.Direction, existing.Origin, existing.Direction, out var tIntersect, out var uIntersect))
			{
				// Check bounds
				if (InBounds(tIntersect, candidate.TMin, candidate.TMax) && InBounds(uIntersect, existing.TMin, existing.TMax))
				{
					// Remove single point tIntersect from candidate, splitting if necessary
					SubtractSinglePoint(candidate.TMin, candidate.TMax, tIntersect, candidate.Origin, candidate.Direction, output);
					return;
				}
			}

			// No overlap
			output.Add(candidate);
		}

		private static bool TryColinearShift(Vector2Int originA, Vector2Int dir, Vector2Int originB, out int shift)
		{
			shift = 0;
			var dx = dir.x;
			var dy = dir.y;
			var delta = new Vector2Int(originB.x - originA.x, originB.y - originA.y);
			// Parallel test: cross(dir, delta) == 0
			if (dx * delta.y - dy * delta.x != 0)
				return false;
			if (dx != 0)
			{
				if (delta.x % dx != 0) return false;
				var k = delta.x / dx;
				if (originA.y + k * dy != originB.y) return false;
				shift = k;
				return true;
			}
			else if (dy != 0)
			{
				if (delta.y % dy != 0) return false;
				var k = delta.y / dy;
				if (originA.x + k * dx != originB.x) return false;
				shift = k;
				return true;
			}
			return false;
		}

		private static bool TrySolveIntersection(Vector2Int o1, Vector2Int d1, Vector2Int o2, Vector2Int d2, out int t, out int u)
		{
			t = 0; u = 0;
			var D = d1.x * d2.y - d1.y * d2.x;
			if (D == 0)
				return false;
			var deltaX = o2.x - o1.x;
			var deltaY = o2.y - o1.y;
			var numT = deltaX * d2.y - deltaY * d2.x;
			var numU = deltaX * d1.y - deltaY * d1.x;
			if (numT % D != 0 || numU % D != 0)
				return false;
			t = numT / D;
			u = numU / D;
			return true;
		}

		private static bool InBounds(int value, int? min, int? max)
		{
			if (min.HasValue && value < min.Value) return false;
			if (max.HasValue && value > max.Value) return false;
			return true;
		}

		private static int? AddNullable(int? a, int b)
		{
			return a.HasValue ? a.Value + b : (int?)null;
		}

		private static void SubtractInterval(int? aMin, int? aMax, int? bMin, int? bMax, Vector2Int origin, Vector2Int dir, List<LatticeLine> output)
		{
			// If B is empty, nothing to subtract
			if (bMin.HasValue && bMax.HasValue && bMin.Value > bMax.Value)
			{
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = aMin, TMax = aMax });
				return;
			}

			// Compute intersection I = A ∩ B
			var iMin = MaxNullable(aMin, bMin);
			var iMax = MinNullable(aMax, bMax);
			if (iMin.HasValue && iMax.HasValue && iMin.Value > iMax.Value)
			{
				// Disjoint
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = aMin, TMax = aMax });
				return;
			}

			// Subtract I from A, may produce up to two segments
			var leftMin = aMin;
			var leftMax = DecrementNullable(iMin);
			if (!IsEmptyInterval(leftMin, leftMax))
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = leftMin, TMax = leftMax });

			var rightMin = IncrementNullable(iMax);
			var rightMax = aMax;
			if (!IsEmptyInterval(rightMin, rightMax))
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = rightMin, TMax = rightMax });
		}

		private static void SubtractSinglePoint(int? aMin, int? aMax, int point, Vector2Int origin, Vector2Int dir, List<LatticeLine> output)
		{
			// If point is outside A, keep A
			if (!InBounds(point, aMin, aMax))
			{
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = aMin, TMax = aMax });
				return;
			}
			var leftMin = aMin;
			var leftMax = DecrementNullable(point);
			if (!IsEmptyInterval(leftMin, leftMax))
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = leftMin, TMax = leftMax });

			var rightMin = IncrementNullable(point);
			var rightMax = aMax;
			if (!IsEmptyInterval(rightMin, rightMax))
				output.Add(new LatticeLine { Origin = origin, Direction = dir, TMin = rightMin, TMax = rightMax });
		}

		private static int? MaxNullable(int? a, int? b)
		{
			if (!a.HasValue) return b;
			if (!b.HasValue) return a;
			return Math.Max(a.Value, b.Value);
		}

		private static int? MinNullable(int? a, int? b)
		{
			if (!a.HasValue) return b;
			if (!b.HasValue) return a;
			return Math.Min(a.Value, b.Value);
		}

		private static int? DecrementNullable(int? a)
		{
			if (!a.HasValue) return null;
			return a.Value - 1;
		}

		private static int? IncrementNullable(int? a)
		{
			if (!a.HasValue) return null;
			return a.Value + 1;
		}

		private static bool IsEmptyInterval(int? min, int? max)
		{
			if (min.HasValue && max.HasValue && min.Value > max.Value) return true;
			return false;
		}

        private static void ClipByHalfPlane(HalfPlane hp, Vector2Int origin, Vector2Int dir, ref int? tMin, ref int? tMax)
        {
            var aj = hp.A;
            var bj = hp.B;
            var cj = hp.C;

            var rate = aj * dir.x + bj * dir.y; // change in (aj x + bj y) per step t
            var rhs = -cj - aj * origin.x - bj * origin.y;

            if (rate == 0)
            {
                // Constraint constant along the line; require it to be satisfied at all t
                if (rhs > 0)
                {
                    // No t satisfies
                    tMin = 1; tMax = 0; // empty interval marker
                }
                return;
            }

            if (rate > 0)
            {
                var bound = CeilDiv(rhs, rate);
                tMin = tMin.HasValue ? Math.Max(tMin.Value, bound) : bound;
            }
            else // rate < 0
            {
                var bound = FloorDiv(rhs, rate);
                tMax = tMax.HasValue ? Math.Min(tMax.Value, bound) : bound;
            }
        }

        // Greatest common divisor (non-negative)
        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                var t = a % b;
                a = b;
                b = t;
            }
            return Math.Abs(a);
        }

        // Solve a x + b y = s for integers. Returns one solution (x0, y0) if exists.
        private static bool TrySolveLinearDiophantine(int a, int b, int s, out int x0, out int y0)
        {
            x0 = 0; y0 = 0;
            if (a == 0 && b == 0)
                return s == 0;
            int x, y;
            var g = ExtendedGcd(Math.Abs(a), Math.Abs(b), out x, out y);
            if (s % g != 0)
                return false;
            long mul = s / g;
            long xx = x * mul;
            long yy = y * mul;
            if (a < 0) xx = -xx;
            if (b < 0) yy = -yy;
            x0 = (int)xx;
            y0 = (int)yy;
            return true;
        }

        // Returns g = gcd(a,b) and finds x,y such that a*x + b*y = g (a,b >= 0)
        private static int ExtendedGcd(int a, int b, out int x, out int y)
        {
            if (b == 0)
            {
                x = 1; y = 0; return a;
            }
            int x1, y1;
            var g = ExtendedGcd(b, a % b, out x1, out y1);
            x = y1;
            y = x1 - (a / b) * y1;
            return g;
        }

        private static int FloorDiv(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            int q = a / b;
            int r = a % b;
            if (r != 0 && ((r > 0) != (b > 0))) q--;
            return q;
        }

        private static int CeilDiv(int a, int b)
        {
            if (b == 0) throw new DivideByZeroException();
            int q = a / b;
            int r = a % b;
            if (r != 0 && ((r > 0) == (b > 0))) q++;
            return q;
        }
    }
}
