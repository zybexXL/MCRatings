namespace MCRatings
{
    partial class SettingsUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsUI));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gridFields = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCleanup = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAPIKeys = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnDiscard = new System.Windows.Forms.Button();
            this.lblReset = new System.Windows.Forms.LinkLabel();
            this.chkFastStart = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            this.txtLanguage = new System.Windows.Forms.ComboBox();
            this.btnAudio = new System.Windows.Forms.Button();
            this.txtTMDBkeys = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.chkWebmedia = new System.Windows.Forms.CheckBox();
            this.tabSettings = new System.Windows.Forms.TabControl();
            this.tabAPI = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.maxListLimit = new System.Windows.Forms.NumericUpDown();
            this.tabFields = new System.Windows.Forms.TabPage();
            this.dgEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgSetting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgOverwrite = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgSourcePlaceHolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridFields)).BeginInit();
            this.tabSettings.SuspendLayout();
            this.tabAPI.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxListLimit)).BeginInit();
            this.tabFields.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridFields
            // 
            this.gridFields.AllowUserToAddRows = false;
            this.gridFields.AllowUserToDeleteRows = false;
            this.gridFields.AllowUserToResizeRows = false;
            this.gridFields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridFields.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgEnabled,
            this.dgSetting,
            this.dgField,
            this.dgOverwrite,
            this.dgSourcePlaceHolder});
            this.gridFields.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.gridFields.Location = new System.Drawing.Point(9, 32);
            this.gridFields.Margin = new System.Windows.Forms.Padding(2);
            this.gridFields.MultiSelect = false;
            this.gridFields.Name = "gridFields";
            this.gridFields.RowHeadersVisible = false;
            this.gridFields.RowTemplate.Height = 24;
            this.gridFields.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gridFields.Size = new System.Drawing.Size(556, 401);
            this.gridFields.TabIndex = 1;
            this.gridFields.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridFields_CellClick);
            this.gridFields.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridFields_CellValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 207);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Cleanup filenames:";
            // 
            // txtCleanup
            // 
            this.txtCleanup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCleanup.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCleanup.Location = new System.Drawing.Point(14, 225);
            this.txtCleanup.Multiline = true;
            this.txtCleanup.Name = "txtCleanup";
            this.txtCleanup.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCleanup.Size = new System.Drawing.Size(544, 40);
            this.txtCleanup.TabIndex = 3;
            this.txtCleanup.TextChanged += new System.EventHandler(this.somethingChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(487, 473);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 21;
            this.btnSave.Text = "OK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "OMDb API Keys:";
            // 
            // txtAPIKeys
            // 
            this.txtAPIKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAPIKeys.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAPIKeys.Location = new System.Drawing.Point(14, 35);
            this.txtAPIKeys.Multiline = true;
            this.txtAPIKeys.Name = "txtAPIKeys";
            this.txtAPIKeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtAPIKeys.Size = new System.Drawing.Size(544, 40);
            this.txtAPIKeys.TabIndex = 1;
            this.txtAPIKeys.TextChanged += new System.EventHandler(this.somethingChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(11, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Enter key(s) separated by space";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel1.Location = new System.Drawing.Point(347, 17);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(211, 15);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Tag = "http://www.omdbapi.com/apikey.aspx";
            this.linkLabel1.Text = "Click here to register for a free API key";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label4.Location = new System.Drawing.Point(11, 268);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(231, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "List of non-title words separated by space";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(233, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "MCRatings to JRiver field mapping:";
            // 
            // btnDiscard
            // 
            this.btnDiscard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDiscard.Location = new System.Drawing.Point(401, 473);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.Size = new System.Drawing.Size(75, 23);
            this.btnDiscard.TabIndex = 20;
            this.btnDiscard.Text = "Cancel";
            this.btnDiscard.UseVisualStyleBackColor = true;
            this.btnDiscard.Click += new System.EventHandler(this.btnDiscard_Click);
            // 
            // lblReset
            // 
            this.lblReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblReset.AutoSize = true;
            this.lblReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReset.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lblReset.LinkColor = System.Drawing.Color.Navy;
            this.lblReset.Location = new System.Drawing.Point(529, 17);
            this.lblReset.Name = "lblReset";
            this.lblReset.Size = new System.Drawing.Size(35, 13);
            this.lblReset.TabIndex = 2;
            this.lblReset.TabStop = true;
            this.lblReset.Text = "Reset";
            this.lblReset.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblReset_LinkClicked);
            // 
            // chkFastStart
            // 
            this.chkFastStart.AutoSize = true;
            this.chkFastStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkFastStart.Location = new System.Drawing.Point(14, 398);
            this.chkFastStart.Name = "chkFastStart";
            this.chkFastStart.Size = new System.Drawing.Size(146, 20);
            this.chkFastStart.TabIndex = 8;
            this.chkFastStart.Text = "Fast playlist loading";
            this.toolTip1.SetToolTip(this.chkFastStart, resources.GetString("chkFastStart.ToolTip"));
            this.chkFastStart.UseVisualStyleBackColor = true;
            this.chkFastStart.CheckedChanged += new System.EventHandler(this.somethingChanged);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 30000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkHelp.LinkColor = System.Drawing.Color.Teal;
            this.linkHelp.Location = new System.Drawing.Point(9, 477);
            this.linkHelp.Margin = new System.Windows.Forms.Padding(3);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(75, 15);
            this.linkHelp.TabIndex = 16;
            this.linkHelp.TabStop = true;
            this.linkHelp.Tag = "https://github.com/zybexXL/MCRatings/wiki#Configuration";
            this.linkHelp.Text = "Settings help";
            this.toolTip1.SetToolTip(this.linkHelp, "Hey, you never know, maybe someone has written it already...");
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // txtLanguage
            // 
            this.txtLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLanguage.FormattingEnabled = true;
            this.txtLanguage.ItemHeight = 16;
            this.txtLanguage.Items.AddRange(new object[] {
            "EN",
            "FR",
            "DE",
            "ES",
            "PT",
            "CN",
            "JP",
            "RU",
            "BR"});
            this.txtLanguage.Location = new System.Drawing.Point(491, 341);
            this.txtLanguage.MaxLength = 5;
            this.txtLanguage.Name = "txtLanguage";
            this.txtLanguage.Size = new System.Drawing.Size(67, 24);
            this.txtLanguage.TabIndex = 7;
            this.txtLanguage.Text = "EN";
            this.toolTip1.SetToolTip(this.txtLanguage, resources.GetString("txtLanguage.ToolTip"));
            this.txtLanguage.TextChanged += new System.EventHandler(this.somethingChanged);
            // 
            // btnAudio
            // 
            this.btnAudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAudio.BackColor = System.Drawing.Color.Transparent;
            this.btnAudio.FlatAppearance.BorderSize = 0;
            this.btnAudio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAudio.Image = global::MCRatings.Properties.Resources.speaker_on;
            this.btnAudio.Location = new System.Drawing.Point(532, 397);
            this.btnAudio.Name = "btnAudio";
            this.btnAudio.Size = new System.Drawing.Size(26, 26);
            this.btnAudio.TabIndex = 10;
            this.toolTip1.SetToolTip(this.btnAudio, "Mute sound effects");
            this.btnAudio.UseVisualStyleBackColor = false;
            this.btnAudio.Click += new System.EventHandler(this.btnAudio_Click);
            // 
            // txtTMDBkeys
            // 
            this.txtTMDBkeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTMDBkeys.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTMDBkeys.Location = new System.Drawing.Point(14, 130);
            this.txtTMDBkeys.Multiline = true;
            this.txtTMDBkeys.Name = "txtTMDBkeys";
            this.txtTMDBkeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTMDBkeys.Size = new System.Drawing.Size(544, 40);
            this.txtTMDBkeys.TabIndex = 2;
            this.txtTMDBkeys.TextChanged += new System.EventHandler(this.somethingChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(11, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "TMDb API Keys:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(11, 173);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 15);
            this.label7.TabIndex = 10;
            this.label7.Text = "Enter key(s) separated by space";
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel2.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel2.Location = new System.Drawing.Point(347, 112);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(211, 15);
            this.linkLabel2.TabIndex = 2;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Tag = "https://www.themoviedb.org/settings/api";
            this.linkLabel2.Text = "Click here to register for a free API key";
            this.linkLabel2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // chkWebmedia
            // 
            this.chkWebmedia.AutoSize = true;
            this.chkWebmedia.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkWebmedia.Location = new System.Drawing.Point(14, 372);
            this.chkWebmedia.Name = "chkWebmedia";
            this.chkWebmedia.Size = new System.Drawing.Size(236, 20);
            this.chkWebmedia.TabIndex = 9;
            this.chkWebmedia.Text = "Use \"webmedia://\" for Trailer URLs";
            this.chkWebmedia.UseVisualStyleBackColor = true;
            this.chkWebmedia.CheckedChanged += new System.EventHandler(this.somethingChanged);
            // 
            // tabSettings
            // 
            this.tabSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabSettings.Controls.Add(this.tabAPI);
            this.tabSettings.Controls.Add(this.tabFields);
            this.tabSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabSettings.Location = new System.Drawing.Point(0, 0);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.SelectedIndex = 0;
            this.tabSettings.Size = new System.Drawing.Size(582, 467);
            this.tabSettings.TabIndex = 0;
            // 
            // tabAPI
            // 
            this.tabAPI.Controls.Add(this.label1);
            this.tabAPI.Controls.Add(this.txtCleanup);
            this.tabAPI.Controls.Add(this.label9);
            this.tabAPI.Controls.Add(this.label8);
            this.tabAPI.Controls.Add(this.label4);
            this.tabAPI.Controls.Add(this.chkWebmedia);
            this.tabAPI.Controls.Add(this.btnAudio);
            this.tabAPI.Controls.Add(this.maxListLimit);
            this.tabAPI.Controls.Add(this.txtLanguage);
            this.tabAPI.Controls.Add(this.label2);
            this.tabAPI.Controls.Add(this.chkFastStart);
            this.tabAPI.Controls.Add(this.label3);
            this.tabAPI.Controls.Add(this.txtAPIKeys);
            this.tabAPI.Controls.Add(this.linkLabel2);
            this.tabAPI.Controls.Add(this.linkLabel1);
            this.tabAPI.Controls.Add(this.label6);
            this.tabAPI.Controls.Add(this.txtTMDBkeys);
            this.tabAPI.Controls.Add(this.label7);
            this.tabAPI.Location = new System.Drawing.Point(4, 25);
            this.tabAPI.Name = "tabAPI";
            this.tabAPI.Padding = new System.Windows.Forms.Padding(3);
            this.tabAPI.Size = new System.Drawing.Size(574, 438);
            this.tabAPI.TabIndex = 2;
            this.tabAPI.Text = "Data source";
            this.tabAPI.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 344);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(167, 16);
            this.label9.TabIndex = 22;
            this.label9.Text = "TMDb preferred language:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 317);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(271, 16);
            this.label8.TabIndex = 22;
            this.label8.Text = "Max number of Actors, Writers, Directors, etc:";
            // 
            // maxListLimit
            // 
            this.maxListLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxListLimit.Location = new System.Drawing.Point(491, 315);
            this.maxListLimit.Name = "maxListLimit";
            this.maxListLimit.Size = new System.Drawing.Size(67, 22);
            this.maxListLimit.TabIndex = 6;
            this.maxListLimit.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.maxListLimit.ValueChanged += new System.EventHandler(this.somethingChanged);
            // 
            // tabFields
            // 
            this.tabFields.Controls.Add(this.label5);
            this.tabFields.Controls.Add(this.gridFields);
            this.tabFields.Controls.Add(this.lblReset);
            this.tabFields.Location = new System.Drawing.Point(4, 25);
            this.tabFields.Name = "tabFields";
            this.tabFields.Padding = new System.Windows.Forms.Padding(3);
            this.tabFields.Size = new System.Drawing.Size(574, 438);
            this.tabFields.TabIndex = 0;
            this.tabFields.Text = "Field mapping";
            this.tabFields.UseVisualStyleBackColor = true;
            // 
            // dgEnabled
            // 
            this.dgEnabled.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgEnabled.HeaderText = "Enabled";
            this.dgEnabled.Name = "dgEnabled";
            this.dgEnabled.Width = 65;
            // 
            // dgSetting
            // 
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgSetting.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgSetting.HeaderText = "MCRatings field";
            this.dgSetting.Name = "dgSetting";
            this.dgSetting.ReadOnly = true;
            this.dgSetting.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dgSetting.Width = 125;
            // 
            // dgField
            // 
            this.dgField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgField.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgField.HeaderText = "JRiver field (Display)";
            this.dgField.Name = "dgField";
            this.dgField.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgOverwrite
            // 
            this.dgOverwrite.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dgOverwrite.HeaderText = "Overwrite";
            this.dgOverwrite.Name = "dgOverwrite";
            this.dgOverwrite.Width = 70;
            // 
            // dgSourcePlaceHolder
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            this.dgSourcePlaceHolder.DefaultCellStyle = dataGridViewCellStyle9;
            this.dgSourcePlaceHolder.HeaderText = "Source";
            this.dgSourcePlaceHolder.Name = "dgSourcePlaceHolder";
            this.dgSourcePlaceHolder.ReadOnly = true;
            this.dgSourcePlaceHolder.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgSourcePlaceHolder.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dgSourcePlaceHolder.Width = 128;
            // 
            // SettingsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(580, 502);
            this.Controls.Add(this.tabSettings);
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.btnDiscard);
            this.Controls.Add(this.btnSave);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 400);
            this.Name = "SettingsUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsUI_FormClosing);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.Shown += new System.EventHandler(this.SettingsUI_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SettingsUI_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.gridFields)).EndInit();
            this.tabSettings.ResumeLayout(false);
            this.tabAPI.ResumeLayout(false);
            this.tabAPI.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxListLimit)).EndInit();
            this.tabFields.ResumeLayout(false);
            this.tabFields.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView gridFields;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCleanup;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAPIKeys;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnDiscard;
        private System.Windows.Forms.LinkLabel lblReset;
        private System.Windows.Forms.Button btnAudio;
        private System.Windows.Forms.CheckBox chkFastStart;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LinkLabel linkHelp;
        private System.Windows.Forms.TextBox txtTMDBkeys;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.CheckBox chkWebmedia;
        private System.Windows.Forms.TabControl tabSettings;
        private System.Windows.Forms.TabPage tabFields;
        private System.Windows.Forms.TabPage tabAPI;
        private System.Windows.Forms.ComboBox txtLanguage;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown maxListLimit;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgSetting;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgField;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgOverwrite;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgSourcePlaceHolder;
    }
}