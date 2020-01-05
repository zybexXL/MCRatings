using MediaCenter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        public Exception lastException;

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

        public IEnumerable<JRiverPlaylist> getPlaylists()
        {
            Playlists = new List<JRiverPlaylist>();
            try
            {
                IMJPlaylistsAutomation iList = jr.GetPlaylists();
                int count = iList.GetNumberPlaylists();
                for (int i = 0; i < count; i++)
                {
                    IMJPlaylistAutomation list = iList.GetPlaylist(i);

                    if (list.Get("type") == "1") continue;      // 0 = playlist, 1 = playlist group, 2 = smartlist
                    string name = list.Name ?? "playlist";

                    if (Regex.IsMatch(name, @"^audio|podcast|\d:\d\d", RegexOptions.IgnoreCase))
                        continue;

                    int fcount = -1;
                    if (!Program.settings.FastStart)
                    {
                        IMJFilesAutomation iFiles = list.GetFiles();
                        iFiles.Filter(Constants.JRFileFilter);
                        fcount = iFiles.GetNumberFiles();
                    }

                    if (Program.settings.FastStart || fcount > 0)
                    {
                        var playlist = new JRiverPlaylist(list.GetID(), name, fcount);
                        Playlists.Add(playlist);
                        yield return playlist;
                    }
                }
            }
            finally
            {
                Playlists = Playlists.OrderBy(p => p.Name.ToLower()).ToList();
            }
        }

        public IEnumerable<MovieInfo> getMovies(JRiverPlaylist playlist, int start = 0, int step = 1)
        {
            IMJPlaylistAutomation pl = jr.GetPlaylistByID(playlist.ID);
            IMJFilesAutomation files = pl.GetFiles();
            files.Filter(Constants.JRFileFilter);
            int num = files.GetNumberFiles();
            playlist.Filecount = num;
            for (int i = start; i < num; i+=step)
                yield return getMovieInfo(files.GetFile(i));
        }

        public MovieInfo getMovieInfo(IMJFileAutomation movie)
        {
            lastException = null;
            try
            {
                // get playlists
                Dictionary<int, string> lists = new Dictionary<int, string>();
                IMJPlaylistsAutomation p = movie.GetPlaylists();
                int count = p.GetNumberPlaylists();
                for (int i = 0; i < count; i++)
                {
                    var pl = p.GetPlaylist(i);
                    if (pl.Get("type") == "0")      // 0 = playlist, 1 = playlist group, 2 = smartlist
                        lists[pl.GetID()] = pl.Name;
                }
                
                // get fields
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
                MovieInfo info = new MovieInfo(movie.GetKey(), JRfields, lists);
                return info;
            }
            catch (Exception ex) { lastException = ex; }
            return null;
        }

        private string getFieldValue(IMJFileAutomation file, AppField field)
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

        public IMJFileAutomation CreateMovie(MovieInfo movie)
        {
            string sample = Program.settings.VideoTemplateFile;
            if (!File.Exists(sample)) throw new Exception("VideoTemplateFile not found, please check settings.xml");

            //string date = movie.DateImported.ToString("yyyyMMdd");
            string tmp = Path.Combine(Path.GetTempPath(), $"{movie.IMDBid}{Path.GetExtension(sample)}");
            File.Copy(sample, tmp, true);
            IMJFileAutomation file = jr.ImportFile(tmp);
            if (file != null)
                movie.JRKey = file.GetKey();

            try { File.Delete(tmp); } catch { }
            return file;
        }

        public bool SaveMovie(MovieInfo movie)
        {
            bool ok = true;
            lastException = null;
            try
            {
                IMJFileAutomation file = null;
                if (movie.JRKey < 0)
                    file = CreateMovie(movie);
                else
                    file = jr.GetFileByKey(movie.JRKey);

                if (file == null) return false;

                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                {
                    JRFieldMap map = null;
                    if (movie.isModified(f) && (f == AppField.Playlists
                        || (Constants.ViewColumnInfo[f].isJRField && Program.settings.FieldMap.TryGetValue(f, out map) && map.enabled)))
                    {
                        string value = movie[f];
                        string jrfield = map?.JRfield;
                        if (f == AppField.Release)
                        {
                            if (DateTime.TryParseExact(movie[f], "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime release))
                                value = Util.DaysSince1900(release).ToString();
                            else
                            {
                                ok = false;
                                continue;
                            }
                        }
                        if (f == AppField.Imported)
                        {
                            if (DateTime.TryParseExact(movie[f], "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime imported))
                                value = Util.DateTimeToEpoch(imported).ToString();
                            else
                            {
                                ok = false;
                                continue;
                            }
                        }
                        if (f == AppField.Year && (jrfield == "Year" || jrfield == "Date") && int.TryParse(movie[f], out int year))
                        {
                            jrfield = "Date";
                            value = Util.DaysSince1900(new DateTime(year, 1, 1)).ToString();
                        }

                        bool saved = (f == AppField.Playlists) ? setPlaylistMembership(file, value) : setFieldValue(file, jrfield, value);
                        if (saved)
                            movie.UpdateSnapshot(f);
                        else
                            ok = false;

                    }
                }
            }
            catch (Exception ex) { lastException = ex; }
            return ok;
        }

        private bool setPlaylistMembership(IMJFileAutomation file, string value)
        {
            bool ok = true;
            try
            {
                var m = Regex.Matches(value ?? "", @"\[ID: (\d+)\]");
                List<int> newLists = m.Cast<Match>().Select(x => int.Parse(x.Groups[1].Value)).ToList();

                string fname = file.Get("Filename", true);
                // remove from old lists
                var currLists = file.GetPlaylists();
                int count = currLists.GetNumberPlaylists();
                for (int i = 0; i < count; i++)
                {
                    var list = currLists.GetPlaylist(i);
                    int id = list.GetID();
                    if (!newLists.Contains(id))
                        ok &= list.RemoveFile(fname);
                    else
                        newLists.Remove(id);
                }

                // add to new lists
                foreach (int i in newLists)
                {
                    var list = jr.GetPlaylistByID(i);
                    ok &= list.AddFile(fname, -1);
                }
            }
            catch { ok = false; }
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
