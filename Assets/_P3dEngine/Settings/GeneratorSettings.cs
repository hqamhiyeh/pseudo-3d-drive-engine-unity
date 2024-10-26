using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Settings
{
    [System.Serializable]
    internal class GeneratorSettings
    {
        [SerializeField] public int WorldUnitsPerUnit    = 100;
        [SerializeField] public int DrawDistance         = 200;
    }
}
