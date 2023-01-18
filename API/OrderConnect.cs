using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private OrderLimpet GetOrder(int orderid)
        {
            return new OrderLimpet(_portalShop.PortalId, orderid, _sessionParams.CultureCodeEdit);
        }
        public string SendOrderEmail()
        {
            try
            {
                var orderid = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                var orderData = new OrderLimpet(_portalShop.PortalId, orderid, _sessionParams.CultureCodeEdit);
                if (orderData.Exists)
                {
                    if (orderData.SendEmail())
                    {
                        var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailsent");
                        var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailsent", "Msg");
                        messageText = messageText.Replace("{email}", orderData.Email);
                        var messageData = new MessageLimpet(_portalShop, messageTitle, messageText);
                        messageData.FadeModel = true;
                        return messageData.GetDisplayHelp();
                    }
                    else
                    {
                        var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailfail");
                        var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailfail", "Msg");
                        messageText = messageText.Replace("{email}", orderData.Email);
                        var messageData = new MessageLimpet(_portalShop, messageTitle, messageText);
                        return messageData.GetDisplayError();
                    }
                }
                LogUtils.LogTracking("SendOrderEmail() - Invalid Order OrderId:" + orderid, _systemData.SystemKey);
                return "Invalid Order";
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }

        public string PrintOrder()
        {
            try
            {
                var orderid = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                var orderData = new OrderLimpet(_portalShop.PortalId, orderid, _sessionParams.CultureCodeEdit);
                if (orderData.Exists)
                {
                    var razorTempl = AssignRemoteTemplate("","PrintOrder.cshtml");
                    return RenderRazorUtils.RazorObjectRender(razorTempl, orderData, _dataObjects, _passSettings, _sessionParams, true);
                }
                LogUtils.LogTracking("PrintOrder() - Invalid Order OrderId:" + orderid, _systemData.SystemKey);
                return "Invalid Order";
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public string PrintShipLabel()
        {
            try
            {
                var orderid = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                var orderData = new OrderLimpet(_portalShop.PortalId, orderid, _sessionParams.CultureCodeEdit);
                if (orderData.Exists)
                {
                    var razorTempl = AssignRemoteTemplate("", "PrintShipLabel.cshtml");
                    return RenderRazorUtils.RazorObjectRender(razorTempl, orderData, _dataObjects, _passSettings, _sessionParams, true);
                }
                LogUtils.LogTracking("PrintShipLabel() - Invalid Order OrderId:" + orderid, _systemData.SystemKey);
                return "Invalid Order";
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public int SaveOrder()
        {
            var orderid = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
            var orderData = new OrderLimpet(_portalShop.PortalId, orderid, _sessionParams.CultureCodeEdit);  
            _passSettings.Add("saved", "true");
            orderData.Save(_postInfo);

            var scode = _paramInfo.GetXmlPropertyInt("genxml/hidden/statuscode");
            if (scode > 0) orderData.ChangeStaus(scode);

            return orderData.OrderId;
        }

        public void DeleteOrder()
        {
            var orderid = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
            if (orderid > 0)
            {
                var orderData = GetOrder(orderid);
                if (orderData != null && orderData.StatusCode == Convert.ToInt32(OrderStatus.Archived))
                {
                    orderData.Delete();
                    LogUtils.LogTracking("delete order: " + orderid + " - " + orderData.OrderNumber + " " + orderData.FullName, _systemData.SystemKey);
                }
            }
        }
        public String AddOrder(string templateName)
        {
            var orderData = GetOrder(-1);
            orderData.StatusCode = (int)OrderStatus.Incomplete;
            return GetOrder(templateName, orderData);
        }

        public String GetOrder(string templateName, int itemId)
        {
            try
            {
                if (itemId > 0)
                {
                    var orderData = GetOrder(itemId);
                    return GetOrder(templateName, orderData);
                }
                return "Invalid Order ItemId";
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetOrder(string templateName, OrderLimpet orderData)
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate(templateName);
                return RenderRazorUtils.RazorDetail(razorTempl, orderData, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }

        }
        public String GetOrderList(string templateName)
        {

            try
            {
                var OrderDataList = new OrderLimpetList(_paramInfo, _portalShop, _sessionParams.CultureCodeEdit, true);
                var razorTempl = _appThemeSystem.GetTemplate(templateName);
                var strOut = RenderRazorUtils.RazorDetail(razorTempl, OrderDataList, _passSettings, _sessionParams, true);
                return strOut;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }

        }
        public bool AddOrderCartItem()
        {
            try
            {
                var productId = _paramInfo.GetXmlPropertyInt("genxml/hidden/productid");
                var orderId = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                if (orderId > 0 && productId > 0)
                {
                    var orderData = GetOrder(orderId);
                    if (orderData.Exists)
                    {
                        orderData.AddProduct(_postInfo, productId);
                        var cKey = CartUtils.GetCartItemKey(productId, _postInfo);
                        orderData.CreateStaticProducts(cKey);
                    }
                    return true;
                }
                LogUtils.LogTracking("ERROR:  Invalid orderId or articleID on AddCartArticle()", _systemData.SystemKey);
                return false;
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return false;
            }
        }
        public int RemoveOrderCartItem()
        {
            try
            {
                var cartitemkey = _paramInfo.GetXmlProperty("genxml/hidden/cartitemkey");
                var orderId = _paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
                if (orderId > 0 && cartitemkey != "")
                {
                    var orderData = GetOrder(orderId);
                    if (orderData.Exists)
                    {
                        orderData.RemoveProduct(cartitemkey);
                        orderData.Update();
                    }
                }
                LogUtils.LogTracking("ERROR:  nvalid orderId or articleID on RemoveCartArticle()", _systemData.SystemKey);
                return orderId;
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return -1;
            }
        }


    }

}
