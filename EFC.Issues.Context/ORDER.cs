using System;
using System.Collections.Generic;

namespace EFC.Issues.Context
{
    public partial class ORDER
    {
        public int ORDER_ID { get; set; }
        public string CLIENT_ID { get; set; }
        public DateTime CREATION_DATE { get; set; }

        public virtual CLIENT CLIENT_ { get; set; }
        public virtual ORDER_DETAIL ORDER_DETAIL { get; set; }
    }
}
