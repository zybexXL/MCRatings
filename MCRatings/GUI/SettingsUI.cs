using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class SettingsUI : Form
    {
        bool dirty = false;
        bool audio = true;
        bool badFields = false;
        bool loading = false;
        Dictionary<AppField, int> rowIndex = new Dictionary<AppField, int>();

        JRiverAPI jr;
        List<Control> labels;
        
        public SettingsUI(JRiverAPI jrAPI, bool startDirty = false)
        {
            loading = true;
            InitializeComponent();

            labels = new List<Control> { lblColor1, lblColor2, lblColor3, lblColor4, lblColor5,
                lblColor6, lblColor7, lblColor8, lblColor9, lblColor10, lblColor11, lblColor12 };

            SourceSelectColumn dgSource = new SourceSelectColumn();
            dgSource.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.TopCenter;
            //srcCol.DefaultCellStyle = dataGridViewCellStyle3;
            dgSource.HeaderText = "Source";
            dgSource.Name = "dgSource";
            dgSource.ReadOnly = true;
            dgSource.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            dgSource.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            dgSource.Width = 128;
            gridFields.Columns.Remove(dgSourcePlaceHolder);
            gridFields.Columns.Add(dgSource);
            dgSource.DisplayIndex = 3;

            DialogResult = DialogResult.Cancel;
            jr = jrAPI;
            gridFields.DoubleBuffered(true);

            ShowSettings(Program.settings);
            setDirty(startDirty);

            // mute icon hidden until a sound is played
            btnAudio.Visible = false;
#if SOUNDFX
            btnAudio.Visible = Directory.Exists(Constants.AudioCache) && Directory.GetFiles(Constants.AudioCache).Length > 0;   
#endif
            loading = false;
        }

        private void MouseDown_Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Native.MouseDragCapture(Handle);
        }

        private void setDirty(bool value = true)
        {
            if (loading) return;
            dirty = value;
            UpdateUI();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
        }

        private void SettingsUI_Shown(object sender, EventArgs e)
        {
            Analytics.Event("GUI", "Settings");
            if (!checkFieldNames(false, true))
                badFields = !checkFieldNames(Program.settings.valid, false);
            if (!Program.settings.valid)
            {
                MessageBox.Show("Please review the JRiver fields to be updated by ZRatings.\n" +
                    "Fields in red don't exist in JRiver - you need to create them (type=String), or specify an alternative field.\n" +
                    "If you don't want a field to be updated, you can disable it.\n\n" +
                    "To get OMDb/TMDb information, you need to register for API keys using the provided links.", "Welcome to ZRatings!",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                tabSettings.SelectedTab = tabFields;
            }
            if (badFields)
                tabSettings.SelectedTab = tabFields;
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // validate mappings
        private bool checkFieldNames(bool show = true, bool create = true)
        {
            Cursor = Cursors.WaitCursor;
            jr?.getFields();     // refresh field list
            bool ok = true;
            List<string> missing = new List<string>();
            foreach (DataGridViewRow row in gridFields.Rows)
            {
                AppField field = (AppField)row.Tag;
                string value = row.Cells["dgField"].Value?.ToString().Trim();
                bool overwrite = (bool)row.Cells["dgOverwrite"].Value;
                bool enabled = (bool)row.Cells["dgEnabled"].Value;
                row.Cells["dgField"].Style = null;
                if (field != AppField.Poster && enabled && (string.IsNullOrEmpty(value) || !jr.Fields.ContainsKey(value.ToLower())))
                {
                    ok = false;
                    missing.Add(value);
                    row.Cells["dgField"].Style.ForeColor = Color.Red;
                }
            }
            Cursor = Cursors.Default;

            if (missing.Count > 0 && create)
            {
                if (DialogResult.Yes == MessageBox.Show($"The following fields do not exist in JRiver:\n    {string.Join("\n    ", missing)}\n\nDo You want to create them?",
                    "Missing fields", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    ok = jr.createFields(missing);
                else
                    ok = false;
            }

            if (!ok && show)
            { 
                tabSettings.SelectedTab = tabFields;
                MessageBox.Show($"One or more fields are undefined or do not exist in JRiver.\nPlease fix or disable the red fields.",
                    "Invalid fields", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ok;
        }

        private bool isValidFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                path = Macro.GetBaseFolder(path);
                if (string.IsNullOrEmpty(path)) return true; // can't check, path starts with $var
                Directory.CreateDirectory(path);
                string test = Path.Combine(path, "~zratings_test~.tmp");
                File.WriteAllText(test, "write permission test");
                File.Delete(test);
                return true;
            }
            catch { }
            return false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dirty)
            {
                if (!checkFieldNames())
                    return;

                // validate Poster folder path
                if (chkPosterSupport.Checked && chkPosterFolder.Checked && !isValidFolder(txtPosterPath.Text))
                {
                    tabSettings.SelectedTab = tabPosters;
                    MessageBox.Show($"Can't access selected Poster folder, please check.",
                     "Inaccessible Poster folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;   
                }

                // validate Actors folder path
                if (chkGetActorPics.Checked && !isValidFolder(txtActorPicsPath.Text))
                {
                    tabSettings.SelectedTab = tabPosters;
                    MessageBox.Show($"Can't access selected Actor Thumbnail folder, please check.",
                     "Inaccessible Actor Thumbnail folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // validate Media subtype list (file filter)
                if (string.IsNullOrWhiteSpace(txtFileFilter.Text))
                    txtFileFilter.Text = "[Media Sub Type]=Movie";

                // save mappings
                foreach (DataGridViewRow row in gridFields.Rows)
                {
                    AppField field = (AppField)row.Tag;
                    string value = row.Cells["dgField"].Value?.ToString().Trim();
                    bool overwrite = (bool)row.Cells["dgOverwrite"].Value;
                    bool enabled = (bool)row.Cells["dgEnabled"].Value;
                    Sources src = ((SourceSelect)row.Cells["dgSource"].Value).Value;
                    Program.settings.FieldMap[field] = new JRFieldMap(field, value, enabled, overwrite, src);
                }

                if (Program.settings.FieldMap[AppField.Description].source == Sources.OMDb && Program.settings.FieldMap[AppField.ShortPlot].enabled)
                {
                    Program.settings.FieldMap[AppField.Description].source = Sources.TMDb;
                    MessageBox.Show($"Description and Short Plot cannot both come from OMDb.\nDescription source was changed to TMDb.", "Source conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Program.settings.CellColors = GetColors();
                Program.settings.Silent = !audio;
                Program.settings.FastStart = chkFastStart.Checked;
                Program.settings.WebmediaURLs = chkWebmedia.Checked;
                Program.settings.StartMaximized = chkMaximized.Checked;
                Program.settings.ShowSmallThumbnails = chkSmallThumbs.Checked;
                Program.settings.FileCleanup = txtCleanup.Text?.Trim();
                Program.settings.APIKeys = txtAPIKeys.Text?.Trim();
                Program.settings.TMDbAPIKeys = txtTMDBkeys.Text?.Trim();
                Program.settings.ListItemsLimit = (int)maxListLimit.Value;
                Program.settings.Language = string.IsNullOrWhiteSpace(txtLanguage.Text) ? "EN" : txtLanguage.Text;
                Program.settings.PosterFolder = txtPosterPath.Text.Trim();
                Program.settings.PosterFilterLanguage = chkPosterFilterLanguage.Checked;
                Program.settings.PosterSortVotes = chkPosterSortVotes.Checked;
                Program.settings.SavePosterCommonFolder = chkPosterFolder.Checked;
                Program.settings.SavePosterMovieFolder = chkPosterMovieFolder.Checked;
                Program.settings.LoadFullSizePoster = chkFullSize.Checked;
                Program.settings.AddActorRoles = chkActorRoles.Checked;
                Program.settings.ActorThumbnailSize = comboActorSize.SelectedIndex;
                Program.settings.SaveActorThumbnails = chkGetActorPics.Checked;
                Program.settings.ActorFolder = txtActorPicsPath.Text.Trim();
                Program.settings.PosterScript = txtPosterScript.Text.Trim();
                Program.settings.ThumbnailScript = txtThumbScript.Text.Trim();
                Program.settings.FileFilter = txtFileFilter.Text.Trim();
                Program.settings.RunPosterScript = chkRunPosterPP.Checked;
                Program.settings.RunThumbnailScript = chkRunThumbPP.Checked;
                Program.settings.ActorSaveAsPng = chkSaveThumbPNG.Checked;
                Program.settings.SortIgnoreArticles = chkIgnoreArticles.Checked;
                Program.settings.ActorPlaceholders = chkThumbPlaceholder.Checked;

                Program.settings.Save();
                dirty = false;

                Analytics.Event("Settings", "SettingsSaved");
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ShowSettings(Settings settings, bool mapOnly = false)
        {
            loading = true;
            gridFields.Rows.Clear();
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
            {
                if (!settings.Collections && f == AppField.Collections)
                    continue;
                if (Constants.ViewColumnInfo[f].isJRField)
                    addRow(settings, f);
            }
            if (mapOnly) return;

            txtCleanup.Text = settings.FileCleanup;
            txtAPIKeys.Text = settings.APIKeys;
            txtTMDBkeys.Text = settings.TMDbAPIKeys;
            chkFastStart.Checked = settings.FastStart;
            chkMaximized.Checked = settings.StartMaximized;
            chkSmallThumbs.Checked = settings.ShowSmallThumbnails;
            chkWebmedia.Checked = settings.WebmediaURLs;
            maxListLimit.Value = settings.ListItemsLimit;
            txtLanguage.Text = settings.Language ?? "EN";
            chkPosterFolder.Checked = settings.SavePosterCommonFolder;
            chkPosterMovieFolder.Checked = settings.SavePosterMovieFolder;
            txtPosterPath.Text = settings.PosterFolder;
            chkPosterFilterLanguage.Checked = settings.PosterFilterLanguage;
            chkPosterSortVotes.Checked = settings.PosterSortVotes;
            chkFullSize.Checked = settings.LoadFullSizePoster;
            chkPosterSupport.Checked = (bool)gridFields.Rows[rowIndex[AppField.Poster]].Cells[0].Value;
            chkActorRoles.Checked = settings.AddActorRoles;
            comboActorSize.SelectedIndex = settings.ActorThumbnailSize;
            chkGetActorPics.Checked = settings.SaveActorThumbnails;
            txtActorPicsPath.Text = settings.ActorFolder;
            txtPosterScript.Text = settings.PosterScript;
            txtThumbScript.Text = settings.ThumbnailScript;
            txtFileFilter.Text = settings.FileFilter;
            chkRunPosterPP.Checked = settings.RunPosterScript;
            chkRunThumbPP.Checked = settings.RunThumbnailScript;
            chkSaveThumbPNG.Checked = settings.ActorSaveAsPng;
            chkIgnoreArticles.Checked = settings.SortIgnoreArticles;
            chkThumbPlaceholder.Checked = settings.ActorPlaceholders;

            audio = !settings.Silent;
            btnAudio.Image = audio ? Properties.Resources.speaker_on : Properties.Resources.speaker_off;

            LoadColors(settings.CellColors ?? Constants.CellColors);

            UpdateUI();
            loading = false;
        }

        private void UpdateUI()
        {
            btnSave.Text = dirty ? "SAVE" : "OK";
            txtPosterPath.Enabled = btnPosterFolder.Enabled = chkPosterMovieFolder.Enabled = chkPosterFolder.Checked;
            groupThumbs.Enabled = chkGetActorPics.Checked;
            groupPoster.Enabled = chkPosterSupport.Checked;
            txtPosterScript.Enabled = chkRunPosterPP.Checked;
            txtThumbScript.Enabled = chkRunThumbPP.Checked;
            gridFields[0, rowIndex[AppField.Poster]].Value = chkPosterSupport.Checked;
        }

        private void addRow(Settings settings, AppField field)
        {
            int row;
            string name = Constants.ViewColumnInfo[field].JRField;
            if (settings.FieldMap.ContainsKey(field))
            {
                JRFieldMap map = settings.FieldMap[field];
                string jrName = map.JRfield;
                // hack: replace tag for some fields - this should be in Constants map, not here...
                if (field == AppField.Title) name = "Title";
                if (field == AppField.Poster) name = "Poster";
                if (field == AppField.Writers) name = "Writers";
                if (field == AppField.Revenue) name = "Revenue";
                if (field == AppField.Release) name = "Release Date";
                if (field == AppField.Roles) name = "Actor Roles";
                bool enabled = map.enabled;
                bool overwrite = map.overwrite;

                Sources source = map.source;
                var sources = JRFieldMap.getSources(field);
                if (!sources.Contains(source)) source = sources.FirstOrDefault();
                SourceSelect ss = new SourceSelect(sources, source);
                row = gridFields.Rows.Add(enabled, name, jrName, overwrite, ss);
                rowIndex[field] = row;

                if (field == AppField.Poster)
                {
                    gridFields.Rows[row].Cells["dgField"].ReadOnly = true;
                    // TODO: this doesn't work for some reason
                    gridFields.Rows[row].Cells["dgField"].Style.ForeColor = Color.Purple;
                }
            }
            else
                row = gridFields.Rows.Add(false, name, name, false, new SourceSelect(null, Sources.None));

            gridFields.Rows[row].Tag = field;
        }

        private void gridFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loading) return;
            AppField field = (AppField)gridFields.Rows[e.RowIndex].Tag;
            if (e.ColumnIndex == 0 && field == AppField.Poster)
                chkPosterSupport.Checked = (bool)gridFields.Rows[e.RowIndex].Cells[0].Value;
            setDirty();
        }

        private void SettingsUI_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                e.Handled = true;
                btnDiscard_Click(null, EventArgs.Empty);
            }
        }

        private void lblReset_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("Reset field mapping to default?", "Reset mapping",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings settings = Settings.DefaultSettings();
                ShowSettings(settings, true);
                setDirty();
            }
        }

        private void SettingsUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Program.settings.valid && !dirty)   // already saved or no changes
                return;

            if (!Program.settings.valid)
            {
                if (DialogResult.No == MessageBox.Show("You need to complete first time configuration.\nCancelling will exit ZRatings.\n\nAre you sure you want to CANCEL?",
                    "Invalid settings", MessageBoxButtons.YesNo, MessageBoxIcon.Stop))
                    e.Cancel = true;
            }
            else if (badFields)
            {
                if (DialogResult.No == MessageBox.Show("Current field mapping has errors. Are you sure you want to close without fixing it?",
                      "Invalid settings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    e.Cancel = true;
                    tabSettings.SelectedTab = tabFields;
                }
            }
            else if (dirty && MessageBox.Show("Close without saving?", "Discard settings",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        }

        private void btnAudio_Click(object sender, EventArgs e)
        {
            setDirty();
            audio = !audio;
            btnAudio.Image = audio ? Properties.Resources.speaker_on : Properties.Resources.speaker_off;
        }

        private void somethingChanged(object sender, EventArgs e)
        {
            setDirty();
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = ((Control)sender).Tag as string;
                if (!string.IsNullOrEmpty(url))
                    Process.Start(Constants.https + url);
            }
            catch { }
        }

        void LoadColors(uint[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
                if (labels[i].Tag == null)
                    labels[i].BackColor = Color.FromArgb((int)colors[i]);
                else
                    labels[i].ForeColor = Color.FromArgb((int)colors[i]);

            lblColor10.BackColor = lblColor11.BackColor = lblColor12.BackColor = lblColor1.BackColor;
        }

        private void lnkResetColors_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadColors(Constants.CellColors);
            setDirty();
        }

        private int ColorDistance(Color color1, Color color2)
        {
            int dist = (Math.Abs(color1.R - color2.R) + Math.Abs(color1.G - color2.G) + Math.Abs(color1.B - color2.B)) / 3;
            return dist;
        }

        private void lblColor_click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            colorDialog1.Color = lbl.Tag == null ? lbl.BackColor : lbl.ForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (ColorDistance(Color.Pink, colorDialog1.Color) < 40 || ColorDistance(Color.LightPink, colorDialog1.Color) < 40 ||
                    ColorDistance(Color.HotPink, colorDialog1.Color) < 40 || ColorDistance(Color.DeepPink, colorDialog1.Color) < 40)
                    if (DialogResult.No == MessageBox.Show($"Pink? Really??!", "Silly color chosen", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        return;

                setDirty();
                if (lbl.Tag == null)
                    lbl.BackColor = colorDialog1.Color;
                else
                    lbl.ForeColor = colorDialog1.Color;
            }
            lblColor10.BackColor = lblColor11.BackColor = lblColor12.BackColor = lblColor1.BackColor;
        }

        uint[] GetColors()
        {
            uint[] colors = new uint[labels.Count];
            for (int i = 0; i < labels.Count; i++)
                colors[i] = labels[i].Tag == null ? (uint)labels[i].BackColor.ToArgb() : (uint)labels[i].ForeColor.ToArgb();
            return colors;
        }

        private void btnPosterFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFolderEx())
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    if (sender as Button == btnPosterFolder)
                        txtPosterPath.Text = dialog.SelectedFolder;
                    else
                        txtActorPicsPath.Text = dialog.SelectedFolder;
        }

        private void comboActorSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13)
                e.Handled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tagMenuItem_Click(object sender, EventArgs e)
        {
            bool shift = ModifierKeys.HasFlag(Keys.Shift);
            var item = sender as ToolStripMenuItem;
            ContextMenuStrip menu = (sender as ToolStripMenuItem)?.Owner as ContextMenuStrip;
            TextBox box = menu.SourceControl as TextBox;
            bool addSpace = "path" != box.Tag as string;
            string tag = item?.Tag as string;

            if (box == null || tag == null) return;
            if (shift) tag = tag.Replace("$", "%");
            box.SelectedText = tag + (addSpace ? " " : "");
        }

        private void scriptTagMenu_Opening(object sender, CancelEventArgs e)
        {
            bool isActors = ((sender as ContextMenuStrip)?.SourceControl as TextBox) == txtThumbScript;
            tagMenuCharacter.Visible = tagMenuDepartment.Visible = tagMenuJob.Visible = tagMenuName.Visible = isActors;
        }
    }
}
