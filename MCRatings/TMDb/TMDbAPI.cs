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
    // class to talk to TMDb - get movie info by Title/Year or by IMDB Id
    // implements caching of responses to reduce traffic and speed up processing
    public class TMDbAPI
    {
        public int lastResponse = 0;
        public bool hasKeys { get { return apikeys != null && apikeys.Count > 0; } }


        List<string> apikeys;// = new List<string>() { "0e5d83fa186fb0261cf16d58dd6f5e42" };
        int keyIndex = 0;
        string apikey { get { return keyIndex >= apikeys.Count ? "" : apikeys[keyIndex]; } }


        public TMDbAPI()
        {
        }

        public TMDbAPI(List<string> keys)
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
            return null;    // not implemented

            lastResponse = -1;
            if (!hasKeys) return null;
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri("https://api.themoviedb.org/");
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
                            Cache.Put("tmdb."+imdb.Groups[1].Value, result);
                    }
                    return result;
                }
            }
            catch { }
            return null;
        }

        public string getByIMDB(string imdb, bool noCache = false)
        {
            lastResponse = 304;
            string cached = noCache ? null : Cache.Get("tmdb."+imdb);
            if (cached != null)
                return cached;

            lastResponse = -1;
            if (!hasKeys) return null;
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri("https://api.themoviedb.org/");
                    string result = null;

                    // find TMDBid by IMDBid - try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"/3/find/{imdb}?api_key={apikey}&language=en-US&external_source=imdb_id").Result;
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

                    Match m = Regex.Match(result, @"""id"":(\d+),");
                    if (!m.Success) return null;
                    int id = int.Parse(m.Groups[1].Value);

                    // get Movie info - try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"/3/movie/{id}?api_key={apikey}&language=en-US&append_to_response=credits,videos,images,keywords").Result;
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
                    if (result != null && !result.Contains("\"status_code\":"))
                        Cache.Put("tmdb."+imdb, result);
                    return result;
                }
            }
            catch { }
            return null;
        }
    }
}
