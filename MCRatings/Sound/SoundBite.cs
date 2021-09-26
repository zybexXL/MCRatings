using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{
    // stores info about a soundclip
    public class SoundBite
    {
        public string Name;
        public string Url;
        public List<string> Keywords;
        public bool isMP3;

        public SoundBite(string name, string url, List<string> keys, bool isMP3 = false)
        {
            Name = name;
            Url = url;
            Keywords = keys;
            this.isMP3 = isMP3;
        }
    }
}