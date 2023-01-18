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
    public class PaymentLimpetList
    {
        private string _langRequired;
        private List<PaymentLimpet> _orderList;
        private const string _tableName = "RocketEcommerceAPI";
        private DNNrocketController _objCtrl;
        private string _searchFilter;
        public PaymentLimpetList(SimplisityInfo paramInfo, PortalShopLimpet portalShop, string langRequired, bool populate)
        {
            PortalShop= portalShop;
            PortalId = PortalShop.PortalId;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();

            SessionParamData = new SessionParams(paramInfo);
            if (SessionParamData.PageSize == 0) SessionParamData.PageSize = 32;

            if (populate) Populate();
        }
        public void Populate()
        {
            _searchFilter += PortalShop.GetFilterPaymentSQL(SessionParamData.Info);
            SessionParamData.RowCount = _objCtrl.GetListCount(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, _tableName);
            PaymentList = _objCtrl.GetList(PortalShop.PortalId, -1, EntityTypeCode, _searchFilter, _langRequired, " order by R1.ItemID desc", 0, SessionParamData.Page, SessionParamData.PageSize, SessionParamData.RowCount, _tableName);
        }
        public SessionParams SessionParamData { get; set; }
        public List<SimplisityInfo> PaymentList { get; set; }
        public List<PaymentLimpet> GetPaymentList()
        {
            _orderList = new List<PaymentLimpet>();
            foreach (var o in PaymentList)
            {
                var orderData = new PaymentLimpet(PortalShop.PortalId, o.ItemID, _langRequired);
                _orderList.Add(orderData);
            }
            return _orderList;
        }
        public string ClientImageFolderMapPath { get; set; }
        public string CultureCode { get; private set; }
        public PortalShopLimpet PortalShop { get; private set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string EntityTypeCode { get { return "PAYMENT"; } }
        public int PortalId { get; set; }

    }
}
