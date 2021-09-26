using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{
    static class Logger
    {
        static string logFile;
        public static bool Enabled = true;
        public static object syncObj = new object();

        public static void Init()
        {
            if (!Enabled) return;
            try
            {
                string folder = Path.Combine(Path.GetTempPath(), "ZRatings");
                Directory.CreateDirectory(folder);
                logFile = Path.Combine(folder, $"ZRatings.{DateTime.Now.ToString("yyyyMMdd")}.log");
            }
            catch { }
        }

        public static void Log(string message)
        {
            if (!Enabled) return;
            if (logFile == null) Init();
            try
            {
                string msg = $"{DateTime.Now.ToString("HH:mm:ss.fff")}  {message}\n";
                lock(syncObj)
                    File.AppendAllText(logFile, msg);
            }
            catch { }
        }

        public static void Log(Exception ex, string message = "")
        {
            if (!Enabled) return;
            if (logFile == null) Init();
            try
            {  
                string msg = $"{DateTime.Now.ToString("HH:mm:ss.fff")}  EXCEPTION: {message}\n   {ex.ToString()}\n\n";
                lock (syncObj)
                    File.AppendAllText(logFile, msg);
            }
            catch { }
        }
    }
}
