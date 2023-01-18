using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class RocketEcommerceAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {

        public IEncodedString RenderRocketEcommerceAPITemplate(string templateName, PortalShopLimpet portalShop, SimplisityRazor model)
        {
            var appThemeSystem = new AppThemeSystemLimpet(portalShop.PortalId, "rocketecommerceapi");
            return RenderTemplate(templateName, appThemeSystem, model, true);
        }
        public IEncodedString ModelDropdown(ProductLimpet productData, string id = "modelid")
        {
            return DropDownList(new SimplisityInfo(), "genxml/hidden/" + id + productData.ProductId, productData.GetModelDictionary(), "class='w3-input w3-border modelselect' ");
        }
        public IEncodedString OptionsInput(ProductLimpet productData, string checkBoxCSS = "w3-check", string dropDownCSS = "w3-select w3-border", string radioButtonCSS = "w3-input", string textBoxCSS = "w3-input w3-border", string labelCSS = "w3-margin-top", string id = "optionid")
        {
            var strOut = "";
            var lp = 1;

            foreach (var option in productData.GetOptions())
            {
                strOut += "<div>";

                if (option.IsCheckBox)
                {
                    strOut += CheckBox(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, option.Name, "class='option" + productData.ProductId + " " + checkBoxCSS + "' optionkey='" + option.OptionKey + "' ",false,false);
                }
                if (option.IsDropDown)
                {
                    strOut += "<div class='" + labelCSS + "'>" + option.Name + "</div>";
                    strOut += DropDownList(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, option.GetOptionFieldDictionary(), "class='option" + productData.ProductId + " " + dropDownCSS + "' optionkey='" + option.OptionKey + "' ","",false);
                }
                if (option.IsRadioButton)
                {
                    strOut += "<div class='" + labelCSS + "'>" + option.Name + "</div>";
                    strOut += RadioButtonList(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, option.GetOptionFieldDictionary(), "class='option" + productData.ProductId + " " + radioButtonCSS + "' optionkey='" + option.OptionKey + "' ","","",false);
                }
                if (option.IsTextBox)
                {
                    strOut += "<div class='" + labelCSS + "'>" + option.Name + "</div>";
                    strOut += TextBox(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, "class='option" + productData.ProductId + " " + textBoxCSS + "' optionkey='" + option.OptionKey + "' maxlength='100' ","",false);
                }

                strOut += HiddenField(new SimplisityInfo(), "genxml/optionkey" + productData.ProductId + lp, "", option.OptionKey, false, lp ); // we need the optionkey for adding to cart.

                strOut += "</div>";
                lp += 1;
            }
            return new RawString(strOut);
        }

        public IEncodedString TextBoxMoney(int portalId, string cultureCode, SimplisityInfo info, String xpath, String attributes = "", String defaultValue = "", bool localized = false, int row = 0, string listname = "", string type = "text")
        {
            if (info == null) info = new SimplisityInfo();
            var value = info.GetXmlPropertyInt(xpath);
            if (localized && !xpath.StartsWith("genxml/lang/"))
            {
                value = info.GetXmlPropertyInt("genxml/lang/" + xpath);
            }
            var upd = getUpdateAttr(xpath, attributes, localized);
            var id = getIdFromXpath(xpath, row, listname);

            // [TODO: add encrypt option.]
            //var value = encrypted ? NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentPortalSettings().GUID.ToString(), info.GetXmlProperty(xpath)) : info.GetXmlProperty(xpath);
            if (value == 0 && GeneralUtils.IsNumeric(defaultValue)) value = Convert.ToInt32(defaultValue);

            var typeattr = "type='" + type + "'";
            if (attributes.ToLower().Contains(" type=")) typeattr = "";

            var portalShop = new PortalShopLimpet(portalId, cultureCode);

            var strOut = "<input value='" + portalShop.CurrencyEdit(value) + "' id='" + id + "' s-xpath='" + xpath + "' " + attributes + " " + upd + " " + typeattr + " />";

            return new RawString(strOut);
        }
        public IEncodedString MaterialIcon(string iconname, string cssclass = "", string attributes = "" )
        {
            return new RawString("<span class='material-icons " + cssclass + "' " + attributes + ">" + iconname + "</span>");
        }
        public IEncodedString RenderHandleBarsRE(Dictionary<string, SimplisityInfo> dataObjects, AppThemeLimpet appTheme, string templateName, string moduleref = "", string cacheKey = "")
        {
            var strOut = "";
            if (cacheKey != "") strOut = (string)CacheUtils.GetCache(moduleref + cacheKey, "hbs");
            if (String.IsNullOrEmpty(strOut))
            {
                string jsonString = SimplisityUtils.ConvertToJson(dataObjects);
                var template = appTheme.GetTemplate(templateName, moduleref);
                JObject model = JObject.Parse(jsonString);
                HandlebarsEngineRE hbEngine = new HandlebarsEngineRE();
                strOut = hbEngine.ExecuteRE(template, model);
                if (cacheKey != "") CacheUtils.SetCache(moduleref + cacheKey, strOut, "hbs");
            }
            return new RawString(strOut);
        }

    }
}
