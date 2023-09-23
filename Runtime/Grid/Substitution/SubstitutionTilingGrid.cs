using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Sylves
{
    /// <summary>
    /// Creates a tiling of the 2d plane from a set of substitution rules.
    /// It is quite flexible:
    /// * imperfect substitution rules where the replacement outline doesn't follow the original outline
    /// * tiles can freely transformed
    ///   * tile equivalence under translation, euclidian motion, isometry, similarity all supported
    ///   * "statistically round" substitutions like the pinwheel substitution supported
    /// </summary>
    public class SubstitutionTilingGrid : BaseSubstitutionTilingGrid
    {
        private ICachePolicy cachePolicy;
        private Dictionary<(int Height, Cell Path), Crumb> crumbCache;
        private List<Crumb> hierarchyCrumbs;


        private SubstitutionTilingGrid(SubstitutionTilingGrid other, SubstitutionTilingBound bound) : base(other, bound)
        {
            // Can share cache - bounds do not change anything fundamental
            cachePolicy = other.cachePolicy;
            crumbCache = other.crumbCache;
            hierarchyCrumbs = other.hierarchyCrumbs;
        }

        private SubstitutionTilingGrid(SubstitutionTilingGrid other, Func<int, InternalPrototile> hierarchy, Matrix4x4 baseTransform) : base(other, hierarchy, baseTransform)
        {
            cachePolicy = other.cachePolicy;
            crumbCache = new Dictionary<(int Height, Cell Path), Crumb>();
            hierarchyCrumbs = new List<Crumb>
            {
                new Crumb
                {
                    parent = null,
                    height = 0,
                    partialPath = new Cell(),
                    transform = baseTransform,
                    prototile = this.hierarchy(0),
                }
            };
        }

        public SubstitutionTilingGrid(Prototile[] prototiles, string[] hierarchy, SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : this(prototiles, i => hierarchy[i % hierarchy.Length], bound, cachePolicy)
        {
        }

        public SubstitutionTilingGrid(Prototile[] prototiles, Func<int, string> hierarchy, SubstitutionTilingBound bound = null, ICachePolicy cachePolicy = null) : base(prototiles, hierarchy, bound)
        {
            this.cachePolicy = cachePolicy ?? CachePolicy.Always;
            crumbCache = new Dictionary<(int Height, Cell Path), Crumb>();
            hierarchyCrumbs = new List<Crumb>
            {
                new Crumb
                {
                    parent = null,
                    height = 0,
                    partialPath = new Cell(),
                    transform = baseTransform,
                    prototile = this.hierarchy(0),
                }
            };
        }

        protected override InternalPrototile GetPrototile(Cell cell, int height) => GetCrumb(cell, height).prototile;


        #region Crumb Utils

        /// <summary>
        /// Crumbs are a node in the partially evaluated tree of nodes.
        /// Crumbs always have their parent defined, unless their parent is
        /// a hierarchy path, then it's optional.
        /// Crumbs do not store children, the circulare reference would interfer with 
        /// garbage collecting unused parts of the tree. Children can be created from scratch
        /// or read out of the cache instead.
        /// </summary>
        private class Crumb
        {
            // Use GetParent, don't read this!
            public Crumb parent;

            public int height;
            public Cell partialPath;

            public Matrix4x4 transform;
            public InternalPrototile prototile;

            public override string ToString() => $"{height}/{partialPath}";
        }


        private void EnsureCached(Crumb crumb)
        {
            crumbCache[(crumb.height, crumb.partialPath)] = crumb;
        }

        // Force creates a crumb which is the ancestor of cell at the given height.
        // Uses the cache if possible
        private Crumb GetCrumb(Cell cell, int height = 0)
        {
            var pathLength = Math.Max(GetPathLength(cell), height);

            // Find the closed crumb we can walk down from
            Crumb crumb = null;
            // TODO: Rather than linear search, could do bisection?
            var partialPath = ClearChildTileAndPathBelow(cell, height);
            var h = height;
            while (h < pathLength)
            {
                if (crumbCache.TryGetValue((h, partialPath), out crumb))
                {
                    break;
                }
                partialPath = SetPathAt(partialPath, h, 0);
                h++;
            }

            // Nothing found, instead create a crumb we can walk down from
            if (crumb == null)
            {
                while(hierarchyCrumbs.Count <= pathLength)
                {
                    hierarchyCrumbs.Add(GetParent(hierarchyCrumbs[hierarchyCrumbs.Count-1]));
                }
                crumb = hierarchyCrumbs[pathLength];
            }

            // Walk down from crumb to desired height
            while (crumb.height > height)
                crumb = GetChild(crumb, GetPathAt(cell, crumb.height - 1));

            return crumb;
        }

        // Gets the crumb that is the parent of the crumb,
        // This is being fast is the main reason to have crumbs.
        private Crumb GetParent(Crumb crumb)
        {
            if (crumb.parent != null)
                return crumb.parent;

            // If we're here, parent is on the hierarchy.
            var parentPrototile = hierarchy(crumb.height + 1);
            var transform = Up(crumb.transform, parentPrototile);
            var parentCrumb = crumb.parent = new Crumb
            {
                parent = null,
                height = crumb.height + 1,
                partialPath = crumb.partialPath,
                transform = transform,
                prototile = parentPrototile,
            };

            EnsureCached(parentCrumb);

            return parentCrumb;
        }

        private Crumb GetChild(Crumb crumb, int childIndex)
        {
            var height = crumb.height - 1;
            var partialPath = SetPathAt(crumb.partialPath, crumb.height - 1, childIndex);
            Crumb child;
            if (crumbCache != null)
            {
                if (crumbCache.TryGetValue((height, partialPath), out child))
                {
                    return child;
                }
            }

            var (transform, prototile) = Down(crumb.transform, crumb.prototile, childIndex);
            child = new Crumb
            {
                parent = crumb,
                height = height,
                partialPath = partialPath,
                transform = transform,
                prototile = prototile,
            };

            EnsureCached(child);
            return child;
        }
        #endregion


        #region Relatives
        public override IGrid Unbounded => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)null);

        public RawSubstitutionTilingGrid Raw => new RawSubstitutionTilingGrid(this, bound);


        public SubstitutionTilingGrid ParentGrid(int n = 1)
        {
            // Check subdividable
            if (tileBits != 0)
                throw new Exception("Parent/subdivision only works on substitution tiling grids with a single tile per prototile");

            var t = baseTransform;
            for (var i = 0; i < n; i++)
            {
                t = Up(t, hierarchy(i + 1));
            }

            return new SubstitutionTilingGrid(this, (h) => hierarchy(h + n), t);
        }

        public Cell CellToParentGrid(Cell cell, int n = 1)
        {
            // TODO: This can be done with bit shifting
            var l = GetPathLength(cell);
            var r = new Cell();
            for (var i = n; i < l; i++)
            {
                r = SetPathAt(r, i - n, GetPathAt(cell, i));
            }
            return r;
        }

        #endregion


        #region CellInfo

        public override ICellType GetCellType(Cell cell)
        {
            if (cellTypes.Count == 1)
                return cellTypes[0];
            var crumb = GetCrumb(cell);
            return NGonCellType.Get(crumb.prototile.ChildTiles[GetChildTileAt(cell)].Length);
        }

        public override bool IsCellInGrid(Cell cell)
        {
            // TODO: Don't use try-catch to validate
            try
            {
                var crumb = GetCrumb(cell);
                var childIndex = GetChildTileAt(cell);
                if (childIndex < 0 || childIndex >= crumb.prototile.ChildTiles.Length)
                    return false;
            }
            catch
            {
                return false;
            }

            if (bound != null)
            {
                return ClearChildTileAndPathBelow(cell, bound.Height) == bound.Path;
            }
            return true;
        }

        #endregion

        #region Topology

        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            var childTile = GetChildTileAt(cell);
            var crumb = GetCrumb(cell);
            var prototile = crumb.prototile;
            var tileInterior = prototile.InteriorTileAdjacencies
                .Where(x => x.fromChild == childTile && x.fromChildSide == (int)dir)
                .ToList();
            if (tileInterior.Count == 1)
            {
                dest = SetChildTileAt(cell, tileInterior[0].toChild);
                inverseDir = (CellDir)tileInterior[0].toChildSide;
                connection = new Connection();
                return true;
            }
            var tileExterior = prototile.ExteriorTileAdjacencies
                .Where(x => x.child == childTile && x.childSide == (int)dir)
                .Single();

            {
                var (otherCrumb, otherSide) = TryMovePrototile(crumb, tileExterior.parentSide);
                var otherPrototile = otherCrumb.prototile;
                var otherPartialPath = otherCrumb.partialPath;

                var otherSubside = tileExterior.parentSubSideCount - 1 - tileExterior.parentSubSide;
                var otherExterior = otherPrototile.ExteriorTileAdjacencies
                    .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                    .Single();
                if (otherExterior.parentSubSideCount != tileExterior.parentSubSideCount)
                    throw new Exception();
                //partialPath.Insert(0, otherExterior.child);
                //return (partialPath, otherExterior.childSide, prototilesByName[otherParent.ChildPrototiles[otherExterior.child].childName]);

                dest = SetChildTileAt(otherPartialPath, otherExterior.child);
                inverseDir = (CellDir)otherExterior.childSide;
                connection = default;
                return true;
            }
        }



        // Given a prototile (height, partialPath), and a side of that prototile,
        // Moves to the adjacent prototile at the same height, returning which side we entered from.
        // Parents must the list of parent prototiles for partial path. It's indexed from height 0, but it'll only be inspected at height+1 and above.
        private (Crumb crumb, int side) TryMovePrototile(Crumb crumb, int prototileSide)
        {
            var partialPath = crumb.partialPath;
            var height = crumb.height;
            var childPrototile = GetPathAt(partialPath, height);
            var parentCrumb = GetParent(crumb);
            var parent = parentCrumb.prototile;

            // Can does the parent directly tell us what is adjacent?
            var interior = parent.InteriorPrototileAdjacencies
                .Where(x => x.fromChild == childPrototile && x.fromChildSide == prototileSide)
                .ToList();
            if (interior.Count == 1)
            {
                return (GetChild(parentCrumb, interior[0].toChild), interior[0].toChildSide);
            }

            // Nope. We'll have to do a move at height one up, then align subsides
            var exterior = parent.ExteriorPrototileAdjacencies
                .Where(x => x.child == childPrototile && x.childSide == prototileSide)
                .Single();
            {
                var (otherParentCrumb, otherSide) = TryMovePrototile(parentCrumb, exterior.parentSide);
                var otherParent = otherParentCrumb.prototile;

                var otherSubside = exterior.parentSubSideCount - 1 - exterior.parentSubSide;

                while (true)
                {
                    if (otherParent.PassthroughPrototileAdjacencies != null)
                    {
                        var passThrough = otherParent.PassthroughPrototileAdjacencies
                            .Where(x => x.fromParentSide == otherSide && x.fromParentSubSide == otherSubside)
                            .ToList();

                        if (passThrough.Count == 1)
                        {
                            // TODO: We can probably optimize this better
                            (otherParentCrumb, otherSide) = TryMovePrototile(otherParentCrumb, passThrough[0].toParentSide);
                            otherParent = otherParentCrumb.prototile;
                            otherSubside = passThrough[0].toParentSubSide;
                            continue;
                        }
                    }
                    break;
                }

                var otherExterior = otherParent.ExteriorPrototileAdjacencies
                    .Where(x => x.parentSide == otherSide && x.parentSubSide == otherSubside)
                    .Single();
                //if (otherExterior.parentSubSideCount != exterior.parentSubSideCount)
                //    throw new Exception($"At height {height+1}, moving from {otherSide}, {otherSubside}, expected {exterior.parentSubSideCount} subSide count, but got ");
                return (GetChild(otherParentCrumb, otherExterior.child), otherExterior.childSide);
            }
        }
        #endregion

        #region Bounds
        public override IGrid BoundBy(IBound bound) => new SubstitutionTilingGrid(this, (SubstitutionTilingBound)(IntersectBounds(bound, this.bound)));
        #endregion

        #region Position
        public override Vector3 GetCellCenter(Cell cell)
        {
            var crumb = GetCrumb(cell);
            return crumb.transform.MultiplyPoint3x4(crumb.prototile.Centers[GetChildTileAt(cell)]);
        }

        public override TRS GetTRS(Cell cell)
        {
            var crumb = GetCrumb(cell);
            return new TRS(GetTRS(crumb.prototile, crumb.transform, GetChildTileAt(cell)));
        }

        #endregion


        #region Shape
        public override void GetPolygon(Cell cell, out Vector3[] vertices, out Matrix4x4 transform)
        {
            var crumb = GetCrumb(cell);
            vertices = crumb.prototile.ChildTiles[GetChildTileAt(cell)];
            transform = crumb.transform;
        }
        #endregion

        #region Query

        public override bool FindCell(Vector3 position, out Cell cell)
        {
            var inputAabb = new Aabb { min = position, max = position };
            foreach (var crumb in GetCrumbsIntersectsApproxInternal(inputAabb))
            {
                for (var childIndex = 0; childIndex < crumb.prototile.ChildTiles.Length; childIndex++)
                {
                    // Currently does fan detection
                    // Doesn't work for convex faces
                    var transformedPosition = crumb.transform.inverse.MultiplyPoint3x4(position);
                    var vertices = crumb.prototile.ChildTiles[childIndex];
                    var v0 = vertices[0];
                    var prev = vertices[1];
                    for (var i = 2; i < vertices.Length; i++)
                    {
                        var v = vertices[i];
                        if (GeometryUtils.IsPointInTrianglePlanar(transformedPosition, v0, prev, v))
                        {
                            cell = SetChildTileAt(crumb.partialPath, childIndex);
                            return true;
                        }
                        prev = v;
                    }
                    continue;
                }
            }
            cell = default;
            return false;
        }

        public override bool FindCell(
            Matrix4x4 matrix,
            out Cell cell,
            out CellRotation rotation)
        {
            var position = matrix.MultiplyPoint3x4(new Vector3());
            var inputAabb = new Aabb { min = position, max = position };
            foreach (var crumb in GetCrumbsIntersectsApproxInternal(inputAabb))
            {
                for (var childIndex = 0; childIndex < crumb.prototile.ChildTiles.Length; childIndex++)
                {
                    // Currently does fan detection
                    // Doesn't work for convex faces
                    var transformedPosition = crumb.transform.inverse.MultiplyPoint3x4(position);
                    var vertices = crumb.prototile.ChildTiles[childIndex];
                    var v0 = vertices[0];
                    var prev = vertices[1];
                    for (var i = 2; i < vertices.Length; i++)
                    {
                        var v = vertices[i];
                        if (GeometryUtils.IsPointInTrianglePlanar(transformedPosition, v0, prev, v))
                        {
                            // Get the cell
                            cell = SetChildTileAt(crumb.partialPath, childIndex);
                            // Get the rotation
                            var cellType = NGonCellType.Get(crumb.prototile.ChildTiles[childIndex].Length);
                            var cellTransform = GetTRS(crumb.prototile, crumb.transform, childIndex);
                            return MeshGrid.GetRotationFromMatrix(cellType, cellTransform, matrix, out rotation);
                        }
                        prev = v;
                    }
                    continue;
                }
            }
            cell = default;
            rotation = default;
            return false;
        }

        public override IEnumerable<Cell> GetCellsIntersectsApprox(Vector3 min, Vector3 max)
        {
            var inputAabb = new Aabb { min = min, max = max };
            foreach (var crumb in GetCrumbsIntersectsApproxInternal(inputAabb))
            {
                for (var i = 0; i < crumb.prototile.ChildTiles.Length; i++)
                {
                    yield return SetChildTileAt(crumb.partialPath, i);
                }
            }
        }


        // Returns a set of non-overlapping prototiles at different heights that collectively contain all prototiles
        private IEnumerable<Crumb> Spigot()
        {
            var crumb = hierarchyCrumbs[0];
            yield return crumb;
            while (true)
            {
                crumb = GetParent(crumb);
                // Skips 0, we just came from there!
                for (var i = 1; i < crumb.prototile.ChildPrototiles.Length; i++)
                {
                    yield return GetChild(crumb, i);
                }
            }
        }


        // Returns the set of height 0 prototiles intersect inputAabb
        // This is done by iterating over spigot (an infinite stream of prototiles)
        // and recursively subdividing ones that look promising.
        // We stop when we haven't found anything in a while.
        private IEnumerable<Crumb> GetCrumbsIntersectsApproxInternal(Aabb inputAabb)
        {
            var stack = new Stack<Crumb>();
            var highestFoundHeight = -1;
            foreach (var t in Spigot())
            {
                if (highestFoundHeight <= t.height - DeadZone && highestFoundHeight != -1)
                {
                    // Haven't found anything in a while, give up.
                    break;
                }

                // For each of these prototiles, walk the entire tree under it
                stack.Push(t);
                while (stack.Count > 0)
                {
                    var crumb = stack.Pop();
                    var bound = crumb.transform * crumb.prototile.bound;

                    if (bound.Intersects(inputAabb))
                    {
                        if (crumb.height == 0)
                        {
                            highestFoundHeight = Math.Max(highestFoundHeight, t.height);
                            yield return crumb;
                        }
                        else
                        {
                            // Recurse
                            for (var i = 0; i < crumb.prototile.ChildPrototiles.Length; i++)
                            {
                                stack.Push(GetChild(crumb, i));
                            }
                        }
                    }
                }
            }
        }



        public override IEnumerable<RaycastInfo> Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity)
        {
            // Computes distance to interection of aabb, or null if it misses the truncated array.
            Func<Crumb, float?> getDist = (crumb) =>
            {
                var iTransform = crumb.transform.inverse;
                var localOrigin = iTransform.MultiplyPoint3x4(origin);
                var localDirection = iTransform.MultiplyVector(direction);
                return crumb.prototile.bound.Raycast(localOrigin, localDirection, maxDistance);
            };
            var queuedRaycastInfos = new PriorityQueue<RaycastInfo>(x => x.distance, (x, y) => -x.distance.CompareTo(y.distance));
            foreach (var (crumb, dist) in RaycastCrumbs(getDist))
            {
                // Raycast against the actual children of this prototile
                for (var i = 0; i < crumb.prototile.ChildTiles.Length; i++)
                {
                    if (MeshRaycast.RaycastPolygonPlanar(origin, direction, crumb.prototile.ChildTiles[i], crumb.transform, out var point, out var childDist, out var side)
                       && childDist < maxDistance)
                    {
                        var ri = new RaycastInfo
                        {
                            cell = SetChildTileAt(crumb.partialPath, i),
                            cellDir = (CellDir?)side,
                            distance = childDist,
                            point = point,
                        };
                        queuedRaycastInfos.Add(ri);
                    }
                }

                // Drain the queue. As the prototiles are in order, and their child tiles are inside their bounds and thus always have a larger dist
                // we know we've seen everything up to dist
                foreach (var ri in queuedRaycastInfos.Drain(dist))
                {
                    yield return ri;
                }
            }
            foreach (var ri in queuedRaycastInfos.Drain())
            {
                yield return ri;
            }
        }


        /// <summary>
        /// Returns all the height 0 prototiles that intersect the ray, in the correct order.
        /// Much like GetPrototilesIntersectsApproxInternal, this walks over all the prototiles in Spigot,
        /// and recursively subdivides them if their bounds indiate they may contain a tile that intersects the ray.
        /// 
        /// As with that method, we stop if we haven't found anything worth outputting in a while.
        /// 
        /// But it's complicated by the fact we must output items in order. To do so, items are kept in a queue, and with similar logic to the stopping logic, we only output 
        /// something when we haven't found anything smaller in a while.
        /// </summary>
        private IEnumerable<(Crumb, float minDist)> RaycastCrumbs(Func<Crumb, float?> getDist)
        {
            var queue = new PriorityQueue<(int initialHeight, Crumb crumb, float dist)>(x => x.dist, (x, y) => -x.dist.CompareTo(y.dist));

            var highestFoundHeight = -1;
            foreach (var t in Spigot())
            {
                if (queue.Count == 0 && highestFoundHeight <= t.height - DeadZone && highestFoundHeight != -1)
                {
                    // Haven't found anything in a while, give up.
                    break;
                }

                var dist = getDist(t);
                if (dist != null)
                {
                    queue.Add((t.height, t, dist.Value));
                }

                while (queue.Count > 0)
                {
                    var t2 = queue.Peek();
                    if (t2.crumb.height > 0)
                    {
                        var (initialHeight, crumb, _) = queue.Pop();
                        // Recurse
                        for (var i = 0; i < crumb.prototile.ChildPrototiles.Length; i++)
                        {
                            var child = GetChild(crumb, i);
                            var childDist = getDist(child);
                            if (childDist != null)
                            {
                                queue.Add((initialHeight, child, childDist.Value));
                            }
                        }
                    }
                    else
                    {
                        // We've found a height 0 prototile that is ahead of everythign else in the queue.
                        // But is it also ahead of future items we'll find from Spigot?
                        if (t2.initialHeight <= t.height - DeadZone)
                        {
                            var (initialHeight, crumb, dist2) = queue.Pop();
                            highestFoundHeight = Math.Max(highestFoundHeight, initialHeight);
                            yield return (crumb, dist2);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // Drain queue
            while (queue.Count > 0)
            {
                var (initialHeight, crumb, dist2) = queue.Pop();
                if (crumb.height > 0)
                {
                    // Recurse
                    for (var i = 0; i < crumb.prototile.ChildPrototiles.Length; i++)
                    {
                        var child = GetChild(crumb, i);
                        var childDist = getDist(child);
                        if (childDist != null)
                        {
                            queue.Add((initialHeight, child, childDist.Value));
                        }
                    }
                }
                else
                {
                    yield return (crumb, dist2);
                }
            }
        }
        #endregion
    }
}

