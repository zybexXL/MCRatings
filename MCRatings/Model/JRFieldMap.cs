using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    // stores info about a JRiver field mapping
    [Serializable]
    public class JRFieldMap
    {
        public AppField field;
        public string JRfield;
        public Sources source;
        public bool enabled;
        public bool overwrite;

        public JRFieldMap() { }   // for serialization

        public JRFieldMap(AppField _field, string _JRField, bool _enabled = true, bool _overwrite = true, Sources src = Sources.None)
        {
            field = _field;
            JRfield = _JRField;
            enabled = _enabled;
            overwrite = _overwrite;
            source = src;
            if (source == Sources.None)
                source = getSources(field).FirstOrDefault();
        }

        public static List<Sources> getSources(AppField field)
        {
            switch (field)
            {
                case AppField.IMDbRating:
                case AppField.IMDbVotes:
                case AppField.RottenTomatoes:
                case AppField.Metascore:
                case AppField.Awards:
                    return new List<Sources>() { Sources.OMDb };

                case AppField.TMDbScore:
                case AppField.OriginalTitle:
                case AppField.Series:
                case AppField.Tagline:
                case AppField.Keywords:
                case AppField.Producer:
                case AppField.Budget:
                case AppField.Trailer:
                case AppField.Poster:
                case AppField.TMDbID:
                case AppField.Roles:
                    return new List<Sources>() { Sources.TMDb };

                case AppField.IMDbID:
                case AppField.Title:
                case AppField.Year:
                case AppField.Release:
                case AppField.MPAARating:
                case AppField.Runtime:
                case AppField.Description:
                case AppField.Genre:
                case AppField.Director:
                case AppField.Writers:
                case AppField.Actors:
                case AppField.Language:
                case AppField.Production:
                case AppField.Country:
                case AppField.Revenue:
                case AppField.Website:
                    return new List<Sources>() { Sources.TMDb, Sources.OMDb };
            }
            return new List<Sources>() { Sources.None }; // no sources
        } 
    }

}
