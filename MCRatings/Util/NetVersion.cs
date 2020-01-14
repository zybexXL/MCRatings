using System;
using Microsoft.Win32;

namespace MCRatings
{
    public class SysVersions
    {
        public static string NetVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                int releaseKey = ndpKey == null ? 0 : (int)ndpKey.GetValue("Release", 0);
                if (releaseKey >= 528040) return ">=4.8";
                if (releaseKey >= 461808) return "4.7.2";
                if (releaseKey >= 461308) return "4.7.1";
                if (releaseKey >= 460798) return "4.7";
                if (releaseKey >= 394802) return "4.6.2";
                if (releaseKey >= 394254) return "4.6.1";
                if (releaseKey >= 393295) return "4.6";
                if (releaseKey >= 379893) return "4.5.2";
                if (releaseKey >= 378675) return "4.5.1";
                if (releaseKey >= 378389) return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "unknown";
            }
        }

        public static string OSVersion()
        {
            string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                string os = (string)key?.GetValue("ProductName", null);
                if (os == null) return $"{Environment.OSVersion.VersionString}{(Environment.Is64BitOperatingSystem ? " x64" : "")}";

                string rel = (string)key?.GetValue("ReleaseId", null) ?? "unknown";
                string build = (string)key?.GetValue("CurrentBuild", null) ?? "unknown";
                string bits = Environment.Is64BitOperatingSystem ? " x64" : "";

                
                return $"{os} {rel}{bits} (Build {build})";
            }
        }
    }
}
