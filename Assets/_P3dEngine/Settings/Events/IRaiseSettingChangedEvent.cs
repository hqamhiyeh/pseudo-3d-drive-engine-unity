using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Settings
{
    internal interface IRaiseSettingChangedEvent
    {
#nullable enable
        event EventHandler<SettingChangedEventArgs>? SettingChanged;
#nullable disable
    }
}