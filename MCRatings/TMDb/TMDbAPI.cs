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


        List<string> apikeys;
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

        public string getByTitle(string title, string year)
        {
            lastResponse = -1;
            if (!hasKeys) return null;
            try
            {
                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri("https://api.themoviedb.org/");
                    string tt = Uri.EscapeDataString(title);
                    string yy = year == null ? "" : $"&year={year}";
                    string result = null;
                    string language = Program.settings.Language?.ToLower();
                    if (string.IsNullOrWhiteSpace(language)) language = "en";

                    // try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"/3/search/movie?api_key={apikey}&language={language}&query={tt}{yy}&page=1&include_adult=true").Result;
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

                    TMDbSearch found = Util.JsonDeserialize<TMDbSearch>(result);
                    if (found.results == null || found.results.Length == 0)
                        return null;

                    int id = found.results[0].id;
                    // get Movie info - try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"/3/movie/{id}?api_key={apikey}&language={language}&append_to_response=credits,videos,images,keywords,alternative_titles,release_dates&include_image_language={language},en,null").Result;
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
                    {
                        var imdb = Regex.Match(result, "\"imdb_id\":\"(.+?)\"");
                        if (imdb.Success)
                            Cache.Put($"tmdb.{language}.{imdb.Groups[1].Value}", result);
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
            string language = Program.settings.Language?.ToLower();
            //string region = Program.settings.Country?.ToLower();
            if (string.IsNullOrWhiteSpace(language)) language = "en";
            //if (string.IsNullOrWhiteSpace(region)) language = "us";

            string cached = noCache ? null : Cache.Get($"tmdb.{language}.{imdb}");
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
                        HttpResponseMessage response = client.GetAsync($"/3/find/{imdb}?api_key={apikey}&language=en&external_source=imdb_id").Result;
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

                    TMDbFind found = Util.JsonDeserialize<TMDbFind>(result);
                    if (found.movie_results == null || found.movie_results.Length == 0)
                        return null;

                    int id = found.movie_results.FirstOrDefault().id;

                    // get Movie info - try with each key
                    for (int i = 0; i < apikeys.Count; i++)
                    {
                        HttpResponseMessage response = client.GetAsync($"/3/movie/{id}?api_key={apikey}&language={language}&append_to_response=credits,videos,images,keywords,alternative_titles,release_dates&include_image_language={language},en,null").Result;
                        lastResponse = (int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            break;
                        }
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            rotateKey();
                    }
                    if (string.IsNullOrEmpty(result) || !result.Contains($"\"imdb_id\":\"{imdb.ToLower()}\""))
                        return null;

                    // save to cache
                    if (result != null && !result.Contains("\"status_code\":"))
                        Cache.Put($"tmdb.{language}.{imdb}", result);
                    return result;
                }
            }
            catch { }
            return null;
        }
    }
}
