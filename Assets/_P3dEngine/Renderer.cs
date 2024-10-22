using Assets._P3dEngine;
using Assets._P3dEngine.Settings;
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
        
        [System.Serializable]
        private class Screen
        {
            [field: SerializeField] public Vector3 Position { get; set; }                           // Unity object that contains all unity renderers
            [field: SerializeField] public int Width { get; set; }                                  // Screen width in pixels
            [field: SerializeField] public int Height { get; set; }                                 // Screen height in pixels
            public float AspectRatio { get; private set; }                                          // Calculated from width/height
            [field: SerializeField] public int PixelsPerUnit { get; set; } = 100;                   // Pixels per Unity unit 
        }

        private RendererSettings _settings;
        [SerializeField] private UnityCamera _unityCamera;
        [SerializeField] private UnityRenderers _unityRenderers;
        [SerializeField] private Screen _screen;
        
        private Mesh _mesh;
        private List<Material> _materials;

        public Renderer() { }

        public void OnAwake()
        {
            // Initialize Unity Camera
            _unityCamera.Camera = _unityCamera.GameObject.GetComponent<UnityEngine.Camera>();
            _unityCamera.Camera.orthographicSize = (float)_screen.Height / (float)_screen.PixelsPerUnit / 2.0f;
            
            // Initialize Unity Renderers
            _unityRenderers.MeshFilter = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.MeshFilter>();
            _unityRenderers.MeshRenderer = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.MeshRenderer>();
            _unityRenderers.SpriteRenderer = _unityRenderers.GameObject.GetComponentInChildren<UnityEngine.SpriteRenderer>();

            // Create and set road plane sprite for sprite renderer        
            Texture2D       roadPlaneTexture    = new(_screen.Width, _screen.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Sprite          roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _screen.Width, _screen.Height), new Vector2(0.5f, 0.5f), _screen.PixelsPerUnit);
            roadPlaneSprite.name = "Road Plane Sprite";
            _unityRenderers.SpriteRenderer.sprite = roadPlaneSprite;
        }

        internal void OnStart()
        {
            if (_settings.UseSpriteRenderer)
            {
                _unityRenderers.MeshRenderer.gameObject.SetActive(false);
                _unityRenderers.SpriteRenderer.gameObject.SetActive(true);
            }
            else
            {
                _unityRenderers.MeshRenderer.gameObject.SetActive(true);
                _unityRenderers.SpriteRenderer.gameObject.SetActive(false);
            }
        }

        internal void SetSettings(RendererSettings settings)
        {
            _settings = settings;
        }

        internal void SetMaterials(List<Material> materials)
        {
            _materials = materials;
            _unityRenderers.MeshRenderer.SetMaterials(materials);
        }

        internal void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
            _unityRenderers.MeshFilter.mesh = mesh;
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

        internal void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength, int screenWidth, int screenHeight, int pixelsPerUnit, Vector3 positionOffset)
        {
            point.View.x = point.World.x - cameraPosition.x;
            point.View.y = point.World.y - cameraPosition.y;
            point.View.z = point.World.z - cameraPosition.z;

            point.Project.x = point.View.x   * (focalLength / point.View.z);
            point.Project.y = point.View.y   * (focalLength / point.View.z);
            point.Project.z = 0.0f;
            point.Project.w = (float)roadWidth * (focalLength / point.View.z);   // w component utilized for road width

            point.Transform.x = point.Project.x * ((float)screenWidth  / 2.0f) / (float)pixelsPerUnit + positionOffset.x;
            point.Transform.y = point.Project.y * ((float)screenHeight / 2.0f) / (float)pixelsPerUnit + positionOffset.y;
            point.Transform.z = point.Project.z + positionOffset.z;
            point.Transform.w = point.Project.w * ((float)screenWidth  / 2.0f) / (float)pixelsPerUnit; // w component utilized for road width
        }

        internal void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength)
        {
            Project(point, roadWidth, cameraPosition, focalLength, _screen.Width, _screen.Height, _screen.PixelsPerUnit, _screen.Position);
        }
    }
}
