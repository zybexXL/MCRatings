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
    // Parses OMDB JSON responses and stores movie info
    public class OMDbInfo
    {
        private string JSON;
        public string IMDBid;
        public string year;
        public string title;

        public string ImdbScore;
        public string ImdbVotes;
        public string RottenScore;
        public string MetaScore;
        public string MPAARating;

        public DateTime ReleaseDate;
        public string Released;
        public string Runtime;
        public string Genre;
        public string Director;
        public string Writer;
        public string Actors;
        public string Plot;
        public string Language;
        public string Country;
        public string BoxOffice;
        public string Production;
        public string Awards;
        public string Website;

        public string Type;
        public bool valid;

        Dictionary<string, string> values = new Dictionary<string, string>();

        public string this[string key] { get { if (values.TryGetValue(key, out string val)) return val; return null; } }

        public OMDbInfo(string json)
        {
            JSON = json;
        }

        public static OMDbInfo Parse(string json)
        {
            if (json == null) return null;
            OMDbInfo info = new OMDbInfo(json);
            if (!info.parse())
                return null;

            return info;
        }

        bool parse()
        {
            if (JSON == null) return false;
            if (!JSON.Contains("Response\":\"True\""))
                return true;        // valid is false

            // quick and dirty JSON parser - splitting pairs of values
            valid = true;
            var matches = Regex.Matches(JSON, @"""(\w+)"":""(.+?)(?<!\\)""[,}]");
            foreach (Match m in matches)
                values[m.Groups[1].Value] = HttpUtility.HtmlDecode(m.Groups[2].Value.Replace("N/A", "").Replace("\\\"", "\""));

            var st = Regex.Match(JSON, "\"Rotten Tomatoes\",\"Value\":\"(.+?)\"");
            if (st.Success) RottenScore = values["Rotten Tomatoes"] = st.Groups[1].Value.Replace("N/A", "");

            values.TryGetValue("imdbID", out IMDBid);
            values.TryGetValue("Year", out year);
            values.TryGetValue("Title", out title);

            values.TryGetValue("imdbRating", out ImdbScore);
            values.TryGetValue("imdbVotes", out ImdbVotes);
            values.TryGetValue("Rotten Tomatoes", out RottenScore);
            values.TryGetValue("Metascore", out MetaScore);
            values.TryGetValue("Rated", out MPAARating);

            values.TryGetValue("Released", out Released);
            values.TryGetValue("Runtime", out Runtime);
            values.TryGetValue("Genre", out Genre);
            values.TryGetValue("Director", out Director);
            values.TryGetValue("Writer", out Writer);

            values.TryGetValue("Awards", out Awards);
            values.TryGetValue("Actors", out Actors);
            values.TryGetValue("Plot", out Plot);
            values.TryGetValue("Language", out Language);
            values.TryGetValue("Country", out Country);
            values.TryGetValue("BoxOffice", out BoxOffice);
            values.TryGetValue("Production", out Production);
            values.TryGetValue("Website", out Website);
            values.TryGetValue("Type", out Type);

            // standartize responses
            MPAARating = Regex.Replace(MPAARating, "^(passed)$", "Passed", RegexOptions.IgnoreCase);
            MPAARating = Regex.Replace(MPAARating, "^(approved)$", "Approved", RegexOptions.IgnoreCase);
            MPAARating = Regex.Replace(MPAARating, "^ur$", "NR", RegexOptions.IgnoreCase);
            MPAARating = Regex.Replace(MPAARating, "^(not rated|unrated)$", "NR", RegexOptions.IgnoreCase);

            // fix list separators
            Genre = fixList(Genre);
            Director = fixList(Director);
            Writer = fixList(Writer);
            Actors = fixList(Actors);
            Language = fixList(Language);
            Country = fixList(Country);
            Production = fixList(Production);

            // fix other items
            RottenScore = RottenScore?.Replace("%", "");
            MetaScore = MetaScore?.Replace("%", "");
            Runtime = Runtime?.Replace(" min", ""); // getRuntime(Runtime);

            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var styles = DateTimeStyles.None;
            if (DateTime.TryParse(Released, culture, styles, out DateTime ReleaseDate))
            {
                // avoid jan 01 => JRiver displays only the year
                if (ReleaseDate.Day == 1 && ReleaseDate.Month == 1)
                    ReleaseDate = new DateTime(ReleaseDate.Year, 1, 2);
                Released = ReleaseDate.ToString("yyyy-MM-dd");
            }
            else
                Released = null;

            if (year != null)
            {
                // check if date is just an year
                Match m = Regex.Match(year, @"(\d\d\d\d)");
                if (m.Success)
                {
                    year = m.Groups[1].Value;
                    if (Released == null)
                        Released = $"{year}-01-02";
                }
            }

            return true;
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
            return string.Join("; ", list.Split(';').Select(a=>a.Trim()).Distinct());
        }

        private string getRuntime(string minutes)
        {
            string mins = minutes?.Replace("min", "").Trim();
            if (int.TryParse(mins, out int min))
                return TimeSpan.FromMinutes(min).ToString(@"hh'h 'mm\m");
            return minutes;
        }
    }
}
