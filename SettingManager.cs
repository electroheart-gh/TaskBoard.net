using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return default;
            }
        }
    }
}
