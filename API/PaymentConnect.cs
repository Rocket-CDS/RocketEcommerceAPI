using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using RocketEcommerceAPI.Interfaces;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private PaymentLimpet GetPayment(int paymentid)
        {
            return new PaymentLimpet(_portalShop.PortalId, paymentid, _sessionParams.CultureCodeEdit);
        }
        public int SavePayment()
        {
            var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
            var paymentData = new PaymentLimpet(_portalShop.PortalId, paymentid, _sessionParams.CultureCodeEdit);
            _passSettings.Add("saved", "true");
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
            var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketECommerce/App_LocalResources/", "Help.emailfail");
            var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketECommerce/App_LocalResources/", "Help.emailfail", "Msg");
            messageText = messageText.Replace("{email}", paymentData.Email);
            var messageData = new MessageLimpet(_portalShop, messageTitle, messageText);
            return messageData.GetDisplayError();
        }
        private string EmailSent(PaymentLimpet paymentData)
        {
            var messageTitle = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketECommerce/App_LocalResources/", "Help.emailsent");
            var messageText = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketECommerce/App_LocalResources/", "Help.emailsent", "Msg");
            messageText = messageText.Replace("{email}", paymentData.Email);
            var messageData = new MessageLimpet(_portalShop, messageTitle, messageText);
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
                var razorTempl = _appThemeSystem.GetTemplate("paymentdetail.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentLimpet, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }

        }

        public string GetRemotePaymentHeader()
        {
            try
            {
                var paymentLimpet = new PaymentLimpet(PortalUtils.GetPortalId(), -1); // create new payment (not on DB until it is updated)
                var razorTempl = AssignRemoteHeaderTemplate();
                var guidkey = _paramInfo.GetXmlProperty("genxml/urlparams/key");
                if (guidkey != "")
                {
                    paymentLimpet = new PaymentLimpet(PortalUtils.GetPortalId(), guidkey);  // if we have a "key" param, we should have a payment record.
                }
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentLimpet, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return LogUtils.LogException(ex);
            }
        }


        public string GetRemotePaymentDisplay()
        {
            try
            {
                var razorTempl = AssignRemoteTemplate();
                var paymentLimpet = new PaymentLimpet(PortalUtils.GetPortalId(), -1); // create new payment (not on DB until it is updated)                
                var guidkey = _paramInfo.GetXmlProperty("genxml/urlparams/key");
                if (guidkey == "") guidkey = _paramInfo.GetXmlProperty("genxml/hidden/key");
                if (guidkey != "") paymentLimpet = new PaymentLimpet(PortalUtils.GetPortalId(), guidkey);  // if we have a "key" param, we should have a payment record.
                if (paymentLimpet.BankAction == PaymentAction.BankPost)
                {
                    var rocketInterface = _systemData.GetProvider(paymentLimpet.PaymentProvider);
                    if (rocketInterface != null)
                    {
                        if (rocketInterface.Assembly != "")
                        {
                            var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                            bankprov.ReturnEvent(_paramInfo);
                            paymentLimpet.Reload(); // reload so we have the changed, that need to be past to the display.
                        }
                    }
                    paymentLimpet.BankAction = PaymentAction.BankReturn;
                    paymentLimpet.Update();
                }
                paymentLimpet.AmountPayCents = paymentLimpet.AmountDueCents;
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, paymentLimpet, _dataObjects, _passSettings, _sessionParams, true);
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
                var PaymentLimpetList = new PaymentLimpetList(_paramInfo, _portalShop, _sessionParams.CultureCodeEdit, true);
                var razorTempl = _appThemeSystem.GetTemplate("paymentlist.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, PaymentLimpetList, _dataObjects, _passSettings, _sessionParams, true);
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
            try
            {
                _portalShop = new PortalShopLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit); // may be change of language.
                var razorTempl = _appThemeSystem.GetTemplate("shippingmethods.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String GetTaxMethodsList()
        {
            try
            {
                _portalShop = new PortalShopLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit); // may be change of language.
                var razorTempl = _appThemeSystem.GetTemplate("taxmethods.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String GetDiscountMethodsList()
        {
            try
            {
                _portalShop = new PortalShopLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit); // may be change of language.
                var razorTempl = _appThemeSystem.GetTemplate("discountmethods.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        #region "Payment Methods"

        public String GetPaymentMethodsList()
        {
            _portalShop.ClearPortalCache();
            _portalShop = new PortalShopLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit); // may be change of language.
            var razorTempl = _appThemeSystem.GetTemplate("paymentmethods.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalShop, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string RedirectPaymentFormToBank()
        {
            try
            {
                var paymentid = 0;
                var strOut = "PAYMENT ERROR";
                var guidkey = _paramInfo.GetXmlProperty("genxml/remote/urlparams/key"); // Use remote key if passed from module.
                if (guidkey == "") guidkey = _paramInfo.GetXmlProperty("genxml/urlparams/key");
                if (guidkey == "") guidkey = _paramInfo.GetXmlProperty("genxml/hidden/key");
                var paymentData = new PaymentLimpet(_portalData.PortalId, guidkey, _sessionParams.CultureCode);
                if (paymentData.PaymentId <= 0)
                    paymentid = SavePayment();
                else
                    paymentid = paymentData.PaymentId;

                if (paymentid > 0)
                {               
                    var paymentLimpet = GetPayment(paymentid);
                    paymentLimpet.Load(_postInfo, _paramInfo);
                    paymentLimpet.SessionData = _sessionParams;
                    strOut = paymentLimpet.RedirectedToBankHtml();
                }
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string PaymentFormReset()
        {
            try
            {
                var paymentid = _paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid");
                var paymentLimpet = GetPayment(paymentid);
                if (paymentLimpet.Exists)
                {
                    paymentLimpet.ResetBankAction();
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string BankIPN()
        {
            var rtn = "";
            try
            {
                var paymentprovider = _paramInfo.GetXmlProperty("genxml/urlparams/paymentprovider");
                if (paymentprovider == "") paymentprovider = _paramInfo.GetXmlProperty("genxml/urlparams/p"); // reduce chars for PayBox.  (150 chars limit)
                // if the payment provider is not in the url, it may have been passed by using the TempStorage.
                if (paymentprovider == "") paymentprovider = _paramInfo.GetXmlProperty("genxml/genxml/paymentprovider");
                if (paymentprovider == "") paymentprovider = _paramInfo.GetXmlProperty("genxml/genxml/p");

                var rocketInterface = _systemData.GetProvider(paymentprovider);
                if (rocketInterface != null && rocketInterface.Assembly != "")
                {
                    var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    rtn = bankprov.NotifyEvent(_paramInfo);
                }
                return rtn;
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
            }
            return rtn;
        }

        #endregion
    }
}
