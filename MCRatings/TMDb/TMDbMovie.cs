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
    // https://api.themoviedb.org/3/movie/603?api_key=0e5d83fa186fb0261cf16d58dd6f5e42&language=en&append_to_response=credits,videos,images,keywords,reviews,alternative_titles,release_dates&include_image_language=en,pt,null
    public class TMDbMovie
    {
        public int id;
        public string imdb_id;
        public string title;
        public string original_title;
        public string tagline;
        public string release_date;
        public string original_language;
        public int runtime;
        public string overview;
        public double vote_average;
        public int vote_count;
        public double popularity;
        public long budget;
        public long revenue;
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

        // converts CSV lists from OMDb into JRiver semi-coloned lists
        // deals with commas and semicolons inside parenthesis substrings
        private string fixList(string list)
        {
            if (string.IsNullOrEmpty(list)) return list;

            // replace existing semicolon by comma
            // replace comma by semicolon; ignore commas between quotes or parenthesis
            list = list.Replace(';', ',');
            list = Regex.Replace(list, @"(?<!\([^\)]+?),", ";");

            // re-join elements with semicolon
            return string.Join("; ", list.Split(';').Select(a => a.Trim()).Distinct());
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
        public string site;
        public string type;
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