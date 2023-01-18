using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DNNrocketAPI.Components
{

    public class ProductOptionField
    {
        public ProductOptionField(int portalId, string cultureCode, SimplisityInfo info)
        {
            CultureCode = cultureCode;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();
            Info = info;
            PortalShop = new PortalShopLimpet(portalId, CultureCode);
            Info = info;
            if (Info == null) Info = new SimplisityInfo();

            if (Ref == "") Ref = GeneralUtils.GetGuidKey();
        }
        public void PriceSetValue(string price)
        {
            PriceCents = PortalShop.CurrencyConvertCents(price);
        }
        public string Ref { get { return Info.GetXmlProperty("genxml/optionsfieldref"); } set { Info.SetXmlProperty("genxml/optionsfieldref", value); } }
        public string Value { get { return Info.GetXmlProperty("genxml/lang/genxml/optionsfieldvalue"); } set { Info.SetXmlProperty("genxml/lang/genxml/optionsfieldvalue", value); } }
        public int PriceCents { get { return Info.GetXmlPropertyInt("genxml/optionsfieldprice"); } set { Info.SetXmlPropertyInt("genxml/optionsfieldprice", value.ToString()); } }
        public decimal Price { get { return PortalShop.CurrencyCentsToDollars(PriceCents); } }
        public string PriceDisplay { get { return PortalShop.CurrencyDisplay(Price); } }
        public string CultureCode { get; set; }
        public PortalShopLimpet PortalShop { get; set; }
        public SimplisityInfo Info { get; set; }
    }

}
