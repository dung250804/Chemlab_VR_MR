using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.model.util
{
    public static class SphereModelUtil
    {
        public static Mesh GenerateIcoSphereMesh(float radius)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<(int, int, int)>();

            CreateDefaultMesh(radius, vertices, triangles, out var triStack);

            while (triStack.TryPop(out var tri))
            {
                SubDivide(tri.Item1, tri.Item2, tri.Item3, tri.Item4, vertices, triangles, triStack, radius);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.SelectMany(t => new[] { t.Item1, t.Item2, t.Item3 }).ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.OptimizeReorderVertexBuffer();
            return mesh;
        }

        private static void CreateDefaultMesh(float radius, List<Vector3> vertices, List<(int, int, int)> triangles,
            out Stack<(int, int, int, int)> triStack)
        {
            triStack = new Stack<(int, int, int, int)>();
            
            var v0 = new Vector3(0, 1, 0);
            var v1 = new Vector3(1, 0, 1);
            var v2 = new Vector3(-1, 0, 1);
            var v3 = new Vector3(1, 0, -1);
            var v4 = new Vector3(-1, 0, -1);
            var v5 = new Vector3(0, -1, 0);
            
            v0.Normalize();
            v1.Normalize();
            v2.Normalize();
            v3.Normalize();
            v4.Normalize();
            v5.Normalize();
            
            vertices.Add(v0 * radius);
            vertices.Add(v1 * radius);
            vertices.Add(v2 * radius);
            vertices.Add(v3 * radius);
            vertices.Add(v4 * radius);
            vertices.Add(v5 * radius);
            
            AddTriangle(2, 1, 0, triangles, triStack, 0);
            AddTriangle(4, 2, 0, triangles, triStack, 0);
            AddTriangle(3, 4, 0, triangles, triStack, 0);
            AddTriangle(1, 3, 0, triangles, triStack, 0);
            AddTriangle(3, 1, 5, triangles, triStack, 0);
            AddTriangle(4, 3, 5, triangles, triStack, 0);
            AddTriangle(2, 4, 5, triangles, triStack, 0);
            AddTriangle(1, 2, 5, triangles, triStack, 0);

            // regular pyramid shape
            // var v0 = new Vector3(radius, radius, radius);
            // var v1 = new Vector3(-radius, radius, -radius);
            // var v2 = new Vector3(-radius, -radius, radius);
            // var v3 = new Vector3(radius, -radius, -radius);
            //
            // v0.Normalize();
            // v1.Normalize();
            // v2.Normalize();
            // v3.Normalize();
            //
            // vertices.Add(v0 * radius);
            // vertices.Add(v1 * radius);
            // vertices.Add(v2 * radius);
            // vertices.Add(v3 * radius);
            //
            // AddTriangle(0, 1, 2, triangles, triStack, 0);
            // AddTriangle(0, 2, 3, triangles, triStack, 0);
            // AddTriangle(0, 3, 1, triangles, triStack, 0);
            // AddTriangle(1, 3, 2, triangles, triStack, 0);
        }

        private static int AddVertex(Vector3 vertex, List<Vector3> vertices)
        {
            vertices.Add(vertex);
            return vertices.Count - 1;
        }

        private static void AddTriangle(int a, int b, int c, List<(int, int, int)> triangles,
            Stack<(int, int, int, int)> triStack, int depth, int maxDepth = 4)
        {
            triangles.Add((a, b, c));
            if (depth >= maxDepth)
            {
                return;
            }

            triStack.Push((a, b, c, depth));
        }

        private static void RemoveTriangle((int, int, int) tri, List<(int, int, int)> triangles)
        {
            triangles.Remove(tri);
        }

        private static int GetMidPoint(int p1, int p2, List<Vector3> vertices, float radius)
        {
            var v1 = vertices[p1];
            var v2 = vertices[p2];
            var mid = (v1 + v2) / 2;

            // re-adjust the mid point to the sphere
            mid = mid.normalized * radius;

            return AddVertex(mid, vertices);
        }

        private static void SubDivide(int a, int b, int c, int depth, List<Vector3> vertices,
            List<(int, int, int)> triangles, Stack<(int, int, int, int)> triStack, float radius)
        {
            var newDepth = depth + 1;

            var ab = GetMidPoint(a, b, vertices, radius);
            var bc = GetMidPoint(b, c, vertices, radius);
            var ca = GetMidPoint(c, a, vertices, radius);

            AddTriangle(a, ab, ca, triangles, triStack, newDepth);
            AddTriangle(b, bc, ab, triangles, triStack, newDepth);
            AddTriangle(c, ca, bc, triangles, triStack, newDepth);
            AddTriangle(ab, bc, ca, triangles, triStack, newDepth);

            // remove the original triangle
            RemoveTriangle((a, b, c), triangles);
        }
    }
}