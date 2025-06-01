using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ZRatings
{
    [SupportedOSPlatform("windows")]

    public static class Util
    {
        public static string SanitizeFilename(string name, bool fixQuotes = true)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (fixQuotes)
            {
                name = Regex.Replace(name, @" ?[\\/;]", ",");
                name = name.Replace('"', '\'');
            }
            return string.Join(" ", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        }

        public static string NoSpaces(string txt) { return txt?.Replace(" ", ""); }
        public static string GetUrl(string txt) { return txt?.Replace(" ", ""); }

        // download image to temp folder
        public static Image LoadImageFromUrl(string url, string id)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!url.StartsWith("http"))
                return LoadImage(url);

            string tmp = Path.Combine(Path.GetTempPath(), "ZRatings", $"tmpImage_{id}.jpg");
            try
            {
                if (Downloader.DownloadUrl(url, tmp))
                    return LoadImage(tmp);
            }
            finally { try { File.Delete(tmp); } catch { } }
            return null;
        }

        // load an Image from file
        public static Image LoadImage(string path)
        {
            try
            {
                if (path == null || !File.Exists(path)) return null;

                byte[] data = File.ReadAllBytes(path);
                using (var ms = new MemoryStream(data))
                {
                    return Image.FromStream(ms);
                }
            }
            catch { }
            return null;
        }

        // does what is says
        public static bool ConvertToPng(string file, string dest, bool deleteOriginal = true)
        {
            try
            {
                if (Path.GetExtension(file)?.ToLower() == ".png")
                    File.Copy(file, dest, true);
                else
                    using (var image = Image.FromFile(file))
                        image.Save(dest, ImageFormat.Png);

                if (deleteOriginal)
                    try { File.Delete(file); } catch { }

                return true;
            }
            catch { }
            return false;
        }

        public static int[] IdentityArray(int size)
        {
            int[] ID = new int[size];
            for (int t = 0; t < size; t++) ID[t] = t;
            return ID;
        }

        public static long NumberValue(string strvalue)
        {
            if (string.IsNullOrEmpty(strvalue)) return 0;
            string num = Regex.Replace(strvalue, @"[^\d]", "");
            if (long.TryParse(num, out long value))
                return value;
            return 0;
        }

        // X*Y pixel count - used as a measurement of picture resolution/quality
        public static long PixelCount(string resolution)
        {
            if (string.IsNullOrEmpty(resolution)) return 0;
            var m = Regex.Match(resolution, @"(\d+)\D+(\d+)");
            if (m.Success)
                return int.Parse(m.Groups[1].Value) * int.Parse(m.Groups[2].Value);
            return 0;
        }

        public static DateTime EpochToDateTime(long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }

        public static long DateTimeToEpoch(DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        // JRiver seems to have an off-by-1 bug in the dates it uses in this old Lotus123 format
        // epoch is 30.12.1989 instead of 31.12.1899
        public static int DaysSince1900(DateTime date)
        {
            return (int)(date - new DateTime(1899, 12, 30)).TotalDays;
        }

        public static T JsonDeserialize<T>(string json, string dateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'")
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var settings = new DataContractJsonSerializerSettings()
                    {
                        DateTimeFormat = new DateTimeFormat(dateFormat),
                        UseSimpleDictionaryFormat = true,
                    };
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch { }
            return default(T);
        }

        public static string JsonSerialize<T>(T obj, string dateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'", bool indent = false)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, true, true, "  "))
                    {
                        var settings = new DataContractJsonSerializerSettings()
                        {
                            DateTimeFormat = new DateTimeFormat(dateFormat),
                            UseSimpleDictionaryFormat = true
                        };
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
                        serializer.WriteObject(writer, obj);
                        writer.Flush();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch { }
            return null;
        }

        public static string OSName()
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (RegistryKey key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                return (string)key.GetValue("ProductName", "Windows");
        }

        public static string FromBase64(string base64, bool reverse = false)
        {
            var bytes = Convert.FromBase64String(base64);
            if (reverse)
                Array.Reverse(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void ShellStart(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            try
            {
                var ps = new ProcessStartInfo(command)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
            catch { }
        }
    }
}
