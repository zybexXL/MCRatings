namespace MCRatings
{
    partial class CollectionImporter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollectionImporter));
            this.browser = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.lblImport = new System.Windows.Forms.ToolStripLabel();
            this.txtURL = new System.Windows.Forms.ToolStripTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.btnHome = new System.Windows.Forms.ToolStripButton();
            this.btnImportAll = new System.Windows.Forms.ToolStripButton();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.btnCancel = new System.Windows.Forms.ToolStripButton();
            this.btnReload = new System.Windows.Forms.ToolStripButton();
            this.btnBack = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // browser
            // 
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.Location = new System.Drawing.Point(0, 31);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.Size = new System.Drawing.Size(1184, 608);
            this.browser.TabIndex = 4;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            this.browser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.browser_Navigated);
            this.browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browser_Navigating);
            this.browser.NewWindow += new System.ComponentModel.CancelEventHandler(this.browser_NewWindow);
            this.browser.ProgressChanged += new System.Windows.Forms.WebBrowserProgressChangedEventHandler(this.browser_ProgressChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnHome,
            this.btnImportAll,
            this.btnImport,
            this.btnCancel,
            this.lblImport,
            this.btnBack,
            this.btnReload,
            this.txtURL});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1184, 31);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // lblImport
            // 
            this.lblImport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lblImport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImport.ForeColor = System.Drawing.Color.White;
            this.lblImport.Name = "lblImport";
            this.lblImport.Size = new System.Drawing.Size(62, 28);
            this.lblImport.Text = "IMPORT:";
            // 
            // txtURL
            // 
            this.txtURL.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(500, 31);
            this.txtURL.Tag = "https://passthepopcorn.me/collages.php";
            this.txtURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtURL_KeyPress);
            // 
            // statusStrip1
            // 
            this.statusStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.toolStripStatusLabel2,
            this.progressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 639);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1184, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(55, 17);
            this.lblStatus.Text = "hey xpto!";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(812, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // progressBar
            // 
            this.progressBar.MarqueeAnimationSpeed = 25;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 16);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // btnHome
            // 
            this.btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHome.Image = global::MCRatings.Properties.Resources.home_fff_24;
            this.btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(28, 28);
            this.btnHome.Text = "Home";
            this.btnHome.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnImportAll
            // 
            this.btnImportAll.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnImportAll.ForeColor = System.Drawing.Color.Aqua;
            this.btnImportAll.Image = global::MCRatings.Properties.Resources.save_as_fff_24;
            this.btnImportAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImportAll.Name = "btnImportAll";
            this.btnImportAll.Size = new System.Drawing.Size(83, 28);
            this.btnImportAll.Text = "All Pages";
            this.btnImportAll.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnImportAll.ToolTipText = "Import all pages from collection";
            this.btnImportAll.Click += new System.EventHandler(this.btnImportAll_Click);
            // 
            // btnImport
            // 
            this.btnImport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnImport.ForeColor = System.Drawing.Color.Lime;
            this.btnImport.Image = global::MCRatings.Properties.Resources.save_fff_24;
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Margin = new System.Windows.Forms.Padding(10, 1, 10, 2);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(86, 28);
            this.btnImport.Text = "This Page";
            this.btnImport.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnImport.ToolTipText = "Import this collection page";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.Yellow;
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(103, 28);
            this.btnCancel.Text = "CANCEL IMPORT";
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnReload
            // 
            this.btnReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReload.Image = global::MCRatings.Properties.Resources.refresh_fff_24;
            this.btnReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReload.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(28, 28);
            this.btnReload.Text = "Reload";
            this.btnReload.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnBack
            // 
            this.btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBack.Image = global::MCRatings.Properties.Resources.back_fff_24;
            this.btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBack.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(28, 28);
            this.btnBack.Text = "Reload";
            this.btnBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // CollectionImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 661);
            this.Controls.Add(this.browser);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(1000, 500);
            this.Name = "CollectionImporter";
            this.Text = "Collection Importer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CollectionImporter_FormClosing);
            this.Load += new System.EventHandler(this.CollectionImporter_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripTextBox txtURL;
        private System.Windows.Forms.ToolStripButton btnReload;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.ToolStripButton btnImportAll;
        private System.Windows.Forms.ToolStripLabel lblImport;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripButton btnCancel;
        private System.Windows.Forms.ToolStripButton btnBack;
    }
}