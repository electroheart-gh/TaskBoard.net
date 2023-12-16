using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal static class Program
    {
        static public AppSettings appSettings = new AppSettings();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var settingsManager = new SettingsManager();

            // 設定を読み込む代わりにSET
            var nameMod = new AppSettings.NameModifier();
            nameMod.Pattern = "Steam";
            nameMod.Substitution = "Steam!";
            nameMod.ForeColor = "GreenYellow";
            appSettings.NameModifiers.Add(nameMod);

            appSettings.NameModifiers.Add(new AppSettings.NameModifier { Pattern = "Steam", Substitution = "Steam!!!", ForeColor = "GreenYellow"});
            appSettings.NameModifiers.Add(new AppSettings.NameModifier { Pattern = "Task", Substitution = "", ForeColor = "Blue" });

            settingsManager.SaveSettings(appSettings);

            // 設定を読み込み
            AppSettings loadedSettings = settingsManager.LoadSettings<AppSettings>();
            if (loadedSettings != null)
            {
                foreach (var nm in loadedSettings.NameModifiers)
                {
                    Console.WriteLine($"Loaded settings - Pattern: {nm.Pattern}, Sub: {nm.Substitution}");
                }
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskBoard());
        }
    }
}
