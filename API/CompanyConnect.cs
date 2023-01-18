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
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("company.cshtml");
                var companyData = new CompanyLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit);
                return RenderRazorUtils.RazorDetail(razorTempl, companyData, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public String CompanySave()
        {
            try
            {
                var companyData = new CompanyLimpet(_portalShop.PortalId, _sessionParams.CultureCode);
                companyData.Save(_postInfo);
                return CompanyEdit();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string AddCompanyImage()
        {
            var companyData = new CompanyLimpet(_portalShop.PortalId, _sessionParams.CultureCode);
            var imgList = ImgUtils.MoveImageToFolder(_postInfo, _portalShop.ImageFolderMapPath, 1);
            foreach (var nam in imgList)
            {
                companyData.Info.SetXmlProperty("genxml/hidden/imagepathlogo", _portalShop.ImageFolderRel.TrimEnd('/') + "/" +  nam);
            }
            companyData.Update();
            return CompanyEdit();
        }


    }
}
