using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Interfaces;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RocketEcommerceAPI.Components
{
    public class PortalShopLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "PortalShop";
        private const string _systemkey = "rocketecommerceapi";
        private DNNrocketController _objCtrl;
        private int _portalId;
        private string _cacheKey;
        private string _cacheGroup;
        public PortalShopLimpet(int portalId, string cultureCode)
        {
            Record = new SimplisityRecord();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            _portalId = portalId;
            _objCtrl = new DNNrocketController();


            if (PortalUtils.PortalExists(portalId))
            {
                // Need to populate, not in cache.
                DocFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(_portalId).TrimEnd('/') + "/rocketecommerceapi/docs";
                DocFolderMapPath = DNNrocketUtils.MapPath(DocFolderRel);
                ImageFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(_portalId).TrimEnd('/') + "/rocketecommerceapi/images";
                ImageFolderMapPath = DNNrocketUtils.MapPath(ImageFolderRel);
                ShopFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(_portalId).TrimEnd('/') + "/rocketecommerceapi";
                ShopFolderMapPath = DNNrocketUtils.MapPath(ShopFolderRel);
                if (!Directory.Exists(ShopFolderMapPath)) Directory.CreateDirectory(ShopFolderMapPath);
                if (!Directory.Exists(ImageFolderMapPath)) Directory.CreateDirectory(ImageFolderMapPath);
                if (!Directory.Exists(DocFolderMapPath)) Directory.CreateDirectory(DocFolderMapPath);
            }

            _cacheKey = "PortalShop" + _portalId + "*" + cultureCode;
            _cacheGroup = "ecom" + portalId.ToString();
            Record = (SimplisityRecord)CacheUtils.GetCache(_cacheKey, _cacheGroup);
            if (Record == null)
            {
                ReadRecord(portalId, cultureCode);
            }

        }

        public void Save(SimplisityInfo info)
        {
            Record.XMLData = info.XMLData;

            var checkouttype = Record.GetXmlPropertyInt("genxml/checkouttype");
            if (checkouttype < 3)
            {
                Record.SetXmlProperty("genxml/checkouttemplate", checkouttype + "_CartDetails.cshtml");
            }

            UpdateCurrencyCode(CurrencyCultureCode);
            Update();
        }
        public void UpdateCurrencyCode(string currencyCultureCode)
        {
            CurrencyCultureCode = currencyCultureCode;
            //currency data
            var cultureInfo = new CultureInfo(CurrencyCultureCode, false);
            NumberFormatInfo nfi = cultureInfo.NumberFormat;
            CurrencyDecimalDigits = nfi.CurrencyDecimalDigits;
            CurrencyDecimalSeparator = nfi.CurrencyDecimalSeparator;
            CurrencyGroupSeparator = nfi.CurrencyGroupSeparator;
            CurrencySymbol = nfi.CurrencySymbol;
            var ri = new RegionInfo(cultureInfo.LCID);
            CurrencyCode = ri.ISOCurrencySymbol;
        }
        public void Update()
        {
            // check for SQL injection
            var sqlInject = false;
            foreach (var s in GetOrderByList())
            {
                if (SecurityInput.CheckForSQLInjection(s.GetXmlProperty("genxml/value")))
                {
                    sqlInject = true;
                    break;
                }
            }
            if (!sqlInject)
            {
                sqlInject = SecurityInput.CheckForSQLInjection(Record.GetXmlProperty("genxml/sqlfilterproduct"));
                if (!sqlInject) sqlInject = SecurityInput.CheckForSQLInjection(Record.GetXmlProperty("genxml/sqlfilterorder"));
                if (!sqlInject) sqlInject = SecurityInput.CheckForSQLInjection(Record.GetXmlProperty("genxml/sqlfilterpayment"));
                if (!sqlInject)
                {
                    Record = _objCtrl.SaveRecord(Record, _tableName); // you must cache what comes back.  that is the copy of the DB.
                    CacheUtils.SetCache(_cacheKey, Record, _cacheGroup);
                }
            }
            if (sqlInject)
            {
                LogUtils.LogSystem("SQL INJECTION Attempt:" + Record.XMLData);
                ReadRecord(_portalId, Record.Lang);
            }
            // update PL settings for QueryParams
            if (AppThemeFolder != "")
            {
                var info = _objCtrl.GetRecordByGuidKey(_portalId, -1, "PLSETTINGS", "PLSETTINGS");
                if (info == null)
                {
                    info = new SimplisityRecord();
                    info.ItemID = -1;
                    info.PortalId = _portalId;
                    info.TypeCode = "PLSETTINGS";
                    info.GUIDKey = "PLSETTINGS";
                }
                if (info != null)
                {
                    var upd = false;
                    var appTheme = new AppThemeLimpet(PortalId, AppThemeFolder, AppThemeVersion, ProjectName);

                    // Add Query Params for Article and Categories
                    var newqueryParamList = new List<SimplisityRecord>();
                    foreach (var qdata in RocketEcommerceAPIUtils.UrlQueryParams(appTheme))
                    {
                        if (info.GetRecordListItem("queryparams", "genxml/textbox/queryparam", qdata.Key) == null)
                        {
                            var qRec = new SimplisityRecord();
                            qRec.SetXmlProperty("genxml/select/tablename", qdata.Value.tablename);
                            qRec.SetXmlProperty("genxml/select/datatype", qdata.Value.datatype);
                            qRec.SetXmlProperty("genxml/textbox/queryparam", qdata.Value.queryparam);
                            qRec.SetXmlProperty("genxml/textbox/systemkey", qdata.Value.systemkey);
                            newqueryParamList.Add(qRec);
                            upd = true;
                        }
                    }
                    if (upd)
                    {
                        // Remove duplicate Query Params
                        foreach (var newq in newqueryParamList)
                        {
                            var idx = 0;
                            foreach (var q in info.GetRecordList("queryparams"))
                            {
                                if (q.GetXmlProperty("genxml/select/datatype") == newq.GetXmlProperty("genxml/select/datatype") && q.GetXmlProperty("genxml/select/systemkey") == newq.GetXmlProperty("genxml/select/systemkey"))
                                {
                                    info.RemoveRecordListItem("queryparams", idx);
                                    break; // should only be 1.
                                }
                                idx += 1;
                            }
                        }
                        foreach (var newq in newqueryParamList)
                        {
                            info.AddRecordListItem("queryparams", newq);
                        }
                    }
                    // Add Menu Provider
                    foreach (var menuproviderData in RocketEcommerceAPIUtils.MenuProvider(appTheme))
                    {
                        if (info.GetRecordListItem("menuprovider", "genxml/textbox/systemkey", menuproviderData.Key) == null)
                        {
                            var mRec = new SimplisityRecord();
                            mRec.SetXmlProperty("genxml/textbox/assembly", menuproviderData.Value.assembly);
                            mRec.SetXmlProperty("genxml/textbox/namespaceclass", menuproviderData.Value.namespaceclass);
                            mRec.SetXmlProperty("genxml/textbox/systemkey", menuproviderData.Value.systemkey);
                            info.AddRecordListItem("menuprovider", mRec);
                            upd = true;
                        }
                    }

                    if (upd) _objCtrl.Update(info);
                }
            }



        }
        private void ReadRecord(int portalId, string cultureCode)
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            Record = _objCtrl.GetRecordByType(portalId, -1, _entityTypeCode, "", "", _tableName);
            if (Record == null || Record.ItemID <= 0)
            {
                Record = new SimplisityInfo();

                var fName = "defaultportalshop.rules";
                var configRel = "/DesktopModules/DNNrocketModules/RocketEcommerceAPI/Config/" + fName;
                var configMapPath = DNNrocketUtils.MapPath(configRel);
                if (File.Exists(configMapPath))
                {
                    var configXML = FileUtils.ReadFile(configMapPath);
                    Record.FromXmlItem(configXML);
                }

                Record.PortalId = portalId;
                Record.ModuleId = -1;
                Record.TypeCode = _entityTypeCode;
                Record.Lang = cultureCode;

                if (PortalUtils.PortalExists(portalId)) // check we have a portal, could be deleted
                {
                    // create folder on first load.
                    PortalUtils.CreateRocketDirectories(_portalId);
                    if (!Directory.Exists(PortalUtils.HomeDNNrocketDirectoryMapPath(_portalId))) Directory.CreateDirectory(PortalUtils.HomeDNNrocketDirectoryMapPath(_portalId));
                }
            }
            CacheUtils.SetCache(_cacheKey, Record, _cacheGroup);
        }
        public void Validate()
        {
            // check for existing page on portal for this system
            var tabid = PagesUtils.CreatePage(_portalId, _systemkey);
            PagesUtils.AddPagePermissions(_portalId, tabid, DNNrocketRoles.Manager);
            PagesUtils.AddPagePermissions(_portalId, tabid, DNNrocketRoles.Editor);
            PagesUtils.AddPagePermissions(_portalId, tabid, DNNrocketRoles.Collaborator);
            PagesUtils.AddPageSkin(_portalId, tabid, "rocketportal", "rocketadmin.ascx");
        }
        public void Delete()
        {
            var sql = " delete from " + _tableName + " where portalid = " + _portalId + " ";
            _objCtrl.ExecSql(sql);
            RemoveCache();
        }
        public void RemoveCache()
        {
            CacheUtils.RemoveCache(_cacheKey, _cacheGroup);
        }
        private string SqlFilterProduct { get { return Record.GetXmlProperty("genxml/sqlfilterproduct"); } }
        private string GetFilterSQL(string SqlFilterTemplate, SimplisityInfo paramInfo)
        {
            FastReplacer fr = new FastReplacer("{", "}", false);
            fr.Append(SqlFilterTemplate);
            var tokenList = fr.GetTokenStrings();
            var nosearchText = true;
            foreach (var token in tokenList)
            {
                var tok = "r/" + token;
                var tokenText = paramInfo.GetXmlProperty(tok).Replace("'", "''");
                if (tokenText != "") nosearchText = false;
                fr.Replace("{" + token + "}", tokenText);
            }
            if (nosearchText) return "";
            var filtersql = " " + fr.ToString() + " ";
            return filtersql;
        }
        public string GetFilterProductSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterProduct, paramInfo);
        }
        public string GetFilterPaymentSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterPayment, paramInfo);
        }
        public string GetFilterOrderSQL(SimplisityInfo paramInfo)
        {
            return GetFilterSQL(SqlFilterOrder, paramInfo);
        }

        public bool IsValidRemote(string securityKey)
        {
            if (Record.GetXmlProperty("genxml/securitykey") == securityKey) return true;
            return false;
        }
        private string SqlFilterOrder { get { return Record.GetXmlProperty("genxml/sqlfilterorder"); } }
        private string SqlFilterPayment { get { return Record.GetXmlProperty("genxml/sqlfilterpayment"); } }
        public string EntityTypeCode { get { return _entityTypeCode; } }

        public List<string> GetValidCultureCodes()
        {
            return DNNrocketUtils.GetCultureCodeList(_portalId);
        }
        public bool IsPaymentProviderActive(string interfaceKey)
        {
            var i = Record.GetRecordListItem("paymentprovidermethod", "genxml/hidden/paymentmethodkey", interfaceKey);
            if (i != null) return i.GetXmlPropertyBool("genxml/checkbox/active");
            return false;
        }
        public List<PaymentInterface> GetAllPaymentMethods()
        {
            var cacheKey = "GetAllPaymentMethods" + _systemkey + _portalId;
            var rtn = (List<PaymentInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;

            var systemData = new SystemLimpet(_systemkey);
            rtn = new List<PaymentInterface>();
            foreach (var provkey in Record.GetRecordList("paymentprovidermethod"))
            {
                var rocketInterface = systemData.GetProvider(provkey.GetXmlProperty("genxml/hidden/paymentmethodkey"));
                if (rocketInterface != null)
                {
                    var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    rtn.Add(bankprov);
                }
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public List<PaymentInterface> GetActivePaymentMethods()
        {
            var cacheKey = "GetActivePaymentMethods" + _systemkey + _portalId;
            var rtn = (List<PaymentInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;

            rtn = new List<PaymentInterface>();
            foreach (var p in GetAllPaymentMethods())
            {
                if (p.Active() && IsPaymentProviderActive(p.PaymentProvKey())) rtn.Add(p);
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public bool IsShippingProviderActive(string interfaceKey)
        {
            var i = Record.GetRecordListItem("shippingprovidermethod", "genxml/hidden/shippingmethodkey", interfaceKey);
            if (i != null) return i.GetXmlPropertyBool("genxml/checkbox/active");
            return false;
        }
        public List<ShippingInterface> GetAllShippingProviders()
        {
            var cacheKey = "GetAllShippingProviders" + _systemkey + _portalId;
            var rtn = (List<ShippingInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;
            var systemData = new SystemLimpet(_systemkey);
            rtn = new List<ShippingInterface>();
            foreach (var provkey in Record.GetRecordList("shippingprovidermethod"))
            {
                var rocketInterface = systemData.GetProvider(provkey.GetXmlProperty("genxml/hidden/shippingmethodkey"));
                if (rocketInterface != null)
                {
                    try
                    {
                        var prov = ShippingInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                        rtn.Add(prov);
                    }
                    catch (Exception ex)
                    {
                        // we might not have the assembly in the bin folder, report and ignore.
                        LogUtils.LogException(ex);
                    }
                }
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public List<ShippingInterface> GetActiveShippingProviders()
        {
            var cacheKey = "GetActiveShippingProviders" + _systemkey + _portalId;
            var rtn = (List<ShippingInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;

            rtn = new List<ShippingInterface>();
            foreach (var p in GetAllShippingProviders())
            {
                if (p.Active() && IsShippingProviderActive(p.ShipProvKey())) rtn.Add(p);
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public bool IsTaxProviderActive(string interfaceKey)
        {
            var i = Record.GetRecordListItem("taxprovidermethod", "genxml/hidden/taxmethodkey", interfaceKey);
            if (i != null) return i.GetXmlPropertyBool("genxml/checkbox/active");
            return false;
        }
        public List<TaxInterface> GetAllTaxProviders(bool useCache = true)
        {
            var cacheKey = "GetAllTaxMethods" + _systemkey + _portalId;
            var rtn = (List<TaxInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null && useCache) return rtn;

            var systemData = new SystemLimpet(_systemkey);
            rtn = new List<TaxInterface>();
            foreach (var provkey in Record.GetRecordList("taxprovidermethod"))
            {
                var rocketInterface = systemData.GetProvider(provkey.GetXmlProperty("genxml/hidden/taxmethodkey"));
                if (rocketInterface != null)
                {
                    var prov = TaxInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    rtn.Add(prov);
                }
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public TaxInterface GetActiveTaxProvider()
        {
            var cacheKey = "GetActiveTaxProvider" + _systemkey + _portalId;
            var rtn = (TaxInterface)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;

            foreach (var p in GetAllTaxProviders())
            {
                if (p.Active())
                {
                    CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
                    return p; // Only 1 tax provider active.
                }
            }
            return null;
        }
        #region "Discount"
        public bool IsDiscountProviderActive(string interfaceKey)
        {
            var i = Record.GetRecordListItem("discountprovidermethod", "genxml/hidden/discountmethodkey", interfaceKey);
            if (i != null) return i.GetXmlPropertyBool("genxml/checkbox/active");
            return false;
        }
        public List<DiscountInterface> GetAllDiscountProviders(bool useCache = true)
        {
            var cacheKey = "GetAllDiscountMethods" + _systemkey + _portalId;
            var rtn = (List<DiscountInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null && useCache) return rtn;

            var systemData = new SystemLimpet(_systemkey);
            rtn = new List<DiscountInterface>();
            foreach (var provkey in Record.GetRecordList("discountprovidermethod"))
            {
                var rocketInterface = systemData.GetProvider(provkey.GetXmlProperty("genxml/hidden/discountmethodkey"));
                if (rocketInterface != null)
                {
                    var prov = DiscountInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    rtn.Add(prov);
                }
            }
            CacheUtils.SetCache(cacheKey, rtn, _cacheGroup);
            return rtn;
        }
        public List<DiscountInterface> GetActiveDiscountProvider()
        {
            var cacheKey = "GetActiveDiscountProvider" + _systemkey + _portalId;
            var rtn = (List<DiscountInterface>)CacheUtils.GetCache(cacheKey, _cacheGroup);
            if (rtn != null) return rtn;

            rtn = new List<DiscountInterface>();
            foreach (var p in GetAllDiscountProviders())
            {
                if (p.Active())
                {
                    rtn.Add(p);
                }
            }
            return rtn;
        }
        #endregion

        #region "orderby"
        public List<SimplisityRecord> GetProductOrderByList()
        {
            return Record.GetRecordList("sqlorderbyproduct");
        }
        public string OrderByProductSQL(string orderbyRef = "")
        {
            var rtnsql = GetProductOrderBy(orderbyRef);
            if (rtnsql == "") rtnsql = GetProductOrderBy(0);
            return rtnsql;
        }
        public List<SimplisityRecord> GetOrderByList()
        {
            return Record.GetRecordList("sqlorderbyproduct");
        }
        public void AddProductOrderBy()
        {
            if (Record.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Record.AddListItem("sqlorderbyproduct");
            Update();
        }
        public string GetProductOrderBy(int idx)
        {
            var info = Record.GetRecordListItem("sqlorderbyproduct", idx);
            if (info == null) return "";
            return info.GetXmlProperty("genxml/value");
        }
        public string GetProductOrderBy(string orderbyref)
        {
            var info = Record.GetRecordListItem("sqlorderbyproduct", "genxml/key", orderbyref);
            if (info == null) return "";
            return info.GetXmlProperty("genxml/value");
        }
        #endregion

        #region "setting"
        public String GetSetting(String key, String defaultValue = "")
        {
            if (Settings != null && Settings.ContainsKey(key)) return Settings[key];
            return defaultValue;
        }
        public Boolean GetSettingBool(String key, Boolean defaultValue = false)
        {
            try
            {
                if (Settings == null) Settings = new Dictionary<String, String>();
                if (Settings.ContainsKey(key))
                {
                    var x = Settings[key];
                    // bool usually stored as "True" "False"
                    if (x.ToLower() == "true") return true;
                    // Test for 1 as true also.
                    if (GeneralUtils.IsNumeric(x))
                    {
                        if (Convert.ToInt32(x) > 0) return true;
                    }
                    return false;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                return defaultValue;
            }
        }
        private Dictionary<string, string> GetSettings()
        {
            var rtn = new Dictionary<string, string>();
            foreach (var s in Record.GetRecordList("settingsdata"))
            {
                var key = s.GetXmlProperty("genxml/textbox/key");
                if (key != "" && !rtn.ContainsKey(key))
                {
                    rtn.Add(key, s.GetXmlProperty("genxml/textbox/value"));
                }
            }
            return rtn;
        }

        #endregion

        public int ArticleCount
        {
            get
            {
                var countInt = 0;
                var cacheCount = CacheUtils.GetCache("ArticleCount" + _portalId, _cacheGroup);
                if (cacheCount == null)
                {
                    var l = _objCtrl.GetList(_portalId, -1, "ART", "", CultureCode, "", 0, 0, 0, 0, _tableName);
                    countInt = l.Count;
                    CacheUtils.SetCache("ArticleCountCount" + _portalId, countInt.ToString(), _cacheGroup);
                }
                else
                {
                    countInt = (int)cacheCount;
                }
                return countInt;
            }
        }
        public bool IsAdminMenuTurnOn(string interfaceKey)
        {
            if (Record.GetXmlProperty("genxml/activeadminmenu/chk[@data='" + interfaceKey + "']/@value") == "true")
            {
                return true;
            }
            return false;
        }
        public bool IsProductDataSectionOn(ProductDataSection productDataSection)
        {
            var x = Convert.ToInt32(productDataSection).ToString();
            if (Record.GetXmlProperty("genxml/productdata/chk[@data='" + x + "']/@value") == "true")
            {
                return true;
            }
            return false;
        }

        private bool IsNumeric(string value)
        {
            int i = 0;
            return int.TryParse(value, out i); //i now = value 
        }
        public int CurrencyConvertCents(string value)
        {
            var rtn = CurrencyConvertToCulture(value);
            var rtnStr = Regex.Replace(rtn.ToString(), "[^0-9]", ""); // remove ALL non numeric
            if (IsNumeric(rtnStr)) return Convert.ToInt32(rtnStr);
            return 0;
        }
        public string CurrencyEdit(int intValue)
        {
            return CurrencyCentsToDollars(intValue).ToString();
        }
        public string CurrencyDisplay(decimal value)
        {
            return value.ToString("C", CultureInfo.GetCultureInfo(CurrencyCultureCode));
        }
        public decimal CurrencyCentsToDollars(int cents)
        {
            // DCL: Unsure why we make everything positive, I think it was a mistake.  
            //var minus = false;
            //if (cents < 0) minus = true;

            var multiplyer = "1";
            var lp = 0;
            while (lp < CurrencyDecimalDigits)
            {
                multiplyer += "0";
                lp += 1;
            }
            var rtn = Convert.ToDecimal(cents) / Convert.ToDecimal(multiplyer);
            //if (minus) rtn = (rtn * -1);
            return CurrencyConvertToCulture(rtn.ToString());
        }
        public decimal CurrencyConvertToCulture(string value)
        {
            // Reformat the amount, to ensure it is a valid currency. 
            // very often the entered decimal seperator is the group seperator.
            // so we convert to try and help, but may still be wrong.  
            // We remove all non-numeric and then enter the decimal seperator at the correct place for the shop currencyculturecode.
            // !!! There is probably a better way to do this !!!
            var minus = false;
            if (value.TrimStart(' ').StartsWith("-")) minus = true;
            if (IsNumeric(value))
            {
                // FIX: 78  --> 78.00
                // no decimal seperator, pad the string to allow for that.
                string padzero = new String('0', CurrencyDecimalDigits);
                value = value + padzero;
            }
            else
            {
                // FIX: 78.3  --> 78.30
                // find out how many decimal numbers after seperator.
                // We do not know what the sepeartor is, so take the last non-numeric as the seperator. (Reverse loop, so first in code.)
                var seperatorCount = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsNumber(value[i])) seperatorCount = (value.Length - i) - 1;
                }
                if (seperatorCount < CurrencyDecimalDigits)
                {
                    value = value.PadRight(value.Length + (CurrencyDecimalDigits - seperatorCount), '0');
                }
            }
            value = Regex.Replace(value, "[^0-9]", ""); // remove ALL non numeric
            if (value.Length < (CurrencyDecimalDigits + 1)) value = value.PadRight((CurrencyDecimalDigits + 1), '0');
            var newamount = "";
            for (int i = 0; i < value.Length; i++)
            {
                if ((value.Length - i) == (CurrencyDecimalDigits))
                {
                    newamount += CurrencyDecimalSeparator;
                }
                newamount += value[i];
            }
            var rtn = Convert.ToDecimal(newamount, CultureInfo.GetCultureInfo(CurrencyCultureCode));
            if (minus) rtn = (rtn * -1);
            return rtn;
        }

        /// <summary>
        /// This is used to populate settings for remote plugins/module.
        /// The base4 code contains the settings required.
        /// </summary>
        /// <returns></returns>
        public string RemoteBase64Params()
        {
            var portalData = new PortalLimpet(_portalId);
            var remoteParams = new RemoteParams();
            remoteParams.EngineURL = portalData.EngineUrlWithProtocol;
            remoteParams.SecurityKey = portalData.SecurityKey;
            return remoteParams.RecordItemBase64;
        }

        public void ClearPortalCache()
        {
            CacheUtils.ClearAllCache(_portalId.ToString());
        }

        #region "Info - PortalCatalog Data"
        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } }
        public SimplisityRecord Record { get; set; }
        public int ProductLimit { get { return Record.GetXmlPropertyInt("genxml/maxproducts"); } }
        public int ProductImageLimit { get { return Record.GetXmlPropertyInt("genxml/productimagelimit"); } }
        public int ProductModelLimit { get { return Record.GetXmlPropertyInt("genxml/productmodellimit"); } }
        public int ProductOptionLimit { get { return Record.GetXmlPropertyInt("genxml/productoptionlimit"); } }
        public int ProductDocumentLimit { get { return Record.GetXmlPropertyInt("genxml/productdocumentlimit"); } }
        public int ProductLinkLimit { get { return Record.GetXmlPropertyInt("genxml/productlinklimit"); } }
        public int ProductCount
        {
            get
            {
                var countInt = 0;
                var cacheCount = CacheUtils.GetCache("ProductCount" + _portalId);
                if (cacheCount == null)
                {
                    var l = _objCtrl.GetList(_portalId, -1, "PRD", "", CultureCode, "", 0, 0, 0, 0, _tableName);
                    countInt = l.Count;
                    CacheUtils.GetCache("ProductCount" + _portalId, countInt.ToString());
                }
                else
                {
                    countInt = (int)cacheCount;
                }
                return countInt;
            }
        }
        public string WebsiteUrl { get { return Record.GetXmlProperty("genxml/merchant/websiteurl"); } set { Record.SetXmlProperty("genxml/merchant/websiteurl", value.ToString()); } }
        public string CurrencyCultureCode
        {
            get
            {
                var rtn = Record.GetXmlProperty("genxml/currencyculturecode");
                if (rtn == "") rtn = DNNrocketUtils.GetCurrentCulture();
                return rtn;
            }
            set { Record.SetXmlProperty("genxml/currencyculturecode", value.ToString()); }
        }
        public int CurrencyDecimalDigits { get { return Record.GetXmlPropertyInt("genxml/currencydecimaldigits"); } set { Record.SetXmlProperty("genxml/currencydecimaldigits", value.ToString()); } }
        public string CurrencyDecimalSeparator { get { return Record.GetXmlProperty("genxml/currencydecimalseparator"); } set { Record.SetXmlProperty("genxml/currencydecimalseparator", value.ToString()); } }
        public string CurrencyGroupSeparator { get { return Record.GetXmlProperty("genxml/currencygroupseparator"); } set { Record.SetXmlProperty("genxml/currencygroupseparator", value.ToString()); } }
        public string CurrencySymbol { get { return Record.GetXmlProperty("genxml/currencysymbol"); } set { Record.SetXmlProperty("genxml/currencysymbol", value.ToString()); } }
        public string CurrencyCode { get { return Record.GetXmlProperty("genxml/currencycode"); } set { Record.SetXmlProperty("genxml/currencycode", value.ToString()); } }
        public int PortalId { get { return _portalId; } }
        public bool Exists { get { if (Record.ItemID > 0) return true; else return false; } }
        public string CultureCode { get { return Record.Lang; } }
        public bool ValidShop { get { if (Record.GetXmlProperty("genxml/maxproducts") != "") return true; else return false; } }
        public DateTime LastSchedulerTime
        {
            get
            {
                if (Record.GetXmlPropertyDate("genxml/lastschedulertime") > DateTime.MinValue)
                    return Record.GetXmlPropertyDate("genxml/lastschedulertime");
                else
                    return DateTime.Now.AddDays(-10);
            }
            set { Record.SetXmlProperty("genxml/lastschedulertime", value.ToString("O"), TypeCode.DateTime); }
        }
        public int SchedulerRunHours
        {
            get
            {
                var rtn = Record.GetXmlPropertyInt("genxml/chedulerrunhours");
                if (Record.GetXmlProperty("genxml/schedulerrunhours") == "") rtn = 24;
                return rtn;
            }
        }
        public string SiteKey { get { return Record.GUIDKey; } set { Record.GUIDKey = value; } }
        public Dictionary<string, string> Settings { get; private set; }
        public string ShopFolderRel { get; set; }
        public string ShopFolderMapPath { get; set; }
        public string ImageFolderRel { get; set; }
        public string ImageFolderMapPath { get; set; }
        public string DocFolderRel { get; set; }
        public string DocFolderMapPath { get; set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string SecurityKey { get { return Record.GetXmlProperty("genxml/securitykey"); } }
        public string LogoRelPath { get { return Record.GetXmlProperty("genxml/hidden/imagepathlogo"); } set { Record.SetXmlProperty("genxml/hidden/imagepathlogo", value); } }
        public bool Active { get { return Record.GetXmlPropertyBool("genxml/active"); } set { Record.SetXmlProperty("genxml/active", value.ToString()); } }
        public bool EmailActive { get { return Record.GetXmlPropertyBool("genxml/emailon"); } }
        public bool DebugMode { get { if (Record == null) return false; else return Record.GetXmlPropertyBool("genxml/debugmode"); } }
        public bool UniqueCartItems { get { if (Record == null) return false; else return Record.GetXmlPropertyBool("genxml/uniquecartitems"); } }        
        public AppThemeDataList AppThemeList
        {
            get
            {
                return new AppThemeDataList(_portalId, "rocketecommerceapi");
            }
        }
        public int PaymentPageId { get { return Record.GetXmlPropertyInt("genxml/textbox/paymentpagetabid"); } }
        public string PaymentPageUrl { get { return PagesUtils.GetPageURL(PaymentPageId); } }
        public int ProductListPageId { get { return Record.GetXmlPropertyInt("genxml/textbox/productlisttabid"); } }
        public string ProductListURL { get { return PagesUtils.GetPageURL(ProductListPageId); } }
        public int ProductDetailPageId { get { return Record.GetXmlPropertyInt("genxml/textbox/productdetailtabid"); } }
        public string ProductDetailURL { get { return PagesUtils.GetPageURL(ProductDetailPageId); } }
        public int CartPageId { get { return Record.GetXmlPropertyInt("genxml/textbox/carttabid"); } }
        public string CartURL { get { return PagesUtils.GetPageURL(CartPageId); } }
        public int CheckOutPageId { get { return Record.GetXmlPropertyInt("genxml/textbox/checkouttabid"); } }
        public string CheckOutURL { get { return PagesUtils.GetPageURL(CheckOutPageId); } }
        public int ImageResize { get { if (Record.GetXmlPropertyInt("genxml/imageresize") > 0) return Record.GetXmlPropertyInt("genxml/imageresize"); else return 640; } }
        public string ProjectName { get { return Record.GetXmlProperty("genxml/select/selectedprojectname"); } set { Record.SetXmlProperty("genxml/select/selectedprojectname", value); } }
        public string AppThemeFolder { get { return Record.GetXmlProperty("genxml/select/apptheme"); } }
        public string AppThemeVersion { get { return Record.GetXmlProperty("genxml/select/appthemeversion"); } }
        public int CartLimit { get { return 50; } }
        public int CartDays { get { return 7; } }
        public bool ManualCategoryOrderby { get { return Info.GetXmlPropertyBool("genxml/checkbox/manualcategoryorderby"); } }
        public string ApiUrl { get { return "/Desktopmodules/dnnrocket/api/rocket/action"; } }


        #endregion

    }
}
