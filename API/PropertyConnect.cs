using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private PropertyLimpet GetActiveProperty(int propertyid)
        {
            return new PropertyLimpet(_portalShop.PortalId, propertyid, _sessionParams.CultureCodeEdit);
        }
        public String GetProperty(int propertyId)
        {
            var razorTempl = _appThemeSystem.GetTemplate("propertydetail.cshtml");
            var propertyData = GetActiveProperty(propertyId);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string GetPropertyList()
        {
            var propertyDataList = new PropertyLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams, _sessionParams.CultureCodeEdit);
            var razorTempl = _appThemeSystem.GetTemplate("PropertyList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyDataList, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AddProperty()
        {
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var razorTempl = _appThemeSystem.GetTemplate("propertydetail.cshtml");
            var propertyData = GetActiveProperty(-1);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, propertyData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public string SaveProperty()
        {
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            _passSettings.Add("saved", "true");
            var propertyData = GetActiveProperty(propertyId);
            var r = propertyData.Save(_postInfo);
            if ( r == -1) _passSettings.Add("duplicateref", "true");
            return GetProperty(r);
        }
        public string DeleteProperty()
        {
            var propertyid = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            if (propertyid > 0)
            {
                var propertyData = GetActiveProperty(propertyid);
                propertyData.Delete();
            }
            return GetPropertyList();
        }
        public string RemovePropertyArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveProduct(articleId);
            articleData.RemoveProperty(propertyId);
            return GetProperty(propertyId);
        }

    }
}

