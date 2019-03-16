using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    static class Program
    {
        public static Version version = new Version(1, 1, 1);          // major, minor, revision
        public static string tagline = "There can be only one!";    // changes on every major or minor release
        public static Settings settings;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AutoUpgrade.MigrateSettings();      // project renamed - migrate JRatings settings
            settings = Settings.Load();

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
            Application.Run(new MCRatingsUI());
        }
    }
}
