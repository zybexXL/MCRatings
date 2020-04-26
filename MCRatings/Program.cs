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
        public static Version version = new Version(3, 1);    // major, minor, revision
        public static string tagline = "\"Nothing spreads like fear\" - Contagion (2001)";        // changes on every major or minor release
        public static Settings settings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Logger.Log($"MCRatings v{version}{(Environment.Is64BitProcess ? " x64" : "")} started");
            Logger.Log($"NetFramework version {SysVersions.NetVersion()} on {SysVersions.OSVersion()}");

            // load settings
            AutoUpgrade.MigrateSettings();      // project renamed - migrate JRatings settings
            settings = Settings.Load();

            // start analytics
            Analytics.Init();
            Analytics.AppStart("MCRatings", version.ToString(), true);

            // delete .bak file from previous upgrade
            string currEXE = Assembly.GetEntryAssembly().Location;
            string bakFile = Path.ChangeExtension(currEXE, ".bak");
            if (File.Exists(bakFile)) try { File.Delete(bakFile); } catch { }
            // cleanup old JRatings binaries
            string jratings = Path.Combine(Path.GetDirectoryName(currEXE), "JRatings.exe");
            if (File.Exists(jratings)) try { File.Delete(jratings); } catch { }
            bakFile = Path.ChangeExtension(jratings, ".bak");
            if (File.Exists(bakFile)) try { File.Delete(bakFile); } catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MCRatingsUI());
            }
            catch (Exception ex) {
                Analytics.Exception(ex, true);
                MessageBox.Show($"Oops! An unhandled exception crashed MCRatings! Please send this info to the developer:\n\n{ex.ToString()}", "He's gone, baby", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(ex, "Fatal Exception!");
            }

            Analytics.AppClose();
            Logger.Log($"MCRatings closed.\n\n");
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
