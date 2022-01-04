using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ZRatings
{
    // Settings are serialized using this class

    [Serializable]
    public class Settings
    {
        const int CURRVERSION = 2;

        public int version;
        public bool valid = false;
        public bool Silent = false;
        public string APIKeys = "";          // omdb
        public string TMDbAPIKeys = "";      // tmdb
        public string FileCleanup = "";
        public List<JRFieldMap> Fields = new List<JRFieldMap>();
        public int CacheDays = Constants.MaxCacheDays;
        public uint[] CellColors;
        public bool FastStart = false;
        public bool Collections = false;
        public bool WebmediaURLs = false;
        public int ListItemsLimit = 5;
        public string Language = "EN";
        public string VideoTemplateFile = "";
        public string appVersion;
        public bool OMDbDisabled = false;
        public bool TMDbDisabled = false;
        public bool StartMaximized = false;
        public bool AddActorRoles = false;
        public bool OMDbBriefPlot = false;

        public string IgnoredArticles = "a;an;the;ein;eine;das;der;die;el;il;la;las;le;les;los;un;une;de l';de la;des;du;l';la;le;les;un;une";
        public bool SortIgnoreArticles = false;
        public bool SortByImportedDate = false;
        public bool ShowSmallThumbnails = true;
        public bool SavePosterCommonFolder = false;
        public bool SavePosterMovieFolder = true;
        public bool PosterFilterLanguage = true;
        public bool PosterSortVotes = false;
        public bool LoadFullSizePoster = true;
        public string PosterFolder;
        public bool RunPosterScript = false;
        public string PosterScript;
        public string FileFilter = "[Media Sub Type]=Movie";

        public bool ActorPlaceholders = true;
        public bool SaveActorThumbnails = false;
        public string ActorFolder;
        public int ActorThumbnailSize = 1;    // medium
        public bool RunThumbnailScript = false;
        public string ThumbnailScript;
        public bool ActorSaveAsPng = true;
        public bool AnalyticsEnabled = true;
        public string AnalyticsID = Guid.NewGuid().ToString();
        public int ScriptCommandTimeout = 30;
        public int ProcessingThreads = 0;

        [XmlIgnore]
        public Dictionary<AppField, JRFieldMap> FieldMap = new Dictionary<AppField, JRFieldMap>();

        [XmlIgnore]
        public List<string> APIkeyList
        {
            get
            {
                return string.IsNullOrEmpty(APIKeys) ? new List<string>() :
                    APIKeys.Trim()
                    .Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
        }

        [XmlIgnore]
        public List<string> TMDBkeyList
        {
            get
            {
                return string.IsNullOrEmpty(TMDbAPIKeys) ? new List<string>() :
                    TMDbAPIKeys.Trim()
                    .Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }
        }

        [XmlIgnore]
        public bool PostersEnabled { get { return FieldMap.TryGetValue(AppField.Poster, out var map) && map.enabled; } }

        [XmlIgnore]
        public string PosterFolderPrefix { get { return Macro.GetBaseFolder(PosterFolder); } }

        [XmlIgnore]
        private string _ignoredArticlesRE;

        [XmlIgnore]
        public string IgnoredArticlesRE { get
            {
                if (!string.IsNullOrEmpty(_ignoredArticlesRE) || string.IsNullOrWhiteSpace(IgnoredArticles))
                    return _ignoredArticlesRE;
                _ignoredArticlesRE = string.Join("|", IgnoredArticles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim()));
                return _ignoredArticlesRE;
            } }

        [XmlIgnore]
        public static bool isMigrated { get; private set; } = false;

        public Settings()
        {
        }

        public static Settings DefaultSettings()
        {
            Settings settings = new Settings();
            settings.BuildFieldMap();
            return settings;
        }

        private void BuildFieldMap()
        {
            if (FieldMap == null) FieldMap = new Dictionary<AppField, JRFieldMap>();
            Dictionary<AppField, JRFieldMap> curr = Fields == null || Fields.Count == 0 ? null : Fields.ToDictionary(f => f.field, f => f);

            foreach (AppField f in Enum.GetValues(typeof(AppField)))
                if (Constants.ViewColumnInfo[f].isJRField)
                {
                    if (curr != null && curr.TryGetValue(f, out var map))
                        FieldMap[f] = map;
                    else
                    {
                        FieldMap[f] = new JRFieldMap(f, Constants.ViewColumnInfo[f].JRField);
                        if (f == AppField.Roles) FieldMap[f].enabled = false;
                        if (f == AppField.ShortPlot) FieldMap[f].enabled = false;
                    }
                }

            Fields = FieldMap.Values.ToList();
        }

        public static Settings Load()
        {
            CheckAvatars();
            Settings settings = null;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (TextReader reader = new StreamReader(Constants.SettingsFile))
                    settings = (Settings)ser.Deserialize(reader);

                settings.BuildFieldMap();

                if (settings.CellColors == null)
                    settings.CellColors = (uint[])Constants.CellColors.Clone();
               
                if (settings.CellColors.Length < Constants.CellColors.Length)
                {
                    uint[] currColors = settings.CellColors;
                    settings.CellColors = (uint[])Constants.CellColors.Clone();
                    for (int i = 0; i < currColors.Length; i++)
                        settings.CellColors[i] = currColors[i];
                }

                bool save = false;

                // change field "Composer" to "Music By" (v3.4.4 bug workaround)
                var jrComposer = settings.Fields.Where(f => f.JRfield?.ToLower() == "composer").SingleOrDefault();
                if (jrComposer != null && settings.appVersion.CompareTo("3.4.5") < 0)
                {
                    jrComposer.JRfield = "Music By";
                    save = true;
                }
                // upgrade settings
                if (settings.valid && settings.version < CURRVERSION)
                {
                    settings.version = CURRVERSION;
                    save = true;
                }

                // generate random clientID
                if (!Guid.TryParse(settings.AnalyticsID, out Guid guid))
                {
                    settings.AnalyticsID = Guid.NewGuid().ToString();
                    save = true;
                }

                if (save)
                    settings.Save();

                return settings;
            }
            catch { }    // errors are handled by the caller when settings is null
            return DefaultSettings();
        }

        public bool Save()
        {
            try
            {
                valid = true;
                Fields = FieldMap.Values.ToList();
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                Directory.CreateDirectory(Path.GetDirectoryName(Constants.SettingsFile));
                using (TextWriter writer = new StreamWriter(Constants.SettingsFile))
                    ser.Serialize(writer, this);
                return true;
            }
            catch { }    // errors are handled by the caller when settings is null
            return false;
        }

        public static void CheckAvatars()
        {
            try
            {
                if (!File.Exists(Constants.AvatarMale))
                    Properties.Resources.male.Save(Constants.AvatarMale, ImageFormat.Png);
                if (!File.Exists(Constants.AvatarFemale))
                    Properties.Resources.female.Save(Constants.AvatarFemale, ImageFormat.Png);
            }
            catch { }
        }
    }
}
