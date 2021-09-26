using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZRatings
{
    // this fixes the rendering of ToolStrip GUI element, removing the 1-pixel gray underline/border
    public class ToolStripBorderFix : ToolStripSystemRenderer
    {
        public ToolStripBorderFix() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // skip render border for main toolstrip component
            if (e.ToolStrip.GetType() != typeof(ToolStrip))
                base.OnRenderToolStripBorder(e);
        }
    }
}
