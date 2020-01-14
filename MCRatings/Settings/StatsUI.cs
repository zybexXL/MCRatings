using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    public partial class StatsUI : Form
    {
        public StatsUI()
        {
            InitializeComponent();
        }

        private void StatsUI_Load(object sender, EventArgs e)
        {
            LoadStats();
        }

        void LoadStats()
        {
            Stats current = Stats.Session;
            Stats total = Stats.Total;

            dgStats.Rows.Clear();
            dgStats.Rows.Add("MCRatings Start Date", current.StartDate.ToString(), total.StartDate.ToString());
            dgStats.Rows.Add("MCRatings Runtime", TimeSpan.FromSeconds(current.AppRuntime).ToString(), TimeSpan.FromSeconds(total.AppRuntime).ToString());
            dgStats.Rows.Add("MCRatings Executions", current.AppRuns, total.AppRuns);

            dgStats.Rows.Add("OMDb Get by ID", current.OMDbGet, total.OMDbGet);
            dgStats.Rows.Add("OMDb Get by Title", current.OMDbSearch, total.OMDbSearch);
            dgStats.Rows.Add("OMDb API Calls", current.OMDbAPICall, total.OMDbAPICall);
            dgStats.Rows.Add("OMDb API Title Not Found", current.OMDbAPINotFound, total.OMDbAPINotFound);
            dgStats.Rows.Add("OMDb API Errors", current.OMDbAPIError, total.OMDbAPIError);

            dgStats.Rows.Add("TMDb Get by ID", current.TMDbGet, total.TMDbGet);
            dgStats.Rows.Add("TMDb Get by Title", current.TMDbSearch, total.TMDbSearch);
            dgStats.Rows.Add("TMDb API Calls", current.TMDbAPICall, total.TMDbAPICall);
            dgStats.Rows.Add("TMDb API Title Not Found", current.TMDbAPINotFound, total.TMDbAPINotFound);
            dgStats.Rows.Add("TMDb API Errors", current.TMDbAPIError, total.TMDbAPIError);

            dgStats.Rows.Add("Cache Hits", current.CacheHit, total.CacheHit);
            dgStats.Rows.Add("Cache Misses", current.CacheMiss, total.CacheMiss);
            dgStats.Rows.Add("Cache Expired", current.CacheExpired, total.CacheExpired);
            dgStats.Rows.Add("Cache Writes", current.CacheAdd, total.CacheAdd);

            if (Program.settings.Collections)
                dgStats.Rows.Add("JRiver Movies Created", current.JRMovieCreate, total.JRMovieCreate);
            dgStats.Rows.Add("JRiver Movies Updated", current.JRMovieUpdate, total.JRMovieUpdate);
            dgStats.Rows.Add("JRiver Fields Updated", current.JRFieldUpdate, total.JRFieldUpdate);
            dgStats.Rows.Add("JRiver API Errors", current.JRError, total.JRError);
            
            for (int i = 3; i <= 7; i++) dgStats.Rows[i].DefaultCellStyle.ForeColor = Color.Blue;
            for (int i = 8; i <= 12; i++) dgStats.Rows[i].DefaultCellStyle.ForeColor = Color.Green;
            for (int i = 13; i <= 16; i++) dgStats.Rows[i].DefaultCellStyle.ForeColor = Color.DarkMagenta;
            for (int i = 17; i < dgStats.Rows.Count; i++) dgStats.Rows[i].DefaultCellStyle.ForeColor = Color.OrangeRed;

            int height = dgStats.ColumnHeadersHeight + 2;
            height += dgStats.Rows.Count * dgStats.Rows[0].Height;    // grid height
            height = this.Height - dgStats.Height + height;       // required form heigh
            height = Math.Min(height, Screen.FromControl(this).Bounds.Height - 100);
            this.Height = height;
            this.Top = (Screen.FromControl(this).Bounds.Height - height) / 2;
        }

        private void StatsUI_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void lnkResetAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("This will reset ALL current and cumulative (total) statistics!\n\nAre you sure you want to reset everything?", "Reset counters", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                Stats.Reset(true);
                LoadStats();
            }
        }

        private void btnResetCurrent_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to reset current statistics?", "Reset counters", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                Stats.Reset(false);
                LoadStats();
            }
        }

        private void StatsUI_Shown(object sender, EventArgs e)
        {
            dgStats.CurrentCell = null;
        }


    }
}
