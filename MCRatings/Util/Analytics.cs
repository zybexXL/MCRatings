using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ZRatings
{
    // Google Analytics tracking
    // ZRatings tracks the following info anonymously:
    // - Number of calls to OMDB/TMDB APIs
    // - Number of movies updated with GET Info, and saved back to JRiver
    // - Number of posters and thumbnails downloaded
    // - Events: start/close application, opening settings/help/stats/posterBrowser

    public class Analytics
    {

#if !ANALYTICS
        public static void Init() {}
        public static void AppStart(string app, string version, bool startSession = true) {}
        public static void AppClose() {}
        public static void ScreenView(string screen, string app = null, string version = null) {}
        public static void Event(string category, string evnt, string label = null, int value = -1) {}
        public static void Timing(string category, string variable, string label, int durationMs) {}
        public static void Exception(Exception ex, bool fatal = false) {}

#else

        // Google Analytics property code - tied to ZRatings developer account
        internal static string property = "VUEtMTM1MzU2MDM4LTE=";
        private static HttpClient client;

        public static void Init()
        {
            property = Encoding.ASCII.GetString(Convert.FromBase64String(property));
            client = new HttpClient();
            client.BaseAddress = new Uri($"{Constants.https}www.google-analytics.com");

            string x64 = Environment.Is64BitOperatingSystem ? "; x64; AMD64" : "";
            client.DefaultRequestHeaders.UserAgent.TryParseAdd($"ZRatings/{Program.version.ToString()} (compatible; Windows; {Util.OSName()}{x64})");
        }

        public static void AppStart(string app, string version, bool startSession = true)
        {
            AnalyticsArgs args = new AnalyticsArgs("Application", "Started");
            if (startSession)
            {
                args.Add("sc", "start");                            // session start
                args.Add("ul", CultureInfo.CurrentCulture.Name);    // user language
            }
            args.Add("an", app);             // app name
            args.Add("av", version);         // app version
            sendAsync(args);

            ScreenView("MainUI", app, version);
        }

        public static void AppClose()
        {
            AnalyticsArgs args = new AnalyticsArgs("Application", "Closed");
            args.Add("sc", "end");                  // session end
            sendAsync(args);
        }

        public static void ScreenView(string screen, string app = null, string version = null)
        {
            AnalyticsArgs args = new AnalyticsArgs("screenview");
            args.Add("cd", screen);                                          // screen name
            args.Add("an", app ?? "ZRatings");                               // app name
            args.Add("av", version ?? Program.version.ToString());           // app version
            sendAsync(args);
        }

        public static void Event(string category, string evnt, string label = null, int value = -1)
        {
            AnalyticsArgs args = new AnalyticsArgs(category, evnt, label, value);
            sendAsync(args);
        }

        public static void Timing(string category, string variable, string label, int durationMs)
        {
            AnalyticsArgs args = new AnalyticsArgs("timing");
            args.Add("utc", category);                  // timing category
            args.Add("utv", variable);                  // timing variable
            args.Add("utt", durationMs.ToString());     // timing time
            args.Add("utl", label);                     // timing label
            sendAsync(args);
        }

        public static void Exception(Exception ex, bool fatal = false)
        {
            AnalyticsArgs args = new AnalyticsArgs("exception");
            args.Add("exd", $"{ex.GetType().ToString()}: {ex.Message}", 150);
            args.Add("exf", fatal ? "1" : "0");
            sendAsync(args);
        }

        private static void sendAsync(AnalyticsArgs args, bool wait=false)
        {
            if (!Program.settings.AnalyticsEnabled)
                return;
            try
            {
                var task = client.PostAsync("/collect", new StringContent(args.EncodedArgs));
                if (wait)
                    task.Wait(2000);
            }
            catch { }
        }
    }

    public class AnalyticsArgs
    {
        List<Tuple<string, string>> parameters = new List<Tuple<string, string>>();

        internal string EncodedArgs { get { return string.Join("&", parameters.Select(a => $"{a.Item1}={HttpUtility.UrlEncode(a.Item2, Encoding.UTF8)}")); } }

        internal AnalyticsArgs(string type = "event")
        {
            Add("v", "1");
            Add("t", type);
            Add("tid", Analytics.property);
            Add("cid", Program.settings.AnalyticsID);
        }
        internal AnalyticsArgs(string category, string action, string label = null, int value = -1) : this()
        {
            Add("ec", category);                // event category
            Add("ea", action);                  // event action
            if (!string.IsNullOrWhiteSpace(label)) Add("el", label);    // event label
            if (value >= 0) Add("ev", value.ToString());                // event value
        }

        internal void Add(string name, string value, int maxLen = 0)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Trim();
                string val = (maxLen == 0 || value.Length <= maxLen) ? value : value.Substring(0, maxLen);
                parameters.Add(new Tuple<string, string>(name, val));
            }
        }
#endif
    }
}
