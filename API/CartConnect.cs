using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        public string GetMiniCartDisplay()
        {
            try
            {
                var razorTempl = _appTheme.GetTemplate("minicart.cshtml");
                if (razorTempl == "") razorTempl = _appThemeDefault.GetTemplate("minicart.cshtml");
                if (razorTempl == "") return "No MiniCart.cshtml Template ";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                _dataObjects.Add("cartdata", cartData);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public string GetMiniCartJson()
        {
            var cartData3 = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("qty", cartData3.QtyCount.ToString());
            dictionary.Add("total", cartData3.TotalDisplay);
            dictionary.Add("subtotal", cartData3.SubTotalDisplay);
            dictionary.Add("taxtotal", cartData3.TaxTotalDisplay);
            dictionary.Add("shiptotal", cartData3.ShippingTotalDisplay);
            var s = "{";
            foreach (var d in dictionary)
            {
                s += "\"" + d.Key + "\":\"" + d.Value + "\",";
            }
            return s.TrimEnd(',') + "}";
        }
        public string AddToCart(string returntype = "")
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.AddProduct(_postInfo);
                if (returntype == "json")
                    return GetMiniCartJson();
                else
                    return GetMiniCartDisplay();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "Error: check log";
            }
        }
        public string RemoveCartItem()
        {
            try
            {
                var cartitemindex = _paramInfo.GetXmlPropertyInt("genxml/hidden/cartitemindex");
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.RemoveProduct(cartitemindex);

                return "";
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string ValidateCart()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.ValidateCart();
                cartData.Update();
                return "";
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string SaveCartList()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.UpdateProductList(_postInfo);
                return GetPublicCartContact();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string SaveCartContact()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.UpdateContact(_postInfo);
                return GetPublicCartBillAddress();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string SaveCartBill()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.UpdateBillAddress(_postInfo);
                return GetPublicCartShipAddress();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string SaveCartShip()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.UpdateShipAddress(_postInfo);
                return GetPublicCartSummary();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }
        public string SaveCartDetails()
        {
            try
            {
                SaveCartContact();
                SaveCartBill();
                SaveCartShip();
                return GetPublicCartSummary();
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                return "FAIL!!!";
            }
        }

        public String GetPublicCartListHeader()
        {
            try
            {
                var razorTempl = AssignRemoteHeaderTemplate();
                if (razorTempl == "") return "";
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartDelete()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                cartData.Delete();
                return GetPublicCartList();
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartList()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartContact()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartBillAddress()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartShipAddress()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartSummary()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public String GetPublicCartPayment()
        {
            try
            {
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var orderData = cartData.ConvertToOrder();                
                // create payment and link to order
                var paymentData = new PaymentLimpet(_portalShop.PortalId, -1, _sessionParams.CultureCode);
                paymentData.Load(orderData, _paramInfo.GetXmlProperty("genxml/hidden/providerkey"));
                
                // we need to session data to get the remote return page.
                paymentData.SessionData = _sessionParams;

                var paymentId = paymentData.Update();

                // attache to order.
                orderData.AddPayment(paymentId);
                orderData.ChangeStaus(OrderStatus.WaitingForPayment);
                orderData.Update();
                return paymentData.RedirectedToBankHtml();
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public string CartBankReturnDisplay()
        {
            var paymentData = CartUtils.CartBankReturn(_paramInfo);
            var razorTempl = AssignRemoteTemplate("", "bankreturn.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public String GetPublicPaymentOptions()
        {
            try
            {
                var razorTempl = _appTheme.GetTemplate("payment.cshtml");
                if (razorTempl == "") razorTempl = _appThemeDefault.GetTemplate("payment.cshtml");
                if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
                var cartData = new CartLimpet(_paramInfo, _sessionParams.CultureCode);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, cartData, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }


    }
}
