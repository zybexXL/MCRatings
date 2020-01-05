namespace MCRatings
{
    partial class SourceSelect
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.optTMDb = new System.Windows.Forms.RadioButton();
            this.optOMDb = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // optTMDb
            // 
            this.optTMDb.AutoSize = true;
            this.optTMDb.Checked = true;
            this.optTMDb.Location = new System.Drawing.Point(2, 0);
            this.optTMDb.Name = "optTMDb";
            this.optTMDb.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.optTMDb.Size = new System.Drawing.Size(55, 17);
            this.optTMDb.TabIndex = 0;
            this.optTMDb.TabStop = true;
            this.optTMDb.Text = "TMDb";
            this.optTMDb.UseVisualStyleBackColor = true;
            // 
            // optOMDb
            // 
            this.optOMDb.AutoSize = true;
            this.optOMDb.Location = new System.Drawing.Point(72, 0);
            this.optOMDb.Name = "optOMDb";
            this.optOMDb.Size = new System.Drawing.Size(56, 17);
            this.optOMDb.TabIndex = 0;
            this.optOMDb.TabStop = true;
            this.optOMDb.Text = "OMDb";
            this.optOMDb.UseVisualStyleBackColor = true;
            // 
            // SourceSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.optOMDb);
            this.Controls.Add(this.optTMDb);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SourceSelect";
            this.Size = new System.Drawing.Size(128, 17);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton optTMDb;
        private System.Windows.Forms.RadioButton optOMDb;
    }
}
