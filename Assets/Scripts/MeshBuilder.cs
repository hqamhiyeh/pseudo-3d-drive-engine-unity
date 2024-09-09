using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    internal class MeshBuilder
    {
        private const int INITIAL_ARRAY_CAPACITY_VERTICES     = 100;
        private const int INITIAL_ARRAY_CAPACITY_NORMALS      = 100;
        private const int INITIAL_ARRAY_CAPACITY_UVS          = 100;
        private const int INITIAL_ARRAY_CAPACITY_TRIANGLES    = 100;

        Vector3[]   vertices;
        Vector3[]   normals;
        Vector2[]   uvs;
        List<int[]> triangles;
        Dictionary<string,int> offsets;

        public MeshBuilder(int submeshCount = 1)
        {
            vertices    = new Vector3[INITIAL_ARRAY_CAPACITY_VERTICES];
            normals     = new Vector3[INITIAL_ARRAY_CAPACITY_NORMALS];
            uvs         = new Vector2[INITIAL_ARRAY_CAPACITY_UVS];
            triangles   = new List<int[]>() { new int[INITIAL_ARRAY_CAPACITY_TRIANGLES] };
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

        public Vector3[] GetVertices() { return vertices[0..(offsets["vertices"] - 1)]; }
        public Vector3[] GetNormals() { return normals[0..(offsets["normals"] - 1)]; }
        public Vector2[] GetUVs() { return uvs[0..(offsets["uvs"] - 1)]; }
        public int[] GetTriangles(int submeshIndex = 0) { return triangles[submeshIndex][0..(offsets[("triangles" + submeshIndex)] - 1)]; }

        public void AddSubmesh()
        {
            triangles.Add(new int[INITIAL_ARRAY_CAPACITY_TRIANGLES]);
            offsets.Add("triangles" + (triangles.Count - 1), 0);
        }

        public void AddVertex(float x, float y, float z)
        {
            if (offsets["vertices"] >= vertices.Length)
                Array.Resize(ref vertices, vertices.Length * 2);
            
            vertices[offsets["vertices"]++].Set(x, y, z);
        }

        public void AddNormal(float x, float y, float z)
        {
            if (offsets["normals"] >= normals.Length)
                Array.Resize(ref normals, normals.Length * 2);
            
            normals[offsets["normals"]++].Set(x, y, z);
        }

        public void AddUV(float u, float v)
        {
            if (offsets["uvs"] >= uvs.Length)
                Array.Resize(ref uvs, uvs.Length * 2);
            
            uvs[offsets["uvs"]++].Set(u, v);
        }

        public void AddTriangle(int a, int b, int c, int submeshIndex = 0)
        {
            string s = "triangles" + submeshIndex;
            int[] submesh = triangles[submeshIndex];
            
            if (offsets[s] + 2 >= submesh.Length)
            {
                Array.Resize<int>(ref submesh, submesh.Length * 2);
                triangles[submeshIndex] = submesh;
            }
            
            triangles[submeshIndex][offsets[s]++] = a;
            triangles[submeshIndex][offsets[s]++] = b;
            triangles[submeshIndex][offsets[s]++] = c;
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
            AddVertex(a.x, a.y, a.z); indices[0] = offsets["vertices"] - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = offsets["vertices"] - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = offsets["vertices"] - 1;
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
            AddVertex(a.x, a.y, a.z); indices[0] = offsets["vertices"] - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = offsets["vertices"] - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = offsets["vertices"] - 1;
            AddVertex(d.x, d.y, d.z); indices[3] = offsets["vertices"] - 1;
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
    }
}
