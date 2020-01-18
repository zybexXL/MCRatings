using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MCRatings
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
        public bool SortByImportedDate = false;

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
                        FieldMap[f] = new JRFieldMap(f, Constants.ViewColumnInfo[f].JRField);
                }

            Fields = FieldMap.Values.ToList();
        }

        public static Settings Load()
        {
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

                // upgrade settings
                if (settings.valid && settings.version < CURRVERSION)
                {
                    settings.version = CURRVERSION;
                    settings.Save();
                }
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
    }

}
