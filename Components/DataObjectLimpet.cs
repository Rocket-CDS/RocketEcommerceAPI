using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class DataObjectLimpet
    {
        private const string _systemkey = "rocketecommerceapi";
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _settings;
        private string _browserid;
        private string _cultureCode;
        public DataObjectLimpet(int portalid, string moduleRef, SessionParams sessionParams, bool editMode = true)
        {
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, cultureCode, sessionParams.ModuleId, sessionParams.TabId, sessionParams.BrowserId);
        }
        public DataObjectLimpet(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId, string browserid)
        {
            Populate(portalid, moduleRef,  cultureCode, moduleId, tabId, browserid);
        }
        public void Populate(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId, string browserid)
        {
            _browserid = browserid;
            _cultureCode = cultureCode;
            _settings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            var systemData = new SystemLimpet(_systemkey);
            var portalShop = new PortalShopLimpet(portalid, cultureCode);
            var shopSettings = new ShopSettingsLimpet(portalid, cultureCode);

            SetDataObject("shopsettings", shopSettings);
            SetDataObject("appthemesystem", new AppThemeSystemLimpet(portalid, _systemkey));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", systemData);
            SetDataObject("appthemeprojects", new AppThemeProjectLimpet());
            SetDataObject("modulesettings", new ModuleContentLimpet(portalid, moduleRef));
            SetDataObject("defaultdata", new DefaultsLimpet());
            SetDataObject("modulesettings", new ModuleContentLimpet(portalid, moduleRef, moduleId, tabId));
            SetDataObject("globalsettings", new SystemGlobalData());
            SetDataObject("appthemedefault", new AppThemeLimpet(portalid, systemData, "Default", "1.0"));
            SetDataObject("appthemeview", new AppThemeLimpet(portalid, portalShop.AppThemeFolder, portalShop.AppThemeVersion, portalShop.ProjectName));
            SetDataObject("companydata", new CompanyLimpet(portalid, cultureCode));
            SetDataObject("portalshop", portalShop);
            SetDataObject("appthemedatalist", new AppThemeDataList(portalid, portalShop.ProjectName, _systemkey));
            SetDataObject("notificationdata", new NotificationLimpet(portalid,cultureCode));
            SetDataObject("categorylist", new CategoryLimpetList(portalid, cultureCode, true));
            SetDataObject("propertylist", new PropertyLimpetList(portalid, cultureCode));
            SetDataObject("portalstats", new PortalShopLimpetStats(portalShop));
            SetDataObject("cartdata", new CartLimpet(_browserid, cultureCode));
            SetDataObject("defaultcategory", new CategoryLimpet(portalid, shopSettings.DefaultCategoryId, cultureCode));

        }
        public void ReloadCart()
        {
            SetDataObject("cartdata", new CartLimpet(_browserid, _cultureCode));
        }
        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);
        }
        public object GetDataObject(String key)
        {
            if (_dataObjects != null && _dataObjects.ContainsKey(key)) return _dataObjects[key];
            return null;
        }
        public void SetSetting(string key, string value)
        {
            if (_settings.ContainsKey(key)) _settings.Remove(key);
            _settings.Add(key, value);
        }
        public string GetSetting(string key)
        {
            if (!_settings.ContainsKey(key)) return "";
            return _settings[key];
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public int PortalId { get { return PortalData.PortalId; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public ModuleContentLimpet ModuleSettings { get { return (ModuleContentLimpet)GetDataObject("modulesettings"); } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public AppThemeLimpet AppThemeDefault { get { return (AppThemeLimpet)GetDataObject("appthemedefault"); } }
        public AppThemeLimpet AppThemeView { get { return (AppThemeLimpet)GetDataObject("appthemeview"); } set { SetDataObject("appthemeview", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public PortalShopLimpet PortalShop { get { return (PortalShopLimpet)GetDataObject("portalshop"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public CompanyLimpet CompanyData { get { return (CompanyLimpet)GetDataObject("companydata"); } }
        public ShopSettingsLimpet ShopSettings { get { return (ShopSettingsLimpet)GetDataObject("shopsettings"); } }
        public CartLimpet CartData { get { return (CartLimpet)GetDataObject("cartdata"); } }
        public CategoryLimpetList CategoryList { get { return (CategoryLimpetList)GetDataObject("categorylist"); } }
        public NotificationLimpet NotificationData { get { return (NotificationLimpet)GetDataObject("notificationdata"); } }        
        public Dictionary<string, string> Settings { get { return _settings; } }

    }
}
