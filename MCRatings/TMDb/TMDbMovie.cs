using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    // Get Movie info + Credits, Keywords, Trailers, Images, Alternative Titles, Release Dates
    // multiple languages supported (fallback to english)
    // https://api.themoviedb.org/3/movie/603?api_key={key}&language=en&append_to_response=credits,videos,images,keywords,reviews,alternative_titles,release_dates&include_image_language=en,pt,null
    public class TMDbMovie
    {
        public int id;
        public string imdb_id;
        public string title;
        public string original_title;
        public string tagline;
        public string release_date;
        public string original_language;
        public int? runtime;
        public string overview;
        public double? vote_average;
        public int? vote_count;
        public double? popularity;
        public long? budget;
        public long? revenue;
        public string homepage;
        public string status;
        public bool video;
        public bool adult;
        public string backdrop_path;
        public string poster_path;
        public TMDbMovieCollection belongs_to_collection;
        public TMDbMovieKeyPair[] genres;
        public TMDbMovieCompany[] production_companies;
        public TMDbMovieCountry[] production_countries;
        public TMDbMovieLanguage[] spoken_languages;
        public TMDbMovieCredits credits;
        public TMDbMovieVideos videos;
        public TMDbMovieImages images;
        public TMDbMovieKeywords keywords;
        public TMDbMovieReviews reviews;
        public TMDbMovieTitles alternative_titles;
        public TMDbMovieReleases release_dates;

        public int status_code = 0;         // error code
        public string status_message;       // error message

        public bool isValid { get { return status_code == 0 && id > 0; } }

        public static TMDbMovie Parse(string json)
        {
            if (json == null) return null;
            try
            {
                TMDbMovie movie = Util.JsonDeserialize<TMDbMovie>(json);
                if (!movie.isValid) return null;

                movie.fixValues();
                return movie;
            }
            catch { }
            return null;
        }

        public string Get(AppField field)
        {
            if (!isValid) return null;
            int listItems = Program.settings.ListItemsLimit;
            if (listItems <= 0) listItems = 1000;

            switch (field)
            {
                case AppField.Title: return title;
                case AppField.Year:
                    string release = release_dates.getEarliestReleaseDate(release_date);
                    if (DateTime.TryParseExact(release, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        return date.Year.ToString();
                    else
                        return null;
                case AppField.Release: return release_dates.getEarliestReleaseDate(release_date); // release_dates?.getReleaseDate(Program.settings.Country) ?? release_date;
                case AppField.IMDbID: return imdb_id;
                case AppField.TMDbScore: return vote_average.HasValue && vote_average > 0 ? vote_average.Value.ToString("0.0").Replace(",", ".") : null;
                case AppField.MPAARating: return release_dates?.getCertification("US"); // Program.settings.Country) ?? release_dates?.getCertification("US");
                case AppField.Runtime: return runtime.HasValue && runtime.Value > 0 ? runtime.Value.ToString() : null;
                case AppField.Genre: return genres == null ? null : fixList(genres.Select(c => c.name.Replace("Science Fiction","Sci-Fi")), listItems);
                case AppField.OriginalTitle: return original_title;
                case AppField.Series: return Regex.Replace(belongs_to_collection?.name ?? "", @"\s*(collection)\s*$", "", RegexOptions.IgnoreCase);
                //case AppField.Production: return production_companies == null ? null : string.Join("; ", production_companies.Select(c => c.name).Take(listItems));
                case AppField.Director: return credits?.crew == null ? null : fixList(credits.crew.Where(c=>c.job == "Director").Select(c => c.name), listItems);
                case AppField.Writers: return credits?.crew == null ? null : fixList(credits.crew.Where(c => c.department == "Writing").Select(c => c.name), listItems);
                case AppField.Producer: return credits?.crew == null ? null : fixList(credits.crew.Where(c => c.job == "Producer").Select(c => c.name), listItems);
                case AppField.Actors: return credits?.cast == null ? null : fixList(credits.cast.OrderBy(c => c.order).Select(c => c.name), listItems);
                case AppField.Keywords: return keywords?.keywords == null ? null : fixCase(fixList(keywords?.keywords.Select(c => c.name), removeCJK: true));
                case AppField.Tagline: return tagline;
                case AppField.Description: return overview;
                case AppField.Language: return spoken_languages == null ? null : fixList(spoken_languages.Select(c => c.englishName), listItems);
                case AppField.Country: return production_countries == null ? null : fixList(production_countries.Select(c => c.name.Replace("United States of America","USA").Replace("United Kingdom", "UK")), listItems);
                case AppField.Budget: return budget.HasValue && budget.Value > 0 ? budget.Value.ToString("$#,##0").Replace(".",",") : null;
                case AppField.Revenue: return revenue.HasValue && revenue.Value > 0 ? revenue.Value.ToString("$#,##0").Replace(".", ",") : null;
                case AppField.Website: return homepage;
                case AppField.Trailer:
                    var video = videos?.results?.FirstOrDefault(v => v.type.ToLower() == "trailer");
                    if (video == null) video = videos?.results?.FirstOrDefault(v => v.type.ToLower() == "clip");
                    if (video != null)
                    {
                        if (Program.settings.WebmediaURLs)
                            return $"webmedia://{video.site}/{video.key}";
                        switch (video.site.ToLower())
                        {
                            case "youtube": return $"https://www.youtube.com/watch?v={video.key}";
                            case "vimeo": return $"https://vimeo.com/{video.key}";
                            default: return $"webmedia://{video.site}/{video.key}";
                        }
                    }
                    break;
            }
            return null;
        }

        private void fixValues()
        {
            // get RottenScore, normalize values
            //RottenScore = Ratings?.Where(r => r.Source == "Rotten Tomatoes").FirstOrDefault()?.Value.Replace("%", "") ?? "";
            //Metascore = Metascore?.Replace("%", "");
            //Runtime = Runtime?.Replace(" min", "");

            //// standartize rating
            //if (Rated != null)
            //{
            //    Rated = Regex.Replace(Rated, "^(passed)$", "Passed", RegexOptions.IgnoreCase);
            //    Rated = Regex.Replace(Rated, "^(approved)$", "Approved", RegexOptions.IgnoreCase);
            //    Rated = Regex.Replace(Rated, "^(ur|not rated|unrated)$", "NR", RegexOptions.IgnoreCase);
            //}

            //// fix list separators
            //Genre = fixList(Genre);
            //Director = fixList(Director);
            //Writer = fixList(Writer);
            //Actors = fixList(Actors);
            //Language = fixList(Language);
            //Country = fixList(Country);
            //Production = fixList(Production);

            //// parse date, change to yyyy-MM-dd
            //if (DateTime.TryParse(Released, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
            //{
            //    // avoid jan 01 => JRiver displays only the year
            //    if (date.Day == 1 && date.Month == 1)
            //        date = new DateTime(date.Year, 1, 2);
            //    ReleaseDate = date;
            //    Released = date.ToString("yyyy-MM-dd");
            //}
            //else
            //    Released = null;

            //if (Year != null)
            //{
            //    // make sure Year is just the year, not a full date
            //    Match m = Regex.Match(Year, @"(\d\d\d\d)");
            //    if (m.Success)
            //    {
            //        Year = m.Groups[1].Value;
            //        if (Released == null)
            //        {
            //            Released = $"{Year}-01-02";
            //            ReleaseDate = new DateTime(int.Parse(Year), 1, 2);
            //        }
            //    }
            //}
        }

        // cleanup list, remove duplicates, return semicolon-separated string
        // deals with commas and semicolons inside parenthesis substrings
        private string fixList(IEnumerable<string> items, int limit=100, bool removeCJK = false)
        {
            List<string> list = items.ToList();
            if (list == null || list.Count == 0) return null;

            if (removeCJK)
                list = list.Where(l => !hasCJKIdeographs(l)).ToList();
            // replace comma by semicolon; ignore commas between quotes or parenthesis
            //list = Regex.Replace(list, @"(?<!\([^\)]+?),", ";");

            // re-join elements with semicolon
            return string.Join("; ", list.Select(i => i.Replace(';', ',').Trim()).Where(s=>!string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.CurrentCultureIgnoreCase).Take(limit));
        }

        private bool hasCJKIdeographs(string s)
        {
            return Regex.IsMatch(s, @"\p{IsCJKUnifiedIdeographs}");
        }

        private string fixCase(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return null;
            txt = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(txt);

            // fix uppercase after numbers (1970S, 21St, ...)
            txt = Regex.Replace(txt, @"\d([A-Z])", delegate (Match m) { return m.Value.ToLower(); });
            txt = Regex.Replace(txt, @"\b(ad|bc|3d|usa|uk)\b", delegate (Match m) { return m.Value.ToUpper(); }, RegexOptions.IgnoreCase);

            return txt;
        }
    }

    public class TMDbMovieKeyPair
    {
        public int id;
        public string name;
    }
    public class TMDbMovieCompany
    {
        public int id;
        public string name;
        public string logo_path;
        public string origin_country;
    }

    public class TMDbMovieCountry
    {
        public string iso_3166_1;   // country
        public string name;
    }

    public class TMDbMovieLanguage
    {
        public string iso_639_1;
        public string name;

        public string englishName { get { 
            return iso639.GetName(iso_639_1) ?? (string.IsNullOrEmpty(name) ? $"[{iso_639_1}]" : name.Replace("No Language","None")); } }
    }

    public class TMDbMovieCredits
    {
        public TMDbMovieCast[] cast;
        public TMDbMovieCrew[] crew;
    }

    public class TMDbMovieCast
    {
        public int cast_id;
        public string character;
        public string credit_id;
        public int gender;
        public int id;
        public string name;
        public int order;
        public string profile_path;
    }

    public class TMDbMovieCrew
    {
        public string department;
        public string credit_id;
        public int gender;
        public int id;
        public string job;
        public string name;
        public string profile_path;
    }

    public class TMDbMovieVideos
    {
        public TMDbMovieVideo[] results;
    }

    public class TMDbMovieVideo
    {
        public string id;
        public string iso_639_1;   // language
        public string iso_3166_1;  // country
        public string key;         // youtube video ID
        public string name;
        public string site;        // "YouTube"
        public string type;        // "Trailer"
        public int size;
    }

    public class TMDbMovieImages
    {
        public TMDbMovieImage[] backdrops;
        public TMDbMovieImage[] posters;
    }

    public class TMDbMovieImage
    {
        public string file_path;
        public double aspect_ratio;
        public int height;
        public int width;
        public int vote_count;
        public double vote_average;
        public string iso_639_1;       // language
    }

    public class TMDbMovieKeywords
    {
        public TMDbMovieKeyPair[] keywords;
    }

    public class TMDbMovieReleases
    {
        public TMDbMovieRelease[] results;

        public string getReleaseDate(string country)
        {
            if (results == null) return null;
            try
            {
                var r = results?.Where(c => c.iso_3166_1 == country.ToUpper()).Select(c => c.release_dates).FirstOrDefault();
                string date = r?.OrderBy(c => c.type).FirstOrDefault()?.release_date.Substring(0, 10);
                if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    if (dt.Month == 1 && dt.Day == 1) dt = new DateTime(dt.Year, 1, 2);
                    return dt.ToString("yyyy-MM-dd");
                }
            }
            catch { }
            return null;
        }

        public string getEarliestReleaseDate(string defaultDate)
        {
            if (results == null) return null;
            try
            {
                var r = results?.SelectMany(c => c.release_dates).Select(c => c.release_date.Substring(0, 10)).ToList();
                if (r == null) return defaultDate;
                r.Add(defaultDate);
                r.Sort();
                if (DateTime.TryParseExact(r[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    if (dt.Month == 1 && dt.Day == 1) dt = new DateTime(dt.Year, 1, 2);
                    return dt.ToString("yyyy-MM-dd");
                }
            }
            catch { }
            return defaultDate;
        }

        public string getCertification(string country)
        {
            if (results == null) return null;
            var countryRel = results?.Where(c => c.iso_3166_1 == country.ToUpper()).Select(c=>c.release_dates).FirstOrDefault();
            var cert = countryRel?.Where(c => !string.IsNullOrEmpty(c.certification)).Select(c => c.certification).FirstOrDefault();
            if (cert != null) return cert;

            var first = results.SelectMany(c => c.release_dates).Where(c=> !string.IsNullOrEmpty(c.certification) && !string.IsNullOrEmpty(c.iso_639_1)).FirstOrDefault();
            return first?.certification;
        }
    }

    public class TMDbMovieRelease
    {
        public string iso_3166_1;      // country
        public TMDbMovieReleaseInfo[] release_dates;
    }

    public class TMDbMovieReleaseInfo
    {
        public string iso_639_1;       // language
        public string certification;
        public string note;
        public string release_date;    // "1999-06-23T00:00:00.000Z"
        public int type;
    }

    public class TMDbMovieCollection
    {
        public int id;
        public string name;
        public string poster_path;
        public string backdrop_path;
    }

    public class TMDbMovieTitles
    {
        public string iso_3166_1;      // country
        public string title;
        public string type;
    }

    public class TMDbMovieReviews
    {
        public int page;
        public int total_pages;
        public int total_results;
        public TMDbMovieReview[] results;
    }

    public class TMDbMovieReview
    {
        public string id;
        public string author;
        public string content;
        public string url;
    }
}