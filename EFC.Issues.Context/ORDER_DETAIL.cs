using System;
using System.Collections.Generic;

namespace EFC.Issues.Context
{
    public partial class ORDER_DETAIL
    {
        public int ORDER_ID { get; set; }
        public string BILLING_TYPE { get; set; }
        public string CLIENT_ID { get; set; }
        public int? BILLING_CONTACT_ID { get; set; }
        public int? SHIPPING_CONTACT_ID { get; set; }

        public virtual CLIENT_CONTACT CLIENT_BILLING_CONTACT { get; set; }
        public virtual CLIENT_CONTACT CLIENT_SHIPPING_CONTACT { get; set; }
        public virtual ORDER ORDER_ { get; set; }
    }
}
