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

namespace MCRatings
{
    public partial class SettingsUI : Form
    {
        bool dirty = false;
        bool audio = true;
        bool badFields = false;

        JRiverAPI jr;
        List<Control> labels;

        public SettingsUI(JRiverAPI jrAPI, bool startDirty = false)
        {
            InitializeComponent();

            labels = new List<Control> { lblColor1, lblColor2, lblColor3, lblColor4, lblColor5,
                lblColor6, lblColor7, lblColor8, lblColor9, lblColor10, lblColor11 };

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
            btnAudio.Visible = Directory.Exists(Constants.AudioCache) && Directory.GetFiles(Constants.AudioCache).Length > 0;  
        }

        private void setDirty(bool value = true)
        {
            dirty = value;
            btnSave.Text = dirty ? "SAVE" : "OK";
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            //int height = gridFields.ColumnHeadersHeight + 2;
            //height += gridFields.Rows.Count * gridFields.Rows[0].Height;    // grid height
            //height = this.Height - gridFields.Height + height;       // required form heigh
            //height = Math.Min(height, Screen.FromControl(this).Bounds.Height - 100);
            //this.Height = height;
            //this.Top = (Screen.FromControl(this).Bounds.Height - height) / 2;
            badFields = !checkFieldNames(Program.settings.valid);
            LoadColors(Program.settings.CellColors ?? Constants.CellColors);
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // validate mappings
        private bool checkFieldNames(bool show = true)
        {
            jr?.getFields();     // refresh field list
            bool ok = true;
            foreach (DataGridViewRow row in gridFields.Rows)
            {
                AppField field = (AppField)row.Tag;
                string value = row.Cells["dgField"].Value?.ToString().Trim();
                bool overwrite = (bool)row.Cells["dgOverwrite"].Value;
                bool enabled = (bool)row.Cells["dgEnabled"].Value;
                row.Cells["dgField"].Style = null;
                if (enabled && string.IsNullOrEmpty(value))
                {
                    row.Cells["dgField"].Style.ForeColor = Color.Red;
                    if (show) MessageBox.Show($"Please enter a valid field name for {Constants.ViewColumnInfo[field].JRField}",
                        "Empty field", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ok = false;
                    //show = false;
                }
                else if (enabled && !jr.Fields.ContainsKey(value.ToLower()))
                {
                    row.Cells["dgField"].Style.ForeColor = Color.Red;
                    if (show) MessageBox.Show($"Field '{value}' doesn't exist in JRiver, please fix or disable the field.",
                        $"Invalid '{Constants.ViewColumnInfo[field].JRField}' field name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ok = false;
                    //show = false;
                }
            }
            return ok;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dirty)
            {
                if (!checkFieldNames())
                    return;

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
                Program.settings.CellColors = GetColors();
                Program.settings.Silent = !audio;
                Program.settings.FastStart = chkFastStart.Checked;
                Program.settings.WebmediaURLs = chkWebmedia.Checked;
                Program.settings.FileCleanup = txtCleanup.Text?.Trim();
                Program.settings.APIKeys = txtAPIKeys.Text?.Trim();
                Program.settings.TMDbAPIKeys = txtTMDBkeys.Text?.Trim();
                Program.settings.ListItemsLimit = (int)maxListLimit.Value;
                Program.settings.Language = string.IsNullOrWhiteSpace(txtLanguage.Text) ? "EN" : txtLanguage.Text;
                Program.settings.Save();
                dirty = false;
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ShowSettings(Settings settings, bool mapOnly = false)
        {
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
            chkWebmedia.Checked = settings.WebmediaURLs;
            maxListLimit.Value = settings.ListItemsLimit;
            txtLanguage.Text = settings.Language ?? "EN";
            audio = !settings.Silent;
            btnAudio.Image = audio ? Properties.Resources.speaker_on : Properties.Resources.speaker_off;
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
                if (field == AppField.Writers) name = "Writers";
                if (field == AppField.Revenue) name = "Revenue";
                if (field == AppField.Release) name = "Release Date";
                bool enabled = map.enabled;
                bool overwrite = map.overwrite;

                Sources source = map.source;
                var sources = JRFieldMap.getSources(field);
                if (!sources.Contains(source)) source = sources.FirstOrDefault();
                SourceSelect ss = new SourceSelect(sources, source);
                row = gridFields.Rows.Add(enabled, name, jrName, overwrite, ss);
            }
            else
                row = gridFields.Rows.Add(false, name, name, false, new SourceSelect(null, Sources.None));

            gridFields.Rows[row].Tag = field;
        }

        private void gridFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            setDirty();
        }

        private void SettingsUI_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
                btnDiscard_Click(null, EventArgs.Empty);
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(((LinkLabel)sender).Tag.ToString());
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
                if (DialogResult.No == MessageBox.Show("You need to complete first time configuration.\nCancelling will exit MCRatings.\n\nAre you sure you want to CANCEL?",
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

        private void gridFields_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex >=0 && e.ColumnIndex == dgSource.Index)
            //{
            //    AppField field = (AppField)gridFields.Rows[e.RowIndex].Tag;
            //    List<Sources> valid = JRFieldMap.getSources(field);
            //    if (valid.Count < 2) return;

            //    // toggle source
            //    if (Enum.TryParse((string)gridFields[e.ColumnIndex, e.RowIndex].Value, out Sources curr))
            //        curr = curr == Sources.TMDb ? Sources.OMDb : Sources.TMDb;
            //    else
            //        curr = Sources.TMDb;
            //    gridFields.Rows[e.RowIndex].Cells["dgSource"].Value = curr.ToString();
            //    gridFields.Rows[e.RowIndex].Cells["dgSource"].Style.ForeColor = curr == Sources.OMDb ? Color.Blue : Color.Green;
            //}
        }

        private void SettingsUI_Shown(object sender, EventArgs e)
        {
            if (!Program.settings.valid)
            {
                MessageBox.Show("Please review the JRiver fields to be updated by MCRatings.\n" +
                    "Fields in red don't exist in JRiver - you need to create them (type=String), or specify an alternative field.\n" +
                    "If you don't want a field to be updated, you can disable it.\n\n" +
                    "To get OMDb/TMDb information, you need to register for API keys using the provided links.", "Welcome to MCRatings!",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                tabSettings.SelectedTab = tabFields;
            }
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

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = ((Control)sender).Tag as string;
                if (!string.IsNullOrEmpty(url))
                    Process.Start(url);
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
            lblColor10.BackColor = lblColor11.BackColor = label1.BackColor;
        }

        private void lnkResetColors_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadColors(Constants.CellColors);
            setDirty();
        }

        private void lblColor_click(object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            colorDialog1.Color = lbl.Tag == null ? lbl.BackColor : lbl.ForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                setDirty();
                if (lbl.Tag == null)
                    lbl.BackColor = colorDialog1.Color;
                else
                    lbl.ForeColor = colorDialog1.Color;
            }
            lblColor10.BackColor = lblColor11.BackColor = label1.BackColor;
        }

        uint[] GetColors()
        {
            uint[] colors = new uint[labels.Count];
            for (int i = 0; i < labels.Count; i++)
                colors[i] = labels[i].Tag == null ? (uint)labels[i].BackColor.ToArgb() : (uint)labels[i].ForeColor.ToArgb();
            return colors;
        }
    }
}
