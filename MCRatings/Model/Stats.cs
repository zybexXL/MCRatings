using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{
    [DataContract]
    public class Stats
    {
        private static Stats _sessionStats;
        private static Stats _savedStats = null;

        // current execution stats
        public static Stats Session {
            get {
                if (_sessionStats == null) _sessionStats = new Stats(true);
                return _sessionStats;
            } }

        // cumulative application stats (persisted)
        public static Stats Saved {
            get {
                if (_savedStats == null) _savedStats = load() ?? new Stats();
                return _savedStats;
            } }

        // current sum of AppStats + SessionStats
        public static Stats Total {
            get {
                Session.AppRuntime = (int)(DateTime.Now - Session.StartDate).TotalSeconds;
                Stats total = new Stats().Add(Session).Add(Saved);
                total.StartDate = Saved.StartDate;
                return total;
            } }

        [DataMember] public DateTime StartDate { get; private set; } = DateTime.Now;

        [DataMember] public int CacheAdd;
        [DataMember] public int CacheHit;
        [DataMember] public int CacheMiss;
        [DataMember] public int CacheExpired;

        [DataMember] public int OMDbGet;            // get by IMDB id
        [DataMember] public int OMDbSearch;         // get by title+year
        [DataMember] public int OMDbAPICall;
        [DataMember] public int OMDbAPINotFound;
        [DataMember] public int OMDbAPIError;

        [DataMember] public int TMDbGet;            // get by IMDB id
        [DataMember] public int TMDbSearch;         // get by title+year
        [DataMember] public int TMDbAPICall;
        [DataMember] public int TMDbAPINotFound;
        [DataMember] public int TMDbAPIError;

        [DataMember] public int ImageDownload;
        [DataMember] public int ImageDownloadError;
        
        [DataMember] public int JRFieldUpdate;
        [DataMember] public int JRMovieUpdate;
        [DataMember] public int JRPosterUpdate;
        [DataMember] public int JRMovieCreate;
        [DataMember] public int JRError;
        
        [DataMember] public int AppException;
        [DataMember] public int AppRuns { get; private set; }
        [DataMember] public int AppRuntime { get; private set; }  // seconds


        public static bool Init()
        {
            return Total != null;   // forces initialization and loading of Session/Saved stats
        }

        private Stats(bool isSession = false)
        {
            if (isSession) AppRuns = 1;
        }

        public static void Reset(bool resetSaved = false)
        {
            if (!resetSaved)
            {
                Save();                     // update totals before reset
                _savedStats = load();
                _savedStats.AppRuns--;      // don't count another run
            }
            _sessionStats = new Stats(true);
            if (resetSaved)
            {
                _savedStats = new Stats();
                Save();
            }
        }

        public static bool Save(string path = null)
        {
            return Total.save();
        }

        private Stats Add(Stats stats)
        {
            CacheAdd += stats.CacheAdd;
            CacheHit += stats.CacheHit;
            CacheMiss += stats.CacheMiss;
            CacheExpired += stats.CacheExpired;

            OMDbGet += stats.OMDbGet;
            OMDbSearch += stats.OMDbSearch;
            OMDbAPICall += stats.OMDbAPICall;
            OMDbAPINotFound += stats.OMDbAPINotFound;
            OMDbAPIError += stats.OMDbAPIError;

            TMDbGet += stats.TMDbGet;
            TMDbSearch += stats.TMDbSearch;
            TMDbAPICall += stats.TMDbAPICall;
            TMDbAPINotFound += stats.TMDbAPINotFound;
            TMDbAPIError += stats.TMDbAPIError;

            ImageDownload += stats.ImageDownload;
            ImageDownloadError += stats.ImageDownloadError;

            JRFieldUpdate += stats.JRFieldUpdate;
            JRMovieUpdate += stats.JRMovieUpdate;
            JRPosterUpdate += stats.JRPosterUpdate;
            JRMovieCreate += stats.JRMovieCreate;
            JRError += stats.JRError;

            AppException += stats.AppException;
            AppRuns += stats.AppRuns;
            AppRuntime += stats.AppRuntime;

            return this;
        }

        private bool save(string path = null)
        {
            if (path == null)
                path = Path.Combine(Constants.DataFolder, Constants.StatsFile);
            try
            {
                string json = Util.JsonSerialize(this);
                File.WriteAllText(path, json);
                return true;
            }
            catch { }
            return false;
        }

        private static Stats load(string path = null)
        {
            if (path == null)
                path = Path.Combine(Constants.DataFolder, Constants.StatsFile);
            try
            {
                string json = File.ReadAllText(path);
                Stats stats = Util.JsonDeserialize<Stats>(json);
                return stats;
            }
            catch { }
            return null;
        }
    }
}
