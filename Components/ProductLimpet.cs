using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RocketEcommerceAPI.Components
{
    public class ProductLimpet
    {
        public const string _tableName = "RocketEcommerceAPI";
        public const string _entityTypeCode = "PRD";
        private DNNrocketController _objCtrl;
        private int _productId;
        private List<SimplisityInfo> _propXrefList;
        private List<int> _propXrefListId;
        private List<string> _propXrefListRef;
        private List<SimplisityInfo> _catXrefList;
        private List<int> _catXrefListId;
        private List<string> _catXrefListRef;
        private string _cacheKey;
        private SessionParams sessionParam;
        private int aticleId;
        private string systemKey;

        public ProductLimpet()
        {
            Info = new SimplisityInfo();
        }
        public ProductLimpet(string xmlExportItem, string langRequired = "")
        {
            Info = new SimplisityInfo();
            Info.FromXmlItem(xmlExportItem);
            CultureCode = langRequired;
            if (CultureCode == "") CultureCode = Info.Lang;
            Info.Lang = CultureCode;
            PortalId = Info.PortalId;
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
        }
        /// <summary>
        /// Should be used to create an product, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="productId"></param>
        /// <param name="langRequired"></param>
        public ProductLimpet(int portalId, int productId, string langRequired = "")
        {
            if (productId <= 0) productId = -1;  // create new record.
            _productId = productId;
            PortalId = portalId;
            Info = new SimplisityInfo();
            Info.ItemID = -1;
            Info.TypeCode = _entityTypeCode;
            Info.ModuleId = -1;
            Info.UserId = -1;
            Info.PortalId = PortalId;

            Populate(langRequired);
        }

        public ProductLimpet(SessionParams sessionParam, int portalId, int aticleId, string cultureCode, string systemKey)
        {
            this.sessionParam = sessionParam;
            PortalId = portalId;
            this.aticleId = aticleId;
            CultureCode = cultureCode;
            this.systemKey = systemKey;
        }

        private void Populate(string cultureCode)
        {
            _cacheKey = "ProductLimpet*" + PortalId + "*" + _productId + "*" + cultureCode + "*" + _tableName;

            CultureCode = cultureCode;

            PortalShop = new PortalShopLimpet(PortalId, CultureCode);

            _objCtrl = new DNNrocketController();
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();

            var info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (info == null)
            {
                if (_productId > 0)
                {
                    info = _objCtrl.GetInfo(_productId, CultureCode, _tableName); // get existing record.
                    if (info != null && info.ItemID > 0) Info = info; // check if we have a real record, or a dummy being created and not saved yet.
                    if (GetModelList().Count <= 0) AddModel();
                }
            }
            else
                Info = info;

            PortalId = Info.PortalId;
            Info.Lang = CultureCode;
        }
        public void PopulateLists()
        {
            _propXrefList = _objCtrl.GetList(PortalId, -1, "PROPXREF", " and R1.[ParentItemId] = " + ProductId + " ", "", "", 0, 0, 0, 0, _tableName);
            _propXrefListId = new List<int>();
            _propXrefListRef = new List<string>();
            foreach (var p in _propXrefList)
            {
                var c = new PropertyLimpet(PortalId, p.XrefItemId, CultureCode);
                _propXrefListId.Add(p.XrefItemId);
                _propXrefListRef.Add(c.Ref);
            }
            _catXrefList = _objCtrl.GetList(PortalId, -1, "CATXREF", " and R1.[ParentItemId] = " + ProductId + " ", "", "", 0, 0, 0, 0, _tableName);
            _catXrefListId = new List<int>();
            _catXrefListRef = new List<string>();
            foreach (var p in _catXrefList)
            {
                var c = new CategoryLimpet(PortalId, p.XrefItemId, CultureCode);
                _catXrefListId.Add(p.XrefItemId);
                _catXrefListRef.Add(c.Ref);
            }
        }


        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*","") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, SecurityInput.RemoveScripts(nod.InnerText));
                }
            }
        }
        public int Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");

            UpdateModels(postInfo.GetList("modellist"));
            UpdateOptions(postInfo.GetList("optionlist"));
            UpdateImages(postInfo.GetList("imagelist"));
            UpdateDocs(postInfo.GetList("documentlist"));
            UpdateLinks(postInfo.GetList("linklist"));

            //------------------------------------------------------------------------------------------
            //NOTE: THE FORMAT FIX CAN ONLY BE DONE IN THE SAVE, IT FIXES THE INVALID HUMAN INPUT.
            // Fix incorrect money value.  All money should be kept in int.
            #region "Money Format"
            var lp = 1;
            var modelList = Info.GetList("modellist");
            foreach (var model in modelList)
            {
                Info.SetXmlPropertyInt("genxml/modellist/genxml[" + lp + "]/textbox/modelprice", PortalShop.CurrencyConvertCents(model.GetXmlProperty("genxml/textbox/modelprice")).ToString());
                Info.SetXmlPropertyInt("genxml/modellist/genxml[" + lp + "]/textbox/modelsaleprice", PortalShop.CurrencyConvertCents(model.GetXmlProperty("genxml/textbox/modelsaleprice")).ToString());
                lp += 1;
            }
            var lp2 = 1;
            var optionList = Info.GetList("optionlist");
            foreach (var option in optionList)
            {
                Info.SetXmlPropertyInt("genxml/optionlist/genxml[" + lp2 + "]/textbox/optionprice", PortalShop.CurrencyConvertCents(option.GetXmlProperty("genxml/textbox/optionprice")).ToString());
                // make sure the key is a valid XML node name (put "key" in front).  This is for imported refs from OpenStore, which might not be compatible.
                var keyData = option.GetXmlProperty("genxml/hidden/optionkey");
                if (!keyData.StartsWith("k")) keyData = "k" + GeneralUtils.GetGuidKey();
                Info.SetXmlProperty("genxml/optionlist/genxml[" + lp2 + "]/hidden/optionkey", keyData);

                lp2 += 1;
            }

            int pmax = 0;
            int pmin = 0;
            int psmax = 0;
            int psmin = 0;
            var modelList2 = Info.GetList("modellist");
            foreach (var m in modelList2)
            {
                var mPrice = m.GetXmlPropertyInt("genxml/textbox/modelprice");
                if (mPrice < pmin || pmin == 0) pmin = mPrice;
                if (mPrice > pmax || pmax == 0) pmax = mPrice;
                var msPrice = m.GetXmlPropertyInt("genxml/textbox/modelsaleprice");
                if (msPrice < psmin || psmin == 0) psmin = msPrice;
                if (msPrice > psmax || psmax == 0) psmax = msPrice;
            }
            if (psmin == 0) psmin = psmax; // if we have a sale price, the minimum should always be set.
            Info.SetXmlPropertyInt("genxml/priceminimum", pmin.ToString());
            Info.SetXmlPropertyInt("genxml/pricemaximum", pmax.ToString());
            Info.SetXmlPropertyInt("genxml/salepriceminimum", psmin.ToString());
            Info.SetXmlPropertyInt("genxml/salepricemaximum", psmax.ToString());
            var bestprice = pmin;
            if (psmin != 0) bestprice = psmin;
            Info.SetXmlPropertyInt("genxml/bestprice", bestprice.ToString());
            #endregion
            //------------------------------------------------------------------------------------------

            // Add SortOrder, it's dropped by the XML update.
            postInfo.SetXmlProperty("genxml/sortorder", postInfo.GetXmlProperty("genxml/sortorder").Replace(".","").Replace(",", ""));
            Info.SortOrder = postInfo.GetXmlPropertyInt("genxml/sortorder");

            if (GetModelList().Count == 0)
            {
                AddModel();
            }

            return ValidateAndUpdate();
        }
        public int Copy()
        {
            Info.ItemID = -1;
            Info.GUIDKey = GeneralUtils.GetUniqueString();
            ClearCache();
            var i = Update(false);
            return i;
        }
        public void ClearCache()
        {
            CacheUtils.RemoveCache(_cacheKey);
        }

        public int Update(bool cacheData = true)
        {
            Info = _objCtrl.SaveData(Info, _tableName);
            if (Info.GUIDKey == "")
            {
                // get unique ref
                Info.GUIDKey = GeneralUtils.GetUniqueString();
                Info = _objCtrl.SaveData(Info, _tableName);
            }
            _objCtrl.RebuildIndex(PortalId, Info.ItemID, SystemKey, _tableName);

            // Rebuild cacheKey, if we are craeting a new product, we will have -1 for productid in the old key.
            _productId = Info.ItemID;
            _cacheKey = "ProductLimpet*" + PortalId + "*" + _productId + "*" + Info.Lang + "*" + _tableName;

            if (cacheData) CacheUtils.SetCache(_cacheKey, Info);

            return Info.ItemID;
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public void AddListItem(string listname)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(listname);
            Update();
        }
        public void Validate()
        {
            // Fix List attr flag (required for handlebars)
            var InfoClone = (SimplisityInfo)Info.Clone();
            if (Info.GetXmlProperty("genxml/modellist/@list") == "") UpdateModels(InfoClone.GetList("modellist"));
            if (Info.GetXmlProperty("genxml/optionlist/@list") == "") UpdateOptions(InfoClone.GetList("optionlist"));
            if (Info.GetXmlProperty("genxml/imagelist/@list") == "") UpdateImages(InfoClone.GetList("imagelist"));
            if (Info.GetXmlProperty("genxml/documentlist/@list") == "") UpdateDocs(InfoClone.GetList("documentlist"));
            if (Info.GetXmlProperty("genxml/linklist/@list") == "") UpdateLinks(InfoClone.GetList("linklist"));
        }

        public int DefaultCategory()
        {
            // use ModuleId to flag default
            if (_catXrefList == null) PopulateLists();
            foreach (var catxrefInfo in _catXrefList)
            {
                if (catxrefInfo.ModuleId > 0) return catxrefInfo.XrefItemId;
            }
            return 0;
        }
        public void SetDefaultCategory(int categoryId)
        {
            // use ModuleId to flag default
            if (_catXrefList == null) PopulateLists();
            foreach (var catxrefInfo in _catXrefList)
            {
                if (catxrefInfo.XrefItemId == categoryId)
                    catxrefInfo.ModuleId = 1;
                else
                    catxrefInfo.ModuleId = 0;
                _objCtrl.Update(catxrefInfo, _tableName);
            }
        }


        #region "models"

        public void UpdateModels(List<SimplisityInfo> modelList)
        {
            Info.RemoveList("modellist");
            foreach (var sInfo in modelList)
            {
                var modelData = new ProductModel(PortalId, sInfo, CultureCode);
                UpdateModel(modelData);
            }
        }
        public List<SimplisityInfo> GetModelList()
        {
            return Info.GetList("modellist");
        }
        public ProductModel AddModel()
        {
            if (GetModelList().Count < PortalShop.ProductModelLimit)
            {
                var sInfo = new SimplisityInfo();
                var modelkey = GeneralUtils.GetUniqueString();
                sInfo.SetXmlProperty("genxml/hidden/modelkey", modelkey);
                Info.AddListItem("modellist", sInfo);
                return GetModel(modelkey);
            }
            return null;
        }
        public void UpdateModel(ProductModel productModel)
        {
            Info.RemoveListItem("modellist", "genxml/hidden/modelkey", productModel.ModelKey);
            Info.AddListItem("modellist", productModel.Info);
        }
        public ProductModel GetModel(int idx)
        {
            return new ProductModel(PortalId, Info.GetListItem("modellist", idx), CultureCode);
        }
        public ProductModel GetModel(string modelKey)
        {
            return new ProductModel(PortalId, Info.GetListItem("modellist", "genxml/hidden/modelkey", modelKey), CultureCode);
        }
        public List<ProductModel> GetModels()
        {
            var rtn = new List<ProductModel>();
            foreach (var i in Info.GetList("modellist"))
            {
                rtn.Add(new ProductModel(PortalId, i, CultureCode));
            }
            return rtn;
        }
        public Dictionary<string, string> GetModelDictionary(string template = "{ref} {name} {price}", string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
           var rtn = new Dictionary<string,string>();
            foreach (var modelData in GetModels())
            {
                if (!rtn.ContainsKey(modelData.ModelKey) && modelData.ModelKey != "")
                {
                    var portalShop = new PortalShopLimpet(PortalId, DNNrocketUtils.GetCurrentCulture());
                    var temp = template.Replace("{ref}", modelData.Ref);
                    temp = temp.Replace("{name}", modelData.Name);
                    temp = temp.Replace("{price}", modelData.BestPriceDisplay(cultureCode));
                    rtn.Add(modelData.ModelKey, temp.Trim(' ')) ;
                }
            }
            return rtn;
        }
        #endregion

        #region "images"
        public void UpdateImages(List<SimplisityInfo> imageList)
        {
            Info.RemoveList("imagelist");
            foreach (var sInfo in imageList)
            {
                var imgData = new ArticleImage(sInfo, "productimage");
                UpdateImage(imgData);
            }
        }
        public List<SimplisityInfo> GetImageList()
        {
            return Info.GetList("imagelist");
        }
        public ArticleImage AddImage(string uniqueName)
        {
            var articleImage = new ArticleImage(new SimplisityInfo(), "productimage");
            if (GetImageList().Count < PortalShop.ProductImageLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                articleImage.RelPath = PortalShop.ImageFolderRel.TrimEnd('/') + "/" + uniqueName.Trim('\\');
                Info.AddListItem("imagelist", articleImage.Info);
                Update();
            }
            return articleImage;
        }
        public void UpdateImage(ArticleImage articleImage)
        {
            Info.RemoveListItem("imagelist", "genxml/hidden/imagekey", articleImage.ImageKey);
            Info.AddListItem("imagelist", articleImage.Info);
        }
        public string DefaultImageUrl()
        {
            var i = GetImage(0);
            if (i == null)
                return "/DesktopModules/DNNrocket/api/images/noimage2.png";
            else
                return "/" + i.RelPath.Trim('/');
        }
        public ArticleImage GetImage(int idx)
        {
            return new ArticleImage(Info.GetListItem("imagelist", idx), "productimage");
        }
        public List<ArticleImage> GetImages()
        {
            var rtn = new List<ArticleImage>();
            foreach ( var i in Info.GetList("imagelist"))
            {
                rtn.Add(new ArticleImage(i, "productimage"));
            }
            return rtn;
        }
        #endregion

        #region "docs"
        public string DocumentListName { get { return "documentlist"; } }
        public void UpdateDocs(List<SimplisityInfo> docList)
        {
            Info.RemoveList(DocumentListName);
            foreach (var sInfo in docList)
            {
                var docData = new ArticleDoc(sInfo, "articledoc");
                UpdateDoc(docData);
            }
        }
        public List<SimplisityInfo> GetDocList()
        {
            return Info.GetList(DocumentListName);
        }
        public ArticleDoc AddDoc(string uniqueName)
        {
            var articleDoc = new ArticleDoc(new SimplisityInfo(), "articledoc");
            if (GetDocList().Count < PortalShop.ProductDocumentLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                articleDoc.RelPath = PortalShop.DocFolderRel.TrimEnd('/') + "/" + uniqueName;
                articleDoc.Name = uniqueName;
                Info.AddListItem(DocumentListName, articleDoc.Info);
                Update();
            }
            return articleDoc;
        }
        public void UpdateDoc(ArticleDoc articleDoc)
        {
            Info.RemoveListItem(DocumentListName, "genxml/hidden/dockey", articleDoc.DocKey);
            Info.AddListItem(DocumentListName, articleDoc.Info);
        }
        public ArticleDoc GetDoc(int idx)
        {
            return new ArticleDoc(Info.GetListItem(DocumentListName, idx), "articledoc");
        }
        public List<ArticleDoc> GetDocs()
        {
            var rtn = new List<ArticleDoc>();
            foreach (var i in Info.GetList(DocumentListName))
            {
                rtn.Add(new ArticleDoc(i, "articledoc"));
            }
            return rtn;
        }
        #endregion

        #region "links"
        public void UpdateLinks(List<SimplisityInfo> linkList)
        {
            Info.RemoveList("linklist");
            foreach (var sInfo in linkList)
            {
                var linkData = new ArticleLink(sInfo, "productlink");
                UpdateLink(linkData);
            }
        }
        public List<SimplisityInfo> GetLinkList()
        {
            return Info.GetList("linklist");
        }
        public ArticleLink AddLink()
        {
            var articleLink = new ArticleLink(new SimplisityInfo(), "productlink");
            if (GetLinkList().Count < PortalShop.ProductLinkLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                Info.AddListItem("linklist", articleLink.Info);
                Update();
            }
            return articleLink;
        }
        public void UpdateLink(ArticleLink articleLink)
        {
            Info.RemoveListItem("linklist", "genxml/hidden/linkkey", articleLink.LinkKey);
            Info.AddListItem("linklist", articleLink.Info);
        }
        public ArticleLink Getlink(int idx)
        {
            return new ArticleLink(Info.GetListItem("linklist", idx), "productlink");
        }
        public List<ArticleLink> Getlinks()
        {
            var rtn = new List<ArticleLink>();
            foreach (var i in Info.GetList("linklist"))
            {
                rtn.Add(new ArticleLink(i, "productlink"));
            }
            return rtn;
        }
        #endregion

        #region "options"
        public void UpdateOptions(List<SimplisityInfo> optionList)
        {
            // Get dictionary of OptionFields in each Option
            var optionFieldList = new Dictionary<string, List<ProductOptionField>>();
            foreach (var productOption in GetOptions())
            {
                var fieldList = new List<ProductOptionField>();
                foreach (var f in productOption.GetOptionFields())
                {
                    fieldList.Add(f);
                }
                optionFieldList.Add(productOption.OptionKey, fieldList);
            }
            // Do update and add existing fields.
            Info.RemoveList("optionlist");
            foreach (var sInfo in optionList)
            {
                var optionData = new ProductOptionLimpet(PortalId, sInfo, ProductId, CultureCode);
                if (optionFieldList.ContainsKey(optionData.OptionKey))
                {
                    var fieldList = optionFieldList[optionData.OptionKey];
                    foreach (var f in fieldList)
                    {
                        optionData.AddOptionField(f);
                    }
                }
                UpdateOption(optionData);
            }
        }
        public List<SimplisityInfo> GetOptionList()
        {
            return Info.GetList("optionlist");
        }
        public ProductOptionLimpet AddOption()
        {
            if (GetOptionList().Count < PortalShop.ProductOptionLimit)
            {
                if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
                var sInfo = new SimplisityInfo();
                var optionkey = "k" + GeneralUtils.GetGuidKey();
                sInfo.SetXmlProperty("genxml/hidden/optionkey", optionkey);
                Info.AddListItem("optionlist", sInfo);
                Update();
                return GetOption(optionkey);
            }
            return null;
        }
        public void UpdateOption(ProductOptionLimpet productOption)
        {
            Info.RemoveListItem("optionlist", "genxml/hidden/optionkey", productOption.OptionKey);
            Info.AddListItem("optionlist", productOption.Info);
        }
        public void ReplaceOption(ProductOptionLimpet productOption)
        {
            // We want to keep the sort order.
            // So rebuild in old order and replace.  [XML - replaceChild(x,y) should do this, but cannot get it to work]
            var l = GetOptions();
            Info.RemoveList("optionlist");
            foreach (var o in l)
            {
                if (o.OptionKey == productOption.OptionKey)
                    Info.AddListItem("optionlist", productOption.Info);
                else
                    Info.AddListItem("optionlist", o.Info);
            }
        }

        public ProductOptionLimpet GetOption(int idx)
        {
            return new ProductOptionLimpet(PortalId, Info.GetListItem("optionlist", idx), ProductId, CultureCode);
        }
        public ProductOptionLimpet GetOption(string optionKey)
        {
            return new ProductOptionLimpet(PortalId, Info.GetListItem("optionlist", "genxml/hidden/optionkey", optionKey), ProductId, CultureCode);
        }
        public List<ProductOptionLimpet> GetOptions()
        {
            var rtn = new List<ProductOptionLimpet>();
            foreach (var i in Info.GetList("optionlist"))
            {
                rtn.Add(new ProductOptionLimpet(PortalId, i, ProductId, CultureCode));
            }
            return rtn;
        }

        #endregion

        #region "category"
        public void AddCategory(int categoryId, bool cascade = true)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null)
            {
                var xrefGuidKey = GUIDKey + "-" + catRecord.GUIDKey;
                var catXrefRecord = _objCtrl.GetRecordByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                if (catXrefRecord == null)
                {
                    var sRec = new SimplisityRecord();
                    sRec.ItemID = -1;
                    sRec.PortalId = PortalId;
                    sRec.ParentItemId = ProductId;
                    sRec.XrefItemId = categoryId;
                    sRec.TypeCode = "CATXREF";
                    sRec.GUIDKey = xrefGuidKey;
                    _objCtrl.Update(sRec, _tableName);

                    if (cascade) AddCasCadeCategory(categoryId, 0);
                }
            }
        }
        private void AddCasCadeCategory(int categoryId, int levelCount)
        {
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null && catRecord.ParentItemId > 0 && levelCount < 50) // use levelCount to stop infinate loop for corrupt data
            {
                if (!IsInCategory(catRecord.ParentItemId))
                {
                    var catParentRecord = _objCtrl.GetRecord(catRecord.ParentItemId, _tableName);
                    var xrefGuidKey = GUIDKey + "-" + catParentRecord.GUIDKey;
                    var casCadeRec = _objCtrl.GetByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                    if (casCadeRec == null)
                    {
                        var sRec = new SimplisityRecord();
                        sRec.ItemID = -1;
                        sRec.PortalId = PortalId;
                        sRec.ParentItemId = ProductId;
                        sRec.XrefItemId = catParentRecord.ItemID;
                        sRec.TypeCode = "CATXREF";
                        sRec.GUIDKey = xrefGuidKey;
                        _objCtrl.Update(sRec, _tableName);
                    }
                }
                AddCasCadeCategory(catRecord.ParentItemId, levelCount + 1);
            }
        }

        public void RemoveCategory(int categoryId)
        {

            var filter = " and xrefitemid = " + categoryId + " and ParentItemid = " + ProductId + " ";
            var l = _objCtrl.GetList(PortalId, -1, "CATXREF", filter, "", "", 0, 0, 0, 0, _tableName);
            foreach (var cx in l)
            {
                //RemoveCasCadeCategory(categoryId);
                _objCtrl.Delete(cx.ItemID, _tableName);
            }
            Update();
        }
        private void RemoveCasCadeCategory(int categoryId)
        {
            var catRecord = _objCtrl.GetRecord(categoryId, _tableName);
            if (catRecord != null && catRecord.ParentItemId > 0)
            {
                var catParentRecord = _objCtrl.GetRecord(catRecord.ParentItemId, _tableName);                
                var xrefGuidKey = GUIDKey + "-" + catParentRecord.GUIDKey;
                var casCadeRec = _objCtrl.GetByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
                if (casCadeRec != null)
                {
                    _objCtrl.Delete(casCadeRec.ItemID, _tableName);
                }
                RemoveCasCadeCategory(catRecord.ParentItemId);
            }
        }
        public void UpdateCategorySortOrder(string categoryGUIDKey, int sortOrder)
        {
            var xrefGuidKey = GUIDKey + "-" + categoryGUIDKey;
            var catXrefRecord = _objCtrl.GetRecordByGuidKey(PortalId, -1, "CATXREF", xrefGuidKey, "", _tableName);
            if (catXrefRecord != null)
            {
                catXrefRecord.SortOrder = sortOrder;
                _objCtrl.Update(catXrefRecord, _tableName);
            }
        }

        public List<CategoryLimpet> GetCategories()
        {
            var rtn = new List<CategoryLimpet>();
            var l = _objCtrl.GetList(PortalId, -1, "CATXREF", " and R1.[ParentItemId] = " + ProductId + " ", "", "", 0, 0, 0, 0, _tableName);
            foreach (var i in l)
            {
                var categoryId = i.XrefItemId;
                var catData = new CategoryLimpet(PortalId, categoryId, CultureCode.ToLower());
                if (catData.Exists)
                    rtn.Add(catData);
                else
                    RemoveCategory(categoryId);
            }
            return rtn;
        }
        public bool IsInCategory(int categoryId)
        {
            var l = _objCtrl.GetList(PortalId, -1, "CATXREF", " and R1.[ParentItemId] = " + ProductId + " and [XrefItemId] = " + categoryId + " ", "", "", 0, 0, 0, 0, _tableName);
            if (l.Count > 0) return true;
            return false;
        }
        public bool IsInCategory(string categoryRef)
        {
            if (_catXrefListId == null) PopulateLists();
            if (_catXrefListRef.Contains(categoryRef)) return true;
            return false;
        }
        #endregion

        #region "property"
        public void AddProperty(int propertyId)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var catRecord = _objCtrl.GetRecord(propertyId, _tableName);
            if (catRecord != null)
            {
                var xrefGuidKey = GUIDKey + "-" + catRecord.GUIDKey;
                var catXrefRecord = _objCtrl.GetRecordByGuidKey(PortalId, -1, "PROPXREF", xrefGuidKey, "", _tableName);
                if (catXrefRecord == null)
                {
                    var sRec = new SimplisityRecord();
                    sRec.ItemID = -1;
                    sRec.PortalId = PortalId;
                    sRec.ParentItemId = ProductId;
                    sRec.XrefItemId = propertyId;
                    sRec.TypeCode = "PROPXREF";
                    sRec.GUIDKey = xrefGuidKey;
                    _objCtrl.Update(sRec, _tableName);
                }
            }
        }
        public void RemoveProperty(int propertyId)
        {
            var filter = " and xrefitemid = " + propertyId + " and ParentItemid = " + ProductId + " ";
            var l = _objCtrl.GetList(PortalId, -1, "PROPXREF", filter, "", "", 0, 0, 0, 0, _tableName);
            foreach (var cx in l)
            {
                _objCtrl.Delete(cx.ItemID, _tableName);
            }
            Update();
        }
        public List<SimplisityInfo> GetPropertiesInfo()
        {
            if (_propXrefList == null) PopulateLists();
            return _propXrefList;
        }
        public List<PropertyLimpet> GetProperties()
        {
            if (_propXrefListId == null) PopulateLists();
            var rtn = new List<PropertyLimpet>();
            foreach (var propertyId in _propXrefListId)
            {
                var propertyData = new PropertyLimpet(PortalId, propertyId, CultureCode);
                if (propertyData.Exists)
                    rtn.Add(propertyData);
                else
                    RemoveProperty(propertyId);
            }
            return rtn;
        }
        public bool HasProperty(int propertyId)
        {
            if (_propXrefListId == null) PopulateLists();
            if (_propXrefListId.Contains(propertyId)) return true;
            return false;
        }
        public bool HasProperty(string propertyRef)
        {
            if (_propXrefListId == null) PopulateLists();
            if (_propXrefListRef.Contains(propertyRef)) return true;
            return false;
        }

        #endregion

        #region "properties"

        public string CultureCode { get; private set; }
        public string EntityTypeCode { get { return _entityTypeCode; } }
        public SimplisityInfo Info { get; set; }
        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int ParentItemId { get { return Info.ParentItemId; } set { Info.ParentItemId = value; } }
        public int ProductId { get { return Info.ItemID; } set { Info.ItemID = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public string ImageFolder { get; set; }
        public string DocumentFolder { get; set; }
        public string AppTheme { get; set; }
        public string AppThemeVersion { get; set; }
        public string AppThemeRelPath { get; set; }
        public PortalShopLimpet PortalShop { get; set; }
        public bool DebugMode { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get {if (Info.ItemID  <= 0) { return false; } else { return true; }; } }
        [Obsolete("Use DefaultImageUrl() or GetImage(0) ")]
        public string LogoRelPath
        {
            get
            {
                var productImage = GetImage(0);
                return productImage.RelPath;
            }
        }
        public string Name { get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/productname"); } set { Info.SetXmlProperty("genxml/lang/genxml/textbox/productname", value); } }
        public string Ref { get { return Info.GetXmlProperty("genxml/textbox/productref"); } set { Info.SetXmlProperty("genxml/textbox/productref", value); } }
        public string Brand { get { return Info.GetXmlProperty("genxml/textbox/brand"); } set { Info.SetXmlProperty("genxml/textbox/brand", value); } }
        public string Keywords { get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/productkeywords"); } set { Info.SetXmlProperty("genxml/lang/genxml/textbox/productkeywords", value); } }
        public string Summary { get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/productsummary"); } set { Info.SetXmlProperty("genxml/lang/genxml/textbox/productsummary", value); } }
        public string RichText { get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/richtext"); } set { Info.SetXmlProperty("genxml/lang/genxml/textbox/richtext", value); } }        
        public int MaxPurchase { get { return Info.GetXmlPropertyInt("genxml/hidden/maxpurchase"); } set { Info.SetXmlProperty("genxml/hidden/maxpurchase", value.ToString()); } }
        public int MinPurchase { get { return Info.GetXmlPropertyInt("genxml/hidden/minpurchase"); } set { Info.SetXmlProperty("genxml/hidden/minpurchase", value.ToString()); } }
        public double CartQty { get { return Info.GetXmlPropertyDouble("genxml/cart/qty"); } set { Info.SetXmlPropertyDouble("genxml/cart/qty", value); } }
        public string CartModelKey { get { return Info.GetXmlProperty("genxml/cart/modelkey"); } set { Info.SetXmlPropertyDouble("genxml/cart/modelkey", value.ToString()); } }
        public double CartTotal { get { return Info.GetXmlPropertyDouble("genxml/cart/total"); } set { Info.SetXmlPropertyDouble("genxml/cart/total", value); } }
        public double CartSubTotal { get { return Info.GetXmlPropertyDouble("genxml/cart/subtotal"); } set { Info.SetXmlPropertyDouble("genxml/cart/subtotal", value); } }
        public double CartTotalTax { get { return Info.GetXmlPropertyDouble("genxml/cart/totaltax"); } set { Info.SetXmlPropertyDouble("genxml/cart/totaltax", value); } }
        public double CartTotalShipping { get { return Info.GetXmlPropertyDouble("genxml/cart/totalshipping"); } set { Info.SetXmlPropertyDouble("genxml/cart/totalshipping", value); } }
        public double CartTotalDiscount { get { return Info.GetXmlPropertyDouble("genxml/cart/totaldiscount"); } set { Info.SetXmlPropertyDouble("genxml/cart/totaldiscount", value); } }
        public bool IsHidden { get { return Info.GetXmlPropertyBool("genxml/checkbox/hidden"); } }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        /// <summary>
        /// Minimum price of a model.  (Without any change for discount, etc.)
        /// </summary>
        /// 
        public int PriceMinimumCents { get { return Info.GetXmlPropertyInt("genxml/priceminimum"); } }
        public decimal PriceMinimum
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceMinimumCents)); }
        }
        public string PriceMinimumDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return PriceMinimum.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public int PriceMaximumCents { get { return Info.GetXmlPropertyInt("genxml/pricemaximum"); } }
        public decimal PriceMaximum
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceMaximumCents)); }
        }
        public string PriceMaximumDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return PriceMaximum.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public string PriceMaximumDisplayNoSymbol(string cultureCode = "")
        {
            return PriceMaximumDisplay(cultureCode).Replace(PortalShop.CurrencySymbol, "").Trim();
        }

        // SALE PRICE
        public int SalePriceMinimumCents { get { return Info.GetXmlPropertyInt("genxml/salepriceminimum"); } }
        public decimal SalePriceMinimum
        {
            get { return (PortalShop.CurrencyCentsToDollars(SalePriceMinimumCents)); }
        }
        public string SalePriceMinimumDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return SalePriceMinimum.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public int SalePriceMaximumCents { get { return Info.GetXmlPropertyInt("genxml/salepricemaximum"); } }
        public decimal SalePriceMaximum
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceMaximumCents)); }
        }
        public string SalePriceMaximumDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return SalePriceMaximum.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }
        public string SalePriceMaximumDisplayNoSymbol(string cultureCode = "")
        {
            return SalePriceMaximumDisplay(cultureCode).Replace(PortalShop.CurrencySymbol, "").Trim();
        }

        // BEST PRICE
        public int BestPriceCents { get { return Info.GetXmlPropertyInt("genxml/bestprice"); } }
        public decimal BestPrice
        {
            get { return (PortalShop.CurrencyCentsToDollars(BestPriceCents)); }
        }
        public string BestPriceDisplay(string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = PortalShop.CurrencyCultureCode;
            return BestPrice.ToString("C", CultureInfo.GetCultureInfo(cultureCode));
        }


        #endregion

    }

}
