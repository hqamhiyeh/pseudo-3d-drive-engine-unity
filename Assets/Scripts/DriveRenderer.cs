using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

public class DriveRenderer : MonoBehaviour
{
    public GameObject driveScreen;
    public SpriteRenderer roadPlane;

    private RenderTexture   _renderTex;
    private RenderTexture   _saveActiveRenderTex;
    private Mesh            _mesh;

    static Material lineMaterial;

    int segmentLength   = 200;          // number of lines per segment
    int segmentCount    = 500;          // number of segments that make up the road
    int roadLength;                     // number of lines that make up the road (calculated)
    int roadWidth       = 2000;         // number of pixels wide the road is
    int rumbleLength    = 3;            // number of segments per rumble strip

    int canvasWidth     = 1920;         // width in pixels
    int canvasHeight    = 1080;         // height in pixels
    int PPU             = 100;          // pixels per unit

    float fieldOfView   = 100;          // ingame camera field of view
    float cameraDepth;                  // z distance camera is from screen (calculated)

    Vector3 cameraPosition = new Vector3(0.0f, 1000.0f, 0.0f);
    Vector3 screenPosition;
    
    Vector3[] newVertices;
    Vector3[] newNormals;
    Vector2[] newUVs;
    int[] newTriangles;

    private List<Segment> road = new List<Segment>();

    private class Segment
    {
        public int index;
        public Point p1;
        public Point p2;
        public Color color;

        public Segment(int index)
        {
            this.index = index;
        }

        public Segment(int index, Point p1, Point p2, Color color)
        {
            this.index = index;
            this.p1 = p1;
            this.p2 = p2;
            this.color = color;
        }
    }

    private class Point
    {
        public Vector3 world;
        public Vector3 camera;
        public Vector2 screen;
        public float screenScale;
        public float screenWidth;

        public Point(Vector3 world, Vector3 camera, Vector3 screen)
        {
            this.world = world;
            this.camera = camera;
            this.screen = screen;
        }
    }

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        Texture2D roadPlaneTex = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        roadPlaneTex.filterMode = FilterMode.Point;
        Sprite roadPlaneSprite = Sprite.Create(roadPlaneTex, new Rect(0, 0, canvasWidth, canvasHeight), new Vector2(0.5f, 0.5f), 100);
        roadPlaneSprite.name = "roadPlaneSprite";
        roadPlane.sprite = roadPlaneSprite;

        cameraDepth = (float) (1 / Math.Tan((fieldOfView / 2) * Math.PI / 180));
        screenPosition = driveScreen.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateRoad();
        CalculateProjection();
        CreateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderObject()
    {
        RenderRoad();
    }

    void GenerateRoad()
    {
        road.Clear();
        
        for (int i = 0; i < segmentCount; i++)
        {
            road.Add(new Segment(i, new Point(new Vector3(0.0f, 0.0f, (i+1)*segmentLength), new Vector3(), new Vector3()), new Point(new Vector3(0.0f, 0.0f, (i+2) * segmentLength), new Vector3(), new Vector3()), (Convert.ToBoolean(Math.Floor((double)(i/rumbleLength)%2))) ? Color.red : Color.white));
        }
    }

    void CalculateProjection()
    {
        foreach (Segment segment in road)
        {
            segment.p1.camera.x = (segment.p1.world.x) - cameraPosition.x;
            segment.p1.camera.y = (segment.p1.world.y) - cameraPosition.y;
            segment.p1.camera.z = (segment.p1.world.z) - cameraPosition.z;
            segment.p1.screenScale = cameraDepth / segment.p1.camera.z;
            segment.p1.screen.x =       (float)Math.Round((canvasWidth / 2)     +   (segment.p1.screenScale * segment.p1.camera.x   * canvasWidth / 2));
            segment.p1.screen.y =       (float)Math.Round((canvasHeight / 2)    -   (segment.p1.screenScale * segment.p1.camera.y   * canvasHeight / 2));
            segment.p1.screenWidth =    (float)Math.Round(                          (segment.p1.screenScale * roadWidth             * canvasWidth / 2));

            segment.p2.camera.x = (segment.p2.world.x) - cameraPosition.x;
            segment.p2.camera.z = (segment.p2.world.z) - cameraPosition.z;
            segment.p2.camera.y = (segment.p2.world.y) - cameraPosition.y;
            segment.p2.screenScale = cameraDepth / segment.p2.camera.z;
            segment.p2.screen.x =       (float)Math.Round((canvasWidth / 2)     +   (segment.p2.screenScale * segment.p2.camera.x   * canvasWidth / 2));
            segment.p2.screen.y =       (float)Math.Round((canvasHeight / 2)    -   (segment.p2.screenScale * segment.p2.camera.y   * canvasHeight / 2));
            segment.p2.screenWidth =    (float)Math.Round(                          (segment.p2.screenScale * roadWidth             * canvasWidth / 2));
        }
    }

    void CreateMesh()
    {
        _mesh = new Mesh();

        newVertices = new Vector3[2*segmentCount];
        newNormals = new Vector3[2*segmentCount];
        newUVs = new Vector2[2*segmentCount];
        newTriangles = new int[3*(2*segmentCount)];

        _mesh.indexFormat = IndexFormat.UInt16;

        for (int i = 0, j = 0; i < segmentCount; i++, j += 2)
        {
            newVertices[j]      = new Vector3(((road[i].p1.screen.x - (road[i].p1.screenWidth / 2.0f)) / PPU) - ((canvasWidth / 2.0f) / PPU) + (screenPosition.x), ((canvasHeight - road[i].p1.screen.y - (canvasHeight / 2.0f)) / PPU) + screenPosition.y);
            newVertices[j + 1]  = new Vector3(((road[i].p1.screen.x + (road[i].p1.screenWidth / 2.0f)) / PPU) - ((canvasWidth / 2.0f) / PPU) + (screenPosition.x), ((canvasHeight - road[i].p1.screen.y - (canvasHeight / 2.0f)) / PPU) + screenPosition.y);
        }
        _mesh.vertices = newVertices;

        for (int i = 0; i < 2*segmentCount; i++)
        {
            newNormals[i] = Vector3.back;
            newUVs[i] = new Vector2(_mesh.vertices[i].x, _mesh.vertices[i].z);
        }
        _mesh.normals = newNormals;
        _mesh.uv = newUVs;

        for (int i = 0, j = 0; i < (2*segmentCount) - 2; i++, j += 3)
        {
            newTriangles[j] = i;
            newTriangles[j + 1] = i + 1;
            newTriangles[j + 2] = i + 2;
        }
        _mesh.triangles = newTriangles;
    }

    static void CreateLineMaterial()
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

    private void RenderRoad()
    {
        _renderTex = RenderTexture.GetTemporary(canvasWidth, canvasHeight);
        _saveActiveRenderTex = RenderTexture.active;
        Graphics.SetRenderTarget(_renderTex);
        GL.Clear(false, true, new Color(0.2f, 0.3f, 0.3f, 1.0f));
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
        Graphics.CopyTexture(_renderTex, roadPlane.sprite.texture);
        GL.PopMatrix();
        Graphics.SetRenderTarget(_saveActiveRenderTex);
        RenderTexture.ReleaseTemporary(_renderTex);
    }
}
