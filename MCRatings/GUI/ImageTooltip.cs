using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    public partial class ImageTooltip : Form
    {
        string loadingPath;
        bool loading;
        Point position;
        Size thumbSize;
        public event EventHandler Clicked;

        public ImageTooltip()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.logo.GetHicon());
        }

        // Updated Image might be null on this call, since it might still be downloading
        // in this case, a placeholder is shown - once it downloads, it will be loaded
        internal void Show(Point point, Image original, string oLabel, Image updated, Size uSize, string uLabel, string uPath)
        {
            if (this.IsDisposed) return;

            lock (this)
            {
                thumbSize = uSize;
                position = point;
                loading = (updated == null && uPath != null);
                loadingPath = loading ? uPath : null;
                if (loading)
                {
                    updated = Util.LoadImage(uPath);
                    loading = updated == null;
                    if (loading)
                        updated = Properties.Resources.LoadingSpinner;  // placeholder
                }
            }

            if (original == null && updated == null) Hide();
            else
            {
                Analytics.Event("GUI", "PosterTooltip", "PosterTooltipView", 1);

                this.Show();
                label2.Text = uLabel;
                label1.Text = oLabel;
                img2.Image = updated;
                img1.Image = original;
                img2.SizeMode = loading ? PictureBoxSizeMode.CenterImage : PictureBoxSizeMode.StretchImage;

                panel2.Visible = img2.Visible = label2.Visible = updated != null;
                panel1.Visible = img1.Visible = label1.Visible = original != null;

                // image sizes
                int maxHeight = original == null ? uSize.Height : original.Height;
                panel1.Height = original == null ? 400 : original.Height + label1.Height + 2;
                panel1.Width = original == null ? 200 : original.Width + 2;

                panel2.Height = maxHeight + label2.Height + 2;
                panel2.Width = loading || updated == null ? Program.settings.ShowSmallThumbnails ? 100 : 200
                    : uSize.Width * maxHeight / uSize.Height + 2;
                
                panel2.Left = original == null ? 0 : panel1.Right + 10;

                // form position
                int height = original == null ? panel2.Bottom + 1 : panel1.Bottom + 1;
                int maxY = Screen.FromControl(this).WorkingArea.Height;
                if (point.Y + 10 + height > maxY)
                    this.Top = maxY - 10 - height;
                else
                    this.Top = point.Y + 5;

                this.Left = point.X + 10;
                this.Height = height;
                this.Width = updated == null ? panel1.Right + 1 : panel2.Right + 1;

            }
        }

        public void OnImageDownloaded(object sender, DownloadItem item)
        {
            lock (this)
                if (!loading || !Visible || item.destPath != loadingPath)
                    return;

            this.BeginInvoke((MethodInvoker)delegate { Show(position, img1.Image, label1.Text, null, thumbSize, label2.Text, loadingPath); });
        }

        private void img1_MouseClick(object sender, MouseEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        private void ImageTooltip_MouseLeave(object sender, EventArgs e)
        {
            if (MousePosition.X < this.Left || MousePosition.X > this.Right || MousePosition.Y < this.Top || MousePosition.Y > this.Bottom)
                Hide();
        }
    }
}
