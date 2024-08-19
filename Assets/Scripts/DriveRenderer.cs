using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class DriveRenderer : MonoBehaviour
{
    [SerializeField] private Camera         _driveCamera;               // Unity camera that's pointed at screen
    [SerializeField] private GameObject     _driveScreen;               // Screen object where the game will be displayed
    [SerializeField] private SpriteRenderer _roadPlaneSR;               // Road plane sprite renderer
    [SerializeField] private MeshRenderer   _roadPlaneMR;               // Road plane mesh renderer
    [SerializeField] private MeshFilter     _roadPlaneMF;               // Road plane mesh filter

    [SerializeField] private Boolean _enableSpriteRenderer = false;     // True enables sprite renderer, disables mesh renderer. False for vice versa

    [SerializeField] private int _viewportWidth     = 1920;             // width in pixels
    [SerializeField] private int _viewportHeight    = 1080;             // height in pixels
    [SerializeField] private int _pixelsPerUnit     = 100;              // pixels per unit

    private List<Point> _points = new List<Point>();                    // All points on road
    private List<Point> _sectionPoints = new List<Point>();
    private List<Segment> _road = new List<Segment>();                  // The complete road to generate
    private List<Segment> _roadSection = new List<Segment>();           // Section of road to display

    private Mesh _mesh;                                                 // The mesh to render
    private MeshBuilder _meshBuilder;                                   // To create meshes

    [SerializeField] private Material _roadMaterialDark;                // Dark color for road mesh
    [SerializeField] private Material _roadMaterialLight;               // Light color for road mesh
    private List<Material> materials = new List<Material>();            // Materials to apply to meshes

    private int _segmentLength      = 200;                              // number of lines per segment
    private int _segmentCount       = 3000;                             // number of segments that make up the road
    private int _roadLength;                                            // number of lines that make up the road (calculated)
    private int _roadWidth          = 2000;                             // number of pixels wide the road is
    private int _rumbleLength       = 3;                                // number of segments per rumble strip
    [SerializeField]
    private int _drawDistance       = 200;                              // number of segments to draw

    private Vector3 _cameraPosition = new Vector3(0.0f, 1000.0f, 0.0f); // POV camera position
    private Vector3 _screenPosition;                                    // Position of screen. Automatically set to position of screen object.
    [SerializeField]
    private float _fieldOfView      = 100;                              // ingame camera field of view
    private float _cameraDistanceZ;                                     // z distance camera is from screen (calculated)

    [SerializeField]
    private int _targetFrameRate    = -1;                               // Frame rate limit. -1 is unlimited.


    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Application.targetFrameRate = _targetFrameRate;

        Texture2D roadPlaneTexture;
        roadPlaneTexture = new Texture2D(_viewportWidth, _viewportHeight, TextureFormat.RGBA32, false);
        roadPlaneTexture.filterMode = FilterMode.Point;
        
        Sprite roadPlaneSprite;
        roadPlaneSprite = Sprite.Create(roadPlaneTexture, new Rect(0, 0, _viewportWidth, _viewportHeight), new Vector2(0.5f, 0.5f), _pixelsPerUnit);
        roadPlaneSprite.name = "roadPlaneSprite";
        _roadPlaneSR.sprite = roadPlaneSprite;

        _driveCamera.orthographicSize = (float)_viewportHeight / (float)_pixelsPerUnit / 2.0f;
        _screenPosition = _driveScreen.transform.position;
        _cameraDistanceZ = (float) (1 / Math.Tan((_fieldOfView / 2) * Math.PI / 180));

        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt16;
        _meshBuilder = new MeshBuilder(2);

        _roadPlaneMR = _driveScreen.GetComponentInChildren<MeshRenderer>();
        _roadPlaneMF = _driveScreen.GetComponentInChildren<MeshFilter>();
        _roadPlaneMF.mesh = _mesh;

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
        if (Input.GetKey("w"))
        {
            _cameraPosition.Set(_cameraPosition.x,_cameraPosition.y,_cameraPosition.z+50);
        }

        GenerateMesh();

        if (_enableSpriteRenderer)
        {
            _roadPlaneMR.enabled = false;
            _roadPlaneSR.enabled = true;
        }
        else
        {
            _roadPlaneSR.enabled = false;
            _roadPlaneMR.enabled = true;
        }
    }

    // OnRenderObject is called after camera has rendered the scene
    private void OnRenderObject()
    {
        if(_enableSpriteRenderer)
            Draw();
    }

    void GenerateRoad()
    {
        _road.Clear();
        
        //for (int i = 0; i < segmentCount; i++)
        //    road.Add(new Segment(i, new Point(new Vector3(0.0f, 0.0f, (i+1)*segmentLength), new Vector3(), new Vector3()), new Point(new Vector3(0.0f, 0.0f, (i+2) * segmentLength), new Vector3(), new Vector3()), (Convert.ToBoolean(Math.Floor((double)(i/rumbleLength)%2))) ? Color.red : Color.white));

        for (int i = 0; i <= _segmentCount; i++)
        {
            _points.Add(new Point(new Vector3(0.0f, 0.0f, (i + 1) * _segmentLength)));
        }

        for (int i = 0; i < _segmentCount; i++)
        {
            _road.Add(new Segment(i, _points[i], _points[i+1], (Convert.ToBoolean(Math.Floor((double)(i / _rumbleLength) % 2))) ? Color.red : Color.white));
        }
    }

    void ProjectAll()
    {
        Point segmentPoint;
        for(int i=0; i <= _segmentCount; i++)
        {
            if (i != _segmentCount)
                segmentPoint = _road[i].p1;
            else
                segmentPoint = _road[i-1].p2;

            Project(segmentPoint);
        }
    }

    void Project(Point point)
    {
        point.camera.x = point.world.x - _cameraPosition.x;
        point.camera.y = point.world.y - _cameraPosition.y;
        point.camera.z = point.world.z - _cameraPosition.z;
        point.screenScale = _cameraDistanceZ / point.camera.z;
        point.screen.x = point.screenScale * point.camera.x;
        point.screen.y = point.screenScale * point.camera.y;
        point.screenWidth = point.screenScale * (float)_roadWidth;

        if (_enableSpriteRenderer)
        {
            /* Sprite Renderer */
            point.transform.x =     point.screen.x * ((float)_viewportWidth  / 2.0f) / (float)_pixelsPerUnit + _screenPosition.x;
            point.transform.y =     point.screen.y * ((float)_viewportHeight / 2.0f) / (float)_pixelsPerUnit + _screenPosition.y;
            point.transform.z =     _screenPosition.z;
            point.transformWidth =  point.screenWidth * ((float)_viewportWidth / 2.0f) / (float)_pixelsPerUnit;
        }
        else
        {
            /* Mesh Renderer */
            point.transform.x =     point.screen.x * ((float)_viewportWidth  / 2.0f) / (float)_pixelsPerUnit;
            point.transform.y =     point.screen.y * ((float)_viewportHeight / 2.0f) / (float)_pixelsPerUnit;
            point.transform.z =     _screenPosition.z;
            point.transformWidth =  point.screenWidth * ((float)_viewportWidth / 2.0f) / (float)_pixelsPerUnit;
        }
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
        _roadSection.Clear();
        Segment baseSegment = _road[(int)Math.Floor(_cameraPosition.z / _segmentLength)];
        Debug.Log("CameraPositionZ = " + _cameraPosition.z);
        Debug.Log("BaseSegmentIndex = " + baseSegment.index);
        Segment segment;
        Point point;
        Boolean addToMesh;
        for (int n = 0, i; n <= _drawDistance; n++)
        {
            i = baseSegment.index + n;
            if (i != _segmentCount)
            {
                segment = _road[i];
                point = _road[i].p1;
                addToMesh = true;
            }
            else
            {
                segment = _road[i - 1]; // i think this is wrong
                point = _road[i - 1].p2;
                addToMesh = false;
            }
            
            Project(point);

            if (addToMesh == true)
                _roadSection.Add(segment);
        }

        _mesh.Clear();
        _meshBuilder.ResetOffsets();
        for (int i = 0; i < _roadSection.Count; i++)
        {
            _meshBuilder.AddVertex(_roadSection[i].p1.transform.x - (_roadSection[i].p1.transformWidth / 2.0f), _roadSection[i].p1.transform.y, _roadSection[i].p1.transform.z);
            _meshBuilder.AddVertex(_roadSection[i].p1.transform.x + (_roadSection[i].p1.transformWidth / 2.0f), _roadSection[i].p1.transform.y, _roadSection[i].p1.transform.z);
            _meshBuilder.AddUV(0, (_roadSection[i].index) % 2 == 0 ? 0 : 1);
            _meshBuilder.AddUV(1, (_roadSection[i].index) % 2 == 0 ? 0 : 0);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
        }
        for (int i = 0, j = 0, submeshIndex; i < (_roadSection.Count) - 1; i++, j+=2)
        {
            submeshIndex = (_roadSection[i].index) % 2 == 0 ? 0 : 1;
            _meshBuilder.AddTriangle(j    , j + 2, j + 1, submeshIndex);
            _meshBuilder.AddTriangle(j + 1, j + 2, j + 3, submeshIndex);
        }

        _meshBuilder.ToMesh(_mesh);
    }

}
