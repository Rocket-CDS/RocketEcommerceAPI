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
    public class OrderLimpetList
    {
        private string _langRequired;
        private List<OrderLimpet> _orderList;
        private const string _tableName = "RocketEcommerceAPI";
        private DNNrocketController _objCtrl;
        private string _searchFilter;
        public OrderLimpetList(SimplisityInfo paramInfo, PortalShopLimpet portalShop, string langRequired, bool populate)
        {
            PortalShop = portalShop;

            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            SessionParamData = new SessionParams(paramInfo);
            if (SessionParamData.PageSize == 0) SessionParamData.PageSize = 32;

            if (populate) Populate();
        }
        public void Populate()
        {
            _searchFilter += PortalShop.GetFilterOrderSQL(SessionParamData.Info);
            SessionParamData.RowCount = _objCtrl.GetListCount(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, _tableName);
            OrderList = _objCtrl.GetList(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, " order by R1.XMLData.value('(genxml/hidden/orderdate)[1]','nvarchar(20)') desc ", 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> OrderList { get; set; }
        public List<OrderLimpet> GetOrderList()
        {
            _orderList = new List<OrderLimpet>();
            foreach (var o in OrderList)
            {
                var orderData = new OrderLimpet(PortalShop.PortalId, o.ItemID, _langRequired);
                _orderList.Add(orderData);
            }
            return _orderList;
        }
        public string ClientImageFolderMapPath { get; set; }
        public string CultureCode { get; private set; }
        public PortalShopLimpet PortalShop { get; private set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string EntityTypeCode { get { return "ORDER"; } }

    }
}
