using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RocketEcommerceAPI.Components
{
    public class CompanyLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "CompanyData";
        private const string _systemkey = "rocketecommerceapi";
        private DNNrocketController _objCtrl;
        private string _guidKey;
        private int _portalId;
        public CompanyLimpet(int portalId, string cultureCode)
        {
            Info = new SimplisityInfo();
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            CultureCode = cultureCode;
            _portalId = portalId;
            _guidKey =  PortalUtils.SiteGuid(portalId);
            _objCtrl = new DNNrocketController();

            var uInfo = _objCtrl.GetByGuidKey(portalId, -1, _entityTypeCode, _guidKey, "", _tableName);
            if (uInfo != null) Info = _objCtrl.GetInfo(uInfo.ItemID, CultureCode, _tableName);
            if (Info == null || Info.ItemID <= 0)
            {
                Info = new SimplisityInfo();
                Info.PortalId = _portalId;
                Info.ModuleId = -1;
                Info.TypeCode = _entityTypeCode;
                Info.GUIDKey = _guidKey;
                Info.Lang = CultureCode;
            }
        }

        public void Save(SimplisityInfo info)
        {
            Info.XMLData = info.XMLData;
            Update();
        }
        public void Update()
        {
            _objCtrl.SaveData(Info, _tableName);
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        public string EntityTypeCode { get { return _entityTypeCode; } }

        public bool IsOpen(DayOfWeek day, string time)
        {
            if (time == "") return false;

            var testTime = LocalUtils.ConvertTimeToDouble(time);
            var open1 = LocalUtils.ConvertTimeToDouble(Info.GetXmlProperty("genxml/textbox/open1-" + Convert.ToInt32(day).ToString()));
            var close1 = LocalUtils.ConvertTimeToDouble(Info.GetXmlProperty("genxml/textbox/close1-" + Convert.ToInt32(day).ToString()));
            var open2 = LocalUtils.ConvertTimeToDouble(Info.GetXmlProperty("genxml/textbox/open2-" + Convert.ToInt32(day).ToString()));
            var close2 = LocalUtils.ConvertTimeToDouble(Info.GetXmlProperty("genxml/textbox/close2-" + Convert.ToInt32(day).ToString()));

            if (open1 < 0 && open2 < 0) return false;
            if (close1 < 0 && close2 < 0) return false;
            if (open1 < 0 && close2 < 0) return false;

            // check morning (Morning time added)
            if (open1 >= 0 && close1 >= 0) if (testTime >= open1 && testTime <= close1) return true;
            // check afternoon  (Afternoon time added)
            if (open2 >= 0 && close2 >= 0) if (testTime >= open2 && testTime <= close2) return true;
            // check full day ( no close1 time added)
            if (close1 < 0 && open1 >= 0 && close2 >= 0) if (testTime >= open1 && testTime <= close2) return true;

            return false;
        }
        public List<string> OpenTimeSegmentList(DayOfWeek day, int segmentMinutes = 30)
        {
            var rtn = new List<string>();
            DateTime midnight = DateTime.Now.Date;

            var loopmax = (24 * 60) / segmentMinutes;
            for (int i = 1; i < loopmax; i++)
            {
                var t = midnight.ToString("HH:mm", new CultureInfo(DNNrocketUtils.GetCurrentCulture()));
                if (!rtn.Contains(t))
                {
                    if (IsOpen(day, t)) rtn.Add(t);
                    midnight = midnight.AddMinutes(segmentMinutes);
                }
            }
            return rtn;
        }
        public List<int> OpenDays(int segmentMinutes = 30)
        {
            var rtn = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                if (OpenTimeSegmentList((DayOfWeek)i, segmentMinutes).Count > 0)
                {
                    rtn.Add(i);
                }
            }
            return rtn;
        }

        #region "Info - PortalShop Data"
        public SimplisityInfo Info { get; set; }
        public string TaxNumber { get { return Info.GetXmlProperty("genxml/merchant/taxnumber"); } }
        public string Phone { get { return Info.GetXmlProperty("genxml/merchant/phone"); } }
        public string IBAN { get { return Info.GetXmlProperty("genxml/merchant/iban"); } }
        public string DisplayAddress { get { return Info.GetXmlProperty("genxml/merchant/displayaddress"); } }
        public string FromEmail { get { return Info.GetXmlProperty("genxml/merchant/fromemail"); } }
        public string ContactEmail { get { if (Info.GetXmlProperty("genxml/merchant/contactemail") == "" )
                    return Info.GetXmlProperty("genxml/merchant/fromemail");
            else
                    return Info.GetXmlProperty("genxml/merchant/contactemail");
            }
        }
        public string OrderEmail
        {
            get
            {
                if (Info.GetXmlProperty("genxml/merchant/orderemail") == "")
                    return Info.GetXmlProperty("genxml/merchant/contactemail");
                else
                    return Info.GetXmlProperty("genxml/merchant/orderemail");
            }
        }
        public string CompanyName { get { return Info.GetXmlProperty("genxml/merchant/companyname"); } set { Info.SetXmlProperty("genxml/merchant/companyname", value.ToString()); } }
        public string StoreName { get { return Info.GetXmlProperty("genxml/storename"); } set { Info.SetXmlProperty("genxml/storename", value.ToString()); } }
        public string WebsiteUrl { get { return Info.GetXmlProperty("genxml/merchant/websiteurl"); } set { Info.SetXmlProperty("genxml/merchant/websiteurl", value.ToString()); } }
        public int PortalId { get { return Info.PortalId; } }
        public int CompanyId { get { return Info.ItemID; } }
        public string CultureCode { get; set; }
        public string SiteKey { get { return Info.GUIDKey; } }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string LogoRelPath { get { return Info.GetXmlProperty("genxml/hidden/imagepathlogo"); } set { Info.SetXmlProperty("genxml/hidden/imagepathlogo", value); } }
        public string MapPath { get { return DNNrocketUtils.MapPath(LogoRelPath); } }

        #endregion

    }
}
