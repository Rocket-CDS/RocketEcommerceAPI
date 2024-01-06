using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class CartItemOptionLimpet
    {
        public CartItemOptionLimpet(ProductOptionLimpet productOption, string optionKeyEncoded, string optionValueEncoded)
        {
            KeyEncoded = optionKeyEncoded;
            ValueEncoded = optionValueEncoded;
            ProductOptionData = productOption;
            Key = GeneralUtils.DeCode(KeyEncoded);
            SelectedRef = GeneralUtils.DeCode(ValueEncoded);
        }
        public int SelectCost
        {
            get
            {
                if (ProductOptionData.IsCheckBox)
                {
                    if (SelectedText == "true") return ProductOptionData.PriceCents;
                }
                if (ProductOptionData.IsDropDown || ProductOptionData.IsRadioButton)
                {
                    var rtnOptionPrice = ProductOptionData.PriceCents;
                    var selectList = ProductOptionData.GetOptionFields();
                    foreach (var pof in selectList)
                    {
                        if (pof.Ref == SelectedRef)
                        {
                            rtnOptionPrice += pof.PriceCents;
                        }
                    }
                    return rtnOptionPrice;
                }
                if (ProductOptionData.IsTextBox)
                {
                    if (SelectedText != "") return ProductOptionData.PriceCents;
                }
                return 0;
            }
        }
        public string SelectedText
        {
            get
            {
                if (ProductOptionData.IsCheckBox)
                {
                    if (SelectedRef.ToLower() == "true") return SelectedRef.ToLower();
                }
                if (ProductOptionData.IsDropDown || ProductOptionData.IsRadioButton)
                {
                    var selectList = ProductOptionData.GetOptionFieldDictionary();
                    if (selectList.ContainsKey(SelectedRef))
                    {
                        var rtn = selectList[SelectedRef];
                        if (ProductOptionData.PriceCents > 0) rtn += "  [+" + ProductOptionData.PriceDisplay() + "]";
                        return rtn;
                    }
                }
                if (ProductOptionData.IsTextBox)
                {
                    return SelectedRef;
                }
                return "";
            }
        }
        public string OptionRef { get { return ProductOptionData.Ref; } }
        public string OptionName { get {return ProductOptionData.Name;}  }
        public ProductOptionLimpet ProductOptionData { set; get; }
        public string KeyEncoded { set; get; }
        public string ValueEncoded { set; get; }
        public string Key { set; get; }
        public string SelectedRef { set; get; }
    }
}
