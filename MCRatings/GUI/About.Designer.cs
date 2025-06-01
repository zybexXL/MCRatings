namespace ZRatings
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            label1 = new System.Windows.Forms.Label();
            lblVersion = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            btnUpdate = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            linkLabel2 = new System.Windows.Forms.LinkLabel();
            linkLabel3 = new System.Windows.Forms.LinkLabel();
            linkLabel1 = new System.Windows.Forms.LinkLabel();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            linkLabel5 = new System.Windows.Forms.LinkLabel();
            linkLabel6 = new System.Windows.Forms.LinkLabel();
            linkLabel7 = new System.Windows.Forms.LinkLabel();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            lblContact = new System.Windows.Forms.LinkLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            linkLabel4 = new System.Windows.Forms.LinkLabel();
            label2 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label14 = new System.Windows.Forms.Label();
            lblDebug = new System.Windows.Forms.Label();
            label18 = new System.Windows.Forms.Label();
            linkLabel9 = new System.Windows.Forms.LinkLabel();
            linkLabel10 = new System.Windows.Forms.LinkLabel();
            label19 = new System.Windows.Forms.Label();
            pictureBox4 = new System.Windows.Forms.PictureBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            pictureBox5 = new System.Windows.Forms.PictureBox();
            picPaypal = new System.Windows.Forms.PictureBox();
            label15 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPaypal).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            label1.Location = new System.Drawing.Point(142, -1);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(250, 71);
            label1.TabIndex = 0;
            label1.Text = "ZRatings";
            label1.MouseDown += MouseDown_Drag;
            // 
            // lblVersion
            // 
            lblVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblVersion.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            lblVersion.Location = new System.Drawing.Point(153, 62);
            lblVersion.Margin = new System.Windows.Forms.Padding(3);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(103, 17);
            lblVersion.TabIndex = 0;
            lblVersion.Text = "Version 1.0.0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label6.Location = new System.Drawing.Point(9, 249);
            label6.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(309, 15);
            label6.TabIndex = 0;
            label6.Text = "JRiver Media Center is © JRiver, Inc. All Rights Reserved";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label7.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            label7.Location = new System.Drawing.Point(153, 85);
            label7.Margin = new System.Windows.Forms.Padding(3);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(128, 15);
            label7.TabIndex = 0;
            label7.Text = "© 2020  Pedro Fonseca";
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnUpdate.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnUpdate.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            btnUpdate.Location = new System.Drawing.Point(154, 117);
            btnUpdate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new System.Drawing.Size(116, 24);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Check for update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnClose.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            btnClose.ForeColor = System.Drawing.Color.White;
            btnClose.Location = new System.Drawing.Point(508, 0);
            btnClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(28, 25);
            btnClose.TabIndex = 1;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel2.LinkColor = System.Drawing.Color.Teal;
            linkLabel2.Location = new System.Drawing.Point(435, 105);
            linkLabel2.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new System.Drawing.Size(82, 15);
            linkLabel2.TabIndex = 5;
            linkLabel2.TabStop = true;
            linkLabel2.Tag = "github.com/zybexXL/MCRatings/blob/master/LICENSE";
            linkLabel2.Text = "GPL v3 license";
            linkLabel2.LinkClicked += linkLabel_LinkClicked;
            // 
            // linkLabel3
            // 
            linkLabel3.AutoSize = true;
            linkLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel3.LinkColor = System.Drawing.Color.Teal;
            linkLabel3.Location = new System.Drawing.Point(432, 84);
            linkLabel3.Margin = new System.Windows.Forms.Padding(3);
            linkLabel3.Name = "linkLabel3";
            linkLabel3.Size = new System.Drawing.Size(85, 15);
            linkLabel3.TabIndex = 5;
            linkLabel3.TabStop = true;
            linkLabel3.Tag = "github.com/zybexXL/MCRatings";
            linkLabel3.Text = "GitHub project";
            toolTip1.SetToolTip(linkLabel3, "Open sauce. Yummy.");
            linkLabel3.LinkClicked += linkLabel_LinkClicked;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel1.LinkColor = System.Drawing.Color.Teal;
            linkLabel1.Location = new System.Drawing.Point(427, 42);
            linkLabel1.Margin = new System.Windows.Forms.Padding(3);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(90, 15);
            linkLabel1.TabIndex = 5;
            linkLabel1.TabStop = true;
            linkLabel1.Tag = "github.com/zybexXL/MCRatings/wiki";
            linkLabel1.Text = "Documentation";
            toolTip1.SetToolTip(linkLabel1, "Hey, you never know, maybe someone has written it already...");
            linkLabel1.LinkClicked += linkLabel_LinkClicked;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label3.ForeColor = System.Drawing.Color.FromArgb(0, 0, 64);
            label3.Location = new System.Drawing.Point(9, 230);
            label3.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(131, 17);
            label3.TabIndex = 0;
            label3.Text = "JRiver Media Center";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label4.ForeColor = System.Drawing.Color.FromArgb(0, 0, 64);
            label4.Location = new System.Drawing.Point(66, 284);
            label4.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(171, 17);
            label4.TabIndex = 7;
            label4.Text = "The Open Movie Database";
            // 
            // linkLabel5
            // 
            linkLabel5.AutoSize = true;
            linkLabel5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel5.LinkArea = new System.Windows.Forms.LinkArea(0, 20);
            linkLabel5.LinkColor = System.Drawing.Color.FromArgb(0, 64, 64);
            linkLabel5.Location = new System.Drawing.Point(146, 231);
            linkLabel5.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel5.Name = "linkLabel5";
            linkLabel5.Size = new System.Drawing.Size(58, 21);
            linkLabel5.TabIndex = 5;
            linkLabel5.TabStop = true;
            linkLabel5.Tag = "jriver.com";
            linkLabel5.Text = "jriver.com";
            linkLabel5.UseCompatibleTextRendering = true;
            linkLabel5.LinkClicked += linkLabel_LinkClicked;
            // 
            // linkLabel6
            // 
            linkLabel6.AutoSize = true;
            linkLabel6.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel6.LinkArea = new System.Windows.Forms.LinkArea(0, 12);
            linkLabel6.LinkColor = System.Drawing.Color.FromArgb(64, 64, 64);
            linkLabel6.Location = new System.Drawing.Point(234, 334);
            linkLabel6.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel6.Name = "linkLabel6";
            linkLabel6.Size = new System.Drawing.Size(76, 15);
            linkLabel6.TabIndex = 5;
            linkLabel6.TabStop = true;
            linkLabel6.Tag = "www.omdbapi.com/legal.htm";
            linkLabel6.Text = "Terms of Use";
            linkLabel6.LinkClicked += linkLabel_LinkClicked;
            // 
            // linkLabel7
            // 
            linkLabel7.AutoSize = true;
            linkLabel7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel7.LinkArea = new System.Windows.Forms.LinkArea(0, 30);
            linkLabel7.LinkColor = System.Drawing.Color.FromArgb(0, 64, 64);
            linkLabel7.Location = new System.Drawing.Point(66, 334);
            linkLabel7.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel7.Name = "linkLabel7";
            linkLabel7.Size = new System.Drawing.Size(109, 21);
            linkLabel7.TabIndex = 5;
            linkLabel7.TabStop = true;
            linkLabel7.Tag = "www.omdbapi.com";
            linkLabel7.Text = "www.omdbapi.com";
            linkLabel7.UseCompatibleTextRendering = true;
            linkLabel7.LinkClicked += linkLabel_LinkClicked;
            // 
            // label8
            // 
            label8.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label8.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label8.Location = new System.Drawing.Point(66, 303);
            label8.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(451, 33);
            label8.TabIndex = 0;
            label8.Text = "Please consider supporting OMDb by becoming a Patron or using the donation links provided on OMDb's website.";
            // 
            // label9
            // 
            label9.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label9.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label9.Location = new System.Drawing.Point(9, 459);
            label9.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(511, 59);
            label9.TabIndex = 0;
            label9.Text = resources.GetString("label9.Text");
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label10.ForeColor = System.Drawing.Color.FromArgb(0, 0, 64);
            label10.Location = new System.Drawing.Point(9, 439);
            label10.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(73, 17);
            label10.TabIndex = 0;
            label10.Text = "Disclaimer";
            // 
            // label11
            // 
            label11.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label11.AutoSize = true;
            label11.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label11.ForeColor = System.Drawing.Color.Teal;
            label11.Location = new System.Drawing.Point(7, 533);
            label11.Margin = new System.Windows.Forms.Padding(3);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(204, 25);
            label11.TabIndex = 0;
            label11.Text = "Do you like ZRatings?";
            // 
            // label12
            // 
            label12.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label12.AutoSize = true;
            label12.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label12.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            label12.Location = new System.Drawing.Point(9, 558);
            label12.Margin = new System.Windows.Forms.Padding(3);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(366, 17);
            label12.TabIndex = 0;
            label12.Text = "Please consider a donation to support my work. Thank you! :)";
            // 
            // lblContact
            // 
            lblContact.AutoSize = true;
            lblContact.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblContact.LinkColor = System.Drawing.Color.Teal;
            lblContact.Location = new System.Drawing.Point(468, 126);
            lblContact.Margin = new System.Windows.Forms.Padding(3);
            lblContact.Name = "lblContact";
            lblContact.Size = new System.Drawing.Size(49, 15);
            lblContact.TabIndex = 5;
            lblContact.TabStop = true;
            lblContact.Tag = "";
            lblContact.Text = "Contact";
            toolTip1.SetToolTip(lblContact, "pbfonseca@gmail.com");
            lblContact.LinkClicked += lblContact_LinkClicked;
            // 
            // linkLabel4
            // 
            linkLabel4.AutoSize = true;
            linkLabel4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel4.LinkColor = System.Drawing.Color.Teal;
            linkLabel4.Location = new System.Drawing.Point(444, 63);
            linkLabel4.Margin = new System.Windows.Forms.Padding(3);
            linkLabel4.Name = "linkLabel4";
            linkLabel4.Size = new System.Drawing.Size(73, 15);
            linkLabel4.TabIndex = 5;
            linkLabel4.TabStop = true;
            linkLabel4.Tag = "yabb.jriver.com/interact/index.php/topic,125575.0.html";
            linkLabel4.Text = "JRiver forum";
            linkLabel4.LinkClicked += linkLabel_LinkClicked;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(238, 286);
            label2.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(80, 15);
            label2.TabIndex = 0;
            label2.Text = "by Brian Fritz";
            // 
            // label5
            // 
            label5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            label5.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            label5.Location = new System.Drawing.Point(12, 153);
            label5.Margin = new System.Windows.Forms.Padding(3);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(509, 2);
            label5.TabIndex = 0;
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label13.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label13.Location = new System.Drawing.Point(9, 185);
            label13.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(511, 34);
            label13.TabIndex = 0;
            label13.Text = "Many many thanks to XPTO (JRiver user) for the awesome testing and feedback provided during development.";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label14.ForeColor = System.Drawing.Color.FromArgb(0, 0, 64);
            label14.Location = new System.Drawing.Point(9, 166);
            label14.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(56, 17);
            label14.TabIndex = 0;
            label14.Text = "Thanks!";
            // 
            // lblDebug
            // 
            lblDebug.AutoSize = true;
            lblDebug.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblDebug.ForeColor = System.Drawing.Color.Red;
            lblDebug.Location = new System.Drawing.Point(310, 122);
            lblDebug.Margin = new System.Windows.Forms.Padding(3);
            lblDebug.Name = "lblDebug";
            lblDebug.Size = new System.Drawing.Size(82, 15);
            lblDebug.TabIndex = 0;
            lblDebug.Text = "debug version";
            lblDebug.Visible = false;
            // 
            // label18
            // 
            label18.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label18.AutoSize = true;
            label18.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label18.Location = new System.Drawing.Point(66, 387);
            label18.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label18.Name = "label18";
            label18.Size = new System.Drawing.Size(405, 15);
            label18.TabIndex = 0;
            label18.Text = "This product uses the TMDb API but is not endorsed or certified by TMDb.";
            // 
            // linkLabel9
            // 
            linkLabel9.AutoSize = true;
            linkLabel9.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel9.LinkArea = new System.Windows.Forms.LinkArea(0, 30);
            linkLabel9.LinkColor = System.Drawing.Color.FromArgb(0, 64, 64);
            linkLabel9.Location = new System.Drawing.Point(66, 404);
            linkLabel9.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel9.Name = "linkLabel9";
            linkLabel9.Size = new System.Drawing.Size(121, 21);
            linkLabel9.TabIndex = 5;
            linkLabel9.TabStop = true;
            linkLabel9.Tag = "www.themoviedb.org";
            linkLabel9.Text = "www.themoviedb.org";
            linkLabel9.UseCompatibleTextRendering = true;
            linkLabel9.LinkClicked += linkLabel_LinkClicked;
            // 
            // linkLabel10
            // 
            linkLabel10.AutoSize = true;
            linkLabel10.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            linkLabel10.LinkArea = new System.Windows.Forms.LinkArea(0, 12);
            linkLabel10.LinkColor = System.Drawing.Color.FromArgb(64, 64, 64);
            linkLabel10.Location = new System.Drawing.Point(234, 404);
            linkLabel10.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            linkLabel10.Name = "linkLabel10";
            linkLabel10.Size = new System.Drawing.Size(76, 15);
            linkLabel10.TabIndex = 5;
            linkLabel10.TabStop = true;
            linkLabel10.Tag = "www.themoviedb.org/documentation/api/terms-of-use";
            linkLabel10.Text = "Terms of Use";
            linkLabel10.LinkClicked += linkLabel_LinkClicked;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label19.ForeColor = System.Drawing.Color.FromArgb(0, 0, 64);
            label19.Location = new System.Drawing.Point(66, 368);
            label19.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            label19.Name = "label19";
            label19.Size = new System.Drawing.Size(95, 17);
            label19.TabIndex = 7;
            label19.Text = "The Movie DB";
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.movie_reel;
            pictureBox4.Location = new System.Drawing.Point(12, 289);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new System.Drawing.Size(48, 48);
            pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox4.TabIndex = 8;
            pictureBox4.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.tmdb_small_blue;
            pictureBox1.Location = new System.Drawing.Point(12, 371);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(48, 48);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.logo;
            pictureBox5.Location = new System.Drawing.Point(12, 13);
            pictureBox5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new System.Drawing.Size(128, 128);
            pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox5.TabIndex = 4;
            pictureBox5.TabStop = false;
            pictureBox5.MouseDown += MouseDown_Drag;
            // 
            // picPaypal
            // 
            picPaypal.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            picPaypal.Image = Properties.Resources.paypal;
            picPaypal.Location = new System.Drawing.Point(412, 533);
            picPaypal.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            picPaypal.Name = "picPaypal";
            picPaypal.Size = new System.Drawing.Size(105, 42);
            picPaypal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPaypal.TabIndex = 2;
            picPaypal.TabStop = false;
            picPaypal.Click += picPaypal_Click;
            // 
            // label15
            // 
            label15.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label15.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            label15.ForeColor = System.Drawing.Color.FromArgb(0, 64, 64);
            label15.Location = new System.Drawing.Point(12, 522);
            label15.Margin = new System.Windows.Forms.Padding(3);
            label15.Name = "label15";
            label15.Size = new System.Drawing.Size(509, 2);
            label15.TabIndex = 0;
            // 
            // About
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(533, 584);
            ControlBox = false;
            Controls.Add(pictureBox4);
            Controls.Add(pictureBox1);
            Controls.Add(label19);
            Controls.Add(label4);
            Controls.Add(lblContact);
            Controls.Add(linkLabel3);
            Controls.Add(linkLabel4);
            Controls.Add(linkLabel1);
            Controls.Add(linkLabel10);
            Controls.Add(linkLabel6);
            Controls.Add(linkLabel9);
            Controls.Add(linkLabel7);
            Controls.Add(linkLabel5);
            Controls.Add(linkLabel2);
            Controls.Add(pictureBox5);
            Controls.Add(picPaypal);
            Controls.Add(btnClose);
            Controls.Add(btnUpdate);
            Controls.Add(label15);
            Controls.Add(label5);
            Controls.Add(lblDebug);
            Controls.Add(label7);
            Controls.Add(label12);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label14);
            Controls.Add(label3);
            Controls.Add(label13);
            Controls.Add(label18);
            Controls.Add(label2);
            Controls.Add(label6);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(lblVersion);
            Controls.Add(label1);
            Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "About";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Load += About_Load;
            KeyPress += About_KeyPress;
            MouseDown += MouseDown_Drag;
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPaypal).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox picPaypal;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel5;
        private System.Windows.Forms.LinkLabel linkLabel6;
        private System.Windows.Forms.LinkLabel linkLabel7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.LinkLabel lblContact;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblDebug;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.LinkLabel linkLabel9;
        private System.Windows.Forms.LinkLabel linkLabel10;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label15;
    }
}