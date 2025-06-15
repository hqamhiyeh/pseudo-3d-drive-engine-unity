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
        [field: SerializeField][field: Space(05)] public Camera Camera { get; set; }
        [field: SerializeField][field: Space(05)] public Road Road { get; set; }
    }
}
