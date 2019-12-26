using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    public class MovieCollection
    {
        public string Title;
        public List<CollectionMovie> Movies;

        public bool isValid { get { return Movies != null && Movies.Count > 0 && !string.IsNullOrEmpty(Movies[0].Title); } }

        public static MovieCollection Parse(string json, string title)
        {
            if (json == null) return null;
            try
            {
                MovieCollection movies = Util.JsonDeserialize<MovieCollection>(json);
                if (!movies.isValid) return null;

                if (title != null && title.Contains("::"))
                    title = title.Substring(0, title.IndexOf("::"));

                movies.Title = title.Trim();
                return movies;
            }
            catch { }
            return null;
        }
    }

    public class CollectionMovie
    {
        public string Title;
        public string Year;
        public string ImdbId;
        public string YoutubeId;
        public string Synopsis;
        public string Cover;
    }
}
