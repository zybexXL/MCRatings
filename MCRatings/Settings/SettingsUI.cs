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

        public SettingsUI(JRiverAPI jrAPI, bool startDirty = false)
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
            jr = jrAPI;
            gridFields.DoubleBuffered(true);
            ShowSettings(Program.settings);
            dirty = startDirty;
            // mute icon hidden until a sound is played
            btnAudio.Visible = Directory.Exists(Constants.AudioCache) && Directory.GetFiles(Constants.AudioCache).Length > 0;  
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            //int height = gridFields.ColumnHeadersHeight + 2;
            //height += gridFields.Rows.Count * gridFields.Rows[0].Height;    // grid height
            //height = this.Height - gridFields.Height + height;       // required form heigh
            //height = Math.Min(height, Screen.FromControl(this).Bounds.Height - 100);
            //this.Height = height;
            //this.Top = (Screen.FromControl(this).Bounds.Height - height) / 2;
            badFields = checkFieldNames(Program.settings.valid);
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
                string value = row.Cells[1].Value?.ToString().Trim();
                bool overwrite = (bool)row.Cells[2].Value;
                bool enabled = (bool)row.Cells[3].Value;
                row.Cells[1].Style = null;
                if (enabled && string.IsNullOrEmpty(value))
                {
                    row.Cells[1].Style.ForeColor = Color.Red;
                    if (show) MessageBox.Show($"Please enter a valid field name for {Constants.ViewColumnInfo[field].JRField}",
                        "Empty field", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ok = false;
                    //show = false;
                }
                else if (enabled && !jr.Fields.ContainsKey(value.ToLower()))
                {
                    row.Cells[1].Style.ForeColor = Color.Red;
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
                    string value = row.Cells[1].Value?.ToString().Trim();
                    bool overwrite = (bool)row.Cells[2].Value;
                    bool enabled = (bool)row.Cells[3].Value;
                    Program.settings.FieldMap[field] = new JRFieldMap(field, value, enabled, overwrite);
                }
                Program.settings.Silent = !audio;
                Program.settings.FastStart = chkFastStart.Checked;
                Program.settings.WebmediaURLs = chkWebmedia.Checked;
                Program.settings.FileCleanup = txtCleanup.Text?.Trim();
                Program.settings.APIKeys = txtAPIKeys.Text?.Trim();
                Program.settings.TMDbAPIKeys = txtTMDBkeys.Text?.Trim();
                Program.settings.PreferredSource = optTMDb.Checked ? 1 : 2;
                Program.settings.ListItemsLimit = (int)maxListLimit.Value;
                Program.settings.Language = string.IsNullOrWhiteSpace(txtLanguage.Text) ? "EN" : txtLanguage.Text;
                Program.settings.Save();
                dirty = false;
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ShowSettings(Settings settings)
        {
            gridFields.Rows.Clear();
            foreach (AppField f in Enum.GetValues(typeof(AppField)))
            {
                if (!settings.Collections && f == AppField.Collections)
                    continue;
                if (Constants.ViewColumnInfo[f].isJRField)
                    addRow(settings, f);
            }
            txtCleanup.Text = settings.FileCleanup;
            txtAPIKeys.Text = settings.APIKeys;
            txtTMDBkeys.Text = settings.TMDbAPIKeys;
            chkFastStart.Checked = settings.FastStart;
            chkWebmedia.Checked = settings.WebmediaURLs;
            if (settings.PreferredSource == 2) optOMDb.Checked = true;
            else optTMDb.Checked = true;
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
                string jrName = settings.FieldMap[field].JRfield;
                // hack: replace tag for some fields - this should be in Constants map, not here...
                if (field == AppField.Title) name = "Title";
                if (field == AppField.Writers) name = "Writers";
                if (field == AppField.Revenue) name = "Revenue";
                if (field == AppField.Release) name = "Release Date";
                bool enabled = settings.FieldMap[field].enabled;
                bool overwrite = settings.FieldMap[field].overwrite;
                row = gridFields.Rows.Add(name, jrName, overwrite, enabled);
                
            }
            else
                row = gridFields.Rows.Add(name, name, false, false);

            gridFields.Rows[row].Tag = field;
        }

        private void gridFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            dirty = true;
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
                Settings settings = new Settings();
                settings.APIKeys = txtAPIKeys.Text;
                settings.TMDbAPIKeys = txtTMDBkeys.Text;
                settings.FileCleanup = txtCleanup.Text;
                ShowSettings(settings);
                dirty = true;
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

        private void gridFields_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex > 1)
                dirty = true;
        }

        private void SettingsUI_Shown(object sender, EventArgs e)
        {
            if (!Program.settings.valid)
            {
                MessageBox.Show("Please review the JRiver fields to be updated by MCRatings.\n" +
                    "Fields in red don't exist in JRiver - you need to create them (type=String), or specify an alternative field.\n" +
                    "If you don't want a field to be updated, you can disable it.\n\n" +
                    "To get OMDb information, you need to register for an API key using the provided link.", "Welcome to MCRatings!",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                tabSettings.SelectedTab = tabFields;
            }
        }

        private void btnAudio_Click(object sender, EventArgs e)
        {
            dirty = true;
            audio = !audio;
            btnAudio.Image = audio ? Properties.Resources.speaker_on : Properties.Resources.speaker_off;
        }

        private void somethingChanged(object sender, EventArgs e)
        {
            dirty = true;
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
    }
}
