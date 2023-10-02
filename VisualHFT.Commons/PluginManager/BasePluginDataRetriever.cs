using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VisualHFT.DataRetriever;
using VisualHFT.PluginManager;
using VisualHFT.UserSettings;
using static System.Net.Mime.MediaTypeNames;

namespace VisualHFT.Commons.PluginManager
{
    public abstract class BasePluginDataRetriever : IDataRetriever, IPlugin, IDisposable
    {
        private Dictionary<string, string> parsedNormalizedSymbols;

        public abstract string Name { get; set; }
        public abstract string Version { get; set; }
        public abstract string Description { get; set; }
        public abstract string Author { get; set; }
        public abstract ISetting Settings { get; set; }
        public ePluginStatus Status { get; set; }
        public abstract Action CloseSettingWindow { get; set; }

        protected abstract void LoadSettings();
        protected abstract void SaveSettings();
        protected abstract void InitializeDefaultSettings();
        


        public event EventHandler<DataEventArgs> OnDataReceived;
        public event EventHandler<VisualHFT.PluginManager.ErrorEventArgs> OnError;

        protected bool _disposed = false; // to track whether the object has been disposed

        public BasePluginDataRetriever()
        {
            // Ensure settings are loaded before starting
            LoadSettings();
            if (Settings == null)
                throw new InvalidOperationException($"{Name} plugin settings has not been loaded.");

            Status = ePluginStatus.LOADED;
        }
        public virtual void Start()
        {
            Status = ePluginStatus.STARTED;
        }
        public virtual void Stop()
        {
            Status = ePluginStatus.STOPPED;
        }
        protected virtual void RaiseOnDataReceived(DataEventArgs args)
        {
            OnDataReceived?.Invoke(this, args);
        }
        public virtual void RaiseOnError(VisualHFT.PluginManager.ErrorEventArgs args)
        {
            OnError?.Invoke(this, args);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Common disposal logic if any
        }
        protected void SaveToUserSettings(ISetting settings)
        {
            UserSettings.SettingsManager.Instance.SetSetting(SettingKey.PLUGIN, GetPluginUniqueID(), settings);
        }
        protected T LoadFromUserSettings<T>() where T : class
        {
            var jObject = UserSettings.SettingsManager.Instance.GetSetting<object>(SettingKey.PLUGIN, GetPluginUniqueID()) as Newtonsoft.Json.Linq.JObject;
            if (jObject != null)
            {
                return jObject.ToObject<T>();
            }
           return null;
        }
        public virtual string GetPluginUniqueID()
        {
            // Get the fully qualified name of the assembly
            string assemblyName = GetType().Assembly.FullName;

            // Concatenate the attributes
            string combinedAttributes = $"{Name}{Author}{Version}{Description}{assemblyName}";

            // Compute the SHA256 hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedAttributes));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public abstract object GetUISettings(); //using object type because this csproj doesn't support UI


        #region Symbol Normalization functions
        // 1. Parsing Method
        protected void ParseSymbols(string input)
        {
            parsedNormalizedSymbols = new Dictionary<string, string>();

            var entries = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(new[] { '(' }, StringSplitOptions.RemoveEmptyEntries);

                var symbol = parts[0].Trim();
                var normalizedSymbol = parts.Length > 1 ? parts[1].Trim(' ', ')') : null;

                parsedNormalizedSymbols[symbol] = normalizedSymbol;
            }
        }

        protected List<string> GetAllNonNormalizedSymbols()
        {
            if (parsedNormalizedSymbols == null)
                return new List<string>();
            return parsedNormalizedSymbols.Keys.ToList();
        }
        // 2. Normalization Method
        protected string GetNormalizedSymbol(string inputSymbol)
        {
            if (parsedNormalizedSymbols == null)
                return string.Empty;
            if (parsedNormalizedSymbols.ContainsKey(inputSymbol))
            {
                return parsedNormalizedSymbols[inputSymbol] ?? inputSymbol;
            }
            return inputSymbol;  // return the original symbol if no normalization is found.
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
