using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    class SourceSelectColumn : DataGridViewColumn
    {
        public SourceSelectColumn() : base(new SourceSelectCell())
        {
        }

        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                // Ensure that the cell used for the template is a CalendarCell.
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(SourceSelectCell)))
                {
                    throw new InvalidCastException("SourceSelectColumn CellTemplate must be a SourceSelectCell");
                }
                base.CellTemplate = value;
            }
        }
    }
}
