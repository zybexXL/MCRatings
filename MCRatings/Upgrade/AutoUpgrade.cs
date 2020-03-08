using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    public static class AutoUpgrade
    {
        const string GitHubReleaseURL = "/repos/zybexXL/MCRatings/releases/latest";

        public static VersionInfo LatestVersion;
        public static DateTime lastCheck;
        public static bool hasUpgrade { get { return LatestVersion != null && LatestVersion.version > Program.version; } }


        public static bool CheckUpgrade(bool checkOnly = false, bool noQuestions = false)
        {
            try
            {
                if (checkOnly)
                {
                    FindLatestRelease(null);
                    return hasUpgrade;
                }

                ProgressUI bar = new ProgressUI("Checking for updates", FindLatestRelease, false);
                if (bar.ShowDialog() == DialogResult.OK && bar.progress.result == true)
                {
                    if (LatestVersion.version > Program.version)
                    {
                        if (noQuestions || DialogResult.Yes == MessageBox.Show($"Version {LatestVersion.version} is now available! Do you want to update?",
                            "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                            if (UpgradeNow())
                                Application.Restart();
                            else
                                MessageBox.Show($"Update failed! Please update manually from the release page:\n{LatestVersion.url}",
                                    "Upgrade error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        MessageBox.Show("You are currently running the latest version.",
                            "No new version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return hasUpgrade;
                }
            }
            catch { }

            MessageBox.Show("Version check failed. Please check directly on the GitHub repository.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            return hasUpgrade;
        }

        public static void FindLatestRelease(ProgressInfo progress)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.github.com");
                    client.DefaultRequestHeaders.Add("User-Agent", "Microsoft .Net HttpClient");        // needed for github
                    HttpResponseMessage response = client.GetAsync(GitHubReleaseURL).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        lastCheck = DateTime.Now;
                        LatestVersion = VersionInfo.Parse(result);
                        if (progress != null)
                            progress.result = true;
                        return;
                    }
                }
            }
            catch { }
            if (progress != null)
                progress.result = false;
        }

        public static bool UpgradeNow()
        {
            Analytics.Event("GUI", "Upgrading");
            ProgressUI bar = new ProgressUI($"Updating MCRatings to v{LatestVersion.version}", DoUpgrade, false);
            return (bar.ShowDialog() == DialogResult.OK && bar.progress.result == true);
        }

        public static void DoUpgrade(ProgressInfo progress)
        {
            try
            {
                string tmpFile = Path.Combine(Path.GetTempPath(), $"MCRatings.{LatestVersion.version}.tmp");
                if (File.Exists(tmpFile)) File.Delete(tmpFile);

                using (var client = new WebClient())
                {
                    progress.subtitle = "downloading new version";
                    progress.Update();
                    client.Headers.Add("User-Agent", "Microsoft .Net HttpClient");
                    client.DownloadFile(LatestVersion.package, tmpFile);

                    progress.subtitle = "updating";
                    string currEXE = Assembly.GetEntryAssembly().Location;
                    string bakFile = Path.ChangeExtension(currEXE, ".bak");
                    if (File.Exists(bakFile)) File.Delete(bakFile);
                    File.Move(currEXE, bakFile);
                    File.Move(tmpFile, currEXE);

                    progress.result = true;
                    progress.subtitle = "starting new version";
                    Application.Restart();
                    return;
                }
            }
            catch { }
            progress.result = false;
        }

        // migrate JRatings settings to MCRatings (project name change)
        public static void MigrateSettings()
        {
            if (!Directory.Exists(Constants.DataFolder) && Directory.Exists(Constants.JRatingsFolder))
            {
                try
                {
                    Directory.CreateDirectory(Constants.DataFolder);
                    File.Copy(Path.Combine(Constants.JRatingsFolder, "settings.xml"), Constants.SettingsFile);
                    Directory.Move(Path.Combine(Constants.JRatingsFolder, "audio"), Constants.AudioCache);
                    Directory.Move(Path.Combine(Constants.JRatingsFolder, "cache"), Constants.OMDBCache);
                }
                catch { } 
            }
        }
    }
}
