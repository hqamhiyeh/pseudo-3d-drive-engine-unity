using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DriveRenderer : MonoBehaviour
{
    [SerializeField] private Camera         _driveCamera;               // Unity camera that's pointed at screen
    [SerializeField] private GameObject     _driveScreen;               // Screen object where the game will be displayed
    [SerializeField] private SpriteRenderer _roadPlaneSR;               // Road plane sprite renderer
    [SerializeField] private MeshRenderer   _roadPlaneMR;               // Road plane mesh renderer
    [SerializeField] private MeshFilter     _roadPlaneMF;               // Road plane mesh filter

    [SerializeField] private Boolean _useSpriteRenderer = false;        // True enables sprite renderer, disables mesh renderer. False for vice versa

    [SerializeField] private int _viewportWidth     = 1920;             // width in pixels
    [SerializeField] private int _viewportHeight    = 1080;             // height in pixels
    [SerializeField] private int _pixelsPerUnit     = 100;              // pixels per unit

    private readonly List<RoadPoint> _road = new();                     // All road points
    private Mesh _mesh;                                                 // The mesh to render, constantly changing.
    private MeshBuilder _meshBuilder;                                   // Builder to create meshes

    [SerializeField] private Material _roadMaterialDark;                // Dark color for road mesh
    [SerializeField] private Material _roadMaterialLight;               // Light color for road mesh
    private List<Material> materials = new();                           // Materials to apply to meshes

    private readonly int _segmentLength      = 200;                     // number of lines per segment
    private readonly int _segmentCount       = 500;                     // number of segments that make up the road
    private readonly int _roadWidth          = 2000;                    // number of pixels wide the road is
    //private readonly int _rumbleLength       = 3;                       // number of segments per rumble strip
    [SerializeField]
    private int _drawDistance       = 200;                              // number of segments to draw

    private Vector3 _cameraPosition = new(0.0f, 1000.0f, 0.0f);         // POV camera position
    private Vector3 _screenPosition;                                    // Position of screen. Automatically set to position of screen object.
    [SerializeField]
    private float _fieldOfView      = 100;                              // ingame camera field of view
    private float _cameraDistanceZ;                                     // z distance camera is from screen (calculated)

    [SerializeField]
    private int _targetFrameRate    = -1;                               // Frame rate limit. -1 is unlimited.

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Set app frame rate
        Application.targetFrameRate = _targetFrameRate;

        // Set Drive scene objects positions and camera 
        _screenPosition = _driveScreen.transform.position;
        _driveCamera.transform.position.Set(_screenPosition.x, _screenPosition.y, _screenPosition.z - 1);
        _driveCamera.orthographicSize = (float)_viewportHeight / (float)_pixelsPerUnit / 2.0f;

        // Create and set road plane sprite for sprite renderer
        Texture2D   roadPlaneTexture    = new(_viewportWidth, _viewportHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Sprite      roadPlaneSprite     = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _viewportWidth, _viewportHeight), new Vector2(0.5f, 0.5f), _pixelsPerUnit);
        roadPlaneSprite.name = "Road Plane Sprite";
        _roadPlaneSR.sprite = roadPlaneSprite;
        
        // Set pov camera distance (normalized) from viewport
        _cameraDistanceZ = (float)(1 / Math.Tan((_fieldOfView / 2) * Math.PI / 180));

        // Initialize empty mesh & builder
        _mesh = new Mesh() { indexFormat = IndexFormat.UInt16 };
        _meshBuilder = new MeshBuilder(2);

        // Set up road mesh renderer
        _roadPlaneMR = _driveScreen.GetComponentInChildren<MeshRenderer>();
        _roadPlaneMF = _driveScreen.GetComponentInChildren<MeshFilter>();
        _roadPlaneMF.mesh = _mesh;

        // Set up materials
        materials.Add(_roadMaterialDark);
        materials.Add(_roadMaterialLight);
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateRoad();
    }

    // FixedUpdate is called every fixed framerate frame
    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_useSpriteRenderer)
        {
            _roadPlaneMR.enabled = false;
            _roadPlaneSR.enabled = true;
        }
        else
        {
            _roadPlaneSR.enabled = false;
            _roadPlaneMR.enabled = true;
        }

        if (Input.GetKey("w"))
            _cameraPosition.Set(_cameraPosition.x, _cameraPosition.y, _cameraPosition.z + 1);
        if (Input.GetKey("s"))
            _cameraPosition.Set(_cameraPosition.x, _cameraPosition.y, _cameraPosition.z - 1);

        GenerateMesh();
    }

    // OnRenderObject is called after camera has rendered the scene
    private void OnRenderObject()
    {
        if(_useSpriteRenderer)
            Draw();
    }
    
    void GenerateRoad()
    {
        _road.Clear();
        for (int i = 0; i < (_segmentCount + 1); i++)
            _road.Add(new RoadPoint(new Vector3(0.0f, 0.0f, (i + 1) * _segmentLength)));
    }

    void Project(RoadPoint point, int roadWidth, Vector3 cameraPosition, float cameraDistanceZ, int viewportHeight, int viewportWidth, int pixelsPerUnit, Vector3 positionOffset)
    {
        point.Camera.x = point.World.x - cameraPosition.x;
        point.Camera.y = point.World.y - cameraPosition.y;
        point.Camera.z = point.World.z - cameraPosition.z;

        point.View.x = point.Camera.x   * (cameraDistanceZ / point.Camera.z);
        point.View.y = point.Camera.y   * (cameraDistanceZ / point.Camera.z);
        point.View.z = 0.0f;
        point.View.w = (float)roadWidth * (cameraDistanceZ / point.Camera.z);

        point.Transform.x = point.View.x * ((float)viewportWidth  / 2.0f) / (float)pixelsPerUnit + positionOffset.x;
        point.Transform.y = point.View.y * ((float)viewportHeight / 2.0f) / (float)pixelsPerUnit + positionOffset.y;
        point.Transform.z = point.View.z + positionOffset.z;
        point.Transform.w = point.View.w * ((float)viewportWidth  / 2.0f) / (float)pixelsPerUnit;
    }

    private void Draw()
    {
        RenderTexture saveActiveRenderTexture = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(_viewportWidth, _viewportHeight);
        Graphics.SetRenderTarget(renderTexture);

        GL.Clear(false, true, new Color(0.2f, 0.3f, 0.3f, 1.0f));
        GL.PushMatrix();
        
        Material material;
        for(int submeshIndex = 0; submeshIndex < _mesh.subMeshCount; submeshIndex++)
        {
            material = materials[submeshIndex];
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

        int startPointIndex = (int)Math.Floor((double)_cameraPosition.z / (double)_segmentLength);


        for (int n = 0, i = (int)Math.Floor((double)_cameraPosition.z / (double)_segmentLength); n < _drawDistance && (i + 1) < _road.Count; n++, i++)
        {
            Project(_road[i    ], _roadWidth, _cameraPosition, _cameraDistanceZ, _viewportHeight, _viewportWidth, _pixelsPerUnit, _useSpriteRenderer == true ? _screenPosition : Vector3.zero);
            Project(_road[i + 1], _roadWidth, _cameraPosition, _cameraDistanceZ, _viewportHeight, _viewportWidth, _pixelsPerUnit, _useSpriteRenderer == true ? _screenPosition : Vector3.zero);

            _meshBuilder.AddVertex(_road[i    ].Transform.x - (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
            _meshBuilder.AddVertex(_road[i    ].Transform.x + (_road[i    ].Transform.w / 2.0f), _road[i    ].Transform.y, _road[i    ].Transform.z);
            _meshBuilder.AddVertex(_road[i + 1].Transform.x - (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);
            _meshBuilder.AddVertex(_road[i + 1].Transform.x + (_road[i + 1].Transform.w / 2.0f), _road[i + 1].Transform.y, _road[i + 1].Transform.z);
            
            _meshBuilder.AddUV(0, (i    ) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV(1, (i    ) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV(0, (i + 1) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV(1, (i + 1) % 2 == 0 ? 0 : 1);
            
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            
            _meshBuilder.AddTriangle(n * 4    , n * 4 + 2, n * 4 + 1, i % 2 == 0 ? 0 : 1);
            _meshBuilder.AddTriangle(n * 4 + 1, n * 4 + 2, n * 4 + 3, i % 2 == 0 ? 0 : 1);
        }
        
        _meshBuilder.ToMesh(_mesh);
    }
    
}
