using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DNNrocketAPI.Components
{
    public class ProductModel
    {
        public ProductModel(int portalId, SimplisityInfo info, string langRequired)
        {
            CultureCode = langRequired;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();
            PortalShop = new PortalShopLimpet(portalId, CultureCode);
            Info = info;
            if (Info == null) Info = new SimplisityInfo();
            if (Info.GetXmlProperty("genxml/hidden/modelkey") == "")
            {
                var modelkey = GeneralUtils.GetUniqueString();
                Info.SetXmlProperty("genxml/hidden/modelkey", modelkey);
            }
            if (Info == null) Info = new SimplisityInfo();
            Weight = Info.GetXmlPropertyInt("genxml/textbox/weight"); // convert to int
        }

        public SimplisityInfo Info { get; private set; }
        public string ModelKey
        {
            get
            {
                var rtn = Info.GetXmlProperty("genxml/hidden/modelkey");
                return rtn;
            }
        }
        public string Ref
        {
            get
            {
                return Info.GetXmlProperty("genxml/textbox/modelref");
            }
            set
            {
                Info.SetXmlProperty("genxml/textbox/modelref", value);
            }
        }
        public string Name
        {
            get
            {
                return Info.GetXmlProperty("genxml/lang/genxml/textbox/modelname");
            }
            set
            {
                Info.SetXmlProperty("genxml/lang/genxml/textbox/modelname", value);
            }
        }
        public int Weight
        {
            get
            {
                return Info.GetXmlPropertyInt("genxml/textbox/weight");
            }
            set
            {
                Info.SetXmlPropertyInt("genxml/textbox/weight", value.ToString());
            }
        }
        public void PriceSetValue(string price)
        {
            PriceCents = PortalShop.CurrencyConvertCents(price);
        }
        public int PriceCents
        {
            get { return Info.GetXmlPropertyInt("genxml/textbox/modelprice"); }
            set { Info.SetXmlPropertyInt("genxml/textbox/modelprice", value.ToString()); }
        }
        public decimal Price
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceCents)); }
        }
        public string PriceDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return Price.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }

        public void SalePriceSetValue(string price)
        {
            SalePriceCents = PortalShop.CurrencyConvertCents(price);
        }
        public int SalePriceCents
        {
            get { return Info.GetXmlPropertyInt("genxml/textbox/modelsaleprice"); }
            set { Info.SetXmlPropertyInt("genxml/textbox/modelsaleprice", value.ToString()); }
        }
        public decimal SalePrice
        {
            get { return (PortalShop.CurrencyCentsToDollars(SalePriceCents)); }
        }
        public string SalePriceDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return SalePrice.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public int BestPriceCents
        {
            get
            {
                if (SalePriceCents > 0 && SalePriceCents < PriceCents)
                    return SalePriceCents;
                else
                    return PriceCents;
            }
        }
        public decimal BestPrice
        {
            get
            {
                if (SalePriceCents > 0 && SalePriceCents < PriceCents)
                    return SalePrice;
                else
                    return Price;
            }
        }
        public string BestPriceDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return BestPrice.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }


        public string CultureCode { get; private set; }
        public PortalShopLimpet PortalShop { get; private set; }


    }
}
