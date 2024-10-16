//#define PROJECT_MESH_WITH_CPU     // Calculate mesh projection with the CPU instead of GPU shaders

using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class DriveEngine : MonoBehaviour
{
    [SerializeField] private UnityEngine.Camera     _unityCamera;           // Unity camera that's pointed at screen
    [SerializeField] private GameObject             _unityScreen;           // Screen object where the game will be displayed

    private GameObject      _roadPlane;
    private MeshFilter      _roadPlaneMF;
    private MeshRenderer    _roadPlaneMR;
    private SpriteRenderer  _roadPlaneSR;

    [SerializeField] private Boolean _useSpriteRenderer = false;            // True enables sprite renderer & disables mesh renderer. False for vice versa
    [SerializeField] private int _targetFrameRate = -1;                     // Frame rate limit. -1 is unlimited.

    [System.Serializable]
    public class ViewportSettings
    {
        public Vector3 Position { get; set; }                               // Viewport position in unity
        [field: SerializeField] public int Width    { get; private set; }   // Viewport width in pixels
        [field: SerializeField] public int Height   { get; private set; }   // Viewport height in pixels
        [field: SerializeField] public int PPU      { get; private set; }   // Viewport pixels per unit
    }
    [field: SerializeField] public ViewportSettings Viewport { get; private set; }
    [field: SerializeField] public Assets.Scripts.Camera Camera { get; private set; }

    //private readonly List<RoadPoint> _road = new();                     // All road points
    private Road _road = new(200, 500, 2000);

    private Mesh _mesh;                                                 // The mesh to render, constantly changing.
    private MeshBuilder _meshBuilder;                                   // Builder to create meshes

    [SerializeField] private Material _roadMaterialDark;                // Dark color for road mesh
    [SerializeField] private Material _roadMaterialLight;               // Light color for road mesh
    private List<Material> _materials = new();                           // Materials to apply to meshes

    [SerializeField]
    private int _drawDistance                   = 200;                  // number of segments to draw

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Set frame rate
        Application.targetFrameRate = _targetFrameRate;
        
        // Set GameObjects
        _roadPlane      = _unityScreen.transform.Find("Road Plane").gameObject;
        _roadPlaneMF    = _roadPlane.GetComponentInChildren<MeshFilter>();
        _roadPlaneMR    = _roadPlane.GetComponentInChildren<MeshRenderer>();
        _roadPlaneSR    = _roadPlane.GetComponentInChildren<SpriteRenderer>();

        // Set settings
        Viewport.Position = _unityScreen.transform.position;
        Camera.OnAwake();

        // Set Drive scene objects' positions and camera 
       
        _unityCamera.transform.position.Set(Viewport.Position.x, Viewport.Position.y, Viewport.Position.z - 1);
        _unityCamera.orthographicSize = (float)Viewport.Height / (float)Viewport.PPU / 2.0f;

        // Create and set road plane sprite for sprite renderer        
        Texture2D       roadPlaneTexture    = new(Viewport.Width, Viewport.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Sprite          roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, Viewport.Width, Viewport.Height), new Vector2(0.5f, 0.5f), Viewport.PPU);
        roadPlaneSprite.name = "Road Plane Sprite";
        _roadPlaneSR.sprite = roadPlaneSprite;
        
        // Set pov camera distance (normalized) from viewport
        //_focalLength = (float)(1 / Math.Tan((Camera.FOV / 2.0) * Math.PI / 180));

        // Set up materials
#if PROJECT_MESH_WITH_CPU
        GetComponent<Shader>().enabled = false;
        _materials.Add(_roadMaterialDark);
        _materials.Add(_roadMaterialLight);
        _roadPlaneMR.SetMaterials(_materials);
#else
        Material prototypeMaterial = Resources.Load("Prototype_Pseudo3D", typeof(Material)) as Material;
        _materials.Add(prototypeMaterial);
        _roadPlaneMR.SetMaterials(_materials);
