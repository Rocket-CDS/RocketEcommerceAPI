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
        private CategoryLimpet GetActiveCategory(int categoryid)
        {
            return new CategoryLimpet(_portalShop.PortalId, categoryid, _sessionParams.CultureCodeEdit);
        }
        public String GetCategory(int categoryId)
        {
            var razorTempl = _appThemeSystem.GetTemplate("categorydetail.cshtml");
            var categoryData = GetActiveCategory(categoryId);
            return RenderRazorUtils.RazorDetail(razorTempl, categoryData, _passSettings, _sessionParams, true);
        }
        public string MoveCategory()
        {
            var parentid = 0;
            var sourceid = _paramInfo.GetXmlPropertyInt("genxml/hidden/sourceid");
            if (sourceid > 0)
            {
                var sourceData = new CategoryLimpet(_portalShop.PortalId, sourceid, _sessionParams.CultureCodeEdit);
                if (sourceData.Exists)
                {
                    parentid = sourceData.ParentItemId;
                    var destparentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/destid");
                    if (destparentid > 0)
                    {
                        var destData = new CategoryLimpet(_portalShop.PortalId, destparentid, _sessionParams.CultureCodeEdit);
                        sourceData.SortOrder = destData.SortOrder + 1;
                    }
                    else
                    {
                        sourceData.SortOrder = -1;  // must be top record.
                    }
                    sourceData.Update();
                    SortCategoryList(sourceData.ParentItemId);
                }
            }
            return GetCategoryList(parentid);
        }
        private void SortCategoryList(int parentid)
        {
            var categoryDataList = new CategoryLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams.CultureCodeEdit, false);
            categoryDataList.Reload();
            var l = categoryDataList.GetCategoryList(parentid);
            var lp = 1;
            foreach (var c in l)
            {
                c.SortOrder = (lp * 5);
                c.Update();
                lp += 1;
            }
            categoryDataList.Validate(); // clear cache
        }
        public string GetCategoryList(int categoryid)
        {
            if (categoryid < 0) categoryid = 0;
            var categoryDataList = new CategoryLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams.CultureCodeEdit, true);
            categoryDataList.SelectedParentId = categoryid;
            var razorTempl = _appThemeSystem.GetTemplate("CategoryList.cshtml");
            return RenderRazorUtils.RazorDetail(razorTempl, categoryDataList, _passSettings, _sessionParams, true);
        }
        public string GetCategoryList()
        {
            return GetCategoryList(_paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid"));
        }
        public String AddCategory()
        {
            var categoryDataList = new CategoryLimpetList(PortalUtils.GetCurrentPortalId(), _sessionParams.CultureCodeEdit, true);
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var razorTempl = _appThemeSystem.GetTemplate("CategoryDetail.cshtml");
            var categoryData = GetActiveCategory(-1);
            var catcount = categoryDataList.GetCategoryList(parentid).Count;

            categoryData.ParentItemId = parentid;
            categoryData.SortOrder = (5 * catcount);
            categoryData.ValidateAndUpdate();

            categoryDataList.Validate();  // clear cache

            return RenderRazorUtils.RazorDetail(razorTempl, categoryData, _passSettings, _sessionParams, true);
        }

        public int SaveCategory()
        {
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            _passSettings.Add("saved", "true");
            var categoryData = GetActiveCategory(categoryId);
            return categoryData.Save(_postInfo);
        }
        public string DeleteCategory()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0)
            {
                var categoryData = GetActiveCategory(categoryid);
                categoryData.Delete();
            }
            return GetCategoryList(0);
        }
        public string AddCategoryImage()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0)
            {
                var categoryData = GetActiveCategory(categoryid);
                categoryData.Save(_postInfo);
                var imgList = ImgUtils.MoveImageToFolder(_postInfo, _portalShop.ImageFolderMapPath);
                categoryData.RemoveImageList();
                foreach (var nam in imgList)
                {
                    categoryData.AddImage(_portalShop.ImageFolderRel, nam);
                }
                return GetCategory(categoryData.CategoryId);
            }
            return "ERROR: Invalid ItemId";
        }
        public string AssignParentCategory()
        {
            var parentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentid");
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            if (categoryid > 0 && parentid != categoryid) // check we don't move to itself
            {
                var sourceData = new CategoryLimpet(_portalShop.PortalId, categoryid, _sessionParams.CultureCodeEdit);
                if (sourceData.Exists)                
                {
                    if (!sourceData.HasChild(parentid))  // check we don't move to a child category
                    {
                        sourceData.ParentItemId = parentid;
                        sourceData.SortOrder = -1;  // must be top record.
                        sourceData.Update();
                        SortCategoryList(sourceData.ParentItemId);
                    }
                }
            }
            return GetCategory(categoryid);
        }
        public string AssignDefaultCategory()
        {
            var categoryid = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            _shopSettings.DefaultCategoryId = categoryid;
            _shopSettings.Update();
            return GetCatalogSettings();
        }
        public string RemoveCategoryArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveProduct(articleId);
            articleData.RemoveCategory(categoryId);
            return GetCategory(categoryId);
        }

    }
}

