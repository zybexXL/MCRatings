using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class About : Form
    {
        // Base64 encoded to avoid AV false positives: https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=59W3EUXWQ4BZS&source=url
        string donationPaypal => Util.FromBase64("bHJ1PWVjcnVvcyZTWkI0UVdYVUUzVzk1PWRpX25vdHR1Yl9kZXRzb2gma2NpbGN4LXNfPWRtYz9yY3NiZXcvbmliLWlnYy9tb2MubGFweWFwLnd3dy8vOnNwdHRo", true);
        string email => Util.FromBase64("bW9jLmxpYW1nQGFjZXNub2ZicDpvdGxpYW0=", true);      // mailto:pbfonseca@gmail.com";

        public About()
        {
            InitializeComponent();
            linkLabel5.Text = Constants.https + linkLabel5.Text;
            //linkLabel7.Tag = Constants.https + linkLabel7.Tag;
            //linkLabel9.Tag = Constants.https + linkLabel9.Tag;
        }

        private void About_Load(object sender, EventArgs e)
        {
            lblVersion.Text = $"Version {Program.version.ToString(3)}";
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
                Util.ShellStart(donationPaypal);
            }
            catch { }
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string url = ((Control)sender).Tag as string;
                if (!string.IsNullOrEmpty(url))
                    Util.ShellStart(Constants.https + url);
            }
            catch { }
        }

        private void lblContact_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.ShellStart($"{email}?subject=ZRatings%20v{Program.version}");
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
            if (AutoUpgrade.CheckUpgrade(noQuestions: AutoUpgrade.hasUpgrade) && AutoUpgrade.restartNeeded)
            {
                MessageBox.Show("ZRatings upgraded! Press OK to launch the new version.", "ZRatings Upgraded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            SetUpgradeLabel();
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            new StatsUI().ShowDialog();
        }
    }
}