#endif

        // Initialize empty mesh & builder
        _mesh = new Mesh() { indexFormat = IndexFormat.UInt16 };
        _meshBuilder = new MeshBuilder(_materials.Count);

        // Set up road mesh renderer
        _roadPlaneMF.mesh = _mesh;

    }

    // Start is called before the first frame update
    void Start()
    {
        if (_useSpriteRenderer)
        {
            _roadPlaneMR.gameObject.SetActive(false);
            _roadPlaneSR.gameObject.SetActive(true);
        }
        else
        {
            _roadPlaneMR.gameObject.SetActive(true);
            _roadPlaneSR.gameObject.SetActive(false);
        }

        //GenerateRoad();

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
        if (_useSpriteRenderer)
            Draw();
    }
    
    //void GenerateRoad()
    //{
    //    _road.Clear();
    //    for (int i = 0; i < (_segmentCount + 1); i++)
    //        _road.Add(new RoadPoint(new Vector3(0.0f, 0.0f, (i + 1) * _segmentLength)));
    //}

    void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float focalLength, int viewportWidth, int viewportHeight, int pixelsPerUnit, Vector3 positionOffset)
    {
        point.View.x = point.World.x - cameraPosition.x;
        point.View.y = point.World.y - cameraPosition.y;
        point.View.z = point.World.z - cameraPosition.z;

        point.Project.x = point.View.x   * (focalLength / point.View.z);
        point.Project.y = point.View.y   * (focalLength / point.View.z);
        point.Project.z = 0.0f;
        point.Project.w = (float)roadWidth * (focalLength / point.View.z);   // w component utilized for road width

        point.Transform.x = point.Project.x * ((float)viewportWidth  / 2.0f) / (float)pixelsPerUnit + positionOffset.x;
        point.Transform.y = point.Project.y * ((float)viewportHeight / 2.0f) / (float)pixelsPerUnit + positionOffset.y;
        point.Transform.z = point.Project.z + positionOffset.z;
        point.Transform.w = point.Project.w * ((float)viewportWidth  / 2.0f) / (float)pixelsPerUnit; // w component utilized for road width
    }

    private void Draw()
    {
        RenderTexture saveActiveRenderTexture = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(Viewport.Width, Viewport.Height);
        Graphics.SetRenderTarget(renderTexture);

        GL.Clear(false, true, new Color(0.2f, 0.3f, 0.3f, 1.0f));
        GL.PushMatrix();
        
        Material material;
        for(int submeshIndex = 0; submeshIndex < _mesh.subMeshCount; submeshIndex++)
        {
            material = _materials[submeshIndex];
            material.SetPass(0);
            Graphics.DrawMeshNow(_mesh, Matrix4x4.identity, submeshIndex);
        }
        Graphics.CopyTexture(renderTexture, _roadPlaneSR.sprite.texture); 
        
        GL.PopMatrix();
        Graphics.SetRenderTarget(saveActiveRenderTexture);
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    private void GenerateMesh()
    {
        _mesh.Clear();
        _meshBuilder.ResetOffsets();

        int startPointIndex = (int)Math.Floor((double)Camera.Position.z / (double)_road.SegmentLength);
        for (int n = 0, i = startPointIndex; n < _drawDistance && (i + 1) < _road.Points.Count; n++, i++)
        {
            _meshBuilder.AddVertex(_road[i    ].World.x - (_road.Width / 2.0f) / Viewport.PPU, _road[i    ].World.y / Viewport.PPU, _road[i    ].World.z / Viewport.PPU);
            _meshBuilder.AddVertex(_road[i    ].World.x + (_road.Width / 2.0f) / Viewport.PPU, _road[i    ].World.y / Viewport.PPU, _road[i    ].World.z / Viewport.PPU);
            _meshBuilder.AddVertex(_road[i + 1].World.x - (_road.Width / 2.0f) / Viewport.PPU, _road[i + 1].World.y / Viewport.PPU, _road[i + 1].World.z / Viewport.PPU);
            _meshBuilder.AddVertex(_road[i + 1].World.x + (_road.Width / 2.0f) / Viewport.PPU, _road[i + 1].World.y / Viewport.PPU, _road[i + 1].World.z / Viewport.PPU);

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

    private void GenerateProjectedMesh()
    {
        _mesh.Clear();
        _meshBuilder.ResetOffsets();

        int startPointIndex = (int)Math.Floor((double)Camera.Position.z / (double)_road.SegmentLength);
        for (int n = 0, i = startPointIndex; n < _drawDistance && (i + 1) < _road.Points.Count; n++, i++)
        {
            Project(_road[i    ], _road.Width, Camera.Position, Camera.FocalLength, Viewport.Width, Viewport.Height, Viewport.PPU, _useSpriteRenderer == true ? Viewport.Position : Vector3.zero);
            Project(_road[i + 1], _road.Width, Camera.Position, Camera.FocalLength, Viewport.Width, Viewport.Height, Viewport.PPU, _useSpriteRenderer == true ? Viewport.Position : Vector3.zero);

            _meshBuilder.AddVertex(_road[i    ].Transform.x - (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
            _meshBuilder.AddVertex(_road[i    ].Transform.x + (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
            _meshBuilder.AddVertex(_road[i + 1].Transform.x - (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);
            _meshBuilder.AddVertex(_road[i + 1].Transform.x + (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);

            _meshBuilder.AddUV(0, (i) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV(1, (i) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 2).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV((_meshBuilder.GetVertex(n * 4 + 3).x - _meshBuilder.GetVertex(n * 4).x) / (_meshBuilder.GetVertex(n * 4 + 1).x - _meshBuilder.GetVertex(n * 4).x), (i + 1) % 2 == 0 ? 0 : 1);

            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);

            _meshBuilder.AddTriangle(n * 4, n * 4 + 2, n * 4 + 1, i % 2 == 0 ? 0 : 1);
            _meshBuilder.AddTriangle(n * 4 + 1, n * 4 + 2, n * 4 + 3, i % 2 == 0 ? 0 : 1);
        }

        _meshBuilder.ToMesh(_mesh);
    }

}
