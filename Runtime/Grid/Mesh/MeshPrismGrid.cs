using UnityEngine;


using System.Collections.Generic;
using System.Linq;

namespace Sylves
{
    /// <summary>
    /// Represents a 3d grid, where each cell is an extrusion of a face along the normals, offset to a given height.
    /// </summary>
    public class MeshPrismGrid : MeshGrid
    {

        private readonly MeshPrismGridOptions meshPrismOptions;

        public MeshPrismGrid(MeshData meshData, MeshPrismGridOptions meshPrismOptions) :
            base(meshData, meshPrismOptions, MeshGridBuilder.Build(meshData, meshPrismOptions), false)
        {
            this.meshPrismOptions = meshPrismOptions;
            BuildMeshDetails();
        }

        internal MeshPrismGrid(MeshData meshData, MeshPrismGridOptions meshPrismOptions, DataDrivenData data, bool is2d) :
            base(meshData, meshPrismOptions, data, is2d)
        {
            this.meshPrismOptions = meshPrismOptions;
        }

        #region Relatives

        public override IDualMapping GetDual()
        {
            var dmb = new DualMeshBuilder(meshData);
            var dualGrid = new MeshPrismGrid(dmb.DualMeshData, new MeshPrismGridOptions(meshPrismOptions)
            {
                MinLayer = meshPrismOptions.MinLayer,
                MaxLayer = meshPrismOptions.MaxLayer + 1,
                LayerOffset = meshPrismOptions.LayerOffset - 0.5f * meshPrismOptions.LayerHeight,
            });
            return new DualMapping(this, dualGrid, dmb.Mapping, meshPrismOptions.MinLayer, meshPrismOptions.MaxLayer);
        }

        private class DualMapping : BasicDualMapping
        {
            Dictionary<(Cell, CellCorner), (Cell, CellCorner)> toDual;
            Dictionary<(Cell, CellCorner), (Cell, CellCorner)> toBase;

            public DualMapping(MeshPrismGrid baseGrid, MeshPrismGrid dualGrid, List<(int primalFace, int primalVert, int dualFace, int dualVert)> rawMapping, int minLayer, int maxLayer) : base(baseGrid, dualGrid)
            {
                toDual = new Dictionary<(Cell, CellCorner), (Cell, CellCorner)>();
                foreach(var (primalFace, primalVert, dualFace, dualVert) in rawMapping)
                {
                    var basePrismInfo = (baseGrid.CellData[new Cell(primalFace, 0, 0)] as MeshCellData).PrismInfo;
                    var dualPrismInfo = (dualGrid.CellData[new Cell(dualFace, 0, 0)] as MeshCellData).PrismInfo;
                    var (primalBack, primalForward) = basePrismInfo.BaseToPrismCorners[(CellCorner)primalVert];
                    var (dualBack, dualForward) = dualPrismInfo.BaseToPrismCorners[(CellCorner)dualVert];
                    foreach (var layer in Enumerable.Range(minLayer, maxLayer - minLayer))
                    {
                        toDual.Add(
                            (new Cell(primalFace, 0, layer), primalForward),
                            (new Cell(dualFace, 0, layer + 1), dualBack)
                            );
                        toDual.Add(
                            (new Cell(primalFace, 0, layer), primalBack),
                            (new Cell(dualFace, 0, layer), dualForward)
                            );
                    }
                }
                toBase = toDual.ToDictionary(kv => kv.Value, kv => kv.Key);
            }

            public override (Cell dualCell, CellCorner inverseCorner)? ToDualPair(Cell cell, CellCorner corner)
            {
                if (toDual.TryGetValue((cell, corner), out var r))
                {
                    return r;
                }
                else
                {
                    return null;
                }
            }

