using MediaCenter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    // talks to JRiver - opens JRiver as a background app if needed (OLE automation)
    // gets list of playlists, list of fields
    // reads and writes Fields from Files (movies)
    public class JRiverAPI
    {
        static IMJAutomation jr;
        public bool Connected;
        public string Version;
        public string Library;
        public List<JRiverPlaylist> Playlists;
        public Dictionary<string, string> Fields;       // displayName -> FieldName
        public int APIlevel;

        public JRiverAPI()
        { }

        ~JRiverAPI()
        { Disconnect(); }

        public bool Connect()
        {
            Connected = CheckConnection();
            if (Connected) return true;

            try
            {
                jr = (IMJAutomation)Marshal.GetActiveObject("MediaJukebox Application");
                Connected = CheckConnection();
                if (Connected) return true;
            }
            catch { }

            try
            {
                jr = new MCAutomation();
                Connected = CheckConnection();
            }
            catch { }
            return Connected;
        }

        public void Disconnect()
        {
            if (jr != null)
                Marshal.FinalReleaseComObject(jr);
            jr = null;
        }

        public bool CheckConnection()
        {
            try
            {
                if (jr == null) return false;
                Version = jr.GetVersion().Version;
                APIlevel = jr.IVersion;
                string path=null;
                jr.GetLibrary(ref Library, ref path);
                return true;
            }
            catch { }
            return false;
        }

        public Dictionary<string, string> getFields()
        {
            Fields = new Dictionary<string, string>();
            try
            {
                IMJFieldsAutomation iFields = jr.GetFields();
                int count = iFields.GetNumberFields();
                for (int i = 0; i < count; i++)
                {
                    IMJFieldAutomation field = iFields.GetField(i);
                    string display = field.GetName(true);
                    string name = field.GetName(false);
                    Fields.Add(display.ToLower(), name);
                }
            }
            catch { }
            return Fields;
        }

        public List<JRiverPlaylist> getPlaylists()
        {
            Playlists = new List<JRiverPlaylist>();
            try
            {
                var iList = jr.GetPlaylists();
                int count = iList.GetNumberPlaylists();
                for (int i = 0; i < count; i++)
                {
                    var list = iList.GetPlaylist(i);
                    var iFiles = list.GetFiles();
                    iFiles.Filter(Constants.JRFileFilter);
                    int fcount = iFiles.GetNumberFiles();
                    if (fcount > 0 && !Regex.IsMatch(list.Name, @"\d:\d\d"))
                        Playlists.Add(new JRiverPlaylist(list.GetID(), list.Name, fcount, list.Path));
                }
            }
            catch { }
            Playlists = Playlists.OrderBy(p => p.Name.ToLower()).ToList();
            return Playlists;
        }

        public IEnumerable<MovieInfo> getMovies(JRiverPlaylist playlist)
        {
            var pl = jr.GetPlaylistByID(playlist.ID);
            var files = pl.GetFiles();
            files.Filter(Constants.JRFileFilter);
            int num = files.GetNumberFiles();
            for (int i = 0; i < num; i++)
                yield return getMovieInfo(files.GetFile(i));
        }

        public MovieInfo getMovieInfo(IMJFileAutomation movie)
        { 
            try
            {
                Dictionary<AppField, string> JRfields = new Dictionary<AppField, string>();
                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                    if (Constants.ViewColumnInfo[f].isJRField)
                        JRfields[f] = getFieldValue(movie, f);

                JRfields[AppField.File] = movie.Get("Filename", true);
                JRfields[AppField.Imported] = movie.Get("Date Imported", false);         //epoch
                if (DateTime.TryParse(JRfields[AppField.Release], out DateTime date))
                    JRfields[AppField.Release] = date.ToString("yyyy-MM-dd");
                else {
                    if (int.TryParse(JRfields[AppField.Release], out int yyyy))
                        JRfields[AppField.Release] = $"{yyyy}-01-02";
                }
                MovieInfo info = new MovieInfo(movie.GetKey(), JRfields);
                return info;
            }
            catch { }
            return null;
        }

        public string getFieldValue(IMJFileAutomation file, AppField field)
        {
            try
            {
                if (Program.settings.FieldMap.TryGetValue(field, out JRFieldMap map))
                {
                    if (!map.enabled) return null;
                    if (Fields.TryGetValue(map.JRfield.ToLower(), out string jrField))
                        return file.Get(jrField, true);
                }
                return "[invalid field name]";
            }
            catch { }
            return "[JRiver Exception!]";
        }

        public bool SaveMovie(MovieInfo movie)
        {
            bool ok = true;
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                if (file == null) return false;

                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                {
                    if (Constants.ViewColumnInfo[f].isJRField
                       && Program.settings.FieldMap.TryGetValue(f, out JRFieldMap map)
                       && map.enabled
                       && movie.isModified(f))
                    {
                        string value = movie[f];
                        string jrfield = map.JRfield;
                        if (f == AppField.Release)
                        {
                            if (DateTime.TryParseExact(movie[f], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime release))
                                value = ExtensionMethods.DaysSince1900(release).ToString();
                        }
                        if (f == AppField.Year && (jrfield == "Year" || jrfield == "Date") && int.TryParse(movie[f], out int year))
                        {
                            jrfield = "Date";
                            value = ExtensionMethods.DaysSince1900(new DateTime(year, 1,1)).ToString();
                        }
                        if (setFieldValue(file, jrfield, value))
                            movie.UpdateSnapshot(f);
                        else
                            ok = false;
                    }
                }
            }
            catch { }
            return ok;
        }

        public bool setFieldValue(IMJFileAutomation file, string displayField, string value)
        {
            try
            {
                if (Fields.TryGetValue(displayField.ToLower(), out string jrField))
                    return file.Set(jrField, value);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString()); }
            return false;
        }

    }

}
