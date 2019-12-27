﻿using System;
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
        public int PreferredSource = 1;     // 1 = TMDB, 2 = OMDB

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
            FieldMap = new Dictionary<AppField, JRFieldMap>();
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
                if (Constants.ViewColumnInfo[f].isJRField)
                    FieldMap.Add(f, new JRFieldMap(f, Constants.ViewColumnInfo[f].JRField));

            Fields = FieldMap.Values.ToList();
        }

        public static Settings Load()
        {
            Settings settings = new Settings();
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (TextReader reader = new StreamReader(Constants.SettingsFile))
                {
                    Settings saved = (Settings)ser.Deserialize(reader);
                    settings.APIKeys = saved.APIKeys;
                    settings.TMDbAPIKeys = saved.TMDbAPIKeys;
                    settings.valid = saved.valid;
                    settings.CacheDays = saved.CacheDays;
                    settings.FastStart = saved.FastStart;
                    settings.Silent = saved.Silent;
                    settings.FileCleanup = saved.FileCleanup?.Replace("\n", "\r\n");
                    settings.version = saved.version;
                    settings.Collections = saved.Collections;
                    settings.WebmediaURLs = saved.WebmediaURLs;
                    settings.ListItemsLimit = saved.ListItemsLimit;
                    settings.Language = saved.Language;
                    settings.ListItemsLimit = saved.ListItemsLimit;

                    if (saved.Fields != null)
                        foreach (var field in saved.Fields)
                            settings.FieldMap[field.field] = field;
                    settings.Fields = settings.FieldMap.Values.ToList();

                    settings.CellColors = saved.CellColors;
                    if (settings.CellColors == null || settings.CellColors.Length != Constants.CellColors.Length)
                        settings.CellColors = (uint[])Constants.CellColors.Clone();
                }
                // upgrade settings
                if (settings.valid && settings.version < CURRVERSION)
                {
                    settings.version = CURRVERSION;
                    settings.Save();
                }
            }
            catch { }    // errors are handled by the caller when settings is null
            return settings;
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
