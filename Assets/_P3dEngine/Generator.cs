using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Rendering;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Generator
    {
        private GeneratorSettings _settings;
        private MeshBuilder _meshBuilder;
        private Mesh _mesh;
        private World _world;

        public Generator() { }
        
        internal void OnAwake()
        {
            
        }

        internal void SetSettings(GeneratorSettings settings)
        {
            _settings = settings;
        }

        //internal void SetCamera(Camera camera)
        //{
        //    _camera = camera;
        //}

        internal void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
        }

        internal void SetWorld(World world)
        {
            _world = world;
        }

        private void InitMeshBuilder(int materialsCount)
        {
            _meshBuilder = new MeshBuilder(materialsCount);
        }

        internal void Initialize(GeneratorSettings settings, int materialsCount)
        {
            SetSettings(settings);
            InitMeshBuilder(materialsCount);
        }

        internal void GenerateWorld()
        {
            Road road = new(200, 500, 2000);
            _world.SetRoad(road);
        }

        internal void GenerateMesh()
        {
            _mesh.Clear();
            _meshBuilder.ResetOffsets();

            int startPointIndex = (int)Math.Floor((double)_world.Camera.Position.z / (double)_world.Road.SegmentLength);
            int drawDistance = _settings.DrawDistance;
            int worldUnitsPerUnit = _settings.WorldUnitsPerUnit;
            for (int n = 0, i = startPointIndex; n < drawDistance && (i + 1) < _world.Road.Points.Count; n++, i++)
            {
                _meshBuilder.AddVertex(_world.Road[i    ].World.x - (_world.Road.Width / 2.0f) / worldUnitsPerUnit, _world.Road[i    ].World.y / worldUnitsPerUnit, _world.Road[i    ].World.z / worldUnitsPerUnit);
                _meshBuilder.AddVertex(_world.Road[i    ].World.x + (_world.Road.Width / 2.0f) / worldUnitsPerUnit, _world.Road[i    ].World.y / worldUnitsPerUnit, _world.Road[i    ].World.z / worldUnitsPerUnit);
                _meshBuilder.AddVertex(_world.Road[i + 1].World.x - (_world.Road.Width / 2.0f) / worldUnitsPerUnit, _world.Road[i + 1].World.y / worldUnitsPerUnit, _world.Road[i + 1].World.z / worldUnitsPerUnit);
                _meshBuilder.AddVertex(_world.Road[i + 1].World.x + (_world.Road.Width / 2.0f) / worldUnitsPerUnit, _world.Road[i + 1].World.y / worldUnitsPerUnit, _world.Road[i + 1].World.z / worldUnitsPerUnit);

                _meshBuilder.AddUV(0, 0);
                _meshBuilder.AddUV(1, 0);
                _meshBuilder.AddUV(0, 1);
                _meshBuilder.AddUV(1, 1);

                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);

                _meshBuilder.AddTriangle(n * 4    , n * 4 + 2, n * 4 + 1);
                _meshBuilder.AddTriangle(n * 4 + 1, n * 4 + 2, n * 4 + 3);
            }

            _meshBuilder.ToMesh(_mesh);
        }

        //internal void GenerateProjectedMesh()
        //{
        //    _mesh.Clear();
        //    _meshBuilder.ResetOffsets();

        //    int startPointIndex = (int)Math.Floor((double)_camera.Position.z / (double)_road.SegmentLength);
        //    int drawDistance = _settings.DrawDistance;
        //    int worldUnitsPerUnit = _settings.WorldUnitsPerUnit;
        //    for (int n = 0, i = startPointIndex; n < drawDistance && (i + 1) < _road.Points.Count; n++, i++)
        //    {
        //        Project(_road[i], _road.Width, _camera.Position, _camera.FocalLength);
        //        Project(_road[i + 1], _road.Width, _camera.Position, _camera.FocalLength);

        //        _meshBuilder.AddVertex(_road[i].Screen.x - (_road[i].Screen.w / 2.0f), _road[i].Screen.y, _road[i].Screen.z);
        //        _meshBuilder.AddVertex(_road[i].Screen.x + (_road[i].Screen.w / 2.0f), _road[i].Screen.y, _road[i].Screen.z);
        //        _meshBuilder.AddVertex(_road[i + 1].Screen.x - (_road[i + 1].Screen.w / 2.0f), _road[i + 1].Screen.y, _road[i + 1].Screen.z);
        //        _meshBuilder.AddVertex(_road[i + 1].Screen.x + (_road[i + 1].Screen.w / 2.0f), _road[i + 1].Screen.y, _road[i + 1].Screen.z);

        //        // This is a not so good attempt at calculating UVs after projection, do not rely on these UVs for perspective correct textures.
        //        _meshBuilder.AddUV(0, (i) % 2 == 0 ? 0 : 1);
        //        _meshBuilder.AddUV(1, (i) % 2 == 0 ? 0 : 1);
        //        _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 2).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);
        //        _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 3).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);

        //        _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
        //        _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
        //        _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
        //        _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);

        //        _meshBuilder.AddTriangle(n * 4, n * 4 + 2, n * 4 + 1, i % 2 == 0 ? 0 : 1);
        //        _meshBuilder.AddTriangle(n * 4 + 1, n * 4 + 2, n * 4 + 3, i % 2 == 0 ? 0 : 1);
        //    }

        //    _meshBuilder.ToMesh(_mesh);
        //}

        //private void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength, int screenWidth, int screenHeight, int pixelsPerUnit, Vector3 positionOffset)
        //{
        //    point.View.x = point.World.x - cameraPosition.x;
        //    point.View.y = point.World.y - cameraPosition.y;
        //    point.View.z = point.World.z - cameraPosition.z;

        //    point.Project.x = point.View.x   * (focalLength / point.View.z);
        //    point.Project.y = point.View.y   * (focalLength / point.View.z);
        //    point.Project.z = 0.0f;
        //    point.Project.w = (float)roadWidth * (focalLength / point.View.z);                          // w component utilized for road width

        //    point.Screen.x = point.Project.x * ((float)screenWidth  / 2.0f) / (float)pixelsPerUnit + positionOffset.x;
        //    point.Screen.y = point.Project.y * ((float)screenHeight / 2.0f) / (float)pixelsPerUnit + positionOffset.y;
        //    point.Screen.z = point.Project.z + positionOffset.z;
        //    point.Screen.w = point.Project.w * ((float)screenWidth  / 2.0f) / (float)pixelsPerUnit;     // w component utilized for road width
        //}

        //private void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength)
        //{
        //    Project(point, roadWidth, cameraPosition, focalLength, _screen.Width, _screen.Height, _screen.PixelsPerUnit, _settings.UseSpriteRenderer == true ? _screen.Position : Vector3.zero);
        //}
    }
}
