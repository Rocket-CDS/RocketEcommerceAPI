using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.ComTypes;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private PaymentLimpet GetPayment(int paymentid)
        {
            return new PaymentLimpet(_dataObject.PortalShop.PortalId, paymentid, _sessionParams.CultureCodeEdit);
        }
        public int SavePayment()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentData = new PaymentLimpet(_dataObject.PortalShop.PortalId, paymentid, _sessionParams.CultureCodeEdit);
            _dataObject.Settings.Add("saved", "true");
            paymentid = paymentData.Save(_postInfo);

            var scode = _paramInfo.GetXmlPropertyInt("genxml/hidden/statuscode");
            if (scode > 0) paymentData.ChangeStaus(scode);

            return paymentid;
        }
        public string SendPaymentEmail()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentLimpet = GetPayment(paymentid);
            paymentLimpet.BankAction = PaymentAction.None;
            paymentLimpet.Update();

            if (paymentLimpet.SendRequestPaymentEmail())
                return EmailSent(paymentLimpet);
            else
                return EmailFail(paymentLimpet);
        }
        public string SendPaymentManagerEmail()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentLimpet = GetPayment(paymentid);
            if (paymentLimpet.SendManagerEmail())
                return EmailSent(paymentLimpet);
            else
                return EmailFail(paymentLimpet);
        }
        public string SendPaymentClientEmail()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentLimpet = GetPayment(paymentid);

            if (paymentLimpet.SendClientEmail())
                return EmailSent(paymentLimpet);
            else
                return EmailFail(paymentLimpet);
        }

        private string EmailFail(PaymentLimpet paymentData)
        {
            var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailfail");
            var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailfail", "Msg");
            messageText = messageText.Replace("{email}", paymentData.Email);
            var messageData = new MessageLimpet(_dataObject.PortalShop, messageTitle, messageText);
            return messageData.GetDisplayError();
        }
        private string EmailSent(PaymentLimpet paymentData)
        {
            var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailsent");
            var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerceAPI/App_LocalResources/", "Help.emailsent", "Msg");
            messageText = messageText.Replace("{email}", paymentData.Email);
            var messageData = new MessageLimpet(_dataObject.PortalShop, messageTitle, messageText);
            messageData.FadeModel = true;
            return messageData.GetDisplayMessage();
        }

        public void DeletePayment()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            GetPayment(paymentid).Delete();
        }
        public string RemovePaymentHistory()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            GetPayment(paymentid).RemoveHistory();
            return GetPaymentDisplay(GetPayment(paymentid));
        }
        public string AddPayment()
        {
            var paymentData = GetPayment(-1);
            paymentData.StatusCode = (int)PaymentStatus.Incomplete;
            return GetPaymentDisplay(paymentData);
        }
        public string GetPaymentDisplay(int paymentId)
        {
            try
            {
                if (paymentId > 0) return GetPaymentDisplay(GetPayment(paymentId));
                return "Invalid Payment PaymentId";
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }
        public string GetPaymentDisplay(PaymentLimpet paymentLimpet)
        {
            try
            {
                paymentLimpet.AmountPayReset(); // reset amountpay, this is a transitional amount for payment. 
                paymentLimpet.Update();
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("paymentdetail.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentLimpet, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }

        }
        public string GetPaymentList()
        {

            try
            {
                var PaymentLimpetList = new PaymentLimpetList(_paramInfo, _dataObject.PortalShop, _sessionParams.CultureCodeEdit, true);
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("paymentlist.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, PaymentLimpetList, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }

        }

        public String GetShippingMethodsList()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("shippingmethods.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetTaxMethodsList()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("taxmethods.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetDiscountMethodsList()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("discountmethods.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }


        #region "Payment Methods"

        public String GetPaymentMethodsList()
        {
            _dataObject.PortalShop.ClearPortalCache();
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("paymentmethods.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string RedirectPaymentFormToBank()
        {
            var paymentid = 0;
            var strOut = "PAYMENT ERROR";
            var guidkey = _paramInfo.GetXmlProperty("genxml/remote/urlparams/key"); // Use remote key if passed from module.
            if (guidkey == "") guidkey = _paramInfo.GetXmlProperty("genxml/urlparams/key");
            if (guidkey == "") guidkey = _paramInfo.GetXmlProperty("genxml/hidden/key");

            var paymentData = new PaymentLimpet(_dataObject.PortalId, guidkey, _sessionParams.CultureCode);
            if (paymentData.PaymentId <= 0)
                paymentid = SavePayment();
            else
                paymentid = paymentData.PaymentId;

            if (paymentid > 0)
            {
                _dataObject.CartData.CollectInStore = false;
                _dataObject.CartData.UpdateBillAddress(_postInfo);
                var cartData = new CartLimpet(_sessionParams.BrowserId, DNNrocketUtils.GetCurrentCulture());

                var paymentLimpet = GetPayment(paymentid);
                paymentLimpet.Load(_postInfo, _paramInfo);
                paymentLimpet.CartId = cartData.Record.ItemID;
                paymentLimpet.SessionData = _sessionParams;

                // Convert cart values, some payment gateways want address for security.
                if (cartData.BillFirstName == "") cartData.BillFirstName = paymentLimpet.FirstName;
                if (cartData.BillLastName == "") cartData.BillLastName = paymentLimpet.LastName;
                if (cartData.BillEmail == "") cartData.BillEmail = paymentLimpet.Email;
                if (cartData.BillCompany == "") cartData.BillCompany = paymentLimpet.Company;
                cartData.Update();

                strOut = paymentLimpet.RedirectedToBankHtml();
            }
            return strOut;
        }
        public string PaymentFormReset()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentLimpet = GetPayment(paymentid);
            if (paymentLimpet.Exists)
            {
                paymentLimpet.ResetBankAction();
            }
            return "";
        }
        public string BankIPN()
        {
            var rtn = "FAIL";
            var paymentprovider = _paramInfo.GetXmlProperty("genxml/urlparams/paymentprovider");
            if (paymentprovider == "") paymentprovider = _paramInfo.GetXmlProperty("genxml/urlparams/p"); // reduce chars for PayBox.  (150 chars limit)
            if (paymentprovider == "") paymentprovider = _paramInfo.GetXmlProperty("genxml/data/paymentprovider"); // from temp data record.
            if (paymentprovider != "")
            {
                var rocketInterface = _dataObject.SystemData.GetProvider(paymentprovider);
                if (rocketInterface != null && rocketInterface.Assembly != "")
                {
                    var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    rtn = bankprov.NotifyEvent(_paramInfo);
                }
            }
            return rtn;
        }

        #endregion
    }
}
