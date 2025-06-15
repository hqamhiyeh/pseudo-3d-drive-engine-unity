using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.U2D;
using UnityEngine;

namespace Assets._P3dEngine
{
    internal class UnityCamera
    {
        public GameObject GameObject { get; private set; }
        public UnityEngine.Camera Camera { get; private set; }                                          // Unity camera that's pointed at screen
        public PixelPerfectCamera PixelPerfectCamera { get; private set; }

        public UnityCamera(GameObject gameObject)
        {
            GameObject = gameObject;
            Camera = GameObject.GetComponent<UnityEngine.Camera>();
        }
    }
}
