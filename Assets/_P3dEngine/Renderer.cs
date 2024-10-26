using Assets._P3dEngine;
using Assets._P3dEngine.Settings;
using Assets._P3dEngine.Shaders;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private class UnityRenderers
        {
            [field: SerializeField] public GameObject GameObject { get; private set; }
            public UnityEngine.MeshFilter MeshFilter { get; set; }                      
            public UnityEngine.MeshRenderer MeshRenderer { get; set; }                  
            public UnityEngine.SpriteRenderer SpriteRenderer { get; set; }              
        }

        private RendererSettings _settings;
        [SerializeField] private UnityCamera _unityCamera;
        [SerializeField] private UnityRenderers _unityRenderers;
        [SerializeField] private Screen _screen;

        [SerializeField] private Pseudo3dShaderData _p3dShaderData;
        private IShader _shader;
        private List<Material> _materials;
        private Mesh _mesh;
        private World _world;


        public Renderer() { }

        public void Initialize(RendererSettings settings)
        {
            /* Initialize settings */
            SetSettings(settings);

            /* Initialize Unity Camera */
            _unityCamera.Camera = _unityCamera.GameObject.GetComponent<UnityEngine.Camera>();
            _unityCamera.Camera.orthographicSize = (float)_screen.Height / (float)_screen.PixelsPerUnit / 2.0f;
            
            /* Initialize Unity Renderers */
            // Init Mesh Filter
            _unityRenderers.MeshFilter = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.MeshFilter>();
            
            // Init Mesh Renderer
            _unityRenderers.MeshRenderer = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.MeshRenderer>();
            _unityRenderers.MeshRenderer.gameObject.SetActive(!_settings.UseSpriteRenderer);

            // Init Sprite Renderer
            _unityRenderers.SpriteRenderer = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.SpriteRenderer>();
            _unityRenderers.SpriteRenderer.gameObject.SetActive(_settings.UseSpriteRenderer);
            // Create and set road plane sprite for sprite renderer
            Texture2D       roadPlaneTexture    = new(_screen.Width, _screen.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Sprite          roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _screen.Width, _screen.Height), new Vector2(0.5f, 0.5f), _screen.PixelsPerUnit);
            roadPlaneSprite.name = "Road Plane Sprite";
            _unityRenderers.SpriteRenderer.sprite = roadPlaneSprite;
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

        internal void SetSettings(RendererSettings settings)
        {
            _settings = settings;
        }

        internal void SetMaterials(List<Material> materials)
        {
            _materials = materials;
            _unityRenderers.MeshRenderer.SetMaterials(_materials);
        }

        internal void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
            _unityRenderers.MeshFilter.mesh = _mesh;
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
            Graphics.CopyTexture(renderTexture, _unityRenderers.SpriteRenderer.sprite.texture);

            GL.PopMatrix();
            Graphics.SetRenderTarget(saveActiveRenderTexture);
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}
