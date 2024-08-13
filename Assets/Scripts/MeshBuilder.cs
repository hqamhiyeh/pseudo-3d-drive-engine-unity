using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class MeshBuilder
    {
        List<Vector3> vertices;
        List<Vector3> normals;
        List<Vector2> uvs;
        List<int> triangles;
        List<List<int>> subTriangles;
        Dictionary<string,int> offset;

        public MeshBuilder()
        {
            vertices    = new List<Vector3>();
            normals     = new List<Vector3>();
            uvs         = new List<Vector2>();
            triangles   = new List<int>();
            offset      = new Dictionary<string, int>()
            {
                { "vertices"    , 0 },
                { "normals"     , 0 },
                { "uvs"         , 0 },
                { "triangles"   , 0 }
            };
        }

        public void AddVertex(float x, float y, float z)
        {
            if (offset["vertices"] < vertices.Count)
                vertices[offset["vertices"]++].Set(x, y, z);
            else
            {
                vertices.Add(new Vector3(x, y, z));
                offset["vertices"]++;
            } 
        }

        public void AddNormal(float x, float y, float z)
        {
            if (offset["normals"] < normals.Count)
                normals[offset["normals"]++].Set(x, y, z);
            else
            {
                normals.Add(new Vector3(x, y, z));
                offset["normals"]++;
            }
        }

        public void AddUV(float u, float v)
        {
            if (offset["uvs"] < uvs.Count)
                uvs[offset["uvs"]++].Set(u, v);
            else
            {
                uvs.Add(new Vector2(u, v));
                offset["uvs"]++;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (offset["triangles"] < triangles.Count)
            {
                triangles[offset["triangles"]++] = a;
                triangles[offset["triangles"]++] = b;
                triangles[offset["triangles"]++] = c;
            }
            else
            {
                triangles.Add(a); offset["triangles"]++;
                triangles.Add(b); offset["triangles"]++;
                triangles.Add(c); offset["triangles"]++;
            }
        }

        public Vector3 GetVertex(int index)
        {
            return vertices[index];
        }

        public Vector3[] GetVertices() { return vertices.ToArray(); }
        public Vector3[] GetNormals() { return normals.ToArray(); }
        public Vector2[] GetUVs() { return uvs.ToArray(); }
        public int[] GetTriangles() { return triangles.ToArray(); }

        public void ToMesh(Mesh mesh)
        {
            mesh.vertices = null;
            mesh.normals = null;
            mesh.uv = null;
            mesh.triangles = null;

            mesh.SetVertices(vertices, 0, offset["vertices"]);
            mesh.SetNormals(normals, 0, offset["normals"]);
            mesh.SetUVs(0, uvs, 0, offset["uvs"]);
            mesh.SetTriangles(triangles, 0, offset["triangles"], 0, true, 0);
        }

        public void ResetOffset()
        {
            offset["vertices"] = 0;
            offset["normals"] = 0;
            offset["uvs"] = 0;
            offset["triangles"] = 0;
        }

        public void AddSegments(List<Segment> segments)
        {

        }
    }
}
