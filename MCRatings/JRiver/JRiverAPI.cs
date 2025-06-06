﻿using MediaCenter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace ZRatings
{
    // talks to JRiver - opens JRiver as a background app if needed (OLE automation)
    // gets list of playlists, list of fields
    // reads and writes Fields from Files (movies)
    public class JRiverAPI : IDisposable
    {
        static IMJAutomation jr;
        public bool Connected;
        public string Version;
        public string Library;
        public List<JRiverPlaylist> Playlists;
        public Dictionary<string, string> Fields;       // displayName -> FieldName
        public int APIlevel;

        public Exception lastException;
        string lastFile = null;

        public JRiverAPI()
        { }

        ~JRiverAPI()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                Disconnect();
                if (lastFile != null && File.Exists(lastFile))
                    File.Delete(lastFile);
            }
            catch { }
        }

        public bool Connect()
        {
            Logger.Log("Connecting to JRiver");
            Connected = CheckConnection();
            if (Connected) return true;

            try
            {
                // connect to existing instance
                Logger.Log("Connect: getting existing JRiver instance");
                jr = (IMJAutomation)COMMarshal.GetActiveObject("MediaJukebox Application");
                Connected = CheckConnection();
                if (Connected) return true;
                else Logger.Log("Connect to existing instance failed!");
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.Connect() - JRiver probably not running"); }

            try
            {
                Logger.Log("Connect: creating new JRiver instance");
                jr = new MCAutomation();
                Connected = CheckConnection();
                if (!Connected)
                    Logger.Log("Connect via MCAutomation object failed!");
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.Connect() - failed to create new instance"); }
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
                Logger.Log("Checking connection");
                if (jr == null) return false;
                Version = jr.GetVersion().Version;
                APIlevel = jr.IVersion;
                Logger.Log($"JRiver version {Version}, APILevel={APIlevel}");
                string path = null;
                jr.GetLibrary(ref Library, ref path);
                Logger.Log($"JRiver library is '{Library}', path={path}");
                return true;
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.CheckConnection()"); }
            return false;
        }

        public bool createFields(List<string> names, bool editable = true, bool saveTag = true)
        {
            bool ok = true;
            foreach (var name in names)
                ok &= createField(name, name, editable, saveTag, false);
            getFields();
            return ok;
        }

        public bool createField(string name, string display = null, bool editable = true, bool saveTag = true, bool refresh = true)
        {
            if (Fields.ContainsKey(name.ToLower()))
                return true;
            try {
                IMJFieldsAutomation iFields = jr.GetFields();
                IMJFieldAutomation field = iFields.CreateFieldSimple(name, display, editable ? 1 : 0, saveTag ? 1 : 0);
                if (refresh)
                    getFields();
                return Fields.ContainsKey(name.ToLower());
            }
            catch (Exception ex) { Logger.Log(ex, $"JRiverAPI.createField({name})"); }
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
                    if (!string.IsNullOrEmpty(name))
                        Fields[name.ToLower()] = name;
                    if (!string.IsNullOrEmpty(display))
                        Fields[display.ToLower()] = name;
                }
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.getFields()"); }
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
                        iFiles.Filter(Program.settings.FileFilter);
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
            files.Filter(Program.settings.FileFilter);
            int num = files.GetNumberFiles();
            playlist.Filecount = num;
            for (int i = start; i < num; i += step)
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
                    {
                        string value = getFieldValue(movie, f);
                        JRfields[f] = fixList(value, f);
                    }

                string imageH = movie.Get("Image Height", true);
                string imageW = movie.Get("Image Width", true);
                string imageFile = movie.Get("Image File", false);
                bool hasPoster = !string.IsNullOrEmpty(imageFile) && !string.IsNullOrEmpty(imageW);
                // \u00A0 = nbsp; using nbsp for original poster and Spaces for new Poster to track the cell change
                JRfields[AppField.Poster] = !hasPoster ? null : $"{imageW}\u00A0x\u00A0{imageH}";

                JRfields[AppField.File] = movie.Get("Filename", true);
                JRfields[AppField.Imported] = movie.Get("Date Imported", false);         //epoch
                if (DateTime.TryParse(JRfields[AppField.Release], out DateTime date))
                    JRfields[AppField.Release] = date.ToString("yyyy-MM-dd");
                else {
                    if (int.TryParse(JRfields[AppField.Release], out int yyyy))
                        JRfields[AppField.Release] = $"{yyyy}-01-02";
                }
                MovieInfo info = new MovieInfo(movie.GetKey(), JRfields, lists);
                info.currPosterPath = !hasPoster ? null : Path.Combine(Path.GetDirectoryName(JRfields[AppField.File]), imageFile);

                return info;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "JRiverAPI.getMovieInfo()");
                Interlocked.Increment(ref Stats.Session.JRError);
                lastException = ex;
            }
            return null;
        }

        
        private string fixList(string value, AppField field)
        {
            if (!string.IsNullOrEmpty(value) && Constants.listFields.Contains(field))
                value = string.Join("; ", value.Split(';').Select(a => a.Trim()));
            return value;
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
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.getFieldValue()"); }
            return "[JRiver Exception!]";
        }

        public string getFieldValue(MovieInfo movie, string field, bool formatted = true)
        {
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                return file.Get(field, formatted);
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.getFieldValue()"); }
            return "[JRiver Exception!]";
        }

        public string resolveExpression(MovieInfo movie, string expression)
        {
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                return file.GetFilledTemplate(expression);
            }
            catch (Exception ex) { Logger.Log(ex, "JRiverAPI.resolveExpression()"); }
            return "[JRiver Exception!]";
        }

        public string getTemplateFilename(MovieInfo movie)
        {
            string template = Program.settings.VideoTemplateFile;
            if (!File.Exists(template)) throw new Exception("VideoTemplateFile not found, please check settings.xml");

            //string date = movie.DateImported.ToString("yyyyMMdd");
            string name = $"{movie.Title ?? movie.FTitle}.{movie.Year ?? movie.FYear}.{movie.IMDBid}".Trim('.');
            name = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            string sample = Path.Combine(Path.GetTempPath(), $"{name}{Path.GetExtension(template)}");
            return sample;
        }

        public IMJFileAutomation CreateMovie(MovieInfo movie)
        {
            string template = Program.settings.VideoTemplateFile;
            if (!File.Exists(template)) throw new Exception("VideoTemplateFile not found, please check settings.xml");

            string sample = movie[AppField.File] ?? getTemplateFilename(movie);
            if (!File.Exists(sample))
            {
                if (lastFile != null && File.Exists(lastFile))
                    File.Move(lastFile, sample);
                else
                    File.Copy(template, sample, true);
            }
            lastFile = sample;

            try
            {
                IMJFileAutomation file = jr.ImportFile(sample);
                if (file != null)
                {
                    file.Set("Media Sub Type", "Movie");
                    movie.JRKey = file.GetKey();
                    movie.UpdateSnapshot(AppField.File);
                    Interlocked.Increment(ref Stats.Session.JRMovieCreate);
                }
                return file;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "JRiverAPI.CreateMovie()");
                Interlocked.Increment(ref Stats.Session.JRError);
                lastException = ex;
            }
            return null;
        }

        public bool SaveMovie(MovieInfo movie, AppField filter)
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

                if (movie.isDirty)
                    Interlocked.Increment(ref Stats.Session.JRMovieUpdate);

                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                {
                    if (filter != AppField.Movie && f != filter)
                        continue;

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

                        bool saved = (f == AppField.Playlists) ? setPlaylistMembership(file, value)
                            : (f == AppField.Poster) ? SavePoster(file, movie)
                            : setFieldValue(file, jrfield, value);

                        if (saved)
                        {
                            Interlocked.Increment(ref Stats.Session.JRFieldUpdate);
                            movie.UpdateSnapshot(f);
                        }
                        else
                        {
                            Interlocked.Increment(ref Stats.Session.JRError);
                            ok = false;
                        }

                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(ex, "JRiverAPI.SaveMovie()");
                Interlocked.Increment(ref Stats.Session.JRError);
                lastException = ex;
                ok = false;
            }
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
            catch (Exception ex)
            {
                Logger.Log(ex, "JRiverAPI.setPlaylistMembership()");
                ok = false;
            }
            return ok;
        }

        public bool setFieldValue(IMJFileAutomation file, string displayField, string value)
        {
            try
            {
                if (Fields.TryGetValue(displayField.ToLower(), out string jrField))
                {
                    bool ok = file.Set(jrField, value);
                    if (!ok)
                        if (file.Get(jrField, false) == value)
                            ok = true;
                    return ok;
                }
            }
            catch { }
            return false;
        }

        public string GetThumbnail(MovieInfo movie, bool small)
        {
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                if (small) return file.GetImageFile(MJImageFileFlags.IMAGEFILE_THUMBNAIL_MEDIUM);
                return file.GetImageFile(MJImageFileFlags.IMAGEFILE_THUMBNAIL_LARGE);
            }
            catch { }
            return null;
        }

        internal bool DeleteFile(MovieInfo movie)
        {
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                return file.SilentDeleteFile();
            }
            catch { }
            return false;
        }

        internal bool RemovePoster(MovieInfo movie)
        {
            try
            {
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                if (SetPosterPath(file, null))
                {
                    movie.currPosterPath = null;
                    return true;
                }
            }
            catch { }
            return false;
        }

        internal bool SetPosterPath(MovieInfo movie, string path, bool force = false)
        {
            try
            {
                string jrpath = path;
                if (path.StartsWith(Path.GetDirectoryName(movie[AppField.File]), StringComparison.InvariantCultureIgnoreCase))
                    jrpath = Path.GetFileName(path);
                IMJFileAutomation file = jr.GetFileByKey(movie.JRKey);
                if (SetPosterPath(file, jrpath))
                {
                    movie.currPosterPath = Path.Combine(Path.GetDirectoryName(movie[AppField.File]), path);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool SetPosterPath(IMJFileAutomation file, string path)
        {
            try
            {
                // clear current file in case new path is the same, so that JR detects a change
                file.SetImageFile(null, MJImageFileFlags.IMAGEFILE_IN_DATABASE);
                if (file.SetImageFile(path, MJImageFileFlags.IMAGEFILE_IN_DATABASE))
                {
                    Interlocked.Increment(ref Stats.Session.JRPosterUpdate);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool SavePoster(IMJFileAutomation file, MovieInfo movie)
        {
            bool remove = movie.newPoster == null && string.IsNullOrWhiteSpace(movie.newPosterPath);
            if (!remove && movie.newPosterPath == null)
                return false;
            try
            {
                string path = remove ? null : movie.newPosterPath;
                if (path != null && path.StartsWith(Path.GetDirectoryName(movie[AppField.File]), StringComparison.InvariantCultureIgnoreCase))
                    path = Path.GetFileName(path);
                if (SetPosterPath(file, path))
                {
                    // \u00A0 is a non-breaking space. It's used here to force the cell content to be different,
                    // so that it's highlighted even if the actual poster resolution is the same 
                    movie[AppField.Poster] = remove ? null : $"{movie.newPoster?.width}\u00A0x\u00A0{movie.newPoster?.height}";
                    movie.currPosterPath = remove ? null : Path.Combine(Path.GetDirectoryName(movie[AppField.File]), movie.newPosterPath);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }

}
