using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MCRatings
{
    // class to talk to TMDb - get movie info by Title/Year or by IMDB Id
    // implements caching of responses to reduce traffic and speed up processing
    public class TMDbAPI
    {
        public int lastResponse = 0;
        public bool hasKeys { get { return apikeys != null && apikeys.Count > 0; } }
        HttpClient client;

        List<string> apikeys;
        int keyIndex = 0;
        string apikey { get { return keyIndex >= apikeys.Count ? "" : apikeys[keyIndex]; } }


        public TMDbAPI()
        {
        }

        public TMDbAPI(List<string> keys)
        {
            setKeys(keys);

            var handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://api.themoviedb.org/");
        }

        ~TMDbAPI()
        {
            client.Dispose();
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

        private string HttpGetRequest(string url)
        {
            string result = null;
            //int code = -1;
            
            // try with each key
            for (int i = 0; i < apikeys.Count; i++)
            {
                Interlocked.Increment(ref Stats.Session.TMDbAPICall);
                HttpResponseMessage response = client.GetAsync($"{url}&api_key={apikey}").Result;
                lastResponse = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    break;
                }
                else
                    Interlocked.Increment(ref Stats.Session.TMDbAPIError);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    rotateKey();
            }
            return result;
        }

        public string getByTitle(string title, string year)
        {
            Interlocked.Increment(ref Stats.Session.TMDbSearch);
            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                string tt = Uri.EscapeDataString(title);
                string yy = year == null ? "" : $"&year={year}";
                string language = Program.settings.Language?.ToLower();
                if (string.IsNullOrWhiteSpace(language)) language = "en";

                // get movie ID
                string result = HttpGetRequest($"/3/search/movie?query={tt}{yy}&page=1&language={language}&include_adult=true");

                TMDbSearch found = string.IsNullOrEmpty(result) ? null : Util.JsonDeserialize<TMDbSearch>(result);
                if (found?.results == null || found.results.Length == 0)
                {
                    Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
                    return null;
                }
                int id = found.results[0].id;

                // get movie Info
                result = HttpGetRequest($"/3/movie/{id}?language={language}&append_to_response=credits,videos,images,keywords,alternative_titles,release_dates&include_image_language={language},en,null");

                if (result != null && !result.Contains("\"status_code\":"))
                {
                    var imdb = Regex.Match(result, "\"imdb_id\":\"(.+?)\"");
                    if (imdb.Success)
                    {
                        Cache.Put($"tmdb.{language}.{imdb.Groups[1].Value}", result);
                        return result;
                    }
                }
                Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        public string getByIMDB(string imdb, bool noCache = false)
        {
            Interlocked.Increment(ref Stats.Session.TMDbGet);
            string language = Program.settings.Language?.ToLower();
            //string region = Program.settings.Country?.ToLower();
            if (string.IsNullOrWhiteSpace(language)) language = "en";
            //if (string.IsNullOrWhiteSpace(region)) language = "us";

            string cached = noCache ? null : Cache.Get($"tmdb.{language}.{imdb}");
            if (cached != null)
                return cached;

            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                // find TMDBid by IMDBid
                string result = HttpGetRequest($"/3/find/{imdb}?language=en&external_source=imdb_id");

                TMDbFind found = string.IsNullOrEmpty(result) ? null : Util.JsonDeserialize<TMDbFind>(result);
                if (found?.movie_results == null || found.movie_results.Length == 0)
                {
                    Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
                    return null;
                }
                int id = found.movie_results.FirstOrDefault().id;

                // get Movie info
                result = HttpGetRequest($"/3/movie/{id}?language={language}&append_to_response=credits,videos,images,keywords,alternative_titles,release_dates&include_image_language={language},en,null");

                if (!string.IsNullOrEmpty(result) && result.Contains($"\"imdb_id\":\"{imdb.ToLower()}\"") && !result.Contains("\"status_code\":"))
                {
                    // save to cache
                    Cache.Put($"tmdb.{language}.{imdb}", result);
                    return result;
                }
                Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }
    }
}
