using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{
    [System.Serializable]
    internal class RendererSettings
    {
        [field: SerializeField] public bool UseSpriteRenderer   { get; set; } = false;
    }
}
    