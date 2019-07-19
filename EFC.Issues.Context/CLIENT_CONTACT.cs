using System;
using System.Collections.Generic;

namespace EFC.Issues.Context
{
    public partial class CLIENT_CONTACT
    {
        public CLIENT_CONTACT()
        {
            ORDER_DETAIL_AS_BILLING_CONTACT = new HashSet<ORDER_DETAIL>();
            ORDER_DETAIL_AS_SHIPPING_CONTACT = new HashSet<ORDER_DETAIL>();
        }

        public string CLIENT_ID { get; set; }
        public int CONTACT_ID { get; set; }
        public string CONTACT_NAME { get; set; }
        public string CONTACT_EMAIL { get; set; }
        public string CONTACT_PHONE { get; set; }

        public virtual CLIENT CLIENT_ { get; set; }
        public virtual ICollection<ORDER_DETAIL> ORDER_DETAIL_AS_BILLING_CONTACT { get; set; }
        public virtual ICollection<ORDER_DETAIL> ORDER_DETAIL_AS_SHIPPING_CONTACT { get; set; }
    }
}
