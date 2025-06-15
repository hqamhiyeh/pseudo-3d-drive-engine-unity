using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    internal class UnityDisplay
    {
        public GameObject GameObject { get; private set; }
        public MeshFilter MeshFilter { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }

        public UnityDisplay(GameObject gameObject)
        {
            GameObject = gameObject;

            MeshFilter = GameObject.GetComponentInChildren<UnityEngine.MeshFilter>();
            MeshRenderer = GameObject.GetComponentInChildren<UnityEngine.MeshRenderer>();
            SpriteRenderer = GameObject.GetComponentInChildren<UnityEngine.SpriteRenderer>();
        }
    }
}
