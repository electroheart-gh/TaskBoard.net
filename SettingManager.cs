using System;
using System.IO;
using System.Text.Json;

namespace TaskBoardWf
{
    internal class SettingsManager
    {
        private string settingsFilePath = "mySettings.json"; // 設定ファイルのパス

        public void SaveSettings<T>(T settingsObject)
        {
            string json = JsonSerializer.Serialize(settingsObject, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        public T LoadSettings<T>()
        {
            try {
                string json = File.ReadAllText(settingsFilePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return default;
            }
        }
    }
}
