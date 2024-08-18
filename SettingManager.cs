using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace TaskBoardWf
{
    internal class SettingManager
    {
        //
        // Save
        //
        private static JsonSerializerOptions writeOptions = new JsonSerializerOptions() {
            WriteIndented = true
        };

        public static void SaveSettings<T>(T settingsObject, string path, bool escaping = true)
        {
            // Fire and forget pattern
            Task.Run(() =>
            {
                if (!escaping) writeOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

                // To minimize risk of file corruption, create temp file and then rename it
                var tempFileName = path + ".temp";
                File.WriteAllText(tempFileName, JsonSerializer.Serialize(settingsObject, writeOptions));

                try {
                    File.Delete(path);
                }
                catch (Exception e) {
                    Logger.LogError(e.Message);
                }
                try {
                    File.Move(tempFileName, path);
                }
                catch (Exception e) {
                    Logger.LogError(e.Message);
                }
                Logger.LogInfo($"SaveSettings finished: {path}");
            });
        }

        public static void SaveSettingsNoEscape<T>(T settingsObject, string path)
        {
            SaveSettings(settingsObject, path, false);
        }

        //
        // Load
        //
        private static JsonSerializerOptions readOptions = new JsonSerializerOptions() {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        public static T LoadSettings<T>(string path)
        {
            try {
                return JsonSerializer.Deserialize<T>(File.ReadAllText(path), readOptions);
            }
            catch (Exception e) {
                Logger.LogError(e.Message);
                return default;
            }
        }
    }
}
