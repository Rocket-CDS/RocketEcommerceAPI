using DNNrocketAPI.Components;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class ModuleContentLimpet : ModuleBase
    {
        public ModuleContentLimpet(int portalId, string moduleRef, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {             
        }
        public int DefaultCategoryId { get { return GetSettingInt("defaultcategory"); } }
        public int ListPageTabId()
        {
            var rtn = GetSettingInt("listpage");
            if (rtn == 0)
            {
                var p = new PortalShopLimpet(PortalId, DNNrocketUtils.GetCurrentCulture());
                rtn = p.ProductListPageId;
            }
            return rtn;
        }
        public int DetailPageTabId()
        {
            var rtn = GetSettingInt("detailpage");
            if (rtn == 0)
            {
                var p = new PortalShopLimpet(PortalId, DNNrocketUtils.GetCurrentCulture());
                rtn = p.ProductDetailPageId;
            }
            return rtn;
        }

    }
}
