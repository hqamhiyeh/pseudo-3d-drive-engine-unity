using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class DriveRenderer : MonoBehaviour
{
    public Camera driveCamera;
    public GameObject driveScreen;
    public SpriteRenderer roadPlane;

    private RenderTexture   _renderTex;
    private RenderTexture   _saveActiveRenderTex;
    private Mesh            _mesh;
    private MeshBuilder     _meshBuilder;

    public Material lineMaterial;
    public Material material1;
    public Material material2;

    int segmentLength           = 200;          // number of lines per segment
    int segmentCount            = 500;          // number of segments that make up the road
    int roadLength;                             // number of lines that make up the road (calculated)
    int roadWidth               = 2000;         // number of pixels wide the road is
    int rumbleLength            = 3;            // number of segments per rumble strip
    public int drawDistance     = 200;          // number of segments to draw

    public int canvasWidth      = 1920;         // width in pixels
    public int canvasHeight     = 1080;         // height in pixels
    int pixelsPerUnit           = 100;          // pixels per unit

    float fieldOfView           = 100;          // ingame camera field of view
    float cameraDepth;                          // z distance camera is from screen (calculated)

    Vector3 cameraPosition = new Vector3(0.0f, 1000.0f, 0.0f);
    Vector3 screenPosition;
    
    private List<Segment> _road = new List<Segment>();
    private List<Segment> _roadSection = new List<Segment>();

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Application.targetFrameRate = -1;

        Texture2D roadPlaneTex = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        roadPlaneTex.filterMode = FilterMode.Point;
        Sprite roadPlaneSprite = Sprite.Create(roadPlaneTex, new Rect(0, 0, canvasWidth, canvasHeight), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        roadPlaneSprite.name = "roadPlaneSprite";
        roadPlane.sprite = roadPlaneSprite;

        driveCamera.orthographicSize = ((float)canvasHeight / (float)pixelsPerUnit) / 2.0f;

        cameraDepth = (float) (1 / Math.Tan((fieldOfView / 2) * Math.PI / 180));
        screenPosition = driveScreen.transform.position;

        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt16;

        _meshBuilder = new MeshBuilder();
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
            cameraPosition.z = cameraPosition.z + 2;
        }

        GenerateMesh();
    }

    // OnRenderObject is called after camera has rendered the scene
    private void OnRenderObject()
    {
        Draw();
    }

    void GenerateRoad()
    {
        _road.Clear();
        
        //for (int i = 0; i < segmentCount; i++)
        //    road.Add(new Segment(i, new Point(new Vector3(0.0f, 0.0f, (i+1)*segmentLength), new Vector3(), new Vector3()), new Point(new Vector3(0.0f, 0.0f, (i+2) * segmentLength), new Vector3(), new Vector3()), (Convert.ToBoolean(Math.Floor((double)(i/rumbleLength)%2))) ? Color.red : Color.white));

        List<Point> points = new List<Point>();
        for (int i = 0; i <= segmentCount; i++)
        {
            points.Add(new Point(new Vector3(0.0f, 0.0f, (i + 1) * segmentLength)));
        }

        for (int i = 0; i < segmentCount; i++)
        {
            _road.Add(new Segment(i, points[i], points[i+1], (Convert.ToBoolean(Math.Floor((double)(i / rumbleLength) % 2))) ? Color.red : Color.white));
        }
    }

    void ProjectAll()
    {
        Point segmentPoint;
        for(int i=0; i <= segmentCount; i++)
        {
            if (i != segmentCount)
                segmentPoint = _road[i].p1;
            else
                segmentPoint = _road[i-1].p2;

            Project(segmentPoint);
        }
    }

    void Project(Point point)
    {
        point.camera.x = (point.world.x) - cameraPosition.x;
        point.camera.y = (point.world.y) - cameraPosition.y;
        point.camera.z = (point.world.z) - cameraPosition.z;
        point.screenScale = cameraDepth / point.camera.z;
        point.screen.x = (float)Math.Round((canvasWidth / 2) + (point.screenScale * point.camera.x * canvasWidth / 2));
        point.screen.y = (float)Math.Round((canvasHeight / 2) - (point.screenScale * point.camera.y * canvasHeight / 2));
        point.screenWidth = (float)Math.Round((point.screenScale * roadWidth * canvasWidth / 2));
        point.transform.x = ((point.screen.x - ((float)canvasWidth / 2.0f)) / (float)pixelsPerUnit) + screenPosition.x;
        point.transform.y = (((float)canvasHeight - point.screen.y - ((float)canvasHeight / 2.0f)) / (float)pixelsPerUnit) + screenPosition.y;
        point.transform.z = screenPosition.z;
        point.transformWidth = point.screenWidth / (float)pixelsPerUnit;
    }

    private void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void Draw()
    {
        _renderTex = RenderTexture.GetTemporary(canvasWidth, canvasHeight);
        _saveActiveRenderTex = RenderTexture.active;
        Graphics.SetRenderTarget(_renderTex);
        GL.Clear(false, true, new Color(0.2f, 0.3f, 0.3f, 1.0f));
        //CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
        Graphics.CopyTexture(_renderTex, roadPlane.sprite.texture);
        GL.PopMatrix();
        Graphics.SetRenderTarget(_saveActiveRenderTex);
        RenderTexture.ReleaseTemporary(_renderTex);
    }

    private void GenerateMesh()
    {
        _roadSection.Clear();
        Segment baseSegment = _road[(int)Math.Floor(cameraPosition.z / segmentLength)];
        Segment segment;
        Point point;
        Boolean addToMesh;
        for (int n = 0, i; n <= drawDistance; n++)
        {
            i = baseSegment.index + n;
            if (i != segmentCount)
            {
                segment = _road[i];
                point = _road[i].p1;
                addToMesh = true;
            }
            else
            {
                segment = _road[i - 1];
                point = _road[i - 1].p2;
                addToMesh = false;
            }
            
            Project(point);

            if (addToMesh == true)
                _roadSection.Add(segment);
        }

        _mesh.Clear();
        _meshBuilder.ResetOffset();
        for (int i = 0; i < _roadSection.Count; i++)
        {
            _meshBuilder.AddVertex(_roadSection[i].p1.transform.x - (_roadSection[i].p1.transformWidth / 2.0f), _roadSection[i].p1.transform.y, _roadSection[i].p1.transform.z);
            _meshBuilder.AddVertex(_roadSection[i].p1.transform.x + (_roadSection[i].p1.transformWidth / 2.0f), _roadSection[i].p1.transform.y, _roadSection[i].p1.transform.z);
        }
        for (int i = 0; i < 2 * _roadSection.Count; i++)
        {
            _meshBuilder.AddUV(_meshBuilder.GetVertex(i).x, _meshBuilder.GetVertex(i).z);
            _meshBuilder.AddNormal(Vector3.back.x, Vector3.back.y, Vector3.back.z);
        }
        for (int i = 0; i < (2 * _roadSection.Count) - 2; i++)
        {
            _meshBuilder.AddTriangle(i, i + 1, i + 2);
        }

        _meshBuilder.ToMesh(_mesh);
    }
}
