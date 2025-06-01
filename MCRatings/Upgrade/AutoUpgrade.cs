using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    public static class AutoUpgrade
    {
        const string GitHubReleaseAPI = "/repos/zybexXL/MCRatings/releases/latest";
        
        public static VersionInfo LatestVersion;
        public static DateTime lastCheck;
        public static bool restartNeeded = false;

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
                        if (noQuestions || DialogResult.Yes == MessageBox.Show($"Version {LatestVersion.version} is now available! Do you want to upgrade?",
                            "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
#if AUTOUPGRADE
                            if (UpgradeNow())
                                return true;
                            else
                                MessageBox.Show($"Upgrade failed! Please upgrade manually from the release page:\n{LatestVersion.url}",
                                    "Upgrade error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                            Util.ShellStart($"{Constants.https}{GitHubReleaseURL}");
#endif
                        else
                            MessageBox.Show("You are currently running the latest version.",
                            "No new version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
                    client.BaseAddress = new Uri($"{Constants.https}api.github.com");
                    client.DefaultRequestHeaders.Add("User-Agent", "Microsoft .Net HttpClient");        // needed for github
                    HttpResponseMessage response = client.GetAsync(GitHubReleaseAPI).Result;
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

#if AUTOUPGRADE
        public static bool UpgradeNow()
        {
            Analytics.Event("GUI", "Upgrading");
            ProgressUI bar = new ProgressUI($"Upgrading ZRatings to v{LatestVersion.version}", DoUpgrade, false);
            return (bar.ShowDialog() == DialogResult.OK && bar.progress.result == true);
        }

        public static void DoUpgrade(ProgressInfo progress)
        {
            progress.result = false;
            try
            {
                string tmpFile = Path.Combine(Path.GetTempPath(), $"ZRatings.{LatestVersion.version}.tmp");
                if (File.Exists(tmpFile)) File.Delete(tmpFile);

                using (var client = new HttpClient())
                {
                    progress.subtitle = "downloading new version";
                    progress.Update();
                    client.DefaultRequestHeaders.Add("User-Agent", "Microsoft .Net HttpClient");
                    var response = client.GetAsync(LatestVersion.package).Result;
                    using (var fs = new FileStream(tmpFile, FileMode.Create))
                        response.Content.CopyToAsync(fs).Wait();

                    progress.subtitle = "upgrading";
                    string currEXE = Application.ExecutablePath;
                    string bakFile = Path.ChangeExtension(currEXE, ".bak");
                    if (File.Exists(bakFile)) File.Delete(bakFile);

                    File.Move(currEXE, bakFile);
                    File.Move(tmpFile, currEXE);

                    progress.result = true;
                    restartNeeded = true;
                    progress.subtitle = "starting new version";
                }
            }
            catch { }
        }

        public static void Cleanup()
        {
            // delete .bak file from previous upgrade
            string currEXE = Application.ExecutablePath;
            string bakFile = Path.ChangeExtension(currEXE, ".bak");
            if (File.Exists(bakFile)) try { File.Delete(bakFile); } catch { }
        }
#endif
    }
}
