using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    // uses Windows MCI API to play WAVs and MP3
    public class SoundPlayer
    {
        //mciSendString 
        [DllImport("winmm.dll")]
        private static extern int mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        [DllImport("winmm.dll")]
        private static extern bool mciGetErrorString(
           int fdwError,
           StringBuilder ErrorText,
           uint ErrorLen);


        private const string MCIAlias = "media";


        public SoundPlayer()
        {
        }

        public void PlayRandom(string cueString)
        {
            Task.Run(() =>
            {
                List<SoundBite> sounds = FindSoundMatches(cueString);
                if (sounds != null && sounds.Count > 0)
                {
                    int random = new Random().Next(sounds.Count);
                    Play(sounds[random]);
                }

            });
        }


        List<SoundBite> FindSoundMatches(string cues)
        {
            List<SoundBite> matches = new List<SoundBite>();
            cues = cues.ToLower();
            foreach (var s in Constants.SoundBank)
                foreach (var k in s.Keywords)
                    if (cues.Contains(k.ToLower()))
                    {
                        matches.Add(s);
                        break;
                    }
            return matches;
        }

        void Play(SoundBite sound)
        {
            string ext = sound.isMP3 ? "mp3" : "wav";
            string file = Path.Combine(Constants.AudioCache, $"{sound.Name}.{ext}");
            if (!File.Exists(file))
                if (!Download(sound.Url, file))
                    return;

            MCIPlay(file, sound.isMP3);
        }

        bool Download(string url, string file)
        {
            try
            {
                using (var client = new WebClient())
                {
                    // Mute control in settings will be visible if folder exists (after first time something plays)
                    Directory.CreateDirectory(Constants.AudioCache);
                    client.DownloadFile(url, file);
                    return true;
                }
            }
            catch { }
            try { File.Delete(file); } catch { }    // download failed
            return false;
        }

        private bool MCIPlay(string file, bool isMP3)
        {
            try
            {
                return MCICommand($"play \"{file}\" ");

                // MCI device "MPEGVideo" is not working on some machines - but automatic command above just works. Weird.
                //if (MCICommand($"open \"{file}\" type MPEGVideo alias {MCIAlias}"))
                //{
                //    MCICommand($"play {MCIAlias} wait");
                //    MCICommand($"close {MCIAlias}");
                //    return true;
                //}
            }
            catch { }
            return false;
        }

        private bool MCICommand(string command)
        {
            try
            {

                int err = mciSendString(command, null, 0, IntPtr.Zero);
                if (err !=0)
                {
                    StringBuilder sb = new StringBuilder(4096);
                    if (mciGetErrorString(err, sb, 4096))
                        Console.WriteLine(sb.ToString());
                }
                return (err == 0);
            }
            catch { }
            return false;
        }
    }
}
