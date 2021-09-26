using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{
    // datagrid columns, default JRiver field mapping
    public enum CellColor {
        Default = 0, ActiveRow, SelectedRow, ActiveSelectedRow, ColumnEdit,
        TitleMismatch, Year1Mismatch, NewValue, Overwrite,
        Confirmed, Unconfirmed, Locked      // foreground
    }

    public enum AppField {
        Movie=0, Selected, Filter, Status, FTitle, FYear,       // non-JR fields
        Title, Year, Imported, Playlists, Release, Poster, IMDbID, TMDbID,
        IMDbRating, IMDbVotes, TMDbScore, RottenTomatoes, Metascore, MPAARating,
        Runtime, OriginalTitle, Series, Collections, Tagline, Description, ShortPlot, Genre, Keywords,
        Production, Producer, Director, Writers, Actors, Roles, Language, Country, Budget, Revenue,
        Awards, Trailer, Website, File
    }

    public enum Sources { None, TMDb, OMDb }

    public class Constants
    {
        public static string https => "https://";      // separate prefix to avoid having many URLs in the exe (it triggers AV false positives)

        static string _dataFolder;
        public static string DataFolder
        {
            get
            {
                if (_dataFolder == null) _dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ZRatings");
                return _dataFolder;
            }
        }

        public static string SettingsFile = Path.Combine(DataFolder, "settings.xml");       // TOOD: move to JSON
        public static string LockedCellsFile = Path.Combine(DataFolder, "lockedFields.json");
        public static string StatsFile = Path.Combine(DataFolder, "stats.json");
        public static string OMDBCache = Path.Combine(DataFolder, "cache");
        public static string AudioCache = Path.Combine(DataFolder, "audio");
        public static string PosterCache = Path.Combine(DataFolder, "posters");
        public static string ProfileCache = Path.Combine(DataFolder, "castcrew");
        public static string AvatarMale = Path.Combine(DataFolder, "AvatarMale.png");
        public static string AvatarFemale = Path.Combine(DataFolder, "AvatarFemale.png");

        public static int MaxCacheDays = 7;      // 1 week
        public static DateTime DummyTimestamp = new DateTime(2020, 1, 1, 0, 0, 0);

        public static uint[] CellColors = new uint[]
        {
                (uint)SystemColors.Window.ToArgb(),         // normal
                (uint)SystemColors.Highlight.ToArgb(),      // current
                (uint)Color.Cyan.ToArgb(),                  // selected
                (uint)Color.DarkCyan.ToArgb(),              // current + selected
                (uint)0xFFFFFFC0,                           // editable
                (uint)0xFFFFFF60,                           // title mismatch
                (uint)0xFFE0D0A0,                           // year+1 mismatch
                (uint)Color.LightGreen.ToArgb(),            // new value
                (uint)0xFFFFC040,                           // overwrite
                (uint)Color.Green.ToArgb(),                 // confirmed (foreground)
                (uint)Color.Blue.ToArgb(),                  // unconfirmed (foreground)
                (uint)Color.Red.ToArgb(),                   // locked (foreground)
        };

        // fields which are semicolon-separated lists
        public static List<AppField> listFields = new List<AppField>() {
            AppField.Actors, AppField.Roles, AppField.Collections, AppField.Country, AppField.Director, AppField.Genre,
            AppField.Keywords, AppField.Language, AppField.Producer, AppField.Production, AppField.Writers };

        // datagrid column names
        // datagrid name, settings name, JR field name (default)
        public static Dictionary<AppField, FieldInfo> ViewColumnInfo = new Dictionary<AppField, FieldInfo>()
        {
            { AppField.Movie, new FieldInfo("Movie", true, 50, 0, typeof(MovieInfo)) }, // hidden
            { AppField.Selected, new FieldInfo("", false, 10, 1, typeof(bool)) },
            { AppField.Status, new FieldInfo("Status", true, 75, 1) },
            { AppField.FTitle, new FieldInfo("File Title", false, 250, 0) },
            { AppField.FYear, new FieldInfo("FYear", false, 50, 1) },
            { AppField.Title, new FieldInfo("JRiver Title", "Name", true, 250, 0) },
            { AppField.Year, new FieldInfo("Year", "Year", true, 50, 1) },              //year of "Date"
            { AppField.Release, new FieldInfo("Release", "Date (release)", true, 75, 1) },
            { AppField.Imported, new FieldInfo("Imported", "Date Imported", false, 120, 1) },
            { AppField.IMDbID, new FieldInfo("IMDbID", "IMDb ID", false, 75, 1) },
            { AppField.TMDbID, new FieldInfo("TMDbID", "TMDb ID", true, 60, 1) },
            { AppField.Poster, new FieldInfo("Poster", "Image File", true, 75, 1) },
            { AppField.IMDbRating, new FieldInfo("IMDb", "IMDb Rating", true, 50, 1) },
            { AppField.IMDbVotes, new FieldInfo("Votes", "IMDb Votes", true, 75, 2) },
            { AppField.TMDbScore, new FieldInfo("TMDb", "TMDb Score", true, 50, 1) },
            { AppField.RottenTomatoes, new FieldInfo("Rotten", "Rotten Tomatoes", true, 50, 1) },
            { AppField.Metascore, new FieldInfo("Meta", "Metascore", true, 50, 1) },
            { AppField.MPAARating, new FieldInfo("MPAA", "MPAA Rating", true, 60, 1) },
            { AppField.Runtime, new FieldInfo("Runtime", "Runtime", true, 60, 1) },
            { AppField.Playlists, new FieldInfo("Lists", true, 50, 1) },
            { AppField.Genre, new FieldInfo("Genre", "Genre", true, 100, 0) },
            { AppField.OriginalTitle, new FieldInfo("Original Title", "Original Title", true, 200, 0) },
            { AppField.Series, new FieldInfo("Series", "Series", true, 100, 0) },
            { AppField.Collections, new FieldInfo("Collections", "Collections", true, 100, 0) },
            { AppField.Producer, new FieldInfo("Producer", "Producer", true, 100, 0) },
            { AppField.Director, new FieldInfo("Director", "Director", true, 100, 0) },
            { AppField.Actors, new FieldInfo("Actors", "Actors", true, 100, 0) },
            { AppField.Roles, new FieldInfo("Roles", "Actor Roles", true, 100, 0) },
            { AppField.Tagline, new FieldInfo("Tagline", "Tag Line", true, 200, 0) },
            { AppField.Description, new FieldInfo("Description", "Description", true, 200, 0) },
            { AppField.ShortPlot, new FieldInfo("Short Plot", "Short Plot", true, 200, 0) },
            { AppField.Keywords, new FieldInfo("Keywords", "Keywords", true, 100, 0) },
            { AppField.Production, new FieldInfo("Studios", "Studios", true, 100, 0) },
            { AppField.Writers, new FieldInfo("Writers", "Screenwriter", true, 100, 0) },
            { AppField.Language, new FieldInfo("Language", "Language", true, 100, 0) },
            { AppField.Country, new FieldInfo("Country", "Country", true, 100, 0) },
            { AppField.Budget, new FieldInfo("Budget", "Budget", true, 100, 2) },
            { AppField.Revenue, new FieldInfo("Revenue", "Gross Revenue", true, 100, 2) },
            { AppField.Awards, new FieldInfo("Awards", "Awards", true, 100, 0) },
            { AppField.Trailer, new FieldInfo("Trailer", "Trailer", true, 100, 0) },
            { AppField.Website, new FieldInfo("Website", "Website", true, 100, 0) },
            { AppField.File, new FieldInfo("File", true, 600, 0) },
            { AppField.Filter, new FieldInfo("Filter", true, 50, 0) },          // hidden
        };

        // items used for filename cleanup
        // dots and underscores are removed from filename before this step, so these particules should not include them
        public static string FileCleanup =
            "proper remastered rerip remux repack" +
            " unrated extended theatrical uncut directors collectors" +
            " dubbed subbed cd1 cd2" +
            " webdl web-dl webrip web-rip dvd dvdscr dvdrip dvd-rip hdtv hdrip hd-rip bdrip brrip br-rip bluray blu-ray" +
            " xvid divx h264 x264 hevc h265 x265" +
            " 2160p 1440p 1080p 720p 480p 576p imax" +
            " dts ac3 ac5 dd5 atmos truehd true-hd hdr mp3 flac";
        //" german italian french english japanese chinese spanish portuguese";

#if SOUNDFX
        public static List<SoundBite> SoundBank = new List<SoundBite>()
        {
            new SoundBite( "brains", "http://soundbible.com/grab.php?id=1051&type=mp3",    // braaaains
                new List<string>(){ "zombie", "of the dead" }, true  ),

            new SoundBite( "indianatheme", "http://www.wavlist.com/movies/227/ij2-theme.wav",    // indiana theme
                new List<string>(){ "indiana", "harrison ford" }  ),

            new SoundBite( "dodgethis", "http://www.wavlist.com/movies/043/mtrx-dodgethis.wav",   // dodge this
                new List<string>(){ "matrix" }  ),

            new SoundBite( "gottapee", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/forrest_gump/pee.wav",   // I gotta pee
                new List<string>(){ "forrest", "tom hanks" }  ),

            new SoundBite( "goodmorning", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/good_morning_vietnam/good_morning.wav",   // good morning
                new List<string>(){ "vietnam", "robin williams" }  ),

            new SoundBite( "offer", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/godfather/offer.wav",   // offer he can't refuse
                new List<string>(){ "godfather", "mafia", "marlon brando" }  ),

            new SoundBite( "chewbacca", "http://www.rosswalker.co.uk/star_wars_sounds/wavs/chewy.wav",   // chewbacca
                new List<string>(){ "starwars", "star wars" }  ),

            new SoundBite( "yoda", "http://www.rosswalker.co.uk/star_wars_sounds/wavs/yoda_try.wav",   // yoda - there is no try
                new List<string>(){ "starwars", "star wars", "yoda" }  ),

            new SoundBite( "usetheforce", "http://www.rosswalker.co.uk/star_wars_sounds/wavs/Obi_Wan_force_2.wav",   // use the force (incomplete?)
                new List<string>(){ "starwars", "star wars" }  ),

            new SoundBite( "iamyourfather", "http://www.rosswalker.co.uk/star_wars_sounds/wavs/vader_father.wav",   // I am your father
                new List<string>(){ "starwars", "star wars" }  ),

            new SoundBite( "damnitjim", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/star_trek/dammit_jim.wav",   // damnit jim
                new List<string>(){ "startrek", "star trek" }  ),

            new SoundBite( "beammeup", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/star_trek/beam_me_up.wav",   // scotty, beam me up
                new List<string>(){ "startrek", "star trek" }  ),

            new SoundBite( "illbeback", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/terminator_1/be_back.wav",   // I'll be back
                new List<string>(){ "terminator", "schwarzenegger" }  ),

            new SoundBite( "iamdracula", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/dracula/welcome.wav",   // I'm Dracula
                new List<string>(){ "dracula", "vampire" }  ),

            new SoundBite( "welcometotheparty", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/die_hard/welcome.wav",   // welcome to the party 
                new List<string>(){ "die hard", "bruce willis" } ),

            new SoundBite( "whoaaa", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/bill_and_teds_excellent/whoa.wav",   // whoaaa
                new List<string>(){ "matrix", "keanu reeves" }  ),

            new SoundBite( "seenthings", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/blade_runner/seen_things.wav",   // I've seen things...
                new List<string>(){ "blade runner", "rutger hauer" } ),

            new SoundBite( "gameover", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/aliens/game_over2.wav",   // game over man
                new List<string>(){ "alien" } ),

            new SoundBite( "bond", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/007/bond2.wav",   // bond, james bond
                new List<string>(){ "james bond", "sean connery", "daniel craig" }  ),

            new SoundBite( "feelluckypunk", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/movie_stars/eastwood/punk.wav",   // do you feel lucky, Punk?
                new List<string>(){ "dirty harry", "clint eastwood" }  ),

            new SoundBite( "goodbadugly", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/movie_stars/eastwood/good_bad_ugly.wav",   // the good, the bad and the ugly
                new List<string>(){ "clint eastwood" }  ),

            new SoundBite( "makemyday", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/movie_stars/eastwood/make_my_day.wav",   // make my day
                new List<string>(){ "clint eastwood" } ),

            new SoundBite( "heresjohnny", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/movies/misc/shining_heres_johnny.wav",   // heeeere's johnny!
                new List<string>(){ "shining", "jack nicholson" } ),

            new SoundBite( "runforrest", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/movies/forrest_gump/run.wav",   // run forrest, run
                new List<string>(){ "forrest", "tom hanks" }  ),

            new SoundBite( "batmaaaan", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/tv/batman/batman_theme_x.wav",   // batmaaaan
                new List<string>(){ "batman" } ),

            new SoundBite( "filthycriminals", "http://www.wavsource.com/snds_2018-06-03_5106726768923853/tv/batman/criminals.wav",   // you filthy criminals
                new List<string>(){ "crime" }  ),

            new SoundBite( "needforspeed", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/top_gun/top_gun_need.wav",   // I feel the need for speed
                new List<string>(){ "tom cruise", "top gun" } ),

            new SoundBite( "hatesnakes", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/raiders_of_the_lost_ark/snakes.wav",   // I hate snakes
                new List<string>(){ "harrison ford", "snake" } ),

            new SoundBite( "raptor", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/jurassic_park/raptor.wav",   // jurassic raptor
                new List<string>(){ "jurassic" }  ),

            new SoundBite( "pileofshit", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/jurassic_park/big_pile.wav",  // big pile of shit
                new List<string>(){ "jurassic" }  ),

            new SoundBite( "theyrealldead", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/mission_impossible/dead2.wav",   // they're all dead
                new List<string>(){ "mission impossible", "tom cruise" }  ),

            new SoundBite( "donttheyeverdie", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/xmen/ever_die.wav",   // don't they ever die
                new List<string>(){ "x-men", "immortal", "highlander" }  ),

            new SoundBite( "gladiator", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/gladiator/what_we_do.wav",   // what we do in life echoes in eternity
                new List<string>(){ "russel crowe", "gladiator" }  ),

            new SoundBite( "onlyone", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/highlander/only_one.wav",   // there can be only one
                new List<string>(){ "immortal", "highlander" }  ),

            new SoundBite( "phonehome", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/et/et_phone_home.wav",   // ET phone home
                new List<string>(){ "spielberg", "e.t." } ),

            //new SoundBite( "bestinlife", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/conan_the_barbarian/best_in_life.wav",   // What is best in life
            //    new List<string>(){ "schwarzenegger", "conan" } ),

            new SoundBite( "lookingatyou", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/casablanca/looking2.wav",   // looking at you
                new List<string>(){ "humphrey bogart" }  ),

            new SoundBite( "haveproblem", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/apollo13/houston_problem.wav",   // houston we have a problem
                new List<string>(){ "apollo" }  ),

            new SoundBite( "cantdothat", "http://www.rosswalker.co.uk/movie_sounds/sounds_files_20150201_1096714/2001_and_2010/cantdo.wav",   //I'm afraid I can't do that
                new List<string>(){ "odyssey", "kubrick" } ),

        };
#endif
    }
}
