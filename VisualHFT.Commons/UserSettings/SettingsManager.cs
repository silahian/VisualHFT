using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.UserSettings
{
    /*
     USAGE:
            // To set a setting
            SettingsManager.Instance.SetSetting(SettingKey.Theme, "Dark");

            // To get a setting
            object theme = SettingsManager.Instance.GetSetting(SettingKey.Theme);

            // To save settings
            SettingsManager.Instance.SaveSettings();     
     */


    public class SettingsManager
    {
        private static readonly Lazy<SettingsManager> lazy = new Lazy<SettingsManager>(() => new SettingsManager());
        private string appDataPath;
        private string settingsFilePath;
        private JsonSerializerSettings _jsonSettings;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static SettingsManager Instance => lazy.Value;

        public UserSettings UserSettings { get; set; }

        public string GetAllSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                return json;
            }
            else
                return null;
        }
        private SettingsManager()
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settingsFilePath = Path.Combine(appDataPath, "VisualHFT", "settings.json");
            _jsonSettings = new JsonSerializerSettings
            {
                Error = HandleDeserializationError,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            // Load settings from file or create new
            LoadSettings();
        }

        private void HandleDeserializationError(object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            if (currentError.Contains("Could not convert string"))
            {
                errorArgs.ErrorContext.Handled = true;
                log.Error(errorArgs.ErrorContext.Error);
            }
        }


        private void LoadSettings()
        {
            // Deserialize from JSON file
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                UserSettings = JsonConvert.DeserializeObject<UserSettings>(json, _jsonSettings);
            }
            else
            {
                UserSettings = new UserSettings();
            }
        }
        private void SaveSettings()
        {
            try
            {

                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Serialize to JSON file
                string json = JsonConvert.SerializeObject(UserSettings);

                //first backup the file
                BackupSettings(settingsFilePath);
                // Write to file
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                log.Error($"An error occurred while saving settings: {ex.ToString()}");
            }
        }
        private void BackupSettings(string filePath)
        {
            // Check if the original file exists
            if (!File.Exists(filePath))
            {
                return; // Exit if the file does not exist
            }

            try
            {
                // Creating a backup filename with date-time stamp
                string directory = Path.GetDirectoryName(filePath);
                string filename = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string timestamp = DateTime.Now.ToString("_MMddyy_HHmm");
                string backupFilePath = Path.Combine(directory, filename + timestamp + extension);

                // Copy the file to create a backup
                File.Copy(filePath, backupFilePath, true);

                // Delete backup files older than 60 days
                var backupFiles = Directory.GetFiles(directory, filename + "_*" + extension);
                foreach (var file in backupFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-60))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                log.Error(ex);
                // Handle exceptions, possibly logging the error if required
            }
        }
        public T GetSetting<T>(SettingKey key, string id) where T : class
        {
            if (UserSettings.ComponentSettings.TryGetValue(key, out var idSettings))
            {
                if (idSettings.TryGetValue(id, out var value))
                {
                    try
                    {
                        if (value is T)
                            return value as T;
                        else
                        {
                            // Attempt to deserialize the object into the expected type
                            T typedValue = JsonConvert.DeserializeObject<T>(value.ToString());
                            if (typedValue != null)
                            {
                                return typedValue;
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
        public void SetSetting(SettingKey key, string id, object value)
        {
            if (!UserSettings.ComponentSettings.ContainsKey(key))
            {
                UserSettings.ComponentSettings[key] = new Dictionary<string, object>();
            }
            UserSettings.ComponentSettings[key][id] = value;
            SaveSettings();
        }



    }

}

