using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    // holds Movie Info for Datagrid display
    // tracks changes to fields
    // processes info from JRiver, extracts FTitle and FYear from file name/path

    public class MovieInfo
    {
        public int JRKey { get; set; }
        private string Fullpath;
        private string Filename;
        public DateTime DateImported;

        public bool selected { get; set; } = false;
        public string FTitle { get { return this[AppField.FTitle]; } }
        public string FYear { get { return this[AppField.FYear]; } }

        public bool matchTitle = false;
        public bool matchYear = false;
        public bool matchYear1 = false;

        public string Title { get { return this[AppField.Title]; } }
        public string Year { get { return this[AppField.Year]; } }
        public string IMDBid { get { return this[AppField.IMDbID]; } }

        public bool hasRatings
        {
            get
            {
                return !string.IsNullOrEmpty(this[AppField.IMDbRating]) || !string.IsNullOrEmpty(this[AppField.Metascore])
                    || !string.IsNullOrEmpty(this[AppField.RottenTomatoes]) || !string.IsNullOrEmpty(this[AppField.MPAARating]);
            }
        }

        Dictionary<AppField, string> fields = new Dictionary<AppField, string>();       // current values
        Dictionary<AppField, string> snapshot = new Dictionary<AppField, string>();     // copy for change detection (original values)
        Dictionary<AppField, string> updates = new Dictionary<AppField, string>();      // received values
        Dictionary<int, string> playlists = new Dictionary<int, string>();

        public DateTime modified { get; private set; }
        public bool isDirty { get { return dirtyFieldMask > 0; } }
        public ulong dirtyFieldMask = 0;
        private bool trackChanges = false;

        public MovieInfo(int key, Dictionary<AppField, string> JRfields, Dictionary<int, string> Playlists)
        {
            JRKey = key;
            fields = new Dictionary<AppField, string>(JRfields);
            fields.TryGetValue(AppField.File, out Fullpath);
            Filename = Path.GetFileNameWithoutExtension(Fullpath);
            playlists = Playlists;

            string lists = playlists == null ? "" : ($"{playlists.Count}\n\n" + string.Join("\n",playlists.Keys.OrderBy(k=>k).Select(k=>$"{playlists[k]} [ID: {k}]").ToList())).Trim();
            if (lists == "0") lists = "";
            fields[AppField.Playlists] = lists;

            if (fields.ContainsKey(AppField.Imported) && long.TryParse(fields[AppField.Imported], out long seconds))
            {
                DateImported = Util.EpochToDateTime(seconds).ToLocalTime();
                fields[AppField.Imported] = DateImported.ToString("yyyy-MM-dd HH:mm:ss");
            }

            TakeSnapshot();
        }

        public string this[AppField field]
        {
            get { if (fields.TryGetValue(field, out string val)) return val; else return null; }
            set { setField(field, value); }
        }

        public string originalValue(AppField field)
        {
            if (snapshot.TryGetValue(field, out string val)) return val;
                return null;
        }

        public void setUpdate(AppField field, string value)
        {
            updates[field] = value;
        }

        public void clearUpdates()
        {
            updates.Clear();
        }

        public string updatedValue(AppField field)
        {
            if (updates.TryGetValue(field, out string val)) return val;
                return null;
        }

        public bool isUpdated(AppField field)
        {
            return updates.ContainsKey(field);
        }

        private void setField(AppField field, string value)
        {
            fields[field] = value;
            modified = DateTime.Now;
            if (trackChanges)
            {
                if (field >= AppField.Title)
                {
                    bool changed = false;
                    if (snapshot.TryGetValue(field, out string original))
                    {
                        if (original != value) changed = true;
                    }
                    else if (!string.IsNullOrEmpty(value))
                        changed = true;

                    if (changed) dirtyFieldMask |= ((ulong)1 << (byte)field);
                    else dirtyFieldMask &= ~((ulong)1 << (byte)field);
                }

                if (field == AppField.FTitle || field == AppField.FYear || field == AppField.Title || field == AppField.Year
                    || field == AppField.IMDbID || field == AppField.Actors || field == AppField.Director
                    || field == AppField.Keywords || field == AppField.OriginalTitle)
                {
                    SetFilterString();
                    CheckMatch();
                }
            }
        }

        public bool isModified(AppField field)
        {
            return isModified(field, this[field]) != 0;
        }

        // return 0 if unmodified, 1 if value changed, 2 if now has value and it was blank before
        public int isModified(AppField field, string curr)
        {
            if (snapshot.TryGetValue(field, out string original))
                return curr == original ? 0 : string.IsNullOrEmpty(original) && !string.IsNullOrEmpty(curr) ? 2 : 1;
            return string.IsNullOrEmpty(curr) ? 0 : 2;
        }

        public void UpdateSnapshot(AppField field)
        {
            snapshot[field] = this[field];
            this[field] = snapshot[field];  // reset dirty flag for the field
        }

        public void TakeSnapshot()
        {
            ExtractTitle();
            SetFilterString();
            CheckMatch();

            dirtyFieldMask = 0;
            modified = DateTime.MinValue;
            snapshot = new Dictionary<AppField, string>(fields);
            trackChanges = true;
        }

        public void restoreSnapshot()
        {
            if (!trackChanges) return;
            fields = new Dictionary<AppField, string>(snapshot);
            TakeSnapshot();
        }

        public void ExtractTitle()
        {
            if (string.IsNullOrEmpty(Fullpath))
                return;

            string path = Fullpath;
            while (!string.IsNullOrEmpty(path))
            {
                int len = path.Length;
                string name = Path.GetFileName(path);
                if (Regex.IsMatch(name, @"\.dvd;1$|\.bluray;1$|\.bluray3d;1$|^ts_video|^video_ts|bdmv$", RegexOptions.IgnoreCase))
                    path = Path.GetDirectoryName(path);
                else
                    break;
                if (path.Length == len)
                    break;
            }

            Match y = Regex.Match(path, @"(\D|^)(19\d\d|20\d\d)(\D|$)");
            if (y.Success)
                fields[AppField.FYear] = y.Groups[2].Value;

            string title = Path.GetFileNameWithoutExtension(path);
            if (!ExtractTitleYear(title))
                if (!ExtractTitleYear(Path.GetFileName(Path.GetDirectoryName(path))))
                    fields[AppField.FTitle] = CleanTitle(title, Program.settings.FileCleanup);
        }

        public void SetFilterString()
        {
            string filter = $"{this[AppField.FTitle]}|{this[AppField.FYear]}|{this[AppField.Title]}|{this[AppField.Year]}|{this[AppField.IMDbID]}|{this[AppField.Actors]}|{this[AppField.Director]}|{this[AppField.Keywords]}|{this[AppField.OriginalTitle]}";
            fields[AppField.Filter] = filter.ToLower().Trim('|');
        }

        public bool CheckMatch()
        {
            string fyear = this[AppField.FYear];
            matchTitle = FuzzyEquals(FTitle, Title);
            int.TryParse(Year, out int ynum);
            matchYear = (fyear ?? "") == (Year ?? "");
            matchYear1 = matchYear || fyear == (ynum + 1).ToString() || fyear == (ynum - 1).ToString();
            return matchTitle && matchYear;
        }

        private bool ExtractTitleYear(string name)
        {
            name = Regex.Replace(name, @"[\._\[\(\{\}\)\]]", " ").Trim();
            var m = Regex.Match(name, @"(.*(?:\D|^))(19\d\d|20\d\d)((?:\D|$).*)");
            if (m.Success)
            {
                string title = CleanTitle(m.Groups[1].Value?.Trim(), Program.settings.FileCleanup);
                if (string.IsNullOrWhiteSpace(title) || title.Length < 1)
                    title = CleanTitle(m.Groups[3].Value?.Trim(), Program.settings.FileCleanup);

                this[AppField.FTitle] = title;
                this[AppField.FYear] = m.Groups[2].Value;
                return true;
            }
            return false;
        }

        private string CleanTitle(string title, string customtags = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            // handle "<original_title>.AKA.<english_title>" common format - keep english title
            Match aka = Regex.Match(title, @".*\WAKA\W(.*)", RegexOptions.IgnoreCase);
            if (aka.Success)
                title = aka.Groups[1].Value;

            // merge embedded and user provided tags, generate regex
            customtags = customtags ?? "";
            string tags = $"{Constants.FileCleanup} {customtags}".Trim();
            tags = Regex.Replace(tags.Trim(), @"\s+", "|").Replace("||", "|").Trim('|');
            tags = $@"((?:\W|^)({tags})(?:\W|$))";

            // remove dots and underscores, trim trailing/starting parenthesis
            title = Regex.Replace(title, @"[\._]", " ").Trim();
            title = Regex.Replace(title, @"[\s\[\]\(\)\{\}\-]+$", "");
            title = Regex.Replace(title, @"^[\s\[\]\(\)\{\}\-]+", "");

            // recursively remove preceding tags, keep title before trailing tags
            var t = Regex.Match(title, tags, RegexOptions.IgnoreCase);
            if (t.Success)
            {
                int split = t.Groups[1].Index;
                if (split == 0)
                    return CleanTitle(title.Substring(t.Groups[1].Length));
                title = title.Substring(0, split);
            }

            // trim trailing/starting parenthesis after cleanup
            title = Regex.Replace(title, @"[\s\[\]\(\)\{\}\-]+$", "");
            title = Regex.Replace(title, @"^[\s\[\]\(\)\{\}\-]+", "");
            return title;
        }

        private bool FuzzyEquals(string a, string b)
        {
            string t1 = Regex.Replace(a ?? "", "[\x20-\x2F\x3A-\x40\x5B-\x60\x7B-\x7E]", "");
            string t2 = Regex.Replace(b ?? "", "[\x20-\x2F\x3A-\x40\x5B-\x60\x7B-\x7E]", "");
            return t1.ToLower() == t2.ToLower();
        }
    }
}
