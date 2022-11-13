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

namespace ZRatings
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
            client.BaseAddress = new Uri($"{Constants.https}api.themoviedb.org/");
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

        public TMDbMovie getByTitle(string title, string year)
        {
            if (string.IsNullOrEmpty(title)) return null;

            Interlocked.Increment(ref Stats.Session.TMDbSearch);
            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                string tt = Uri.EscapeDataString(title);
                string yy = year == null ? "" : $"&year={year}";
                string language = Program.settings.Language?.ToLower();
                if (string.IsNullOrWhiteSpace(language)) language = "en";

                // get movie ID
                string result = HttpGetRequest($"/3/search/movie?query={tt}{yy}&page=1&include_adult=true");

                TMDbSearch found = string.IsNullOrEmpty(result) ? null : Util.JsonDeserialize<TMDbSearch>(result);
                if (found?.results == null || found.results.Length == 0)
                {
                    Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
                    return null;
                }
                // get movie Info
                return getByID(found.results[0].id, language);              
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        public TMDbMovie getByIMDB(string imdb, bool noCache = false)
        {
            if (string.IsNullOrEmpty(imdb)) return null;

            Interlocked.Increment(ref Stats.Session.TMDbGet);
            string language = Program.settings.Language?.ToLower();
            if (string.IsNullOrWhiteSpace(language)) language = "en";

            string cached = noCache ? null : Cache.Get($"tmdb.{language}.{imdb}");
            if (cached != null)
            {
                var movie = TMDbMovie.Parse(cached);
                movie.cached = true;
                if (movie.images != null && movie.images.id == movie.id)    // ignore old cached items which don't have full image lists
                    return movie;
            }

            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                // find TMDBid by IMDBid
                string result = HttpGetRequest($"/3/find/{imdb}?external_source=imdb_id");

                TMDbFind found = string.IsNullOrEmpty(result) ? null : Util.JsonDeserialize<TMDbFind>(result);
                if (found?.movie_results == null || found.movie_results.Length == 0)
                {
                    Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
                    return null;
                }

                // get movie Info
                int id = found.movie_results.FirstOrDefault().id;
                return getByID(id, language, imdb);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        public TMDbMovie getByID(string id, string imdb, bool noCache = false)
        {
            if (!int.TryParse(id, out int tmdbId)) return null;

            Interlocked.Increment(ref Stats.Session.TMDbGet);
            string language = Program.settings.Language?.ToLower();
            if (string.IsNullOrWhiteSpace(language)) language = "en";

            string cached = noCache ? null : string.IsNullOrEmpty(imdb) ? null : Cache.Get($"tmdb.{language}.{imdb}");
            if (cached != null)
            {
                var movie = TMDbMovie.Parse(cached);
                movie.cached = true;
                if (movie.images != null && movie.images.id == movie.id)    // ignore old cached items which don't have full image lists
                    return movie;
            }

            if (!hasKeys || lastResponse == (int)HttpStatusCode.Unauthorized) return null;
            try
            {
                // get movie Info
                return getByID(tmdbId, language, null);
            }
            catch { Interlocked.Increment(ref Stats.Session.AppException); }
            return null;
        }

        private TMDbMovie getByID(int id, string language = "en", string imdb = null)
        {
            // get movie Info
            var result = HttpGetRequest($"/3/movie/{id}?language={language}&append_to_response=credits,videos,keywords,alternative_titles,release_dates");//,images&include_image_language={language},en,null");
            var Movie = TMDbMovie.Parse(result);
            if (Movie != null && Movie.status_code <= 1)
            {
                // get posters
                result = HttpGetRequest($"/3/movie/{id}/images?");
                TMDbMovieImages images = Util.JsonDeserialize<TMDbMovieImages>(result);
                if (images != null && images.id == id)
                    Movie.images = images;

                if (!string.IsNullOrEmpty(Movie.imdb_id) && (imdb == null || imdb.ToLower() == Movie.imdb_id.ToLower()))
                    Cache.Put($"tmdb.{language}.{Movie.imdb_id}", Util.JsonSerialize(Movie));

                return Movie;
            }
            Interlocked.Increment(ref Stats.Session.TMDbAPINotFound);
            return null;
        }

        public static Tuple<int, int> GetThumbnailSize(PosterSize size, int width, int height)
        {
            int w = (size == PosterSize.Original ? width : size == PosterSize.Large ? 342 : size == PosterSize.Medium ? 185 : 92);
            int h = size == PosterSize.Original ? height : height * w / width;
            return new Tuple<int, int>(w, h);
        }

        public static string GetImageUrl(string uri, PosterSize size, out string cachePath, ImageType imageType = ImageType.Poster, bool addLetterSub = false)
        {
            if (uri != null && uri.StartsWith("\\\\"))
            { 
                cachePath = uri;
                return $"file:///{uri.Replace('\\','/')}";
            }
            cachePath = null;
            uri = uri?.Trim(new char[] { '/', '\\' });
            if (string.IsNullOrEmpty(uri)) return null;

            string cacheRoot = imageType == ImageType.Poster ? Constants.PosterCache : Constants.ProfileCache;
            if (addLetterSub)
            {
                char char1 = Char.ToLowerInvariant(uri[0]);
                if (Char.IsDigit(char1)) char1 = '#';
                else if (char1 < 'a' || char1 > 'z') char1 = '@';
                cacheRoot = Path.Combine(cacheRoot, $"{char1}");
            }

            string large = imageType == ImageType.Poster ? "w342" : "h632";
            string res = size == PosterSize.Large ? large : size == PosterSize.Medium ? "w185" : size == PosterSize.Small ? "w92" : "original";

            cachePath = Path.Combine(cacheRoot, res, uri);
            return $"{Constants.https}image.tmdb.org/t/p/{res}/{uri}";
        }
    }
}
