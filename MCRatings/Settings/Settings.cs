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
        public bool valid = false;
        public bool Silent = false;
        public string APIKeys;
        public string FileCleanup;
        public List<JRFieldMap> Fields = new List<JRFieldMap>();
        public int CacheDays;
        public uint[] CellColors;
        public bool FastStart = false;

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

        public Settings()
        {
        }

        public static Settings Load()
        {
            Settings settings = new Settings();
            settings.Reset();
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                using (TextReader reader = new StreamReader(Constants.SettingsFile))
                {
                    Settings saved = (Settings)ser.Deserialize(reader);
                    settings.APIKeys = saved.APIKeys;
                    settings.valid = saved.valid;
                    settings.CacheDays = saved.CacheDays;
                    settings.FastStart = saved.FastStart;
                    settings.Silent = saved.Silent;
                    settings.FileCleanup = saved.FileCleanup?.Replace("\n","\r\n");

                    if (saved.Fields != null)
                        foreach (var field in saved.Fields)
                            settings.FieldMap[field.field] = field;
                    settings.Fields = settings.FieldMap.Values.ToList();

                    settings.CellColors = saved.CellColors;
                    if (settings.CellColors == null || settings.CellColors.Length != Constants.CellColors.Length)
                        settings.CellColors = (uint[])Constants.CellColors.Clone();
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

        public void Reset()
        {
            FieldMap = new Dictionary<AppField, JRFieldMap>();
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
                if (Constants.ViewColumnInfo[f].isJRField)
                    FieldMap.Add(f, new JRFieldMap(f, Constants.ViewColumnInfo[f].JRField));

            Fields = FieldMap.Values.ToList();
            APIKeys = "";
            FileCleanup = "";
            Silent = false;
            CacheDays = Constants.MaxCacheDays;
            FastStart = false;
            // colors are NOT reset here
        }
    }

}
