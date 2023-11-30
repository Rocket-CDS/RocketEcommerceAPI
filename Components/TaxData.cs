using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class TaxData
    {
        public int CartTotalCents { set; get; }
        public int ShippingTotalCents { set; get; }
        //Calculated Tax
        public int TotalCents { set; get; }
        // Tax that needs to be added to the order total. (Tax may be inclued in price)
        public int TotalCentsAdd { set; get; }

    }
}
