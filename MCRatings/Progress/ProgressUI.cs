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
    // Runs a given task as a BackgroundWorker while displaying a progress bar
    public partial class ProgressUI : Form
    {
        public ProgressInfo progress;
        BackgroundWorker worker;
        Action<ProgressInfo> bgAction;

        public ProgressUI(string title, Action<ProgressInfo> action, object args = null, bool canCancel = true)
        {
            InitializeComponent();
            bgAction = action;
            this.progress = new ProgressInfo(title) { args = args, canCancel = canCancel };
            init();
        }

        public void init()
        {
            progress.RefreshHandler = this.RefreshInfo;
            
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += WorkerWork;
            worker.ProgressChanged += WorkerProgress;
            worker.RunWorkerCompleted += WorkerCompleted;

            this.DialogResult = DialogResult.None;
        }

        private void Progress_Load(object sender, EventArgs e)
        {
            RefreshInfo();
            RefreshETA();
            etaTimer.Start();
        }

        private void ProgressUI_Shown(object sender, EventArgs e)
        {
            worker?.RunWorkerAsync();
        }

        private void etaTimer_Tick(object sender, EventArgs e)
        {
            RefreshETA();
        }

        private void WorkerWork(object sender, DoWorkEventArgs e)
        {
            bgAction?.Invoke(progress);
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = progress.cancelled ? DialogResult.Cancel : DialogResult.OK;
            this.Close();
        }

        private void WorkerProgress(object sender, ProgressChangedEventArgs e)
        {
            RefreshInfo();
        }

        public void RefreshInfo()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { RefreshInfo(); });
                return;
            }
            if (!this.IsHandleCreated || this.IsDisposed) return;
            if (progress == null) return;

            btnCancel.Visible = progress.canCancel;
            lblTitle.Text = progress.title;
            lblSubtitle.Text = progress.subtitle;
            lblCount.Visible = lblETA.Visible = progress.totalItems > 0;

            if (lblCount.Visible)
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                int percent = (int)(100.0 * progress.currentItem / progress.totalItems);
                progressBar1.Value = percent > 100 ? 100 : percent;

                lblCount.Text = $"{progress.currentItem}/{progress.totalItems}";
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
            }
        }

        public void RefreshETA()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { RefreshETA(); });
                return;
            }

            if (lblCount.Visible)
            {
                if (progress.currentItem < 10) lblETA.Text = "--:--";
                else
                {
                    double total = (DateTime.Now - progress.startTime).TotalSeconds * progress.totalItems / progress.currentItem;
                    TimeSpan eta = (progress.startTime.AddSeconds(total) - DateTime.Now);
                    if (eta < TimeSpan.Zero) eta = TimeSpan.Zero;
                    if (eta.Days > 30) lblETA.Text = $"never";
                    else if (eta.Days > 0) lblETA.Text = $"{eta.TotalDays:0.#} d";
                    else if (eta.Hours > 0) lblETA.Text = eta.ToString(@"hh\hmm");
                    else lblETA.Text = eta.ToString(@"mm\:ss");
                }
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            progress.paused = true;
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to cancel this operation?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Stop))
            {
                worker.CancelAsync();
                progress.cancelled = true;
                progress.paused = false;
                this.Close();
            }
            progress.paused = false;
        }

        private void Progress_FormClosing(object sender, FormClosingEventArgs e)
        {
            etaTimer.Stop();
            if (worker != null && worker.IsBusy)
                worker?.CancelAsync();
        }
    }
}
