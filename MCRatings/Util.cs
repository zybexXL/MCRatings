using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
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

        public static long DateTimeToEpoch(DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        // JRiver seems to have an off-by-1 bug in the dates it uses in this old Lotus123 format
        // epoch is 30.12.1989 instead of 31.12.1899
        public static int DaysSince1900(DateTime date)
        {
            return (int)(date - new DateTime(1899, 12, 30)).TotalDays;
        }

        public static T JsonDeserialize<T>(string json, string dateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'")
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var settings = new DataContractJsonSerializerSettings()
                    {
                        DateTimeFormat = new DateTimeFormat(dateFormat),
                        UseSimpleDictionaryFormat = true,
                    };
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
                    return (T)serializer.ReadObject(ms);
                }
            }
            catch { }
            return default(T);
        }

        public static string JsonSerialize<T>(T obj, string dateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'", bool indent = false)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, true, true, "  "))
                    {
                        var settings = new DataContractJsonSerializerSettings()
                        {
                            DateTimeFormat = new DateTimeFormat(dateFormat),
                            UseSimpleDictionaryFormat = true
                        };
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
                        serializer.WriteObject(writer, obj);
                        writer.Flush();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch { }
            return null;
        }
    }
}
