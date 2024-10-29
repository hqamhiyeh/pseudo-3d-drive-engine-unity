using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{

    [System.Serializable]
    internal class RendererSettings
    {
#nullable enable
        public event Action? SettingChanged;
#nullable disable

        private bool _useSpriteRenderer;
        [SerializeField] private bool _UseSpriteRenderer;
        public bool UseSpriteRenderer { get => _useSpriteRenderer; set { _useSpriteRenderer = value; SettingChanged?.Invoke(); } }

        public void ApplyEditorValues()
        {
            UseSpriteRenderer = _UseSpriteRenderer;
        }
    }
}
    