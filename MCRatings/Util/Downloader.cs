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

namespace ZRatings
{
    public enum DownloadType { Poster, Person };

    public class DownloadItem
    {
        public string url;
        public string destPath;
        public AutoResetEvent OnFinished;
        public bool running;
        public bool result;
        public MovieInfo movie;
        public TMDbMoviePerson person;
        public DownloadType imgType;
        public bool isPlaceholder;
        public bool postProcess;

        public DownloadItem(string URL, string path, MovieInfo m, TMDbMoviePerson p = null, bool process = true)
        {
            url = URL;
            destPath = path;
            movie = m;
            person = p;
            postProcess = process;
            imgType = p == null ? DownloadType.Poster : DownloadType.Person;
            isPlaceholder = p != null && string.IsNullOrEmpty(p.profile_path);
        }
    }

    // Download Manager 
    // Queue of download items (posters/thumbnails), with X downloading threads in background
    // can perform sync or async downloads
    public class Downloader
    {
        public event EventHandler<DownloadItem> OnDownloadComplete;
        public event EventHandler<int> OnQueueChanged;

        int activeThreads = 0;
        List<DownloadItem> Queue = new List<DownloadItem>();
        
        AutoResetEvent signal = new AutoResetEvent(false);
        Thread controller;
        bool stop = false;
        static int count = 0;

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
        public bool DownloadWait(string url, string path, MovieInfo movie, TMDbMoviePerson person = null, bool overwrite = false, bool process = true)
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
                return doDownload(new DownloadItem(url, path, movie, person, process));
            else
            {
                task.OnFinished.WaitOne();
                return task.result;
            }
        }

        // asynchronous download
        public bool QueueDownload(string url, string dest, MovieInfo movie, TMDbMoviePerson person = null, bool priority = false, bool overwrite = false, bool process = true)
        {
            if (url == null || dest == null) return false;
            if (!overwrite && File.Exists(dest) && new FileInfo(dest).Length > 0)
                return false;

            lock (Queue)
            {
                var task = Queue.Find(q => q.destPath == dest);
                if (task != null)
                {
                    if (priority)
                    {
                        Queue.Remove(task);
                        Queue.Insert(0, task);
                    }
                    return true;   // already queued
                }

                task = new DownloadItem(url, dest, movie, person, process);
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
            int maxThreads = Program.settings.ProcessingThreads;
#if DEBUG
            maxThreads = 1;
#endif
            if (maxThreads <= 0 || maxThreads > 2 * Environment.ProcessorCount) maxThreads = Environment.ProcessorCount;

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
            bool ok = false;
            try
            {
                bool pngConvert = item.destPath.ToLower().EndsWith(".png") && !item.url.ToLower().EndsWith(".png");
                if (!item.url.ToLower().StartsWith("http:"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(item.destPath));
                    if (pngConvert)
                        Util.ConvertToPng(item.url, item.destPath, false);
                    else
                        File.Copy(item.url, item.destPath, true);
                    ok = true;
                }
                else
                    ok = DownloadUrl(item.url, item.destPath, pngConvert);

                if (!ok)
                    Interlocked.Increment(ref Stats.Session.ImageDownloadError);
                else
                {
                    if (item.postProcess)
                        ok = PostProcess(item);
                    if (item.isPlaceholder)
                        File.SetCreationTimeUtc(item.destPath, Constants.DummyTimestamp);
                }
            }
            catch (Exception ex) {
                Logger.Log(ex, $"doDownload({item.destPath})");
                ok = false; }
            return ok;
        }

        internal static bool DownloadUrl(string url, string destpath, bool pngConvert = false)
        {
            string temp = null;
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
                        int counter = Interlocked.Increment(ref count);

                        temp = Path.Combine(Path.GetTempPath(), "ZRatings", $"{counter.ToString("D4")}_{Path.GetFileName(url)}");
                        Directory.CreateDirectory(Path.GetDirectoryName(temp));

                        using (FileStream sw = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
                            response.Content.CopyToAsync(sw).Wait();

                        if (new FileInfo(temp).Length > 0)
                        {
                            if (File.Exists(destpath))
                                File.Delete(destpath);

                            Directory.CreateDirectory(Path.GetDirectoryName(destpath));
                            if (pngConvert)
                                Util.ConvertToPng(temp, destpath, true);
                            else
                                File.Move(temp, destpath);
                            return true;
                        }
                    }
                    else Logger.Log($"Error {response.StatusCode} downloading image: {url}");
                }
            }
            catch (Exception ex) {
                Logger.Log(ex, $"Exception downloading image\n\tsource: {url}\n\ttarget: {destpath}"); }
            finally
            {
                try
                {
                    if (temp != null && File.Exists(temp))
                        File.Delete(temp);
                }
                catch { }
            }
            return false;
        } 

        bool PostProcess(DownloadItem item)
        {
            try
            {
                File.SetAttributes(item.destPath, FileAttributes.Archive);

                string script = null;
                if (item.imgType == DownloadType.Person && Program.settings.RunThumbnailScript)
                    script = Program.settings.ThumbnailScript;
                else if (item.imgType == DownloadType.Poster && Program.settings.RunPosterScript)
                    script = Program.settings.PosterScript;

                if (script == null) return true;
                script = Macro.resolveScript(script, item.destPath, item.movie, item.person);

                if (ExecuteCommand(script, Path.GetDirectoryName(item.destPath)))
                {
                    // remove A attribute
                    File.SetAttributes(item.destPath, FileAttributes.Normal);
                    return true;
                }
            }
            catch (Exception ex) { Logger.Log(ex, $"PostProcess {item.destPath}"); }
            return false;
        }

        bool ExecuteCommand(string cmdLine, string workDir = null)
        {
            if (string.IsNullOrWhiteSpace(cmdLine)) return false;

            Analytics.Event("Image", "PostProcessing", "PostProcessRuns", 1);

            SplitCommandLine(cmdLine, out string cmd, out string args);
            if (workDir != null && workDir.StartsWith("\\\\"))
                workDir = null;
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = cmd,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = workDir,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            int timeout = Program.settings.ScriptCommandTimeout * 1000;
            Process p = Process.Start(info);
            int pid = p.Id;
            Logger.Log($"Process {pid,5} Executing PP command: {cmdLine}");

            var stdout = p.StandardOutput.ReadToEndAsync();
            var stderr = p.StandardError.ReadToEndAsync();

            if (!p.WaitForExit(timeout))
            {
                Logger.Log("Error - Command timed out after {timeout} seconds!");
                try { p.Kill(); } catch { }
            }

            string output = stdout.Result;
            string error = stderr.Result;
            Logger.Log($"Process {pid,5} exitcode: {p.ExitCode}, STDOUT:\n{output}");
            if (!string.IsNullOrEmpty(error)) Logger.Log($"Process {pid,5} STDERR:\n{error}");

            return p.ExitCode == 0;
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
