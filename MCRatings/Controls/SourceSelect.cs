using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    public partial class SourceSelect : UserControl
    {
        bool isBlank = false;

        public Sources Value
        {
            get
            {
                return isBlank ? Sources.None
                    : optOMDb.Checked ? Sources.OMDb
                    : optTMDb.Checked ? Sources.TMDb
                    : Sources.None;
            }
            set
            {
                optOMDb.Checked = value == Sources.OMDb;
                optTMDb.Checked = value == Sources.TMDb;
            }
        }

        public SourceSelect(List<Sources> sources, Sources selected)
        {
            InitializeComponent();

            if (sources == null) sources = new List<Sources>() { Sources.None };
            isBlank = sources.Contains(Sources.None);
            optTMDb.Visible = sources.Contains(Sources.TMDb);
            optOMDb.Visible = sources.Contains(Sources.OMDb);
            Value = selected;
        }

        public bool OnClick(MouseEventArgs m)
        {
            var curr = Value;
            if (optTMDb.Visible && m.X > optTMDb.Left && m.X < optTMDb.Right && m.Y > optTMDb.Top && m.Y < optTMDb.Bottom)
                optTMDb.Checked = true;
            else if (optOMDb.Visible && m.X > optOMDb.Left && m.X < optOMDb.Right && m.Y > optOMDb.Top && m.Y < optOMDb.Bottom)
                optOMDb.Checked = true;

            return (Value != curr); // value changed
                
        }
    }
}
