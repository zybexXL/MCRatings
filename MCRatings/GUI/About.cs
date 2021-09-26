using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class About : Form
    {
        string donationPaypal = Constants.https + "www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=59W3EUXWQ4BZS&source=url";
        const string email = "pbfonseca@gmail.com";

        public About()
        {
            InitializeComponent();
            linkLabel5.Text = Constants.https + linkLabel5.Text;
            linkLabel7.Text = Constants.https + linkLabel7.Text;
            linkLabel9.Text = Constants.https + linkLabel9.Text;
        }

        private void About_Load(object sender, EventArgs e)
        {
            lblVersion.Text = $"Version {Program.version}";
#if DEBUG
            lblDebug.Visible = true;
#endif
            SetUpgradeLabel();

            Analytics.Event("GUI", "About");
        }

        private void MouseDown_Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Native.MouseDragCapture(Handle);
        }

        private void SetUpgradeLabel()
        {
            if (AutoUpgrade.hasUpgrade)
            {
                btnUpdate.Text = $"Update to v{AutoUpgrade.LatestVersion.version}";
                btnUpdate.ForeColor = Color.Red;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void picPaypal_Click(object sender, EventArgs e)
        {
            try
            {
                Analytics.Event("GUI", "Donation");
                Process.Start(donationPaypal);
            }
            catch { }
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

        private void lblContact_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = $"mailto:{email}?subject=ZRatings%20v{Program.version.ToString()}";
                Process.Start(url);
            }
            catch { }
        }

        private void About_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            AutoUpgrade.CheckUpgrade(noQuestions: AutoUpgrade.hasUpgrade);
            SetUpgradeLabel();
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            new StatsUI().ShowDialog();
        }
    }
}
