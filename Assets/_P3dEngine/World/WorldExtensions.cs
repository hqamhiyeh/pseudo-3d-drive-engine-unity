using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine
{
    internal static class WorldExtensions
    {
        public static void Generate(this World world)
        {
            world.Road.Initialize();
        }
    }
}
