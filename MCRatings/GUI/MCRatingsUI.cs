using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace MCRatings
{

    public partial class MCRatingsUI : Form
    {
        OMDbAPI omdbAPI = new OMDbAPI(Program.settings.APIkeyList);
        TMDbAPI tmdbAPI = new TMDbAPI(Program.settings.TMDBkeyList);
        JRiverAPI jrAPI = new JRiverAPI();
        List<MovieInfo> movies = new List<MovieInfo>();
        bool loading = true;
        int currSearchTask = 0;
        int lastClickedRow = 0;
        int currentPlaylist = -1;
        bool dragSelect = false;
        int dragStartRow = -1;
        Point dragStart = Point.Empty;
        bool spacePressed = false;
        Point ContextMenuPosition;
        private SoundPlayer Player = new SoundPlayer();
        string clipPlaylists = "MCRatings.Playlists";
        List<MovieInfo> copiedMovies = null;       // last CTRL+C
        ImageTooltip imgTooltip;
        PosterBrowser posterBrowser;
        Downloader downloader;
        bool inEvent = false;

        public MCRatingsUI()
        {
            InitializeComponent();
            gridMovies.DoubleBuffered(true);

            this.comboLists.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboLists.DrawItem += drawCombobox;
            comboLists.DisplayMember = "Name";
            imgTooltip = new ImageTooltip();
            imgTooltip.Clicked += OnImageTooltipClicked;

            posterBrowser = new PosterBrowser();
            posterBrowser.OnPosterSelected += PosterBrowser_OnPosterSelected;
            downloader = new Downloader();
            downloader.OnDownloadComplete += imgTooltip.OnImageDownloaded;
            downloader.OnQueueChanged += (sender, count) => UpdateTaskCount(count);

            this.Text = $"MCRatings v{Program.version} - {Program.tagline}";
            Stats.Init();
        }

        #region Form and Keyboard/Button events

        private void Form1_Load(object sender, EventArgs e)
        {
            // fill current monitor (with border)
            this.Width = Screen.FromControl(this).Bounds.Width - 200;
            this.Height = Screen.FromControl(this).Bounds.Height - 100;
            this.Left = 100;
            this.Top = 50;

            // load playlists
            if (!GetPlayLists(true))
                this.Close();
            loading = false;

            ApplyColors();
            UpdateTaskCount(0);

            // check for upgrade in background task
            Task.Run(() =>
            {
                if (AutoUpgrade.CheckUpgrade(checkOnly: true))
                    SetStatus($"MCRatings v{AutoUpgrade.LatestVersion.version} is available! Click there to update  \u27a4 \u27a4 \u27a4", true);
            });
        }

        private void MCRatingsUI_Shown(object sender, EventArgs e)
        {
            if (Program.settings.StartMaximized)
                this.WindowState = FormWindowState.Maximized;

            // check field map, show SettingsUI if needed
            bool showSettings = !Program.settings.valid;
            if (!showSettings)
                foreach (var map in Program.settings.FieldMap.Values)
                {
                    if (map.field == AppField.Poster || (map.field == AppField.Collections && !Program.settings.Collections))
                        continue;
                    if (map.enabled && (string.IsNullOrWhiteSpace(map.JRfield) || !jrAPI.Fields.ContainsKey(map.JRfield.ToLower())))
                        showSettings = true;
                }

            // show SettingsUI if there are no valid settings (first time) or if field mapping has problems
            if (showSettings)
            {
                if (new SettingsUI(jrAPI, true).ShowDialog() != DialogResult.OK)
                    if (!Program.settings.valid)
                        this.Close();
                ApplyColors();
            }
            // show About dialog once (flag is reset during upgrade)
            else if (Program.settings.appVersion != Program.version.ToString())
            {
                Program.settings.appVersion = Program.version.ToString();
                Program.settings.Save();
                new About().ShowDialog();
            }

            comboLists.Focus();
        }

        private void MCRatingsUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSave.Enabled)
                if (DialogResult.Cancel == MessageBox.Show("You have unsaved changes.\nAre you sure you want to exit?", "Discard changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                    e.Cancel = true;

            downloader.Stop();
            posterBrowser?.Exit();
            LockedCells.Save();
            Stats.Save();
        }

        private void btnAbout_MouseDown(object sender, MouseEventArgs e)
        {
            lblStatus.Focus();
            if (e.Button == MouseButtons.Right)
                new StatsUI().ShowDialog();
            else
                new About().ShowDialog();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            lblStatus.Focus();
            if (new SettingsUI(jrAPI).ShowDialog() == DialogResult.OK)
            {
                ApplyColors();
                omdbAPI = new OMDbAPI(Program.settings.APIkeyList);
                tmdbAPI = new TMDbAPI(Program.settings.TMDBkeyList);
            }
        }

        // handle some special keys - F3, Tab, ctrl+F, Alt-P
        private void MCRatingsUI_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.KeyCode == Keys.P && e.Alt)
            {
                comboLists.Focus();
                comboLists.DroppedDown = true;
            }
            else if (e.KeyCode == Keys.F3)
                btnSearch_Click(null, EventArgs.Empty);
            else if (e.KeyCode == Keys.Tab && gridMovies.Focused)
                txtSearch.Focus();
            else if (e.KeyCode == Keys.F && e.Control)      // CTRL+F - find
                txtSearch.Focus();
            else if (e.KeyCode == Keys.X && e.Alt)          // ALT+X = Exit
                this.Close();
            else
                e.Handled = false;

            e.SuppressKeyPress = e.Handled;
        }

        // capture CTRL+C on a datagridView cell in normal or Edit mode
        // CTRL+C is not captured by KeyPress/Down events, needs to be on this global event
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
                if (gridMovies.CurrentCell != null && (gridMovies.Focused || gridMovies.IsCurrentCellInEditMode))
                {
                    // copy list of selected movies (for ctrl+shift+v)
                    copiedMovies = GetSelectedMovies();  
                    // copy current cell value
                    string value = gridMovies.CurrentCell.Value.ToString();
                    if (gridMovies.CurrentCell.IsInEditMode && gridMovies.CurrentCell.EditType == typeof(DataGridViewTextBoxEditingControl))
                    {
                        var textbox = gridMovies.EditingControl as DataGridViewTextBoxEditingControl;
                        if (textbox != null && !string.IsNullOrEmpty(textbox.SelectedText))
                            value = textbox.SelectedText;
                    }
                    if (!string.IsNullOrEmpty(value))
                        try { Clipboard.SetText(value.Trim()); } catch { }
                    return true;
                }

            // CTRL+SHIFT+V = paste movie Info
            if (keyData == (Keys.Control | Keys.Shift | Keys.V) && copiedMovies != null)
            {
                var destMovies = GetSelectedMovies();
                PasteSelectedMovies(copiedMovies, destMovies);
                return true;    
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // paste 1 or more movies into other rows - useful when replacing existing movies with new version
        // copies Playlist membership and DateImported as well
        // if 1 movie copied => paste movie info, overwriting info
        // if multiple movies copied => paste only on matching IMDB rows
        private void PasteSelectedMovies(List<MovieInfo> source, List<MovieInfo> dest)
        {
            if (source == null || dest == null || source.Count == 0 || dest.Count == 0)
                return;

            DateTime start = DateTime.Now;
            bool refresh = false;
            foreach (var m in dest)
            {
                if (string.IsNullOrEmpty(m.IMDBid)) continue;
                var src = source.Where(s => s.IMDBid == m.IMDBid).SingleOrDefault();
                if (src == null && dest.Count == 1 && source.Count == 1)
                    src = source[0];    // for single movie copy, allow different IMDBid

                if (src != null && src != m)
                {
                    var locked = LockedCells.GetLockedFields(m.JRKey);
                    bool updated = false;
                    foreach (AppField f in Enum.GetValues(typeof(AppField)))
                        if (f >= AppField.Title && f != AppField.File && f != AppField.Poster && (locked == null || !locked.Contains(f)))
                        {
                            overwriteField(m, f, src[f], chkOverwrite.Checked, true);
                            refresh = true;
                            updated = true;
                        }
                    if (updated)
                        setMovieStatus(m, true);
                }
            }

            if (refresh)
            {
                UpdateDataGrid(start);
                gridMovies.Refresh();
                updateModifiedCount();
            }
        }

        #endregion

        #region UI status labels

        void ApplyColors()
        {
            gridMovies.DefaultCellStyle.BackColor = getColor(CellColor.Default);
            gridMovies.DefaultCellStyle.SelectionBackColor = getColor(CellColor.ActiveRow);
            if (gridMovies.Columns.Count > 0)
            {
                gridMovies.Columns[(int)AppField.IMDbID].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.FTitle].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.FYear].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.Imported].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.Playlists].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);

            }
            gridMovies.Refresh();

            // other cell colors are applied by onPaint events
        }

        Color getColor(CellColor color)
        {
            if (Program.settings.CellColors != null && Program.settings.CellColors.Length > (int)color)
                return Color.FromArgb((int)Program.settings.CellColors[(int)color]);
            if (Constants.CellColors.Length > (int)color)
                return Color.FromArgb((int)Constants.CellColors[(int)color]);
            return Color.Pink;
        }
    
        // update status label
        private void SetStatus(string status, bool isError = false)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { SetStatus(status, isError); });
                return;
            }
            lblStatus.Text = status;
            lblStatus.ForeColor = isError ? Color.Green : Color.Blue;
        }

        private void updateSelectedCount()
        {
            int count = movies.Count(m => m.selected);
            lblSelected.Text = $"({count})";
            lblSelected.ForeColor = count == 0 ? SystemColors.ControlText : Color.Teal;
        }

        private void updateModifiedCount()
        {
            int count = movies == null ? 0 : movies.Count(m => m.isDirty);
            lblChanges.Text = count == 0 ? "no changes" : $"{count} changed movie{(count > 1 ? "s" : "")}";
            lblChanges.ForeColor = count == 0 ? SystemColors.ControlDarkDark : Color.Red;
            btnSave.Enabled = count > 0;
        }

        private void chkOverwrite_CheckedChanged(object sender, EventArgs e)
        {
            chkOverwrite.ForeColor = chkOverwrite.Checked ? Color.Red : SystemColors.ControlText;
        }

        private void UpdateTaskCount(int tasks = 0)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { UpdateTaskCount(tasks); });
                return;
            }

            lblTaskCount.Text = tasks.ToString();
            lblTaskCount.Visible = imgSpinner.Visible = tasks > 0;
            if (tasks > 0)
            {
                lblStatus.Width = imgSpinner.Left - lblStatus.Left - 10;
                lblTaskCount.Left = (imgSpinner.Width - lblTaskCount.Width) / 2 + imgSpinner.Left;
            }
            else
                lblStatus.Width = btnAbout.Left - lblStatus.Left - 10;
        }

        #endregion

        #region JRiver connect

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            string currList = comboLists.Text;
            if (GetPlayLists())
                comboLists.Text = currList;
            comboLists.Focus();
        }

        private bool GetPlayLists(bool startup = false)
        {
            var progressUI = new ProgressUI("Connecting to JRiver...", ConnectJRiver, null, false);
            if (startup)
                progressUI.StartPosition = FormStartPosition.CenterScreen;

            progressUI.ShowDialog();

            if (!jrAPI.Connected)
            {
                Analytics.Event("JRiver", "ConnectFailed");
                progressUI.Close();
                MessageBox.Show("Cannot connect to JRiver, please make sure it's installed on this PC", "No JRiver!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (jrAPI.Playlists.Count == 0)
            {
                MessageBox.Show("Failed to load list of Video Playlists from JRiver!\nDo you have movies?", "No Videos?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            SetStatus($"Connected to JRiver v{jrAPI.Version} - {jrAPI.Library}");
            if (startup)
            {
                Analytics.Event("JRiver", "Connected", $"JRiver v{jrAPI.Version.ToString()}", 1);
                Analytics.Event("JRiver", "ListPlaylists", "PlayListCount", jrAPI.Playlists.Count);
            }

            comboLists.DataSource = jrAPI.Playlists;
            var allfiles = jrAPI.Playlists.Where(l => l.Name == "All Files (empty search)").FirstOrDefault();
            if (allfiles != null)
                comboLists.SelectedItem = allfiles;
            else
                comboLists.SelectedItem = jrAPI.Playlists.FirstOrDefault();

            currentPlaylist = comboLists.SelectedIndex;
            return true;
        }

        private void ConnectJRiver(ProgressInfo progress)
        {
            progress.result = false;
            progress.subtitle = "Starting JRiver";
            progress.Update(true);
            if (!jrAPI.Connect())
                return;

            progress.subtitle = "Reading playlists";
            progress.Update(true);
            foreach (var playlist in jrAPI.getPlaylists())
            {
                if (!Program.settings.FastStart)
                {
                    progress.subtitle = $"Reading playlist - {playlist.Name}";
                    progress.Update(true);
                }
            }

            progress.subtitle = "Reading fields";
            progress.Update();
            jrAPI.getFields();

            progress.result = true;
        }

        #endregion

        #region PlayList loading

        // adds playlist filecount to each combobox entry
        private void drawCombobox(object cmb, DrawItemEventArgs args)
        {
            args.DrawBackground();
            JRiverPlaylist item = (JRiverPlaylist)this.comboLists.Items[args.Index];

            Rectangle r1 = args.Bounds;
            r1.Width = r1.Width - 40;
            using (SolidBrush sb = new SolidBrush(args.ForeColor))
            {
                args.Graphics.DrawString(item.Name, args.Font, sb, r1);
            }

            if (item.Filecount >= 0)
            {
                string txt = item.Filecount.ToString();
                SizeF size = args.Graphics.MeasureString(txt, args.Font);
                Rectangle r2 = args.Bounds;
                r2.X = args.Bounds.Width - (int)size.Width - 1;
                r2.Width = (int)size.Width + 1;

                using (SolidBrush sb = new SolidBrush(args.State.HasFlag(DrawItemState.Selected) ? Color.White : Color.DarkCyan))
                {
                    args.Graphics.DrawString(item.Filecount.ToString(), args.Font, sb, r2);
                }
            }
        }

        private void comboLists_DropDownClosed(object sender, EventArgs e)
        {
            if (!loading && comboLists.SelectedIndex != currentPlaylist)
                btnLoad_Click(null, EventArgs.Empty);
        }

        private void comboLists_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && comboLists.SelectedIndex != currentPlaylist)
            {
                e.Handled = true;
                btnLoad_Click(null, EventArgs.Empty);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (btnSave.Enabled)
            {
                if (DialogResult.Cancel == MessageBox.Show("Loading a playlist will discard all changes.\nAre you sure you want to continue?", "Discard changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                {
                    loading = true;
                    comboLists.SelectedIndex = currentPlaylist;
                    loading = false;
                    return;
                }
            }

            currentPlaylist = comboLists.SelectedIndex;
            if (!jrAPI.Connect())
            {
                MessageBox.Show("Cannot connect to JRiver, please make sure it's installed on this PC", "No JRiver!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var bar = new ProgressUI("Loading JRiver movies", LoadJRiverMovies, comboLists.SelectedItem);
            if (bar.ShowDialog() == DialogResult.OK && bar.progress.result == true)
                PopulateDataGrid();

            Analytics.Event("JRiver", "LoadPlaylist", "MoviesLoaded", movies.Count);

            updateModifiedCount();
            updateSelectedCount();
        }

        private void LoadJRiverMovies(ProgressInfo progress)
        {
            progress.result = false;
            JRiverPlaylist playlist = progress.args as JRiverPlaylist;
            if (playlist == null) return;

            LockedCells.Load(jrAPI.Library);

            List<MovieInfo> newMovies = new List<MovieInfo>();
            progress.totalItems = playlist.Filecount;
            progress.Update();

            int i = 0;
            int[] IDs = Util.IdentityArray(Environment.ProcessorCount);
            Task.WaitAll(IDs.Select(id => Task.Run(() =>
            {
                foreach (var movie in jrAPI.getMovies(playlist, id, IDs.Length))    // iterator
                {
                    if (progress.cancelled)
                        break;
                    if (progress.totalItems < 0)
                        progress.totalItems = playlist.Filecount;
                    progress.currentItem = Interlocked.Increment(ref i);
                    if (movie != null)
                    {
                        lock (newMovies)
                            newMovies.Add(movie);
                        progress.subtitle = movie.Title;
                        Interlocked.Increment(ref progress.success);
                    }
                    else
                        Interlocked.Increment(ref progress.fail);

                    progress.Update(false);

                    while (progress.paused)
                        Thread.Sleep(250);
                }
            })).ToArray());

            if (!progress.cancelled)
            {
                movies = newMovies;
                progress.result = true;
            }
        }

        private void UpdateDataGrid(DateTime minTime)
        {
            BindingSource bs = gridMovies.DataSource as BindingSource;
            DataTable dt = bs.DataSource as DataTable;
            foreach (DataRow row in dt.Rows)
            {
                MovieInfo m = row[(int)AppField.Movie] as MovieInfo;
                if (m.modified >= minTime)
                {
                    row[(int)AppField.Selected] = m.selected;
                    foreach (AppField c in Enum.GetValues(typeof(AppField)))
                        if ((int)c > 1) row[(int)c] = m[c];     // skip first 2

                    // sortable numeric columns
                    row[(int)AppField.IMDbVotes] = (m[AppField.IMDbVotes] ?? "").PadLeft(10);
                    row[(int)AppField.RottenTomatoes] = (m[AppField.RottenTomatoes] ?? "").PadLeft(3);
                    row[(int)AppField.Metascore] = (m[AppField.Metascore] ?? "").PadLeft(3);
                    row[(int)AppField.Runtime] = (m[AppField.Runtime] ?? "").PadLeft(3);
                }
            }
            gridMovies.Refresh();
        }

        private void PopulateDataGrid(bool updateStatus = true)
        {
            copiedMovies = null;
            gridMovies.DataSource = null;
            lastClickedRow = 0;
            txtSearch.Text = "";
            chkShowSelected.Checked = false;
            ApplyFilter();

            // create datatable and columns
            DataTable dt = new DataTable();
            foreach (AppField c in Enum.GetValues(typeof(AppField)))
            {
                FieldInfo info = Constants.ViewColumnInfo[c];
                dt.Columns.Add(c.ToString(), info.dataType);
            }

            // add movie rows
            foreach (var m in movies)
            {
                object[] values = new object[dt.Columns.Count];
                values[(int)AppField.Movie] = m;
                values[(int)AppField.Selected] = m.selected;
                foreach (AppField c in Enum.GetValues(typeof(AppField)))
                    if ((int)c > 1) values[(int)c] = m[c];     // skip first 2

                // sortable numeric columns
                values[(int)AppField.IMDbVotes] = (m[AppField.IMDbVotes] ?? "").PadLeft(10);
                values[(int)AppField.RottenTomatoes] = (m[AppField.RottenTomatoes] ?? "").PadLeft(3);
                values[(int)AppField.Metascore] = (m[AppField.Metascore] ?? "").PadLeft(3);
                values[(int)AppField.Runtime] = (m[AppField.Runtime] ?? "").PadLeft(3);
                dt.Rows.Add(values);
            }

            // apply datasource
            BindingSource bs = new BindingSource();
            bs.DataSource = dt;
            gridMovies.DataSource = bs;
            bs.Sort = (Program.settings.SortByImportedDate ? "Imported DESC, " : "") + "Title, Year";

            // fix column headers, set read only, hide disabled columns
            gridMovies.Columns[(int)AppField.Selected].HeaderText = "";
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
            {
                var info = Constants.ViewColumnInfo[f];
                gridMovies.Columns[(int)f].HeaderText = info.GridHeader;
                gridMovies.Columns[(int)f].ReadOnly = info.Readonly;
                gridMovies.Columns[(int)f].Width = info.Width;
                gridMovies.Columns[(int)f].DefaultCellStyle.Alignment =
                    info.Alignment == 0 ? DataGridViewContentAlignment.TopLeft :
                    info.Alignment == 1 ? DataGridViewContentAlignment.TopCenter :
                    DataGridViewContentAlignment.TopRight;

                if (Program.settings.FieldMap.TryGetValue(f, out JRFieldMap map)
                    && !map.enabled)
                    gridMovies.Columns[(int)f].Visible = false;
            }

            // column sizes, sort, hide helper columns
            gridMovies.Columns[(int)AppField.Selected].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            gridMovies.Columns[(int)AppField.Selected].SortMode = DataGridViewColumnSortMode.Automatic;
            gridMovies.Columns[(int)AppField.Movie].Visible = false;
            gridMovies.Columns[(int)AppField.Filter].Visible = false;
            gridMovies.Columns[(int)AppField.Collections].Visible = Program.settings.Collections;
            
            // column colors
            gridMovies.Columns[(int)AppField.IMDbID].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.FTitle].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.FYear].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.Imported].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.Playlists].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.Status].DefaultCellStyle.BackColor = Color.Gainsboro;

            gridMovies.Columns[(int)AppField.Playlists].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            // adjust column order
            gridMovies.Columns[(int)AppField.Title].DisplayIndex = 4;
            gridMovies.Columns[(int)AppField.Year].DisplayIndex = 5;
            gridMovies.Columns[(int)AppField.IMDbID].DisplayIndex = 6;
            gridMovies.Columns[(int)AppField.IMDbID].Frozen = true;

            if (gridMovies.Rows.Count > 0)
                gridMovies.CurrentCell = gridMovies.Rows[0].Cells[1];

            gridMovies.Refresh();
            gridMovies.Focus();
        }

        #endregion

        #region GetMovieInfo

        private List<MovieInfo> GetSelectedMovies(bool selectCurrent = true)
        {
            var selected = movies.Where(m => m.selected).ToList();
            if (selected.Count == 0 && selectCurrent && gridMovies.CurrentRow != null)
            {
                MovieInfo currmovie = (MovieInfo)gridMovies.CurrentRow.Cells[0].Value as MovieInfo;
                if (currmovie != null)
                    selected = new List<MovieInfo> { currmovie };
            }
            return selected;
        }

        private void btnGetMovieInfo_Click(object sender, EventArgs e)
        {
            bool noCache = ModifierKeys.HasFlag(Keys.Shift);
            bool forceBlanks = ModifierKeys.HasFlag(Keys.Control);
            var selected = GetSelectedMovies(true); 

            if (selected.Count == 0)
            {
                MessageBox.Show("Please select movies to process", "No rows selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!omdbAPI.hasKeys)
            {
                MessageBox.Show("Please enter the API key(s) for OMDb in Settings", "Missing API key", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (noCache && selected.Count > 100)
            {
                if (DialogResult.No == MessageBox.Show($"Are you sure you want to get OMDb info for {selected.Count} movies with Cache disabled?", "Disable OMDb cache", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    return;
            }

            // play a soundbite
            if (!Program.settings.Silent)
            {
                string audiocues = getAudioCues(selected);
                Player.PlayRandom(audiocues);
                Analytics.Event("Audio", "PlaySoundClip");
            }

            int oCalls = Stats.Session.OMDbAPICall;
            int tCalls = Stats.Session.TMDbAPICall;

            var bar = new ProgressUI("Getting movie information", GetMovieInfo, selected);
            bar.progress.totalItems = selected.Count;
            bar.progress.canOverwrite = chkOverwrite.Checked;
            bar.progress.useAltTitle = chkUseJRTitle.Checked;
            bar.progress.noCache = noCache;
            bar.progress.blanks = forceBlanks;

            bar.ShowDialog();   // ignore result - even if cancelled, the already updated movies are kept
            
            string msg = $"{bar.progress.success} updated";
            if (bar.progress.fail > 0) msg += $", {bar.progress.fail} failed";
            if (bar.progress.skip > 0) msg += $", {bar.progress.skip} skipped";
            SetStatus(msg);

            // analytics
            if (bar.progress.success > 0) Analytics.Event("API", "GetMovieInfo", "GetInfoSuccess", bar.progress.success);
            if (bar.progress.fail > 0) Analytics.Event("API", "GetMovieInfo", "GetInfoFails", bar.progress.fail);
            if (Stats.Session.OMDbAPICall - oCalls > 0) Analytics.Event("API", "OMDb API Calls", "OMDbAPICalls", Stats.Session.OMDbAPICall - oCalls);
            if (Stats.Session.TMDbAPICall - tCalls > 0) Analytics.Event("API", "TMDb API Calls", "TMDbAPICalls", Stats.Session.TMDbAPICall - tCalls);
            if (bar.progress.success > 0)
                Analytics.Timing("API", "GetMovieInfo", "AverageTime", (int)(DateTime.Now - bar.progress.startTime).TotalMilliseconds/bar.progress.success);

            this.Cursor = Cursors.WaitCursor;
            UpdateDataGrid(bar.progress.startTime);
            updateModifiedCount();
            updateSelectedCount();
            this.Cursor = Cursors.Default;

            if (posterBrowser != null && posterBrowser.Visible)
                ShowPictureBrowser(posterBrowser.currMovie, false, true);

            if (omdbAPI.lastResponse == 401)
                MessageBox.Show("OMDb keys are invalid or reached the daily limit!", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Error);

            gridMovies.Focus();
        }

        private string getAudioCues(List<MovieInfo> movies)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var movie in movies)
                sb.AppendLine($"{movie.IMDBid}|{movie.Title}|{movie.FTitle}|{movie[AppField.Actors]}|{movie[AppField.Director]}");

            return sb.ToString();
        }

        private void GetMovieInfo(ProgressInfo progress)
        {
            progress.result = false;
            List<MovieInfo> movies = progress.args as List<MovieInfo>;
            if (movies == null) return;
            if (omdbAPI != null) omdbAPI.lastResponse = -1;  // reset
            if (tmdbAPI != null) tmdbAPI.lastResponse = -1;  // reset

            int i = 0;
            int[] IDs = Util.IdentityArray(Environment.ProcessorCount);
            Task.WaitAll(IDs.Select(id => Task.Run(() =>
            {
                for (int x = id; x < movies.Count; x += IDs.Length)
                {
                    if (progress.cancelled)
                        return;

                    while (progress.paused)
                        Thread.Sleep(250);

                    var movie = movies[x];
                    bool FindByName = string.IsNullOrEmpty(movie[AppField.IMDbID]);
                    string title = progress.useAltTitle ? movie.Title : movie.FTitle;
                    string year = progress.useAltTitle ? movie.Year : movie.FYear;
                    progress.currentItem = Interlocked.Increment(ref i);
                    progress.subtitle = title;
                    progress.Update(false);

                    string omdb = null;
                    string tmdb = null;
                    OMDbMovie omdbInfo = null;
                    TMDbMovie tmdbInfo = null;
                    string imdb = FindByName ? null : movie[AppField.IMDbID];
                    bool doOMDb = !Program.settings.OMDbDisabled;
                    bool doTMDb = !Program.settings.TMDbDisabled;

                    if (FindByName)
                    {
                        Sources preference = Program.settings.FieldMap[AppField.IMDbID].source;
                        if (doTMDb && preference == Sources.TMDb)
                            tmdbInfo = tmdbAPI?.getByTitle(title, year);
                        if (doOMDb && (preference == Sources.OMDb || tmdb == null))
                            omdbInfo = omdbAPI?.getByTitle(title, year);
                        // get TMDB if preference was OMDB but it returned null
                        if (doTMDb && preference == Sources.OMDb && omdb == null && tmdb == null)
                            tmdbInfo = tmdbAPI?.getByTitle(title, year);

                        imdb = omdbInfo?.imdbID ?? tmdbInfo?.imdb_id;
                    }

                    if (doOMDb && imdb != null && omdb == null)
                        omdbInfo = omdbAPI?.getByIMDB(imdb, noCache: progress.noCache);

                    if (doTMDb && imdb != null && tmdb == null)
                        tmdbInfo = tmdbAPI?.getByIMDB(imdb, noCache: progress.noCache);

                    movie.omdbInfo = omdbInfo;
                    movie.tmdbInfo = tmdbInfo;
                    movie.newPoster = null;
                    movie.lockPoster = false;

                    if ((omdbInfo != null && omdbInfo.isValid) || (tmdbInfo != null && tmdbInfo.isValid))
                    {
                        bool ok = false;
                        Interlocked.Increment(ref progress.success);
                        movie.clearUpdates();
                        movie.setUpdate(AppField.Imported, movie[AppField.Imported]);
                        movie.setUpdate(AppField.Playlists, movie[AppField.Playlists]);

                        foreach (AppField f in Enum.GetValues(typeof(AppField)))
                            if (f >= AppField.Title && f != AppField.Imported && f!= AppField.Playlists && f!= AppField.File)
                                ok |= overwriteField(movie, f, getPreferredValue(f, omdbInfo, tmdbInfo), progress.canOverwrite, progress.blanks);

                        if (movie.isModified(AppField.Poster))
                            movie.newPoster = tmdbInfo.bestPoster;

                        setMovieStatus(movie, true);
                        movie.selected = false;
                    }
                    else
                    {
                        setMovieStatus(movie, false);
                        Interlocked.Increment(ref progress.fail);
                    }
                }
            })).ToArray());

            progress.subtitle = "downloading poster and cast/crew thumbnails";
            progress.Update(true);

            // queue poster downloads
            int posterCount = 0;
            int thumbCount = 0;
            if (Program.settings.PostersEnabled)
                foreach (var m in movies)
                    if (m.isModified(AppField.Poster))
                    {
                        if (DownloadPoster(m)) posterCount++;
                        if (DownloadThumbnail(m)) thumbCount++;
                    }

            // queue actor/crew downloads
            int actorCount = 0;
            if (Program.settings.SaveActorThumbnails && Directory.Exists(Program.settings.ActorFolder))
                foreach (var m in movies)
                    actorCount += DownloadActors(m);

            if (actorCount > 0) Analytics.Event("Image", "ActorDownload", "ActorDownloads", actorCount);
            if (posterCount > 0) Analytics.Event("Image", "PosterDownload", "PosterDownloads", posterCount);
            if (thumbCount > 0) Analytics.Event("Image", "ThumbDownload", "ThumbnailDownloads", thumbCount);

            progress.skip = movies.Count - progress.success - progress.fail;
            progress.result = !progress.cancelled;
        }

        private void setMovieStatus(MovieInfo movie, bool updated = false)
        {
            string isNew = movie.JRKey < 0 ? "NEW, " : "";
            if (updated)
                movie[AppField.Status] = movie.isDirty ? $"{isNew}updated" : $"{isNew}no change";
            else
                movie[AppField.Status] = $"{isNew}not found";
        }

        private string getPreferredValue(AppField field, OMDbMovie omdb, TMDbMovie tmdb)
        {
            string ovalue = omdb?.Get(field);
            string tvalue = tmdb?.Get(field);
            if (string.IsNullOrWhiteSpace(ovalue)) return tvalue;
            if (string.IsNullOrWhiteSpace(tvalue)) return ovalue;

            return Program.settings.FieldMap[field].source == Sources.OMDb ? ovalue : tvalue;
        }

        // overwrites a cell if overwrite flags are enabled for the field
        // first restores original cell value to prevent stale info from previous GET operations to remain behind
        private bool overwriteField(MovieInfo movie, AppField field, string value, bool masterOverwrite, bool acceptBlanks = false, bool forced = false)
        {
            Program.settings.FieldMap.TryGetValue(field, out var map);
            bool fieldOverwrite = map?.overwrite ?? true;
            bool fieldEnabled = map?.enabled ?? true;

            if (LockedCells.isLocked(movie.JRKey, field)) return false;

            if (fieldEnabled && field != AppField.IMDbID && field != AppField.Collections)
                if (movie.JRKey >= 0 || field != AppField.Imported)
                    movie[field] = movie.originalValue(field);    // restore original value 
            if (!acceptBlanks && string.IsNullOrEmpty(value)) return false;

            movie.setUpdate(field, value);

            if (fieldEnabled && (value ?? "") != (movie[field] ?? "") && (string.IsNullOrEmpty(movie[field]) || (fieldOverwrite && masterOverwrite)) || field == AppField.Collections)
            {
                if (field == AppField.Poster && !forced && Util.PixelCount(value) <= Util.PixelCount(movie[field]))
                    return false;
                
                // special handling for Revenue and Budget - only overwrite if value is higher
                if ((field == AppField.Revenue || field == AppField.Budget) && Util.NumberValue(value) < Util.NumberValue(movie[field]))
                    return false;

                // special handling for Collection (merge with existing list)
                if (field == AppField.Collections)
                    value = mergeList(movie[field], value);

                movie[field] = value;
                return true;
            }
            return false;
        }

        private string mergeList(string list1, string list2)
        {
            if (string.IsNullOrWhiteSpace(list1)) return list2;
            if (string.IsNullOrWhiteSpace(list2)) return list1;
            var l1 = list1.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var l2 = list2.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var l1lower = l1.Select(l => l.ToLower()).ToList();

            foreach (var l in l2)
                if (!l1lower.Contains(l.ToLower()))
                {
                    l1.Add(l);
                    l1lower.Add(l.ToLower());
                }
            return string.Join("; ", l1);
        }
        #endregion

        #region JRiver Save
        private void btnSave_Click(object sender, EventArgs e)
        {
            var changed = movies.Where(m => m.isDirty).ToList();
            if (changed.Count == 0)
            {
                MessageBox.Show("No modified movies, nothing to save.", "Nothing changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!jrAPI.CheckConnection() && !jrAPI.Connect())
            {
                MessageBox.Show("Can't connect to JRiver, please check!", "Connection lost", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bar = new ProgressUI("Saving changes", SaveChanges, changed);
            bar.progress.totalItems = changed.Count;
            bar.ShowDialog();

            string msg = $"{bar.progress.success} saved";
            if (bar.progress.fail > 0) msg += $", {bar.progress.fail} failed";
            if (bar.progress.skip > 0) msg += $", {bar.progress.skip} skipped";

            if (bar.progress.success > 0) Analytics.Event("JRiver", "Save", "MoviesSaved", bar.progress.success);
            if (bar.progress.fail > 0) Analytics.Event("JRiver", "Save", "MoviesSaveFailed", bar.progress.fail);

            if (posterBrowser != null && posterBrowser.Visible)
                ShowPictureBrowser(posterBrowser.currMovie, false, true);

            SetStatus(msg);
            UpdateDataGrid(bar.progress.startTime);
            updateModifiedCount();
            if (bar.progress.fail > 0)
                MessageBox.Show($"Error saving changed movies to JRiver!\n{bar.progress.fail} movies still have unsaved changes."
                    + jrAPI.lastException == null ? "" : $"\n\nException: {jrAPI.lastException?.Message}",
                    "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

            gridMovies.Focus();
        }

        private void SaveChanges(ProgressInfo progress)
        {
            progress.result = false;
            List<MovieInfo> movies = progress.args as List<MovieInfo>;
            if (movies == null) return;

            int i = 0;
            foreach (var movie in movies)
            {
                if (progress.cancelled)
                    return;

                while (progress.paused)
                    Thread.Sleep(250);

                bool hasPoster = movie.isModified(AppField.Poster) && movie.newPoster != null;
                progress.currentItem = ++i;
                progress.subtitle = (progress.useAltTitle ? movie.Title : movie.FTitle ) + (hasPoster ? " [with poster]" : "");
                progress.Update(false);

                bool posterLock = movie.lockPoster;
                if (hasPoster) 
                    movie.newPosterPath = SavePoster(movie);

                if (jrAPI.SaveMovie(movie))
                {
                    //movie.clearUpdates();
                    movie[AppField.Status] = "saved";
                    progress.success++;
                }
                else
                {
                    progress.fail++;
                    movie[AppField.Status] = "save failed";
                }

                if (hasPoster && posterLock && !movie.isModified(AppField.Poster))
                    LockedCells.Lock(movie.JRKey, AppField.Poster);
            }

            progress.result = true;
        }
        #endregion

        #region Search and Filter
    
        // clicking the "X changed movies" label triggers a filter to show only changed movies
        private void lblChanges_Click(object sender, EventArgs e)
        {
            if (!btnSave.Enabled) return;

            txtSearch.Text = "";
            SelectRows(new Predicate<MovieInfo>(m => m.isDirty), true);
            chkShowSelected.Checked = true;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search();
            gridMovies.Focus();
        }

        // search first match or next match
        // if Filter checkbox is on, just applies the datagrid Filter
        private void Search(bool fromNext = true)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate () { Search(fromNext); });
                return;
            }
            if (chkFilter.Checked)
                ApplyFilter();
            else
            {
                int next = fromNext ? 1 : 0;
                if (string.IsNullOrEmpty(txtSearch.Text)) return;
                string query = txtSearch.Text.ToLower();
                int start = gridMovies.CurrentRow == null ? 0 : gridMovies.CurrentRow.Index;
                for (int i = 0; i < gridMovies.Rows.Count; i++)
                {
                    int r = i + start + next;
                    if (r >= gridMovies.Rows.Count)
                        r = r - gridMovies.Rows.Count;  // loop around

                    DataGridViewRow row = gridMovies.Rows[r];
                    MovieInfo info = row.Cells[(int)AppField.Movie].Value as MovieInfo;
                    if ((row.Cells[(int)AppField.Filter].Value as string).Contains(query))
                    {
                        gridMovies.CurrentCell = row.Cells[1];
                        txtSearch.ForeColor = Color.Black;
                        return;
                    }
                }
                txtSearch.ForeColor = Color.Red;    // not found
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            Search();
            txtSearch.Focus();
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                Search();
            }
            else if (e.KeyChar == 27)
            {
                txtSearch.Text = "";
                Search();
                e.Handled = true;
            }
            else
            {
                // do search a few ms after a keypress
                Task.Run(() => delayedSearch(++currSearchTask));
            }
        }

        // search after 500ms
        public void delayedSearch(int counter)
        {
            Thread.Sleep(500);
            if (currSearchTask == counter)
                Search(false);
        }

        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
            gridMovies.Focus();
        }

        private void ApplyFilter()
        {
            string filter = "";
            txtSearch.ForeColor = Color.Black;
            gridMovies.ClearSelection();
            BindingSource bs = gridMovies.DataSource as BindingSource;
            if (bs == null) return;

            inEvent = true;
            if(chkFilter.Checked && !string.IsNullOrEmpty(txtSearch.Text))
            {
                // escape *%[]" - enclose in square brackets; remove single quotes
                string text = Regex.Replace(txtSearch.Text, @"([""\*%\[\]])", @"[$1]");
                text = text.Replace("'", "");
                filter = $"[Filter] like '%{text}%'";
            }
            if (chkShowSelected.Checked)
                if (filter == "") filter = "[Selected] = True";
                else filter += " and [Selected] = True";

            bs.RemoveFilter();
            bs.Filter = filter;

            gridMovies.ClearSelection();
            gridMovies.Refresh();
            txtSearch.ForeColor = bs.Count == 0 ? Color.Red : Color.Black;
            inEvent = false;
        }

        private void chkShowSelected_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
            gridMovies.Focus();
        }

        #endregion

        #region Context Menu
        
        // selects/unselects rows based on predicate
        private void SelectRows(Predicate<MovieInfo> predicate, bool fullList = false)
        {
            gridMovies.ClearSelection();
            List<DataRowView> changed = new List<DataRowView>();
            BindingSource bs = gridMovies.DataSource as BindingSource;
            if (bs == null) return;

            inEvent = true;
            bs.SuspendBinding();

            if (fullList)
                bs.RemoveFilter();

            for (int i = 0; i < bs.Count; i++)
                changed.Add((DataRowView)bs[i]);
            foreach (DataRowView row in changed)
            {
                MovieInfo movie = (MovieInfo)row[(int)AppField.Movie] as MovieInfo;
                bool selected = predicate(movie);
                movie.selected = selected;
                row[(int)AppField.Selected] = selected;
            }

            if (chkShowSelected.Checked)
                ApplyFilter();      // resets inEvent
            inEvent = true;
            gridMovies.ClearSelection();
            bs.ResumeBinding();
            gridMovies.Refresh();
            updateSelectedCount();
            inEvent = false;
        }

        private void menuSelectAll_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => true), chkShowSelected.Checked);
        }

        private void menuClearSelection_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => false));
            chkShowSelected.Checked = false;
        }

        private void menuToggleSelection_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.selected), chkShowSelected.Checked);
        }

        private void menuSelectValidID_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !string.IsNullOrEmpty(m.IMDBid)));
        }

        private void menuSelectMissingID_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => string.IsNullOrEmpty(m.IMDBid)));
        }

        private void menuSelectMissingTitle_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => string.IsNullOrEmpty(m.Title) || string.IsNullOrEmpty(m.Year)));
        }

        private void menuSelectMismatchTitle_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.matchTitle || !m.matchYear));
        }

        private void menuSelectMismatchTitleYear1_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.matchTitle || !m.matchYear1));
        }

        private void menuSelectChanged_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.isDirty));
        }

        private void menuSelectUnchanged_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.isDirty));
        }

        private void menuSelectAdded1_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.DateImported > DateTime.Now.Date));
        }

        private void menuSelectAdded3_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.DateImported > DateTime.Now.AddDays(-3)));
        }

        private void menuSelectAdded5_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.DateImported > DateTime.Now.AddDays(-5)));
        }

        private void menuSelectAdded7_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.DateImported > DateTime.Now.AddDays(-7)));
        }

        private void menuSelectAdded30_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.DateImported > DateTime.Now.AddDays(-30)));
        }

        private void menuSelectMissingRatings_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.hasRatings));
        }

        private void menuSelectLocked_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => LockedCells.HasLockedFields(m.JRKey)));
        }

        private void menuSelectNoPoster_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => m.currPosterPath == null));
        }

        private void menuSelectCommonPoster_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Program.settings.PosterFolder))
                return;
            SelectRows(new Predicate<MovieInfo>(m => m.currPosterPath != null
                && m.currPosterPath.StartsWith(Program.settings.PosterFolder, StringComparison.InvariantCultureIgnoreCase)));
        }

        private void menuRevertField_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.RowIndex >= 0 && hit.ColumnIndex > 1 && hit.Type == DataGridViewHitTestType.Cell)
            {
                MovieInfo m = gridMovies.Rows[hit.RowIndex].Cells[0].Value as MovieInfo;
                gridMovies.Rows[hit.RowIndex].Cells[hit.ColumnIndex].Value = m.originalValue((AppField)hit.ColumnIndex);
                m[(AppField)hit.ColumnIndex] = m.originalValue((AppField)hit.ColumnIndex);
            }
            gridMovies.Refresh();
            updateModifiedCount();
        }

        private void menuRevertRow_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.RowIndex >= 0)
            {
                var row = gridMovies.Rows[hit.RowIndex];
                MovieInfo m = row.Cells[0].Value as MovieInfo;
                m.restoreSnapshot();
                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                    if (f >= AppField.Status)
                        row.Cells[(int)f].Value = m[f];
            }
            gridMovies.Refresh();
            updateModifiedCount();
        }

        private void menuRevertThisColumn_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.ColumnIndex > (int)AppField.Status)
            {
                for (int i = 0; i < gridMovies.RowCount; i++)
                {
                    var row = gridMovies.Rows[i];
                    MovieInfo m = row.Cells[0].Value as MovieInfo;
                    row.Cells[hit.ColumnIndex].Value = m.originalValue((AppField)hit.ColumnIndex);
                    m[(AppField)hit.ColumnIndex] = m.originalValue((AppField)hit.ColumnIndex);
                }
            }
            gridMovies.Refresh();
            updateModifiedCount();
        }

        private void menuDiscardChanges_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < gridMovies.RowCount; i++)
            {
                var row = gridMovies.Rows[i];
                MovieInfo m = row.Cells[0].Value as MovieInfo;
                if (m.isDirty)
                {
                    m.restoreSnapshot();
                    foreach (AppField f in Enum.GetValues(typeof(AppField)))
                        if (f >= AppField.Status)
                            row.Cells[(int)f].Value = m.originalValue(f);
                }
                else {
                    row.Cells[(int)AppField.Status].Value = null;
                    m[AppField.Status] = null;
                }
            }
            gridMovies.Refresh();
            updateModifiedCount();
            this.Cursor = Cursors.Default;
        }

        private void menuRevertSelectedRows_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gridMovies.RowCount; i++)
            {
                var row = gridMovies.Rows[i];
                MovieInfo m = row.Cells[0].Value as MovieInfo;
                if (m.selected)
                {
                    m.restoreSnapshot();
                    foreach (AppField f in Enum.GetValues(typeof(AppField)))
                        if (f >= AppField.Status)
                            row.Cells[(int)f].Value = m.originalValue(f);
                }
            }
            gridMovies.Refresh();
            updateModifiedCount();
        }

        private void menuCopyField_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.RowIndex >= 0 && hit.ColumnIndex > 1 && hit.Type == DataGridViewHitTestType.Cell)
            {
                try
                {
                    string text = gridMovies[hit.ColumnIndex, hit.RowIndex].Value?.ToString();
                    if (string.IsNullOrEmpty(text)) return;

                    AppField field = (AppField)hit.ColumnIndex;
                    if (field == AppField.Playlists)
                        Clipboard.SetData(clipPlaylists, text);
                    else
                        Clipboard.SetText(text);
                }
                catch { }
            }
        }

        private void menuPaste_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.RowIndex >= 0 && hit.ColumnIndex > 1 && hit.Type == DataGridViewHitTestType.Cell)
            {
                AppField field = (AppField)hit.ColumnIndex;
                MovieInfo m = gridMovies.Rows[hit.RowIndex].Cells[0].Value as MovieInfo;

                if (LockedCells.isLocked(m.JRKey, field)) return;

                string value = null;
                try
                {
                    if (field == AppField.Playlists)
                    {
                        if (!Clipboard.ContainsData(clipPlaylists))
                            return;
                        else
                            value = (string)Clipboard.GetData(clipPlaylists);
                    }
                    else if (!gridMovies.Columns[hit.ColumnIndex].ReadOnly && Clipboard.ContainsText())
                        value = Clipboard.GetText();
                    else return;
                }
                catch { }
                if (value == null) return;

                gridMovies.Rows[hit.RowIndex].Cells[hit.ColumnIndex].Value = value;
                m[(AppField)hit.ColumnIndex] = value;
                setMovieStatus(m, true);
                gridMovies.Rows[hit.RowIndex].Cells[(int)AppField.Status].Value = m[AppField.Status];
                gridMovies.Refresh();
                updateModifiedCount();
            }
        }

        private void menuLockField_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
            if (hit.RowIndex >= 0 && hit.ColumnIndex > 1 && hit.Type == DataGridViewHitTestType.Cell)
            {
                AppField field = (AppField)hit.ColumnIndex;
                MovieInfo m = gridMovies.Rows[hit.RowIndex].Cells[0].Value as MovieInfo;
                bool state = LockedCells.Toggle(m.JRKey, field);
                gridMovies.InvalidateRow(hit.RowIndex);
            }
        }

        #endregion

        #region datagrid events

        // handles checkbox click - marks row Movie as Selected
        private void grid2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[(int)AppField.Movie].Value as MovieInfo;
            if (m == null) return;

            if (e.ColumnIndex == 1 && !spacePressed)     // select checkbox
            {
                bool select = (bool)gridMovies[1, e.RowIndex].EditedFormattedValue == true;
                BindingSource bs = gridMovies.DataSource as BindingSource;
                DataRowView row = bs[e.RowIndex] as DataRowView;

                bs.SuspendBinding();
                m.selected = select;
                row[(int)AppField.Selected] = select;
                bs.ResumeBinding();

                if (!chkShowSelected.Checked)
                    gridMovies.CurrentCell = gridMovies[3, e.RowIndex];
                updateSelectedCount();
            }
        }

        // applies colors to cells according to movie status/properties
        private void grid2_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = gridMovies.Rows[e.RowIndex];
            MovieInfo m = row.Cells[(int)AppField.Movie].Value as MovieInfo;
            if (m == null) return;

            if (m.selected)
            {
                row.DefaultCellStyle.BackColor = getColor(CellColor.SelectedRow);
                row.DefaultCellStyle.SelectionBackColor = getColor(CellColor.ActiveSelectedRow);
            }
            else row.DefaultCellStyle = null;

            if (!m.matchYear)
                row.Cells[(int)AppField.FYear].Style.BackColor = m.matchYear1 ?
                     getColor(CellColor.Year1Mismatch) : getColor(CellColor.TitleMismatch);
            else row.Cells[(int)AppField.FYear].Style = null;

            if (!m.matchTitle)
                row.Cells[(int)AppField.FTitle].Style.BackColor = getColor(CellColor.TitleMismatch);
            else row.Cells[(int)AppField.FTitle].Style = null;

            if (!m.isDirty && (m[AppField.Status]??"").Contains("updated"))
            {
                m[AppField.Status] = null;
                row.Cells[(int)AppField.Status].Value = null;
            }

            bool hasStatus = m[AppField.Status] != null;
            var locked = LockedCells.GetLockedFields(m.JRKey);
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
            {
                if (f >= AppField.Title && f != AppField.File)
                {
                    int mod = m.isModified(f, m[f]);
                    if (mod > 0)
                    {
                        row.Cells[(int)f].Style.BackColor = mod == 1 ? getColor(CellColor.Overwrite) : getColor(CellColor.NewValue);
                        row.Cells[(int)f].Style.ForeColor = Color.Empty;   // inherit
                    }
                    else
                    {
                        // locked cell - paint foreground, or backround if cell value is blank
                        if (locked != null && locked.Contains(f))
                        {
                            bool empty = string.IsNullOrWhiteSpace(row.Cells[(int)f].Value?.ToString());
                            row.Cells[(int)f].Style.ForeColor = empty ? Color.Empty : getColor(CellColor.Locked);
                            row.Cells[(int)f].Style.BackColor = empty ? getColor(CellColor.Locked) : Color.Empty;
                        }
                        else if (hasStatus)
                        {
                            row.Cells[(int)f].Style.ForeColor = m.isUpdated(f) ? getColor(CellColor.Confirmed) : getColor(CellColor.Unconfirmed);
                            row.Cells[(int)f].Style.BackColor = Color.Empty;   // inherit
                        }
                        else
                            row.Cells[(int)f].Style = null;
                    }
                }
            }

            switch ((m[AppField.Status] ?? "").Replace("NEW, ",""))
            {
                case "updated": row.Cells[(int)AppField.Status].Style.ForeColor = Color.DarkGreen; break;
                case "saved": row.Cells[(int)AppField.Status].Style.ForeColor = Color.DarkGreen; break;
                case "save failed": row.Cells[(int)AppField.Status].Style.ForeColor = Color.Red; break;
                case "not found": row.Cells[(int)AppField.Status].Style.ForeColor = Color.Red; break;
                case "NEW": row.Cells[(int)AppField.Status].Style.ForeColor = Color.Blue; break;
                default: row.Cells[(int)AppField.Status].Style.ForeColor = Color.Black; break;
            }
        }

        // handles cell click
        // Click on row 1 followed by SHIFT+Click on row 2 -> select rows
        // CTRL+click on IMDB ID -> open IMDb page
        // CTRL+click on file path -> open folder
        // 
        private void grid2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            try
            {
                // shift click on a row
                MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                if (ModifierKeys.HasFlag(Keys.Shift) && lastClickedRow >= 0)
                {
                    BindingSource bs = gridMovies.DataSource as BindingSource;
                    bs.SuspendBinding();
                    for (int i = Math.Min(lastClickedRow, e.RowIndex); i <= Math.Max(lastClickedRow, e.RowIndex); i++)
                    {
                        ((MovieInfo)(gridMovies.Rows[i].Cells[0].Value)).selected = true;
                        gridMovies.Rows[i].Cells[(int)AppField.Selected].Value = true;
                    }
                    bs.ResumeBinding();
                    gridMovies.ClearSelection();
                    updateSelectedCount();
                    gridMovies.Refresh();
                }
                lastClickedRow = e.RowIndex;    // row becomes the last clicked

                
                AppField field = (AppField)e.ColumnIndex;
                if (Program.settings.PostersEnabled)
                {
                    if (field == AppField.Poster)
                        ShowPictureBrowser(m, true);    // bring to front
                    else if (posterBrowser?.Visible ?? false)
                        ShowPictureBrowser(m, false);   // change poster
                }

                // handle CTRL+click on IMDB link
                if (field == AppField.IMDbID && ModifierKeys.HasFlag(Keys.Control))
                {
                    string id = gridMovies.CurrentCell.EditedFormattedValue as string;
                    if (id != null && id.ToLower().StartsWith("tt"))
                        Process.Start($"https://www.imdb.com/title/{id.ToLower()}/");
                }
                // handle CTRL+click on Trailer link
                if ((field == AppField.Trailer || field == AppField.Website) && ModifierKeys.HasFlag(Keys.Control))
                {
                    string url = gridMovies.CurrentCell.EditedFormattedValue as string;
                    if (url != null && url.ToLower().StartsWith("http"))
                        Process.Start(url);
                }
                // handle CTRL+click on File path
                else if (field == AppField.File && ModifierKeys.HasFlag(Keys.Control))
                {
                    string path = gridMovies.CurrentCell.EditedFormattedValue as string;
                    if (!string.IsNullOrEmpty(path))
                        Process.Start(Path.GetDirectoryName(path));
                }
            }
            catch { }   // ignore link click errors
        }

        // double click on cell => COPY value to clipboard
        private void grid2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                string text = gridMovies[e.ColumnIndex, e.RowIndex].Value?.ToString();
                if (!string.IsNullOrEmpty(text))
                    try { Clipboard.SetText(text.Trim()); } catch { }
            }
        }

        // Cell End edit => update underlying MovieInfo object, update modified count
        private void grid2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if ((e.ColumnIndex == (int)AppField.IMDbID ||
                e.ColumnIndex == (int)AppField.FTitle ||
                e.ColumnIndex == (int)AppField.Imported ||
                e.ColumnIndex == (int)AppField.FYear))
            {
                MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                string txt = gridMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (e.ColumnIndex == (int)AppField.Imported)
                {
                    if (!DateTime.TryParseExact(txt, "yyyy-M-d H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime imported))
                    {
                        gridMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = m[(AppField)e.ColumnIndex];
                        return;
                    }
                }
                if (m[(AppField)e.ColumnIndex] != txt)
                {
                    m[(AppField)e.ColumnIndex] = txt;
                    if (e.ColumnIndex == (int)AppField.IMDbID || e.ColumnIndex == (int)AppField.Imported)
                        updateModifiedCount();
                }
            }
        }

        // ToolTip needed - get cell contents, wrap text if too long; append [Previous Value] when value changed
        private void grid2_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            e.ToolTipText = null;
            if (Program.settings.PostersEnabled && e.ColumnIndex == (int)AppField.Poster && e.RowIndex >= 0)
            {
                MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                Task.Run(() =>
                {
                    try
                    {
                        DateTime start = DateTime.Now;
                        string url2 = null, poster2 = null;
                        PosterSize size = Program.settings.ShowSmallThumbnails ? PosterSize.Medium : PosterSize.Large;
                        if (m.isModified(AppField.Poster))
                        {
                            // start thumbnail download in another thread (probably already downloaded after GET)
                            url2 = TMDbAPI.GetPosterUrl(m.newPoster?.file_path, size, out poster2);
                            if (url2 != null) downloader.QueueDownload(url2, poster2, m, priority: true);
                        }
                        
                        // get current JRiver thumbnail
                        string poster1 = jrAPI.GetThumbnail(m, Program.settings.ShowSmallThumbnails);
                        
                        //imgTooltip.hide(true);  // dispose previous images

                        // load new images (img2 might not yet exist)
                        var img1 = Util.LoadImage(poster1); 
                        var img2 = Util.LoadImage(poster2);

                        bool locked = LockedCells.isLocked(m.JRKey, AppField.Poster);
                        string label = !locked ? "current" : Program.settings.ShowSmallThumbnails ? "LOCKED" : "current [LOCKED]";
                        string lbl1 = poster1 == null ? "no poster" : $"{label}: {(m.originalValue(AppField.Poster)?.Replace(" ","").Replace("\u00A0", "") ?? "no poster")}";
                        string lbl2 = poster2 == null ? null : $"new: {m.newPoster?.width}x{m.newPoster?.height}";

                        // image2 thumbnail size
                        Size thumbSize = img2 == null ? new Size(0, 0) : new Size(img2.Width, img2.Height);
                        if (m.newPoster != null && img2 == null && url2 != null)
                        {
                            var sz = TMDbAPI.GetThumbnailSize(size, m.newPoster.width, m.newPoster.height);
                            thumbSize = new Size(sz.Item1, sz.Item2);
                        }

                        // only show if cursor is on same cell for more than X miliseconds
                        var ellapsed = DateTime.Now - start;
                        if (!imgTooltip.Visible && ellapsed.TotalMilliseconds < 100)
                           Thread.Sleep(101 - (int)ellapsed.TotalMilliseconds);

                        ShowPosterToolTip(m, img1, lbl1, img2, lbl2, poster2, thumbSize, e.RowIndex, e.ColumnIndex);
                    }
                    catch { imgTooltip.Hide(); }
                });
            }
            else
            {
                if (imgTooltip.Visible) imgTooltip.Hide();
                if (e.ColumnIndex > 2 && e.RowIndex >= 0)
                {
                    var field = (AppField)e.ColumnIndex;
                    MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                    string txt = gridMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    if (field == AppField.IMDbVotes || field == AppField.RottenTomatoes || field == AppField.Metascore || field == AppField.Runtime)
                        txt = txt.Trim();
                    int eq = m.isModified(field, txt);
                    if (eq != 0)
                    {
                        string prev = m.originalValue(field);
                        if (!string.IsNullOrEmpty(prev))
                        {
                            //string sep = prev != null && prev.Length > 80 ? "\r\n" : "";
                            txt += $"\n\n[Previous Value:]\n{prev}";
                        }
                    }
                    string locked = LockedCells.isLocked(m.JRKey, field) ? "[LOCKED VALUE]\n" : "";
                    e.ToolTipText = locked + txt.WordWrap(100);
                }
            }
        }

        // Mouse Down event can be the start of a Mouse-drag select
        private void gridMovies_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var hit = gridMovies.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.Cell)
                {
                    dragStartRow = hit.RowIndex;
                    dragStart = e.Location;
                }
                else
                    dragStartRow = -1;
            }
        }

        // check if we are doing Mouse-drag select, change cursor if so (ignores small mouse drags)
        private void gridMovies_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && dragStart != Point.Empty)
                if (Math.Abs(e.X - dragStart.X) + Math.Abs(e.Y - dragStart.Y) > 10)     // ignore small drag
                {
                    Cursor.Current = Cursors.NoMoveVert;
                    dragSelect = true;
                }
        }

        // When doing mouse-drag select, the mouse release completes the action. This [un]selects the dragged rows.
        private void gridMovies_MouseUp(object sender, MouseEventArgs e)
        {
            //LastMouseClick = e.Location;    // for context menu
            dragStart = Point.Empty;

            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && dragSelect)
            {
                dragSelect = false;
                Cursor.Current = Cursors.Default;

                var hit = gridMovies.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.Cell && hit.RowIndex != dragStartRow)
                {
                    bool select = e.Button == MouseButtons.Right;
                    if (!ModifierKeys.HasFlag(Keys.Shift)) select = !select;
                    BindingSource bs = gridMovies.DataSource as BindingSource;
                    bs.SuspendBinding();

                    List<DataRowView> rows = new List<DataRowView>();
                    for (int i = Math.Max(dragStartRow, hit.RowIndex); i >= Math.Min(dragStartRow, hit.RowIndex); i--)
                        if (i < gridMovies.Rows.Count)
                            rows.Add((DataRowView)bs[i]);

                    foreach (DataRowView row in rows)
                    {
                        ((MovieInfo)row[0]).selected = select;
                        row[(int)AppField.Selected] = select;
                    }
                    bs.ResumeBinding();
                    gridMovies.ClearSelection();
                    updateSelectedCount();
                }
            }
        }

        // if mouse leaves the datagrid, cancel any mouse-drag in progress. This event doesn't seem to fire during a drag...
        private void gridMovies_MouseLeave(object sender, EventArgs e)
        {
            dragStartRow = -1;
            dragSelect = false;
            dragStart = Point.Empty;
        }

        // Handle SPACE keypress to select movies
        private void gridMovies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && gridMovies.Focused && gridMovies.CurrentCell != null)
            {
                BindingSource bs = gridMovies.DataSource as BindingSource;
                int row = gridMovies.CurrentCell.RowIndex;
                int count = gridMovies.RowCount;
                if (row + 1 < count)
                    gridMovies.CurrentCell = gridMovies.Rows[row + 1].Cells[gridMovies.CurrentCell.ColumnIndex];
                else if (chkShowSelected.Checked)
                {
                    gridMovies.ClearSelection();
                    bs.SuspendBinding();
                }
                bool selected = (bool)gridMovies.Rows[row].Cells[(int)AppField.Selected].Value;
                ((MovieInfo)gridMovies.Rows[row].Cells[0].Value).selected = !selected;
                gridMovies.Rows[row].Cells[(int)AppField.Selected].Value = !selected;
                bs.ResumeBinding();
                updateSelectedCount();
                e.Handled = true;
                spacePressed = true;
            }
        }

        // handle SPACE key released
        private void gridMovies_KeyUp(object sender, KeyEventArgs e)
        {
            if (spacePressed)
            {
                e.Handled = true;
                spacePressed = false;
            }
        }

        // if doing mouse-drag-unselect (right mouse button), cancel the Context Menu
        private void gridMenu_Opening(object sender, CancelEventArgs e)
        {
            if (dragSelect)
                e.Cancel = true;
            else
            {
                ContextMenuPosition = gridMovies.PointToClient(MousePosition);
                DataGridView.HitTestInfo hit = gridMovies.HitTest(ContextMenuPosition.X, ContextMenuPosition.Y);
                if (hit.RowIndex >= 0 && hit.ColumnIndex > 1 && hit.Type == DataGridViewHitTestType.Cell)
                {
                    gridMovies.CurrentCell = gridMovies[hit.ColumnIndex, hit.RowIndex];
                    menuPosters.Visible = Program.settings.PostersEnabled;
                    menuPosterSync.Enabled = menuPosterTransferIn.Enabled = menuPosterTransferOut.Enabled = Program.settings.SavePosterCommonFolder;
                    AppField field = (AppField)hit.ColumnIndex;
                    MovieInfo m = gridMovies.Rows[hit.RowIndex].Cells[0].Value as MovieInfo;
                    menuLockField.Text = LockedCells.isLocked(m.JRKey, field) ? "Unlock field" : "Lock field";
                    menuLockField.Enabled = !m.isModified(field) && (field >= AppField.Title && field != AppField.File && field != AppField.Playlists && gridMovies.Columns[hit.ColumnIndex].ReadOnly);
                    menuPaste.Enabled = !gridMovies.Columns[hit.ColumnIndex].ReadOnly || field == AppField.Playlists;
                    menuRevertField.Enabled = field >= AppField.FTitle;
                    menuRevertThisColumn.Enabled = field >= AppField.FTitle;
                    menuOpenImdb.Enabled = m?.IMDBid != null;
                    menuOpenTmdb.Enabled = m?.tmdbInfo != null && m.tmdbInfo.id > 0;
                    menuOpenTrailer.Enabled = m?[AppField.Trailer] != null;
                    menuOpenPosterBrowser.Visible = Program.settings.PostersEnabled;
                }
                else
                    e.Cancel = true;
            }
        }

        #endregion

        #region IMDB shortcuts

        private void menuShortcutFilename_Click(object sender, EventArgs e)
        {
            createShortcuts(1);
        }

        private void menuShortcutTitle_Click(object sender, EventArgs e)
        {
            createShortcuts(2);
        }

        private void menuShortcutID_Click(object sender, EventArgs e)
        {
            createShortcuts(3);
        }

        private void createShortcuts(int operation)
        {
            var selected = GetSelectedMovies(true);
            if (selected.Count > 0)
            {
                var bar = new ProgressUI("Creating IMDB shortcuts", createShortcutsTask, selected);
                bar.progress.totalItems = selected.Count;
                bar.progress.useAltTitle = chkUseJRTitle.Checked;
                bar.progress.opcode = operation;

                bar.ShowDialog();   // ignore result
                SetStatus($"{bar.progress.success} shortcuts created/updated");

                if (bar.progress.success > 0) Analytics.Event("Menu", "CreateImdbShortcuts", "ShortcutsCreated", bar.progress.success);
            }
            else SetStatus("No movies selected");
        }

        private void createShortcutsTask(ProgressInfo progress)
        {
            progress.result = false;
            List<MovieInfo> movies = progress.args as List<MovieInfo>;
            if (movies == null) return;

            int i = 0;
            foreach (var movie in movies)
            {
                if (progress.cancelled)
                    return;

                while (progress.paused)
                    Thread.Sleep(250);

                string imdb = movie[AppField.IMDbID];
                string title = progress.useAltTitle ? movie.Title : movie.FTitle;
                string year = progress.useAltTitle ? movie.Year : movie.FYear;
                string file = Path.GetFileNameWithoutExtension(movie[AppField.File]);
                string dir = Path.GetDirectoryName(movie[AppField.File]);

                progress.currentItem = ++i;
                if (string.IsNullOrEmpty(imdb)) continue;
                progress.subtitle = title;
                progress.Update(false);

                string imdbfile = null;
                switch (progress.opcode)
                {
                    case 1: imdbfile = $"{file}.url"; break;
                    case 2: imdbfile = $"{title} [{year}].url"; break;
                    case 3: imdbfile = $"IMDb_{imdb}.url"; break;
                }
                try
                {
                    string text = $"[InternetShortcut]\r\nURL=https://www.imdb.com/title/{imdb}/\r\n";
                    File.WriteAllText(Path.Combine(dir, imdbfile), text);
                    progress.success++;
                }
                catch
                {
                    progress.fail++;
                }
            }
            progress.result = true;
        }

        #endregion

        #region Collections

        private void MCRatingsUI_DragEnter(object sender, DragEventArgs e)
        {
            string[] files = (string[])(e.Data.GetData(DataFormats.FileDrop, false));
            if (files.Length > 0 && Path.GetExtension(files[0]).ToLower().StartsWith(".htm"))
                e.Effect = DragDropEffects.Copy;
        }

        private void MCRatingsUI_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])(e.Data.GetData(DataFormats.FileDrop, false));
            if (files.Length == 0) return;

            if (!Program.settings.Collections)
            {
                MessageBox.Show("Please enable 'Collections' in Settings.xml");
                return;
            }

            string html = File.ReadAllText(files[0]);
            string title = null;
            var m = Regex.Match(html, @"<title>(.+?)</title>");
            if (m.Success)
                title = HttpUtility.HtmlDecode(m.Groups[1].Value.Trim());

            m = Regex.Match(html, @"coverViewJsonData\[ 0 \] = ({.+});\n");
            if (m.Success)
            {
                string json = m.Groups[1].Value;
                PtpCollection col = PtpCollection.Parse(json, title);
                if (col != null)
                {
                    ParseCollection(col);
                    return;
                }
            }
            MessageBox.Show("Invalid collection file - could not parse JSON data");
        }

        private void ParseCollection(PtpCollection collection)
        {
            DateTime start = DateTime.Now;

            int count = collection.Movies.Count;
            var col = collection.Movies.ToDictionary(c=>c.ImdbId ?? $"{c.Title}|{c.Year}", c=>c);

            BindingSource bs = gridMovies.DataSource as BindingSource;
            DataTable dt = bs?.DataSource as DataTable;

            int repeated = 0;
            int added = 0;
            int created = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    MovieInfo m = row[(int)AppField.Movie] as MovieInfo;
                    string imdb = m[AppField.IMDbID]?.Replace("tt", "");
                    string ftitle = string.IsNullOrEmpty(m[AppField.FTitle]) ? null : $"{m[AppField.FTitle]}|{m[AppField.FYear]}";
                    string jrtitle = string.IsNullOrEmpty(m[AppField.Title]) ? null : $"{m[AppField.Title]}|{m[AppField.Year]}";

                    if (!string.IsNullOrEmpty(imdb) && col.TryGetValue(imdb, out PtpCollectionMovie dup)
                        || (ftitle != null && col.TryGetValue(ftitle, out dup))
                        || (jrtitle != null && col.TryGetValue(jrtitle, out dup)))
                    {
                        dup.tag = true;
                        m.selected = true;
                        var curr = m[AppField.Collections].Split(';').Select(c => c.ToLower().Trim()).ToList();
                        if (curr.Contains(collection.Title.ToLower()))
                            repeated++;
                        else
                        {
                            if (overwriteField(m, AppField.Collections, collection.Title, chkOverwrite.Checked))
                                m[AppField.Status] = "updated";
                            added++;
                        }
                    }

                }

                DateTime dtNow = DateTime.Now;
                string now = dtNow.ToString("yyyy-MM-dd HH:mm:ss");

                var newMovies = col.Select(c => c.Value).Where(c => c.tag == false).ToList();
                foreach (var m in newMovies)
                {
                    Dictionary<AppField, string> fields = new Dictionary<AppField, string>();
                    MovieInfo mov = new MovieInfo(-1, fields, null);
                    mov[AppField.Status] = "NEW";
                    mov[AppField.FTitle] = m.Title == null ? null : HttpUtility.HtmlDecode(m.Title);
                    mov[AppField.FYear] = m.Year;
                    mov.TakeSnapshot();

                    mov[AppField.Title] = mov[AppField.FTitle];
                    mov[AppField.Year] = m.Year;
                    mov[AppField.Collections] = $"{collection.Title}; MISSING";
                    mov[AppField.Imported] = now;
                    mov[AppField.IMDbID] = string.IsNullOrEmpty(m.ImdbId) ? null : $"tt{m.ImdbId}";
                    mov.selected = true;
                    movies.Add(mov);

                    // add movie row
                    object[] values = new object[dt.Columns.Count];
                    values[(int)AppField.Movie] = mov;
                    values[(int)AppField.Selected] = mov.selected;
                    foreach (AppField c in Enum.GetValues(typeof(AppField)))
                        if ((int)c > 1) values[(int)c] = mov[c];     // skip first 2

                    dt.Rows.Add(values);
                    created++;
                }
            }

            UpdateDataGrid(start);
            updateModifiedCount();
            updateSelectedCount();
            SetStatus($"Imported collection '{collection.Title}': {count} movies, {added + repeated} matches, {created} new");
        }

        #endregion

        #region posters

        private void ShowPosterToolTip(MovieInfo m, Image original, string lbl1, Image updated, string lbl2, string path2, Size size2, int row, int col)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { ShowPosterToolTip(m, original, lbl1, updated, lbl2, path2, size2, row, col); });
                return;
            }

            if (original == null && path2 == null)
                imgTooltip.Hide();
            else
            {
                var point = gridMovies.PointToClient(MousePosition);
                var hit = gridMovies.HitTest(point.X, point.Y);
                if (hit.RowIndex != row || hit.ColumnIndex != col)
                    return;

                Rectangle rect = gridMovies.GetCellDisplayRectangle(col, row, false);
                point = gridMovies.PointToScreen(rect.Location);
                var location = new Point(point.X + gridMovies.Columns[col].Width / 2, point.Y);
                imgTooltip.Tag = m;
                imgTooltip.Show(location, original, lbl1, updated, size2, lbl2, path2);
            }
        }

        private void OnImageTooltipClicked(object sender, EventArgs args)
        {
            MovieInfo m = imgTooltip.Tag as MovieInfo;
            if (m != null)
                ShowPictureBrowser(m, true);
        }

        private void PosterBrowser_OnPosterSelected(object sender, EventArgs e)
        {
            MovieInfo movie = posterBrowser.currMovie;
            if (posterBrowser.selectedPoster != null)
            {
                DateTime start = DateTime.Now;
                string res = $"{posterBrowser.selectedPoster.width} x {posterBrowser.selectedPoster.height}";
                if (overwriteField(movie, AppField.Poster, res, chkOverwrite.Checked, forced: true))
                {
                    movie.newPoster = posterBrowser.selectedPoster;
                    movie.lockPoster = posterBrowser.selectAndLock;
                }

                if (DownloadPoster(movie))
                    Analytics.Event("Image", "PosterDownload", "PosterDownloads", 1);
                if (DownloadThumbnail(movie))
                    Analytics.Event("Image", "ThumbDownload", "ThumbnailDownloads", 1);

                UpdateDataGrid(start);
                gridMovies.Refresh();
                updateModifiedCount();
                this.BringToFront();
            }
        }

        private void ShowPictureBrowser(MovieInfo m, bool bringtoFront, bool reload=false)
        {
            if (!Program.settings.PostersEnabled) return;

            if (posterBrowser == null || posterBrowser.IsDisposed)
                posterBrowser = new PosterBrowser();

            if (!posterBrowser.Visible) reload = true;

            // get current JRiver thumbnail
            string currPoster = jrAPI.GetThumbnail(m, false);
            string currRes = currPoster == null ? "no poster" : m.originalValue(AppField.Poster)?.Replace(" ", "") ?? "no poster";
            var img = Util.LoadImage(currPoster);
            posterBrowser.ShowMovie(m, img, currRes, reload);
            if (bringtoFront)
                posterBrowser.BringToFront();
        }

        // queue thumbnail and poster download in background
        private bool DownloadPoster(MovieInfo m)
        {
            var poster = m.newPoster;
            if (poster != null)
            {
                // poster
                string url = TMDbAPI.GetPosterUrl(m.newPoster?.file_path, PosterSize.Original, out string path);
                return (url != null && downloader.QueueDownload(url, path, m));
            }
            return false;
        }

        private bool DownloadThumbnail(MovieInfo m)
        {
            var poster = m.newPoster;
            if (poster != null)
            {
                // thumbnail
                PosterSize size = Program.settings.ShowSmallThumbnails ? PosterSize.Medium : PosterSize.Large;
                string url = TMDbAPI.GetPosterUrl(m.newPoster?.file_path, size, out string path);
                return (url != null && downloader.QueueDownload(url, path, m, priority: true));
            }
            return false;
        }

        // queue thumbnail and poster download in background
        private int DownloadActors(MovieInfo movie)
        {
            if (movie?.tmdbInfo == null) return 0;
            int items = Program.settings.ListItemsLimit;
            if (items <= 0) items = 100;

            var cast = movie.tmdbInfo.getCast(items);
            cast.AddRange(movie.tmdbInfo.getCrew(items));

            PosterSize size = (PosterSize)Program.settings.ActorThumbnailSize;

            int count = 0;
            foreach (var c in cast)
            {
                string url = TMDbAPI.GetCastUrl(c.profile_path, size);
                if (string.IsNullOrEmpty(url)) continue;

                string file = Util.SanitizeFilename(c.name) + Path.GetExtension(url);
                file = Path.Combine(Program.settings.ActorFolder, file);
                string pngFile = Program.settings.ActorSaveAsPng ? Path.ChangeExtension(file, ".png") : file;

                if (!File.Exists(pngFile))
                    if (downloader.QueueDownload(url, file, movie, c, convertToPng: Program.settings.ActorSaveAsPng))
                        count++;
            }
            return count;
        }

        // download and copy poster file to movie/common folder
        private string SavePoster(MovieInfo movie)
        {
            try
            {
                // get poster
                string url = TMDbAPI.GetPosterUrl(movie.newPoster.file_path, PosterSize.Original, out string path);
                if (url != null && downloader.DownloadWait(url, path, movie))
                {
                    string dest = null;
                    string ext = Path.GetExtension(path);
                    // save to movie folder
                    if (Program.settings.SavePosterMovieFolder || !Program.settings.SavePosterCommonFolder)
                    {
                        dest = Path.ChangeExtension(movie[AppField.File], ext);
                        File.Copy(path, dest, true);
                        dest = Path.GetFileName(dest);  // just the filename needed for JRiver
                    }
                    // save to common folder
                    if (Program.settings.SavePosterCommonFolder && !string.IsNullOrWhiteSpace(Program.settings.PosterFolder))
                    {
                        string filename = Path.GetFileNameWithoutExtension(movie[AppField.File]);
                        dest = Path.Combine(Program.settings.PosterFolder, $"{filename}.{movie.JRKey}{ext}");
                        Directory.CreateDirectory(Program.settings.PosterFolder);
                        File.Copy(path, dest, true);
                        FileInfo fi = new FileInfo(dest);
                        fi.LastWriteTime = DateTime.Now;
                    }
                    return dest;
                }
            }
            catch (Exception ex) { Logger.Log(ex, "SavePoster"); }
            return null;
        }

        private void MCRatingsUI_MouseLeave(object sender, EventArgs e)
        {
            imgTooltip.Hide();
        }

        private void gridMovies_SelectionChanged(object sender, EventArgs e)
        {
            if (!Program.settings.PostersEnabled || posterBrowser == null || !posterBrowser.Visible) return;
            if (!inEvent && gridMovies.SelectedRows.Count == 1)
            {
                var row = gridMovies.SelectedRows[0];
                MovieInfo m = row.Cells[0].Value as MovieInfo;
                if (m != null)
                    ShowPictureBrowser(m, false);
            }
        }

        private void menuRebuildThumbs_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Yeah, that would be nice.\nUnfortunately, JRiver doesn't have an API to do that, " +
                "so when we change the Poster the thumbnails are not regenerated.\n\n" +
                "Please use the 'Rebuild Thumbnail' menu in JRiver directly.", "No way Jose",
                MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void menuPosterTransferIn_Click(object sender, EventArgs e)
        {
            PosterTransfer(true);
        }

        private void menuPosterTransferOut_Click(object sender, EventArgs e)
        {
            PosterTransfer(false);
        }

        private void PosterTransfer(bool toCommon)
        {
            if (!Program.settings.SavePosterCommonFolder || string.IsNullOrWhiteSpace(Program.settings.PosterFolder))
                return;

            Cursor = Cursors.WaitCursor;
            int ok = 0;
            int err = 0;
            int skip = 0;
            var movies = GetSelectedMovies();
            foreach (var m in movies)
            {
                if (m.currPosterPath == null)
                    err++;
                else if (toCommon == m.currPosterPath.StartsWith(Program.settings.PosterFolder, StringComparison.InvariantCultureIgnoreCase))
                    skip++;
                else
                {
                    if (syncPoster(m, out string newPath) && jrAPI.SetPosterPath(m, newPath))
                        ok++;
                    else
                        err++;
                }
            }
            SetStatus($"{ok} poster(s) transferred{(skip > 0 ? $", {skip} already there" : "")}{(err > 0 ? $", {err} copy error(s)" : "")}");
            Cursor = Cursors.Default;
        }

        private bool syncPoster(MovieInfo m, out string newPath)
        {
            newPath = null;
            if (m.JRKey < 0 || m.currPosterPath == null) return false;

            try
            {
                string common = Program.settings.PosterFolder;
                string curr = m.currPosterPath;
                string ext = Path.GetExtension(curr);
                string filename = Path.GetFileNameWithoutExtension(m[AppField.File]);
                string posterInMovie = Path.ChangeExtension(m[AppField.File], ext);
                string posterInCommon = Path.Combine(common, $"{filename}.{m.JRKey}{ext}");

                newPath = posterInCommon;
                if (curr.StartsWith(common, StringComparison.InvariantCultureIgnoreCase))
                    newPath = posterInMovie;
                else
                    Directory.CreateDirectory(common);

                File.Copy(curr, newPath, true);
                return true;
            }
            catch { }
            return false;
        }

        private void menuPosterSync_Click(object sender, EventArgs e)
        {
            if (!Program.settings.SavePosterCommonFolder || string.IsNullOrWhiteSpace(Program.settings.PosterFolder))
                return;

            Cursor = Cursors.WaitCursor;
            var movies = GetSelectedMovies();
            int ok = 0;
            int err = 0;
            foreach (var m in movies)
            {
                if (syncPoster(m, out string newPoster))
                    ok++;
               else
                    err++;
            }
            SetStatus($"{ok} poster(s) synced{(err > 0 ? $", {err} copy error(s)" : "")}");
            Cursor = Cursors.Default;
        }

        private void menuRemovePoster_Click(object sender, EventArgs e)
        {
            var movies = GetSelectedMovies();
            DateTime start = DateTime.Now;
            foreach (var m in movies)
            {
                if (overwriteField(m, AppField.Poster, null, chkOverwrite.Checked, true, true))
                {
                    m.newPoster = null;
                    m.newPosterPath = null;
                }
            }
            UpdateDataGrid(start);
            gridMovies.Refresh();
            updateModifiedCount();
        }

        #endregion

        private void menuOpenPosterBrowser_Click(object sender, EventArgs e)
        {
            MovieInfo currmovie = gridMovies.CurrentRow?.Cells[0].Value as MovieInfo;
            ShowPictureBrowser(currmovie, true);
        }

        private void menuOpenFolder_Click(object sender, EventArgs e)
        {
            MovieInfo currmovie = gridMovies.CurrentRow?.Cells[0].Value as MovieInfo;
            try
            {
                if (currmovie != null)
                    Process.Start(Path.GetDirectoryName(currmovie[AppField.File]));
            }
            catch { }
        }

        private void menuOpenImdb_Click(object sender, EventArgs e)
        {
            MovieInfo currmovie = gridMovies.CurrentRow?.Cells[0].Value as MovieInfo;
            try
            {
                if (currmovie?.IMDBid != null && currmovie.IMDBid.ToLower().StartsWith("tt"))
                    Process.Start($"https://www.imdb.com/title/{currmovie.IMDBid.ToLower()}");
            }
            catch { }
        }

        private void menuOpenTmdb_Click(object sender, EventArgs e)
        {
            MovieInfo currmovie = gridMovies.CurrentRow?.Cells[0].Value as MovieInfo;
            try
            {
                if (currmovie?.tmdbInfo != null && currmovie.tmdbInfo.id > 0)
                    Process.Start($"https://www.themoviedb.org/movie/{currmovie.tmdbInfo.id}");
            }
            catch { }
        }

        private void menuOpenTrailer_Click(object sender, EventArgs e)
        {
            MovieInfo currmovie = gridMovies.CurrentRow?.Cells[0].Value as MovieInfo;
            try
            {
                string url = currmovie?[AppField.Trailer];
                if (url != null && url.ToLower().StartsWith("http"))
                    Process.Start(url);
            }
            catch { }
        }

        private void menuOpenStatistics_Click(object sender, EventArgs e)
        {
            new StatsUI().ShowDialog();
        }
    }
}

