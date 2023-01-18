using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {

        private string RemoteSettings()
        {
            try
            {
                var appThemeDataList = new AppThemeDataList(_org, _systemData.SystemKey);
                var razorTempl = _appThemeSystem.GetTemplate("RemoteSettings.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string SaveSettings()
        {
            try
            {
                if (_moduleRef != "")
                {
                    var remoteModule = new RemoteModule(_portalShop.PortalId, _moduleRef);
                    remoteModule.Save(_postInfo);
                    // update sitekey after Save(), it replaces all XML.
                    remoteModule.SiteKey = _sessionParams.SiteKey;
                    remoteModule.Update();

                    //reload data for render
                    _dataObjects.Remove("remotemodule");
                    _dataObjects.Add("remotemodule", remoteModule);
                    _org = remoteModule.ProjectName;

                }
                return RemoteSettings();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string ClearSettings()
        {
            try
            {
                if (_moduleRef != "")
                {
                    var remoteModule = new RemoteModule(_portalShop.PortalId, _moduleRef);
                    remoteModule.Record.XMLData = "<genxml></genxml>";
                    remoteModule.Update();
                    //reload data for render
                    _dataObjects.Remove("remotemodule");
                    _dataObjects.Add("remotemodule", remoteModule);
                }
                return RemoteSettings();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string AppThemeVersions()
        {
            try
            {
                var appTheme = _postInfo.GetXmlProperty("genxml/remote/appthemeview");
                if (_paramInfo.GetXmlProperty("genxml/hidden/ctrl") == "appthemeviewversion") appTheme = _postInfo.GetXmlProperty("genxml/remote/appthemeview");
                var appThemeData = new AppThemeLimpet(_portalData.PortalId, appTheme, "", _org);
                if (!appThemeData.Exists) return "Invalid AppTheme: " + appTheme;
                var razorTempl = _appThemeSystem.GetTemplate("RemoteAppThemeVersions.cshtml");
                var dataObjects = new Dictionary<string, object>();
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeData, dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}
