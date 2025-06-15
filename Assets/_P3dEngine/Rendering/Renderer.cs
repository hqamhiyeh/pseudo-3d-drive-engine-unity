using Assets._P3dEngine;
using Assets._P3dEngine.Interface;
using Assets._P3dEngine.Settings;
using Assets._P3dEngine.Shaders;
using System;
using System.Collections.Generic;
using System.Transactions.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Renderer
    {
        private IRendererSettings   _settings;
        private World               _world;
        private Mesh                _mesh;
        private List<Material>      _materials;

        private MeshGenerator       _meshGenerator;
        private UnityDisplay        _unityDisplay;
        private UnityCamera         _unityCamera;
        [SerializeField][Space(05)] private RenderWindow _fullWindow;
        [SerializeField][Space(05)] private RenderWindow _gameWindow;

        private IShader _shader;
        [SerializeField][Space(05)] private Pseudo3dShaderConfig _pseudo3dShaderConfig;

        public Renderer() { }

        public void Initialize()
        {
            InitMeshGenerator();
            InitUnityComponents();
            InitSettings();
            InitMesh();
            InitMaterials();
            InitSpriteRenderer();
        }

        private void InitUnityComponents()
        {
            _unityDisplay   =   new UnityDisplay( GameObject.FindWithTag("P3dUnityDisplay") );
            _unityCamera    =   new UnityCamera ( GameObject.FindWithTag("P3dUnityCamera")  );
        }

        private void InitSettings()
        {
            SetSettings( EngineSettings.GetNewDefaultSettings() );
        }

        private void InitMesh()
        {
            SetMesh( new Mesh() { indexFormat = IndexFormat.UInt16, name = "Road" } );
        }

        private void InitMaterials()
        {
            List<Material> materials = new()
            {
                (Material)Resources.Load("PrototypeMaterials/Prototype_CustomPseudo3D"),
                //(Material)Resources.Load("PrototypeMaterials/Prototype_Road1"),
                //(Material)Resources.Load("PrototypeMaterials/Prototype_Road2")
            };

            SetMaterials(materials);
        }

        private void InitMeshGenerator()
        {
            _meshGenerator = new MeshGenerator()
            {
                RenderWindow = _gameWindow
            };
        }

        private void InitSpriteRenderer()
        {
            // Create and set road plane sprite for sprite renderer
            Texture2D       roadPlaneTexture    = new(_gameWindow.Width, _gameWindow.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Sprite          roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _gameWindow.Width, _gameWindow.Height), new Vector2(0.5f, 0.5f), _settings.PixelsPerUnit);
            roadPlaneSprite.name = "Road Plane Sprite";
            _unityDisplay.SpriteRenderer.sprite = roadPlaneSprite;
        }

        internal void SetSettings(IRendererSettings settings)
        {
            _settings = settings;
            _settings.SettingChanged += SettingChangedEventHandler;
            ConfigureSettings();
            ApplyAllSettings();
        }

        public void SetWorld(World world)
        {
            _world = world;
            ConfigureWorld();
        }

        private void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
            ConfigureMesh();
        }

        private void SetMaterials(List<Material> materials)
        {
            _materials = materials;
            ConfigureMaterials();
        }

        private void ConfigureSettings()
        {
            _meshGenerator.Settings = _settings;
        }

        private void ConfigureWorld()
        {
            _meshGenerator.World = _world;
        }

        private void ConfigureMesh()
        {
            _meshGenerator.Mesh = _mesh;
            _unityDisplay.MeshFilter.mesh = _mesh;
        }

        private void ConfigureMaterials()
        {
            _meshGenerator.Materials = _materials;
            _unityDisplay.MeshRenderer.SetMaterials(_materials);
        }

        internal void OnStart()
        {
            if (_materials[0].shader.name == "Custom/Pseudo3d") 
            {
                Pseudo3dShaderData pseudo3dShaderData = new()
                {
                    Material            = _materials[0],
                    Camera              = _world.Camera,
                    Window              = _gameWindow,
                    RendererSettings    = _settings
                };
                _shader = new Pseudo3dShader(pseudo3dShaderData, _pseudo3dShaderConfig);
            }
        }

        public void GenerateMesh()
        {
            _meshGenerator.GenerateMesh();
        }

        public void GenerateProjectedMesh()
        {
            _meshGenerator.GenerateProjectedMesh();
        }

        public void UpdateShaderUniforms()
        {
            _shader?.SetUniforms();
        }

        public void DrawToSprite()
        {
            RenderTexture saveActiveRenderTexture = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(_gameWindow.Width, _gameWindow.Height);
            Graphics.SetRenderTarget(renderTexture);

            GL.Clear(false, true, new Color(0.2f, 0.3f, 0.3f, 1.0f));
            GL.PushMatrix();

            Material material;
            for (int submeshIndex = 0; submeshIndex < _mesh.subMeshCount; submeshIndex++)
            {
                material = _materials[submeshIndex];
                material.SetPass(0);
                Graphics.DrawMeshNow(_mesh, Matrix4x4.identity, submeshIndex);
            }
            Graphics.CopyTexture(renderTexture, _unityDisplay.SpriteRenderer.sprite.texture);

            GL.PopMatrix();
            Graphics.SetRenderTarget(saveActiveRenderTexture);
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        private void SetUnityCameraOrthographicSize()
        {
            _unityCamera.Camera.orthographicSize = (float)_fullWindow.Height / (float)_settings.PixelsPerUnit / 2.0f;
        }

        private void SetActiveUnityDisplayRenderer()
        {
            _unityDisplay.MeshRenderer.enabled = !_settings.UseSpriteRenderer;
            _unityDisplay.SpriteRenderer.enabled = _settings.UseSpriteRenderer;
        }

        private void ApplyAllSettings()
        {
            ApplySettings("all");
        }

        private void ApplySettings(string settingName)
        {
            bool all = settingName.ToLower() == "all";
            
            if ( all || settingName == nameof(IRendererSettings.PixelsPerUnit) )
            {
                SetUnityCameraOrthographicSize();
            }

            if ( all || settingName == nameof(IRendererSettings.UseSpriteRenderer) )
            {
                SetActiveUnityDisplayRenderer();
            }
        }

        private void SettingChangedEventHandler(object sender, SettingChangedEventArgs e)
        {
            ApplySettings(e.SettingName);
        }
    }
}
