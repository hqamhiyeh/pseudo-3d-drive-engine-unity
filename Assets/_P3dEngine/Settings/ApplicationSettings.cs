using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{
    [System.Serializable]
    internal class ApplicationSettings
    {
        [field: SerializeField] public int TargetFrameRate      { get; set; } = -1;
        [field: SerializeField] public int WorldUnitsPerUnit    { get; set; } = 100;
        [field: SerializeField] public int DrawDistance         { get; set; } = 200;
    }
}
