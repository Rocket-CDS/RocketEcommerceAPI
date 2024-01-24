using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class PortalShopLimpetStats
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "PortalShopSTATS";
        private DNNrocketController _objCtrl;
        private string _guidKey;
        private int _portalId;
        private string _currencyCultureCode;
        public PortalShopLimpetStats(PortalShopLimpet portalShop)
        {
            Record = new SimplisityInfo();
            _objCtrl = new DNNrocketController();
            _portalId = portalShop.PortalId;
            _guidKey = _entityTypeCode + "*" + _portalId;
            _currencyCultureCode = portalShop.CurrencyCultureCode;

            if (!portalShop.Exists)
            {
                var u = _objCtrl.GetByGuidKey(_portalId, -1, _entityTypeCode, _guidKey, "", _tableName);
                if (u != null)
                {
                    LogUtils.LogSystem("ERROR: Invalid PortalShop. PortalShopStats.cs  (rocketecommerceapi)");
                }
            }
            else
            {
                Record = _objCtrl.GetByGuidKey(_portalId, -1, _entityTypeCode, _guidKey, "", _tableName);
                if (Record == null)
                {
                    Record = new SimplisityRecord();
                    Record.PortalId = _portalId;
                    Record.ModuleId = -1;
                    Record.TypeCode = _entityTypeCode;
                    Record.GUIDKey = _guidKey;
                }
            }
        }
        public void Update()
        {
            _objCtrl.Update(Record, _tableName);
        }
        public void Delete()
        {
            _objCtrl.Delete(Record.ItemID, _tableName);
        }

        public void RunCalculation(bool fullStats = false)
        {
            ProductCount = _objCtrl.GetListCount(_portalId,-1,"PRD","","","RocketEcommerceAPI");
            OrderCount = _objCtrl.GetListCount(_portalId, -1, "ORDER", "", "", "RocketEcommerceAPI");
            ClientCount = _objCtrl.GetListCount(_portalId, -1, "CLIENT", "", "", "RocketEcommerceAPI");

            var sqlOrderSelect = "and MONTH(ModifiedDate) = " + DateTime.Now.Month + " AND YEAR(ModifiedDate) = " + DateTime.Now.Year + " ";
            var orderList = _objCtrl.GetList(_portalId, -1, "ORDER", sqlOrderSelect, "", "", 0, 0, 0, 0, "RocketEcommerceAPI");
            var processList = new List<string>();
            foreach (var orderInfo in orderList)
            {
                var odate = orderInfo.GetXmlPropertyDate("genxml/hidden/orderdate");
                var k = odate.Year + "-" + odate.Month;
                if (!processList.Contains(k)) processList.Add(k);
            }
            foreach (var dateKey in processList)
            {
                UpdateOrderStats(dateKey);
            }
            Update();
        }
        private void UpdateOrderStats(string datekey)
        {
            var s2 = datekey.Split('-');
            string month = s2[1];
            string year = s2[0];
            var sqlOrderSelect = "and MONTH([XMLData].value('(genxml/hidden/orderdate)[1]','datetime')) = " + month + " AND YEAR([XMLData].value('(genxml/hidden/orderdate)[1]','datetime')) = " + year + " ";
            var orderList = _objCtrl.GetList(_portalId, -1, "ORDER", sqlOrderSelect, "", "", 0, 0, 0, 0, "RocketEcommerceAPI");
            double total = 0;
            double taxtotal = 0;
            double discounttotal = 0;
            double shiptotal = 0;
            double subtotal = 0;
            int cartitemcount = 0;
            var statusDict = new Dictionary<int, int>();
            foreach (var orderInfo in orderList)
            {
                var orderData = new OrderLimpet(_portalId, orderInfo.ItemID, _currencyCultureCode);
                subtotal += orderData.SubTotalCents;
                total += orderData.TotalCents;
                taxtotal += orderData.TaxTotalCents;
                discounttotal += orderData.DiscountTotalCents;
                shiptotal += orderData.ShippingTotalCents;
                cartitemcount += orderData.GetCartItemList().Count;

                if (statusDict.ContainsKey(orderData.StatusCode))
                    statusDict[orderData.StatusCode] += 1;
                else
                    statusDict.Add(orderData.StatusCode, 1);
            }
            var record = new SimplisityRecord();
            record.SetXmlProperty("genxml/datekey", datekey);
            record.SetXmlProperty("genxml/subtotal", subtotal.ToString());
            record.SetXmlProperty("genxml/total", total.ToString());
            record.SetXmlProperty("genxml/taxtotal", taxtotal.ToString());
            record.SetXmlProperty("genxml/discounttotal", discounttotal.ToString());
            record.SetXmlProperty("genxml/shiptotal", shiptotal.ToString());
            record.SetXmlProperty("genxml/cartitemcount", cartitemcount.ToString());

            foreach (int s in Enum.GetValues(typeof(OrderStatus)))
            {
                if (statusDict.ContainsKey(s))
                    record.SetXmlProperty("genxml/status" + s.ToString(), statusDict[s].ToString());
                else
                    record.SetXmlProperty("genxml/status" + s.ToString(), "0");
            }
            Record.RemoveRecordListItem("orders", "genxml/datekey", datekey);
            Record.AddRecordListItem("orders", record);

            // Total Order Status
            statusDict = new Dictionary<int, int>();
            var orderStatList = Record.GetRecordList("orders");
            foreach (var o in orderStatList)
            {
                var nodList = o.XMLDoc.SelectNodes("genxml/*[starts-with(name(), 'status')]");
                foreach (XmlNode os in nodList)
                {
                    var statuscode = os.Name.Replace("status", "");
                    if (GeneralUtils.IsNumeric(statuscode) && GeneralUtils.IsNumeric(os.InnerText))
                    {
                        if (statusDict.ContainsKey(Convert.ToInt32(statuscode)))
                            statusDict[Convert.ToInt32(statuscode)] += Convert.ToInt32(os.InnerText);
                        else
                            statusDict.Add(Convert.ToInt32(statuscode), Convert.ToInt32(os.InnerText));
                    }
                }
            }
            foreach (var s in statusDict)
            {
                Record.SetXmlProperty("genxml/status" + s.Key.ToString(), statusDict[s.Key].ToString());
            }
        }
        public Dictionary<string, string> StatusTotals()
        {
            var rtn = new Dictionary<string, string>();
            var nodList = Record.XMLDoc.SelectNodes("genxml/*[starts-with(name(), 'status')]");
            foreach (XmlNode os in nodList)
            {
                rtn.Add(os.Name.Replace("status", ""), os.InnerText);
            }
            return rtn;
        }
        public SimplisityRecord GetOrderTotalsByDate(int year, int month)
        {
            return Record.GetRecordListItem("orders", "genxml/datekey", year + "-" + month);
        }
        public List<SimplisityRecord> OrderTotals()
        {
            return Record.GetRecordList("orders");
        }

        #region "Stats Data"
        public SimplisityRecord Record { get; set; }
        public int ProductCount { get { return Record.GetXmlPropertyInt("genxml/productcount"); } set { Record.SetXmlProperty("genxml/productcount", value.ToString()); } }
        public int OrderCount { get { return Record.GetXmlPropertyInt("genxml/ordercount"); } set { Record.SetXmlProperty("genxml/ordercount", value.ToString()); } }
        public int ClientCount { get { return Record.GetXmlPropertyInt("genxml/clientcount"); } set { Record.SetXmlProperty("genxml/clientcount", value.ToString()); } }
        public double TotalSoldMoney { get { return Record.GetXmlPropertyDouble("genxml/totalsoldmoney"); } set { Record.SetXmlProperty("genxml/totalsoldmoney", value.ToString()); } }
        public double TotalSoldCount { get { return Record.GetXmlPropertyDouble("genxml/totalsoldcount"); } set { Record.SetXmlProperty("genxml/totalsoldcount", value.ToString()); } }
        public double MonthSoldMoney { get { return Record.GetXmlPropertyDouble("genxml/monthsoldmoney"); } set { Record.SetXmlProperty("genxml/monthsoldmoney", value.ToString()); } }
        public double MonthSoldCount { get { return Record.GetXmlPropertyDouble("genxml/monthsoldcount"); } set { Record.SetXmlProperty("genxml/monthsoldcount", value.ToString()); } }

        #endregion

    }
}
