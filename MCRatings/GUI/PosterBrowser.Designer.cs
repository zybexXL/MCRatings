namespace MCRatings
{
    partial class PosterBrowser
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PosterBrowser));
            this.browser = new System.Windows.Forms.WebBrowser();
            this.lblRes = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblCurr = new System.Windows.Forms.Label();
            this.lblThumbError = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.comboLanguage = new System.Windows.Forms.ToolStripComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnSelectLockOriginal = new System.Windows.Forms.Button();
            this.btnSelectOriginal = new System.Windows.Forms.Button();
            this.btnFitLeft = new System.Windows.Forms.Button();
            this.imgBox = new System.Windows.Forms.PictureBox();
            this.btnHome = new System.Windows.Forms.ToolStripButton();
            this.btnPrev = new System.Windows.Forms.ToolStripButton();
            this.btnNext = new System.Windows.Forms.ToolStripButton();
            this.btnSelect = new System.Windows.Forms.ToolStripButton();
            this.btnSelectLock = new System.Windows.Forms.ToolStripButton();
            this.btnThumbsize = new System.Windows.Forms.ToolStripButton();
            this.btnSort = new System.Windows.Forms.ToolStripButton();
            this.btnFitRight = new System.Windows.Forms.ToolStripButton();
            this.btnCast = new System.Windows.Forms.ToolStripButton();
            this.btnShowAvatars = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBox)).BeginInit();
            this.SuspendLayout();
            // 
            // browser
            // 
            this.browser.AllowWebBrowserDrop = false;
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.IsWebBrowserContextMenuEnabled = false;
            this.browser.Location = new System.Drawing.Point(0, 44);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.ScriptErrorsSuppressed = true;
            this.browser.Size = new System.Drawing.Size(753, 417);
            this.browser.TabIndex = 0;
            this.browser.WebBrowserShortcutsEnabled = false;
            this.browser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.browser_Navigated);
            this.browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browser_Navigating);
            this.browser.NewWindow += new System.ComponentModel.CancelEventHandler(this.browser_NewWindow);
            this.browser.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.browser_PreviewKeyDown);
            // 
            // lblRes
            // 
            this.lblRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRes.ForeColor = System.Drawing.Color.Green;
            this.lblRes.Location = new System.Drawing.Point(9, 55);
            this.lblRes.Margin = new System.Windows.Forms.Padding(0);
            this.lblRes.Name = "lblRes";
            this.lblRes.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblRes.Size = new System.Drawing.Size(307, 18);
            this.lblRes.TabIndex = 1;
            this.lblRes.Text = "3000 x 1500";
            this.lblRes.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.Wheat;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.AutoScrollMargin = new System.Drawing.Size(10, 10);
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectLockOriginal);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectOriginal);
            this.splitContainer1.Panel1.Controls.Add(this.btnFitLeft);
            this.splitContainer1.Panel1.Controls.Add(this.lblRes);
            this.splitContainer1.Panel1.Controls.Add(this.imgBox);
            this.splitContainer1.Panel1.Controls.Add(this.lblCurr);
            this.splitContainer1.Panel1.Controls.Add(this.lblThumbError);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.splitContainer1.Panel2.Controls.Add(this.browser);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip1);
            this.splitContainer1.Panel2MinSize = 200;
            this.splitContainer1.Size = new System.Drawing.Size(1084, 461);
            this.splitContainer1.SplitterDistance = 327;
            this.splitContainer1.TabIndex = 8;
            this.splitContainer1.TabStop = false;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            this.splitContainer1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseDoubleClick);
            // 
            // lblCurr
            // 
            this.lblCurr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurr.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblCurr.Location = new System.Drawing.Point(9, 35);
            this.lblCurr.Name = "lblCurr";
            this.lblCurr.Size = new System.Drawing.Size(307, 20);
            this.lblCurr.TabIndex = 8;
            this.lblCurr.Text = "Current Poster";
            this.lblCurr.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblThumbError
            // 
            this.lblThumbError.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThumbError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblThumbError.Location = new System.Drawing.Point(96, 9);
            this.lblThumbError.Margin = new System.Windows.Forms.Padding(0);
            this.lblThumbError.Name = "lblThumbError";
            this.lblThumbError.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblThumbError.Size = new System.Drawing.Size(220, 15);
            this.lblThumbError.TabIndex = 1;
            this.lblThumbError.Text = "Failed to load full-size Poster!";
            this.lblThumbError.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.lblThumbError, "Failed to load full-size Poster!");
            this.lblThumbError.Click += new System.EventHandler(this.img1_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnHome,
            this.btnPrev,
            this.btnNext,
            this.btnSelect,
            this.btnSelectLock,
            this.toolStripSeparator1,
            this.btnThumbsize,
            this.btnSort,
            this.btnFitRight,
            this.comboLanguage,
            this.btnCast,
            this.btnShowAvatars});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.toolStrip1.Size = new System.Drawing.Size(753, 44);
            this.toolStrip1.TabIndex = 11;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // comboLanguage
            // 
            this.comboLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLanguage.DropDownWidth = 140;
            this.comboLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.comboLanguage.Items.AddRange(new object[] {
            "All languages"});
            this.comboLanguage.Margin = new System.Windows.Forms.Padding(10, 0, 1, 0);
            this.comboLanguage.MaxDropDownItems = 10;
            this.comboLanguage.Name = "comboLanguage";
            this.comboLanguage.Size = new System.Drawing.Size(140, 39);
            this.comboLanguage.ToolTipText = "Language filter\r\nNote that languages are entered by TMDb users and sometimes are " +
    "not correct.";
            this.comboLanguage.DropDownClosed += new System.EventHandler(this.comboLanguage_DropDownClosed);
            this.comboLanguage.SelectedIndexChanged += new System.EventHandler(this.LanguageChanged);
            // 
            // btnSelectLockOriginal
            // 
            this.btnSelectLockOriginal.BackgroundImage = global::MCRatings.Properties.Resources.AcceptLock32;
            this.btnSelectLockOriginal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectLockOriginal.FlatAppearance.BorderSize = 0;
            this.btnSelectLockOriginal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(243)))), ((int)(((byte)(198)))));
            this.btnSelectLockOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectLockOriginal.Location = new System.Drawing.Point(69, 8);
            this.btnSelectLockOriginal.Name = "btnSelectLockOriginal";
            this.btnSelectLockOriginal.Size = new System.Drawing.Size(24, 24);
            this.btnSelectLockOriginal.TabIndex = 9;
            this.btnSelectLockOriginal.TabStop = false;
            this.toolTip1.SetToolTip(this.btnSelectLockOriginal, "Select the current poster and lock the cell.\r\nLocking will prevent ZRatings from" +
        " suggesting a different poster next time.");
            this.btnSelectLockOriginal.UseVisualStyleBackColor = true;
            this.btnSelectLockOriginal.Click += new System.EventHandler(this.btnSelectLockOriginal_Click);
            // 
            // btnSelectOriginal
            // 
            this.btnSelectOriginal.BackgroundImage = global::MCRatings.Properties.Resources.Accept32;
            this.btnSelectOriginal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectOriginal.FlatAppearance.BorderSize = 0;
            this.btnSelectOriginal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(243)))), ((int)(((byte)(198)))));
            this.btnSelectOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectOriginal.Location = new System.Drawing.Point(39, 8);
            this.btnSelectOriginal.Name = "btnSelectOriginal";
            this.btnSelectOriginal.Size = new System.Drawing.Size(24, 24);
            this.btnSelectOriginal.TabIndex = 9;
            this.btnSelectOriginal.TabStop = false;
            this.toolTip1.SetToolTip(this.btnSelectOriginal, "Select the current poster");
            this.btnSelectOriginal.UseVisualStyleBackColor = true;
            this.btnSelectOriginal.Click += new System.EventHandler(this.btnSelectOriginal_Click);
            // 
            // btnFitLeft
            // 
            this.btnFitLeft.BackgroundImage = global::MCRatings.Properties.Resources.height32;
            this.btnFitLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFitLeft.FlatAppearance.BorderSize = 0;
            this.btnFitLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(243)))), ((int)(((byte)(198)))));
            this.btnFitLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFitLeft.Location = new System.Drawing.Point(9, 8);
            this.btnFitLeft.Name = "btnFitLeft";
            this.btnFitLeft.Size = new System.Drawing.Size(24, 24);
            this.btnFitLeft.TabIndex = 9;
            this.btnFitLeft.TabStop = false;
            this.toolTip1.SetToolTip(this.btnFitLeft, "Switch poster scaling on left side:");
            this.btnFitLeft.UseVisualStyleBackColor = true;
            this.btnFitLeft.Click += new System.EventHandler(this.btnFitLeft_Click);
            // 
            // imgBox
            // 
            this.imgBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgBox.BackColor = System.Drawing.Color.White;
            this.imgBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imgBox.Location = new System.Drawing.Point(9, 76);
            this.imgBox.Margin = new System.Windows.Forms.Padding(0);
            this.imgBox.Name = "imgBox";
            this.imgBox.Size = new System.Drawing.Size(307, 356);
            this.imgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgBox.TabIndex = 0;
            this.imgBox.TabStop = false;
            this.toolTip1.SetToolTip(this.imgBox, "This is a thumbnail!\r\nClick to load full-size Poster");
            this.imgBox.Click += new System.EventHandler(this.img1_Click);
            // 
            // btnHome
            // 
            this.btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHome.Image = global::MCRatings.Properties.Resources.home64;
            this.btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHome.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(36, 36);
            this.btnHome.ToolTipText = "Back to poster thumbnails";
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPrev.Enabled = false;
            this.btnPrev.Image = global::MCRatings.Properties.Resources.arrow_left_sm_2a2b2c_32;
            this.btnPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(36, 36);
            this.btnPrev.ToolTipText = "Previous poster";
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnNext
            // 
            this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNext.Enabled = false;
            this.btnNext.Image = global::MCRatings.Properties.Resources.arrow_right_sm_2a2b2c_32;
            this.btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(36, 36);
            this.btnNext.ToolTipText = "Next poster";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelect.Enabled = false;
            this.btnSelect.Image = global::MCRatings.Properties.Resources.Accept32;
            this.btnSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(36, 36);
            this.btnSelect.ToolTipText = "Select this poster";
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnSelectLock
            // 
            this.btnSelectLock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectLock.Enabled = false;
            this.btnSelectLock.Image = global::MCRatings.Properties.Resources.AcceptLock32;
            this.btnSelectLock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectLock.Name = "btnSelectLock";
            this.btnSelectLock.Size = new System.Drawing.Size(36, 36);
            this.btnSelectLock.ToolTipText = "Select this poster and lock the cell.\r\nLocking will prevent ZRatings from sugges" +
    "ting a different poster next time.";
            this.btnSelectLock.Click += new System.EventHandler(this.btnSelectLock_Click);
            // 
            // btnThumbsize
            // 
            this.btnThumbsize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnThumbsize.Image = global::MCRatings.Properties.Resources.grid9_64;
            this.btnThumbsize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnThumbsize.Name = "btnThumbsize";
            this.btnThumbsize.Size = new System.Drawing.Size(36, 36);
            this.btnThumbsize.ToolTipText = "Toggle between small and large thumbnails";
            this.btnThumbsize.Click += new System.EventHandler(this.btnThumbsize_Click);
            // 
            // btnSort
            // 
            this.btnSort.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSort.Image = global::MCRatings.Properties.Resources.resolution64;
            this.btnSort.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(36, 36);
            this.btnSort.ToolTipText = resources.GetString("btnSort.ToolTipText");
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // btnFitRight
            // 
            this.btnFitRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFitRight.Image = global::MCRatings.Properties.Resources.height32;
            this.btnFitRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFitRight.Name = "btnFitRight";
            this.btnFitRight.Size = new System.Drawing.Size(36, 36);
            this.btnFitRight.ToolTipText = resources.GetString("btnFitRight.ToolTipText");
            this.btnFitRight.Click += new System.EventHandler(this.btnFitRight_Click);
            // 
            // btnCast
            // 
            this.btnCast.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnCast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCast.Image = global::MCRatings.Properties.Resources.oscar32;
            this.btnCast.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCast.Name = "btnCast";
            this.btnCast.Size = new System.Drawing.Size(36, 36);
            this.btnCast.ToolTipText = "Toggle between Poster view and Cast/Crew view";
            this.btnCast.Click += new System.EventHandler(this.btnCast_Click);
            // 
            // btnShowAvatars
            // 
            this.btnShowAvatars.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnShowAvatars.AutoSize = false;
            this.btnShowAvatars.Checked = true;
            this.btnShowAvatars.CheckOnClick = true;
            this.btnShowAvatars.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnShowAvatars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAvatars.Image = global::MCRatings.Properties.Resources.user_414445_24;
            this.btnShowAvatars.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnShowAvatars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAvatars.Name = "btnShowAvatars";
            this.btnShowAvatars.Size = new System.Drawing.Size(28, 28);
            this.btnShowAvatars.ToolTipText = "Show placeholders when there\'s no picture available";
            this.btnShowAvatars.Visible = false;
            this.btnShowAvatars.Click += new System.EventHandler(this.btnShowAvatars_Click);
            // 
            // PosterBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(1084, 461);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "PosterBrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Poster Browser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PosterSelector_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PosterSelector_KeyPress);
            this.Resize += new System.EventHandler(this.PosterSelector_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Label lblRes;
        private System.Windows.Forms.PictureBox imgBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblThumbError;
        private System.Windows.Forms.Label lblCurr;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripButton btnPrev;
        private System.Windows.Forms.ToolStripButton btnNext;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSelect;
        private System.Windows.Forms.ToolStripButton btnSort;
        private System.Windows.Forms.ToolStripButton btnFitRight;
        private System.Windows.Forms.ToolStripComboBox comboLanguage;
        private System.Windows.Forms.Button btnFitLeft;
        private System.Windows.Forms.ToolStripButton btnSelectLock;
        private System.Windows.Forms.ToolStripButton btnThumbsize;
        private System.Windows.Forms.ToolStripButton btnCast;
        private System.Windows.Forms.Button btnSelectLockOriginal;
        private System.Windows.Forms.Button btnSelectOriginal;
        private System.Windows.Forms.ToolStripButton btnShowAvatars;
    }
}