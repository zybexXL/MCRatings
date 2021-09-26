using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZRatings
{

    [DataContract]
    public class OMDbRating
    {
        [DataMember] public string Source { get; set; }
        [DataMember] public string Value { get; set; }
    }
}
