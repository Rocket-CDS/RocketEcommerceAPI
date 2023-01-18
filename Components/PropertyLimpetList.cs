using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class PropertyLimpetList
    {
        private const string _tableName = "RocketEcommerceAPI";
        private List<PropertyLimpet> _propertyList;
        private DNNrocketController _objCtrl;
        private SessionParams _sessionParams;

        public PropertyLimpetList(int portalId, SessionParams sessionParams, string langRequired, RemoteModule remoteModule = null)
        {
            PortalId = portalId;
            CultureCode = langRequired;
            EntityTypeCode = "PROP";
            TableName = _tableName;
            RemoteModule = remoteModule;
            _sessionParams = sessionParams;

            if (CultureCode == "") CultureCode = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            Populate();
        }
        public void Populate()
        {
            var filter = "";
            if (_sessionParams.Info.GetXmlProperty("r/propertysearchtext") != "") filter = " and [XMLData].value('(genxml/lang/genxml/textbox/name)[1]','nvarchar(max)') like '%" + _sessionParams.Info.GetXmlProperty("r/propertysearchtext") + "%' ";
            _sessionParams.RowCount = _objCtrl.GetListCount(PortalId, -1, EntityTypeCode, filter, CultureCode, _tableName);
            DataList = _objCtrl.GetList(PortalId, -1, EntityTypeCode, filter, CultureCode, " order by [XMLData].value('(genxml/textbox/ref)[1]','nvarchar(max)') ", 0, _sessionParams.Page, _sessionParams.PageSize, _sessionParams.RowCount, TableName);
            PopulatePropertyList();
        }
        public void DeleteAll()
        {
            foreach (var r in DataList)
            {
                _objCtrl.Delete(r.ItemID);
            }
        }
        public RemoteModule RemoteModule { get; set; }
        public List<SimplisityInfo> DataList { get; private set; }
        public int PortalId { get; set; }
        public string TableName { get; set; }
        public string EntityTypeCode { get; set; }
        public string CultureCode { get; set; }
        public List<PropertyLimpet> GetPropertyList(string groupRef = "")
        {
            if (groupRef != "")
            {
                var rtnList = new List<PropertyLimpet>();
                foreach (var p in _propertyList)
                {
                    if (p.IsInGroup(groupRef)) rtnList.Add(p);
                }
                return rtnList;
            }
            return _propertyList;
        }
        private List<PropertyLimpet> PopulatePropertyList()
        {
            _propertyList = new List<PropertyLimpet>();
            foreach (var o in DataList)
            {
                var propertyData = new PropertyLimpet(PortalId, o.ItemID, CultureCode);
                _propertyList.Add(propertyData);
            }
            return _propertyList;
        }
        public void Validate()
        {
            // validate properties
            foreach (var pInfo in DataList)
            {
                var propertyData = new PropertyLimpet(PortalId, pInfo.ItemID, CultureCode);
                propertyData.ValidateAndUpdate();
            }
            Reload();
        }
        /// <summary>
        /// Clear cache and Reload list 
        /// </summary>
        public void Reload()
        {
            Populate();
        }
    }

}
