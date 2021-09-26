using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ZRatings
{
    // Get movie info by ImdbID
    // http://www.omdbapi.com/?i=tt0133093&plot=full&apikey=xxxxxxxx&type=movie
    // Find Movie by Title + Year
    // http://www.omdbapi.com/?t=The%20Matrix&year=1999&plot=full&apikey=xxxxxxxx&type=movie


    [DataContract]
    public class OMDbMovie
    {
        [DataMember] public string Title { get; set; }
        [DataMember] public string Year { get; set; }

        [DataMember] public string imdbID { get; set; }
        [DataMember] public string imdbRating { get; set; }
        [DataMember] public string imdbVotes { get; set; }
        [DataMember] public string Metascore { get; set; }
        [DataMember] public string Rated { get; set; }
        [DataMember] public OMDbRating[] Ratings { get; set; }

        [DataMember] public string Released { get; set; }
        [DataMember] public string Runtime { get; set; }
        [DataMember] public string Genre { get; set; }
        [DataMember] public string Director { get; set; }
        [DataMember] public string Writer { get; set; }
        [DataMember] public string Actors { get; set; }
        [DataMember] public string Plot { get; set; }
        [DataMember] public string Language { get; set; }
        [DataMember] public string Country { get; set; }
        [DataMember] public string Production { get; set; }

        [DataMember] public string BoxOffice { get; set; }
        [DataMember] public string Poster { get; set; }
        [DataMember] public string Awards { get; set; }
        [DataMember] public string Website { get; set; }

        [DataMember] public string Type { get; set; }
        [DataMember] public string DVD { get; set; }
        [DataMember] public string Response { get; private set; }

        public string RottenScore { get; private set; }
        public DateTime ReleaseDate { get; private set; }

        public bool cached = false;


        public bool isValid { get { return Response == "True"; } }

        public static OMDbMovie Parse(string json)
        {
            if (json == null) return null;
            json = json.Replace("\"N/A\"", "\"\"");
            json = HttpUtility.HtmlDecode(json);        // fix some HTLM entities

            try
            {
                OMDbMovie movie = Util.JsonDeserialize<OMDbMovie>(json);
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
            switch (field)
            {
                case AppField.Title: return Title;
                case AppField.Year: return Year;
                case AppField.Release: return Released;
                case AppField.IMDbID: return imdbID;
                case AppField.IMDbRating: return imdbRating;
                case AppField.IMDbVotes: return imdbVotes;
                case AppField.RottenTomatoes: return RottenScore;
                case AppField.Metascore: return Metascore;
                case AppField.MPAARating: return Rated;
                case AppField.Runtime: return Runtime;
                case AppField.Genre: return Genre;
                case AppField.Production: return Production;
                case AppField.Director: return Director;
                case AppField.Writers: return Writer;
                case AppField.Actors: return Actors;
                case AppField.Description:
                case AppField.ShortPlot: return Plot;
                case AppField.Language: return Language;
                case AppField.Country: return Country;
                case AppField.Revenue: return BoxOffice;
                case AppField.Awards: return Awards;
                case AppField.Website: return Website;
            }
            return null;
        }

        private void fixValues()
        {
            // get RottenScore, normalize values
            RottenScore = Ratings?.Where(r => r.Source == "Rotten Tomatoes").FirstOrDefault()?.Value.Replace("%","") ?? "";
            Metascore = Metascore?.Replace("%", "");
            Runtime = Runtime?.Replace(" min", "");

            // standartize rating
            if (Rated != null)
            {
                Rated = Regex.Replace(Rated, "^(passed)$", "Passed", RegexOptions.IgnoreCase);
                Rated = Regex.Replace(Rated, "^(approved)$", "Approved", RegexOptions.IgnoreCase);
                Rated = Regex.Replace(Rated, "^(ur|not rated|unrated)$", "NR", RegexOptions.IgnoreCase);
            }

            // fix list separators
            Genre = fixList(Genre);
            Director = fixList(Director);
            Writer = fixList(Writer);
            Actors = fixList(Actors);
            Language = fixList(Language);
            Country = fixList(Country);
            Production = fixList(Production);

            // parse date, change to yyyy-MM-dd
            if (DateTime.TryParse(Released, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
            {
                // avoid jan 01 => JRiver displays only the year
                if (date.Day == 1 && date.Month == 1)
                    date = new DateTime(date.Year, 1, 2);
                ReleaseDate = date;
                Released = date.ToString("yyyy-MM-dd");
            }
            else
                Released = null;

            if (Year != null)
            {
                // make sure Year is just the year, not a full date
                Match m = Regex.Match(Year, @"(\d\d\d\d)");
                if (m.Success)
                {
                    Year = m.Groups[1].Value;
                    if (Released == null)
                    {
                        Released = $"{Year}-01-02";
                        ReleaseDate = new DateTime(int.Parse(Year), 1, 2);
                    }
                }
            }
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
            return string.Join("; ", list.Split(';').Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct());
        }
    }
}
