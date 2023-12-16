using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal static class Program
    {
        // Global variable for configuration
        static public AppSettings appSettings = new AppSettings();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Read configuration
            var settingsManager = new SettingsManager();
            appSettings = settingsManager.LoadSettings<AppSettings>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskBoard());
        }
    }
}
