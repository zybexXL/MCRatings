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
            Interlocked.Increment(ref Stats.Session.OMDbSearch);
            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
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
                        Interlocked.Increment(ref Stats.Session.OMDbAPICall);
                        HttpResponseMessage response = client.GetAsync($"?t={tt}{yy}{plot}&apikey={apikey}").Result;
                        lastResponse = (int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            break;
                        }
                        else
                            Interlocked.Increment(ref Stats.Session.OMDbAPIError);

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            rotateKey(); 
                    }

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

                    return result;
                }
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        public string getByIMDB(string imdb, bool full = true, bool noCache = false)
        {
            Interlocked.Increment(ref Stats.Session.OMDbGet);
            string cached = noCache ? null : Cache.Get(imdb);
            if (cached != null)
                return cached;

            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
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
                        Interlocked.Increment(ref Stats.Session.OMDbAPICall);
                        HttpResponseMessage response = client.GetAsync($"?i={imdb}{plot}&apikey={apikey}").Result;
                        lastResponse = (int)response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result = response.Content.ReadAsStringAsync().Result;
                            break;
                        }
                        else
                            Interlocked.Increment(ref Stats.Session.OMDbAPIError);

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            rotateKey();
                    }

                    // save to cache
                    if (result != null && result.Contains("Response\":\"True\""))
                        Cache.Put(imdb, result);
                    else
                        Interlocked.Increment(ref Stats.Session.OMDbAPINotFound);
                    return result;
                }
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }
    }
}
