using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.UserSettings
{
    [Serializable]
    public class UserSettings
    {
        public Dictionary<SettingKey, object> Settings { get; set; }
        public Dictionary<SettingKey, Dictionary<string, object>> ComponentSettings { get; set; }

        public UserSettings()
        {
            Settings = new Dictionary<SettingKey, object>();
            ComponentSettings = new Dictionary<SettingKey, Dictionary<string, object>>();
        }
    }


}
