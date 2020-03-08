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
    // class to talk to OMDb - get movie info by Title/Year or by IMDB Id
    // implements caching of responses to reduce traffic and speed up processing
    public class OMDbAPI
    {
        public int lastResponse = 0;
        public bool hasKeys { get { return apikeys != null && apikeys.Count > 0; } }
        HttpClient client;

        List<string> apikeys;
        int keyIndex = 0;
        string apikey { get { return keyIndex >= apikeys.Count ? "" : apikeys[keyIndex]; } }


        public OMDbAPI(List<string> keys)
        {
            setKeys(keys);

            var handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            client = new HttpClient(handler);
            client.BaseAddress = new Uri("http://www.omdbapi.com/");
        }

        ~OMDbAPI()
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
            int code = -1;
            // try with each key
            for (int i = 0; i < apikeys.Count; i++)
            {
                // check if another thread got Unauthorized
                if (lastResponse == (int)HttpStatusCode.Unauthorized) return null;
                string key = apikey;

                Interlocked.Increment(ref Stats.Session.OMDbAPICall);
                HttpResponseMessage response = client.GetAsync($"{url}&apikey={key}").Result;
                code = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    lastResponse = code;
                    return result;
                }
                else
                    Interlocked.Increment(ref Stats.Session.OMDbAPIError);

                lock (this)
                {
                    if (lastResponse == (int)HttpStatusCode.Unauthorized)
                        break;
                    if (response.StatusCode == HttpStatusCode.Unauthorized && apikeys.Count > 1 && key == apikey) // check if another thread rotated it already
                        rotateKey();
                }
            }
            lock(this)
                if (lastResponse != (int)HttpStatusCode.Unauthorized)
                    lastResponse = code;
            return result;
        }

        public OMDbMovie getByTitle(string title, string year, bool full = true)
        {
            Interlocked.Increment(ref Stats.Session.OMDbSearch);
            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                string tt = Uri.EscapeDataString(title);
                string result = HttpGetRequest($"?t={tt}{(year == null ? "" : $"&y={year}")}{(full ? "&plot=full" : "")}");

                bool found = false;
                if (result != null && result.Contains("Response\":\"True\""))
                {
                    var imdb = Regex.Match(result, "\"imdbID\":\"(.+?)\",");
                    found = imdb.Success;
                    // save to cache
                    if (found)
                        Cache.Put(imdb.Groups[1].Value, result);
                }
                if (!found)
                    Interlocked.Increment(ref Stats.Session.OMDbAPINotFound);

                return OMDbMovie.Parse(result);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        public OMDbMovie getByIMDB(string imdb, bool full = true, bool noCache = false)
        {
            Interlocked.Increment(ref Stats.Session.OMDbGet);
            string cached = noCache ? null : Cache.Get(imdb);
            if (cached != null)
            {
                var movie = OMDbMovie.Parse(cached);
                movie.cached = true;
                return movie;
            }

            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                string result = HttpGetRequest($"?i={imdb}{(full ? "&plot=full" : "")}");

                // save to cache
                if (result != null && result.Contains("Response\":\"True\""))
                    Cache.Put(imdb, result);
                else
                    Interlocked.Increment(ref Stats.Session.OMDbAPINotFound);
                return OMDbMovie.Parse(result);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }
    }
}
