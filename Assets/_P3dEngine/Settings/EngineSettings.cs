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
    internal class EngineSettings : IApplyEditorValues, IApplicationSettings, IRendererSettings
    {
#nullable enable
        public event EventHandler<SettingChangedEventArgs>? SettingChanged;
#nullable disable

        private const int   DEFAULT_TargetFrameRate     = -1;
        private const int   DEFAULT_WorldUnitsPerUnit   = 100;
        private const int   DEFAULT_PixelsPerUnit       = 100;
        private const bool  DEFAULT_UseSpriteRenderer   = true;
        private const int   DEFAULT_DrawDistance        = 200;

        private int     _targetFrameRate                        = DEFAULT_TargetFrameRate;
        private int     _worldUnitsPerUnit                      = DEFAULT_WorldUnitsPerUnit;
        private int     _pixelsPerUnit                          = DEFAULT_PixelsPerUnit;
        private bool    _useSpriteRenderer                      = DEFAULT_UseSpriteRenderer;
        private int     _drawDistance                           = DEFAULT_DrawDistance;      
        
        /*
         * Editor Values
         */
        [Header("Application Settings")][Space(5)]
        [SerializeField] private int    m_TargetFrameRate       = DEFAULT_TargetFrameRate;

        [Space(5)]
        [Header("Render Settings")][Space(5)]
        [SerializeField] private int    m_WorldUnitsPerUnit     = DEFAULT_WorldUnitsPerUnit;
        [SerializeField] private int    m_PixelsPerUnit         = DEFAULT_PixelsPerUnit;
        [SerializeField] private bool   m_UseSpriteRenderer     = DEFAULT_UseSpriteRenderer;
        [SerializeField] private int    m_DrawDistance          = DEFAULT_DrawDistance;
        
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

        public static EngineSettings GetNewDefaultSettings()
        {
            return new EngineSettings();
        }

    }
}
