using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCRatings
{
    class SourceSelectCell : DataGridViewCell
    {
        public SourceSelectCell() : base()
        {

        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
            DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
            DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            using (var brush = new SolidBrush(cellStyle.BackColor))
                graphics.FillRectangle(brush, cellBounds);

            var ctrl = (SourceSelect)value;
            var img = new Bitmap(ctrl.Width, ctrl.Height);
            int offsetY = (cellBounds.Height - ctrl.Height) / 2;
            ctrl.DrawToBitmap(img, new Rectangle(0, 0, ctrl.Width, ctrl.Height));
            graphics.DrawImage(img, cellBounds.Location.X, cellBounds.Location.Y + offsetY);
            base.PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
        }

        public override Type ValueType => typeof(SourceSelect);

        //public override object DefaultNewRowValue => Sources.None;

        //public override Type EditType => typeof(SourceSelect);

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            var ctrl = Value as SourceSelect;
            bool changed = ctrl.OnClick(new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta));
            ctrl.Refresh();
            DataGridView.InvalidateCell(e.ColumnIndex, e.RowIndex);

            if (changed)
                RaiseCellValueChanged(new DataGridViewCellEventArgs(e.ColumnIndex, e.RowIndex));
        }
    }
}
