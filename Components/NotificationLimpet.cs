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
    public class NotificationLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private const string _entityTypeCode = "NotificationData";
        private const string _systemkey = "rocketecommerceapi";
        private DNNrocketController _objCtrl;
        private string _guidKey;
        private int _portalId;
        public NotificationLimpet(int portalId, string cultureCode)
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();
            CultureCode = cultureCode;
            _portalId = portalId;
            _guidKey = _entityTypeCode + portalId;
            _objCtrl = new DNNrocketController();

            Info = _objCtrl.GetByGuidKey(portalId, -1, _entityTypeCode, _guidKey, "", _tableName, CultureCode);
            if (Info == null || Info.ItemID <= 0)
            {
                Info = new SimplisityInfo();
                Info.PortalId = _portalId;
                Info.ModuleId = -1;
                Info.TypeCode = _entityTypeCode;
                Info.GUIDKey = _guidKey;
                Info.Lang = CultureCode;
            }
        }

        public void Save(SimplisityInfo info)
        {
            Info.XMLData = info.XMLData;
            Update();
        }
        public void Update()
        {
            _objCtrl.SaveData(Info, _tableName);
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        public string EntityTypeCode { get { return _entityTypeCode; } }


        #region "Info - PortalShop Data"
        public SimplisityInfo Info { get; set; }
        public int PortalId { get { return Info.PortalId; } }
        public string CultureCode { get; set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string SiteKey { get { return Info.GUIDKey; } }

        public string Message_EmailPaymentSubject { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/emailpaymentsubject"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/emailpaymentsubject", value.ToString()); } }
        public string Message_EmailOrderSubject { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/emailordersubject"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/emailordersubject", value.ToString()); } }
        public string Message_Email { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/email"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/email", value.ToString()); } }
        public string Message_StorePickUp { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/storepickup"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/storepickup", value.ToString()); } }
        public string Message_CheckoutOK { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/checkoutok"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/checkoutok", value.ToString()); } }
        public string Message_CheckoutFail { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/checkoutfail"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/checkoutfail", value.ToString()); } }
        public string Message_PaymentForm { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/paymentform"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/paymentform", value.ToString()); } }
        public string Message_PaymentFormEmail { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/paymentformemail"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/paymentformemail", value.ToString()); } }
        public string Message_PaymentFormOK { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/paymentformok"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/paymentformok", value.ToString()); } }
        public string Message_PaymentFormFail { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/paymentformfail"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/paymentformfail", value.ToString()); } }
        public string Message_PaymentReceived { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/paymentreceived"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/paymentreceived", value.ToString()); } }
        public string Message_WaitingForPayment { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/waitingforpayment"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/waitingforpayment", value.ToString()); } }

        public string Message_BankInstructions { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/bankinstructions"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/bankinstructions", value.ToString()); } }
        public string Message_BankPaymentButton { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/bankpayment"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/bankpayment", value.ToString()); } }


        public string Message_ManualPaymentButton { get { return Info.GetXmlProperty("genxml/lang/genxml/messages/manualpayment"); } set { Info.SetXmlProperty("genxml/lang/genxml/messages/manualpayment", value.ToString()); } }

        

        #endregion

    }
}
