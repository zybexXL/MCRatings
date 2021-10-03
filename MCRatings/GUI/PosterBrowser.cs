using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class PosterBrowser : Form
    {
        internal event EventHandler OnPosterSelected;
        internal event EventHandler OnPrevKey;
        internal event EventHandler OnNextKey;

        const int WM_APPCOMMAND = 0x0319;

        TMDbMovieImage[] posters;
        List<TMDbMovieImage> filtered;
        TMDbMovieImage currPoster;
        Image currThumbnail;
        Image currFullPoster;
        internal TMDbMovieImage selectedPoster;
        internal MovieInfo currMovie;
        internal bool selectAndLock = false;

        List<Tuple<string, int>> languages = new List<Tuple<string, int>>();
        bool loading = true;
        bool inEvent = false;
        bool closing = false;
        bool isHiRes = false;
        int scrollPos = -1;
        bool showCast = false;

        int FitModeRight = 0;
        int FitModeLeft = 0;
        int SortMode = 0;
        bool SmallThumbs = false;
        string currLanguage;
        DateTime lastScroll = DateTime.MinValue;
        HtmlElement currElement;
        List<string> movieCredits;

        public PosterBrowser()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.logo.GetHicon());
            toolStrip1.Renderer = new ToolStripBorderFix();

            comboLanguage.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            comboLanguage.ComboBox.DrawItem += drawCombobox;
            Width = Screen.FromControl(this).Bounds.Width - 200;
            Height = Screen.FromControl(this).Bounds.Height - 100;
            Left = 100;
            Top = 50;

            SmallThumbs = Program.settings.ShowSmallThumbnails;
            updateToolbar();
            splitContainer1.Panel2.MouseWheel += OnMouseWheel;
            loading = false;
        }

        // capture mouse back/forward keys
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (this.IsHandleCreated && this.Visible && m.Msg == WM_APPCOMMAND)
            {
                long key = (m.LParam.ToInt64() >> 16) & 0x3f;
                // detect APPCOMMAND_BROWSER_BACKWARD
                if (key == 7)
                    processKey(Keys.BrowserHome);
                else if (key == 1)
                    processKey(Keys.BrowserBack);
                else if (key == 2)
                    processKey(Keys.BrowserForward);

            }
            base.WndProc(ref m);
        }

        // handle mouse scrollwheel
        private void OnMouseWheel(object sender, MouseEventArgs args)
        {
            if (browser.Document?.Body == null) return;
            var docArea = browser.Document.Body.ClientRectangle;

            if (browser.Width >= docArea.Width && browser.Height >= docArea.Height)
            {
                if (DateTime.Now.Subtract(lastScroll).TotalMilliseconds > 250)
                {
                    lastScroll = DateTime.Now;
                    if (args.Delta < 0) NextPoster();
                    if (args.Delta > 0) PrevPoster();
                }
            }
        }

        // load posters for a movie
        public void ShowMovie(MovieInfo movie, Image thumbnail, string currRes, bool reload=false)
        {
            if (!reload && posters != null && posters == movie?.tmdbInfo?.images?.posters)
            {
                Show();
                return;
            }

            if (currRes == "no poster")
                thumbnail = null;

            currMovie = movie;
            currThumbnail = thumbnail;
            currFullPoster = null;
            selectedPoster = movie.newPoster;
            Cursor = Cursors.Default;
            scrollPos = -1;

            Text = $"Poster Browser - {movie.Title} ({movie.Year})";
            posters = movie?.tmdbInfo?.images?.posters;
            int idx = 1;
            if (posters != null)
                foreach (var p in posters)
                    p.index = idx++;

            splitContainer1.Panel1Collapsed = thumbnail == null;
            lblRes.Text = currRes;
            ToggleThumbnail(thumbnail, false);
            SetThumbnailScaling();

            Show();
            populateLanguages();
            LoadHome();
            loading = false;

            if (Program.settings.LoadFullSizePoster)
                LoadHighResPoster();

            Analytics.Event("GUI", "PosterBrowser", "PosterBrowserCount", 1);
        }

        // populate languages dropdown with available poster languages
        // reverts to last user-selected language when possible
        private void populateLanguages()
        {
            loading = true;
            string noLang = "No Language";
            //string currLanguage = comboLanguage.SelectedIndex >= 0 ? comboLanguage.Text : null;
            Dictionary<string, int> langs = new Dictionary<string, int>();
            if (posters != null)
                foreach (var poster in posters)
                {
                    string lang = iso639.GetName(poster.iso_639_1) ?? noLang;
                    if (langs.ContainsKey(lang)) langs[lang] = langs[lang] + 1;
                    else langs[lang] = 1;
                }
            var ordered = langs.OrderBy(l => l.Key).ToList();
            ordered.Insert(0, new KeyValuePair<string, int>("All languages", posters == null ? 0 : posters.Length));
            if (langs.ContainsKey(noLang))
            {
                ordered.RemoveAll(p => p.Key == noLang);
                ordered.Add(new KeyValuePair<string, int>(noLang, langs[noLang]));
            }

            comboLanguage.ComboBox.DataSource = ordered;
            comboLanguage.ComboBox.DisplayMember = "Key";
            comboLanguage.ComboBox.ValueMember = "Key";
            comboLanguage.SelectedIndex = 0;

            if (currLanguage != null)
                comboLanguage.Text = currLanguage;
            loading = false;
        }

        // adds playlist filecount to each combobox entry
        private void drawCombobox(object cmb, DrawItemEventArgs args)
        {
            args.DrawBackground();
            if (args.Index >= 0)
            {
                KeyValuePair<string, int> item = (KeyValuePair<string, int>)this.comboLanguage.Items[args.Index];
                string count = $"{item.Value}";

                Rectangle r1 = args.Bounds;
                r1.Width = r1.Width - (int)args.Graphics.MeasureString(count, args.Font).Width - 10;
                using (SolidBrush sb = new SolidBrush(args.ForeColor))
                {
                    args.Graphics.DrawString(item.Key, args.Font, sb, r1);
                }

                if (item.Value >= 0)
                {
                    SizeF size = args.Graphics.MeasureString(count, args.Font);
                    Rectangle r2 = args.Bounds;
                    r2.X = args.Bounds.Width - (int)size.Width - 3;
                    r2.Width = (int)size.Width + 1;

                    using (SolidBrush sb = new SolidBrush(args.State.HasFlag(DrawItemState.Selected) ? Color.White : Color.Blue))
                    {
                        args.Graphics.DrawString(count, args.Font, sb, r2);
                    }
                } 
            }
            args.DrawFocusRectangle();
        }

        // handle PREV toolbar button
        private bool PrevPoster()
        {
            if (currPoster == null || filtered == null || !btnPrev.Enabled) return false;
            int curr = filtered.IndexOf(currPoster);
            if (curr > 0)
            {
                LoadPoster(filtered[curr - 1]);
                return true;
            }
            return false;
        }

        // handle NEXT toolbar button
        private bool NextPoster(bool wrap = false)
        {
            if (currPoster == null || filtered == null || !btnNext.Enabled) return false;
            int curr = filtered.IndexOf(currPoster);
            if (wrap && curr > 0 && curr == filtered.Count - 1) {
                LoadPoster(filtered[0]);
                return true;
            }
            else if (curr >= 0 && curr < filtered.Count - 1)
            {
                LoadPoster(filtered[curr + 1]);
                return true;
            }
            return false;
        }

        private void LoadHome()
        {
            LoadPoster(null);
        }

        private void LoadPoster(TMDbMovieImage poster)
        {
            if (!loading)
            {
                if (currPoster == null && browser.Document != null)
                    scrollPos = browser.Document.GetElementsByTagName("HTML")[0].ScrollTop;
                
                currPoster = poster;
                if (currPoster == null)
                    if (showCast)
                        LoadDocument(getCreditsHtml());
                    else
                        LoadDocument(getMovieHtml());
                else
                    LoadDocument(getPosterHtml(poster));

                updateToolbar();
            }
        }

        private void LoadDocument(string html)
        {
            loading = true;
            if (browser.IsBusy)
            {
                browser.Stop();
                Application.DoEvents();
            }
            browser.DocumentText = html;
            browser.Document.MouseMove += browser_MouseMove;
            loading = false;
        }

        // tracks the HTML element where the mouse is hoovering
        // this is needed to know where the user clicked with SHIFT+Click
        void browser_MouseMove(object sender, HtmlElementEventArgs e)
        {
            currElement = browser.Document.GetElementFromPoint(e.ClientMousePosition);
        }

        // restore last scroll position
        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (currPoster == null && browser.Document?.Body != null && scrollPos > 0)
                browser.Document.GetElementsByTagName("HTML")[0].ScrollTop = scrollPos;
        }

        // handle click on a poster/link
        private void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (loading || e.Url.AbsolutePath == "blank") return;

            int id = -1;
            e.Cancel = true;
            TMDbMovieImage poster = null;
            Match m = Regex.Match(e.Url.Query, @"\?p=(\d+)");
            if (m.Success)
            {
                id = int.Parse(m.Groups[1].Value);
                poster = posters.SingleOrDefault(p => p.index == id);
            }
            switch (e.Url.AbsolutePath)
            {
                case "/":
                    LoadHome(); break;
                case "open":
                    LoadPoster(poster);
                    break;
                case "select":
                    selectPoster(poster, false);
                    break;
                case "lock":
                    selectPoster(poster, true);
                    break;
                case "credits":
                    OpenBrowserCredit(id);
                    break;
            }
        }

        // open poster in external browser
        private void OpenBrowserPoster(TMDbMovieImage poster)
        {
            if (poster == null) return;
            string url = TMDbAPI.GetImageUrl(poster.file_path, PosterSize.Original, out string cachedOriginal);
            OpenBrowser(url);
        }

        // open poster in external browser
        private void OpenBrowserCredit(int id)
        {
            if (movieCredits != null && id >= 0 && movieCredits.Count >= id)
            {
                string url = TMDbAPI.GetImageUrl(movieCredits[id], PosterSize.Original, out string cached);
                OpenBrowser(url);
            }
        }

        private void OpenBrowser(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            try
            {
                Process.Start(url);
            }
            catch { }
        }

        // adjust size of thumbnail/poster according to selected scaling mode
        private void SetThumbnailScaling()
        {
            if (inEvent || imgBox.Image == null || WindowState == FormWindowState.Minimized) return;

            inEvent = true;
            switch (FitModeLeft)
            {
                //case 3: // browser
                case 0: // vertical
                    splitContainer1.Panel1.AutoScroll = false;
                    imgBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
                    imgBox.Height = splitContainer1.Bottom - imgBox.Top - 10;
                    imgBox.Width = imgBox.Image.Width * imgBox.ClientRectangle.Height / imgBox.Image.Height;
                    imgBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    int split = Math.Min(imgBox.Right + 10, splitContainer1.Width - splitContainer1.Panel2MinSize - 10);
                    if (split > 0)
                        splitContainer1.SplitterDistance = split;
                    //panelScroll.Top = splitContainer1.Panel1.Bounds.Height - panelScroll.Height - 2;
                    break;
                case 1: // horizontal
                    imgBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    int w = splitContainer1.Panel1.Bounds.Width - 16;
                    int h = imgBox.Image.Height * (w - 1) / imgBox.Image.Width + 1;
                    if (imgBox.Top + h + 15 > splitContainer1.Panel1.Bounds.Height)
                    {
                        splitContainer1.Panel1.AutoScroll = true;
                        splitContainer1.Panel1.AutoScrollMargin = new Size(0, 10);
                        //panelScroll.Top = imgBox.Top + h + 30;   // force bar
                        w = splitContainer1.Panel1.Bounds.Width - 30;
                        h = imgBox.Image.Height * (w - 1) / imgBox.Image.Width + 1;
                    }
                    else
                    {
                        //chkHighRes.Top = splitContainer1.Panel1.Bounds.Bottom - 20;
                        splitContainer1.Panel1.AutoScroll = false;
                    }
                    imgBox.Width = w;
                    imgBox.Height = h;
                    imgBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
                case 2: // no scale
                    splitContainer1.Panel1.AutoScroll = true;
                    splitContainer1.Panel1.AutoScrollMargin = new Size(10, 10);
                    imgBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    imgBox.Width = imgBox.Image.Width + 1;
                    imgBox.Height = imgBox.Image.Height + 1;
                    imgBox.SizeMode = PictureBoxSizeMode.Normal;
                    //panelScroll.Top = splitContainer1.Panel1.DisplayRectangle.Height - panelScroll.Height - 2;
                    break;
            }
            lblCurr.Width = lblRes.Width = splitContainer1.SplitterDistance - 20;
            lblThumbError.Width = splitContainer1.SplitterDistance - 20 - btnSelectLockOriginal.Right;
            inEvent = false;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!inEvent)
                SetThumbnailScaling();
        }

        private void PosterSelector_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                e.Handled = true;
                Close();
            }
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            if (!loading)
                LoadHome();
        }

        // handles SHIFT-Click on an image (open in browser)
        private void browser_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (ModifierKeys.HasFlag(Keys.Shift) && currElement?.Id != null)
            {
                // open poster
                var m = Regex.Match(currElement.Id, @"poster(\d+)");
                if (m.Success)
                {
                    int id = int.Parse(m.Groups[1].Value);
                    var poster = posters.SingleOrDefault(p => p.index == id);
                    OpenBrowserPoster(poster);
                    return;
                }
                // open Credit 
                m = Regex.Match(currElement.Id, @"credit(\d+)");
                if (m.Success)
                {
                    int id = int.Parse(m.Groups[1].Value);
                    OpenBrowserCredit(id);
                }
            }
        }

        public string getPosterHtml(TMDbMovieImage poster)
        {
            string url = TMDbAPI.GetImageUrl(poster.file_path, PosterSize.Original, out string cachedOriginal);
            string style1 = "";
            string style2 = "border:1px solid gray;";
            switch (FitModeRight)
            {
                //case 3:
                case 0:
                    style1 = "height: calc(100vh - 45px);";
                    style2 += "height: 100%;"; break;
                case 1:
                    style1 = "width: 100%;";
                    style2 += "width: 100%;"; break;
            }
            StringBuilder sb = new StringBuilder();
            string css = getCSS();
            sb.AppendLine($"<!DOCTYPE html><html>"
                + $"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" />"
                + $"<head><style>{css}</style></head><body>");

            string lang = iso639.GetName(poster.iso_639_1) ?? "No Language";
            string res = $"{poster.width}x{poster.height}";
            string vote = poster.vote_average.ToString("0.000");
            int pos = filtered.IndexOf(poster) + 1;
            sb.AppendLine($"<div style=\"margin-top:8px;margin-left:10px;\">");
            sb.AppendLine($"<div class=\"caption\">({pos} of {filtered.Count}) #{poster.index}: {lang} \x25CF {vote} \x25CF <font color=\"008000\">{res}</font></div>");
            sb.AppendLine($"<div style=\"{style1}\"><a href=\"full?p={poster.index}\"><img id=\"poster{poster.index}\" style=\"{style2}\" src=\"{url}\" /></a></div>");
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        public string getMovieHtml()
        {
            StringBuilder sb = new StringBuilder();
            string css = getCSS();
            sb.AppendLine($"<!DOCTYPE html><html>"
                + $"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" />"
                + $"<head><style>{css}</style></head><body>");

            filtered = posters?.ToList();
            if (comboLanguage.SelectedIndex > 0)
                filtered = posters?.Where(p => (iso639.GetName(p.iso_639_1) ?? "No Language") == comboLanguage.Text).ToList();

            if (SortMode == 0)
                filtered = filtered?.OrderByDescending(p => p.width * p.height).ThenByDescending(p => p.vote_average).ToList();
            else
                filtered = filtered?.OrderByDescending(p => p.vote_average).ThenByDescending(p => p.width * p.height).ToList();

            if (filtered != null)
            {
                sb.AppendLine("<div class=\"flex-container wrap\">");
                foreach (var poster in filtered)
                {
                    try
                    {
                        string selected = (poster == selectedPoster) ? "selected" : "";
                        string lang = iso639.GetName(poster.iso_639_1) ?? "No Language";
                        string res = $"{poster.width}x{poster.height}";
                        string vote = poster.vote_average.ToString("0.000");
                        string url = TMDbAPI.GetImageUrl(poster.file_path, SmallThumbs ? PosterSize.Medium : PosterSize.Large, out string cached);
                        if (File.Exists(cached)) url = new Uri(cached).AbsoluteUri;
                        //if (File.Exists(cachedOriginal)) original = new Uri(cachedOriginal).AbsoluteUri;
                        string size = SmallThumbs ? "small" : "large";
                        sb.AppendLine($"<div class=\"flex-item {size}thumb center caption {size}caption {selected}\">");
                        if (SmallThumbs)
                            sb.AppendLine($"<div>#{poster.index}: {lang} \x25CF {vote}<br><font color=\"008000\">{res}</font></div>");
                        else
                            sb.AppendLine($"<div>#{poster.index}: {lang} \x25CF {vote} \x25CF <font color=\"008000\">{res}</font></div>");
                        int width = SmallThumbs ? 185 : 342;
                        sb.AppendLine($"<a href=\"open?p={poster.index}\"><img id=\"poster{poster.index}\" style=\"border:1px solid gray; width: {width}px;\" src=\"{url}\" /></a>");
                        sb.AppendLine($"<div><a href=\"select?p={poster.index}\">Select</a> \x25CF <a href=\"lock?p={poster.index}\">Select+Lock</a></div></div>");
                    }
                    catch { }
                    
                }
                sb.AppendLine("</div>");
            }
            else if (currMovie?.tmdbInfo == null)
                sb.AppendLine("<div class=\"error\">Movie has no posters or TMDb info not loaded yet</div>");
            else
                sb.AppendLine("<div class=\"error\">Movie has no posters</div>");

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        public string getCreditsHtml()
        {
            StringBuilder sb = new StringBuilder();
            string css = getCSS();
            sb.AppendLine($"<!DOCTYPE html><html>"
                + $"<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" />"
                + $"<head><style>{css}</style></head><body>");

            List<string> creditIndex = new List<string>();

            if (currMovie?.tmdbInfo != null)
            {
                var cast = currMovie.tmdbInfo?.getCast(20, !btnShowAvatars.Checked);
                var crew = currMovie.tmdbInfo?.getCrew(2, !btnShowAvatars.Checked);

                sb.AppendLine("<div class=\"flex-container wrap\">");
                foreach (var credit in crew)
                {
                    try
                    {
                        int id = creditIndex.Count;
                        creditIndex.Add(credit.profile_path);
                        string url = TMDbAPI.GetImageUrl(credit.profile_path, SmallThumbs ? PosterSize.Medium : PosterSize.Large, out string cached);
                        if (url == null && cached == null)
                            cached = credit.gender == 1 ? Constants.AvatarFemale : Constants.AvatarMale;
                        if (File.Exists(cached)) url = new Uri(cached).AbsoluteUri;
                        //if (File.Exists(cachedOriginal)) original = new Uri(cachedOriginal).AbsoluteUri;
                        string size = SmallThumbs ? "small" : "large";
                        sb.AppendLine($"<div class=\"flex-item {size}thumb1 center caption smallcaption \">");
                        sb.AppendLine($"<div class=\"namecaption\">{credit.name}</div><div class=\"jobcaption\">{credit.job}</div>");
                        int width = SmallThumbs ? 140 : 280;
                        sb.AppendLine($"<a href=\"credits?p={id}\"><img id=\"credit{id}\" style=\"border:1px solid gray; width: {width}px;\" src=\"{url}\" /></a>");
                        sb.AppendLine($"<br><br></div>");
                    }
                    catch { }
                }
                sb.AppendLine("</div>");

                sb.AppendLine("<div class=\"flex-container wrap\">");
                foreach (var credit in cast)
                {
                    try
                    {
                        int id = creditIndex.Count;
                        creditIndex.Add(credit.profile_path);
                        string url = TMDbAPI.GetImageUrl(credit.profile_path, SmallThumbs ? PosterSize.Medium : PosterSize.Large, out string cached);
                        if (url == null && cached == null)
                            cached = credit.gender == 1 ? Constants.AvatarFemale : Constants.AvatarMale;
                        if (File.Exists(cached)) url = new Uri(cached).AbsoluteUri;
                        //if (File.Exists(cachedOriginal)) original = new Uri(cachedOriginal).AbsoluteUri;
                        string size = SmallThumbs ? "small" : "large";
                        sb.AppendLine($"<div class=\"flex-item {size}thumb1 center caption smallcaption \">");
                        sb.AppendLine($"<div class=\"namecaption\">{credit.name}</div><div class=\"rolecaption\">{credit.character}</div>");
                        int width = SmallThumbs ? 140 : 280;
                        sb.AppendLine($"<a href=\"credits?p={id}\"><img id=\"credit{id}\" style=\"border:1px solid gray; width: {width}px;\" src=\"{url}\" /></a>");
                        sb.AppendLine($"<br><br></div>");
                    }
                    catch { }

                }
                sb.AppendLine("</div>");
            }
            else if (currMovie?.tmdbInfo == null)
                sb.AppendLine("<div class=\"error\">Movie has no actors/crew or TMDb info not loaded yet</div>");
            else
                sb.AppendLine("<div class=\"error\">Movie has no actors/crew</div>");

            sb.AppendLine("</body></html>");

            movieCredits = creditIndex;
            return sb.ToString();
        }

        private string getCSS()
        {
            // #a0ffa0 = light green
            // #ffd040 = orange
            return $@"
body {{
    margin: 0;
    padding: 0;
    background: #FFFFC0;
}}

img {{  vertical-align: middle; }}

.flex-container {{
  padding: 0;
  margin-top: 8px;
  margin-left: 1px;
  list-style: none;
  border: 0px solid red;
  -ms-box-orient: horizontal;
  display: -webkit-box;
  display: -moz-box;
  display: -ms-flexbox;
  display: -moz-flex;
  display: -webkit-flex;
  display: flex;
}}

.wrap    {{ 
  -webkit-flex-wrap: wrap;
  flex-wrap: wrap;
}}  

.flex-item {{
  padding: 0px;
  margin: 0px;
  margin-bottom: 10px;
  margin-right: 0px;
}}

.smallthumb {{ width: 205px; }}
.largethumb {{ width: 362px; }}
.smallthumb1 {{ width: 160px; }}
.largethumb1 {{ width: 300px; }}

.selected {{ background: #a0ffa0; }}

.center {{ text-align: center; }}

.caption {{
  color: #404040;
  font-weight: bold;
  font-size: 9pt;
  font-family: Segoe UI, Calibri, Arial, Helvetica, sans-serif;
  line-height: 24px; 
}}

.smallcaption {{
  line-height: 18px;
}}

.namecaption {{
  color: #008000;
  font-weight: bold;
  font-size: 1.2em;
  line-height: 18px; 
}}

.jobcaption {{
  color: #a04000;
  font-weight: bold;
  font-size: 1.0em;
}}

.rolecaption {{
  color: #0000FF;
  font-weight: bold;
  font-style: italic;
  font-size: 1.0em;
}}

.error {{
  margin-top: 30px;
  margin-left: 5px;
  font-weight: bold;
  font-size: 10pt;
  color: #ff4040;
  font-family: Segoe UI, Calibri, Arial, Helvetica, sans-serif;
}}
";
        }

        private void PosterSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        public void Exit()
        {
            closing = true;
            this.Close();
        }

        // double-click on split separator = split 50/50
        private void splitContainer1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.X - splitContainer1.SplitterDistance) < 5)
                splitContainer1.SplitterDistance = splitContainer1.Width / 2 - 1;
        }

        // click = toggle between thumbnail and full-res poster
        // shift-click = open full poster in external viewer
        private void img1_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
                OpenBrowser(currMovie.currPosterPath);
            else if (isHiRes)
                ToggleThumbnail(currThumbnail, false);
            else if (currFullPoster != null)
                ToggleThumbnail(currFullPoster, true);
            else
                LoadHighResPoster();
        }

        private void ToggleThumbnail(Image img, bool isFullPoster)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { ToggleThumbnail(img, isFullPoster); });
                return;
            }

            this.Cursor = Cursors.Default;
            string tooltip = "";
            if (img != null)
            {
                lblRes.ForeColor = isFullPoster ? Color.Green : Color.Black;
                imgBox.Image = img;
                isHiRes = isFullPoster;
                lblThumbError.Visible = false;
                tooltip = isFullPoster ? "Full-size Poster" : "This is a thumbnail.\nClick to load full-size Poster";
                SetThumbnailScaling();
            }
            else
            {
                if (currThumbnail != null)
                    ToggleThumbnail(currThumbnail, false);
                lblThumbError.Visible = true;
                lblRes.ForeColor = Color.Red;
                tooltip = "This is a thumbnail.\nFull-size Poster failed to load, click to retry";
            }
            toolTip1.SetToolTip(imgBox, $"{tooltip}\n\nPoster location:\n{currMovie?.currPosterPath}");
        }

        // loads original poster from movie location
        private void LoadHighResPoster()
        {
            if (isHiRes || currMovie.currPosterPath == null) return;
            if (currFullPoster != null)
                ToggleThumbnail(currFullPoster, true);
            else
            {
                MovieInfo curr = currMovie;
                this.Cursor = Cursors.WaitCursor;
                Task.Run(() =>
                {
                    var img = Util.LoadImageFromUrl(curr.currPosterPath, curr.JRKey.ToString());
                    if (currMovie == curr)
                    {
                        currFullPoster = img;
                        ToggleThumbnail(img, true);
                    }
                });
            }
        }

        private void PosterSelector_Resize(object sender, EventArgs e)
        {
            SetThumbnailScaling();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            showCast = false;
            LoadHome();
        }

        private void updateToolbar()
        {
            toolStrip1.SuspendLayout();
            btnThumbsize.Image = SmallThumbs ? Properties.Resources.grid9_64 : Properties.Resources.grid4_64;
            btnSort.Image = SortMode == 0 ? Properties.Resources.dimensions64 : Properties.Resources.thumbsUp64;
            btnFitRight.Image = FitModeRight == 0 ? Properties.Resources.height32 
                : FitModeRight == 1 ? Properties.Resources.width32
                : Properties.Resources.fullsize64;
            btnFitLeft.BackgroundImage = FitModeLeft == 0 ? Properties.Resources.height32
                : FitModeLeft == 1 ? Properties.Resources.width32
                : Properties.Resources.fullsize64;


            btnThumbsize.ToolTipText = $"Showing {(SmallThumbs ? "small":"large")} thumbnails";
            btnSort.ToolTipText = $"Posters sorted by {(SortMode == 0 ? "Resolution" : "Votes")}";
            btnFitRight.ToolTipText = FitModeRight == 0 ? "Poster is scaled to fit vertically" 
                : FitModeRight == 1 ? "Poster is scaled to fit horizontally" 
                : "Showing original sized poster (no scaling)";
            toolTip1.SetToolTip(btnFitLeft, FitModeLeft == 0 ? "Poster is scaled to fit vertically"
                : FitModeLeft == 1 ? "Poster is scaled to fit horizontally"
                : "Showing original sized poster (no scaling)");

            int curr = filtered == null ? -1 : filtered.IndexOf(currPoster);
            btnPrev.Enabled = currPoster != null && curr > 0;
            btnNext.Enabled = currPoster != null && curr >= 0 && curr < filtered.Count - 1;
            btnFitRight.Enabled = currPoster != null;
            btnSort.Enabled = currPoster == null && !showCast;
            btnThumbsize.Enabled = currPoster == null;

            btnSelect.Enabled = btnSelectLock.Enabled = currPoster != null;
            //btnThumbsize.Visible = btnSort.Visible = currPoster == null;
            toolStrip1.ResumeLayout();
        }

        private void btnThumbsize_Click(object sender, EventArgs e)
        {
            SmallThumbs = !SmallThumbs;
            updateToolbar();
            scrollPos = 0;
            LoadHome();
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            SortMode = (1 - SortMode);
            updateToolbar();
            scrollPos = 0;
            LoadHome();
        }

        private void btnFitRight_Click(object sender, EventArgs e)
        {
            FitModeRight++;
            if (FitModeRight == 3) FitModeRight = 0;
            updateToolbar();

            LoadPoster(currPoster);
        }

        private void comboLanguage_DropDownClosed(object sender, EventArgs e)
        {
            browser.Focus();
            currLanguage = comboLanguage.Text;
        }

        private void btnFitLeft_Click(object sender, EventArgs e)
        {
            FitModeLeft++;
            if (FitModeLeft == 3) FitModeLeft = 0;
            updateToolbar();
            SetThumbnailScaling();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currPoster == null)
                OnNextKey?.Invoke(this, EventArgs.Empty);
            else
                NextPoster();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currPoster == null)
                OnPrevKey?.Invoke(this, EventArgs.Empty);
            else
                PrevPoster();
        }

        private void browser_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            processKey(e.KeyData);
        }

        private bool processKey(Keys key)
        {
            switch (key)
            {
                case Keys.Escape:
                    if (currPoster == null) Close();
                    else LoadHome();
                    break;
                case Keys.Up:
                case Keys.Back:
                case Keys.BrowserHome:
                    LoadHome();
                    return true;
                case Keys.Right:
                    NextPoster();
                    return true;
                case Keys.BrowserForward:
                    NextPoster(true);
                    return true;
                case Keys.Left:
                    PrevPoster();
                    return true;
                case Keys.BrowserBack:
                    //if (!PrevPoster())
                        LoadHome();
                    return true;
            }
            return false;
        }

        private void selectPoster(TMDbMovieImage poster, bool locked = false)
        {
            //if (poster == null) return;
            selectedPoster = poster;
            selectAndLock = locked;
            OnPosterSelected?.Invoke(this, EventArgs.Empty);
            LoadHome();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            selectPoster(currPoster, false);
        }

        private void btnSelectLock_Click(object sender, EventArgs e)
        {
            selectPoster(currPoster, true);
        }

        private void btnSelectOriginal_Click(object sender, EventArgs e)
        {
            selectPoster(null, false);
        }

        private void btnSelectLockOriginal_Click(object sender, EventArgs e)
        {
            selectPoster(null, true);
        }

        private void btnCast_Click(object sender, EventArgs e)
        {
            showCast = !showCast;
            btnShowAvatars.Visible = showCast;
            if (showCast)
                Analytics.Event("GUI", "ActorBrowser");
            LoadHome();
        }

        private void btnShowAvatars_Click(object sender, EventArgs e)
        {
            LoadHome();
        }
    }
}