            public override (Cell baseCell, CellCorner inverseCorner)? ToBasePair(Cell cell, CellCorner corner)
            {

                if (toBase.TryGetValue((cell, corner), out var r))
                {
                    return r;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Query
        protected override RaycastInfo? RaycastCell(Cell cell, Vector3 rayOrigin, Vector3 direction)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            if (meshCellData.CellType == CubeCellType.Instance && meshCellData.PrismInfo.BackDir != (CellDir)CubeDir.Back)
            {
                // Fast path?
                GetCubeCellVertices(cell, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8);
                var hit = MeshRaycast.RaycastCube(rayOrigin, direction, v1, v2, v3, v4, v5, v6, v7, v8);
                if (hit != null)
                {
                    var hit2 = hit.Value;
                    hit2.cell = cell;
                    return hit2;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                float bestDistance = float.MaxValue;
                RaycastInfo? bestHit = null;
                foreach (var (v0, v1, v2, cellDir) in GetTriangleMesh(cell))
                {
                    var hit = MeshRaycast.RaycastTri(rayOrigin, direction, v0, v1, v2, out var point, out var distance);
                    if (hit && distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestHit = new RaycastInfo
                        {
                            cell = cell,
                            cellDir = cellDir,
                            distance = distance,
                            point = point,
                        };
                    }
                }
                return bestHit;
            }
        }

        /// <summary>
        /// Returns generalized winding number as a fraction of the sphere area.
        /// This should sum to an integer value when summed over all triangles in a closed mesh.
        /// https://users.cs.utah.edu/~ladislav/jacobson13robust/jacobson13robust.html
        /// </summary>
        private float GeneralizedWinding(Vector3 p, Vector3 vi, Vector3 vj, Vector3 vk)
        {
            p -= vi;
            vj -= vi;
            vk -= vi;
            // vi := (0,0,0), a := vi - p
            var a = -p;
            // arbitrary position for vj, b := vj - p
            var b = vj - p;
            // arbitrary position for vk, c := vk - p
            var c = vk - p;
            // determinant of (a,b,c)
            var detabc = a.x * b.y * c.z + b.x * c.y * a.z + c.x * a.y * b.z -
                a.x * c.y * b.z - b.x * a.y * c.z - c.x * b.y * a.z;
            var al = a.magnitude;
            var bl = b.magnitude;
            var cl = c.magnitude;
            // divisor in atan
            var divisor = al * bl * cl + Vector3.Dot(a, b) * cl + Vector3.Dot(b, c) * al + Vector3.Dot(c, a) * bl;
            var sabc = 2 * Mathf.Atan(detabc / divisor);
            return sabc / (4 * Mathf.PI);
        }

        protected override bool IsPointInCell(Vector3 position, Cell cell)
        {
            // Uses raycasts. Not robust enough
            if(false)
            {
                var c = 0;
                foreach (var (v0, v1, v2, _) in GetTriangleMesh(cell))
                {
                    if (MeshRaycast.RaycastTri(position, Vector3.right, v0, v1, v2, out var _, out var _, out var side))
                    {
                        c += side ? 1 : -1;
                    }
                }
                return c != 0;
            }

            var d = 0f;
            foreach (var (v0, v1, v2, dir) in GetTriangleMesh(cell))
            {
                d += GeneralizedWinding(position, v0, v1, v2); ;
            }
            return Mathf.RoundToInt(d) != 0;
        }
        #endregion

        #region Shape

        // Follows mesh conventions for vertex order (i.e. v1-v4 are the face vertices and v5-v8 are repeats at a different normal offset).
        private void GetCubeCellVertices(Cell cell, out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4, out Vector3 v5, out Vector3 v6, out Vector3 v7, out Vector3 v8)
        {
            // TODO: Share this code with QuadInterpolation.InterpolatePosition?
            var (face, submesh, layer) = (cell.x, cell.y, cell.z);

            var meshOffset1 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset - meshPrismOptions.LayerHeight / 2;
            var meshOffset2 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset + meshPrismOptions.LayerHeight / 2;
            QuadInterpolation.GetCorners(meshData, submesh, face, meshPrismOptions.InvertWinding, meshOffset1, meshOffset2, out v1, out v2, out v3, out v4, out v5, out v6, out v7, out v8);
        }

        private static readonly ILookup<CellDir, int> s_faces = new[]
        {
                ((CellDir)(CubeDir.Left), 0),
                ((CellDir)(CubeDir.Right), 1),
                ((CellDir)(CubeDir.Up), 2),
                ((CellDir)(CubeDir.Down), 3),
                ((CellDir)(CubeDir.Forward), 4),
                ((CellDir)(CubeDir.Back), 5),
            }.ToLookup(x => x.Item1, x => x.Item2);

        private static readonly TRS s_trsIdentity = new TRS();

        public override IEnumerable<(Vector3, Vector3, Vector3, CellDir)> GetTriangleMesh(Cell cell)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            var face = meshCellData.Face;
            var prismInfo = meshCellData.PrismInfo;
            var vertices = meshData.vertices;
            var normals = meshData.normals;
            var (faceIndex, submesh, layer) = (cell.x, cell.y, cell.z);


            var meshOffset1 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset - meshPrismOptions.LayerHeight / 2;
            var meshOffset2 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset + meshPrismOptions.LayerHeight / 2;

            // Explore all the square sides
            for (var i = 0; i < face.Length; i++)
            {
                var v1 = vertices[face[i]];
                var v2 = vertices[face[(i + 1) % face.Length]];
                var n1 = normals[face[i]];
                var n2 = normals[face[(i + 1) % face.Length]];
                var baseCellDir = MeshGridBuilder.EdgeIndexToCellDir(i, face.Count, meshPrismOptions.DoubleOddFaces);
                var cellDir = prismInfo.BaseToPrism(baseCellDir);
                yield return (v1 + n1 * meshOffset1, v1 + n1 * meshOffset2, v2 + n2 * meshOffset2, cellDir);
                yield return (v1 + n1 * meshOffset1, v2 + n2 * meshOffset2, v2 + n2 * meshOffset1, cellDir);
            }
            // Currently does fan detection
            // Doesn't work for convex faces
            {
                var v0 = vertices[face[0]];
                var n0 = normals[face[0]];
                var v1 = vertices[face[1]];
                var n1 = normals[face[1]];
                for (var i = 2; i < face.Count; i++)
                {
                    var v2 = vertices[face[i]];
                    var n2 = normals[face[i]];
                    yield return (v0 + n0 * meshOffset2, v2 + n2 * meshOffset2, v1 + n1 * meshOffset2, prismInfo.ForwardDir);
                    yield return (v0 + n0 * meshOffset1, v1 + n1 * meshOffset1, v2 + n2 * meshOffset1, prismInfo.BackDir);
                    v1 = v2;
                    n1 = n2;
                }
            }
        }

        public override void GetMeshData(Cell cell, out MeshData meshData, out Matrix4x4 transform)
        {
            GetCellMesh(cell, out meshData, out var trs, out var _);
            transform = trs.ToMatrix();
        }

        public void GetCellMesh(Cell cell, out MeshData meshData, out TRS trs, out ILookup<CellDir, int> faces)
        {
            var meshCellData = CellData[cell] as MeshCellData;
            var cellType = meshCellData.CellType;
            if (cellType is CubeCellType)
            {
                if(meshCellData.PrismInfo.BackDir != (CellDir)CubeDir.Back)
                {
                    // TODO: This would be easy to support, just swap out s_faces.
                    throw new System.NotImplementedException("UseXZPlane not supported");
                }

                GetCubeCellVertices(cell, out var v1, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8);
                var vertices = new[] { v1, v2, v3, v4, v5, v6, v7, v8 };

                // z-forward convetion (see TestMeshes.Cube)
                var indices = new[]
                {
                    7, 6, 2, 3, // Left
                    0, 1, 5, 4, // Right
                    2, 6, 5, 1, // Up
                    0, 4, 7, 3, // Down
                    4, 5, 6, 7, // Forward
                    3, 2, 1, 0, // Back
                };

                meshData = new MeshData();
                meshData.vertices = vertices;
                meshData.indices = new[] { indices };
                meshData.topologies = new[] { MeshTopology.Quads };

                faces = s_faces;
                trs = s_trsIdentity;
            }
            else
            {
                // Similar to ExtrudePolygonToPrism
                var face = meshCellData.Face;
                var prismInfo = meshCellData.PrismInfo;
                var vertices = this.meshData.vertices;
                var normals = this.meshData.normals;
                var (faceIndex, submesh, layer) = (cell.x, cell.y, cell.z);


                var meshOffset1 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset - meshPrismOptions.LayerHeight / 2;
                var meshOffset2 = meshPrismOptions.LayerHeight * layer + meshPrismOptions.LayerOffset + meshPrismOptions.LayerHeight / 2;

                var n = face.Length;
                var outVertices = new Vector3[n * 2];
                var outIndices = new int[n * 4 + n * 2];
                var outFaces = new (CellDir, int)[n + 2];

                // Find the vertices
                for (var i = 0; i < n; i++)
                {
                    var v1 = vertices[face[i]];
                    var n1 = normals[face[i]];
                    outVertices[i] = v1 + n1 * meshOffset1;
                    outVertices[i + n] = v1 + n1 * meshOffset2;
                }

                // Explore all the square sides
                for (var i = 0; i < n; i++)
                {
                    outIndices[i * 4 + 0] = i;
                    outIndices[i * 4 + 1] = (i + 1) % n;
                    outIndices[i * 4 + 2] = (i + 1) % n + n;
                    outIndices[i * 4 + 3] = ~(i + n);
                    var baseCellDir = MeshGridBuilder.EdgeIndexToCellDir(i, face.Count, meshPrismOptions.DoubleOddFaces);
                    var cellDir = prismInfo.BaseToPrism(baseCellDir);

                    outFaces[i] = (baseCellDir, i);
                }
                // Top and bottom
                for (var i = 0; i < n; i++)
                {
                    outIndices[n * 4 + i] = n - 1 - i;
                    outIndices[n * 5 + i] = n + i;
                }
                outIndices[n * 5 - 1] = ~outIndices[n * 5 - 1];
                outIndices[n * 6 - 1] = ~outIndices[n * 6 - 1];
                outFaces[n] = (prismInfo.BackDir, n);
                outFaces[n + 1] = (prismInfo.ForwardDir, n + 1);

                meshData = new MeshData
                {
                    vertices = outVertices,
                    indices = new[] { outIndices },
                    topologies = new MeshTopology[] { MeshTopology.NGon },
                };
                trs = s_trsIdentity;
                faces = outFaces.ToLookup(x => x.Item1, x => x.Item2);
            }
        }
        #endregion

        #region Impl

        protected override (Vector3, Vector3) ComputeBounds(Cell cell)
        {
            var meshCellData = (MeshCellData)CellData[cell];
            var minMeshOffset = meshPrismOptions.MinLayer * meshPrismOptions.LayerHeight + meshPrismOptions.LayerOffset;
            var maxMeshOffset = meshPrismOptions.MaxLayer * meshPrismOptions.LayerHeight + meshPrismOptions.LayerOffset;

            var face = meshCellData.Face;
            var cellMin = meshData.vertices[face[0]];
            var cellMax = cellMin;
            for (var i = 1; i < face.Count; i++)
            {
                var v = meshData.vertices[face[i]];
                var n = meshData.normals[face[i]];
                var v1 = v + n * minMeshOffset;
                var v2 = v + n * maxMeshOffset;
                cellMin = Vector3.Min(cellMin, v1);
                cellMax = Vector3.Max(cellMax, v1);
                cellMin = Vector3.Min(cellMin, v2);
                cellMax = Vector3.Max(cellMax, v2);
            }
            return (cellMin, cellMax);
        }

        #endregion
    }
}
