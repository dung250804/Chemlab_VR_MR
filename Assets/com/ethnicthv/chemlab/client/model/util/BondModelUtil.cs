using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.util
{
    /// <summary>
    /// Model Generation Utility for Bonds <br/>
    ///     - All Model will have default rotation facing the positive y-axis <br/>
    ///     - All Model will have default position at the origin <br/>
    /// </summary>
    public static class BondModelUtil
    {
        public static Mesh GenerateSingleBond(float radius, float length)
        {
            var mesh = new Mesh();
            var (vertices, indices) = GenerateCylinder(radius, length, 16);
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        public static Mesh GenerateDoubleBond(float radius, float length)
        {
            var mesh = new Mesh();
            var (vertices1, indices1) = GenerateCylinder(radius, length, 16);
            var (vertices2, indices2) = GenerateCylinder(radius, length, 16);

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var offset = new Vector3(radius*1.5f, 0, 0);

            // offset the second cylinder
            for (var i = 0; i < vertices2.Count; i++)
            {
                vertices2[i] += offset;
            }

            for (var i = 0; i < vertices1.Count; i++)
            {
                vertices1[i] -= offset;
            }

            for (var i = 0; i < indices2.Count; i++)
            {
                indices2[i] += vertices1.Count;
            }

            vertices.AddRange(vertices1);
            vertices.AddRange(vertices2);

            indices.AddRange(indices1);
            indices.AddRange(indices2);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }
        
        public static Mesh GenerateTripleBond(float radius, float length)
        {
            var mesh = new Mesh();
            var (vertices1, indices1) = GenerateCylinder(radius, length, 16);
            var (vertices2, indices2) = GenerateCylinder(radius, length, 16);
            var (vertices3, indices3) = GenerateCylinder(radius, length, 16);

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var offset = new Vector3(radius * 2.25f, 0, 0);

            // offset the second cylinder
            for (var i = 0; i < vertices2.Count; i++)
            {
                vertices2[i] += offset;
            }

            for (var i = 0; i < vertices3.Count; i++)
            {
                vertices3[i] -= offset;
            }

            for (var i = 0; i < indices2.Count; i++)
            {
                indices2[i] += vertices1.Count;
            }

            for (var i = 0; i < indices3.Count; i++)
            {
                indices3[i] += vertices1.Count + vertices2.Count;
            }

            vertices.AddRange(vertices1);
            vertices.AddRange(vertices2);
            vertices.AddRange(vertices3);

            indices.AddRange(indices1);
            indices.AddRange(indices2);
            indices.AddRange(indices3);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        private static (List<Vector3>, List<int>) GenerateCylinder(float radius, float height, int nbSides)
        {
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            var step = 2 * Mathf.PI / nbSides;
            for (var i = 0; i < nbSides; i++)
            {
                var angle = i * step;
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, height, z));
                vertices.Add(new Vector3(x, 0, z));
            }

            for (var i = 0; i < nbSides; i++)
            {
                var i1 = i * 2;
                var i2 = (i * 2 + 1) % (nbSides * 2);
                var i3 = (i * 2 + 3) % (nbSides * 2);
                var i4 = (i * 2 + 2) % (nbSides * 2);
                indices.Add(i3);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i4);
                indices.Add(i3);
                indices.Add(i1);
            }

            return (vertices, indices);
        }
    }
}