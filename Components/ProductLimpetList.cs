using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{

    public class ProductLimpetList
    {
        private string _langRequired;
        private List<ProductLimpet> _articleList;
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "PRD";
        private DNNrocketController _objCtrl;
        private string _searchFilter;
        private int _searchcategoryid;
        private int _catidurl;        
        private string _systemKey;
        private string _propRef;
        private int _catid;

        public ProductLimpetList(int categoryId, PortalShopLimpet portalShop, string langRequired, bool populate)
        {
            PortalShop = portalShop;
            _systemKey = portalShop.SystemKey;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            var paramInfo = new SimplisityInfo();
            SessionParamData = new SessionParams(paramInfo);
            SessionParamData.PageSize = 0;

            _searchcategoryid = categoryId;

            if (populate) Populate();
        }
        public ProductLimpetList(SessionParams sessionParams, PortalShopLimpet portalShop, string langRequired, bool populate, bool showHidden = true, int defaultCategoryId = 0)
        {
            InitArticleList(sessionParams, portalShop, langRequired, populate, showHidden, defaultCategoryId);
        }
        private void InitArticleList(SessionParams sessionParams, PortalShopLimpet portalShop, string langRequired, bool populate, bool showHidden = true, int defaultCategoryId = 0)
        {
            SessionParamData = sessionParams;
            PortalShop = portalShop;
            _systemKey = portalShop.SystemKey;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            if (sessionParams.PageSize == 0) sessionParams.PageSize = 24;
            if (sessionParams.Page <= 0) sessionParams.Page = 1;

            _catid = sessionParams.GetInt("catid");
            _catidurl = _catid;
            if (_catid == 0) _catid = defaultCategoryId;

            if (sessionParams.OrderByRef == "" && _catid == 0) sessionParams.OrderByRef = "sqlorderby-product-name";

            if (populate) Populate(showHidden);
        }

        public void Populate(bool showHidden = true)
        {
            ShopSettings = new ShopSettingsLimpet(PortalShop.PortalId, SessionParamData.CultureCode);

            _searchFilter = "";
            ClearFilter = false;
            ClearCategory = false;

            var searchText = PortalShop.GetFilterProductSQL(SessionParamData.Info);
            var propertyFilter = GetPropertyFilterSQL();

            if (!ShopSettings.InCategoryFilter)
            {
                if (_catidurl > 0)
                {
                    searchText = "";
                    propertyFilter = "";
                    ClearPropertyFilters();
                    SessionParamData.SearchText = "";
                }
                if (propertyFilter != "") _searchcategoryid = 0;
            }
            if (searchText != "") _searchcategoryid = 0;

            _searchFilter += searchText;
            _searchFilter += propertyFilter;
            if (_searchcategoryid > 0) _searchFilter += " and [CATXREF].[XrefItemId] = " + _searchcategoryid + " ";

            // Filter hidden
            if (!showHidden) _searchFilter += " and NOT(isnull([XMLData].value('(genxml/checkbox/hidden)[1]','nvarchar(4)'),'false') = 'true') ";

            var orderby = "";
            if (_searchcategoryid > 0 && PortalShop.ManualCategoryOrderby)
                orderby = " order by [CATXREF].[SortOrder] "; // use manual sort for articles by category;
            else
            {
                if (ShopSettings.ManualOrderBy) orderby = PortalShop.OrderByProductSQL("sqlorderby-product-default");
                if (orderby == "") orderby = PortalShop.OrderByProductSQL("sqlorderby-product-name");
            }
            if (orderby == "") orderby = " order by productname.GUIDKey ";

            SessionParamData.RowCount = _objCtrl.GetListCount(PortalShop.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, _tableName);
            RecordCount = SessionParamData.RowCount; 
            DataList = _objCtrl.GetList(PortalShop.PortalId, -1, _entityTypeCode, _searchFilter, _langRequired, orderby, 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        public void DeleteAll()
        {
            var l = GetAllArticlesForShopPortal();
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID);
            }
        }
        private void ClearPropertyFilters()
        {
            ClearFilter = true;
            var nodList = SessionParamData.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    SessionParamData.Info.SetXmlProperty("r/" + nod.Name, "false");
                }
            }
        }
        private string GetPropertyFilterSQL()
        {
            //Filter Property
            var checkboxfilter = "";
            RemoteModule remoteModule = null;
            var nodList = SessionParamData.Info.XMLDoc.SelectNodes("r/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null && nodList.Count > 0) remoteModule = new RemoteModule(PortalShop.PortalId, SessionParamData.ModuleRef);
            foreach (XmlNode nod in nodList)
            {
                if (nod.InnerText.ToLower() == "true")
                {
                    var propid = nod.Name.Replace("checkboxfilter", "");
                    // NOTE: checkbox for filter must be called "checkboxfilterand"
                    if (remoteModule.Record.GetXmlPropertyBool("genxml/checkbox/checkboxfilterand"))
                    {
                        if (checkboxfilter != "") checkboxfilter += " and ";
                        checkboxfilter += " [PROPXREF].[XrefItemId] = " + propid + " ";
                    }
                    else
                    {
                        if (checkboxfilter != "") checkboxfilter += " or ";
                        checkboxfilter += " [PROPXREF].[XrefItemId] = " + propid + " ";
                    }
                }
            }
            if (checkboxfilter != "")
            {
                return " and ( " + checkboxfilter + " ) ";
            }
            return "";
        }

        public bool ClearFilter { get; private set; }
        public bool ClearCategory { get; private set; }
        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> DataList { get; private set; }
        public PortalShopLimpet PortalShop { get; set; }
        public ShopSettingsLimpet ShopSettings { get; private set; }
        public int RecordCount { get; set; }
        public int CategoryId { get { return _searchcategoryid; } }
        public List<ProductLimpet> GetArticleList()
        {
            _articleList = new List<ProductLimpet>();
            foreach (var o in DataList)
            {
                var articleData = new ProductLimpet(PortalShop.PortalId, o.ItemID, _langRequired);
                _articleList.Add(articleData);
            }
            return _articleList;
        }
        public void SortOrderMove(int toItemId)
        {
            SortOrderMove(SessionParamData.SortActivate, toItemId);
        }
        public void SortOrderMove(int fromItemId, int toItemId)
        {
            if (fromItemId > 0 && toItemId > 0)
            {
                var moveData = new ProductLimpet(PortalShop.PortalId, fromItemId, _langRequired);
                var toData = new ProductLimpet(PortalShop.PortalId, toItemId, _langRequired);

                var newSortOrder = toData.SortOrder - 1;
                if (moveData.SortOrder < toData.SortOrder) newSortOrder = toData.SortOrder + 1;

                moveData.SortOrder = newSortOrder;
                moveData.Update();
                SessionParamData.CancelItemSort();
            }
        }

        public List<SimplisityInfo> GetAllArticlesForShopPortal()
        {
            return _objCtrl.GetList(PortalShop.PortalId, -1, _entityTypeCode, "", _langRequired, "", 0, 0, 0, 0, _tableName);
        }
        public void ReIndex()
        {
            var list = GetAllArticlesForShopPortal();
            foreach (var pInfo in list)
            {
                _objCtrl.RebuildIndex(PortalShop.PortalId, pInfo.ItemID, _systemKey, _tableName);
            }
        }
        public void Validate()
        {
            var list = GetAllArticlesForShopPortal();
            foreach (var pInfo in list)
            {
                var productData = new ProductLimpet(PortalShop.PortalId, pInfo.ItemID, _langRequired);
                productData.ValidateAndUpdate();
            }
        }

        public string PagingUrl(int page)
        {
            var categoryData = new CategoryLimpet(PortalShop.PortalId, CategoryId, _langRequired);
            var url = SessionParamData.PageUrl.TrimEnd('/') + PortalShop.ProductListPagingUrl;
            url = url.Replace("{page}", page.ToString());
            url = url.Replace("{pagesize}", SessionParamData.PageSize.ToString());
            url = url.Replace("{catid}", categoryData.CategoryId.ToString());
            url = url.Replace("{categoryname}", categoryData.Name);
            url = LocalUtils.TokenReplacementCultureCode(url, _langRequired.ToLower());
            return url;
        }
        public string ListUrl()
        {
            return SessionParamData.PageUrl;
        }
    }

}
