using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DNNrocketAPI.Components
{
    public class ProductOptionLimpet
    {
        public ProductOptionLimpet(int portalId, SimplisityInfo info, int productId, string langRequired = "")
        {
            ProductId = productId;
            CultureCode = langRequired;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();
            PortalShop = new PortalShopLimpet(portalId, CultureCode);
            Info = info;
            if (Info == null) Info = new SimplisityInfo();
        }

        public SimplisityInfo Info { get; private set; }
        public string OptionKey
        {
            get
            {
                var rtn = Info.GetXmlProperty("genxml/hidden/optionkey");
                return rtn;
            }
        }
        public string Ref
        {
            get
            {
                return Info.GetXmlProperty("genxml/textbox/ref");
            }
            set
            {
                Info.SetXmlProperty("genxml/textbox/ref", value);
            }
        }
        public string Name
        {
            get
            {
                return Info.GetXmlProperty("genxml/lang/genxml/textbox/name");
            }
            set
            {
                Info.SetXmlProperty("genxml/lang/genxml/textbox/name", value);
            }
        }
        public void PriceSetValue(string price)
        {
            PriceCents = PortalShop.CurrencyConvertCents(price);
        }
        public int PriceCents
        {
            get { return Info.GetXmlPropertyInt("genxml/textbox/optionprice"); }
            set { Info.SetXmlPropertyInt("genxml/textbox/optionprice", value.ToString()); }
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
        public string ControlType
        {
            get
            {
                var rtn = Info.GetXmlProperty("genxml/dropdown/controltype");
                return rtn;
            }
        }
        public bool IsRequired
        {
            get
            {
                return Info.GetXmlPropertyBool("genxml/checkbox/required");
            }
        }
        public bool IsCheckBox
        {
            get
            {
                if (ControlType.ToLower() == "checkbox") return true;
                return false;
            }
        }
        public bool IsDropDown
        {
            get
            {
                if (ControlType.ToLower() == "dropdown") return true;
                return false;
            }
        }
        public bool IsRadioButton
        {
            get
            {
                if (ControlType.ToLower() == "radiobutton") return true;
                return false;
            }
        }
        public bool IsTextBox
        {
            get
            {
                if (ControlType.ToLower() == "textbox") return true;
                return false;
            }
        }

        #region "Option Fields"

        public List<ProductOptionField> GetOptionFields()
        {
            var rtn = new List<ProductOptionField>();
            var l = Info.GetList("optionfields");
            foreach (var f in l)
            {
                var pof = new ProductOptionField(PortalShop.PortalId,CultureCode, f);
                rtn.Add(pof);
            }
            return rtn;
        }
        public void AddOptionField()
        {
            var pof = new ProductOptionField(PortalShop.PortalId, CultureCode, new SimplisityInfo());
            AddOptionField(pof);
        }
        public void AddOptionField(ProductOptionField pof)
        {
            Info.AddListItem("optionfields", pof.Info);
        }
        public ProductOptionField GetOptionField(string fieldRef)
        {
            var l = GetOptionFields();
            foreach (var pof in l)
            {
                if (pof.Ref == fieldRef) return pof;
            }
            return null;
        }
        public void UpdateOptionField(ProductOptionField productOptionField)
        {
            Info.RemoveListItem("optionfields", "genxml/optionsfieldref", productOptionField.Ref);
            AddOptionField(productOptionField);
        }
        public void SaveOptionFields(List<SimplisityInfo> productOptionFieldList)
        {
            Info.RemoveList("optionfields");
            foreach (var f in productOptionFieldList)
            {
                var productOptionField = new ProductOptionField(PortalShop.PortalId, CultureCode, f);
                //only updated from form.  The currency convert will go wring otherwise.
                productOptionField.Info.SetXmlPropertyInt("genxml/optionsfieldprice", PortalShop.CurrencyConvertCents(productOptionField.Info.GetXmlProperty("genxml/optionsfieldprice")).ToString());
                UpdateOptionField(productOptionField);
            }
        }

        public void DeleteOptionField(string fieldRef)
        {
            var l = GetOptionFields();
            Info.RemoveList("optionfields");
            foreach (var pof in l)
            {
                if (pof.Ref != fieldRef) AddOptionField(pof);
            }
        }
        public Dictionary<string, string> GetOptionFieldDictionary(string template = "{value} {price}", string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = CultureCode;
            var rtn = new Dictionary<string, string>();
            var l = GetOptionFields();
            foreach (var pof in l)
            {
                if (!rtn.ContainsKey(pof.Ref) && pof.Ref != "")
                {
                    var temp = template.Replace("{ref}", pof.Ref);
                    temp = temp.Replace("{value}", pof.Value);
                    if (pof.PriceCents > 0)
                        temp = temp.Replace("{price}", pof.PriceDisplay);
                    else
                        temp = temp.Replace("{price}", "");

                    rtn.Add(pof.Ref, temp);
                }
            }
            return rtn;
        }


        #endregion

        public string CultureCode { get; private set; }
        public int ProductId { get; private set; }
        public PortalShopLimpet PortalShop { get; private set; }


    }
}
