using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using RazorEngine.Text;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Interfaces;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class RocketEcommerceAPITokens<T> : DNNrocketAPI.render.DNNrocketTokens<T>
    {
        // Define data classes, so we can use intellisense in inject templates
        public AppThemeLimpet appTheme;
        public AppThemeLimpet appThemeDefault;
        public AppThemeSystemLimpet appThemeSystem;
        public AppThemeSystemLimpet appThemeDirectory;
        public AppThemeLimpet appThemeDirectoryDefault;
        public ModuleContentLimpet moduleData;
        public SimplisityInfo moduleDataInfo;
        public PortalLimpet portalData;
        public List<string> enabledlanguages = DNNrocketUtils.GetCultureCodeList();
        public SessionParams sessionParams;
        public UserParams userParams;
        public SimplisityInfo info;
        public SimplisityInfo infoempty;
        public SystemLimpet systemData;
        public SystemLimpet systemDirectoryData;
        public CategoryLimpetList categoryDataList;
        public CategoryLimpet categoryData;
        public PropertyLimpetList propertyDataList;
        public PropertyLimpet propertyData;
        public AppThemeProjectLimpet appThemeProjects;
        public DefaultsLimpet defaultData;
        public SystemGlobalData globalSettings;
        public AppThemeDataList appThemeDataList;
        public CompanyLimpet companyData;
        public NotificationLimpet notificationData;
        public PortalShopLimpetStats portalStats;
        public CartLimpet cartData;
        public PortalShopLimpet portalShop;
        public ShopSettingsLimpet shopSettings;
        public ProductLimpet productData;
        public CategoryLimpet defaultCategory;
        public ProductLimpetList productList;

        public string AssigDataModel(SimplisityRazor sModel)
        {
            appTheme = (AppThemeLimpet)sModel.GetDataObject("appthemeview");
            appThemeDefault = (AppThemeLimpet)sModel.GetDataObject("appthemedefault");
            appThemeSystem = (AppThemeSystemLimpet)sModel.GetDataObject("appthemesystem");          
            appThemeProjects = (AppThemeProjectLimpet)sModel.GetDataObject("appthemeprojects");
            appThemeDataList = (AppThemeDataList)sModel.GetDataObject("appthemedatalist");
            portalShop = (PortalShopLimpet)sModel.GetDataObject("portalshop");
            systemData = (SystemLimpet)sModel.GetDataObject("systemdata");
            portalData = (PortalLimpet)sModel.GetDataObject("portaldata");
            shopSettings = (ShopSettingsLimpet)sModel.GetDataObject("shopsettings");
            moduleData = (ModuleContentLimpet)sModel.GetDataObject("modulesettings");
            var mRec = new SimplisityRecord();
            if (moduleData != null) mRec = moduleData.Record;
            moduleDataInfo = new SimplisityInfo(mRec);
            categoryDataList = (CategoryLimpetList)sModel.GetDataObject("categorylist");
            categoryData = (CategoryLimpet)sModel.GetDataObject("categorydata");
            propertyDataList = (PropertyLimpetList)sModel.GetDataObject("propertylist");
            propertyData = (PropertyLimpet)sModel.GetDataObject("propertydata");
            defaultData = (DefaultsLimpet)sModel.GetDataObject("defaultdata");
            globalSettings = (SystemGlobalData)sModel.GetDataObject("globalsettings");
            sessionParams = sModel.SessionParamsData;
            userParams = (UserParams)sModel.GetDataObject("userparams");
            companyData = (CompanyLimpet)sModel.GetDataObject("companydata");
            notificationData = (NotificationLimpet)sModel.GetDataObject("notificationdata");
            portalStats = (PortalShopLimpetStats)sModel.GetDataObject("portalstats");
            cartData = (CartLimpet)sModel.GetDataObject("cartdata");
            productData = (ProductLimpet)sModel.GetDataObject("productdata");
            productList = (ProductLimpetList)sModel.GetDataObject("productlist");
            defaultCategory = (CategoryLimpet)sModel.GetDataObject("defaultcategory");

            if (sessionParams == null) sessionParams = new SessionParams(new SimplisityInfo());
            info = new SimplisityInfo();
            if (productData != null) info = productData.Info;

            infoempty = new SimplisityInfo();

            AddProcessDataResx(appTheme, true);
            if (systemData != null) AddProcessData("resourcepath", systemData.SystemRelPath + "/App_LocalResources/");

            // use return of "string", so we don;t get error with converting void to object.
            return "";
        }
        /// <summary>
        /// Dropdown select for Tax values that can be added on a product.
        /// </summary>
        /// <param name="productData">The product data.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IEncodedString TaxDropDown(SimplisityInfo productInfo)
        {
            var taxProv = portalShop.GetActiveTaxProvider();
            if (taxProv == null)
            {
                var taxRange = new Dictionary<string, string>();
                if (taxRange.Count == 0) taxRange.Add("", "");
                return DropDownList(productInfo, "genxml/select/taxref", taxRange, "class='w3-input w3-border taxselect' ");
            }
            else
            {
                var taxRange = taxProv.GetTaxRange();
                if (taxRange.Count == 0) taxRange.Add("", "");
                return DropDownList(productInfo, "genxml/select/taxref", taxRange, "class='w3-input w3-border taxselect' ");
            }
        }

        public IEncodedString ModelDropdown(ProductLimpet productData, string id = "modelid")
        {
            return DropDownList(new SimplisityInfo(), "genxml/hidden/" + id + productData.ProductId, productData.GetModelDictionary(), "class='w3-input w3-border modelselect' ");
        }
        public IEncodedString OptionsInput(ProductLimpet productData, string checkBoxCSS = "w3-check", string dropDownCSS = "w3-select w3-border", string radioButtonCSS = "w3-radio w3-margin-left", string textBoxCSS = "w3-input w3-border", string labelCSS = "w3-margin-top", string id = "optionid")
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
                    var oDict = option.GetOptionFieldDictionary();
                    var defaultKey = "";
                    if (oDict.Count > 0) defaultKey = oDict.First().Key;
                    strOut += RadioButtonList(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, oDict, "class='option" + productData.ProductId + " ' optionkey='" + option.OptionKey + "' ", defaultKey, "",false,0,"", radioButtonCSS);
                }
                if (option.IsTextBox)
                {
                    var required = "";
                    if (option.IsRequired) required = "required";
                    strOut += "<div class='" + labelCSS + "'>" + option.Name + "</div>";
                    strOut += TextBox(new SimplisityInfo(), "genxml/" + id + productData.ProductId + lp, required + " name='" + id + productData.ProductId + lp + "'  class='option" + productData.ProductId + " rocketoptiontextbox " + textBoxCSS + "' optionkey='" + option.OptionKey + "' maxlength='100' ","",false);
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

    }
}
