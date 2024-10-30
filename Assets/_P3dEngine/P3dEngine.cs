//#define PROJECT_MESH_WITH_CPU     // Calculate mesh projection with the CPU instead of GPU shaders

using Assets._P3dEngine;
using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: InternalsVisibleTo("P3dEngineEditor")]

internal interface IP3dEngineEditor
{
    enum EditorValuesGroup
    {
        All,
        Camera,
        RendererSettings
    }

    void ApplyEditorValues(EditorValuesGroup editorValuesGroup);
}

public class P3dEngine : MonoBehaviour, IP3dEngineEditor
{
    [SerializeField] private Settings _settings;
    [SerializeField] private PrototypeMaterials _prototypeMaterials;
    [SerializeField] private Generator _generator;
    [SerializeField] private Assets._P3dEngine.Renderer _renderer;
    [field: SerializeField] internal World World { get; private set; }
    
    private Mesh _mesh;
    private List<Material> _materials;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        ApplyEditorValues();
        ApplyApplicationSettings();

        // Initialize empty mesh
        _mesh = new Mesh() { indexFormat = IndexFormat.UInt16 };
        _mesh.name = "Road";

        // Set up materials
#if PROJECT_MESH_WITH_CPU
        _materials = new List<Material>();
        _materials.Add(_prototypeMaterials.Cpu.RoadDark);
        _materials.Add(_prototypeMaterials.Cpu.RoadLight);
#else
        _materials = new List<Material>();
        _materials.Add(_prototypeMaterials.Gpu.CustomPseudo3d);
#endif

        // Set up generator
        _generator.Initialize((IGeneratorSettings)_settings, _materials.Count);
        _generator.SetMesh(_mesh);
        _generator.SetWorld(World);
        _generator.GenerateWorld();

        // Set up renderer
        _renderer.Initialize((IRendererSettings)_settings);
        _renderer.SetMaterials(_materials);
        _renderer.SetMesh(_mesh);
        _renderer.SetWorld(World);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _renderer.OnStart();
    }

    // FixedUpdate is called every fixed framerate frame
    void FixedUpdate()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        ProcessInput();

#if PROJECT_MESH_WITH_CPU
        GenerateProjectedMesh();
#else
        _generator.GenerateMesh();
        _renderer.UpdateShaderUniforms();
#endif
    }

    // OnRenderObject is called after camera has rendered the scene
    void OnRenderObject()
    {
        if (_settings.UseSpriteRenderer)
            _renderer.DrawToSprite();
    }

    private void ProcessInput()
    {
        if (Input.GetKey("w"))
            World.Camera.Position.Set(World.Camera.Position.x, World.Camera.Position.y, World.Camera.Position.z + 6);
        if (Input.GetKey("s"))
            World.Camera.Position.Set(World.Camera.Position.x, World.Camera.Position.y, World.Camera.Position.z - 6);
    }

    private void PollSettings()
    {

    }

    private void ApplyApplicationSettings()
    {
        IApplicationSettings applicationSettings = _settings;
        Application.targetFrameRate = applicationSettings.TargetFrameRate;
    }

    private void ApplyEditorValues()
    {
        ((IP3dEngineEditor)this).ApplyEditorValues(IP3dEngineEditor.EditorValuesGroup.All);
    }

    void IP3dEngineEditor.ApplyEditorValues(IP3dEngineEditor.EditorValuesGroup editorValuesGroup)
    {
        bool all = editorValuesGroup == IP3dEngineEditor.EditorValuesGroup.All;

        if (all || editorValuesGroup == IP3dEngineEditor.EditorValuesGroup.Camera)
            World.Camera.ApplyEditorValues();

        if (all || editorValuesGroup == IP3dEngineEditor.EditorValuesGroup.RendererSettings)
            _settings.ApplyEditorValues();
    }
}

