using Assets._P3dEngine.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine.Shaders
{
    internal class Pseudo3dShaderData
    {
        public Material Material;
        public Camera Camera;
        public RenderWindow Window;
        public IRendererSettings RendererSettings;
    }
}
