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
    public partial class AppRenamed : Form
    {
        public AppRenamed()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Because apparently they own the letters J and MC.\nAnd here I was thinking our alphabet was much older...", "Latin? What's that?");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Well, I for one was a bit pissed off.");
        }
    }
}
