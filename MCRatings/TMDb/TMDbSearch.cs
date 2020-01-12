using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MCRatings
{
    // find movie by ImdbID
    // https://api.themoviedb.org/3/find/tt4633694?api_key={key}&language=en&external_source=imdb_id
    public class TMDbFind
    {
        public TMDbSearchResult[] movie_results;
        //"person_results": [],
        //"tv_results": [],
        //"tv_episode_results": [],
        //"tv_season_results": []
    }

    // find movie by Title + Year
    // https://api.themoviedb.org/3/search/movie?api_key={key}&language=en-US&query=The%20Matrix&page=1&include_adult=false&year=1999
    public class TMDbSearch
    {
        public int page;
        public int total_pages;
        public int total_results;
        public TMDbSearchResult[] results;
    }

    public class TMDbSearchResult
    {
        public int id;
        public double popularity;
        public string title;
        public string original_title;
        public string release_date;
        public string original_language;
        public string overview;

        public int vote_count;
        public double vote_average;

        public string backdrop_path;
        public string poster_path;
        public bool video;
        public bool adult;
        public int[] genre_ids;
    }
    
    
}
