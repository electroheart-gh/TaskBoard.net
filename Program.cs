using System;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal static class Program
    {
        // Global variable for configuration
        static public AppSettings appSettings = new AppSettings();

        // Constant for configuration
        private const string configFilePath = "mySettings.json";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Read configuration
            appSettings = SettingManager.LoadSettings<AppSettings>(configFilePath);
            if (appSettings is null) {
                appSettings = new AppSettings();
            }
            // Save configuration
            SettingManager.SaveSettingsNoEscape(appSettings, configFilePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskBoard());
        }
    }
}
