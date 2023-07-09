using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Razor.Parser.SyntaxTree;
using DNNrocketAPI;
using DNNrocketAPI.Components;

namespace RocketEcommerceAPI.Components
{
    public class MenuDirectory : IMenuInterface
    {
        private CategoryLimpetList _categoryDataList;
        private string _systemkey = "rocketecommerceapi";
        public List<PageRecordData> GetMenuItems(int portalId, string cultureCode, string systemkey, string rootRef = "")
        {
            if (!String.IsNullOrEmpty(systemkey)) _systemkey = systemkey;

            var rtn = new List<PageRecordData>();
            var portalContent = new PortalShopLimpet(portalId, cultureCode);
            _categoryDataList = new CategoryLimpetList(portalId, cultureCode);
            var rootId = ParentId(rootRef);
            var treelist = _categoryDataList.GetCategoryTree(rootId);
            foreach (var catData in treelist)
            {
                var p = new PageRecordData();
                p.Name = catData.Name;
                p.KeyWords = catData.Keywords;
                p.Description = catData.Summary;
                p.Title = catData.Name;
                if (catData.ParentItemId == 0)
                    p.ParentPageId = 0;
                else
                    p.ParentPageId = catData.ParentItemId;
                p.PageId = catData.CategoryId;
                p.Url = PagesUtils.NavigateURL(portalContent.ProductListPageId) + "/catid/" + catData.CategoryId + "/" + DNNrocketUtils.UrlFriendly(catData.Name);
                rtn.Add(p);
            }
            return rtn;
        }


        public string TokenPrefix()
        {
            return "[CATDIR";
        }
        public int PageId(int portalId, string cultureCode)
        {
            var portalContent = new PortalShopLimpet(portalId, cultureCode);
            return portalContent.ProductListPageId;
        }
        public int ParentId(string rootRef)
        {
            var rootId = 0;
            if (rootRef == "") return rootId;
            var rootCatList = _categoryDataList.GetCategoryByRef(rootRef);
            if (rootCatList.Count > 0) rootId = rootCatList.First().CategoryId;
            return rootId;
        }
    }

}
