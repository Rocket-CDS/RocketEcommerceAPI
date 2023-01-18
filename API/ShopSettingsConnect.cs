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
            _shopSettings.Save(_postInfo);
            return GetCatalogSettings();
        }

        public string GetCatalogSettings()
        {
            var razorTempl = _appThemeSystem.GetTemplate("ShopSettings.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _shopSettings, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.ErrorMsg != "") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string DeleteCatalogSettings()
        {
            _shopSettings.Delete();
            return GetCatalogSettings();
        }




    }
}

