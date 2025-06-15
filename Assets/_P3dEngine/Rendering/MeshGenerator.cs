using Assets._P3dEngine.Settings;
using PlasticPipe.PlasticProtocol.Messages;
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
    internal class MeshGenerator
    {
        private IRendererSettings   _settings;
        private World               _world;
        private Mesh                _mesh;
        private List<Material>      _materials;
        private RenderWindow        _renderWindow;

        public IRendererSettings   Settings         { get => _settings;     set =>  _settings       = value; }
        public World               World            { get => _world;        set =>  _world          = value; }
        public Mesh                Mesh             { get => _mesh;         set =>  _mesh           = value; }
        public List<Material>      Materials        { get => _materials; set { _materials = value; ConfigureMeshBuilder(); } }
        public RenderWindow        RenderWindow     { get => _renderWindow; set =>  _renderWindow   = value; }
        
        private readonly MeshBuilder _meshBuilder;

        public MeshGenerator()
        {
            _meshBuilder = new MeshBuilder();
        }

        private void ConfigureMeshBuilder()
        {
            _meshBuilder.SetSubmeshCount(_materials.Count);
        }

        public void GenerateMesh()
        {
            _mesh.Clear();
            _meshBuilder.ResetOffsets();

            double cameraPositionZ = (double)_world.Camera.Position.z;
            int startPointIndex = (int)Math.Floor( (cameraPositionZ >= 0 ? cameraPositionZ : 0) / (double)_world.Road.SegmentLength );
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

        public void GenerateProjectedMesh()
        {
            _mesh.Clear();
            _meshBuilder.ResetOffsets();

            double cameraPositionZ = (double)_world.Camera.Position.z;
            int startPointIndex = (int)Math.Floor( (cameraPositionZ >= 0 ? cameraPositionZ : 0) / (double)_world.Road.SegmentLength );
            int drawDistance = _settings.DrawDistance;
            int worldUnitsPerUnit = _settings.WorldUnitsPerUnit;
            for (int n = 0, i = startPointIndex; n < drawDistance && (i + 1) < _world.Road.Points.Count; n++, i++)
            {   
                Project(_world.Road[i    ], _world.Road.Width, _world.Camera.Position, _world.Camera.FocalLength);
                Project(_world.Road[i + 1], _world.Road.Width, _world.Camera.Position, _world.Camera.FocalLength);

                _meshBuilder.AddVertex(_world.Road[i    ].Screen.x - (_world.Road[i    ].Screen.w / 2.0f), _world.Road[i    ].Screen.y, _world.Road[i    ].Screen.z);
                _meshBuilder.AddVertex(_world.Road[i    ].Screen.x + (_world.Road[i    ].Screen.w / 2.0f), _world.Road[i    ].Screen.y, _world.Road[i    ].Screen.z);
                _meshBuilder.AddVertex(_world.Road[i + 1].Screen.x - (_world.Road[i + 1].Screen.w / 2.0f), _world.Road[i + 1].Screen.y, _world.Road[i + 1].Screen.z);
                _meshBuilder.AddVertex(_world.Road[i + 1].Screen.x + (_world.Road[i + 1].Screen.w / 2.0f), _world.Road[i + 1].Screen.y, _world.Road[i + 1].Screen.z);

                // This is a not so good attempt at calculating UVs after projection, do not rely on these UVs for perspective correct textures.
                _meshBuilder.AddUV(0, (i) % 2 == 0 ? 0 : 1);
                _meshBuilder.AddUV(1, (i) % 2 == 0 ? 0 : 1);
                _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 2).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);
                _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 3).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);

                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
                _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);

                _meshBuilder.AddTriangle(n * 4 + 0, n * 4 + 2, n * 4 + 1, i % 2 == 0 ? 0 : 1);
                _meshBuilder.AddTriangle(n * 4 + 1, n * 4 + 2, n * 4 + 3, i % 2 == 0 ? 0 : 1);
            }

            _meshBuilder.ToMesh(_mesh);
        }

        private void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength, int windowWidth, int windowHeight, int pixelsPerUnit, Vector3 positionOffset)
        {
            point.View.x = point.World.x - cameraPosition.x;
            point.View.y = point.World.y - cameraPosition.y;
            point.View.z = point.World.z - cameraPosition.z;

            point.Project.x = point.View.x * (focalLength / point.View.z);
            point.Project.y = point.View.y * (focalLength / point.View.z);
            point.Project.z = 0.0f;
            point.Project.w = (float)roadWidth * (focalLength / point.View.z);                          // w component utilized for road width

            point.Screen.x = point.Project.x * ((float)windowWidth  / 2.0f) / (float)pixelsPerUnit + positionOffset.x;
            point.Screen.y = point.Project.y * ((float)windowHeight / 2.0f) / (float)pixelsPerUnit + positionOffset.y;
            point.Screen.z = point.Project.z + positionOffset.z;
            point.Screen.w = point.Project.w * ((float)windowWidth / 2.0f) / (float)pixelsPerUnit;      // w component utilized for road width
        }

        private void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength)
        {
            Project(point, roadWidth, cameraPosition, focalLength, _renderWindow.Width, _renderWindow.Height, _settings.PixelsPerUnit, _settings.UseSpriteRenderer == true ? _renderWindow.Position : Vector3.zero);
        }

    }
}
