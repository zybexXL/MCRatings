using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCRatings
{
    static class Cache
    {
        public static void Put(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
                return;

            Interlocked.Increment(ref Stats.Session.CacheAdd);
            try
            {
                string path = Path.Combine(Constants.OMDBCache, key.Substring(key.Length - 1));
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, $"{key}.json"), data);
            }
            catch { }
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            int maxAge = Program.settings.CacheDays;
            if (maxAge == 0) maxAge = Constants.MaxCacheDays;
            try
            {
                string path = Path.Combine(Constants.OMDBCache, key.Substring(key.Length - 1), $"{key}.json");
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                {
                    if (maxAge < 0 || (DateTime.Now - fi.LastWriteTime).TotalDays > maxAge)
                    {
                        Interlocked.Increment(ref Stats.Session.CacheExpired);
                        File.Delete(path);
                    }
                    else
                    {
                        Interlocked.Increment(ref Stats.Session.CacheHit);
                        return File.ReadAllText(path);
                    }
                }
            }
            catch { }
            Interlocked.Increment(ref Stats.Session.CacheMiss);
            return null;
        }
    }
}
