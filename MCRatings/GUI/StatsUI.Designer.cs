namespace ZRatings
{
    partial class StatsUI
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgStats = new System.Windows.Forms.DataGridView();
            this.cMetric = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cSession = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lnkResetAll = new System.Windows.Forms.LinkLabel();
            this.btnResetCurrent = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dgStats)).BeginInit();
            this.SuspendLayout();
            // 
            // dgStats
            // 
            this.dgStats.AllowUserToAddRows = false;
            this.dgStats.AllowUserToDeleteRows = false;
            this.dgStats.AllowUserToOrderColumns = true;
            this.dgStats.AllowUserToResizeRows = false;
            this.dgStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgStats.BackgroundColor = System.Drawing.SystemColors.HighlightText;
            this.dgStats.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgStats.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgStats.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cMetric,
            this.cSession,
            this.cTotal});
            this.dgStats.Location = new System.Drawing.Point(7, 7);
            this.dgStats.Name = "dgStats";
            this.dgStats.ReadOnly = true;
            this.dgStats.RowHeadersVisible = false;
            this.dgStats.Size = new System.Drawing.Size(472, 499);
            this.dgStats.TabIndex = 0;
            // 
            // cMetric
            // 
            this.cMetric.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.cMetric.HeaderText = "";
            this.cMetric.Name = "cMetric";
            this.cMetric.ReadOnly = true;
            this.cMetric.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // cSession
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.cSession.DefaultCellStyle = dataGridViewCellStyle5;
            this.cSession.HeaderText = "Current";
            this.cSession.Name = "cSession";
            this.cSession.ReadOnly = true;
            this.cSession.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cSession.Width = 150;
            // 
            // cTotal
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle6.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.cTotal.DefaultCellStyle = dataGridViewCellStyle6;
            this.cTotal.HeaderText = "Totals";
            this.cTotal.Name = "cTotal";
            this.cTotal.ReadOnly = true;
            this.cTotal.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.cTotal.Width = 150;
            // 
            // lnkResetAll
            // 
            this.lnkResetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkResetAll.AutoSize = true;
            this.lnkResetAll.BackColor = System.Drawing.Color.Transparent;
            this.lnkResetAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkResetAll.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkResetAll.Location = new System.Drawing.Point(377, 509);
            this.lnkResetAll.Name = "lnkResetAll";
            this.lnkResetAll.Size = new System.Drawing.Size(92, 15);
            this.lnkResetAll.TabIndex = 18;
            this.lnkResetAll.TabStop = true;
            this.lnkResetAll.Text = "reset everything";
            this.lnkResetAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkResetAll_LinkClicked);
            // 
            // btnResetCurrent
            // 
            this.btnResetCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetCurrent.AutoSize = true;
            this.btnResetCurrent.BackColor = System.Drawing.Color.Transparent;
            this.btnResetCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetCurrent.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.btnResetCurrent.Location = new System.Drawing.Point(244, 509);
            this.btnResetCurrent.Name = "btnResetCurrent";
            this.btnResetCurrent.Size = new System.Drawing.Size(75, 15);
            this.btnResetCurrent.TabIndex = 18;
            this.btnResetCurrent.TabStop = true;
            this.btnResetCurrent.Text = "reset current";
            this.btnResetCurrent.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnResetCurrent_LinkClicked);
            // 
            // StatsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 531);
            this.Controls.Add(this.btnResetCurrent);
            this.Controls.Add(this.lnkResetAll);
            this.Controls.Add(this.dgStats);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "StatsUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Statistics";
            this.Load += new System.EventHandler(this.StatsUI_Load);
            this.Shown += new System.EventHandler(this.StatsUI_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.StatsUI_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.dgStats)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgStats;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMetric;
        private System.Windows.Forms.DataGridViewTextBoxColumn cSession;
        private System.Windows.Forms.DataGridViewTextBoxColumn cTotal;
        private System.Windows.Forms.LinkLabel lnkResetAll;
        private System.Windows.Forms.LinkLabel btnResetCurrent;
    }
}