using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    // class to talk to OMDb - get movie info by Title/Year or by IMDB Id
    // implements caching of responses to reduce traffic and speed up processing
    public class OMDbAPI
    {
        public int lastResponse = 0;
        public bool hasKeys { get { return apikeys != null && apikeys.Count > 0; } }


        List<string> apikeys;
        int keyIndex = 0;
        string apikey { get { return keyIndex >= apikeys.Count ? "" : apikeys[keyIndex]; } }


        public OMDbAPI(List<string> keys)
        {
            setKeys(keys);
        }

        public void setKeys(List<string> keys)
        {
            apikeys = keys ?? new List<string>();
            keyIndex = 0;
        }

        private void rotateKey()
        {
            if (++keyIndex >= apikeys.Count)
                keyIndex = 0;
        }

        public string getByTitle(string title, string year, bool full = true)
        {
            lastResponse = -1;
            if (!hasKeys) return null;
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri("http://www.omdbapi.com/");
                    string tt = Uri.EscapeDataString(title);
                    string yy = year == null ? "" : $"&y={year}";
                    string plot = full ? "&plot=full" : "";
                    string result = null;
                    
                    // try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"?t={tt}{yy}{plot}&apikey={apikey}").Result;
                        lastResponse = (int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            break;
                        }
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            rotateKey(); 
                    }
                    if (string.IsNullOrEmpty(result))
                        return null;

                    // save to cache
                    if (result.Contains("Response\":\"True\""))
                    {
                        var imdb = Regex.Match(result, "\"imdbID\":\"(.+?)\",");
                        if (imdb.Success)
                            CachePut(imdb.Groups[1].Value, result);
                    }
                    return result;
                }
            }
            catch { }
            return null;
        }

        public string getByIMDB(string imdb, bool full = true, bool noCache = false)
        {
            lastResponse = 304;
            if (!noCache && CacheGet(imdb, out string cached))
                return cached;

            lastResponse = -1;
            if (!hasKeys) return null;
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri("http://www.omdbapi.com/");
                    string plot = full ? "&plot=full" : "";
                    string result = null;

                    // try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"?i={imdb}{plot}&apikey={apikey}").Result;
                        lastResponse = (int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            break;
                        }
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            rotateKey();
                    }
                    if (string.IsNullOrEmpty(result))
                        return null;

                    // save to cache
                    if (result != null && result.Contains("Response\":\"True\""))
                        CachePut(imdb, result);
                    return result;
                }
            }
            catch { }
            return null;
        }

        public void CachePut(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
                return;
            try
            {
                string path = Path.Combine(Constants.OMDBCache, key.Substring(key.Length - 1));
                Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, $"{key}.json"), data);
            }
            catch { }
        }

        public bool CacheGet(string key, out string data)
        {
            data = null;
            if (string.IsNullOrEmpty(key))
                return false;

            int maxAge = Program.settings.CacheDays;
            if (maxAge == 0) maxAge = Constants.MaxCacheDays;
            try
            {
                string path = Path.Combine(Constants.OMDBCache, key.Substring(key.Length - 1), $"{key}.json");
                FileInfo fi = new FileInfo(path);
                if (!fi.Exists) return false;
                if (maxAge < 0 || (DateTime.Now - fi.LastWriteTime).TotalDays > maxAge)
                    File.Delete(path);
                else
                {
                    data = File.ReadAllText(path);
                    return true;
                }
            }
            catch { }
            return false;
        }

    }
}
