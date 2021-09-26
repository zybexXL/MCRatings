using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    public partial class GetInfo : Form
    {
        public GetInfo()
        {
            InitializeComponent();
        }

        private void GetInfo_Load(object sender, EventArgs e)
        {
            foreach (AppField c in Enum.GetValues(typeof(AppField)))
            {
                if (c >= AppField.Title && c!= AppField.File && c!= AppField.Imported && c!= AppField.Playlists)
                {
                    FieldInfo info = Constants.ViewColumnInfo[c];
                    listFields.Items.Add(info.GridHeader);
                }
            }
        }
    }
}
