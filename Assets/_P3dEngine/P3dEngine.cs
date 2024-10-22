//#define PROJECT_MESH_WITH_CPU     // Calculate mesh projection with the CPU instead of GPU shaders

using Assets._P3dEngine;
using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

public class P3dEngine : MonoBehaviour
{
    [System.Serializable]
    private class EngineSettings
    {
        [field: SerializeField] public ApplicationSettings ApplicationSettings  { get; private set; }
        [field: SerializeField] public RendererSettings RendererSettings        { get; private set; }
    }

    [SerializeField] private EngineSettings _settings;
    [field: SerializeField] internal Assets._P3dEngine.Renderer Renderer { get; private set; }
    [field: SerializeField] internal Assets._P3dEngine.Camera Camera { get; private set; }
    

    [System.Serializable]
    private class CpuMaterials
    {
        [SerializeField] public Material RoadMaterialDark;
        [SerializeField] public Material RoadMaterialLight;
    }

    [SerializeField] private CpuMaterials _cpuMaterials;
    
    [System.Serializable]
    private class GpuMaterials
    {
        [SerializeField] public Material PrototypeMaterial;
    }
    
    [SerializeField] private GpuMaterials _gpuMaterials;
    
    private Road _road = new(200, 500, 2000);                           
    private MeshBuilder _meshBuilder;                                   
    private Mesh _mesh;                                                 
    private List<Material> _materials;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        SetSettings();

        Renderer.OnAwake();
        Camera.OnAwake();

        // Set up materials
        _materials = new List<Material>();
#if PROJECT_MESH_WITH_CPU
        GetComponent<Shader>().enabled = false;
        _materials.Add(_cpuMaterials.RoadMaterialDark);
        _materials.Add(_cpuMaterials.RoadMaterialLight);
#else
        _materials.Add(_gpuMaterials.PrototypeMaterial);
#endif

        // Initialize empty mesh & builder
        _mesh = new Mesh() { indexFormat = IndexFormat.UInt16 };
        _meshBuilder = new MeshBuilder(_materials.Count);

        // Set up mesh & materials on renderer
        Renderer.SetMaterials(_materials);
        Renderer.SetMesh(_mesh);
    }

    // Start is called before the first frame update
    void Start()
    {
        Renderer.OnStart();
    }

    // FixedUpdate is called every fixed framerate frame
    private void FixedUpdate()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey("w"))
        //    Camera.Position.Set(Camera.Position.x, Camera.Position.y, Camera.Position.z + 6);
        //if (Input.GetKey("s"))
        //    Camera.Position.Set(Camera.Position.x, Camera.Position.y, Camera.Position.z - 6);

#if PROJECT_MESH_WITH_CPU
        GenerateProjectedMesh();
#else
        GenerateMesh();
#endif

    }

    // OnRenderObject is called after camera has rendered the scene
    private void OnRenderObject()
    {
        if (_settings.RendererSettings.UseSpriteRenderer)
            Renderer.DrawToSprite();
    }

    void SetSettings()
    {
        SetApplicationSettings();
        Renderer.SetSettings(_settings.RendererSettings);
    }

    void SetApplicationSettings()
    {
        Application.targetFrameRate = _settings.ApplicationSettings.TargetFrameRate;
    }

    void GenerateMesh()
    {
        _mesh.Clear();
        _meshBuilder.ResetOffsets();

        int startPointIndex = (int)Math.Floor((double)Camera.Position.z / (double)_road.SegmentLength);
        int drawDistance = _settings.ApplicationSettings.DrawDistance;
        int worldUnitsPerUnit = _settings.ApplicationSettings.WorldUnitsPerUnit;
        for (int n = 0, i = startPointIndex; n < drawDistance && (i + 1) < _road.Points.Count; n++, i++)
        {
            _meshBuilder.AddVertex(_road[i    ].World.x - (_road.Width / 2.0f) / worldUnitsPerUnit, _road[i    ].World.y / worldUnitsPerUnit, _road[i    ].World.z / worldUnitsPerUnit);
            _meshBuilder.AddVertex(_road[i    ].World.x + (_road.Width / 2.0f) / worldUnitsPerUnit, _road[i    ].World.y / worldUnitsPerUnit, _road[i    ].World.z / worldUnitsPerUnit);
            _meshBuilder.AddVertex(_road[i + 1].World.x - (_road.Width / 2.0f) / worldUnitsPerUnit, _road[i + 1].World.y / worldUnitsPerUnit, _road[i + 1].World.z / worldUnitsPerUnit);
            _meshBuilder.AddVertex(_road[i + 1].World.x + (_road.Width / 2.0f) / worldUnitsPerUnit, _road[i + 1].World.y / worldUnitsPerUnit, _road[i + 1].World.z / worldUnitsPerUnit);

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

    //void GenerateProjectedMesh()
    //{
    //    _mesh.Clear();
    //    _meshBuilder.ResetOffsets();

    //    int startPointIndex = (int)Math.Floor((double)Camera.Position.z / (double)_road.SegmentLength);
    //    int drawDistance = _settings._applicationSettings.DrawDistance;
    //    int worldUnitsPerUnit = _settings._applicationSettings.WorldUnitsPerUnit;
    //    for (int n = 0, i = startPointIndex; n < drawDistance && (i + 1) < _road.Points.Count; n++, i++)
    //    {
    //        Renderer.Project(_road[i    ], _road.Width, Camera.Position, Camera.FocalLength, Viewport.Width, Viewport.Height, Viewport.PPU, _useSpriteRenderer == true ? Viewport.Position : Vector3.zero);
    //        Renderer.Project(_road[i + 1], _road.Width, Camera.Position, Camera.FocalLength, Viewport.Width, Viewport.Height, Viewport.PPU, _useSpriteRenderer == true ? Viewport.Position : Vector3.zero);

    //        _meshBuilder.AddVertex(_road[i    ].Transform.x - (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
    //        _meshBuilder.AddVertex(_road[i    ].Transform.x + (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
    //        _meshBuilder.AddVertex(_road[i + 1].Transform.x - (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);
    //        _meshBuilder.AddVertex(_road[i + 1].Transform.x + (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);

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

}
