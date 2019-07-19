using System;
using System.Collections.Generic;
using System.Text;

namespace EFC.Issues.Test.Mapping
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public string ClientId { get; set; }
        public string BillingType { get; set; }
        public ClientContact BillingContact { get; set; }
        public ClientContact ShippingContact { get; set; }
    }
}
