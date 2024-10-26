using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class PrototypeMaterials
    {
        [System.Serializable]
        public class CpuMaterials
        {
            [SerializeField] public Material RoadDark;
            [SerializeField] public Material RoadLight;
        }

        [SerializeField] public CpuMaterials Cpu;

        [System.Serializable]
        public class GpuMaterials
        {
            [SerializeField] public Material UnlitTexture;
            [SerializeField] public Material CustomPseudo3d;
        }

        [SerializeField] public GpuMaterials Gpu;
    }
}
