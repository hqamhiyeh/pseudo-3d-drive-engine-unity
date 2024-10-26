using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class World
    {
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: NonSerialized] public Road Road { get; private set; }

        public void SetRoad(Road road)
        {
            Road = road;
        }
    }
}
