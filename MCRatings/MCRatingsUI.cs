using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{

    public partial class MCRatingsUI : Form
    {
        OMDbAPI omdbAPI = new OMDbAPI(Program.settings.APIkeyList);
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
        Point LastMouseClick;
        private SoundPlayer Player = new SoundPlayer();

        public MCRatingsUI()
        {
            InitializeComponent();
            gridMovies.DoubleBuffered(true);

            this.comboLists.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboLists.DrawItem += drawCombobox;
            comboLists.DisplayMember = "Name";

            this.Text = $"MCRatings v{Program.version} - {Program.tagline}";
        }

        #region Form and Button events

        private void Form1_Load(object sender, EventArgs e)
        {
            // fill current monitor (with border)
            this.Width = Screen.FromControl(this).Bounds.Width - 200;
            this.Height = Screen.FromControl(this).Bounds.Height - 100;

            this.Left = 100;
            this.Top = 50;

            // load playlists
            if (!LoadPlaylists(true))
                this.Close();
            loading = false;

            ApplyColors();

            // check for upgrade in background task
            Task.Run(() =>
            {
                if (AutoUpgrade.CheckUpgrade(checkOnly: true))
                    SetStatus($"MCRatings v{AutoUpgrade.LatestVersion.version} is available! Click there to update  \u27a4 \u27a4 \u27a4", true);
            });
        }

        private void MCRatingsUI_Shown(object sender, EventArgs e)
        {
            // check field map, show SettingsUI if needed
            bool showSettings = !Program.settings.valid;
            if (!showSettings)
                foreach (var map in Program.settings.FieldMap.Values)
                    if (map.enabled && (string.IsNullOrWhiteSpace(map.JRfield) || !jrAPI.Fields.ContainsKey(map.JRfield.ToLower())))
                        showSettings = true;

            // show SettingsUI if there are no valid settings (first time) or if field mapping has problems
            if (!Program.settings.valid || showSettings)
                if (new SettingsUI(jrAPI, true).ShowDialog() != DialogResult.OK)
                    if (!Program.settings.valid)
                        this.Close();

            comboLists.Focus();
        }

        private void MCRatingsUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSave.Enabled)
                if (DialogResult.Cancel == MessageBox.Show("You have unsaved changes.\nAre you sure you want to exit?", "Discard changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                    e.Cancel = true;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
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
            else
                e.Handled = false;
        }

        // capture CTRL+C on a datagridView cell in normal or Edit mode
        // CTRL+C is not captured by KeyPress/Down events, needs to be on this global event
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
                if (gridMovies.CurrentCell != null && (gridMovies.Focused || gridMovies.IsCurrentCellInEditMode))
                {
                    string value = gridMovies.CurrentCell.Value.ToString();
                    if (gridMovies.CurrentCell.IsInEditMode && gridMovies.CurrentCell.EditType == typeof(DataGridViewTextBoxEditingControl))
                    {
                        var textbox = gridMovies.EditingControl as DataGridViewTextBoxEditingControl;
                        if (textbox != null && !string.IsNullOrEmpty(textbox.SelectedText))
                            value = textbox.SelectedText;
                    }
                    if (!string.IsNullOrEmpty(value))
                        Clipboard.SetText(value);
                    return true;
                }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        #endregion

        #region Settings and Colors

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (new SettingsUI(jrAPI).ShowDialog() == DialogResult.OK)
                omdbAPI = new OMDbAPI(Program.settings.APIkeyList);
        }

        private void menuColorGuide_Click(object sender, EventArgs e)
        {
            if (new ColorEditor().ShowDialog() == DialogResult.OK)
                ApplyColors();
        }

        void ApplyColors()
        {
            gridMovies.DefaultCellStyle.BackColor = getColor(CellColor.Default);
            gridMovies.DefaultCellStyle.SelectionBackColor = getColor(CellColor.ActiveRow);
            if (gridMovies.Columns.Count > 0)
            {
                gridMovies.Columns[(int)AppField.IMDbID].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.FTitle].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
                gridMovies.Columns[(int)AppField.FYear].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
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
    
        #endregion

        #region Status Labels

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

        #endregion

        #region JRiver connect

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            string currList = comboLists.Text;
            if (LoadPlaylists())
                comboLists.Text = currList;
            comboLists.Focus();
        }

        private bool LoadPlaylists(bool startup = false)
        {
            var progressUI = new ProgressUI("Connecting to JRiver...", ConnectJRiver, null, false);
            if (startup)
                progressUI.StartPosition = FormStartPosition.CenterScreen;

            progressUI.ShowDialog();

            if (!jrAPI.Connected)
            {
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

            comboLists.DataSource = jrAPI.Playlists;
            var allfiles = jrAPI.Playlists.Where(l => l.Name == "All Files (empty search)").FirstOrDefault();
            if (allfiles != null)
                comboLists.SelectedItem = allfiles;
            else
                comboLists.SelectedItem = jrAPI.Playlists.FirstOrDefault();

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
            jrAPI.getPlaylists();

            progress.subtitle = "Reading fields";
            progress.Update();
            jrAPI.getFields();

            progress.result = true;
        }

        #endregion

        #region PlayList ComboBox

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

        #endregion

        #region PlayList Load

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

            updateModifiedCount();
            updateSelectedCount();
        }

        private void LoadJRiverMovies(ProgressInfo progress)
        {
            progress.result = false;
            JRiverPlaylist playlist = progress.args as JRiverPlaylist;
            if (playlist == null) return;

            List<MovieInfo> newMovies = new List<MovieInfo>();
            progress.totalItems = playlist.Filecount;
            progress.Update();

            int i = 0;
            foreach (var movie in jrAPI.getMovies(playlist))    // iterator
            {
                if (progress.cancelled)
                    return;

                progress.currentItem = ++i;
                if (movie != null)
                {
                    newMovies.Add(movie);
                    progress.subtitle = movie.Title;
                    progress.success++;
                }
                else
                    progress.fail++;

                progress.Update(false);

                while (progress.paused)
                    Thread.Sleep(250);
            }
            movies = newMovies;
            progress.result = true;
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
                }
            }
            gridMovies.Refresh();
        }

        private void PopulateDataGrid(bool updateStatus = true)
        {
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
                dt.Rows.Add(values);
            }

            // apply datasource
            BindingSource bs = new BindingSource();
            bs.DataSource = dt;
            gridMovies.DataSource = bs;

            // fix column headers, set read only, hide disabled columns
            gridMovies.Columns[1].HeaderText = "";
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

            // column colors
            gridMovies.Columns[(int)AppField.IMDbID].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.FTitle].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.FYear].DefaultCellStyle.BackColor = getColor(CellColor.ColumnEdit);
            gridMovies.Columns[(int)AppField.Status].DefaultCellStyle.BackColor = Color.Gainsboro;

            gridMovies.Columns[(int)AppField.Selected].Frozen = true;

            // adjust column order
            gridMovies.Columns[(int)AppField.IMDbID].DisplayIndex = 3;
            gridMovies.Columns[(int)AppField.Imported].DisplayIndex = 8;

            if (gridMovies.Rows.Count > 0)
                gridMovies.CurrentCell = gridMovies.Rows[0].Cells[1];

            gridMovies.Refresh();
            gridMovies.Focus();
        }

        #endregion

        #region OMDB fetch

        private void btnGetMovieInfo_Click(object sender, EventArgs e)
        {
            bool noCache = ModifierKeys.HasFlag(Keys.Shift);

            var selected = movies.Where(m => m.selected).ToList();
            if (selected.Count == 0 && gridMovies.CurrentRow != null)
            {
                MovieInfo currmovie = (MovieInfo)gridMovies.CurrentRow.Cells[0].Value as MovieInfo;
                if (currmovie != null)
                    selected = new List<MovieInfo> { currmovie };
            }

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
            }

            var bar = new ProgressUI("Getting movie information", GetMovieInfo, selected);
            bar.progress.totalItems = selected.Count;
            bar.progress.canOverwrite = chkOverwrite.Checked;
            bar.progress.useAltTitle = chkUseJRTitle.Checked;
            bar.progress.noCache = noCache;

            bar.ShowDialog();   // ignore result - even if cancelled, the already updated movies are kept
            
            string msg = $"{bar.progress.success} updated";
            if (bar.progress.fail > 0) msg += $", {bar.progress.fail} failed";
            if (bar.progress.skip > 0) msg += $", {bar.progress.skip} skipped";

            SetStatus(msg);
            UpdateDataGrid(bar.progress.startTime);
            
            updateModifiedCount();
            updateSelectedCount();

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

            int i = 0;
            foreach (var movie in movies)
            {
                if (progress.cancelled)
                    return;

                while (progress.paused)
                    Thread.Sleep(250);

                bool FindByName = string.IsNullOrEmpty(movie[AppField.IMDbID]);
                string title = progress.useAltTitle ? movie.Title : movie.FTitle;
                string year = progress.useAltTitle ? movie.Year : movie.FYear;
                progress.currentItem = ++i;
                progress.subtitle = title;
                progress.Update(false);

                string omdb;
                if (FindByName)
                    omdb = omdbAPI?.getByTitle(title, year);
                else
                    omdb = omdbAPI?.getByIMDB(movie[AppField.IMDbID], noCache: progress.noCache);

                if (omdbAPI.lastResponse == 401)    // unauthorized, keys expended
                {
                    progress.skip = movies.Count - progress.success - progress.fail;
                    progress.result = true;
                    return;
                }

                var info = OMDbInfo.Parse(omdb);
                if (info != null && info.valid)
                {
                    progress.success++;
                    movie.clearUpdates();
                    bool ok = overwriteField(movie, AppField.IMDbID, info.IMDBid, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.IMDbRating, info.ImdbScore, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.IMDbVotes, info.ImdbVotes, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.RottenTomatoes, info.RottenScore, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Metascore, info.MetaScore, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.MPAARating, info.MPAARating, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Runtime, info.Runtime, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Genre, info.Genre, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Director, info.Director, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Writers, info.Writer, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Actors, info.Actors, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Description, info.Plot, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Language, info.Language, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Country, info.Country, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Awards, info.Awards, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Revenue, info.BoxOffice, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Production, info.Production, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Website, info.Website, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Release, info.Released, progress.canOverwrite);

                    ok |= overwriteField(movie, AppField.Title, info.title, progress.canOverwrite);
                    ok |= overwriteField(movie, AppField.Year, info.year, progress.canOverwrite);

                    movie[AppField.Status] = ok && movie.isDirty ? "updated" : "no change";
                    movie.selected = false;
                }
                else
                {
                    movie[AppField.Status] = "not found";
                    progress.fail++;
                }
            }
            progress.result = true;
        }

        // overwrites a cell if overwrite flags are enabled for the field
        // first restores original cell value to prevent stale info from previous GET operations to remain behind
        private bool overwriteField(MovieInfo movie, AppField field, string value, bool masterOverwrite)
        {
            bool fieldOverwrite = Program.settings.FieldMap[field].overwrite;
            bool fieldEnabled = Program.settings.FieldMap[field].enabled;

            if (fieldEnabled && field != AppField.IMDbID)
                movie[field] = movie.originalValue(field);    // restore original value 
            if (string.IsNullOrEmpty(value)) return false;

            movie.setUpdate(field, value);

            if (fieldEnabled && value != movie[field] && (string.IsNullOrEmpty(movie[field]) || (fieldOverwrite && masterOverwrite)))
            {
                movie[field] = value;
                return true;
            }
            return false;
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

            var bar = new ProgressUI("Saving changes", SaveChanges, changed);
            bar.progress.totalItems = changed.Count;
            bar.ShowDialog();

            string msg = $"{bar.progress.success} saved";
            if (bar.progress.fail > 0) msg += $", {bar.progress.fail} failed";
            if (bar.progress.skip > 0) msg += $", {bar.progress.skip} skipped";

            SetStatus(msg);
            UpdateDataGrid(bar.progress.startTime);
            updateModifiedCount();
            if (bar.progress.fail > 0)
                MessageBox.Show($"Error saving changed movies to JRiver!\n{bar.progress.fail} movies still have unsaved changes.",
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

                progress.currentItem = ++i;
                progress.subtitle = progress.useAltTitle ? movie.Title : movie.FTitle;
                progress.Update(false);

                if (jrAPI.SaveMovie(movie))
                {
                    movie.clearUpdates();
                    movie[AppField.Status] = "saved";
                    progress.success++;
                }
                else
                {
                    progress.fail++;
                    movie[AppField.Status] = "save failed";
                }
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
            SelectRows(new Predicate<MovieInfo>(m => m.isDirty));
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

            if (chkFilter.Checked && !string.IsNullOrEmpty(txtSearch.Text))
                filter = $"[Filter] like '%{txtSearch.Text}%'";
            if (chkShowSelected.Checked)
                if (filter == "") filter = "[Selected] = True";
                else filter += " and [Selected] = True";

            bs.RemoveFilter();
            bs.Filter = filter;

            gridMovies.ClearSelection();
            gridMovies.Refresh();
            txtSearch.ForeColor = bs.Count == 0 ? Color.Red : Color.Black;
        }

        private void chkShowSelected_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilter();
            gridMovies.Focus();
        }

        #endregion

        #region Context Menu
        
        // selects/unselects rows based on predicate
        private void SelectRows(Predicate<MovieInfo> predicate)
        {
            gridMovies.ClearSelection();
            List<DataRowView> changed = new List<DataRowView>();
            BindingSource bs = gridMovies.DataSource as BindingSource;
            if (bs == null) return;

            bs.SuspendBinding();
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
                ApplyFilter();
            gridMovies.ClearSelection();
            bs.ResumeBinding();
            gridMovies.Refresh();
            updateSelectedCount();
        }

        private void menuSelectAll_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => true));
        }

        private void menuClearSelection_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => false));
        }

        private void menuToggleSelection_Click(object sender, EventArgs e)
        {
            SelectRows(new Predicate<MovieInfo>(m => !m.selected));
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

        private void menuRevertField_Click(object sender, EventArgs e)
        {
            DataGridView.HitTestInfo hit = gridMovies.HitTest(LastMouseClick.X, LastMouseClick.Y);
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
            DataGridView.HitTestInfo hit = gridMovies.HitTest(LastMouseClick.X, LastMouseClick.Y);
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
            DataGridView.HitTestInfo hit = gridMovies.HitTest(LastMouseClick.X, LastMouseClick.Y);
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
            for (int i = 0; i < gridMovies.RowCount; i++)
            {
                var row = gridMovies.Rows[i];
                MovieInfo m = row.Cells[0].Value as MovieInfo;
                m.restoreSnapshot();
                foreach (AppField f in Enum.GetValues(typeof(AppField)))
                    if (f >= AppField.Status)
                        row.Cells[(int)f].Value = m.originalValue(f);
            }
            gridMovies.Refresh();
            updateModifiedCount();
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
                    gridMovies.CurrentCell = gridMovies[2, e.RowIndex];
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

            if (!m.isDirty && m[AppField.Status] == "updated")
            {
                m[AppField.Status] = null;
                row.Cells[(int)AppField.Status].Value = null;
            }

            bool hasStatus = m[AppField.Status] != null;
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
            {
                if (f >= AppField.Title)
                {
                    int mod = m.isModified(f, row.Cells[(int)f].Value as string);
                    if (mod > 0)
                    {
                        row.Cells[(int)f].Style.BackColor = mod == 1 ? getColor(CellColor.Overwrite) : getColor(CellColor.NewValue);
                        row.Cells[(int)f].Style.ForeColor = Color.Empty;   // inherit
                    }
                    else if (mod == 0 && hasStatus && m.isUpdated(f))
                    {
                        row.Cells[(int)f].Style.ForeColor = Color.Green;
                        row.Cells[(int)f].Style.BackColor = Color.Empty;   // inherit
                    }
                    else
                        row.Cells[(int)f].Style = null;
                }
            }

            switch (m[AppField.Status])
            {
                case "updated": row.Cells[(int)AppField.Status].Style.ForeColor = Color.DarkGreen; break;
                case "saved": row.Cells[(int)AppField.Status].Style.ForeColor = Color.DarkGreen; break;
                case "save failed": row.Cells[(int)AppField.Status].Style.ForeColor = Color.Red; break;
                case "not found": row.Cells[(int)AppField.Status].Style.ForeColor = Color.Red; break;
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
                if (ModifierKeys.HasFlag(Keys.Shift) && lastClickedRow >= 0)
                {
                    BindingSource bs = gridMovies.DataSource as BindingSource;
                    bs.SuspendBinding();
                    for (int i = Math.Min(lastClickedRow, e.RowIndex); i <= Math.Max(lastClickedRow, e.RowIndex); i++)
                    {
                        ((MovieInfo)gridMovies.Rows[i].Cells[0].Value).selected = true;
                        gridMovies.Rows[i].Cells[(int)AppField.Selected].Value = true;
                    }
                    bs.ResumeBinding();
                    gridMovies.ClearSelection();
                    updateSelectedCount();
                    gridMovies.Refresh();
                }
                lastClickedRow = e.RowIndex;    // row becomes the last clicked

                // handle CTRL+click on IMDB link
                if (e.ColumnIndex == (int)AppField.IMDbID && ModifierKeys.HasFlag(Keys.Control))
                {
                    string id = gridMovies.CurrentCell.EditedFormattedValue as string;
                    if (id != null && id.ToLower().StartsWith("tt"))
                        Process.Start($"https://www.imdb.com/title/{id.ToLower()}/");
                }
                // handle CTRL+click on File path
                else if (e.ColumnIndex == (int)AppField.File && ModifierKeys.HasFlag(Keys.Control))
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
                    Clipboard.SetText(text);
            }
        }

        // Cell End edit => update underlying MovieInfo object, update modified count
        private void grid2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == (int)AppField.IMDbID ||
                e.ColumnIndex == (int)AppField.FTitle ||
                e.ColumnIndex == (int)AppField.FYear) && e.RowIndex >= 0)
            {
                MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                string txt = gridMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                m[(AppField)e.ColumnIndex] = txt;
            }
            if (e.ColumnIndex == (int)AppField.IMDbID)
                updateModifiedCount();
        }

        // ToolTip needed - get cell contents, wrap text if too long; append [Previous Value] when value changed
        private void grid2_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            e.ToolTipText = "";
            if (e.ColumnIndex > 2 && e.RowIndex >= 0)
            {
                MovieInfo m = gridMovies.Rows[e.RowIndex].Cells[0].Value as MovieInfo;
                string txt = gridMovies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                int eq = m.isModified((AppField)e.ColumnIndex, txt);
                if (eq != 0)
                {
                    string prev = m.originalValue((AppField)e.ColumnIndex);
                    if (!string.IsNullOrEmpty(prev))
                    {
                        //string sep = prev != null && prev.Length > 80 ? "\r\n" : "";
                        txt += $"\n\n[Previous Value:]\n{prev}";
                    }
                }
                e.ToolTipText = txt.WordWrap(100);
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
            LastMouseClick = e.Location;    // for context menu
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
        }

        #endregion
    }
}
