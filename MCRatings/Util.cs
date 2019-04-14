using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    public static class Util
    {

        public static long NumberValue(string strvalue)
        {
            string num = Regex.Replace(strvalue, @"[^\d]", "");
            if (long.TryParse(num, out long value))
                return value;
            return 0;
        }

        public static DateTime EpochToDateTime(long epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }

        // JRiver seems to have an off-by-1 bug in the dates it uses in this old Lotus123 format
        // epoch is 30.12.1989 instead of 31.12.1899
        public static int DaysSince1900(DateTime date)
        {
            return (int)(date - new DateTime(1899, 12, 30)).TotalDays;
        }

    }
}
