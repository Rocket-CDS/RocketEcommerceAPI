using DNNrocketAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
/// <summary>
/// Some function are used in both Cart and Order.  Because we use a different table for each, we can't combined the class.
/// These functions are used in both the CartLimpet and OrderLimpet
/// </summary>
    public static class CartUtils
    {
        public static List<CartItemLimpet> GetCartItemList(SimplisityRecord record, string cultureCode)
        {
            var rtn = new List<CartItemLimpet>();
            var l = GetItemList(record);
            foreach (var aInfo in l)
            {
                var p = new CartItemLimpet(record.PortalId, aInfo, cultureCode);
                if (p.Exists) rtn.Add(p);
            }
            return rtn;
        }
        public static List<SimplisityRecord> GetItemList(SimplisityRecord record)
        {
            return record.GetRecordList("products");
        }

        public static string GetCartItemKey(int productId, SimplisityInfo postInfo)
        {
            if (productId > 0)
            {
                var productData = new ProductLimpet(PortalUtils.GetPortalId(), productId);
                if (productData.Exists)
                {
                    var qty = postInfo.GetXmlPropertyDouble("genxml/textbox/qty");
                    var modelkey = postInfo.GetXmlProperty("genxml/hidden/modelid" + productData.ProductId);

                    var optionpostArray = new Dictionary<string, string>();
                    var optionpostkey = "";
                    foreach (var o in productData.GetOptions())
                    {
                        var optionkeyNode = postInfo.XMLDoc.SelectSingleNode("genxml/*[text()=\"" + o.OptionKey + "\"]");
                        if (optionkeyNode != null)
                        {
                            var postlp = optionkeyNode.Name.Replace("optionkey", "");
                            var postoptionvalue = GeneralUtils.EnCode(postInfo.GetXmlProperty("genxml/optionid" + postlp));
                            optionpostkey += postoptionvalue + "*";
                            optionpostArray.Add(o.OptionKey, postoptionvalue);
                        }
                    }
                    optionpostkey = optionpostkey.TrimEnd('*');

                    return productData.ProductId + "*" + modelkey + "*" + optionpostkey;
                }
            }
            return "";
        }

        public static SimplisityRecord AddProduct(int productId, SimplisityRecord record, SimplisityInfo postInfo)
        {
            if (productId > 0)
            {
                var productData = new ProductLimpet(PortalUtils.GetPortalId(), productId);
                if (productData.Exists)
                {
                    var qty = postInfo.GetXmlPropertyDouble("genxml/textbox/qty");
                    if (qty == 0) qty = 1;
                    var modelkey = postInfo.GetXmlProperty("genxml/hidden/modelid" + productId);
                    if (modelkey == "" && productData.GetModelList().Count > 0) // take first model
                    {
                        modelkey = productData.GetModel(0).ModelKey;
                    }

                    var optionpostArray = new Dictionary<string, string>();
                    var optionpostkey = "";
                    foreach (var o in productData.GetOptions())
                    {
                        var optionkeyNode = postInfo.XMLDoc.SelectSingleNode("genxml/*[text()=\"" + o.OptionKey + "\"]");
                        if (optionkeyNode != null)
                        {
                            var postlp = optionkeyNode.Name.Replace("optionkey", "");
                            var postoptionvalue = GeneralUtils.EnCode(postInfo.GetXmlProperty("genxml/optionid" + postlp));
                            optionpostkey += postoptionvalue + "*";
                            optionpostArray.Add(o.OptionKey, postoptionvalue);
                        }
                    }
                    optionpostkey = optionpostkey.TrimEnd('*');

                    var key = productId + "*" + modelkey + "*" + optionpostkey;

                    var listItemExists = true;
                    var pRec = record.GetRecordListItem("products", "genxml/key", key);
                    if (pRec == null)
                    {
                        listItemExists = false;
                        pRec = new SimplisityRecord();
                    }
                    pRec.SetXmlProperty("genxml/key", key);
                    pRec.SetXmlProperty("genxml/qty", (pRec.GetXmlPropertyDouble("genxml/qty") + qty).ToString(), TypeCode.Double);
                    pRec.SetXmlProperty("genxml/productid", productId.ToString());
                    pRec.SetXmlProperty("genxml/modelid", modelkey);
                    foreach (var o in optionpostArray)
                    {
                        pRec.SetXmlProperty("genxml/options/" + o.Key, o.Value);
                    }
                    if (listItemExists) record.RemoveRecordListItem("products", "genxml/key", key);
                    record.AddRecordListItem("products", pRec);
                }
            }
            return record;
        }

        public static SimplisityRecord UpdateProductList(SimplisityRecord record, SimplisityInfo postInfo)
        {
            var productListPost = postInfo.GetList("products");
            foreach (var p in productListPost)
            {
                XmlNodeList cartOptionsNodes = postInfo.XMLDoc.SelectNodes("genxml/products/genxml[key=\"" + p.GetXmlProperty("genxml/key") + "\"]/cartoptions/*");
                if (cartOptionsNodes != null)
                {
                    record.RemoveXmlNode("genxml/products/genxml[key=\"" + p.GetXmlProperty("genxml/key") + "\"]/cartoptions");
                    foreach (XmlNode cartNod in cartOptionsNodes)
                    {
                        if (cartNod != null) record.SetXmlProperty("genxml/products/genxml[key=\"" + p.GetXmlProperty("genxml/key") + "\"]/cartoptions/" + cartNod.Name, cartNod.InnerText);
                    }
                }
                var nod = record.XMLDoc.SelectSingleNode("genxml/products/genxml[key=\"" + p.GetXmlProperty("genxml/key") + "\"]/qty");
                if (nod != null) record.SetXmlPropertyDouble("genxml/products/genxml[key=\"" + p.GetXmlProperty("genxml/key") + "\"]/qty", p.GetXmlPropertyDouble("genxml/qty"));
            }
            return record;
        }

        public static SimplisityRecord UpdateContact(SimplisityRecord record, SimplisityInfo postInfo)
        {
            var nodList = postInfo.XMLDoc.SelectNodes("genxml/contact/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/contact/" + x.Name, x.InnerText);
            }
            nodList = postInfo.XMLDoc.SelectNodes("genxml/textbox/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/textbox/" + x.Name, x.InnerText);
            }
            nodList = postInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/checkbox/" + x.Name, x.InnerText);
            }
            return record;
        }
        public static SimplisityRecord UpdateBillAddress(SimplisityRecord record, SimplisityInfo postInfo)
        {
            // clear nodes
            var nodList = record.XMLDoc.SelectNodes("genxml/billaddress/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/billaddress/" + x.Name, "");
            }
            // add new nodes
            nodList = postInfo.XMLDoc.SelectNodes("genxml/billaddress/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/billaddress/" + x.Name, x.InnerText);
            }
            return record;
        }
        public static SimplisityRecord UpdateShipAddress(SimplisityRecord record, SimplisityInfo postInfo)
        {
            // clear nodes
            var nodList = record.XMLDoc.SelectNodes("genxml/shipaddress/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/shipaddress/" + x.Name, "");
            }
            // add new nodes
            nodList = postInfo.XMLDoc.SelectNodes("genxml/shipaddress/*");
            foreach (XmlNode x in nodList)
            {
                record.SetXmlProperty("genxml/shipaddress/" + x.Name, x.InnerText);
            }
            return record;
        }

        public static int CalculateShippingCost(int orderId, string cultureCode, PortalShopLimpet portalShop)
        {
            var orderData = new OrderLimpet(portalShop.PortalId, orderId, cultureCode);
            return CalculateShippingCost(orderData, portalShop);
        }
        public static int CalculateShippingCost(OrderLimpet orderData, PortalShopLimpet portalShop)
        {
            if (orderData.CollectInStore) return 0;
            int shiptotal = 0;
            var shipMethods = portalShop.GetActiveShippingProviders();
            if (shipMethods == null) return 0;
            foreach (var shipprov in shipMethods)
            {
                var calc = shipprov.CalculateShippingCost(orderData);
                if (calc < shiptotal || shiptotal == 0) shiptotal = calc;
            }
            return shiptotal;
        }
        public static int CalculateShippingCost(CartLimpet cartData, PortalShopLimpet portalShop)
        {
            if (cartData.CollectInStore) return 0;
            int shiptotal = 0;
            var shipMethods = portalShop.GetActiveShippingProviders();
            if (shipMethods == null) return 0;
            foreach (var shipprov in shipMethods)
            {
                var calc = shipprov.CalculateShippingCost(cartData);
                if (calc < shiptotal || shiptotal == 0) shiptotal = calc;
            }
            return shiptotal;
        }
        public static int CalculateShippingCost(int cartId, PortalShopLimpet portalShop)
        {
            var cartData = new CartLimpet(cartId);
            return CalculateShippingCost(cartData, portalShop);
        }

        public static PaymentLimpet CartBankReturn(SimplisityInfo paramInfo)
        {
            var paymentData = new PaymentLimpet(PortalUtils.GetPortalId(), -1); // create new payment (not on DB until it is updated)
            try
            {
                var systemData = new SystemLimpet("rocketecommerceapi");
                var guidkey = paramInfo.GetXmlProperty("genxml/remote/urlparams/key"); // Use remote key if passed from module.
                if (guidkey == "") guidkey = paramInfo.GetXmlProperty("genxml/urlparams/key");
                if (guidkey == "") guidkey = paramInfo.GetXmlProperty("genxml/hidden/key");
                if (guidkey != "") paymentData = new PaymentLimpet(PortalUtils.GetPortalId(), guidkey);  // if we have a "key" param, we should have a payment record.
                if (paymentData.BankAction == PaymentAction.BankPost)
                {
                    var rocketInterface = systemData.GetProvider(paymentData.PaymentProvider);
                    if (rocketInterface != null)
                    {
                        if (rocketInterface.Assembly != "")
                        {
                            var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                            bankprov.ReturnEvent(paramInfo);
                            paymentData.Reload(); // reload so we have the changed, that need to be past to the display.
                        }
                    }
                    paymentData.BankAction = PaymentAction.BankReturn;
                    paymentData.Update();
                }
                if (paymentData.Exists)
                {
                    paymentData.AmountPayCents = paymentData.AmountDueCents;
                    paymentData.Update();
                }
                else
                    LogUtils.LogTracking("ERROR: NO PAYMENT FOUND - key: " + guidkey, systemData.SystemKey);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }
            return paymentData;
        }


    }
}
