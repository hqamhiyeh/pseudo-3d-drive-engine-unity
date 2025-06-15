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
        @default,
        perspective_unity,
        perspective_unity_flip,
        perspective_opengl_RH,
        perspective_opengl_LH,
        perspective_directx_RH,
        perspective_directx_LH
    };

    [System.Serializable]
    internal class Pseudo3dShaderConfig
    {
        [SerializeField] public ProjectionMatrix ProjectionMatrix;
        [SerializeField] public bool GetGpuProjectionMatrix     = true;
        [SerializeField] public bool ToRenderTexture            = false;
        [SerializeField] public bool EnableProjectionTransform  = true;
        [SerializeField] public bool EnableViewportTransform    = true;
    }
}
