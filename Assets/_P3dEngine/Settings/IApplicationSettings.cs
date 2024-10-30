using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Settings
{
    internal interface IApplicationSettings : ISettingsEventsPublisher
    {
        int TargetFrameRate { get; }
    }
}
