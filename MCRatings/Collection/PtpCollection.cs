using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MCRatings
{
    // PTP Movie Collection
    public class PtpCollection
    {
        public string Title;
        public List<PtpCollectionMovie> Movies;
        
        public bool isValid { get { return Movies != null && Movies.Count > 0 && !string.IsNullOrEmpty(Movies[0].Title); } }

        public static PtpCollection Parse(string json, string title)
        {
            if (json == null) return null;
            try
            {
                PtpCollection movies = Util.JsonDeserialize<PtpCollection>(json);
                if (!movies.isValid) return null;

                if (title != null && title.Contains("::"))
                    title = title.Substring(0, title.IndexOf("::"));

                movies.Title = title?.Trim();
                return movies;
            }
            catch { }
            return null;
        }
    }
}
