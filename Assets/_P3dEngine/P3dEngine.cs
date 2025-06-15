//#define PROJECT_MESH_WITH_CPU     // Calculate mesh projection with the CPU instead of GPU shaders

using Assets._P3dEngine;
using Assets._P3dEngine.Interface;
using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[assembly: InternalsVisibleTo("P3dEngineEditor")]

public class P3dEngine : MonoBehaviour, IP3dEngineEditor
{
    [SerializeField][Space(05)] internal EngineSettings _settings;
    [SerializeField][Space(15)] private Assets._P3dEngine.Renderer _renderer;
    [SerializeField][Space(15)] private World _world;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        ApplyEditorValues();
        ApplyApplicationSettings();

        // Generate world
        _world.Generate();

        // Set up renderer
        _renderer.Initialize();
        _renderer.SetSettings(_settings);
        _renderer.SetWorld(_world);
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
        _renderer.GenerateProjectedMesh();
#else
        _renderer.GenerateMesh();
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
            _world.Camera.Position.Set(_world.Camera.Position.x, _world.Camera.Position.y, _world.Camera.Position.z + 6);
        if (Input.GetKey("s"))
            _world.Camera.Position.Set(_world.Camera.Position.x, _world.Camera.Position.y, _world.Camera.Position.z - 6);
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

        if (all || editorValuesGroup == IP3dEngineEditor.EditorValuesGroup.Settings)
            _settings.ApplyEditorValues();

        if (all || editorValuesGroup == IP3dEngineEditor.EditorValuesGroup.Camera)
            _world.Camera.ApplyEditorValues();
    }
}

