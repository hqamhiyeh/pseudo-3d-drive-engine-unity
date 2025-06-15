using Assets._P3dEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Camera : IApplyEditorValues
    {
        private const float DEFAULT_CAMERA_HEIGHT   = 300.0f;
        private const int   DEFAULT_CAMERA_FOV      = 60;
        private const float DEFAULT_CAMERA_ASPECT   = 1.778f;

        [SerializeField] private Vector3 _position;
        private int _fov;
        [SerializeField] private int m_FOV;  // For unity inspector
        private float _focalLength;

        public Camera()
        {
            _position = new(0.0f, DEFAULT_CAMERA_HEIGHT, 0.0f);
            FOV = DEFAULT_CAMERA_FOV;
            AspectRatio = DEFAULT_CAMERA_ASPECT;
        }

        public ref Vector3 Position { get => ref _position; }
        public int FOV
        {
            get => _fov;
            set
            {
                if (value <= 0 || value >= 180)
                    throw new ArgumentOutOfRangeException(nameof(FOV), "FOV must be greater than 0 and less than 180.");
                
                m_FOV = _fov = value;
                CalculateFocalLength();
            }
        }
        public float FocalLength { get => _focalLength; }
        [field: SerializeField] public float AspectRatio { get; set; }

        public void CalculateFocalLength()
        {
            _focalLength = (float)(1.0 / Math.Tan((_fov / 2.0) * Math.PI / 180.0));
        }

        public void ApplyEditorValues()
        {
            FOV = m_FOV;
        }
    }
}
