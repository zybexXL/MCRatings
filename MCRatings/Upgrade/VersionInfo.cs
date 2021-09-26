using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZRatings
{
    // deserializes GitHub release response
    public class VersionInfo
    {
        public Version version;         // "tag_name": "0.9.1",
        public string url;              // "html_url": "https://github.com/zybexXL/MCRatings/releases/tag/1.0",
        public string title;            // "name": "Initial public release",
        public string body;             // "body": "First public release!"
        public int size;                // "size": 296960,
        public DateTime date;           // "published_at": "2019-02-27T21:36:09Z",
        public string package;          // "browser_download_url": "https://github.com/zybexXL/MCRatings/releases/download/1.0/MCRatings.exe"


        // parse GitHub release JSON
        public static VersionInfo Parse(string json)
        {
            if (json == null) return null;
            VersionInfo ver = new VersionInfo();

            try
            {
                // quick and dirty JSON extraction
                Match m1 = Regex.Match(json, @"""tag_name"": ?""v?([\d\.]+)""");
                ver.version = Version.Parse(m1.Groups[1].Value);

                m1 = Regex.Match(json, @"""html_url"": ?""(.+?)""");
                ver.url = m1.Groups[1].Value;

                m1 = Regex.Match(json, @"""name"": ?""(.+?)""");
                ver.title = m1.Groups[1].Value;

                m1 = Regex.Match(json, @"""body"": ?""(.+?)""");
                ver.body = m1.Groups[1].Value;

                m1 = Regex.Match(json, @"""size"": ?(\d+),");
                ver.size = int.Parse(m1.Groups[1].Value);

                m1 = Regex.Match(json, @"""published_at"": ?""(.+?)""");
                ver.date = DateTime.ParseExact(m1.Groups[1].Value, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                m1 = Regex.Match(json, @"""browser_download_url"": ?""(.+?)""");
                ver.package = m1.Groups[1].Value;

                return ver;
            }
            catch { }
            return null;
        }
    }
}
