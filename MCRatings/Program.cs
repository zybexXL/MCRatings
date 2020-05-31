using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    static class Program
    {
        public static Version version = new Version(3, 2, 0);    // major, minor, revision
        public static string tagline = "\"The One Name They All Fear\" - Van Helsing (2004)";        // changes on every major or minor release
        public static Settings settings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // project renamed - migrate MCRatings settings
            if (Settings.MigrationNeeded)
            {
                new AppRenamed().ShowDialog();
                if (!Settings.MigrateSettings())
                { 
                    MessageBox.Show("Failed to migrate settings to new folder!\n\nPlease manually rename the folder:\n" +
                        $"    {Constants.MCRatingsFolder}\nto:\n    {Constants.DataFolder}", "Rename/Move failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Logger.Log($"ZRatings v{version}{(Environment.Is64BitProcess ? " x64" : "")} started");
            Logger.Log($"NetFramework version {SysVersions.NetVersion()} on {SysVersions.OSVersion()}");

            // load settings
            Directory.CreateDirectory(Constants.DataFolder);
            settings = Settings.Load();

            // start analytics
            Analytics.Init();
            Analytics.AppStart("ZRatings", version.ToString(), true);

            // remove previous version after upgrading
            AutoUpgrade.Cleanup();

            try
            {
                Application.Run(new MCRatingsUI());
            }
            catch (Exception ex) {
                Analytics.Exception(ex, true);
                MessageBox.Show($"Oops! An unhandled exception crashed ZRatings! Please send this info to the developer:\n\n{ex.ToString()}", "He's gone, baby", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(ex, "Fatal Exception!");
            }

            Analytics.AppClose();
            Logger.Log($"ZRatings closed.\n\n");
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Analytics.Exception(e.Exception);
            Logger.Log(e.Exception, "Unhandled thread exception!");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Analytics.Exception(e.ExceptionObject as Exception, e.IsTerminating);
            Logger.Log(e.ExceptionObject as Exception, $"Unhandled exception! Terminating? {e.IsTerminating}");
        }
    }
}
