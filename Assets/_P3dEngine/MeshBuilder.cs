using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets._P3dEngine
{
    internal class MeshBuilder
    {
        private const int INITIAL_ARRAY_CAPACITY_VERTICES     = 100;
        private const int INITIAL_ARRAY_CAPACITY_NORMALS      = 100;
        private const int INITIAL_ARRAY_CAPACITY_UVS          = 100;
        private const int INITIAL_ARRAY_CAPACITY_TRIANGLES    = 100;

        private Vector3[]   _vertices;
        private Vector3[]   _normals;
        private Vector2[]   _uvs;
        private readonly List<int[]> _triangles;
        private readonly Dictionary<string,int> _offsets;
        
        public MeshBuilder(int submeshCount = 1)
        {
            _vertices    = new Vector3[INITIAL_ARRAY_CAPACITY_VERTICES];
            _normals     = new Vector3[INITIAL_ARRAY_CAPACITY_NORMALS];
            _uvs         = new Vector2[INITIAL_ARRAY_CAPACITY_UVS];
            _triangles   = new List<int[]>() { new int[INITIAL_ARRAY_CAPACITY_TRIANGLES] };
            _offsets     = new Dictionary<string, int>()
            {
                { "vertices"    , 0 },
                { "normals"     , 0 },
                { "uvs"         , 0 },
                { "triangles0"  , 0 },
            };
            for (int i = 1; i < submeshCount; i++)
                AddSubmesh();
        }

        public IReadOnlyList<Vector3>    Vertices    { get => (_offsets["vertices"]  == 0 ? Array.Empty<Vector3>() : _vertices   [0..(_offsets["vertices"]   - 1)]); }
        public IReadOnlyList<Vector3>    Normals     { get => _offsets["normals"]   == 0 ? Array.Empty<Vector3>() : _normals    [0..(_offsets["normals"]    - 1)]; }
        public IReadOnlyList<Vector2>    UVs         { get => _offsets["uvs"]       == 0 ? Array.Empty<Vector2>() : _uvs        [0..(_offsets["uvs"]        - 1)]; }
        public IReadOnlyDictionary<string, IReadOnlyList<int>> Triangles
        { 
            get
            {
                Dictionary<string, IReadOnlyList<int>> triangles = new();
                for (int submeshIndex = 0; submeshIndex < _triangles.Count; submeshIndex++)
                    triangles.Add("triangles" + submeshIndex, _offsets["triangles" + submeshIndex] == 0 ? Array.Empty<int>() : _triangles[submeshIndex][0..(_offsets[("triangles" + submeshIndex)] - 1)]);
                return triangles;
            }
        }

        public Vector3 GetVertex(int verticesIndex) { return _vertices[verticesIndex]; }

        public void AddSubmesh()
        {
            _triangles.Add(new int[INITIAL_ARRAY_CAPACITY_TRIANGLES]);
            _offsets.Add("triangles" + (_triangles.Count - 1), 0);
        }

        public void AddVertex(float x, float y, float z)
        {
            if (_offsets["vertices"] >= _vertices.Length)
                Array.Resize(ref _vertices, _vertices.Length * 2);
            
            _vertices[_offsets["vertices"]++].Set(x, y, z);
        }

        public void AddNormal(float x, float y, float z)
        {
            if (_offsets["normals"] >= _normals.Length)
                Array.Resize(ref _normals, _normals.Length * 2);
            
            _normals[_offsets["normals"]++].Set(x, y, z);
        }

        public void AddUV(float u, float v)
        {
            if (_offsets["uvs"] >= _uvs.Length)
                Array.Resize(ref _uvs, _uvs.Length * 2);
            
            _uvs[_offsets["uvs"]++].Set(u, v);
        }

        public void AddTriangle(int a, int b, int c, int submeshIndex = 0)
        {
            string s = "triangles" + submeshIndex;
            int[] submesh = _triangles[submeshIndex];
            
            if (_offsets[s] + 2 >= submesh.Length)
            {
                Array.Resize<int>(ref submesh, submesh.Length * 2);
                _triangles[submeshIndex] = submesh;
            }
            
            _triangles[submeshIndex][_offsets[s]++] = a;
            _triangles[submeshIndex][_offsets[s]++] = b;
            _triangles[submeshIndex][_offsets[s]++] = c;
        }

        public void ToMesh(Mesh mesh)
        {
            mesh.vertices = null;
            mesh.normals = null;
            mesh.uv = null;
            mesh.triangles = null;
            mesh.subMeshCount = _triangles.Count;
            mesh.SetVertices(_vertices, 0, _offsets["vertices"]);
            mesh.SetNormals(_normals, 0, _offsets["normals"]);
            mesh.SetUVs(0, _uvs, 0, _offsets["uvs"]);
            for (int i = 0; i < _triangles.Count; i++)
                mesh.SetTriangles(_triangles[i], 0, _offsets["triangles" + i], i, true, 0);
        }

        public void ResetOffsets()
        {
            _offsets["vertices"] = 0;
            _offsets["normals"] = 0;
            _offsets["uvs"] = 0;
            _offsets["triangles0"] = 0;
            for (int i = 1; i < _triangles.Count; i++)
                _offsets["triangles" + i] = 0;
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, int submeshIndex)
        {
            int[] indices = new int[3];
            AddVertex(a.x, a.y, a.z); indices[0] = _offsets["vertices"] - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = _offsets["vertices"] - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = _offsets["vertices"] - 1;
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
            AddVertex(a.x, a.y, a.z); indices[0] = _offsets["vertices"] - 1;
            AddVertex(b.x, b.y, b.z); indices[1] = _offsets["vertices"] - 1;
            AddVertex(c.x, c.y, c.z); indices[2] = _offsets["vertices"] - 1;
            AddVertex(d.x, d.y, d.z); indices[3] = _offsets["vertices"] - 1;
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
