using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ZRatings
{
    // PTP Collection movie entry
    public class PtpCollectionMovie
    {
        public string Title;
        public string Year;
        public string ImdbId;
        public string YoutubeId;
        public string Synopsis;
        public string Cover;

        public bool tag = false;
    }
}
