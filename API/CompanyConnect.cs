using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        public String CompanyEdit()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("company.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CompanyData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String CompanySave()
        {
            var companyData = new CompanyLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCode);
            companyData.Save(_postInfo);
            return CompanyEdit();
        }
        public string AddCompanyImage()
        {
            var companyData = new CompanyLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCode);
            var imgList = ImgUtils.MoveImageToFolder(_postInfo, _dataObject.PortalShop.ImageFolderMapPath, 1);
            foreach (var nam in imgList)
            {
                companyData.Info.SetXmlProperty("genxml/hidden/imagepathlogo", _dataObject.PortalShop.ImageFolderRel.TrimEnd('/') + "/" +  nam);
            }
            companyData.Update();
            return CompanyEdit();
        }


    }
}
