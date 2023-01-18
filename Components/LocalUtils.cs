using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public static class LocalUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketEcommerceAPI";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources";

        /// <summary>
        /// Get a resouerce string from a resx file in "/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources" 
        /// </summary>
        /// <param name="resourceKey">[filename].[resourcekey]</param>
        /// <param name="resourceExt">[resource key extention]</param>
        /// <param name="cultureCode">[culturecode to fetch]</param>
        /// <returns></returns>
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources", resourceKey, resourceExt, cultureCode);
        }
        
        public static double ConvertTimeToDouble(string timeString)
        {
            if (timeString == "") return -1;
            string[] samplesplit = timeString.Split(':');
            if (samplesplit.Length == 2) return double.Parse(samplesplit[0]) + double.Parse(samplesplit[1]) / 60;
            if (samplesplit.Length == 3) return double.Parse(samplesplit[0]) + double.Parse(samplesplit[1]) / 60 + double.Parse(samplesplit[2]) / 6000;
            return -1;
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

        public static string ReloadPage(int portalId)
        {
            try
            {
                // user does not have access, logoff
                UserUtils.SignOut();

                var portalData = new PortalLimpet(portalId);
                var appThemeSystem = new AppThemeSystemLimpet("rocketecommerceapi");
                var razorTempl = appThemeSystem.GetTemplate("Reload.cshtml");
                var rtn = RenderRazorUtils.RazorProcessData(razorTempl, portalData, null, null, null, true);
                return rtn.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }

}
