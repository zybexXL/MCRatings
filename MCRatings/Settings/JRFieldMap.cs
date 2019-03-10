using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    // stores info about a JRiver field mapping
    [Serializable]
    public class JRFieldMap
    {
        public AppField field;
        public string JRfield;
        public bool enabled;
        public bool overwrite;
        public bool numeric;

        public JRFieldMap() { }   // for serialization

        public JRFieldMap(AppField _field, string _JRField, bool _enabled = true, bool _overwrite = true, bool _numeric = false)
        {
            field = _field;
            JRfield = _JRField;
            enabled = _enabled;
            overwrite = _overwrite;
            numeric = _numeric;
        }
    }

}
