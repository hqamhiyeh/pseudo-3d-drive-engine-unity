using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Settings
{
    internal class SettingChangedEventArgs : EventArgs
    {
        public string SettingName { get; private set; }
        
        public SettingChangedEventArgs(string settingName) : base()
        {
            SettingName = settingName;
        }
    }
}
