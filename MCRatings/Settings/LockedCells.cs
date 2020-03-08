using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    // stores persistent list of locked cells for current library
    public class LockedCellList
    {
        public Dictionary<int, string> lockedFields;
    }

    public static class LockedCells
    {
        static Dictionary<int, List<AppField>> locked;
        static bool isDirty;
        static string library;

        public static bool isLocked(int id, AppField field)
        {
            return locked.TryGetValue(id, out var list) && list.Contains(field);
        }

        public static void Lock(int id, AppField field)
        {
            if (!locked.TryGetValue(id, out var list))
            {
                locked[id] = new List<AppField>() { field };
                isDirty = true;
            }
            else if (!list.Contains(field))
            { 
                list.Add(field);
                isDirty = true;
            }
        }

        public static void Unlock(int id, AppField field)
        {
            if (locked.TryGetValue(id, out var list))
            {
                list.Remove(field);
                if (list.Count == 0)
                    locked.Remove(id);
                isDirty = true;
            }
        }

        public static bool Toggle(int id, AppField field)
        {
            bool state = isLocked(id, field);
            if (state) Unlock(id, field);
            else Lock(id, field);

            return !state;
        }

        public static List<AppField> GetLockedFields(int id)
        {
            if (locked.TryGetValue(id, out var list)) return list;
            return null;
        }

        public static bool HasLockedFields(int id)
        {
            return (locked.TryGetValue(id, out var list) && list.Count > 0);
        }

        public static void Load(string libName = null)
        {
            if (isDirty) Save();
            string lib = libName == null ? null : string.Join("_", libName.Split(Path.GetInvalidFileNameChars()));
            if (library != null && lib == library) return; // already loaded

            library = lib;
            string path = Constants.LockedCellsFile;
            if (library != null)
                path = Path.Combine(Constants.DataFolder, Path.GetFileNameWithoutExtension(path) + $".{library}.json");

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var cellList = Util.JsonDeserialize<LockedCellList>(json);

                    locked = new Dictionary<int, List<AppField>>();
                    foreach (int key in cellList.lockedFields.Keys)
                    {
                        List<AppField> values = new List<AppField>();
                        string[] cols = cellList.lockedFields[key].Split(',');
                        foreach (string col in cols)
                            if (!string.IsNullOrEmpty(col.Trim()))
                                if (Enum.TryParse<AppField>(col.Trim(), out AppField value))
                                    values.Add(value);
                        locked[key] = values;
                    }
                }
                catch (Exception ex) {
                    Logger.Log(ex, $"Failed to load LockedCells from {path}");
                    locked = new Dictionary<int, List<AppField>>();
                }
            }
            else
                locked = new Dictionary<int, List<AppField>>();

            isDirty = false;
        }

        public static void Save()
        {
            if (!isDirty) return;
            string path = Constants.LockedCellsFile;
            if (library != null)
                path = Path.Combine(Constants.DataFolder, Path.GetFileNameWithoutExtension(path) + $".{library}.json");

            try
            {
                Dictionary<int, string> cellList = new Dictionary<int, string>();
                foreach (var key in locked.Keys)
                    cellList[key] = string.Join(",", locked[key].Select(t => t.ToString()));
                    
                var list = new LockedCellList() { lockedFields = cellList };
                string json = Util.JsonSerialize(list);
                File.WriteAllText(path, json);
                isDirty = false;
            }
            catch (Exception ex) { Logger.Log(ex, $"Failed to save LockedCells to {path}"); }
        }
    }
}
