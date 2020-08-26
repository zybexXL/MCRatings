using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace MCRatings
{
    // PTP Movie Collection
    public class PtpCollection
    {
        public List<PtpCollectionMovie> Movies { get; set; }

        public int Count => isValid ? Movies.Count : 0;
        public int TotalCount;
        public string Title;
        public int Error = 0;
        public int Pages = 1;

        public bool isMultipage => Pages > 1;

        public bool isValid { get { return Movies != null && Movies.Count > 0 && !string.IsNullOrEmpty(Movies[0].Title); } }

        public static PtpCollection Parse(string html)
        {
            PtpCollection col = new PtpCollection();
            var m = Regex.Match(html, @"<title>(.+?)</title>");
            if (m.Success)
            {
                col.Title = HttpUtility.HtmlDecode(m.Groups[1].Value.Trim());
                if (col.Title != null && col.Title.Contains("::"))
                    col.Title = col.Title.Substring(0, col.Title.IndexOf("::")).Trim();
            }

            m = Regex.Match(html, @"coverViewJsonData\[ 0 \] = ({.+});\n");
            if (m.Success)
            {
                string json = m.Groups[1].Value;
                PtpCollection movies = Util.JsonDeserialize<PtpCollection>(json);
                if (movies != null && movies.isValid)
                {
                    col.Movies = movies.Movies;
                    col.TotalCount = movies.Count;

                    // total collection length
                    m = Regex.Match(html, @"<li>Torrents: (\d+)</li>", RegexOptions.IgnoreCase);
                    if (m.Success) col.TotalCount = int.Parse(m.Groups[1].Value);

                    // collection page count
                    MatchCollection m2 = Regex.Matches(html, @"\?page=(\d+)");
                    if (m2.Count > 0)
                    {
                        col.Pages = m2.Cast<Match>().Select(p => int.Parse(p.Groups[1].Value)).Max();
                        if (Regex.IsMatch(html, @"class=""pagination__current-page"">\d+-\d+</span></div>"))
                            col.Pages++;
                    }
                }
                else col.Error = 1;   // parse failed
            }

            return col;
        }
    }
}
