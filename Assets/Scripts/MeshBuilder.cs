using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts
{
    internal class MeshBuilder
    {
        List<Vector3> vertices;
        List<Vector3> normals;
        List<Vector2> uvs;
        List<List<int>> triangles;
        Dictionary<string,int> offsets;

        public MeshBuilder(int submeshCount = 1)
        {
            vertices    = new List<Vector3>();
            normals     = new List<Vector3>();
            uvs         = new List<Vector2>();
            triangles   = new List<List<int>>() { new List<int>() };
            offsets     = new Dictionary<string, int>()
            {
                { "vertices"    , 0 },
                { "normals"     , 0 },
                { "uvs"         , 0 },
                { "triangles0"  , 0 },
            };
            for (int i = 1; i < submeshCount; i++)
                AddSubmesh();
        }

        public Vector3[] GetVertices() { return vertices.ToArray(); }
        public Vector3[] GetNormals() { return normals.ToArray(); }
        public Vector2[] GetUVs() { return uvs.ToArray(); }
        public int[] GetTriangles(int submeshIndex = 0) { return triangles[submeshIndex].ToArray(); }

        public void AddSubmesh()
        {
            triangles.Add(new List<int>());
            offsets.Add("triangles" + (triangles.Count - 1), 0);
        }

        public void AddVertex(float x, float y, float z)
        {
            if (offsets["vertices"] < vertices.Count)
                vertices[offsets["vertices"]++].Set(x, y, z);
            else
            {
                vertices.Add(new Vector3(x, y, z));
                offsets["vertices"]++;
            } 
        }

        public void AddNormal(float x, float y, float z)
        {
            if (offsets["normals"] < normals.Count)
                normals[offsets["normals"]++].Set(x, y, z);
            else
            {
                normals.Add(new Vector3(x, y, z));
                offsets["normals"]++;
            }
        }

        public void AddUV(float u, float v)
        {
            if (offsets["uvs"] < uvs.Count)
                uvs[offsets["uvs"]++].Set(u, v);
            else
            {
                uvs.Add(new Vector2(u, v));
                offsets["uvs"]++;
            }
        }

        public void AddTriangle(int a, int b, int c, int submeshIndex = 0)
        {
            string s = "triangles" + submeshIndex;
            if (offsets[s] < triangles[submeshIndex].Count)
            {
                triangles[submeshIndex][offsets[s]++] = a;
                triangles[submeshIndex][offsets[s]++] = b;
                triangles[submeshIndex][offsets[s]++] = c;
            }
            else
            {
                triangles[submeshIndex].Add(a); offsets[s]++;
                triangles[submeshIndex].Add(b); offsets[s]++;
                triangles[submeshIndex].Add(c); offsets[s]++;
            }
        }

        public void ToMesh(Mesh mesh)
        {
            mesh.vertices = null;
            mesh.normals = null;
            mesh.uv = null;
            mesh.triangles = null;
            mesh.subMeshCount = triangles.Count;
            mesh.SetVertices(vertices, 0, offsets["vertices"]);
            mesh.SetNormals(normals, 0, offsets["normals"]);
            mesh.SetUVs(0, uvs, 0, offsets["uvs"]);
            for (int i = 0; i < triangles.Count; i++)
                mesh.SetTriangles(triangles[i], 0, offsets["triangles" + i], i, true, 0);
        }

        public void ResetOffsets()
        {
            offsets["vertices"] = 0;
            offsets["normals"] = 0;
            offsets["uvs"] = 0;
            offsets["triangles0"] = 0;
            for (int i = 1; i < triangles.Count; i++)
                offsets["triangles" + i] = 0;
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, int submeshIndex)
        {
            int[] indices = new int[3];
            AddVertex(a.x, a.y, a.z); indices[0] = vertices.Count - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = vertices.Count - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = vertices.Count - 1;
            AddUV(0, 0);
            AddUV(1, 0);
            AddUV(0, 1);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddTriangle(indices[0], indices[2], indices[1]);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int submeshIndex)
        {
            int[] indices = new int[4];
            AddVertex(a.x, a.y, a.z); indices[0] = vertices.Count - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = vertices.Count - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = vertices.Count - 1;
            AddVertex(d.x, d.y, d.z); indices[3] = vertices.Count - 1;
            AddUV(0, 0);
            AddUV(1, 0);
            AddUV(0, 1);
            AddUV(1, 1);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            AddTriangle(indices[0], indices[2], indices[1]);
            AddTriangle(indices[1], indices[2], indices[3]);
        }

        public void AddSegments(List<Segment> segments)
        {

        }
    }
}
