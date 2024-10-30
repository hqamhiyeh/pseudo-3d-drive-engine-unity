using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{
    [System.Serializable]
    internal class Settings : ISettingsEditor, IApplicationSettings, IGeneratorSettings, IRendererSettings
    {
#nullable enable
        public event Action? SettingValueChanged;
#nullable disable

        // Target Frame Rate
        private int _targetFrameRate = -1;
        [SerializeField] private int _TargetFrameRate;
        public int TargetFrameRate { get => _targetFrameRate; set { _TargetFrameRate = _targetFrameRate = value; } }

        // World Units Per Unit
        private int _worldUnitsPerUnit = 100;
        [SerializeField] private int _WorldUnitsPerUnit;
        public int WorldUnitsPerUnit { get => _worldUnitsPerUnit; set { _WorldUnitsPerUnit = _worldUnitsPerUnit = value; } }

        // Draw Distance
        private int _drawDistance = 200;
        [SerializeField] private int _DrawDistance;
        public int DrawDistance { get => _drawDistance; set { _DrawDistance = _drawDistance = value; } }

        // Pixels Per Unit
        private int _pixelsPerUnit = 100;
        [SerializeField] private int _PixelsPerUnit;
        public int PixelsPerUnit { get => _pixelsPerUnit; set { _PixelsPerUnit = _pixelsPerUnit = value; SettingValueChanged?.Invoke(); Debug.Log("Invoke"); } }

        // Use Sprite Renderer
        private bool _useSpriteRenderer = true;
        [SerializeField] private bool _UseSpriteRenderer;
        public bool UseSpriteRenderer { get => _useSpriteRenderer; set { _UseSpriteRenderer = _useSpriteRenderer = value; SettingValueChanged?.Invoke(); } }

        public void ApplyEditorValues()
        {
            TargetFrameRate     = _TargetFrameRate;
            WorldUnitsPerUnit   = _WorldUnitsPerUnit;
            DrawDistance        = _DrawDistance;
            PixelsPerUnit       = _PixelsPerUnit;
            UseSpriteRenderer   = _UseSpriteRenderer;
        }

    }
}
