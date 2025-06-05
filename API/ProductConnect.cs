using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private ProductLimpet GetActiveProduct(int productid)
        {
            return new ProductLimpet(_dataObject.PortalShop.PortalId, productid, _sessionParams.CultureCodeEdit);
        }
        public int SaveArticle()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            _dataObject.Settings.Add("saved", "true");
            var productData = new ProductLimpet(_dataObject.PortalShop.PortalId, productId, _sessionParams.CultureCodeEdit);
            return productData.Save(_postInfo);
        }
        public void DeleteArticle()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            GetActiveProduct(productId).Delete();
        }
        public void CopyArticle()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var productData = GetActiveProduct(productId);
            productData.ProductId = -1;
            var newProductId = productData.Copy();
            // create all languages
            var l = DNNrocketUtils.GetCultureCodeList();
            foreach (var c in l)
            {
                productData = new ProductLimpet(_dataObject.PortalShop.PortalId, productId, c);
                var newproductData = new ProductLimpet(_dataObject.PortalShop.PortalId, newProductId, c);
                newproductData.Info.XMLData = productData.Info.XMLData;
                newproductData.Name += " - " + LocalUtils.ResourceKey("RE.copy", "Text", c);
                newproductData.Update();
            }
            // add categories
            productData = new ProductLimpet(_dataObject.PortalShop.PortalId, productId, _sessionParams.CultureCodeEdit);
            var newproductData2 = new ProductLimpet(_dataObject.PortalShop.PortalId, newProductId, _sessionParams.CultureCodeEdit);
            foreach (var c in productData.GetCategories())
            {
                newproductData2.AddCategory(c.CategoryId);
            }
            // add properties
            foreach (var p in productData.GetProperties())
            {
                newproductData2.AddProperty(p.PropertyId);
            }
            newproductData2.ClearCache();
        }
        public string AddArticleModel()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productId > 0)
            {
                var productData = GetActiveProduct(productId);
                productData.Save(_postInfo);
                productData.AddModel();
                productData.Update();
                return GetProduct(productData.ProductId);
            }
            return "ERROR: Invalid ItemId";
        }
        public string AddArticleImage()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productId > 0)
            {
                var productData = GetActiveProduct(productId);
                productData.Save(_postInfo);
                var imgList = RocketUtils.ImgUtils.MoveImageToFolder(UserUtils.GetCurrentUserId(), _postInfo, _dataObject.PortalShop.ImageFolderMapPath, PortalUtils.TempDirectoryMapPath());
                foreach (var nam in imgList)
                {
                    productData.AddImage(nam);
                }
                return GetProduct(productData.ProductId);
            }
            return "ERROR: Invalid ItemId";
        }
        public string AddArticleImage64()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productId > 0)
            {
                var productData = GetActiveProduct(productId);
                productData.Save(_postInfo);

                var userfilename = UserUtils.GetCurrentUserId() + "_base64image.jpg";
                var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + userfilename;
                
                var base64image = _postInfo.GetXmlProperty("genxml/base64image");
                var baseArray = base64image.Split(',');
                if (baseArray.Length >= 2)
                {
                    base64image = baseArray[1];
                    var sInfo = new SimplisityInfo();
                    sInfo.SetXmlProperty("genxml/hidden/fileuploadlist", "base64image.jpg");

                    var bytes = Convert.FromBase64String(base64image);
                    using (var imageFile = new FileStream(baseFileMapPath, FileMode.Create))
                    {
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }

                    var imgList = RocketUtils.ImgUtils.MoveImageToFolder(UserUtils.GetCurrentUserId(), _postInfo, _dataObject.PortalShop.ImageFolderMapPath, PortalUtils.TempDirectoryMapPath());
                    foreach (var nam in imgList)
                    {
                        productData.AddImage(nam);
                    }
                    return GetProduct(productData.ProductId);
                }
            }
            return "ERROR: Invalid ItemId or base64 string";
        }
        public string AddArticleOption()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productId > 0)
            {
                var productData = GetActiveProduct(productId);
                productData.Save(_postInfo);
                productData.AddOption();
                return GetProduct(productData.ProductId);
            }
            return "ERROR: Invalid ItemId";
        }
        public String AddOptionsField()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var optionkey = _paramInfo.GetXmlProperty("genxml/hidden/optionkey");
            var productData = GetActiveProduct(productId);
            var productOption = productData.GetOption(optionkey);
            productOption.AddOptionField();
            productData.ReplaceOption(productOption);
            productData.Update();
            return GetOptionsField();
        }
        public String GetOptionsField()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("productoptionsfield.cshtml");
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var optionkey = _paramInfo.GetXmlProperty("genxml/hidden/optionkey");
            var productData = GetActiveProduct(productId);
            var productOption = productData.GetOption(optionkey);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, productOption, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String SaveOptionsField()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var optionkey = _paramInfo.GetXmlProperty("genxml/hidden/optionkey");
            var productData = GetActiveProduct(productId);
            var productOption = productData.GetOption(optionkey);
            productOption.SaveOptionFields(_postInfo.GetList("optionfieldlist"));           
            productData.ReplaceOption(productOption);
            productData.Update();
            return GetOptionsField();
        }
        public string AddProductDoc()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var productData = GetActiveProduct(productId);
            productData.Save(_postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var fileList = DocUtils.UploadSecureBase64file(filenameList, filebase64List, _dataObject.PortalShop.DocFolderMapPath);
                foreach (var imgFileMapPath in fileList)
                {
                    productData.AddDoc(imgFileMapPath.Value, Path.GetFileName(imgFileMapPath.Key));
                }
            }
            return GetProduct(productData.ProductId);
        }
        public string AddArticleLink()
        {
            var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productId > 0)
            {
                var productData = GetActiveProduct(productId);
                productData.Save(_postInfo);
                productData.AddLink();
                return GetProduct(productData.ProductId);
            }
            return "ERROR: Invalid ItemId";
        }
        public String AddArticle()
        {
            if (_dataObject.PortalShop.ProductCount < _dataObject.PortalShop.ProductLimit)
            {
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("productdetail.cshtml");
                var articleData = GetActiveProduct(-1);
                _dataObject.SetDataObject("productdata", articleData);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            return GetProductList(); // return list if we cannot add any more.
        }
        public String GetProduct(int productId)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("productdetail.cshtml");
            var articleData = GetActiveProduct(productId);
            _dataObject.SetDataObject("productdata", articleData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public String GetProductList()
        {
            var articleDataList = new ProductLimpetList(_sessionParams, _dataObject.PortalShop, _sessionParams.CultureCodeEdit, true);
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("productlist.cshtml");
            _dataObject.SetDataObject("productlist", articleDataList);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleDataList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetProductSelectList()
        {
            var orderid = _paramInfo.GetXmlProperty("genxml/hidden/orderid");
            _dataObject.Settings.Add("orderid", orderid);
            var portalShop = new PortalShopLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCodeEdit);
            var articleDataList = new ProductLimpetList(_sessionParams, _dataObject.PortalShop, _sessionParams.CultureCodeEdit, true);
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("ProductSelectList.cshtml");
            _dataObject.SetDataObject("productlist", articleDataList);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleDataList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetProductSelectDetail()
        {
            var orderid = _paramInfo.GetXmlProperty("genxml/hidden/orderid");
            _dataObject.Settings.Add("orderid", orderid);
            var itemid = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (itemid <= 0) return "Invalid productId.";
            var articleData = new ProductLimpet(_dataObject.PortalShop.PortalId, itemid, _sessionParams.CultureCodeEdit);
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("ProductSelectDetail.cshtml");
            _dataObject.SetDataObject("productdata", articleData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicProductListHeader(string appendix)
        {
            // assume we want a product page, if we have a productid
            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productid == 0) productid = _paramInfo.GetXmlPropertyInt("genxml/urlparams/productid");
            if (productid > 0)
            {
                var articleData = new ProductLimpet(_dataObject.PortalShop.PortalId, productid, _sessionParams.CultureCode);
                _dataObject.SetDataObject("productdata", articleData);
            }

            var razorTempl = AssignRemoteTemplate(appendix);
            if (razorTempl == "") return "";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalShop.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicProductList()
        {
            var template = _paramInfo.GetXmlProperty("genxml/hidden/template");
            return RocketEcommerceAPIUtils.DisplayView(_dataObject, _paramInfo, template);
        }
        public String GetPublicProductDetail()
        {
            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productid == 0) productid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/productid");
            var articleData = new ProductLimpet(_dataObject.PortalShop.PortalId, productid, _sessionParams.CultureCodeEdit);

            if (!articleData.Exists) return "404";

            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template.  Check engine server. Theme: '" + _dataObject.AppTheme.AppThemeFolder;

            _dataObject.SetDataObject("productdata", articleData);

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, _dataObject.PortalShop.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicArticleSEO()
        {
            // check if we are looking for a detail page. 
            var sRec = new SimplisityRecord();
            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productid == 0) productid = _paramInfo.GetXmlPropertyInt("genxml/remote/urlparams/productid");
            if (productid > 0)
            {
                var articleData = new ProductLimpet(_dataObject.PortalShop.PortalId, productid, _sessionParams.CultureCodeEdit);
                _dataObject.SetDataObject("productdata", articleData);
                // do detail
                sRec.SetXmlProperty("genxml/title", articleData.Name);
                sRec.SetXmlProperty("genxml/description", articleData.Summary);
                sRec.SetXmlProperty("genxml/keywords", articleData.Keywords);
            }
            return sRec.ToXmlItem();
        }
        public string ProductDocumentList()
        {
            var productid = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var docList = new List<object>();
            foreach (var i in DNNrocketUtils.GetFiles(DNNrocketUtils.MapPath(_dataObject.PortalShop.DocFolderRel)))
            {
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/name", i.Name);
                sInfo.SetXmlProperty("genxml/relname", _dataObject.PortalShop.DocFolderRel + "/" + i.Name);
                sInfo.SetXmlProperty("genxml/fullname", i.FullName);
                sInfo.SetXmlProperty("genxml/extension", i.Extension);
                sInfo.SetXmlProperty("genxml/directoryname", i.DirectoryName);
                sInfo.SetXmlProperty("genxml/lastwritetime", i.LastWriteTime.ToShortDateString());
                docList.Add(sInfo);
            }

            _dataObject.Settings.Add("uploadcmd", "productadmin_docupload");
            _dataObject.Settings.Add("deletecmd", "productadmin_docdelete");
            _dataObject.Settings.Add("productid", productid.ToString());

            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("DocumentSelect.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, docList, new Dictionary<string, object>(), _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;

        }
        public void ProductDocumentUploadToFolder()
        {
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(_dataObject.PortalShop.DocFolderMapPath)) Directory.CreateDirectory(_dataObject.PortalShop.DocFolderMapPath);
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, _dataObject.PortalShop.DocFolderMapPath + "\\" + friendlyname, true);
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }

            }
        }
        public void ProductDeleteDocument()
        {
            var docfolder = _postInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (docfolder == "") docfolder = "docs";
            var docDirectory = PortalUtils.HomeDNNrocketDirectoryMapPath() + "\\" + docfolder;
            var docList = _postInfo.GetXmlProperty("genxml/hidden/dnnrocket-documentlist").Split(';');
            foreach (var i in docList)
            {
                if (i != "")
                {
                    var documentname = GeneralUtils.DeCode(i);
                    var docFile = docDirectory + "\\" + documentname;
                    if (File.Exists(docFile))
                    {
                        File.Delete(docFile);
                    }
                }
            }

        }
        public string AssignProductCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveProduct(articleId);
            articleData.AddCategory(categoryId);
            return GetArticleCategoryList(articleData);
        }
        public string AssignDefaultProductCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveProduct(articleId);
            articleData.SetDefaultCategory(categoryId);
            articleData = GetActiveProduct(articleId);
            return GetArticleCategoryList(articleData);
        }
        public string RemoveProductCategory()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var categoryId = _paramInfo.GetXmlPropertyInt("genxml/hidden/categoryid");
            var articleData = GetActiveProduct(articleId);
            articleData.RemoveCategory(categoryId);
            return GetArticleCategoryList(articleData);
        }
        public string AssignArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveProduct(articleId);
            articleData.AddProperty(propertyId);

            return GetProductPropertyList(articleData);
        }
        public string RemoveArticleProperty()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
            var propertyId = _paramInfo.GetXmlPropertyInt("genxml/hidden/propertyid");
            var articleData = GetActiveProduct(articleId);
            articleData.RemoveProperty(propertyId);

            return GetProductPropertyList(articleData);
        }
        public String GetArticleCategoryList(ProductLimpet articleData)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("ProductCategoryList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;
        }
        public String GetProductPropertyList(ProductLimpet articleData)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("ProductPropertyList.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            return pr.RenderedText;
        }



    }
}
