using DNNrocketAPI;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Components;
using Simplisity;
using Newtonsoft.Json;

namespace RocketEcommerceAPI.Components
{
    public static class CountryUtils
    {

        #region "API"

        public static String CultureSelect(string systemKey, SessionParams sessionParams)
        {
            try
            {
                var appThemeSystem = new AppThemeSystemLimpet(systemKey);
                var strOut = "";
                var objCtrl = new DNNrocketController();
                var razorTempl = appThemeSystem.GetTemplate("CultureCodeSelect.cshtml","");

                var l = DNNrocketUtils.GetCultureCodeList();
                var objl = new List<object>();
                foreach (var s in l)
                {
                    objl.Add(s);
                }
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, objl, null, null, sessionParams, true);

                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static String CountrySettings(string systemKey, bool saved, SessionParams sessionParams = null)
        {
            try
            {
                var appThemeSystem = new AppThemeSystemLimpet(systemKey);
                var razorTempl = appThemeSystem.GetTemplate("CountrySettings.cshtml", "");
                var countryData = new CountryLimpet(PortalUtils.GetPortalId());

                var passSettings = new Dictionary<string, string>();
                if (saved) passSettings.Add("saved", "true");

                return RenderRazorUtils.RazorDetail(razorTempl, countryData.Info, passSettings, sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void CountrySave(SimplisityInfo postInfo)
        {
            var countryData = new CountryLimpet(PortalUtils.GetPortalId());
            countryData.Save(postInfo);
        }

        #endregion

        public static string CountryListJson(bool allowempty = true)
        {
            var countryData = new CountryLimpet(PortalUtils.GetPortalId());
            var jsonList = new List<ValuePair>();
            var valuePair = new ValuePair();
            if (allowempty)
            {
                valuePair.Key = "";
                valuePair.Value = "";
                jsonList.Add(valuePair);
            }
            foreach (var i in countryData.GetSelectedDictCountries())
            {
                valuePair.Key = i.Key;
                valuePair.Value = i.Value;
                jsonList.Add(valuePair);
            }
            string result = JsonConvert.SerializeObject(jsonList);
            return result;
        }

        public static object RegionListJson(string countrycode, bool allowempty = true)
        {
            var jsonList = new List<ValuePair>();
            var valuePair = new ValuePair();
            if (allowempty)
            {
                valuePair.Key = "";
                valuePair.Value = "";
                jsonList.Add(valuePair);
            }
            foreach (var i in DNNrocketUtils.GetRegionList(countrycode))
            {
                valuePair = new ValuePair();
                valuePair.Key = i.Key;
                valuePair.Value = i.Value;
                jsonList.Add(valuePair);
            }
            return jsonList;
        }

    }
}
