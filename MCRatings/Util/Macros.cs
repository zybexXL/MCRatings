using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCRatings
{
    public class Macro
    {
        public static string resolvePath(string path, string file, MovieInfo movie, TMDbMoviePerson person = null)
        {
            // no quotes added to args
            return new Macro(true).resolve(path, file, movie, person, true);
        }

        public static string resolveScript(string script, string file, MovieInfo movie, TMDbMoviePerson person)
        {
            // quotes added to args if needed
            return new Macro(false).resolve(script, file, movie, person);
        }

        bool noQuotes = false;

        public Macro(bool pathMode = false)
        {
            noQuotes = pathMode;
        }

        string resolve(string path, string filename, MovieInfo movie, TMDbMoviePerson person = null, bool isPathElement = false)
        {
            if (path == null) return null;
            try
            {
                if (filename == null) filename = "";
                path = replace(path, "imagename", Path.GetFileNameWithoutExtension(filename));
                path = replace(path, "imagefile", Path.GetFileName(filename));
                path = replace(path, "imagedir", Path.GetDirectoryName(filename));
                path = replace(path, "image", filename);

                string movieFile = movie[AppField.File] ?? "";
                path = replace(path, "moviename", Path.GetFileNameWithoutExtension(movieFile));
                path = replace(path, "moviefile", Path.GetFileName(movieFile));
                path = replace(path, "moviedir", Path.GetDirectoryName(movieFile));
                path = replace(path, "movie", movieFile);

                path = replace(path, "rating", movie[AppField.MPAARating]);
                path = replace(path, "metascore", movie[AppField.Metascore]);
                path = replace(path, "imdbscore", movie[AppField.IMDbRating]);
                path = replace(path, "rottenscore", movie[AppField.RottenTomatoes]);
                path = replace(path, "languages?", movie[AppField.Language]);
                path = replace(path, "studios?", movie[AppField.Production]);
                path = replace(path, "country", movie[AppField.Country]);
                path = replace(path, "awards?", movie[AppField.Awards]);

                string oTitle = string.IsNullOrEmpty(movie[AppField.OriginalTitle]) ? movie.Title : movie[AppField.OriginalTitle];
                path = replace(path, "title", isPathElement ? Util.SanitizeFilename(movie.Title, false) : movie.Title);
                path = replace(path, "originaltitle", isPathElement ? Util.SanitizeFilename(oTitle, false) : oTitle);
                path = replace(path, "year", movie.Year);
                path = replace(path, "imdb(id)?", movie.IMDBid);
                path = replace(path, "tmdb(id)?", movie[AppField.TMDbID]);

                string role = person?.department == "Writing" ? "Writer" : person?.job ?? person?.character;
                path = replace(path, "namerole", $"{person?.name} [{role}]", true);
                path = replace(path, "name", person?.name, true);
                path = replace(path, "job", person?.job);
                path = replace(path, "department", person?.department);
                path = replace(path, "character", person?.character, true);
                path = replace(path, "role", role, true);
                path = replace(path, "type", person == null ? "POSTER" : person.job != null ? "CREW" : "CAST");

                return path;
            }
            catch (Exception ex) {
                Logger.Log(ex, $"Macros.Resolve: Exception!\n{ex}");
            }
            return null;
        }


        string replace(string text, string tag, string replacement, bool fixChars = false, bool fixPath = false)
        {
            // replace slashes by commas, double quotes by single quotes
            if (fixChars && replacement != null)
            {
                replacement = Regex.Replace(replacement, @" ?[\\/]", ",");
                replacement = replacement.Replace('"', '\'');
            }
            if (replacement == null) replacement = "";

            Regex rx = new Regex($@"(""?)([%\$]){tag}(?:\[(\d+)\s*(,\s*\d+)?\])?", RegexOptions.IgnoreCase);
            int currIndex = 0;
            while (true)
            {
                Match m = rx.Match(text, currIndex);
                if (!m.Success)
                    break;

                bool cut = int.TryParse(m.Groups[3].Value, out int start) && start >= 0;
                if (!int.TryParse(m.Groups[4].Value.Trim(new char[] { ',', ' ' }), out int count) || count < 1)
                    count = 1;

                if (cut)
                {
                    if (start < replacement.Length)
                        replacement = replacement.Substring(start, Math.Min(count, replacement.Length - start));
                    else
                        replacement = "";

                    if (count > replacement.Length) count = replacement.Length;
                    replacement = replacement.Substring(0, count);
                }

                bool isQuoted = m.Groups[1].Value == "\"";
                if (m.Groups[1].Value == "" && m.Groups[2].Value == "$")
                    replacement = quote(replacement);
                replacement = m.Groups[1].Value + replacement;
                
                text = rx.Replace(text, replacement, 1);
                currIndex = m.Index + replacement.Length;
                // %tag => no quotes
                //text = Regex.Replace(text, $@"%{tag}", replacement ?? "", RegexOptions.IgnoreCase);
                // "$tag" (already quoted)
                //text = Regex.Replace(text, $@"""(\${tag})", $"\"{replacement}", RegexOptions.IgnoreCase);
                // $tag => quote if needed
                //text = Regex.Replace(text, $@"\${tag}", quote(replacement), RegexOptions.IgnoreCase);
                //return text;
            }
            if (fixPath && text != null) text = Util.SanitizeFilename(text, false);
            return text;
        }

        string quote(string arg)
        {
            if (noQuotes) return arg;
            char[] needQuotes = { ' ', '"', '&', '^' };

            arg = arg?.Trim();
            if (string.IsNullOrWhiteSpace(arg)) return "\"\"";   // empty
            if (arg.IndexOfAny(needQuotes) >= 0) return $"\"{arg}\""; // quoted
            return arg;
        }

        public static string GetBaseFolder(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            int split = path.IndexOfAny(new char[] { '$', '%' });
            if (split >= 0)
                return Path.GetDirectoryName(path.Substring(0, split) + "dummy");
            return path;
        }
    }
}
