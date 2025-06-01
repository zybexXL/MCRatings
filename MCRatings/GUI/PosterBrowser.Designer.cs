namespace ZRatings
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PosterBrowser));
            browser = new System.Windows.Forms.WebBrowser();
            lblRes = new System.Windows.Forms.Label();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            btnSelectLockOriginal = new System.Windows.Forms.Button();
            btnSelectOriginal = new System.Windows.Forms.Button();
            btnFitLeft = new System.Windows.Forms.Button();
            imgBox = new System.Windows.Forms.PictureBox();
            lblCurr = new System.Windows.Forms.Label();
            lblThumbError = new System.Windows.Forms.Label();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnHome = new System.Windows.Forms.ToolStripButton();
            btnPrev = new System.Windows.Forms.ToolStripButton();
            btnNext = new System.Windows.Forms.ToolStripButton();
            btnSelect = new System.Windows.Forms.ToolStripButton();
            btnSelectLock = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnThumbsize = new System.Windows.Forms.ToolStripButton();
            btnSort = new System.Windows.Forms.ToolStripButton();
            btnFitRight = new System.Windows.Forms.ToolStripButton();
            comboLanguage = new System.Windows.Forms.ToolStripComboBox();
            btnCast = new System.Windows.Forms.ToolStripButton();
            btnShowAvatars = new System.Windows.Forms.ToolStripButton();
            lblPosterCount = new System.Windows.Forms.ToolStripLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)imgBox).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // browser
            // 
            browser.AllowWebBrowserDrop = false;
            browser.Dock = System.Windows.Forms.DockStyle.Fill;
            browser.IsWebBrowserContextMenuEnabled = false;
            browser.Location = new System.Drawing.Point(0, 45);
            browser.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            browser.MinimumSize = new System.Drawing.Size(23, 23);
            browser.Name = "browser";
            browser.ScriptErrorsSuppressed = true;
            browser.Size = new System.Drawing.Size(879, 487);
            browser.TabIndex = 0;
            browser.WebBrowserShortcutsEnabled = false;
            browser.Navigated += browser_Navigated;
            browser.Navigating += browser_Navigating;
            browser.NewWindow += browser_NewWindow;
            browser.PreviewKeyDown += browser_PreviewKeyDown;
            // 
            // lblRes
            // 
            lblRes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblRes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblRes.ForeColor = System.Drawing.Color.Green;
            lblRes.Location = new System.Drawing.Point(10, 63);
            lblRes.Margin = new System.Windows.Forms.Padding(0);
            lblRes.Name = "lblRes";
            lblRes.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            lblRes.Size = new System.Drawing.Size(357, 21);
            lblRes.TabIndex = 1;
            lblRes.Text = "3000 x 1500";
            lblRes.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = System.Drawing.Color.Wheat;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.AutoScroll = true;
            splitContainer1.Panel1.AutoScrollMargin = new System.Drawing.Size(10, 10);
            splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            splitContainer1.Panel1.Controls.Add(btnSelectLockOriginal);
            splitContainer1.Panel1.Controls.Add(btnSelectOriginal);
            splitContainer1.Panel1.Controls.Add(btnFitLeft);
            splitContainer1.Panel1.Controls.Add(lblRes);
            splitContainer1.Panel1.Controls.Add(imgBox);
            splitContainer1.Panel1.Controls.Add(lblCurr);
            splitContainer1.Panel1.Controls.Add(lblThumbError);
            splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            splitContainer1.Panel2.Controls.Add(browser);
            splitContainer1.Panel2.Controls.Add(toolStrip1);
            splitContainer1.Panel2MinSize = 200;
            splitContainer1.Size = new System.Drawing.Size(1265, 532);
            splitContainer1.SplitterDistance = 381;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 8;
            splitContainer1.TabStop = false;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            splitContainer1.MouseDoubleClick += splitContainer1_MouseDoubleClick;
            // 
            // btnSelectLockOriginal
            // 
            btnSelectLockOriginal.BackgroundImage = Properties.Resources.AcceptLock32;
            btnSelectLockOriginal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btnSelectLockOriginal.FlatAppearance.BorderSize = 0;
            btnSelectLockOriginal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(229, 243, 198);
            btnSelectLockOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSelectLockOriginal.Location = new System.Drawing.Point(80, 9);
            btnSelectLockOriginal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSelectLockOriginal.Name = "btnSelectLockOriginal";
            btnSelectLockOriginal.Size = new System.Drawing.Size(28, 28);
            btnSelectLockOriginal.TabIndex = 9;
            btnSelectLockOriginal.TabStop = false;
            toolTip1.SetToolTip(btnSelectLockOriginal, "Select the current poster and lock the cell.\r\nLocking will prevent ZRatings from suggesting a different poster next time.");
            btnSelectLockOriginal.UseVisualStyleBackColor = true;
            btnSelectLockOriginal.Click += btnSelectLockOriginal_Click;
            // 
            // btnSelectOriginal
            // 
            btnSelectOriginal.BackgroundImage = Properties.Resources.Accept32;
            btnSelectOriginal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btnSelectOriginal.FlatAppearance.BorderSize = 0;
            btnSelectOriginal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(229, 243, 198);
            btnSelectOriginal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSelectOriginal.Location = new System.Drawing.Point(46, 9);
            btnSelectOriginal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSelectOriginal.Name = "btnSelectOriginal";
            btnSelectOriginal.Size = new System.Drawing.Size(28, 28);
            btnSelectOriginal.TabIndex = 9;
            btnSelectOriginal.TabStop = false;
            toolTip1.SetToolTip(btnSelectOriginal, "Select the current poster");
            btnSelectOriginal.UseVisualStyleBackColor = true;
            btnSelectOriginal.Click += btnSelectOriginal_Click;
            // 
            // btnFitLeft
            // 
            btnFitLeft.BackgroundImage = Properties.Resources.height32;
            btnFitLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            btnFitLeft.FlatAppearance.BorderSize = 0;
            btnFitLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(229, 243, 198);
            btnFitLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFitLeft.Location = new System.Drawing.Point(10, 9);
            btnFitLeft.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnFitLeft.Name = "btnFitLeft";
            btnFitLeft.Size = new System.Drawing.Size(28, 28);
            btnFitLeft.TabIndex = 9;
            btnFitLeft.TabStop = false;
            toolTip1.SetToolTip(btnFitLeft, "Switch poster scaling on left side:");
            btnFitLeft.UseVisualStyleBackColor = true;
            btnFitLeft.Click += btnFitLeft_Click;
            // 
            // imgBox
            // 
            imgBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            imgBox.BackColor = System.Drawing.Color.White;
            imgBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            imgBox.Location = new System.Drawing.Point(10, 88);
            imgBox.Margin = new System.Windows.Forms.Padding(0);
            imgBox.Name = "imgBox";
            imgBox.Size = new System.Drawing.Size(357, 410);
            imgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            imgBox.TabIndex = 0;
            imgBox.TabStop = false;
            toolTip1.SetToolTip(imgBox, "This is a thumbnail!\r\nClick to load full-size Poster");
            imgBox.Click += img1_Click;
            // 
            // lblCurr
            // 
            lblCurr.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblCurr.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblCurr.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            lblCurr.Location = new System.Drawing.Point(10, 40);
            lblCurr.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCurr.Name = "lblCurr";
            lblCurr.Size = new System.Drawing.Size(357, 23);
            lblCurr.TabIndex = 8;
            lblCurr.Text = "Current Poster";
            lblCurr.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblThumbError
            // 
            lblThumbError.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblThumbError.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
            lblThumbError.Location = new System.Drawing.Point(112, 10);
            lblThumbError.Margin = new System.Windows.Forms.Padding(0);
            lblThumbError.Name = "lblThumbError";
            lblThumbError.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            lblThumbError.Size = new System.Drawing.Size(257, 17);
            lblThumbError.TabIndex = 1;
            lblThumbError.Text = "Failed to load full-size Poster!";
            lblThumbError.TextAlign = System.Drawing.ContentAlignment.TopRight;
            toolTip1.SetToolTip(lblThumbError, "Failed to load full-size Poster!");
            lblThumbError.Click += img1_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnHome, btnPrev, btnNext, btnSelect, btnSelectLock, toolStripSeparator1, btnThumbsize, btnSort, btnFitRight, comboLanguage, btnCast, btnShowAvatars, lblPosterCount });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            toolStrip1.Size = new System.Drawing.Size(879, 45);
            toolStrip1.TabIndex = 11;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnHome
            // 
            btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnHome.Image = Properties.Resources.home64;
            btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnHome.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
            btnHome.Name = "btnHome";
            btnHome.Size = new System.Drawing.Size(36, 36);
            btnHome.ToolTipText = "Back to poster thumbnails";
            btnHome.Click += btnHome_Click;
            // 
            // btnPrev
            // 
            btnPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnPrev.Enabled = false;
            btnPrev.Image = Properties.Resources.arrow_left_sm_2a2b2c_32;
            btnPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnPrev.Name = "btnPrev";
            btnPrev.Size = new System.Drawing.Size(36, 36);
            btnPrev.ToolTipText = "Previous poster";
            btnPrev.Click += btnPrev_Click;
            // 
            // btnNext
            // 
            btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNext.Enabled = false;
            btnNext.Image = Properties.Resources.arrow_right_sm_2a2b2c_32;
            btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(36, 36);
            btnNext.ToolTipText = "Next poster";
            btnNext.Click += btnNext_Click;
            // 
            // btnSelect
            // 
            btnSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnSelect.Enabled = false;
            btnSelect.Image = Properties.Resources.Accept32;
            btnSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new System.Drawing.Size(36, 36);
            btnSelect.ToolTipText = "Select this poster";
            btnSelect.Click += btnSelect_Click;
            // 
            // btnSelectLock
            // 
            btnSelectLock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnSelectLock.Enabled = false;
            btnSelectLock.Image = Properties.Resources.AcceptLock32;
            btnSelectLock.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSelectLock.Name = "btnSelectLock";
            btnSelectLock.Size = new System.Drawing.Size(36, 36);
            btnSelectLock.ToolTipText = "Select this poster and lock the cell.\r\nLocking will prevent ZRatings from suggesting a different poster next time.";
            btnSelectLock.Click += btnSelectLock_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // btnThumbsize
            // 
            btnThumbsize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnThumbsize.Image = Properties.Resources.grid9_64;
            btnThumbsize.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnThumbsize.Name = "btnThumbsize";
            btnThumbsize.Size = new System.Drawing.Size(36, 36);
            btnThumbsize.ToolTipText = "Toggle between small and large thumbnails";
            btnThumbsize.Click += btnThumbsize_Click;
            // 
            // btnSort
            // 
            btnSort.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnSort.Image = Properties.Resources.resolution64;
            btnSort.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSort.Name = "btnSort";
            btnSort.Size = new System.Drawing.Size(36, 36);
            btnSort.ToolTipText = resources.GetString("btnSort.ToolTipText");
            btnSort.Click += btnSort_Click;
            // 
            // btnFitRight
            // 
            btnFitRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnFitRight.Image = Properties.Resources.height32;
            btnFitRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFitRight.Name = "btnFitRight";
            btnFitRight.Size = new System.Drawing.Size(36, 36);
            btnFitRight.ToolTipText = resources.GetString("btnFitRight.ToolTipText");
            btnFitRight.Click += btnFitRight_Click;
            // 
            // comboLanguage
            // 
            comboLanguage.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            comboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboLanguage.DropDownWidth = 140;
            comboLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            comboLanguage.Items.AddRange(new object[] { "All languages" });
            comboLanguage.Margin = new System.Windows.Forms.Padding(10, 0, 1, 0);
            comboLanguage.MaxDropDownItems = 10;
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new System.Drawing.Size(163, 39);
            comboLanguage.ToolTipText = "Language filter\r\nNote that languages are entered by TMDb users and sometimes are not correct.";
            comboLanguage.DropDownClosed += comboLanguage_DropDownClosed;
            comboLanguage.SelectedIndexChanged += LanguageChanged;
            // 
            // btnCast
            // 
            btnCast.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            btnCast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnCast.Image = Properties.Resources.oscar32;
            btnCast.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCast.Name = "btnCast";
            btnCast.Size = new System.Drawing.Size(36, 36);
            btnCast.ToolTipText = "Toggle between Poster view and Cast/Crew view";
            btnCast.Click += btnCast_Click;
            // 
            // btnShowAvatars
            // 
            btnShowAvatars.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            btnShowAvatars.AutoSize = false;
            btnShowAvatars.Checked = true;
            btnShowAvatars.CheckOnClick = true;
            btnShowAvatars.CheckState = System.Windows.Forms.CheckState.Checked;
            btnShowAvatars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnShowAvatars.Image = Properties.Resources.user_414445_24;
            btnShowAvatars.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            btnShowAvatars.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnShowAvatars.Name = "btnShowAvatars";
            btnShowAvatars.Size = new System.Drawing.Size(28, 28);
            btnShowAvatars.ToolTipText = "Show placeholders when there's no picture available";
            btnShowAvatars.Visible = false;
            btnShowAvatars.Click += btnShowAvatars_Click;
            // 
            // lblPosterCount
            // 
            lblPosterCount.ForeColor = System.Drawing.Color.FromArgb(192, 0, 0);
            lblPosterCount.Margin = new System.Windows.Forms.Padding(10, 1, 0, 2);
            lblPosterCount.Name = "lblPosterCount";
            lblPosterCount.Size = new System.Drawing.Size(132, 36);
            lblPosterCount.Text = "Showing 1 of 20 posters";
            // 
            // PosterBrowser
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            ClientSize = new System.Drawing.Size(1265, 532);
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(1047, 571);
            Name = "PosterBrowser";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Poster Browser";
            FormClosing += PosterSelector_FormClosing;
            KeyPress += PosterSelector_KeyPress;
            Resize += PosterSelector_Resize;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)imgBox).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);

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
        private System.Windows.Forms.ToolStripLabel lblPosterCount;
    }
}