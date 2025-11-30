using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketEcommerceAPI.Components
{
    public class CartLimpetList
    {
        private string _langRequired;
        private List<CartLimpet> _cartList;
        private const string _tableName = "DNNrocketTemp";
        private DNNrocketController _objCtrl;
        private string _searchFilter;
        public CartLimpetList(SimplisityInfo paramInfo, PortalShopLimpet portalShop, string langRequired, bool populate)
        {
            PortalShop = portalShop;

            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            SessionParamData = new SessionParams(paramInfo);
            if (SessionParamData.PageSize == 0) SessionParamData.PageSize = 32;

            _cartList = new List<CartLimpet>();

            if (populate) Populate();
        }
        public void Populate(string searchFilter = "")
        {
            //_searchFilter += PortalShop.GetFilterOrderSQL(SessionParamData.Info);
            _searchFilter = searchFilter;
            SessionParamData.RowCount = _objCtrl.GetListCount(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, _tableName);
            CartList = _objCtrl.GetList(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, " order by R1.ItemID desc", 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> CartList { get; set; }
        public List<CartLimpet> GetCartList()
        {
            _cartList = new List<CartLimpet>();
            foreach (var o in CartList)
            {
                _cartList.Add(new CartLimpet(o.GUIDKey, o.Lang));
            }
            return _cartList;
        }
        public string ClientImageFolderMapPath { get; set; }
        public PortalShopLimpet PortalShop { get; private set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string EntityTypeCode { get { return "CART"; } }

    }
}
