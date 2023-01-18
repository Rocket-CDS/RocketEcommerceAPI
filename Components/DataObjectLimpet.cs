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
        private Dictionary<string, object> _dataObjects;
        private Dictionary<string, string> _passSettings;
        public DataObjectLimpet(int portalid, string moduleRef, SessionParams sessionParams, bool editMode = true)
        {
            var cultureCode = sessionParams.CultureCodeEdit;
            if (!editMode) cultureCode = sessionParams.CultureCode;
            Populate(portalid, moduleRef, cultureCode, sessionParams.ModuleId, sessionParams.TabId);
        }
        public DataObjectLimpet(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId)
        {
            Populate(portalid, moduleRef,  cultureCode, moduleId, tabId);
        }
        public void Populate(int portalid, string moduleRef, string cultureCode, int moduleId, int tabId)
        {
            _passSettings = new Dictionary<string, string>();
            _dataObjects = new Dictionary<string, object>();
            var systemData = new SystemLimpet(SystemKey);
            var portalShop = new PortalShopLimpet(portalid, cultureCode);

            SetDataObject("appthemesystem", new AppThemeSystemLimpet(portalid, SystemKey));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", systemData);
            SetDataObject("appthemeprojects", new AppThemeProjectLimpet());
            SetDataObject("modulesettings", new ModuleEcommerceLimpet(portalid, moduleRef));
            SetDataObject("defaultdata", new DefaultsLimpet());
            SetDataObject("globalsettings", new SystemGlobalData());
            SetDataObject("appthemedefault", new AppThemeLimpet(portalid, systemData, "Default", "1.0"));
            SetDataObject("appthemeview", new AppThemeLimpet(portalid, portalShop.AppThemeFolder, portalShop.AppThemeVersion, portalShop.ProjectName));
            SetDataObject("companydata", new CompanyLimpet(portalid, cultureCode));
            SetDataObject("portalshop", portalShop);
            SetDataObject("appthemedatalist", new AppThemeDataList(portalShop.ProjectName, SystemKey));
            SetDataObject("notificationdata", new NotificationLimpet(portalid,cultureCode));
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
            if (_passSettings.ContainsKey(key)) _passSettings.Remove(key);
            _passSettings.Add(key, value);
        }
        public string GetSetting(string key)
        {
            if (!_passSettings.ContainsKey(key)) return "";
            return _passSettings[key];
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public string SystemKey { get { return "rocketpayment"; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public AppThemeSystemLimpet AppThemeSystemEcom { get { return (AppThemeSystemLimpet)GetDataObject("ecomappthemesystem"); } }
        public AppThemeLimpet AppThemeDefault { get { return (AppThemeLimpet)GetDataObject("appthemedefault"); } }
        public AppThemeLimpet AppThemeView { get { return (AppThemeLimpet)GetDataObject("appthemeview"); } set { SetDataObject("appthemeview", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public SystemLimpet SystemDataEcom { get { return (SystemLimpet)GetDataObject("ecomsystemdata"); } }
        public PortalShopLimpet PortalShop { get { return (PortalShopLimpet)GetDataObject("portalshop"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public CompanyLimpet CompanyData { get { return (CompanyLimpet)GetDataObject("companydata"); } }
        public Dictionary<string, string> Settings { get { return _passSettings; } }

    }
}
