using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif

using static Sylves.Test.TestUtils;


namespace Sylves.Test
{
    [TestFixture]
    public class TriangleInterpolationTest
    {
        [Test]
        public void TestTriangleInterpolation()
        {
            var v = new[] { new Vector2(5, 0), new Vector2(10, 0), new Vector2(0, 5) };
            var f = TriangleInterpolation.Interpolate(v[0], v[1], v[2]);
            // Checks that the corners of TestMeshes.PlaneXY (which has vertices in the canconical order)
            // behaves as expected
            AssertAreEqual(v[0], f(TestMeshes.Equilateral.vertices[0]), 1e-6);
            AssertAreEqual(v[1], f(TestMeshes.Equilateral.vertices[1]), 1e-6);
            AssertAreEqual(v[2], f(TestMeshes.Equilateral.vertices[2]), 1e-6);
        }


        [Test]
        public void StdBarycentric()
        {
            // Test standard barycentric coordinates
            var center = new Vector2(0, 0);
            Vector3 bary = TriangleInterpolation.StdBarycentric(center);

            // In the center, all coords should be roughly equal
            Assert.AreEqual(bary.x, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(bary.y, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(bary.z, 1.0f / 3.0f, 0.01f);

            // Test corners
            Vector2 corner1 = new Vector2(0.5f, -0.28868f); // approximating (0.5, -sqrt(3)/6)
            Vector3 bary1 = TriangleInterpolation.StdBarycentric(corner1);
            Assert.AreEqual(bary1.x, 1.0f, 0.01f);
            Assert.AreEqual(bary1.y, 0.0f, 0.01f);
            Assert.AreEqual(bary1.z, 0.0f, 0.01f);
        }

        [Test]
        public void Interpolate2D()
        {
            // Create a simple triangle mesh
            var mesh = new MeshData();
            mesh.vertices = new []{
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0)
            };
            mesh.normals = new []{
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1)
            };
            mesh.indices = new[] { new[] { 0, 1, 2 } };
            mesh.topologies = new[] { MeshTopology.Triangles };

            // Create interpolator
            var interpolator = TriangleInterpolation.InterpolatePosition(mesh, 0, 0, false);

            // Test interpolation at center of triangle
            Vector3 center = new Vector3();
            Vector3 result = interpolator(center);

            // Should be at centroid (1/3, 1/3, 0)
            Assert.AreEqual(result.x, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result.y, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result.z, 0.0f, 0.01f);
        }

        [Test]
        public void Interpolate3D()
        {
            // Create a simple triangle mesh with normals
            var mesh = new MeshData();
            mesh.vertices = new[]{
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0)
            };
            mesh.normals = new[]{
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1)
            };
            mesh.indices = new[] { new[] { 0, 1, 2 } };
            mesh.topologies = new[] { MeshTopology.Triangles };

            // Create 3D interpolator
            var interpolator = TriangleInterpolation.InterpolatePosition(
                mesh, 0, 0, false, -0.5f, 0.5f);

            // Test interpolation at center of triangle, bottom face
            Vector3 centerBottom = new Vector3(0, 0, -0.5f);
            Vector3 result1 = interpolator(centerBottom);

            // Should be at (1/3, 1/3, -0.5)
            Assert.AreEqual(result1.x, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result1.y, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result1.z, -0.5f, 0.01f);

            // Test interpolation at center of triangle, top face
            Vector3 centerTop = new Vector3(0, 0, 0.5f);
            Vector3 result2 = interpolator(centerTop);

            // Should be at (1/3, 1/3, 0.5)
            Assert.AreEqual(result2.x, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result2.y, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result2.z, 0.5f, 0.01f);
        }

        [Test]
        public void JacobiCalculator()
        {
            var sqrt3 = Mathf.Sqrt(3);
            // Create a simple triangle mesh
            var mesh = new MeshData();
            mesh.vertices = new[]{
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0)
            };
            mesh.normals = new[]{
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1)
            };
            mesh.indices = new[] { new[] { 0, 1, 2 } };
            mesh.topologies = new[] { MeshTopology.Triangles };

            // Create Jacobian calculator
            var jacobiCalc = TriangleInterpolation.JacobiPosition(mesh, 0, 0, false);

            // Calculate Jacobian at center of triangle
            Vector3 center = new Vector3(0, 0, 0);
            Matrix4x4 jacobi = jacobiCalc(center);

            // For this simple triangle, the partial derivatives should be simple
            Assert.AreEqual(jacobi.m00, 1.0f, 0.01f);  // dx/du = 1
            Assert.AreEqual(jacobi.m01, -sqrt3 / 3, 0.01f);  // dx/dv = 0
            Assert.AreEqual(jacobi.m10, 0.0f, 0.01f);  // dy/du = 0
            Assert.AreEqual(jacobi.m11, 2 / sqrt3, 0.01f);  // dy/dv = 1

            // The position in the last column should match the interpolated point
            Assert.AreEqual(jacobi.m03, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(jacobi.m13, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(jacobi.m23, 0.0f, 0.01f);
        }

        [Test]
        public void InterpolateUV()
        {
            // Create a simple triangle mesh with UVs
            var mesh = new MeshData();
            mesh.vertices = new[]{
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 0)
            };
            mesh.normals = new[]{
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 1)
            };
            mesh.uv = new[]{
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };
            mesh.indices = new[] { new[] { 0, 1, 2 } };
            mesh.topologies = new[] { MeshTopology.Triangles };

            // Create UV interpolator
            var uvInterpolator = TriangleInterpolation.InterpolateUv(mesh, 0, 0, false);

            // Test interpolation at center of triangle
            Vector3 center = new Vector3(0, 0, 0);
            Vector2 result = uvInterpolator(center);

            // Should be at (1/3, 1/3)
            Assert.AreEqual(result.x, 1.0f / 3.0f, 0.01f);
            Assert.AreEqual(result.y, 1.0f / 3.0f, 0.01f);
        }
    }
}
