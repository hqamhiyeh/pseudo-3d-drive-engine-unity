using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Device;

namespace Assets._P3dEngine.Shaders
{   
    internal class Pseudo3dShader : IShader
    {
        readonly Pseudo3dShaderData _data;

        Material _material;
        Camera _camera;
        Window _window;
        IRendererSettings _settings;

        Vector3 _cameraPosition;
        int     _cameraFov;
        float   _cameraFocalLength;

        Vector3 _windowPosition;
        int     _windowWidth;
        int     _windowHeight;
        float   _windowAspectRatio;

        int     _settingsPixelsPerUnit;
        bool    _settingsUseSpriteRenderer;

        ProjectionMatrix _projectionMatrix  = ProjectionMatrix.perspective_unity_flip;
        bool _getGpuProjectionMatrix        = true;
        bool _toRenderTexture               = false;
        bool _enableProjectionTransform     = true;
        bool _enableScreenTransform       = true;

        // Transformation matrices
        UnityEngine.Matrix4x4 _mIdentity;
        UnityEngine.Matrix4x4 _mView;
        UnityEngine.Matrix4x4 _mPan;
        UnityEngine.Matrix4x4 _mTilt;
        UnityEngine.Matrix4x4 _mRoll;
        UnityEngine.Matrix4x4 _mProjection;
        UnityEngine.Matrix4x4 _mScreen;
        
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
        float f;                        // "focal" length
        
        // Screen Matrix Parameters
        int width;
        int height;
        int ppu;                        // pixels per unit
        UnityEngine.Vector4 position;   // screen position offset
        
        /*// Debug Fields
        UnityEngine.Camera c;
        UnityEngine.Matrix4x4 m;
        UnityEngine.FrustumPlanes fp;
        */

        public Pseudo3dShader(Pseudo3dShaderData shaderData)
        {
            _data = shaderData;
            Initialize();
        }

        private void Initialize()
        {
            _material           = _data.Material;
            _window             = _data.Window;
            _camera             = _data.Camera;
            _settings           = _data.RendererSettings;

            _mIdentity   = UnityEngine.Matrix4x4.identity;
            _mView       = UnityEngine.Matrix4x4.identity;
            _mPan        = UnityEngine.Matrix4x4.identity;
            _mTilt       = UnityEngine.Matrix4x4.identity;
            _mRoll       = UnityEngine.Matrix4x4.identity;
            _mProjection = UnityEngine.Matrix4x4.identity;
            _mScreen   = UnityEngine.Matrix4x4.identity;

            /*// Debug Unity Camera Projection Matrix
            c = GameObject.Find("Camera").GetComponent<UnityEngine.Camera>();
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
            
        }

        public void SetUniforms()
        {
            _cameraPosition             = _camera.Position;
            _cameraFov                  = _camera.FOV;
            _cameraFocalLength          = _camera.FocalLength;
            _windowPosition             = _window.Position;
            _windowWidth                = _window.Width;
            _windowHeight               = _window.Height;
            _windowAspectRatio          = _window.AspectRatio;
            _settingsPixelsPerUnit      = _settings.PixelsPerUnit;
            _settingsUseSpriteRenderer  = _settings.UseSpriteRenderer;
            _projectionMatrix           = _data.ProjectionMatrix;
            _getGpuProjectionMatrix     = _data.GetGpuProjectionMatrix;
            _toRenderTexture            = _data.ToRenderTexture;
            _enableProjectionTransform  = _data.EnableProjectionTransform;
            _enableScreenTransform      = _data.EnableViewportTransform;

            // View Matrix
            cameraX = _cameraPosition.x / 100.0f;
            cameraY = _cameraPosition.y / 100.0f;
            cameraZ = _cameraPosition.z / 100.0f;
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
            fovy    = _cameraFov;
            aspect  = (float)_windowWidth / (float)_windowHeight;
            near    = _material.GetFloat("_Near");
            far     = _material.GetFloat("_Far");
            f = (float)Math.Tan((fovy / 2.0) * (Math.PI / 180.0));

            switch(_projectionMatrix)
            {
                case ProjectionMatrix.perspective_unity:
                {
                    _mProjection = UnityEngine.Matrix4x4.identity;
                    _mProjection = UnityEngine.Matrix4x4.Perspective(fovy, aspect, near, far);
                    break;
                }
                case ProjectionMatrix.perspective_unity_flip:
                {
                    _mProjection = UnityEngine.Matrix4x4.identity;
                    _mProjection = UnityEngine.Matrix4x4.Perspective(fovy, aspect, near, far);
                    _mProjection.m02 *= -1.0f;
                    _mProjection.m12 *= -1.0f;
                    _mProjection.m22 *= -1.0f;
                    _mProjection.m32 *= -1.0f;
                    break;
                }
                case ProjectionMatrix.perspective_opengl_LH:
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
                case ProjectionMatrix.perspective_opengl_RH:
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
                case ProjectionMatrix.perspective_directx_LH:
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
                case ProjectionMatrix.perspective_directx_RH:
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
                _mProjection = GL.GetGPUProjectionMatrix(_mProjection, _toRenderTexture == true);

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

            // Screen Matrix (Separate into viewport & screen matrices, viewport is normalized)
            width       = _windowWidth;
            height      = _windowHeight;
            ppu         = _settingsPixelsPerUnit;
            position    = (UnityEngine.Vector4) _windowPosition;
            position.w  = 1.0f;
            _mScreen.m00 = (width  / 2.0f) / ppu;
            _mScreen.m11 = (height / 2.0f) / ppu;
            _mScreen.SetColumn(3, _settingsUseSpriteRenderer ? position : _mIdentity.GetColumn(3));
            _material.SetMatrix("_Screen", _enableScreenTransform == true ? _mScreen : _mIdentity);
        }
    }
}
