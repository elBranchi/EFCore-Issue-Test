using System;
using System.Collections.Generic;

namespace EFC.Issues.Context
{
    public partial class CLIENT
    {
        public CLIENT()
        {
            CLIENT_CONTACT = new HashSet<CLIENT_CONTACT>();
            ORDER = new HashSet<ORDER>();
        }

        public string CLIENT_ID { get; set; }
        public string CLIENT_NAME { get; set; }

        public virtual ICollection<CLIENT_CONTACT> CLIENT_CONTACT { get; set; }
        public virtual ICollection<ORDER> ORDER { get; set; }
    }
}
