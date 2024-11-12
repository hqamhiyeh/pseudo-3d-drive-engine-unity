using Assets._P3dEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{
    [System.Serializable]
    internal class Settings : IApplyEditorValues, IApplicationSettings, IGeneratorSettings, IRendererSettings
    {
#nullable enable
        public event EventHandler<SettingChangedEventArgs>? SettingChanged;
#nullable disable

        private int     _targetFrameRate        = -1;
        private int     _worldUnitsPerUnit      = 100;
        private int     _pixelsPerUnit          = 100;
        private bool    _useSpriteRenderer      = true;
        private int     _drawDistance           = 200;
        
        [Header("Application Settings")][Space(5)]
        [SerializeField] private int    m_TargetFrameRate;

        [Space(5)]
        [Header("Render Settings")][Space(5)]
        [SerializeField] private int    m_WorldUnitsPerUnit;
        [SerializeField] private int    m_PixelsPerUnit;
        [SerializeField] private bool   m_UseSpriteRenderer;
        [SerializeField] private int    m_DrawDistance;
        
        public int  TargetFrameRate     { get => _targetFrameRate;      set { m_TargetFrameRate      = _targetFrameRate      = value;   RaiseSettingChangedEvent( nameof(TargetFrameRate)   );  } }
        public int  WorldUnitsPerUnit   { get => _worldUnitsPerUnit;    set { m_WorldUnitsPerUnit    = _worldUnitsPerUnit    = value;   RaiseSettingChangedEvent( nameof(WorldUnitsPerUnit) );  } }
        public int  PixelsPerUnit       { get => _pixelsPerUnit;        set { m_PixelsPerUnit        = _pixelsPerUnit        = value;   RaiseSettingChangedEvent( nameof(PixelsPerUnit)     );  } }
        public bool UseSpriteRenderer   { get => _useSpriteRenderer;    set { m_UseSpriteRenderer    = _useSpriteRenderer    = value;   RaiseSettingChangedEvent( nameof(UseSpriteRenderer) );  } }
        public int  DrawDistance        { get => _drawDistance;         set { m_DrawDistance         = _drawDistance         = value;   RaiseSettingChangedEvent( nameof(DrawDistance)      );  } }

        private void RaiseSettingChangedEvent(string settingName)
        {
            SettingChanged?.Invoke( this, new SettingChangedEventArgs(settingName) );
        }

        public void ApplyEditorValues()
        {
            TargetFrameRate     = m_TargetFrameRate;
            WorldUnitsPerUnit   = m_WorldUnitsPerUnit;
            PixelsPerUnit       = m_PixelsPerUnit;
            UseSpriteRenderer   = m_UseSpriteRenderer;
            DrawDistance        = m_DrawDistance;
        }

    }
}
