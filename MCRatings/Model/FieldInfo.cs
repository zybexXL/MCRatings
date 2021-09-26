using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{
    // mapping and UI info for fields - JRating name, JRiver name, Display size, flags
    public class FieldInfo
    {
        public bool isJRField { get { return JRField != null; } }
        public string JRField;
        public string GridHeader;
        public bool Readonly;
        public int Width;
        public int Alignment;
        public Type dataType;

        public FieldInfo(string header, bool readOnly, int width, int aligment)
        {
            GridHeader = header;
            Readonly = readOnly;
            Width = width;
            Alignment = aligment;
            dataType = typeof(string);
        }

        public FieldInfo(string header, string jrField, bool readOnly, int width, int alignment)
            : this(header, readOnly, width, alignment)
        {
            JRField = jrField;
        }

        public FieldInfo(string header, bool readOnly, int width, int alignment, Type type)
            : this(header, readOnly, width, alignment)
        {
            dataType = type;
        }
    }
}
