using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using RocketEcommerceAPI.Components;

namespace RocketEcommerceAPI.Components
{
    public class PropertyLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private string _cacheKey;

        private DNNrocketController _objCtrl;
        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="articleId"></param>
        /// <param name="langRequired"></param>
        public PropertyLimpet(int portalId, int propertyId, string langRequired, bool populate = true)
        {
            _cacheKey = "PropertyLimpet*" + portalId + "*" + propertyId + "*" + langRequired + "*" + _tableName;

            if (propertyId <= 0) propertyId = -1;  // create new record.
            PortalId = portalId;
            EntityTypeCode = "PROP";
            TableName = _tableName;
            CultureCode = langRequired;

            Info = new SimplisityInfo();
            Info.ItemID = propertyId;
            Info.TypeCode = EntityTypeCode;
            Info.ModuleId = -1;
            Info.UserId = -1;
            Info.PortalId = PortalId;
            Info.Lang = CultureCode;

            Populate();
        }
        private void Populate()
        {
            _objCtrl = new DNNrocketController();
            var info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (info == null)
            {
                info = _objCtrl.GetInfo(PropertyId, CultureCode, TableName); // get existing record.
                if (info != null) // check if we have a real record.
                {
                    Info = info;
                    CacheUtils.SetCache(_cacheKey, Info);
                }
                else
                    Info.ItemID = -1; // flags categories does not exist yet.

                Info.Lang = CultureCode; // reset langauge, for legacy record, without lang.
                PortalId = Info.PortalId;
            }
            else
                Info = info;
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
        }
        public void Delete()
        {
            if (Info.ItemID > 0)
            {
                // remove property xref
                var catxrefList = _objCtrl.GetList(PortalId, -1, "CATXREF", " and [XrefItemId] = " + PropertyId + " ", "", "", 0, 0, 0, 0, TableName);
                foreach (var catxrefRecord in catxrefList)
                {
                    _objCtrl.Delete(catxrefRecord.ItemID, TableName);
                }
                _objCtrl.Delete(Info.ItemID, TableName);
                CacheUtils.ClearAllCache();
            }
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
        public int Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");

            Info.RemoveXmlNode("genxml/checkboxlist");
            Info.AddXmlNode(postInfo.GetXmlNode("genxml/checkboxlist"), "group", "genxml/checkboxlist");
            Info.GUIDKey = Ref;
            var testInfo = _objCtrl.GetByGuidKey(PortalId, -1, EntityTypeCode, Info.GUIDKey, "", _tableName, CultureCode);
            if (testInfo != null && testInfo.ItemID != Info.ItemID)
            {
                return -1;
            }
            else
            {
                return ValidateAndUpdate();
            }

        }
        public List<SimplisityInfo> GetArticlesInfo()
        {
            var searchFilter = " and [PROPXREF].[XrefItemId] = " + PropertyId  + " ";
            return _objCtrl.GetList(PortalShop.PortalId, -1, "PRD", searchFilter, CultureCode, "", 100, 0, 0, 0, _tableName);
        }
        public int Update()
        {
            Info = _objCtrl.SaveData(Info, TableName);
            _cacheKey = "PropertyLimpet*" + Info.PortalId + "*" + Info.ItemID + "*" + Info.Lang + "*" + _tableName;
            CacheUtils.RemoveCache(_cacheKey);
            return Info.ItemID;
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public void Validate()
        {
        }
        public List<string> PropertyGroups()
        {
            var rtn = new List<string>();
            var nodList = Info.XMLDoc.SelectNodes("genxml/checkboxlist/group/chk");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    if (nod.Attributes["value"].InnerText.ToLower() == "true")
                    {
                        rtn.Add(nod.Attributes["data"].InnerText);
                    }
                }
            }
            return rtn;
        }
        public bool IsInGroup(string groupref)
        {
            return Info.GetXmlPropertyBool("genxml/checkboxlist/group/chk[@data='" + groupref + "']/@value");
        }

        #region "properties"

        public string CultureCode { get; set; }
        public string EntityTypeCode { get; set; }
        public SimplisityInfo Info { get; set; }
        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int PropertyId { get { return Info.ItemID; } set { Info.ItemID = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public string ImageFolder { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get { if (Info.ItemID <= 0) { return false; } else { return true; }; } }
        public string TableName { get; set; }
        public int Level { get { return Info.GetXmlPropertyInt("genxml/textbox/level"); } set { Info.SetXmlProperty("genxml/textbox/level", value.ToString()); } }
        public string ArticleListName { get { return "articlelist"; } }
        public PortalShopLimpet PortalShop { get; set; }
        public string Ref { get { return Info.GetXmlProperty(RefXPath); } }
        public string RefXPath { get { return "genxml/textbox/ref"; } }
        public string RichText { get { return Info.GetXmlProperty(RichTextXPath); } }
        public string RichTextXPath { get { return "genxml/lang/genxml/textbox/propertyrichtext"; } }
        public string Name { get { return Info.GetXmlProperty(NameXPath); } }
        public string NameXPath { get { return "genxml/lang/genxml/textbox/name"; } }
        public string Summary { get { return Info.GetXmlProperty(SummaryXPath); } }
        public string SummaryXPath { get { return "genxml/lang/genxml/textbox/summary"; } }
        public string Keywords { get { return Info.GetXmlProperty(KeywordsXPath); } }
        public string KeywordsXPath { get { return "genxml/lang/genxml/textbox/propertykeywords"; } }
        public bool Hidden { get { return Info.GetXmlPropertyBool("genxml/checkbox/hidden"); } }
        public bool HiddenByCulture { get { return Info.GetXmlPropertyBool("genxml/lang/genxml/checkbox/hidden"); } }
        public bool IsHidden { get { if (Hidden || HiddenByCulture) return true; else return false; } }
        #endregion

    }

}
