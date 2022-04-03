﻿using NUnit.Framework;
#if UNITY
using UnityEngine;
#endif


namespace Sylves.Test
{
    [TestFixture]
    public class MeshPrismGridTest
    {
        MeshData meshData;
        MeshPrismOptions options;
        public MeshPrismGridTest()
        {
            meshData = new MeshData();
            Vector3[] vertices = {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, -0.5f, -0.5f),
                new Vector3 (+0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, -0.5f),
                new Vector3 (-0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, +0.5f, +0.5f),
                new Vector3 (+0.5f, -0.5f, +0.5f),
                new Vector3 (-0.5f, -0.5f, +0.5f),
            };
            int[] quads = {
                0, 3, 2, 1,
                2, 3, 4, 5,
                1, 2, 5, 6,
                0, 7, 4, 3,
                5, 4, 7, 6,
                0, 1, 6, 7,
            };

            meshData.subMeshCount = 1;
            meshData.vertices = vertices;
            meshData.indices = new[] { quads };
            meshData.topologies = new[] { MeshTopology.Quads };
            meshData.RecalculateNormals();

            options = new MeshPrismOptions
            {
                LayerHeight = 1,
                MinLayer = 0,
                MaxLayer = 2,
            };
        }

        [Test]
        public void TestFindCell()
        {
            var g = new MeshPrismGrid(meshData, options);
            GridTest.FindCell(g, new Cell(0, 0, 1));
        }

        [Test]
        public void TestGetTRS()
        {
            // quad 0 points z-
            var g = new MeshPrismGrid(meshData, options);
            var trs = g.GetTRS(new Cell());
            var v = trs.ToMatrix().MultiplyVector(Vector3.forward);
            TestUtils.AssertAreEqual(Vector3.back, v, 1e-6);
        }
    }
}
