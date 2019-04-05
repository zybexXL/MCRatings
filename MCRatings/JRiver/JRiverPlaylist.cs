using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    // holds info about JRiver playlist
    public class JRiverPlaylist
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Filecount { get; set; }
        public string Path { get; set; }

        public JRiverPlaylist(int id, string name, int count, string path = null)
        {
            ID = id;
            Name = name;
            Filecount = count;
            Path = path;
        }
    }
}
