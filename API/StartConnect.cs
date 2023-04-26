using DNNrocketAPI.Components;
using DNNrocketAPI.Interfaces;
using Newtonsoft.Json;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect : IProcessCommand
    {
        private const string _systemkey = "rocketecommerceapi";

        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private SystemLimpet _systemData;
        private PortalShopLimpet _portalShop;
        private Dictionary<string, string> _passSettings;
        private SessionParams _sessionParams;
        private UserParams _userParams;
        private AppThemeSystemLimpet _appThemeSystem;
        private string _moduleRef;
        private AppThemeLimpet _appTheme;
        private AppThemeLimpet _appThemeDefault;
        private RemoteModule _remoteModule;
        private Dictionary<string, object> _dataObjects;
        private PortalLimpet _portalData;
        private string _org;
        private AppThemeProjectLimpet _orgData;
        private CompanyLimpet _companyData;
        private ShopSettingsLimpet _shopSettings;
        private int _defaultCategoryId;

        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var jsonOut = "";
            var storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketsystem_edit":
                    strOut = RocketSystem();
                    break;
                case "rocketsystem_init":
                    strOut = RocketSystemInit();
                    break;
                case "rocketsystem_delete":
                    strOut = RocketSystemDelete();
                    break;
                case "rocketsystem_save":
                    SavePortalShop();
                    strOut = RocketSystem();
                    break;                    


                case "dashboard_get":
                    strOut = GetDashboard();
                    break;
                case "dashboard_clearallcache":
                    CacheFileUtils.ClearFileCacheAllPortals();
                    CacheUtils.ClearAllCache();
                    CacheUtils.ClearAllCache();
                    DNNrocketUtils.ClearAllCache();
                    strOut = "OK";
                    break;
                case "rocketecommerceapi_adminpanel":
                    strOut = AdminPanel();
                    break;
                case "dashboard_paymethodsort":
                    strOut = PayMethodSort();
                    break;
                case "dashboard_shipmethodsort":
                    strOut = ShipMethodSort();
                    break;


                case "portalshops_save":
                    SavePortalShop();
                    strOut = RocketSystem();
                    break;
                case "portalshops_delete":
                    DeletePortalShop();
                    strOut = RocketSystem();
                    break;
                case "portalshops_addsetting":
                    _portalShop.Record.AddListItem("settingsdata");
                    _portalShop.Update();
                    strOut = RocketSystem();
                    break;
                case "portalshops_reset":
                    ResetPortalShop(_paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"));
                    strOut = RocketSystem();
                    break;
                case "portalshops_validateshop":
                    strOut = ValidateShop();
                    break;
                case "portalshops_country":
                    strOut = CountrySettings();
                    break;
                case "portalshops_calcstats":
                    strOut = CalculateStats();
                    break;
                case "portalshops_copylanguage":
                    strOut = CopyLanguage();
                    break;
                    


                case "productadmin_editproductlist":
                    strOut = GetProductList();
                    break;
                case "productadmin_productsearch":
                    strOut = GetProductList();
                    break;
                case "productadmin_editproduct":
                    strOut = GetProduct(_paramInfo.GetXmlPropertyInt("genxml/hidden/productid"));
                    break;
                case "productadmin_addproduct":
                    strOut = AddArticle();
                    break;
                case "productadmin_savedata":
                    strOut = GetProduct(SaveArticle());
                    break;
                case "productadmin_delete":
                    DeleteArticle();
                    strOut = GetProductList();
                    break;
                case "productadmin_copy":
                    CopyArticle();
                    strOut = GetProductList();
                    break;
                case "productadmin_addimage":
                    strOut = AddArticleImage();
                    break;
                case "productadmin_addimage64":
                    strOut = AddArticleImage64();
                    break;
                case "productadmin_adddoc":
                    strOut = AddProductDoc();
                    break;
                case "productadmin_addlink":
                    strOut = AddArticleLink();
                    break;
                case "productadmin_addmodel":
                    strOut = AddArticleModel();
                    break;
                case "productadmin_addoption":
                    strOut = AddArticleOption();
                    break;
                case "productadmin_docupload":
                    ProductDocumentUploadToFolder();
                    strOut = GetProduct(_paramInfo.GetXmlPropertyInt("genxml/hidden/productid"));
                    break;
                case "productadmin_docdelete":
                    ProductDeleteDocument();
                    strOut = GetProduct(_paramInfo.GetXmlPropertyInt("genxml/hidden/productid"));
                    break;
                case "productadmin_docselectlist":
                    strOut = ProductDocumentList();
                    break;
                case "productadmin_assigncategory":
                    strOut = AssignProductCategory();
                    break;
                case "productadmin_removecategory":
                    strOut = RemoveProductCategory();
                    break;
                case "productadmin_getoptionsfield":
                    strOut = GetOptionsField();
                    break;
                case "productadmin_saveoptionsfield":
                    strOut = SaveOptionsField();
                    break;
                case "productadmin_addoptionsfield":
                    strOut = AddOptionsField();
                    break;
                case "productadmin_assignproperty":
                    strOut = AssignArticleProperty();
                    break;
                case "productadmin_removeproperty":
                    strOut = RemoveArticleProperty();
                    break;
                case "productadmin_assigndefaultcategory":
                    strOut = AssignDefaultProductCategory();
                    break;




                case "categoryadmin_add":
                    strOut = AddCategory();
                    break;
                case "categoryadmin_editlist":
                    strOut = GetCategoryList();
                    break;
                case "categoryadmin_edit":
                    strOut = GetCategory(_paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid"));
                    break;
                case "categoryadmin_delete":
                    strOut = DeleteCategory();
                    break;
                case "categoryadmin_save":
                    strOut = GetCategory(SaveCategory());
                    break;
                case "categoryadmin_addimage":
                    strOut = AddCategoryImage();
                    break;
                case "categoryadmin_move":
                    strOut = MoveCategory();
                    break;
                case "categoryadmin_assignparent":
                    strOut = AssignParentCategory();
                    break;
                case "categoryadmin_removearticle":
                    strOut = RemoveCategoryArticle();
                    break;
                case "categoryadmin_assigndefault":
                    strOut = AssignDefaultCategory();
                    break;


                case "propertyadmin_add":
                    strOut = AddProperty();
                    break;
                case "propertyadmin_editlist":
                    strOut = GetPropertyList();
                    break;
                case "propertyadmin_edit":
                    strOut = GetProperty(_paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid"));
                    break;
                case "propertyadmin_delete":
                    strOut = DeleteProperty();
                    break;
                case "propertyadmin_save":
                    strOut = SaveProperty();
                    break;
                case "propertyadmin_removeproduct":
                    strOut = RemovePropertyArticle();
                    break;


                case "company_edit":
                    strOut = CompanyEdit();
                    break;
                case "company_save":
                    strOut = CompanySave();
                    break;
                case "company_addimage":
                    strOut = AddCompanyImage();
                    break;


                case "notification_edit":
                    strOut = NotificationEdit();
                    break;
                case "notification_save":
                    strOut = NotificationSave();
                    break;
                case "notification_reset":
                    strOut = NotificationReset();
                    break;


                case "orderadmin_editlist":
                    strOut = GetOrderList("orderlist.cshtml");
                    break;
                case "orderadmin_search":
                    strOut = GetOrderList("orderlist.cshtml");
                    break;
                case "orderadmin_editorder":
                    var orderId = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                    strOut = GetOrder("orderdetail.cshtml", orderId);
                    break;
                case "orderadmin_addorder":
                    strOut = AddOrder("orderdetail.cshtml");
                    break;
                case "orderadmin_savedata":
                    var oid = SaveOrder();
                    strOut = GetOrder("orderdetail.cshtml", oid);
                    break;
                case "orderadmin_delete":
                    DeleteOrder();
                    strOut = GetOrderList("orderlist.cshtml");
                    break;
                case "orderadmin_articleselectlist":
                    strOut = GetProductSelectList();
                    break;
                case "orderadmin_articleselectdetail":
                    strOut = GetProductSelectDetail();
                    break;
                case "orderadmin_addorderarticle":
                    AddOrderCartItem();
                    strOut = ""; // close model after added.
                    break;
                case "orderadmin_removearticle":
                    var orderid = RemoveOrderCartItem();
                    strOut = GetOrder("orderdetail.cshtml", orderid);
                    break;
                case "orderadmin_printorder":
                    strOut = PrintOrder();
                    break;
                case "orderadmin_printshiplabel":
                    strOut = PrintShipLabel();
                    break;
                case "orderadmin_sendemail":
                    strOut = SendOrderEmail();
                    break;



                case "paymentadmin_editlist":
                    strOut = GetPaymentList();
                    break;
                case "paymentadmin_search":
                    strOut = GetPaymentList();
                    break;
                case "paymentadmin_editpayment":
                    strOut = GetPaymentDisplay(_paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid"));
                    break;
                case "paymentadmin_addpayment":
                    strOut = AddPayment();
                    break;
                case "paymentadmin_savedata":
                    int paymentid = SavePayment();
                    strOut = GetPaymentDisplay(paymentid);
                    break;
                case "paymentadmin_paymentemail":
                    SavePayment();
                    strOut = SendPaymentEmail();
                    break;
                case "paymentadmin_receivedemail":
                    SavePayment();
                    strOut = SendPaymentClientEmail();
                    break;
                case "paymentadmin_delete":
                    DeletePayment();
                    strOut = GetPaymentList();
                    break;
                case "paymentadmin_removehistory":
                    strOut = RemovePaymentHistory();
                    break;
                case "paymentadmin_testrequestemail":
                    strOut = SendPaymentEmail();
                    break;
                case "paymentadmin_testmanageremail":
                    strOut = SendPaymentManagerEmail();
                    break;
                case "paymentadmin_testclientemail":
                    strOut = SendPaymentClientEmail();
                    break;


                case "settingcountry_save":
                    CountryUtils.CountrySave(postInfo);
                    strOut = CountryUtils.CountrySettings(_systemkey, true, _sessionParams);
                    break;
                case "settingcountry_get":
                    strOut = CountryUtils.CountrySettings(_systemkey, false, _sessionParams);
                    break;
                case "settingcountry_getregion":
                    rtnDic.Add("outputhtml", "");
                    var regionlist = CountryUtils.RegionListJson(paramInfo.GetXmlProperty("genxml/hidden/activevalue"), paramInfo.GetXmlPropertyBool("genxml/hidden/allowempty"));
                    rtnDic.Add("outputjson", regionlist);
                    break;
                case "settingcountry_selectculturecode":
                    rtnDic.Add("outputhtml", CountryUtils.CultureSelect(_systemkey, _sessionParams));
                    break;


                case "paymentmethods_list":
                    strOut = GetPaymentMethodsList();
                    break;
                case "shippingmethods_list":
                    strOut = GetShippingMethodsList();
                    break;
                case "taxmethods_list":
                    strOut = GetTaxMethodsList();
                    break;
                case "discountmethods_list":
                    strOut = GetDiscountMethodsList();
                    break;


                case "remote_login":
                    strOut = Components.LocalUtils.ReloadPage(_portalData.PortalId);
                    break;
                case "remote_productlist":
                    strOut = GetPublicProductList();
                    break;
                case "remote_productlistseo":
                    strOut = "";
                    break;
                case "remote_productlistheader":
                    strOut = GetPublicProductListHeader("header");
                    break;
                case "remote_productlistbeforeheader":
                    strOut = GetPublicProductListHeader("beforeheader");
                    break;
                case "remote_productdetail":
                    strOut = GetPublicProductDetail();
                    break;
                case "remote_productdetailseo":
                    strOut = "";
                    break;

                case "remote_payment":
                    strOut = GetRemotePaymentDisplay();
                    break;
                case "remote_paymentheader":
                    strOut = GetRemotePaymentHeader();
                    break;


                case "remote_pay": // deprecreated: use "remote_publicpay"
                    strOut = RedirectPaymentFormToBank();
                    break;
                case "remote_publicpay":
                    strOut = RedirectPaymentFormToBank();
                    break;
                case "remote_payreset": 
                    strOut = PaymentFormReset();
                    break;
                case "remote_publicnotify":
                    strOut = BankIPN();
                    break;


                case "remote_remotefail":
                    strOut = "Invalid Remote Connection - Check Remote Security Key";
                    break;



                case "rocketecommerceapi_minicartqty":
                    var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                    strOut = cartData.QtyCount.ToString();
                    break;
                case "rocketecommerceapi_minicarttotal":
                    var cartData2 = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                    strOut = cartData2.TotalDisplay;
                    break;
                case "rocketecommerceapi_minicartjson":
                    rtnDic.Add("outputhtml", "");
                    rtnDic.Add("outputjson", GetMiniCartJson());
                    break;
                case "rocketecommerceapi_minicart":
                    strOut = GetMiniCartDisplay();
                    break;
                case "rocketecommerceapi_addtocart":
                    strOut = AddToCart();
                    break;
                case "rocketecommerceapi_addtocartjson":
                    rtnDic.Add("outputhtml", "");
                    rtnDic.Add("outputjson", AddToCart("json"));
                    break;
                case "rocketecommerceapi_removecartitem":
                    RemoveCartItem();
                    strOut = GetPublicCartList();
                    break;
                case "rocketecommerceapi_cartdelete":
                    strOut = GetPublicCartDelete();
                    break;
                case "rocketecommerceapi_cartlist":
                    strOut = GetPublicCartList();
                    break;
                case "rocketecommerceapi_cartdetails":
                    strOut = GetPublicCartContact();
                    break;
                case "rocketecommerceapi_cartvalidate":
                    strOut = ValidateCart();
                    break;
                case "rocketecommerceapi_savecartlist":
                    strOut = SaveCartList();
                    break;
                case "rocketecommerceapi_savecartcontact":
                    strOut = SaveCartContact();
                    break;
                case "rocketecommerceapi_savecartbill":
                    strOut = SaveCartBill();
                    break;
                case "rocketecommerceapi_savecartship":
                    strOut = SaveCartShip();
                    break;
                case "rocketecommerceapi_cartcontact":
                    strOut = GetPublicCartContact();
                    break;
                case "rocketecommerceapi_cartbill":
                    strOut = GetPublicCartBillAddress();
                    break;
                case "rocketecommerceapi_cartship":
                    strOut = GetPublicCartShipAddress();
                    break;
                case "rocketecommerceapi_cartsummary":
                    strOut = GetPublicCartSummary();
                    break;
                case "rocketecommerceapi_cartpayment":
                    strOut = GetPublicCartPayment();
                    break;
                case "rocketecommerceapi_paymentoptions":
                    strOut = GetPublicPaymentOptions();
                    break;
                case "rocketecommerceapi_savecart":
                    strOut = SaveCartDetails();
                    break;

                case "settingsadmin_edit":
                    strOut = GetCatalogSettings();
                    break;
                case "settingsadmin_delete":
                    strOut = DeleteCatalogSettings();
                    break;
                case "settingsadmin_save":
                    strOut = SaveCatalogSettings();
                    break;


                case "remote_cartbankreturn":
                    strOut = CartBankReturnDisplay();
                    break;
                case "remote_cartbankreturnheader":
                    strOut = GetPublicProductListHeader("header");
                    break;
                case "remote_editoption":
                    strOut = "false";
                    break;
                case "remote_settings":
                    strOut = RemoteSettings();
                    break;
                case "remote_settingssave":
                    strOut = SaveSettings();
                    break;
                case "remote_clearsettings":
                    strOut = ClearSettings();
                    break;                    
                case "remote_getappthemeversions":
                    strOut = AppThemeVersions();
                    break;


                case "remote_publicview":
                    strOut = GetPublicProductView();
                    break;
            }

            if (paramCmd == "remote_publicview" || paramCmd == "remote_publicmenu")
            {
                rtnDic.Add("remote-seoheader", GetPublicArticleSEO());
                rtnDic.Add("remote-firstheader", GetPublicProductListHeader("beforeheader"));
                rtnDic.Add("remote-lastheader", GetPublicProductListHeader("header"));        
                rtnDic.Add("remote-cache", "True");
            }
            _remoteModule.AppThemeFolder = _remoteModule.AppThemeViewFolder; // We do not have a edit AppTheme in the module. (Part of hte catalog edit)
            _remoteModule.AppThemeVersion = _remoteModule.AppThemeViewVersion;
            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _remoteModule.Record.ToXmlItem());
            // if we have a searchtext we do not want to cache.
            if (_sessionParams.SearchText != "") rtnDic.Remove("remote-cache");

            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }
        private String RocketSystem()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("RocketSystem.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String RocketSystemInit()
        {
            try
            {
                var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
                if (newportalId > 0)
                {
                    _portalShop = new PortalShopLimpet(newportalId, _sessionParams.CultureCodeEdit);
                    _portalShop.Validate();
                    _portalShop.Update();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String RocketSystemDelete()
        {
            try
            {
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                if (portalId > 0)
                {
                    _portalShop = new PortalShopLimpet(portalId, _sessionParams.CultureCodeEdit);
                    _portalShop.Delete();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String RocketSystemValid()
        {
            try
            {
                var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                if (portalId > 0)
                {
                    _portalShop = new PortalShopLimpet(portalId, _sessionParams.CultureCodeEdit);
                    if (!_portalShop.ValidShop)
                    {
                        var razorTempl = _appThemeSystem.GetTemplate("InvalidSystem.cshtml");
                        var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                        if (pr.StatusCode != "00") return pr.ErrorMsg;
                        return pr.RenderedText;

                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #region "General Functionas"
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _systemData = new SystemLimpet(_systemkey);
            _appThemeSystem = new AppThemeSystemLimpet(PortalUtils.GetPortalId(), _systemData.SystemKey);
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _userParams = new UserParams(_sessionParams.BrowserSessionId);
            _passSettings = new Dictionary<string, string>();
            _orgData = new AppThemeProjectLimpet();
            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            if (_moduleRef == "") _moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");

            // Assign Langauge
            DNNrocketUtils.SetCurrentCulture();
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            var rtnCultureCode = _sessionParams.CultureCodeEdit;
            if (paramCmd.StartsWith("remote_")) rtnCultureCode = _sessionParams.CultureCode;

            // Users should only have access to their own services.
            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid >= 0 && PortalUtils.GetPortalId() == 0)
            {
                _remoteModule = new RemoteModule(portalid, _moduleRef);
                _portalShop = new PortalShopLimpet(portalid, rtnCultureCode); // Portal 0 is admin, editing portal setup
            }
            else
            {
                _remoteModule = new RemoteModule(PortalUtils.GetPortalId(), _moduleRef);
                _portalShop = new PortalShopLimpet(PortalUtils.GetPortalId(), rtnCultureCode); // editing current portal
                if (!_portalShop.Active) return "";
            }

            _portalData = new PortalLimpet(_portalShop.PortalId);
            _appThemeDefault = new AppThemeLimpet(_portalShop.PortalId, _systemData, "Default", "1.0");
            _shopSettings = new ShopSettingsLimpet(_portalShop.PortalId, rtnCultureCode);

            // If the module is not on the display page, we need to create alink to the detail page.  This setting providers the new page url.
            var detailpageurl = _remoteModule.Record.GetXmlProperty("genxml/remote/detailpageurl" + _sessionParams.CultureCode);
            if (detailpageurl != "") _sessionParams.PageDetailUrl = detailpageurl;
            var listpageurl = _remoteModule.Record.GetXmlProperty("genxml/remote/listpageurl" + _sessionParams.CultureCode);
            if (listpageurl != "") _sessionParams.PageListUrl = listpageurl;

            //deaful pagesize
            if (_sessionParams.PageSize == 0) _paramInfo.SetXmlPropertyInt("genxml/hidden/pagesize", _remoteModule.Record.GetXmlPropertyInt("genxml/remote/pagesize"));

            // set default category
            _defaultCategoryId = _remoteModule.Record.GetXmlPropertyInt("genxml/remote/categoryid");
            if (_defaultCategoryId <= 0) _defaultCategoryId = _shopSettings.DefaultCategoryId;

            _org = _remoteModule.ProjectName;
            if (_org == "") _org = _orgData.DefaultProjectName();
            if (_postInfo.GetXmlProperty("genxml/select/selectedproject") != "") _portalShop.ProjectName = _postInfo.GetXmlProperty("genxml/select/selectedproject");
            if (_portalShop.ProjectName == "") _portalShop.ProjectName = _orgData.DefaultProjectName();

            _appTheme = new AppThemeLimpet(_portalShop.PortalId, _remoteModule.AppThemeViewFolder, _remoteModule.AppThemeViewVersion, _org);
            _companyData = new CompanyLimpet(_portalShop.PortalId, rtnCultureCode);
            var securityData = new SecurityLimpet(_portalShop.PortalId, _systemData.SystemKey, _rocketInterface, -1, -1);

            _dataObjects = new Dictionary<string, object>();
            _dataObjects.Add("remotemodule", _remoteModule);
            _dataObjects.Add("apptheme", _appTheme);
            _dataObjects.Add("appthemesystem", _appThemeSystem);
            _dataObjects.Add("appthemedefault", _appThemeDefault);
            _dataObjects.Add("portalshop", _portalShop);
            _dataObjects.Add("portaldata", _portalData);
            _dataObjects.Add("shopsettings", _shopSettings);
            _dataObjects.Add("systemdata", _systemData);
            _dataObjects.Add("companydata", _companyData);
            _dataObjects.Add("securitydata", securityData);
            var categoryDataList = new CategoryLimpetList(_portalShop.PortalId, rtnCultureCode, true, _remoteModule);
            _dataObjects.Add("categorylist", categoryDataList);
            var propertyList = new PropertyLimpetList(_portalShop.PortalId, _sessionParams, rtnCultureCode, _remoteModule);
            _dataObjects.Add("propertylist", propertyList);
            _dataObjects.Add("notificationdata",new NotificationLimpet(_portalShop.PortalId, _sessionParams.CultureCode));

            // if we have a remote_view, find the required cmd from module settings.
            if (paramCmd.StartsWith("remote_"))
            {
                var sk = _paramInfo.GetXmlProperty("genxml/remote/securitykey");
                if (!UserUtils.IsEditor() && !paramCmd.StartsWith("remote_public"))
                {
                    if (paramCmd == "remote_pay") return paramCmd; // legacy cmd
                    if (_portalData.SecurityKey != sk) paramCmd = "";
                }
                else
                {
                    var convertCmd = _paramInfo.GetXmlProperty("genxml/remote/urlparams/cmd"); // use the remote URL param "cmd" if it exists.
                    if (convertCmd == "") convertCmd = _remoteModule.Record.GetXmlProperty("genxml/select/cmd"); // use remote selected cmd.
                    if (paramCmd == "remote_publicview") paramCmd = convertCmd;
                    if (paramCmd == "remote_publicviewheader") paramCmd = convertCmd + "header";
                    if (paramCmd == "remote_publicviewbeforeheader") paramCmd = convertCmd + "beforeheader";
                    if (paramCmd == "remote_publicseo") paramCmd = convertCmd + "seo";                    
                }
            }
            else
            {
                paramCmd = securityData.HasSecurityAccess(paramCmd, "remote_login");
            }

            return paramCmd;
        }

        private String GetDashboard()
        {
            try
            {
                var portalStats = new PortalShopLimpetStats(_portalShop);
                _dataObjects.Add("portalstats", portalStats);
                var razorTempl = _appThemeSystem.GetTemplate("Dashboard.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string AdminPanel()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("AdminPanel.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        #endregion

        #region "Remote Helpers"
        private string AssignRemoteBeforeHeaderTemplate(string templateName = "")
        {
            return AssignRemoteTemplate("beforeheader", templateName);
        }
        private string AssignRemoteHeaderTemplate(string templateName = "")
        {
            return AssignRemoteTemplate("header", templateName);
        }
        private string AssignRemoteTemplate(string appendix = "", string templateName = "")
        {
            var template = templateName;
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/remote/template");
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/hidden/remotetemplate");
            if (template == "") template = _paramInfo.GetXmlProperty("genxml/remote/remotetemplate");
            if (template == "") template = _remoteModule.Record.GetXmlProperty("genxml/hidden/template");
            if (template == "") template = "view.cshtml";
            var templateRtn = _appTheme.GetTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template));
            if (templateRtn == "") templateRtn = _appThemeDefault.GetTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template));
            if (templateRtn == "") templateRtn = _appThemeSystem.GetTemplate(Path.GetFileNameWithoutExtension(template) + appendix + Path.GetExtension(template));
            return templateRtn;
        }

        #endregion

    }
}
