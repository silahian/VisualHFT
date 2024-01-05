using VisualHFT.UserSettings;

namespace VisualHFT.PluginManager
{
    public interface IPlugin
    {
        string Name { get; set; }
        string Version { get; set; }
        string Description { get; set; }
        string Author { get; set; }
        ISetting Settings { get; set; }
        ePluginStatus Status { get; set; }
        Action CloseSettingWindow { get; set; }

        event EventHandler<ErrorEventArgs> OnError;


        string GetPluginUniqueID();
        object GetUISettings(); //using object type because this csproj doesn't support UI
        object GetCustomUI();   //Allow to setup own UI for the plugin
                                //using object type because this csproj doesn't support UI
    }
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string PluginName { get; set; }
        public bool IsCritical { get; set; }
    }

}
