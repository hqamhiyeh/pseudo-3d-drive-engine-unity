using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Settings
{
    internal interface IGeneratorSettings : IRaiseSettingChangedEvent
    {
        int WorldUnitsPerUnit { get; }
        int DrawDistance { get; }
    }
}
