using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Screen
    {
        [field: SerializeField] public Vector3 Position { get; set; }                           // Position of screen in Unity (automatically set based on selected Unity Renderers)
        [field: SerializeField] public int Width { get; set; }                                  // Screen width in pixels
        [field: SerializeField] public int Height { get; set; }                                 // Screen height in pixels
        public float AspectRatio { get; private set; }                                          // Calculated from Width/Height
        [field: SerializeField] public int PixelsPerUnit { get; set; } = 100;                   // Pixels per Unity unit 
    }
}