using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Shaders
{
    internal enum ProjectionMatrix
    {
        perspective_unity,
        perspective_unity_flip,
        perspective_opengl_RH,
        perspective_opengl_LH,
        perspective_directx_RH,
        perspective_directx_LH
    };

    [System.Serializable]
    internal class Pseudo3dShaderData
    {
        [NonSerialized]  public Material Material;
        [NonSerialized]  public Camera Camera;
        [NonSerialized]  public Window Window;
        [NonSerialized]  public IRendererSettings RendererSettings;
        [SerializeField] public ProjectionMatrix ProjectionMatrix;
        [SerializeField] public bool GetGpuProjectionMatrix;
        [SerializeField] public bool ToRenderTexture;
        [SerializeField] public bool EnableProjectionTransform;
        [SerializeField] public bool EnableViewportTransform;
    }
}
