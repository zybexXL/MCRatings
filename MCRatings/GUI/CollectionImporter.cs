using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class CollectionImporter : Form
    {
        bool closing = false;
        public event EventHandler<PtpCollection> CollectionLoaded;

        bool isLoadingPages = false;
        int currentPage = 0;
        PtpCollection Collection = null;
        int loads = 0;

        public CollectionImporter()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.logo.GetHicon());
        }

        private void CollectionImporter_Load(object sender, EventArgs e)
        {
            Navigate($"{Constants.https}{Util.NoSpaces(txtURL.Tag as string)}", false);
        }

        private void CollectionImporter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        public void Exit()
        {
            closing = true;
            this.Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            string html = browser.DocumentText;
            PtpCollection col = PtpCollection.Parse(html);
            Import(col);
        }

        private void btnImportAll_Click(object sender, EventArgs e)
        {
            string html = browser.DocumentText;
            PtpCollection col = PtpCollection.Parse(html);

            if (col == null || !col.isMultipage)
                Import(col);
            else
                StartImportAll(col.Pages);
        }

        void StartImportAll(int pages)
        {
            btnCancel.Visible = true;
            btnImport.Enabled = btnImportAll.Enabled = btnReload.Enabled = btnHome.Enabled = btnBack.Enabled = txtURL.Enabled = false;
            currentPage = 1;
            Collection = null;
            isLoadingPages = true;
            LoadCollectionPage(currentPage, pages);
        }

        void CompleteImportAll(bool cancel = false)
        {
            isLoadingPages = false;
            currentPage = 0;

            if (!cancel && Collection != null)
                Import(Collection);

            Collection = null;
            btnCancel.Visible = false;
            btnImport.Enabled = btnImportAll.Enabled = btnReload.Enabled = btnHome.Enabled = btnBack.Enabled = txtURL.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CompleteImportAll(true);
            Navigate(txtURL.Text);
        }

        private void LoadCollectionPage(int page, int pages)
        {
            string url = browser.Url.ToString();
            if (!Regex.IsMatch(url, @"id=\d+"))
                lblStatus.Text = "Unexpected URL format";

            if (Regex.IsMatch(url, @"page=\d+"))
                url = Regex.Replace(url, @"page=\d+", $"page={page}");
            else
                url = url + $"&page={page}";

            lblStatus.Text = $"Loading collection page {page} of {pages}";
            Navigate(url, false);
        }

        private void Import(PtpCollection col)
        {
            if (col != null && col.isValid)
            {
                lblStatus.Text = $"Importing {col.Count} movies";
                Application.DoEvents();
                CollectionLoaded?.Invoke(this, col);
                lblStatus.Text = $"Import completed";
            }
            else
                lblStatus.Text = "This doesn't look like a valid Collection page!";
        }

        private void Navigate(string url, bool updateLabel = true)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (browser.IsBusy)
                browser.Stop();

            if (updateLabel) lblStatus.Text = "loading page...";
            browser.Navigate(url.Trim());
        }

        private void browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            txtURL.Text = e.Url.ToString();
            btnImport.Visible = btnImportAll.Visible = lblImport.Visible = false;
            progressBar.Visible = true;
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string html = browser.DocumentText;
            PtpCollection col = PtpCollection.Parse(html);

            bool isValid = col != null && col.isValid;

            if (isLoadingPages)
            {
                if (isValid)
                {
                    if (Collection == null) Collection = col;
                    else Collection.Movies.AddRange(col.Movies);
                    if (currentPage < col.Pages)
                        BeginInvoke((MethodInvoker) delegate { LoadCollectionPage(++currentPage, col.Pages); });
                    else
                        CompleteImportAll();
                }
                else
                {
                    CompleteImportAll(true);
                    lblStatus.Text = "Failed to load current collection page";
                }
            }

            if (!isLoadingPages)
            {
                btnImport.Text = isValid ? $"This page ({col.Movies.Count})" : "This page";
                btnImportAll.Text = isValid && col.Pages > 1 ? $"All {col.Pages} pages ({col.TotalCount})" : "All pages";

                progressBar.Visible = false;
                btnImport.Visible = lblImport.Visible = isValid;
                btnImportAll.Visible = isValid && col.Pages > 1;

                if (isValid) lblStatus.Text = $"{col.TotalCount} movies in Collection '{col.Title}' {(col.isMultipage ? $"({col.Pages} pages)" : "")}";
                else if (++loads > 1) lblStatus.Text = "";
            }
        }

        private void browser_NewWindow(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            Navigate(txtURL.Tag as string);
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Navigate(txtURL.Text);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (browser.CanGoBack)
                browser.GoBack();
        }

        private void txtURL_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                Navigate(txtURL.Text);
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (txtURL.Text != e.Url.ToString())
                Navigate(e.Url.ToString());
        }

        private void browser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            //if (isLoadingPages && browser.Document != null)
            //{
            //    try
            //    {
            //        dynamic htmldoc = browser.Document.DomDocument as dynamic;
            //        dynamic node = htmldoc.getElementById("movie-view-container") as dynamic;
            //        node?.parentNode?.removeChild(node);

            //        dynamic node2 = htmldoc.querySelector(".sidebar");
            //        node2?.parentNode?.removeChild(node2);
            //    }
            //    catch (Exception ex) { lblStatus.Text = ex.Message; }
            //}
        }
    }
}
