using DNNrocketAPI;
using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class ShopSettingsLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "SHOPSETTINGS";

        private DNNrocketController _objCtrl;
        private int _portalId;
        private string _cacheKey;

        public ShopSettingsLimpet(int portalId, string cultureCode)
        {
            Info = new SimplisityInfo();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            CultureCode = cultureCode;
            _portalId = portalId;

            _objCtrl = new DNNrocketController();

            _cacheKey = _entityTypeCode + portalId + "*" + cultureCode;

            Info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (Info == null)
            {
                Info = _objCtrl.GetByType(portalId, -1, _entityTypeCode, "", "", _tableName);
                if (Info == null || Info.ItemID <= 0)
                {
                    Info = new SimplisityInfo();
                    Info.PortalId = _portalId;
                    Info.ModuleId = -1;
                    Info.TypeCode = _entityTypeCode;
                    Info.Lang = cultureCode;
                }
            }
        }

        public void Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/radio/*");

            //Event List
            var sql = "SELECT SUBSTRING((SELECT distinct ',' + replace([XMLData].value('(genxml/textbox/eventname)[1]','nvarchar(max)'),',','.') FROM {databaseOwner}[{objectQualifier}RocketEcommerceAPI] where portalid = " + PortalId + " and typecode = 'order' FOR XML PATH('')),2,8000) AS CSV";
            var eventreturn = _objCtrl.ExecSql(sql);
            EventCSV = eventreturn;

            Update();
        }
        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*", "") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
        }

        public void Update()
        {
            Info = _objCtrl.SaveData(Info, _tableName);
            CacheUtils.SetCache(_cacheKey, Info);
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
            CacheUtils.RemoveCache(_cacheKey);
        }
        public Dictionary<string,string> GetPropertyGroups()
        {
            var rtn = new Dictionary<string, string>();
            var s = PropertyGroups.Split(',');
            foreach (var g in s)
            {
                rtn.Add(g, g);
            }
            return rtn;
        }
        public Dictionary<string, string> EventNameList()
        {
            var rtn = new Dictionary<string, string>();
            var s = EventCSV.Split(',');
            foreach (var g in s)
            {
                rtn.Add(g, g);
            }
            return rtn;
        }
        public string EntityTypeCode { get { return Info.GUIDKey; } }
        public SimplisityInfo Info { get; set; }
        public int PortalId { get { return Info.PortalId; } }
        public bool Exists { get { if (Info.ItemID > 0) return true; else return false; }  }
        public string CultureCode { get; set; }
        public string PropertyGroups { get { return Info.GetXmlProperty("genxml/textbox/propertygroups"); } }
        public bool ManualCategoryOrderby { get { return Info.GetXmlPropertyBool("genxml/checkbox/manualcategoryorderby"); } }
        public bool InCategoryFilter { get { return Info.GetXmlPropertyBool("genxml/checkbox/incategoryfilter"); } }
        public bool ManualOrderBy { get { return Info.GetXmlPropertyBool("genxml/checkbox/manualorderby"); } }

        // use /config/ so it does not get overwritten on save.
        public int DefaultCategoryId { get { return Info.GetXmlPropertyInt("genxml/config/defaultcategoryid"); } set { Info.SetXmlPropertyInt("genxml/config/defaultcategoryid", value); } }
        public string SystemKey { get { return "rocketecommerceapi"; }  }
        public string EventName { get { return Info.GetXmlProperty("genxml/textbox/eventname"); } set { Info.SetXmlProperty("genxml/textbox/eventname", value.ToString()); } }
        public string EventCSV { get { return Info.GetXmlProperty("genxml/config/eventlist"); } set { Info.SetXmlProperty("genxml/config/eventlist", value.ToString()); } }

    }
}
