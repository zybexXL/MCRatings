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
    public partial class ColorEditor : Form
    {
        List<Control> labels;

        public ColorEditor()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            labels = new List<Control> { label1, label2, label3, label4, label5, label6, label7, label8, label9 };
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ColorKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void lblColor_click(object sender, EventArgs e)
        {
            colorDialog1.Color = ((Label)sender).BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                ((Label)sender).BackColor = colorDialog1.Color;

        }

        private void ColorGuide_Load(object sender, EventArgs e)
        {
            LoadColors(Program.settings.CellColors ?? Constants.CellColors);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadColors(Constants.CellColors);
        }

        void LoadColors(uint[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
                labels[i].BackColor = Color.FromArgb((int)colors[i]);
        }

        uint[] GetColors()
        {
            uint[] colors = new uint[labels.Count];
            for (int i = 0; i < labels.Count; i++)
                colors[i] = (uint)labels[i].BackColor.ToArgb();
            return colors;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Program.settings.CellColors = GetColors();
            Program.settings.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
