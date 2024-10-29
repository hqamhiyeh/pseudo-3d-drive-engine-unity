using Assets._P3dEngine.Settings;
using Assets._P3dEngine.Shaders;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Renderer
    {
        [System.Serializable]
        private class UnityCamera
        {
            [field: SerializeField] public GameObject GameObject { get; private set; }
            public UnityEngine.Camera Camera { get; set; }                                          // Unity camera that's pointed at screen
            public PixelPerfectCamera PixelPerfectCamera { get; set; }
        }

        [System.Serializable]
        private class UnityDisplay
        {
            [field: SerializeField] public GameObject GameObject { get; private set; }
            public UnityEngine.MeshFilter MeshFilter { get; set; }                      
            public UnityEngine.MeshRenderer MeshRenderer { get; set; }                  
            public UnityEngine.SpriteRenderer SpriteRenderer { get; set; }              
        }

        private RendererSettings _settings;
        [SerializeField] private UnityCamera _unityCamera;
        [SerializeField] private UnityDisplay _unityDisplay;
        [SerializeField] private Screen _screen;

        [SerializeField] private Pseudo3dShaderData _p3dShaderData;
        private IShader _shader;
        private List<Material> _materials;
        private Mesh _mesh;
        private World _world;

        public Renderer() { }

        public void Initialize(RendererSettings settings)
        {
            /* Initialize Unity Camera */
            _unityCamera.Camera = _unityCamera.GameObject.GetComponent<UnityEngine.Camera>();
            _unityCamera.Camera.orthographicSize = (float)_screen.Height / (float)_screen.PixelsPerUnit / 2.0f;
            
            /* Initialize Unity Display */
            _unityDisplay.MeshFilter = _unityDisplay.GameObject.GetComponentInChildren<UnityEngine.MeshFilter>();
            _unityDisplay.MeshRenderer = _unityDisplay.GameObject.GetComponentInChildren<UnityEngine.MeshRenderer>();
            _unityDisplay.SpriteRenderer = _unityDisplay.GameObject.GetComponentInChildren<UnityEngine.SpriteRenderer>();
            InitSpriteRenderer();

            /* Initialize settings */
            SetSettings(settings);
            _settings.SettingChanged += RefreshSettings;
        }

        private void InitSpriteRenderer()
        {
            // Create and set road plane sprite for sprite renderer
            Texture2D       roadPlaneTexture    = new(_screen.Width, _screen.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Sprite          roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _screen.Width, _screen.Height), new Vector2(0.5f, 0.5f), _screen.PixelsPerUnit);
            roadPlaneSprite.name = "Road Plane Sprite";
            _unityDisplay.SpriteRenderer.sprite = roadPlaneSprite;
        }

        internal void OnStart()
        {
            if (_materials[0].shader.name == "Custom/Pseudo3d")
            {
                Pseudo3dShaderData p3dsd = _p3dShaderData;
                p3dsd.Material                     = _materials[0];
                p3dsd.Camera                       = _world.Camera;
                p3dsd.Screen                       = _screen;
                p3dsd.UseSpriteRenderer            = _settings.UseSpriteRenderer;
                _shader = new Pseudo3dShader(p3dsd);
            }
        }

        internal void UpdateShaderUniforms()
        {
            _shader?.SetUniforms();
        }

        private void ApplySettings()
        {
            _unityDisplay.MeshRenderer.enabled = !_settings.UseSpriteRenderer;
            _unityDisplay.SpriteRenderer.enabled = _settings.UseSpriteRenderer;
        }
        
        private void RefreshSettings()
        {
            ApplySettings();
            Debug.Log("[Renderer] INFO: Settings refreshed.");
        }

        internal void SetSettings(RendererSettings settings)
        {
            _settings = settings;
            ApplySettings();
        }

        internal void SetMaterials(List<Material> materials)
        {
            _materials = materials;
            _unityDisplay.MeshRenderer.SetMaterials(_materials);
        }

        internal void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
            _unityDisplay.MeshFilter.mesh = _mesh;
        }

        internal void SetWorld(World world)
        {
            _world = world;
        }

        internal void DrawToSprite()
        {
            RenderTexture saveActiveRenderTexture = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(_screen.Width, _screen.Height);
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
    }
}
