using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketEcommerceAPI.Components
{
    public class OrderLimpet : CartDataClass 
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "ORDER";
        private const string _systemkey = "rocketecommerceapi";
        private SimplisityRecord _oldRecord;

        private DNNrocketController _objCtrl;
        public OrderLimpet(int portalId, int orderId, string langaugeRequired)
        {
            Record = new SimplisityInfo();
            PortalId = portalId;
            CultureCode = langaugeRequired;
            _objCtrl = new DNNrocketController();
            Populate(orderId);
        }

        private void Populate(int orderId)
        {
            if (orderId > 0)
            {
                var record = _objCtrl.GetRecord(PortalId, EntityTypeCode, orderId, -1, true, _tableName); // get existing record.
                if (record != null && record.ItemID > 0) Record = record; // check if we have a real record, or a dummy being created and not saved yet.
            }
            if (orderId <= 0)
            {
                Record.TypeCode = EntityTypeCode;
                Record.PortalId = PortalId;
                Record.ItemID = -1;
                Record.ModuleId = -1;
                Record.UserId = -1;
                OrderDate = DateTime.Now;
            }

            _oldRecord = (SimplisityRecord)Record.Clone();
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
        }
        public void Delete()
        {
            _objCtrl.Delete(Record.ItemID, _tableName);
        }

        public void Save(SimplisityInfo postInfo)
        {

            if (postInfo.XMLDoc.SelectSingleNode("genxml/textbox") != null)
            {
                Record.RemoveXmlNode("genxml/textbox");
                Record.AddXmlNode(postInfo.XMLDoc.SelectSingleNode("genxml/textbox").OuterXml, "textbox", "genxml");
            }
            if (postInfo.XMLDoc.SelectSingleNode("genxml/checkbox") != null)
            {
                Record.RemoveXmlNode("genxml/checkbox");
                Record.AddXmlNode(postInfo.XMLDoc.SelectSingleNode("genxml/checkbox").OuterXml, "checkbox", "genxml");
            }
            if (postInfo.XMLDoc.SelectSingleNode("genxml/shipaddress") != null)
            {
                Record.RemoveXmlNode("genxml/shipaddress");
                Record.AddXmlNode(postInfo.XMLDoc.SelectSingleNode("genxml/shipaddress").OuterXml, "shipaddress", "genxml");
            }
            if (postInfo.XMLDoc.SelectSingleNode("genxml/billaddress") != null)
            {
                Record.RemoveXmlNode("genxml/billaddress");
                Record.AddXmlNode(postInfo.XMLDoc.SelectSingleNode("genxml/billaddress").OuterXml, "billaddress", "genxml");
            }

            ValidateOrder();

            Update();
        }
        public void Update(string comment = "")
        {
            AddHistory(comment);

            if (OrderGuid == "") OrderGuid = GeneralUtils.GetGuidKey();
            var orderid = _objCtrl.Update(Record, _tableName);
            Populate(orderid);
            if (OrderNumber == "")
            {
                OrderNumber = OrderDate.ToString("yy") + OrderDate.ToString("MM") + OrderDate.ToString("dd") + "-" + orderid.ToString();
                _objCtrl.Update(Record, _tableName);
                Populate(orderid);
            }
        }
        public void AddListItem(string listname)
        {
            if (Record.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Record.AddListItem(listname);
            Update();
        }
        public void ChangeStaus(int orderStatusInt)
        {
            ChangeStaus((OrderStatus)Enum.ToObject(typeof(OrderStatus), (int)orderStatusInt));
        }
        public void ChangeStaus(OrderStatus orderStatus)
        {
            if (StatusCode != (int)orderStatus)
            {
                AddHistory("Status");
                StatusCode = (int)orderStatus;
            }
            Update();
        }
        public void AddHistory(string comment = "")
        {
            if (_oldRecord.GetXmlPropertyInt("genxml/hidden/statuscode") > 0 || (comment != ""))  // we do not need a history record for 1st save.
            {
                if (
                (StatusCode != _oldRecord.GetXmlPropertyInt("genxml/hidden/statuscode")) ||
                (TotalCents != _oldRecord.GetXmlPropertyInt("genxml/hidden/total")) ||
                (comment != "")
                )
                {
                    var sRec = new SimplisityInfo();
                    sRec.SetXmlProperty("genxml/statuscode", StatusCode.ToString());
                    sRec.SetXmlProperty("genxml/userid", UserUtils.GetCurrentUserId().ToString());
                    sRec.SetXmlProperty("genxml/username", UserUtils.GetCurrentUserName());
                    sRec.SetXmlProperty("genxml/datetime", DateTime.Now.ToString("O"), TypeCode.DateTime);
                    sRec.SetXmlProperty("genxml/comment", comment);
                    sRec.SetXmlProperty("genxml/total", TotalDisplay);
                    Record.AddListItem("history", sRec.XMLData);
                }
            }

        }
        public List<SimplisityRecord> GetHistory()
        {
            return Record.GetRecordList("history");
        }

        #region "Email"

        public bool SendEmail()
        {
            return EmailSend(Email, "OrderEmail.cshtml");
        }
        public bool SendManagerEmail()
        {
            var companyLimpet = new CompanyLimpet(PortalId, CultureCode);
            return EmailSend(companyLimpet.OrderEmail, "OrderEmail.cshtml");
        }
        private bool EmailSend(string email, string templateName)
        {
            var systemData = new SystemLimpet(_systemkey);
            if (GeneralUtils.IsEmail(email))
            {
                var notificationData = new NotificationLimpet(PortalId, PreferedCultureCode);
                var emailLimpet = new EmailLimpet(PortalId, this, PreferedCultureCode);
                if (emailLimpet.SendEmail(email, templateName, notificationData.Message_EmailOrderSubject))
                {
                    Update("SENT: " + templateName + " > " + email);
                    return true;
                }
                else
                {
                    Update("SENT FAILED: " + templateName + " > " + email);
                    Update("ERROR " + emailLimpet.ErrorMessage);
                    return false;
                }
            }
            else
            {
                Update("Invalid Email: " + email);
                return false;
            }
        }

        #endregion

        #region "Order Payments"
        public void AddPayment(int paymentId)
        {
            Record.RemoveRecordListItem("payments", "genxml/paymentid", paymentId.ToString());
            var rec = new SimplisityRecord();
            rec.SetXmlPropertyInt("genxml/paymentid", paymentId.ToString());
            Record.AddRecordListItem("payments", rec);
        }
        public List<PaymentLimpet> GetPaymentList()
        {
            var rtn = new List<PaymentLimpet>();
            var l = Record.GetRecordList("payments");
            foreach (var p in l)
            {
                var paymentId = p.GetXmlPropertyInt("genxml/paymentid");
                if (paymentId > 0)
                {
                    var paymentData = new PaymentLimpet(PortalId, paymentId, CultureCode);
                    if (paymentData.Exists) rtn.Add(paymentData);
                }
            }
            return rtn;
        }

        #endregion


        #region "Cart Products"
        public List<CartItemLimpet> GetCartItemList()
        {
            return CartUtils.GetCartItemList(Record, CultureCode);
        }

        public void AddProduct(SimplisityInfo postInfo, int productId)
        {
            Record = CartUtils.AddProduct(productId, Record, postInfo);
            ValidateOrder();
            AddHistory("+");
        }
        public void RemoveProduct(int cartItemIndex)
        {
            Record.RemoveRecordListItem("products", cartItemIndex);
            ValidateOrder();
            AddHistory("-");
        }
        public void RemoveProduct(string cartItemKey)
        {
            Record.RemoveRecordListItem("products", "genxml/key", cartItemKey);
            ValidateOrder();
            AddHistory("-");
        }
        public void RemoveAllProducts()
        {
            Record.RemoveRecordList("products");
            ValidateOrder();
            AddHistory("-");
        }

        #endregion

        public void ValidateOrder()
        {
            int total = 0;
            var cartItems = GetCartItemList();
            foreach (var ci in cartItems)
            {
                total += Convert.ToInt32(ci.TotalWithOptionsCents);
            }
            SubTotalCents = total;

            // Shipping calc
            ShippingTotalCents = CartUtils.CalculateShippingCost(OrderId, CultureCode, PortalShop);

            TotalCents = SubTotalCents + DiscountTotalCents + ShippingTotalCents + TaxTotalCents;

            Update();
        }

        public void CreateStaticProducts(string cartitemKey = "")
        {
            // get product XML , so we have a static copy on order creation
            foreach (var ci in GetCartItemList())
            {
                if (cartitemKey == ci.Key || cartitemKey == "")
                {
                    /// <summary>
                    /// Static Products are XML data node attached to the cart, when an order is placed.
                    /// This gives us the data at the exact time the client made the order.
                    /// </summary>
                    var productData = new ProductLimpet(PortalId, ci.ProductId, CultureCode); // get new instance, incase it's changed.
                    if (productData.Exists)
                    {
                        Record.RemoveXmlNode("genxml/products/genxml[key='" + ci.Key + "']/productdata");
                        Record.AddXmlNode("<productdata>" + productData.Info.ToXmlItem() + "</productdata>", "productdata", "genxml/products/genxml[key='" + ci.Key + "']");
                    }
                    Update();
                }
            }
        }



        #region "Properties"

        public string SystemKey { get { return _systemkey; } }
        public string EntityTypeCode { get { return _entityTypeCode; } }
        public bool Exists { get { if (Record.ItemID <= 0) { return false; } else { return true; }; } }
        public int ModuleId { get { return Record.ModuleId; } set { Record.ModuleId = value; } }
        public int XrefItemId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public int ParentItemId { get { return Record.ParentItemId; } set { Record.ParentItemId = value; } }
        public string GUIDKey { get { return Record.GUIDKey; } set { Record.GUIDKey = value; } }
        public int SortOrder { get { return Record.SortOrder; } set { Record.SortOrder = value; } }
        public int OrderId { get { return Record.ItemID; } }
        public string InvoiceRef { get { return Record.GetXmlProperty("genxml/textbox/invoiceref"); } }
        public DateTime OrderDate { get { return Record.GetXmlPropertyDate("genxml/hidden/orderdate"); } set { Record.SetXmlProperty("genxml/hidden/orderdate", value.ToString("O"), TypeCode.DateTime); } }
        public string OrderGuid { get { return Record.GetXmlProperty("genxml/hidden/orderguid"); } set { Record.SetXmlProperty("genxml/hidden/orderguid", value); } }
        public string OrderNumber { get { return Record.GetXmlProperty("genxml/hidden/ordernumber"); } set { Record.SetXmlProperty("genxml/hidden/ordernumber", value); } }
        public string TaxNumber { get { return Record.GetXmlProperty("genxml/hidden/taxnumber"); } set { Record.SetXmlProperty("genxml/hidden/taxnumber", value); } }
        public string DiscountCode { get { return Record.GetXmlProperty("genxml/hidden/discountcode"); } set { Record.SetXmlProperty("genxml/hidden/discountcode", value); } }
        public int CartId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public int UserId { get { return Record.UserId; } set { Record.UserId = value; } }
        public int StatusCode { get { return Record.SortOrder; } set {
                Record.SortOrder = value; // use the sortorder to help speed selection in SQL
            } 
        }
        public OrderStatus Status
        {
            get
            {
                if (Enum.IsDefined(typeof(OrderStatus), StatusCode))
                {
                    return (OrderStatus)StatusCode;
                }
                else
                {
                    return OrderStatus.Incomplete;
                }
            }
            set
            {
                Record.SortOrder = Convert.ToInt32(value);  // use the sortorder to help speed selection in SQL
            }
        }
        public string Notes { get { return Record.GetXmlProperty("genxml/textbox/notes"); } }
        public PortalShopLimpet PortalShop { get; private set; }
        public string EventName { get { return Record.GetXmlProperty("genxml/textbox/eventname"); } set { Record.SetXmlProperty("genxml/textbox/eventname", value.ToString()); } }

        #endregion

        #region "Totals"

        public int TotalCents { get { return Record.GetXmlPropertyInt("genxml/hidden/total"); } set { Record.SetXmlProperty("genxml/hidden/total", value.ToString()); } }
        public decimal Total
        {
            get { return (PortalShop.CurrencyCentsToDollars(TotalCents)); }
        }
        public string TotalDisplay
        {
            get { return Total.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }

        public int SubTotalCents { get { return Record.GetXmlPropertyInt("genxml/hidden/subtotal"); } set { Record.SetXmlProperty("genxml/hidden/subtotal", value.ToString()); } }
        public decimal SubTotal
        {
            get { return (PortalShop.CurrencyCentsToDollars(SubTotalCents)); }
        }
        public string SubTotalDisplay
        {
            get { return SubTotal.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }

        public int ShippingTotalCents { get { return Record.GetXmlPropertyInt("genxml/hidden/shippingtotal"); } set { Record.SetXmlProperty("genxml/hidden/shippingtotal", value.ToString()); } }
        public decimal ShippingTotal
        {
            get { return (PortalShop.CurrencyCentsToDollars(ShippingTotalCents)); }
        }
        public string ShippingTotalDisplay
        {
            get { return ShippingTotal.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        public int TaxTotalCents { get { return Record.GetXmlPropertyInt("genxml/hidden/taxtotal"); } set { Record.SetXmlProperty("genxml/hidden/taxtotal", value.ToString()); } }
        public decimal TaxTotal
        {
            get { return (PortalShop.CurrencyCentsToDollars(TaxTotalCents)); }
        }
        public string TaxTotalDisplay
        {
            get { return TaxTotal.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        public int DiscountTotalCents { get { return Record.GetXmlPropertyInt("genxml/hidden/discounttotal"); } set { Record.SetXmlProperty("genxml/hidden/discounttotal", value.ToString()); } }
        public decimal DiscountTotalTotal
        {
            get { return (PortalShop.CurrencyCentsToDollars(DiscountTotalCents)); }
        }
        public string DiscountTotalDisplay
        {
            get { return DiscountTotalTotal.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        public string PreferedCultureCode { get { return Record.GetXmlProperty("genxml/textbox/preferedculturecode"); } set { Record.SetXmlProperty("genxml/textbox/preferedculturecode", value); } }

        #endregion
    }
}
