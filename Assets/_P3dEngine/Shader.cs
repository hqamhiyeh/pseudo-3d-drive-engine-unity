// Attach this script component to a unity object with a Mesh Renderer that uses shader 'Pseudo3D'

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Shader : MonoBehaviour
{
    [SerializeField] Material _material;
    [SerializeField] private GameObject _screen;
    [SerializeField] private GameObject _roadPlaneSR;
    [SerializeField] private GameObject _roadPlaneMR;
    [SerializeField] private Boolean _useSpriteRenderer;
    
    private enum ProjectionMatrix
    {
        perspective,
        perspective_flip,
        opengl_RH,
        opengl_LH,
        directx_RH,
        directx_LH
    };

    [Space]
    [SerializeField] ProjectionMatrix _projectionMatrix = ProjectionMatrix.perspective_flip;
    [SerializeField] Boolean _getGpuProjectionMatrix    = true;
    [SerializeField] Boolean _toRenderTexture           = false;
    [SerializeField] Boolean _enableProjectionTransform = true;
    [SerializeField] Boolean _enableViewportTransform   = true;

    UnityEngine.Matrix4x4 _mIdentity;
    UnityEngine.Matrix4x4 _mView;
    UnityEngine.Matrix4x4 _mPan;
    UnityEngine.Matrix4x4 _mTilt;
    UnityEngine.Matrix4x4 _mRoll;
    UnityEngine.Matrix4x4 _mProjection;
    UnityEngine.Matrix4x4 _mViewport;

    // View Matrix Parameters
    float cameraX;
    float cameraY;
    float cameraZ;
    float pan;
    float tilt;
    float roll;

    // Projection Matrix Parameters
    float fovy;
    float aspect;
    float near;
    float far;
    float f;    // "focal" length

    // Viewport Matrix Parameters
    int width;
    int height;
    int ppu;    // pixels per unit
    UnityEngine.Vector4 position;   // screen position offset

    /*// Debug Fields
    [SerializeField] Camera c;
    UnityEngine.Matrix4x4 m;
    UnityEngine.FrustumPlanes fp;
    */

    // Start is called before the first frame update
    void Start()
    {
        _mIdentity   = UnityEngine.Matrix4x4.identity;
        _mView       = UnityEngine.Matrix4x4.identity;
        _mPan        = UnityEngine.Matrix4x4.identity;
        _mTilt       = UnityEngine.Matrix4x4.identity;
        _mRoll       = UnityEngine.Matrix4x4.identity;
        _mProjection = UnityEngine.Matrix4x4.identity;
        _mViewport   = UnityEngine.Matrix4x4.identity;

        /*// Debug Unity Camera Projection Matrix
        m = c.projectionMatrix;
        fp = m.decomposeProjection;
        Debug.Log( "Unity Camera Projection Matrix..." );
        Debug.Log( "Top: "      + fp.top);
        Debug.Log( "Bottom: "   + fp.bottom);
        Debug.Log( "Left: "     + fp.left);
        Debug.Log( "Right: "    + fp.right);
        Debug.Log( "Near: "     + fp.zNear);
        Debug.Log( "Far: "      + fp.zFar);
        */
        
        position = (UnityEngine.Vector4) _screen.transform.position;
        position.w = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Process Keyboard Input
        if (Input.GetKey("w"))
            _material.SetFloat("_CameraZ", cameraZ += 0.06f);
        if (Input.GetKey("s"))
            _material.SetFloat("_CameraZ", cameraZ -= 0.06f);
        if (Input.GetKey("a"))
            _material.SetFloat("_CameraX", cameraX -= 0.01f);
        if (Input.GetKey("d"))
            _material.SetFloat("_CameraX", cameraX += 0.01f);

        // View Matrix
        cameraX = _material.GetFloat("_CameraX");
        cameraY = _material.GetFloat("_CameraY");
        cameraZ = _material.GetFloat("_CameraZ");
        _mView.m03 = -cameraX;
        _mView.m13 = -cameraY;
        _mView.m23 = -cameraZ;

        pan  = _material.GetFloat("_Pan");
        _mPan.m00 = (float)  Math.Cos(pan * (Math.PI / 180));
        _mPan.m20 = (float)  Math.Sin(pan * (Math.PI / 180));
        _mPan.m02 = (float) -Math.Sin(pan * (Math.PI / 180));
        _mPan.m22 = (float)  Math.Cos(pan * (Math.PI / 180));

        tilt = _material.GetFloat("_Tilt");
        _mTilt.m11 = (float)  Math.Cos(tilt * (Math.PI / 180));
        _mTilt.m21 = (float)  Math.Sin(tilt * (Math.PI / 180));
        _mTilt.m12 = (float) -Math.Sin(tilt * (Math.PI / 180));
        _mTilt.m22 = (float)  Math.Cos(tilt * (Math.PI / 180));

        roll = _material.GetFloat("_Roll");
        _mRoll.m00 = (float)  Math.Cos(roll * (Math.PI / 180));
        _mRoll.m10 = (float) -Math.Sin(roll * (Math.PI / 180));
        _mRoll.m01 = (float)  Math.Sin(roll * (Math.PI / 180));
        _mRoll.m11 = (float)  Math.Cos(roll * (Math.PI / 180));
        
        _material.SetMatrix("_View", _mRoll * _mTilt * _mPan * _mView);

        // Projection Matrix
        fovy    = _material.GetFloat("_FovY");
        aspect  = _material.GetFloat("_Aspect");
        near    = _material.GetFloat("_Near");
        far     = _material.GetFloat("_Far");
        f = (float)Math.Tan((fovy / 2.0) * (Math.PI / 180.0));

        switch(_projectionMatrix)
        {
            case ProjectionMatrix.perspective:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection = UnityEngine.Matrix4x4.Perspective(fovy, aspect, near, far);
                break;
            }
            case ProjectionMatrix.perspective_flip:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection = UnityEngine.Matrix4x4.Perspective(fovy, aspect, near, far);
                _mProjection.m02 *= -1.0f;
                _mProjection.m12 *= -1.0f;
                _mProjection.m22 *= -1.0f;
                _mProjection.m32 *= -1.0f;
                break;
            }
            case ProjectionMatrix.opengl_LH:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection.m00 = aspect * (1.0f / f);
                _mProjection.m11 = 1.0f / f;
                _mProjection.m22 = -(far + near) / (far - near);
                _mProjection.m23 = 1.0f;
                _mProjection.m32 = (2.0f * far * near) / (far - near);
                _mProjection.m33 = 0.0f;
                break;
            }
            case ProjectionMatrix.opengl_RH:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection.m00 = aspect * (1.0f / f);
                _mProjection.m11 = 1.0f / f;
                _mProjection.m22 = -(far + near) / (far - near);
                _mProjection.m23 = -1.0f;
                _mProjection.m32 = (-2.0f * far * near) / (far - near);
                _mProjection.m33 = 0.0f;
                break;
            }
            case ProjectionMatrix.directx_LH:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection.m00 = (1.0f / aspect) * (1.0f / f);
                _mProjection.m11 = 1.0f / f;
                _mProjection.m22 = far / (far - near);
                _mProjection.m23 = 1.0f;
                _mProjection.m32 = 1.0f;
                _mProjection.m33 = 0.0f;
                break;
            }
            case ProjectionMatrix.directx_RH:
            {
                _mProjection = UnityEngine.Matrix4x4.identity;
                _mProjection.m00 = aspect * (1.0f / f);
                _mProjection.m11 = 1.0f / f;
                _mProjection.m22 = far / (far - near);
                _mProjection.m23 = -1.0f;
                _mProjection.m32 = near * (far - near);
                _mProjection.m33 = 0.0f;
                break;
            }
        }

        if(_getGpuProjectionMatrix == true)
            _mProjection = GL.GetGPUProjectionMatrix(_mProjection, _toRenderTexture == true ? true : false);

        _material.SetMatrix("_Perspective", _enableProjectionTransform == true ? _mProjection : _mIdentity);

        /*// Debug POV Camera Projection Matrix
        fp = mProjection.decomposeProjection;
        Debug.Log( "POV Camera Projection Matrix..." );
        Debug.Log( "Top: "      + fp.top    );
        Debug.Log( "Bottom: "   + fp.bottom );
        Debug.Log( "Left: "     + fp.left   );
        Debug.Log( "Right: "    + fp.right  );
        Debug.Log( "Near: "     + fp.zNear  );
        Debug.Log( "Far: "      + fp.zFar   );
        */

        // Viewport Matrix (Separate into viewport & screen matrices, viewport is normalized)
        width   = _material.GetInteger("_Width");
        height  = _material.GetInteger("_Height");
        ppu     = _material.GetInteger("_PPU");
        _mViewport.m00 = (width  / 2.0f) / ppu;
        _mViewport.m11 = (height / 2.0f) / ppu;
        _mViewport.SetColumn(3, _useSpriteRenderer ? position : _mIdentity.GetColumn(3));
        _material.SetMatrix("_Viewport", _enableViewportTransform == true ? _mViewport : _mIdentity);
    }
}
