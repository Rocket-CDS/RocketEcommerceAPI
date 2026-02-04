using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        public string GetMiniCartDisplay()
        {
            var razorTempl = _dataObject.AppTheme.GetTemplate("minicart.cshtml");
            if (razorTempl == "") razorTempl = _dataObject.AppThemeDefault.GetTemplate("minicart.cshtml");
            if (razorTempl == "") return "No MiniCart.cshtml Template ";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string GetMiniCartJson()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("qty", _dataObject.CartData.QtyCount.ToString());
            dictionary.Add("total", _dataObject.CartData.TotalDisplay);
            dictionary.Add("subtotal", _dataObject.CartData.SubTotalDisplay);
            dictionary.Add("taxtotal", _dataObject.CartData.TaxTotalDisplay);
            dictionary.Add("shiptotal", _dataObject.CartData.ShippingTotalDisplay);
            var s = "{";
            foreach (var d in dictionary)
            {
                s += "\"" + d.Key + "\":\"" + d.Value + "\",";
            }
            return s.TrimEnd(',') + "}";
        }
        public string AddToCart(string returntype = "")
        {
            _dataObject.CartData.AddProduct(_postInfo);
            if (returntype == "json")
                return GetMiniCartJson();
            else
                return GetMiniCartDisplay();
        }
        public string AddToCartReturnCartList()
        {
            _dataObject.CartData.AddProduct(_postInfo);
            return GetPublicCartList();
        }
        public string RemoveCartItem()
        {
            var cartitemindex = _paramInfo.GetXmlPropertyInt("genxml/hidden/cartitemindex");
            _dataObject.CartData.RemoveProduct(cartitemindex);
            return "";
        }
        public string ValidateCart()
        {
            _dataObject.CartData.ValidateCart();
            _dataObject.CartData.Update();
            return "";
        }
        public string SaveCartList()
        {
            _dataObject.CartData.UpdateProductList(_postInfo);
            _dataObject.ReloadCart();
            return GetPublicCartContact();
        }
        public string SaveCartContact()
        {
            _dataObject.CartData.UpdateContact(_postInfo);
            return GetPublicCartBillAddress();
        }
        public string SaveCartBill()
        {
            _dataObject.CartData.UpdateBillAddress(_postInfo);
            return GetPublicCartShipAddress();
        }
        public string SaveCartShip()
        {
            _dataObject.CartData.UpdateShipAddress(_postInfo);
            return GetPublicCartSummary();
        }
        public string SaveCartDetails()
        {
            SaveCartContact();
            SaveCartBill();
            SaveCartShip();
            return GetPublicCartSummary();
        }

        public String GetPublicCartListHeader()
        {
            var razorTempl = AssignRemoteHeaderTemplate();
            if (razorTempl == "") return "";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartDelete()
        {
            _dataObject.CartData.Delete();
            return GetPublicCartList();
        }
        public String GetPublicCartList()
        {
            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartContact()
        {
            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartBillAddress()
        {
            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartShipAddress()
        {
            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartSummary()
        {
            _dataObject.SetDataObject("cartdata", new CartLimpet(_sessionParams.BrowserId, _sessionParams.CultureCode)); // reload, for discount changes.

            var razorTempl = AssignRemoteTemplate();
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetPublicCartPayment()
        {
                var orderData = _dataObject.CartData.ConvertToOrder();
                // create payment and link to order
                var paymentData = new PaymentLimpet(_dataObject.PortalId, -1, _sessionParams.CultureCode);
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
        public string CartBankReturnDisplay()
        {
            var paymentData = CartUtils.CartBankReturn(_paramInfo);
            var razorTempl = AssignRemoteTemplate("", "bankreturn.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public String GetPublicPaymentOptions()
        {
            var razorTempl = _dataObject.AppTheme.GetTemplate("payment.cshtml");
            if (razorTempl == "") razorTempl = _dataObject.AppThemeDefault.GetTemplate("payment.cshtml");
            if (razorTempl == "") return "No Razor Template for Cart List.  Check the remoteParams class that has been past.";
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.CartData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }


    }
}
