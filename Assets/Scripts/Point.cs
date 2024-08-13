using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class Point
    {
        public Vector3 world;
        public Vector3 camera;
        public Vector2 screen;
        public float screenScale;
        public float screenWidth;
        public Vector3 transform;
        public float transformWidth;

        public Point(Vector3 world)
        {
            this.world = world;
        }
    }
}
