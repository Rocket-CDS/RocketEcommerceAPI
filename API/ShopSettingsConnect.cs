using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {

        public string SaveCatalogSettings()
        {
            _dataObject.ShopSettings.Save(_postInfo);
            return GetCatalogSettings();
        }
        public string AddPropertyGroup()
        {
            _dataObject.ShopSettings.Save(_postInfo);
            _dataObject.ShopSettings.AddGroup(new SimplisityInfo());
            return GetCatalogSettings();
        }
        public string GetCatalogSettings()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("ShopSettings.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ShopSettings, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string DeleteCatalogSettings()
        {
            _dataObject.ShopSettings.Delete();
            return GetCatalogSettings();
        }




    }
}

