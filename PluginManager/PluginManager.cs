using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VisualHFT.Commons.Studies;
using VisualHFT.DataRetriever;

namespace VisualHFT.PluginManager
{
    public static class PluginManager
    {
        private static List<IPlugin> ALL_PLUGINS = new List<IPlugin>();
        private static object _locker = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LoadPlugins()
        {
            // 1. By default load all dll's in current Folder. 
            var pluginsDirectory = AppDomain.CurrentDomain.BaseDirectory; // This gets the directory where your WPF app is running
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_locker)
                    LoadPluginsByDirectory(pluginsDirectory);
            });

            // 3. Load Other Plugins in different folders

            // 4. If is Started, then Start

            // 5. If empty or Stopped. Do nothing.
        }
        public static List<IPlugin> AllPlugins { get { lock (_locker) return ALL_PLUGINS; } }
        public static bool AllPluginsReloaded { get; internal set; }

        public static void StartPlugins()
        {
            lock (_locker)
            {
                if (ALL_PLUGINS.Count == 0) { return; }
                foreach (var plugin in ALL_PLUGINS)
                {
                    StartPlugin(plugin);
                }
            }
        }

        public static void StartPlugin(IPlugin plugin)
        {
            try
            {
                if (plugin != null)
                {
                    if (plugin is IDataRetriever dataRetriever)
                    {
                        //DATA RETRIEVER = WEBSOCKETS
                        var processor = new VisualHFT.DataRetriever.DataProcessor(dataRetriever);
                        dataRetriever.StartAsync();
                    }
                    else if (plugin is IStudy study)
                    {
                        study.StartAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void StopPlugin(IPlugin plugin)
        {
            try
            {
                if (plugin != null)
                {
                    if (plugin is IDataRetriever dataRetriever)
                    {
                        //DATA RETRIEVER = WEBSOCKETS
                        var processor = new VisualHFT.DataRetriever.DataProcessor(dataRetriever);
                        dataRetriever.StopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void SettingPlugin(IPlugin plugin)
        {
            UserControl _ucSettings = null;
            try
            {
                if (plugin != null)
                {
                    var formSettings = new View.PluginSettings();
                    plugin.CloseSettingWindow = () => {
                        formSettings.Close();
                    };

                    _ucSettings = plugin.GetUISettings() as UserControl;
                    if (_ucSettings == null)
                    {
                        plugin.CloseSettingWindow = null;
                        formSettings = null;
                        return;
                    }
                    formSettings.MainGrid.Children.Add(_ucSettings);
                    formSettings.Title = $"{plugin.Name} Settings";
                    formSettings.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    formSettings.Topmost = true;
                    formSettings.ShowInTaskbar = false;
                    formSettings.ShowDialog();
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static void UnloadPlugins()
        {
            lock (_locker)
            {
                if (ALL_PLUGINS.Count == 0) { return; }
                foreach (var plugin in ALL_PLUGINS.OfType<IDisposable>())
                {
                    plugin.Dispose();
                }
            }
        }


        private static void LoadPluginsByDirectory(string pluginsDirectory)
        {
            foreach (var file in Directory.GetFiles(pluginsDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    foreach (var type in assembly.GetExportedTypes())
                    {
                        if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IPlugin)))
                        {
                            var plugin = Activator.CreateInstance(type) as IPlugin;
                            if (string.IsNullOrEmpty(plugin.Name))
                                continue;
                            ALL_PLUGINS.Add(plugin);
                            plugin.OnError += Plugin_OnError;
                            log.Info("Plugins: " + plugin.Name + " loaded OK.");
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw new Exception($"Plugin {file} has failed to load. Error: " + ex.Message);
                }

            }

        }
        private static void Plugin_OnError(object? sender, ErrorEventArgs e)
        {
            if (e.IsCritical)
            {
                log.Error(e.PluginName, e.Exception);
                Helpers.HelperCommon.GLOBAL_DIALOGS["error"](e.Exception.Message, e.PluginName);
            }
            else
            {
                //LOG error
                log.Error(e.PluginName, e.Exception);
            }

        }

    }
}
