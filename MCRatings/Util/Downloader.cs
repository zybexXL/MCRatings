using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MCRatings
{
    public enum DownloadType { Poster, Person };

    public class DownloadItem
    {
        public string url;
        public string destPath;
        public AutoResetEvent OnFinished;
        public bool running;
        public bool result;
        public bool convertPNG;
        public MovieInfo movie;
        public TMDbMoviePerson person;
        public DownloadType imgType;        

        public DownloadItem(string URL, string path, MovieInfo m, TMDbMoviePerson p = null, bool pngConvert = false)
        {
            url = URL;
            destPath = path;
            movie = m;
            person = p;
            convertPNG = pngConvert;
            imgType = p == null ? DownloadType.Poster : DownloadType.Person;
        }
    }

    // Download Manager 
    // Queue of download items (posters/thumbnails), with X downloading threads in background
    // can perform sync or async downloads
    public class Downloader
    {
        public event EventHandler<DownloadItem> OnDownloadComplete;
        public event EventHandler<int> OnQueueChanged;

        int maxThreads = Constants.MaxDownloadThreads;    // parallel downloads
        int activeThreads = 0;
        List<DownloadItem> Queue = new List<DownloadItem>();
        
        AutoResetEvent signal = new AutoResetEvent(false);
        Thread controller;
        bool stop = false;

        public Downloader()
        {
            controller = new Thread(Controller);
            controller.IsBackground = true;
            controller.Name = "DownloadController";
            controller.Start();
        }

        ~Downloader()
        {
            Stop();
        }

        public void Stop()
        {
            stop = true;
            try {
                controller.Interrupt();
                signal.Set();
            }
            catch { }
        }

        // starts a new synchronous download or waits for an existing one to complete (if file is already downloading)
        public bool DownloadWait(string url, string path, MovieInfo movie, TMDbMovieImage person = null, bool overwrite = false)
        {
            if (url == null || path == null) return false;
            if (!overwrite && File.Exists(path) && new FileInfo(path).Length > 0)
                return true;

            DownloadItem task;
            lock (Queue)
            {
                task = Queue.Find(q => q.destPath == path);
                if (task != null)
                {
                    if (!task.running)
                    {
                        Queue.Remove(task);
                        OnQueueChanged?.Invoke(this, Queue.Count);
                    }
                    else
                        task.OnFinished = new AutoResetEvent(false);
                }
            }

            if (task == null || !task.running)
                return doDownload(new DownloadItem(url, path, movie));
            else
            {
                task.OnFinished.WaitOne();
                return task.result;
            }
        }

        // asynchronous download
        public bool QueueDownload(string url, string path, MovieInfo movie, TMDbMoviePerson person = null, bool priority = false, bool overwrite = false, bool convertToPng = false)
        {
            if (url == null || path == null) return false;
            if (!overwrite && File.Exists(path) && new FileInfo(path).Length > 0)
                return false;

            lock (Queue)
            {
                var task = Queue.Find(q => q.destPath == path);
                if (task != null)
                {
                    if (priority)
                    {
                        Queue.Remove(task);
                        Queue.Insert(0, task);
                    }
                    return true;   // already queued
                }

                task = new DownloadItem(url, path, movie, person, convertToPng);
                if (priority)
                    Queue.Insert(0, task);
                else
                    Queue.Add(task);

                OnQueueChanged?.Invoke(this, Queue.Count);
                signal.Set();
                return true;
            }
        }

        void Controller()
        {
            while (!stop)
            {
                try
                {
                    bool signaled = signal.WaitOne();
                    if (stop) break;

                    lock (Queue)
                    {
                        while (Queue.Count > 0 && activeThreads < maxThreads)
                        {
                            var task = Queue.Where(q => !q.running).FirstOrDefault();
                            if (task == null) break;

                            task.running = true;
                            if (ThreadPool.QueueUserWorkItem(new WaitCallback(Worker), task))
                                Interlocked.Increment(ref activeThreads);
                            else
                                break;  // should never happen // famous last words
                        }
                    }
                }
                catch (ThreadInterruptedException) { break; }
                catch (ThreadAbortException) { break; }
                catch (Exception ex) { Logger.Log(ex, "Download Controller exception"); }
            }
        }

        void Worker(object obj)
        {
            var item = obj as DownloadItem;

            // download
            item.result = doDownload(item);

            lock (Queue)
            {
                Queue.Remove(item);
                item.OnFinished?.Set();
            }

            OnDownloadComplete?.Invoke(this, item);
            OnQueueChanged?.Invoke(this, Queue.Count);

            Interlocked.Decrement(ref activeThreads);
            signal.Set();
        }

        private bool doDownload(DownloadItem item)
        {
            Interlocked.Increment(ref Stats.Session.ImageDownload);
            bool ok = DownloadUrl(item.url, item.destPath);

            if (!ok)
                Interlocked.Increment(ref Stats.Session.ImageDownloadError);
            else
                ok = PostProcess(item);
            
            return ok;
        }

        internal static bool DownloadUrl(string url, string destpath)
        {
            try
            {
                var handler = new HttpClientHandler() { 
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string temp = Path.Combine(Path.GetTempPath(), "MCRatings", Path.GetFileName(url));
                        Directory.CreateDirectory(Path.GetDirectoryName(temp));

                        using (FileStream sw = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
                            response.Content.CopyToAsync(sw).Wait();

                        if (new FileInfo(temp).Length > 0)
                        {
                            if (File.Exists(destpath))
                                File.Delete(destpath);

                            Directory.CreateDirectory(Path.GetDirectoryName(destpath));
                            File.Move(temp, destpath);
                            return true;
                        }
                        else
                        {
                            File.Delete(temp);
                            return false;
                        }
                    }
                    else Logger.Log($"Error {response.StatusCode} downloading image: {url}");
                }
            }
            catch (Exception ex) { Logger.Log(ex, $"Exception downloading image\n\tsource: {url}\n\ttarget: {destpath}"); }

            return false;
        }

        bool PostProcess(DownloadItem item)
        {
            if (item.convertPNG)
                item.destPath = Util.ConvertToPng(item.destPath);

            string script = null;
            if (item.imgType == DownloadType.Person && Program.settings.RunThumbnailScript)
                script = Program.settings.ThumbnailScript;
            else if (item.imgType == DownloadType.Poster && Program.settings.RunPosterScript)
                script = Program.settings.PosterScript;

            if (script == null) return false;
            string image = item.destPath;
            script = Regex.Replace(script, @"\$imagename", quote(Path.GetFileNameWithoutExtension(image)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$imagefile", quote(Path.GetFileName(image)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$imagedir", quote(Path.GetDirectoryName(image)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$image", quote(image), RegexOptions.IgnoreCase);

            string movieFile = item.movie[AppField.File] ?? "";
            script = Regex.Replace(script, @"\$moviename", quote(Path.GetFileNameWithoutExtension(movieFile)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$moviefile", quote(Path.GetFileName(movieFile)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$moviedir", quote(Path.GetDirectoryName(movieFile)), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$movie", quote(movieFile), RegexOptions.IgnoreCase);

            script = Regex.Replace(script, @"\$rating", quote(item.movie[AppField.MPAARating]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$metascore", quote(item.movie[AppField.Metascore]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$imdbscore", quote(item.movie[AppField.IMDbRating]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$rottenscore", quote(item.movie[AppField.RottenTomatoes]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$languages?", quote(item.movie[AppField.Language]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$studios?", quote(item.movie[AppField.Production]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$country", quote(item.movie[AppField.Country]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$awards?", quote(item.movie[AppField.Awards]), RegexOptions.IgnoreCase);

            script = Regex.Replace(script, @"\$title", quote(item.movie.Title), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$originaltitle", quote(item.movie[AppField.OriginalTitle]), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$year", quote(item.movie.Year), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$imdb(id)?", quote(item.movie.IMDBid), RegexOptions.IgnoreCase);

            script = Regex.Replace(script, @"\$namerole", quote($"{item.person?.name} [{item.person?.job ?? item.person?.character}]"), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$name", quote(item.person?.name), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$job", quote(item.person?.job), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$department", quote(item.person?.department), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$character", quote(item.person?.character), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$role", quote(item.person?.job ?? item.person?.character), RegexOptions.IgnoreCase);
            script = Regex.Replace(script, @"\$type", item.person == null ? "POSTER" : item.person.job != null ? "CREW" : "CAST", RegexOptions.IgnoreCase);

            script = Regex.Replace(script, @"\\""(.*?)""", "$1");
            return ExecuteCommand(script, Path.GetDirectoryName(image));
        }

        string quote(string arg)
        {
            char[] needQuotes = { ' ','"','&', '^' };

            arg = arg?.Trim();
            if (string.IsNullOrWhiteSpace(arg)) return "\"\"";   // empty
            if (arg.IndexOfAny(needQuotes) >= 0) return $"\"{arg}\""; // quoted
            return arg;
        }

        bool ExecuteCommand(string cmdLine, string workDir = null)
        {
            if (string.IsNullOrWhiteSpace(cmdLine)) return false;
            try
            {
                Analytics.Event("Image", "PostProcessing", "PostProcessRuns", 1);

                Logger.Log($"Executing PostProcessing command: {cmdLine}");
                SplitCommandLine(cmdLine, out string cmd, out string args);
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    FileName = cmd,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = workDir,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                int timeout = Program.settings.ScriptCommandTimeout * 1000;
                Process p = Process.Start(info);
                if (!p.WaitForExit(timeout))
                {
                    Logger.Log("Error - Command timed out after {timeout} seconds!");
                    try { p.Kill(); } catch { }
                }
                Logger.Log($"Command exit code = {p.ExitCode}");
                return p.ExitCode == 0;
            }
            catch (Exception ex) { Logger.Log(ex, $"ExecuteCommand()"); }
            return false;
            
        }

        void SplitCommandLine(string cmdLine, out string cmd, out string args)
        {
            // check if command string is quoted
            cmdLine = cmdLine.Trim();
            Match m1 = Regex.Match(cmdLine, @"^("".*?"")(.*)$");    // non-greedy match to find first quoted string only
            if (m1.Success)
            {
                cmd = m1.Groups[1].Value;
                args = m1.Groups[2].Value;
            }
            else
            {
                // split on first space (default)
                string[] split = cmdLine.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                cmd = split.Length > 1 ? split[0] : cmdLine;
                args = split.Length > 1 ? split[1] : "";
            }

            cmd = cmd.Trim().Trim('"');
        }
    }
}
