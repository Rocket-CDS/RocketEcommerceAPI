using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    /// <summary>
    /// The CartLimpet keeps track of Cart data.  Once the cart is finalized it is turned into an order.
    /// The CartLimpet uses the temporary table, to stop unrequired records being created on the RocketEcommerceAPI table.
    /// </summary>
    public class CartLimpet : CartDataClass
    {
        private const string _tableName = "DNNrocketTemp"; 
        private const string _entityTypeCode = "CART";
        private const string _systemkey = "rocketecommerceapi";
        private DNNrocketController _objCtrl;
        private string _browserid;
        public CartLimpet(string browserid, string langaugeRequired)
        {
            CultureCode = langaugeRequired;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();
            _browserid = browserid;

            Record = _objCtrl.GetRecordByGuidKey(PortalUtils.GetCurrentPortalId(), -1, _entityTypeCode, _browserid, "", _tableName);
            populate();
        }
        public CartLimpet(int cartId)
        {
            CultureCode = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();
            if (cartId > 0)
            {
                Record = _objCtrl.GetRecord(cartId, _tableName);
                CultureCode = Record.Lang;
            }
            populate();
        }
        private void populate()
        {
            if (Record == null)
            {
                Record = new SimplisityRecord();
                Record.TypeCode = _entityTypeCode;
                Record.GUIDKey = _browserid;
                Record.PortalId = PortalUtils.GetCurrentPortalId();
                Record.UserId = UserUtils.GetCurrentUserId();
                Record.Lang = CultureCode;
            }
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
            CartItemList = CartUtils.GetCartItemList(Record, CultureCode);
            ItemCount = CartItemList.Count;

            ValidateCart();
        }
        public void Update()
        {
            ValidateCart();
            _objCtrl.Update(Record, _tableName);
        }
        public void AddProduct(SimplisityInfo postInfo)
        {
            var productid = postInfo.GetXmlPropertyInt("genxml/hidden/productid");
            if (productid > 0)
            {
                Record = CartUtils.AddProduct(productid, Record, postInfo);
                Update();
            }
        }
        public void RemoveProduct(int cartItemIndex)
        {
            Record.RemoveRecordListItem("products", cartItemIndex);
            Update();
        }
        public void RemoveProduct(string cartItemKey)
        {
            Record.RemoveRecordListItem("products", "genxml/key", cartItemKey);
            Update();
        }
        public void RemoveAllProducts()
        {
            Record.RemoveRecordList("products");
            Update();
        }
        public void ValidateCart()
        {
            // Use collect for billing address
            if (CollectInStore)
                CopyContactToBill();
            else
                CopyBillToContact();

            // Cart Item Check and Calc
            int total = 0;
            int qty = 0;
            // Get any updated cart items.
            CartItemList = CartUtils.GetCartItemList(Record, CultureCode);
            foreach (var ci in CartItemList)
            {
                total += ci.TotalWithOptionsCents;
                qty += ci.Qty;
            }
            QtyCount = qty;
            SubTotalCents = total;

            // Shipping calc
            ShippingTotalCents = CartUtils.CalculateShippingCost(this, PortalShop);

            //tax calc
            var taxData = CartUtils.CalculateTaxCost(this, PortalShop);
            TaxTotalCents = taxData.TotalCents;

            //tax calc
            DiscountTotalCents = CartUtils.CalculateDiscountCost(this, PortalShop);

            TotalCents = SubTotalCents + DiscountTotalCents + ShippingTotalCents + taxData.TotalCentsAdd;
        }
        public void UpdateProductList(SimplisityInfo postInfo)
        {
            Record = CartUtils.UpdateProductList(Record, postInfo);
            Update();
        }
        public void UpdateContact(SimplisityInfo postInfo)
        {
            Record = CartUtils.UpdateContact(Record, postInfo);
            Update();
        }
        public void UpdateBillAddress(SimplisityInfo postInfo)
        {
            Record = CartUtils.UpdateBillAddress(Record, postInfo);
            if (ShipUseBillAddress) CopyBillToShip();
            Update();
        }
        public void UpdateShipAddress(SimplisityInfo postInfo)
        {
            Record = CartUtils.UpdateShipAddress(Record, postInfo);
            if (ShipUseBillAddress) CopyBillToShip();
            Update();
        }
        public void Delete()
        {
            if (Record.ItemID > 0) _objCtrl.Delete(Record.ItemID, _tableName);
        }
        public OrderLimpet ConvertToOrder()
        {
            ValidateCart();
            var oRecord = (SimplisityRecord)Record.Clone();
            OrderLimpet orderData = null;
            if (OrderId > 0)
            {
                orderData = new OrderLimpet(Record.PortalId, OrderId, CultureCode);                
                if (orderData.Exists)
                {
                    orderData.Record.XMLData = oRecord.XMLData;
                    orderData.OrderDate = DateTime.Now;
                    orderData.ValidateOrder();
                    return orderData;
                }
            }
            oRecord.ItemID = -1;
            oRecord.TypeCode = "ORDER";
            var orderId = _objCtrl.Update(oRecord, "RocketEcommerceAPI");
            orderData = new OrderLimpet(Record.PortalId, orderId, CultureCode);
            orderData.CartId = Record.ItemID;
            orderData.OrderDate = DateTime.Now;

            var shopSettings = new ShopSettingsLimpet(Record.PortalId, CultureCode);
            orderData.EventName = shopSettings.EventName;

            // get product XML , so we have a static copy on order creation + update order
            orderData.CreateStaticProducts();
            orderData.ValidateOrder();
            OrderId = orderData.OrderId;

            Update();
            return orderData;
        }
        private void CopyBillToShip()
        {
            var nodList = Record.XMLDoc.SelectNodes("genxml/billaddress/*");
            foreach (XmlNode x in nodList)
            {
                Record.SetXmlProperty("genxml/shipaddress/" + x.Name.Replace("bill", "ship"), x.InnerText);
            }
        }
        private void CopyContactToBill()
        {
            if (CollectInStore)
            {
                Record.RemoveXmlNode("genxml/billaddress");
                Record.RemoveXmlNode("genxml/shipaddress");
                Record.SetXmlProperty("genxml/billaddress/billfirstname", Record.GetXmlProperty("genxml/textbox/firstname"));
                Record.SetXmlProperty("genxml/billaddress/billlastname", Record.GetXmlProperty("genxml/textbox/lastname"));
                Record.SetXmlProperty("genxml/billaddress/billemail", Record.GetXmlProperty("genxml/textbox/email"));
                Record.SetXmlProperty("genxml/billaddress/billphone", Record.GetXmlProperty("genxml/textbox/phone"));
                Record.SetXmlProperty("genxml/billaddress/billcompany", Record.GetXmlProperty("genxml/textbox/company"));
            }
        }
        private void CopyBillToContact()
        {
            if (!CollectInStore)
            {
                if (Record.GetXmlProperty("genxml/textbox/firstname") == "") Record.SetXmlProperty("genxml/textbox/firstname", Record.GetXmlProperty("genxml/billaddress/billfirstname"));
                if (Record.GetXmlProperty("genxml/textbox/lastname") == "") Record.SetXmlProperty("genxml/textbox/lastname", Record.GetXmlProperty("genxml/billaddress/billlastname"));
                if (Record.GetXmlProperty("genxml/textbox/email") == "") Record.SetXmlProperty("genxml/textbox/email", Record.GetXmlProperty("genxml/billaddress/billemail"));
                if (Record.GetXmlProperty("genxml/textbox/phone") == "") Record.SetXmlProperty("genxml/textbox/phone", Record.GetXmlProperty("genxml/billaddress/billphone"));
                if (Record.GetXmlProperty("genxml/textbox/company") == "") Record.SetXmlProperty("genxml/textbox/company", Record.GetXmlProperty("genxml/billaddress/billcompany"));
            }
        }
        public int OrderId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public PortalShopLimpet PortalShop { get; private set; }
        public int ItemCount { get; private set; }
        public int QtyCount { get; private set; }
        public List<CartItemLimpet> CartItemList { get; private set; }


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
        public decimal DiscountTotal
        {
            get { return (PortalShop.CurrencyCentsToDollars(DiscountTotalCents)); }
        }
        public string DiscountTotalDisplay
        {
            get { return DiscountTotal.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }

        #endregion

    }
}
