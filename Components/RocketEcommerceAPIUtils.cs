using DNNrocketAPI;
using DNNrocketAPI.Components;
using RazorEngine.Compilation.VisualBasic;
using RocketPortal.Components;
using Simplisity;
using Simplisity.TemplateEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public static class RocketEcommerceAPIUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketEcommerceAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources";

        public static ProductLimpet GetProductData(int portalId, int articleId, string cultureCode, bool useCache = true)
        {
            var cacheKey = "ProductLimpet*" + portalId + "*" + articleId + "*" + cultureCode;
            var groupId = "ecom" + portalId;
            var articleData = (ProductLimpet)CacheUtils.GetCache(cacheKey, groupId);
            if (articleData == null)
            {
                articleData = new ProductLimpet(portalId, articleId, cultureCode);
                if (articleId > 0) CacheUtils.SetCache(cacheKey, articleData, groupId);
                LogUtils.LogSystem("RocketDirectoryAPIUtils.GetArticleData: " + cacheKey);
            }
            return articleData;
        }
        public static Dictionary<string, QueryParamsData> UrlQueryParams(AppThemeLimpet appThemeView)
        {
            var rtn = new Dictionary<string, QueryParamsData>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, "");
                    foreach (var r in t.GetRecordList("queryparams"))
                    {
                        var queryParamsData = new QueryParamsData();
                        queryParamsData.queryparam = r.GetXmlProperty("genxml/queryparam");
                        queryParamsData.tablename = r.GetXmlProperty("genxml/tablename");
                        queryParamsData.systemkey = r.GetXmlProperty("genxml/systemkey");
                        queryParamsData.datatype = r.GetXmlProperty("genxml/datatype");
                        queryParamsData.queryparamvalue = "";
                        if (!rtn.ContainsKey(queryParamsData.queryparam)) rtn.Add(queryParamsData.queryparam, queryParamsData);
                    }
                }
            }
            return rtn;
        }
        public static Dictionary<string, MenuProviderData> MenuProvider(AppThemeLimpet appThemeView)
        {
            var rtn = new Dictionary<string, MenuProviderData>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, "");
                    foreach (var r in t.GetRecordList("menuprovider"))
                    {
                        var menoproviderData = new MenuProviderData();
                        menoproviderData.assembly = r.GetXmlProperty("genxml/assembly");
                        menoproviderData.namespaceclass = r.GetXmlProperty("genxml/namespaceclass");
                        menoproviderData.systemkey = r.GetXmlProperty("genxml/systemkey");
                        if (!rtn.ContainsKey(menoproviderData.systemkey)) rtn.Add(menoproviderData.systemkey, menoproviderData);
                    }
                }
            }
            return rtn;
        }
        public static Dictionary<string, string> ModuleTemples(AppThemeLimpet appThemeView, string moduleRef)
        {
            var rtn = new Dictionary<string,string>();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, moduleRef);
                    foreach (var r in t.GetRecordList("moduletemplates"))
                    {
                        if (!rtn.ContainsKey(r.GetXmlProperty("genxml/file"))) rtn.Add(r.GetXmlProperty("genxml/file"), r.GetXmlProperty("genxml/name"));
                    }
                }
            }
            return rtn;
        }
        public static SimplisityRecord GetSelectedModuleTemple(AppThemeLimpet appThemeView, string moduleRef, string templateFileName)
        {
            var rtn = new SimplisityRecord();
            if (appThemeView != null)
            {
                foreach (var tfile in appThemeView.GetTemplatesDep())
                {
                    var t = appThemeView.GetModT(tfile.Key, moduleRef);
                    foreach (var r in t.GetRecordList("moduletemplates"))
                    {
                        if (r.GetXmlProperty("genxml/file") == templateFileName) return r;
                    }
                }
            }
            return rtn;
        }
        public static List<SimplisityRecord> DependanciesList(int portalId, string moduleRef, SessionParams sessionParam)
        {
            var rtn = new List<SimplisityRecord>();
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, false);
            if (dataObject.AppTheme != null)
            {
                foreach (var depfile in dataObject.AppTheme.GetTemplatesDep())
                {
                    var dep = dataObject.AppTheme.GetDep(depfile.Key, moduleRef);
                    foreach (var r in dep.GetRecordList("deps"))
                    {
                        var urlstr = r.GetXmlProperty("genxml/url");
                        if (urlstr.Contains("{"))
                        {
                            if (dataObject.PortalData != null) urlstr = urlstr.Replace("{domainurl}", dataObject.PortalData.EngineUrlWithProtocol);
                            if (dataObject.AppTheme != null) urlstr = urlstr.Replace("{appthemefolder}", dataObject.AppTheme.AppThemeVersionFolderRel);
                            if (dataObject.AppThemeSystem != null) urlstr = urlstr.Replace("{appthemesystemfolder}", dataObject.AppThemeSystem.AppThemeVersionFolderRel);
                        }
                        r.SetXmlProperty("genxml/id", CacheUtils.Md5HashCalc(urlstr));
                        r.SetXmlProperty("genxml/url", urlstr);
                        rtn.Add(r);
                    }
                }
            }
            return rtn;
        }
        public static string AdminHeader(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template)
        {
            return ViewHeader(portalId, systemKey, moduleRef, sessionParam, template);
        }
        public static string ViewHeader(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template)
        {
            var moduleSettings = new ModuleContentLimpet(portalId, moduleRef, sessionParam.ModuleId, sessionParam.TabId);
            if (moduleSettings.DisableHeader) return "";

            var urlKey = RocketEcommerceAPIUtils.UrlQueryArticleKey(portalId, systemKey);
            var articleId = sessionParam.GetInt(urlKey);
            var cacheKey = moduleRef + "*" + articleId + "*" + template;
            var rtn = (string)CacheFileUtils.GetCache(portalId, cacheKey, "ecom" + portalId);
            if (rtn != null && !moduleSettings.DisableCache) return rtn;

            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, false);
            if (articleId > 0)
            {
                var articleData = new ProductLimpet(dataObject.PortalId, articleId, sessionParam.CultureCode);
                dataObject.SetDataObject("productdata", articleData);
            }
            var razorTempl = dataObject.AppTheme.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            CacheFileUtils.SetCache(portalId, cacheKey, pr.RenderedText, "ecom" + portalId);
            return pr.RenderedText;
        }
        private static int GetProductId(int portalId, string systemKey, SessionParams sessionParams)
        {
            var rtn = 0;
            var paramidList = DNNrocketUtils.GetQueryKeys(portalId);
            foreach (var p in paramidList)
            {
                if (p.Value.systemkey == systemKey && p.Value.datatype.ToLower() == "article") // use "article" to match config data
                {
                    rtn = sessionParams.GetInt(p.Key);
                    break;
                }
            }
            return rtn;
        }
        private static DataObjectLimpet ListData(DataObjectLimpet dataObject)
        {
            var sortorderkey = dataObject.ModuleSettings.GetSetting("sortorderkey");
            if (sortorderkey != "") dataObject.SessionParamsData.OrderByRef = sortorderkey; // use module setting if set.
            var defaultCat = dataObject.SessionCatId();
            if (defaultCat == 0) defaultCat = dataObject.ModuleSettings.DefaultCategoryId;
            if (defaultCat == 0) defaultCat = dataObject.ShopSettings.DefaultCategoryId;
            var articleDataList = new ProductLimpetList(dataObject.SessionParamsData, dataObject.PortalShop, dataObject.SessionParamsData.CultureCode, true, false, defaultCat);
            dataObject.SetDataObject("productlist", articleDataList);
            var categoryData = new CategoryLimpet(dataObject.PortalShop.PortalId, articleDataList.CategoryId, dataObject.SessionParamsData.CultureCode);
            dataObject.SetDataObject("categorydata", categoryData);
            return dataObject;
        }
        private static DataObjectLimpet DetailData(ProductLimpet productData, DataObjectLimpet dataObject)
        {
            if (productData.Exists)
            {
                dataObject.SetDataObject("productdata", productData);
                var articleCategoryData = new CategoryLimpet(dataObject.PortalShop.PortalId, productData.DefaultCategory(), dataObject.SessionParamsData.CultureCode);
                dataObject.SetDataObject("productcategorydata", articleCategoryData);
            }
            var catid = dataObject.SessionCatId();
            var categoryData = new CategoryLimpet(dataObject.PortalShop.PortalId, catid, dataObject.SessionParamsData.CultureCode);
            dataObject.SetDataObject("categorydata", categoryData);
            return dataObject;
        }
        public static string DisplayView(DataObjectLimpet dataObject, SimplisityInfo paramInfo = null, string template = "")
        {
            var sessionParam = dataObject.SessionParamsData;
            if (sessionParam.PageSize == 0) sessionParam.PageSize = dataObject.ModuleSettings.GetSettingInt("pagesize");

            // ------------------------------
            // CacheKey, with properties
            if (template == "") template = dataObject.ModuleSettings.GetSetting("displaytemplate");
            if (template == "") template = "view.cshtml";
            var cacheKey = template + "*" + dataObject.ModuleSettings.ModuleRef + "*" + sessionParam.UrlFriendly + "-" + sessionParam.OrderByRef + "-" + sessionParam.Page + "-" + sessionParam.PageSize;
            cacheKey += "-" + sessionParam.Get("rocketpropertyidtag");
            var nodList = sessionParam.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    cacheKey += "-" + nod.Name;
                    cacheKey += "-" + nod.InnerText;
                }
            }
            // ------------------------------

            if (sessionParam.SearchText == "" && !sessionParam.GetBool("disablecache"))
            {
                var rtn = (string)CacheUtils.GetCache(cacheKey, "ecom" + dataObject.PortalId);
                if (!String.IsNullOrEmpty(rtn) && !dataObject.ModuleSettings.DisableCache) return rtn;
            }
            var aticleId = GetProductId(dataObject.PortalId, dataObject.SystemKey, dataObject.SessionParamsData);
            var cmdType = dataObject.SessionParamsData.Get("cmdtype");
            if (cmdType == "") cmdType = "listdetail";
            var modt = RocketEcommerceAPIUtils.GetSelectedModuleTemple(dataObject.AppTheme, dataObject.ModuleSettings.ModuleRef, template);
            if (modt != null && modt.GetXmlProperty("genxml/cmd") != "") cmdType = modt.GetXmlProperty("genxml/cmd");

            LogUtils.LogSystem("START  DisplayView() cmdType: " + cmdType);

            if (cmdType == "list" || cmdType == "listdetail")
            {
                var articleData = RocketEcommerceAPIUtils.GetProductData(dataObject.PortalShop.PortalId, aticleId, dataObject.SessionParamsData.CultureCode);
                if (articleData.Exists)
                    dataObject = DetailData(articleData, dataObject);
                else
                    dataObject = ListData(dataObject);
            }
            if (cmdType == "listonly")
            {
                dataObject = ListData(dataObject);
            }
            if (cmdType == "detailonly")
            {
                var articleData = RocketEcommerceAPIUtils.GetProductData(dataObject.PortalShop.PortalId, aticleId, dataObject.SessionParamsData.CultureCode);
                dataObject = DetailData(articleData, dataObject);
            }
            if (cmdType == "satellite")
            {
                // load datalist, without populating it, for satelite modules.  use GetArticleList(ModuleContentLimpet moduleData, int maxReturn = 20) method.
                var articleDataList = new ProductLimpetList(sessionParam, dataObject.PortalShop, sessionParam.CultureCode, false, false);
                dataObject.SetDataObject("articlelist", articleDataList);
            }
            if (cmdType == "catmenu")
            {
                var defaultCat = dataObject.SessionCatId();
                if (defaultCat == 0) defaultCat = dataObject.ModuleSettings.DefaultCategoryId;
                if (defaultCat == 0) defaultCat = dataObject.ShopSettings.DefaultCategoryId;
                var categoryData = new CategoryLimpet(dataObject.PortalShop.PortalId, defaultCat, sessionParam.CultureCode);
                dataObject.SetDataObject("categorydata", categoryData);
            }
            if (cmdType == "minicart")
            {
            }
            if (cmdType == "checkout")
            {
            }
            if (cmdType == "pay")
            {
                var paymentid = sessionParam.GetInt("paymentid");
                if (paymentid > 0)
                {
                    var paymentData = new PaymentLimpet(dataObject.PortalId, paymentid, sessionParam.CultureCode);
                    if (paymentData.UserId == UserUtils.GetCurrentUserId() || UserUtils.IsManager())
                    {
                        dataObject.SetDataObject("paymentdata", paymentData);
                    }
                }
                var orderid = sessionParam.GetInt("orderid");
                if (orderid > 0)
                {
                    var orderData = new OrderLimpet(dataObject.PortalId, orderid, sessionParam.CultureCodeEdit);
                    if (orderData.UserId == UserUtils.GetCurrentUserId() || UserUtils.IsManager())
                    {
                        dataObject.SetDataObject("orderdata", orderData);
                    }
                }
            }

            // ----- RETURN ------------------------------------------------------------
            var rtncmd = sessionParam.Get("rtncmd");
            switch (rtncmd)
            {
                case "remote_cartbankreturn":
                    // get the interface and call the API.
                    var rocketInterface = new RocketInterface(dataObject.SystemData.SystemInfo, "remote");
                    var returnDictionary = DNNrocketUtils.GetProviderReturn(rtncmd, dataObject.SystemData.SystemInfo, rocketInterface, new SimplisityInfo(), paramInfo, dataObject.AppTheme.AppThemeVersionFolderRel, "");
                    if (returnDictionary.ContainsKey("outputhtml"))
                    {
                        return (string)returnDictionary["outputhtml"];
                    }
                    LogUtils.LogSystem("END  DisplayView() cmdType: " + cmdType + " - rtnCmd: remote_cartbankreturn");
                    break;
                default:
                    var razorTempl = dataObject.AppTheme.GetTemplate(template, dataObject.ModuleSettings.ModuleRef);
                    var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
                    if (pr.StatusCode != "00") return pr.ErrorMsg;
                    if (sessionParam.SearchText == "")
                    {
                        CacheUtils.SetCache(cacheKey, pr.RenderedText, "ecom" + dataObject.PortalId);
                    }
                    LogUtils.LogSystem("END  DisplayView() cmdType: " + cmdType);
                    return pr.RenderedText;
            }
            return "ERROR: DisplayView() cmdType: " + cmdType;
        }
        public static string DisplayView(int portalId, string moduleRef, SessionParams sessionParam, SimplisityInfo paramInfo = null, string template = "")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, false);
            return DisplayView(dataObject, paramInfo, template);
        }
        public static string DisplaySystemView(int portalId, string systemKey, string moduleRef, SessionParams sessionParam, string template, bool editMode = true)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, sessionParam, false);
            if (dataObject.AppThemeSystem == null) return "No System View";

            var razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
            if (razorTempl == "") razorTempl = dataObject.AppThemeSystem.GetTemplate(template, moduleRef);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, sessionParam, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString(ResourcePath, resourceKey, resourceExt, cultureCode);
        }        
        public static string TokenReplacementCultureCode(string str, string CultureCode)
        {
            if (CultureCode == "") return str;
            str = str.Replace("{culturecode}", CultureCode);
            var s = CultureCode.Split('-');
            if (s.Count() == 2)
            {
                str = str.Replace("{language}", s[0]);
                str = str.Replace("{country}", s[1]);
            }
            return str;
        }
        public static string UrlQueryKey(int portalId, string systemKey, string dataType)
        {
            var cacheKey = "UrlQueryKey*" + portalId + "*" + systemKey + "*" + dataType;
            var paramKey = (string)CacheUtils.GetCache(cacheKey, "ecom" + portalId);
            if (paramKey == null)
            {
                paramKey = "";
                var paramidList = DNNrocketUtils.GetQueryKeys(portalId);
                foreach (var paramDict in paramidList)
                {
                    if (systemKey == paramDict.Value.systemkey && paramDict.Value.datatype == dataType)
                    {
                        paramKey = paramDict.Value.queryparam;
                    }
                }
                CacheUtils.SetCache(cacheKey, paramKey, "ecom" + portalId);
            }
            return paramKey;
        }
        public static string UrlQueryCategoryKey(int portalId, string systemKey)
        {
            return UrlQueryKey(portalId, systemKey, "category");
        }
        public static string UrlQueryArticleKey(int portalId, string systemKey)
        {
            return UrlQueryKey(portalId, systemKey, "article");
        }


    }

}
