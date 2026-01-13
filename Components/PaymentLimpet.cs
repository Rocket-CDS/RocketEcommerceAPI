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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class PaymentLimpet
    {
        private const string _tableName = "RocketEcommerceAPI";
        private DNNrocketController _objCtrl;
        private SimplisityRecord _oldRecord;
        public PaymentLimpet(int portalId, int paymentId, string langRequired = "")
        {            
            _objCtrl = new DNNrocketController();
            if (paymentId <= 0) paymentId = -1;  // create new record.
            PortalId = portalId;
            Populate(paymentId, langRequired);
        }
        public PaymentLimpet(int portalId, string guidkey, string langRequired = "")
        {
            _objCtrl = new DNNrocketController();
            var paymentId = -1;  // create new record.
            PortalId = portalId;
            var pRecord = _objCtrl.GetRecordByGuidKey(portalId, -1, EntityTypeCode, guidkey, "", _tableName);
            if (pRecord != null) paymentId = pRecord.ItemID;
            Populate(paymentId, langRequired);
        }
        /// <summary>
        /// Used to reload the data from the DB, when it has been updated by a provider.
        /// </summary>
        public void Reload()
        {
            Populate(PaymentId, CultureCode);
        }
        private void Populate(int paymentId, string langRequired)
        {
            Exists = false;
            CultureCode = langRequired;
            if (CultureCode == "") CultureCode = DNNrocketUtils.GetEditCulture();

            var rec = _objCtrl.GetRecord(paymentId, _tableName);
            if (rec == null)
            {
                Record = new SimplisityInfo(CultureCode);
                Record.ItemID = paymentId;
                Record.TypeCode = EntityTypeCode;
                Record.ModuleId = -1;
                Record.UserId = -1;
                Record.PortalId = PortalId;
                Record.GUIDKey = GeneralUtils.GetGuidKey();
                PaymentDate = DateTime.Now;
            }
            else
            {
                Record = rec;
                Exists = true;
            }
            _oldRecord = (SimplisityRecord)Record.Clone();
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
            SystemData = new SystemLimpet("rocketecommerceapi");
        }
        public void Load(SimplisityInfo postInfo, SimplisityInfo paramInfo)
        {
            Email = postInfo.GetXmlProperty("genxml/textbox/email");
            Company = postInfo.GetXmlProperty("genxml/textbox/company");
            FirstName = postInfo.GetXmlProperty("genxml/textbox/firstname");
            LastName = postInfo.GetXmlProperty("genxml/textbox/lastname");
            Ref = postInfo.GetXmlProperty("genxml/textbox/ref");
            Notes = postInfo.GetXmlProperty("genxml/textbox/notes");

            var userid = paramInfo.GetXmlPropertyInt("genxml/hidden/userid");
            if (userid == 0) userid = UserUtils.GetCurrentUserId();
            UserId = userid;
            OrderId = paramInfo.GetXmlPropertyInt("genxml/hidden/orderid");
            PaymentProvider = paramInfo.GetXmlProperty("genxml/hidden/providerkey");
            AmountPayCents = PortalShop.CurrencyConvertCents(PortalShop.CurrencyConvertToCulture(postInfo.GetXmlProperty("genxml/textbox/amountpay")).ToString());
            CartId = 0;
        }
        public void Load(OrderLimpet orderData, string providerkey)
        {
            OrderId = orderData.OrderId;
            Email = orderData.Email;
            Company = orderData.BillCompany;
            FirstName = orderData.FirstName;
            LastName = orderData.LastName;
            Ref = orderData.InvoiceRef;
            Notes = orderData.Notes;
            var userid = orderData.UserId;
            if (userid <= 0) userid = UserUtils.GetCurrentUserId();
            UserId = userid;
            PaymentProvider = providerkey;
            AmountPayCents = orderData.TotalCents;
            CartId = orderData.CartId;
        }


        public void Delete()
        {
            _objCtrl.Delete(Record.ItemID, _tableName);
            LogUtils.LogTracking("delete payment: " + Record.ItemID + " - " + Name + " " + Email, "rocketecommerceapi");
        }

        public int Save(SimplisityInfo postInfo)
        {
            // Only update textbox fields so history list is not removed.
            ///Record.XMLData = postInfo.XMLData;
            Record.RemoveXmlNode("genxml/textbox");
            Record.AddXmlNode(postInfo.XMLDoc.SelectSingleNode("genxml/textbox").OuterXml, "textbox", "genxml");

            // we rewrite to the property, so we use the correct format for the culture.
            var pAmount = PortalShop.CurrencyConvertToCulture(Record.GetXmlPropertyRaw("genxml/textbox/amount"));
            Record.SetXmlPropertyInt("genxml/textbox/amount", PortalShop.CurrencyConvertCents(pAmount.ToString()).ToString());

            var pAmountPaid = PortalShop.CurrencyConvertToCulture(Record.GetXmlPropertyRaw("genxml/textbox/amountpaid"));
            Record.SetXmlPropertyInt("genxml/textbox/amountpaid", PortalShop.CurrencyConvertCents(pAmountPaid.ToString()).ToString());

            var pAmountP = PortalShop.CurrencyConvertToCulture(Record.GetXmlPropertyRaw("genxml/textbox/amountpay"));
            Record.SetXmlPropertyInt("genxml/textbox/amountpay", PortalShop.CurrencyConvertCents(pAmountP.ToString()).ToString());

            if (AmountPaid == Amount) BankAction = PaymentAction.None;

            Record.ItemID = _objCtrl.Update(Record, _tableName);
            return Record.ItemID;
        }
        public int Update(string comment = "")
        {
            AddHistory(comment);

            if (StatusCode == 0) Status = PaymentStatus.Incomplete; // on first create status is "0";
            Record.SortOrder = StatusCode;  // Use SortOrder field to get an index for filter.
            Record.ItemID = _objCtrl.Update(Record, _tableName);
            _oldRecord = Record; // so we can check for change and add history.
            return Record.ItemID;
        }
        public void AddListItem(string listname)
        {
            if (Record.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Record.AddListItem(listname);
            Update();
        }
        public void ChangeStaus(int orderStatusInt)
        {
            ChangeStaus((PaymentStatus)Enum.ToObject(typeof(PaymentStatus), (int)orderStatusInt));
        }
        public void ChangeStaus(PaymentStatus paymentStatus)
        {
            StatusCode = (int)paymentStatus;
            if (OrderId > 0)
            {
                var orderData = new OrderLimpet(PortalId, OrderId, CultureCode);
                if (StatusCode == Convert.ToInt32(PaymentStatus.PaymentOK) || StatusCode == Convert.ToInt32(PaymentStatus.Overpaid))
                {
                    orderData.StatusCode = Convert.ToInt32(OrderStatus.Paid);
                    orderData.Update();
                }
                if (StatusCode == Convert.ToInt32(PaymentStatus.PaymentNotVerified) || StatusCode == Convert.ToInt32(PaymentStatus.OverpaidNotVerified))
                {
                    orderData.StatusCode = Convert.ToInt32(OrderStatus.PaymentNotVerified);
                    orderData.Update();
                }
                if (paymentStatus == PaymentStatus.WaitingForPayment)
                {
                    orderData.StatusCode = Convert.ToInt32(OrderStatus.WaitingForPayment);
                    orderData.Update();
                }
            }
            Update();
        }
        public void AddHistory(string comment = "")
        {
            if (_oldRecord.GetXmlPropertyInt("genxml/hidden/statuscode") > 0 || (comment != ""))  // we do not need a history record for 1st save.
            {
                if (
                (StatusCode != _oldRecord.GetXmlPropertyInt("genxml/hidden/statuscode")) ||
                (AmountCents != _oldRecord.GetXmlPropertyInt("genxml/textbox/amount")) ||
                (AmountPaidCents != _oldRecord.GetXmlPropertyInt("genxml/textbox/amountpaid")) ||
                (comment != "")
                )
                {
                    var sRec = new SimplisityRecord();
                    sRec.SetXmlProperty("genxml/statuscode", _oldRecord.GetXmlPropertyInt("genxml/hidden/statuscode").ToString());
                    sRec.SetXmlProperty("genxml/userid", UserUtils.GetCurrentUserId().ToString());
                    sRec.SetXmlProperty("genxml/username", UserUtils.GetCurrentUserName());
                    sRec.SetXmlProperty("genxml/datetime", DateTime.Now.ToString("O"), TypeCode.DateTime);
                    sRec.SetXmlProperty("genxml/comment", comment);
                    sRec.SetXmlProperty("genxml/amount", AmountDisplay);
                    sRec.SetXmlProperty("genxml/amountpaid", AmountPaidDisplay);

                    Record.AddRecordListItem("history", sRec);
                }
            }
        }
        public List<SimplisityRecord> GetHistory()
        {
            return Record.GetRecordList("history");
        }
        public void RemoveHistory()
        {
            Record.RemoveRecordList("history");
            Update(DNNrocketUtils.GetResourceString(SystemData.SystemRelPath + "/App_LocalResources/", "RE.removehistory"));
        }
        public bool SendEmail(string template = "PaymentEmail.cshtml")
        {
            return EmailSend(Email, template);
        }
        /// <summary>
        /// Manager notification email
        /// </summary>
        /// <returns></returns>
        public bool SendManagerEmail()
        {
            var companyData = new CompanyLimpet(PortalId, CultureCode);
            return EmailSend(companyData.ContactEmail, "PaymentEmailManager.cshtml");
        }
        /// <summary>
        /// Payment received email
        /// </summary>
        /// <returns></returns>
        public bool SendClientEmail()
        {
            return EmailSend(Email, "PaymentEmailClient.cshtml");
        }
        /// <summary>
        /// Payment Request Email
        /// </summary>
        /// <returns></returns>
        public bool SendRequestPaymentEmail()
        {
            return EmailSend(Email, "PaymentEmail.cshtml");
        }
        public void SendEmailPaymentOK()
        {
            // Order
            if (OrderId > 0)
            {
                var ordData = new OrderLimpet(PortalId, OrderId, PreferedCultureCode);
                if (ordData.Exists)
                {
                    ordData.SendEmail();
                    ordData.SendManagerEmail();
                }
                else
                {
                    SendManagerEmail();
                    SendClientEmail();
                }
            }
            else
            {
                SendManagerEmail();
                SendClientEmail();
            }
        }
        private bool EmailSend(string email, string templateName)
        {
            if (GeneralUtils.IsEmail(email))
            {
                var notificationData = new NotificationLimpet(PortalId, PreferedCultureCode);
                var emailLimpet = new EmailLimpet(PortalId, this, CultureCode);
                if (emailLimpet.SendEmail(email, templateName, notificationData.Message_EmailPaymentSubject))
                {
                    Update(DNNrocketUtils.GetResourceString(SystemData.SystemRelPath + "/App_LocalResources/", "Help.emailsent") + ": " + email);
                    return true;
                }
                else
                {
                    Update(DNNrocketUtils.GetResourceString(SystemData.SystemRelPath + "/App_LocalResources/", "Help.emailfail") + ": " + email);
                    Update("ERROR " + emailLimpet.ErrorMessage);
                    return false;
                }
            }
            else
            {
                Update("Invalid Email - " + DNNrocketUtils.GetResourceString(SystemData.SystemRelPath + "/App_LocalResources/", "Help.emailfail") + ": " + email);
                return false;
            }
        }

        public string RedirectedToBankHtml()
        {
            var strOut = "Invalid Bank Redirect";
            var rocketInterface = SystemData.GetProvider(PaymentProvider);
            if (rocketInterface != null && AmountPayCents > 0)
            {
                if (rocketInterface.Assembly != "")
                {
                    Status = PaymentStatus.WaitingForBank;
                    BankAction = PaymentAction.BankPost;
                    if (AmountCents == 0) AmountCents = AmountPayCents;
                    Update();

                    var bankprov = PaymentInterface.Instance(rocketInterface.Assembly, rocketInterface.NameSpaceClass);
                    strOut = bankprov.GetBankRemotePost(this);
                }
            }
            return strOut;
        }
        public void PaymentFailed()
        {
            if (BankAction == PaymentAction.BankPost)
            {
                Status = PaymentStatus.PaymentFailed;
                BankAction = PaymentAction.BankReturn;
                Update();
            }
        }
        public void ResetBankAction()
        {
            AmountPayCents = 0;
            BankAction = PaymentAction.None;
            Update();
        }
        public void RemoveCart()
        {
            //Delete Cart on succesful payment. (may not exist)
            if (CartId > 0) _objCtrl.Delete(CartId, "DNNrocketTemp");
        }
        public void Paid(bool verified)
        {
            // check if already updated.
            if (BankAction == PaymentAction.BankPost || verified)
            {
                BankAction = PaymentAction.BankReturn;
                Update();

                Record.Lang = CultureCode; // set to current language, so currency if calculated correctly.

                if (AmountCents <= 0) AmountCents = AmountPayCents;

                // [WORKAROUND: We are turning off overpaid amount.]]
                // if (AmountPayCents != 0) AmountPaidCents += AmountPayCents;
                if (AmountPayCents != 0) AmountPaidCents = AmountPayCents; 
                // I think this is due to a race condition with the return.  
                // Overpayment should never happen in this system.
                // If at anytime they do need to work, this will need to be solved for each payment gateway.

                if (AmountPaidCents < AmountCents)
                {
                    if (verified)
                        ChangeStaus(PaymentStatus.Partial);
                    else
                        ChangeStaus(PaymentStatus.PartialNotVerified);
                }
                else
                {
                    if (AmountPaidCents == AmountCents)
                    {
                        if (verified)
                            ChangeStaus(PaymentStatus.PaymentOK);
                        else
                            ChangeStaus(PaymentStatus.PaymentNotVerified);
                    }
                    else
                    {
                        if (verified)
                            ChangeStaus(PaymentStatus.Overpaid);
                        else
                            ChangeStaus(PaymentStatus.OverpaidNotVerified);
                    }
                }
                SendEmailPaymentOK(); // before we assign the AmountDue, so we use the amount paid in the email.
                AmountPayCents = 0;
            }
            RemoveCart();
        }


        #region "Properties"

        public SessionParams SessionData { get; set; }
        public SystemLimpet SystemData { get; private set; }
        public PortalShopLimpet PortalShop { get; private set; }
        public SimplisityRecord Record { get; private set; }
        public string CultureCode { get; private set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public string EntityTypeCode { get { return "PAYMENT"; } }
        public int PortalId { get; set; }
        public bool Exists { get; private set; }
        public int ModuleId { get { return Record.ModuleId; } set { Record.ModuleId = value; } }
        public int XrefItemId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public string PaymentGuid { get { return Record.GUIDKey; } }
        public int UserId { get { return Record.UserId; } set { Record.UserId = value; } }
        public int PaymentId { get { return Record.ItemID; } }
        public string GUIDKey { get { return Record.GUIDKey; } }
        public string PaymentKey { get { return Record.GUIDKey; } }
        public int OrderId { get { return Record.ParentItemId; } set { Record.ParentItemId = value; } }
        public string InvoiceRef { get { return Record.GetXmlProperty("genxml/textbox/invoiceref"); } set { Record.SetXmlProperty("genxml/textbox/invoiceref", value); } }
        public string Ref { get { return Record.GetXmlProperty("genxml/textbox/ref"); } set { Record.SetXmlProperty("genxml/textbox/ref", value); } }
        public string SecurityCode { get { return Record.GetXmlProperty("genxml/hidden/securitycode"); } set { Record.SetXmlProperty("genxml/hidden/securitycode", value); } }
        public DateTime PaymentDate { get { return Record.GetXmlPropertyDate("genxml/hidden/paymentdate"); } set { Record.SetXmlProperty("genxml/hidden/paymentdate", value.ToString("O"), TypeCode.DateTime); } }
        public int StatusCode { get { return Record.GetXmlPropertyInt("genxml/hidden/statuscode"); } set {
                Record.SortOrder = value;  // use the sortorder to help speed selection in SQL
                Record.SetXmlProperty("genxml/hidden/statuscode", value.ToString());
            } }
        public PaymentStatus Status
        {
            get
            {
                if (Enum.IsDefined(typeof(PaymentStatus), StatusCode))
                {
                    return (PaymentStatus)StatusCode;
                }
                else
                {
                    return PaymentStatus.Incomplete;
                }
            }
            set {
                Record.SortOrder = Convert.ToInt32(value);  // use the sortorder to help speed selection in SQL
                Record.SetXmlProperty("genxml/hidden/statuscode", Convert.ToInt32(value).ToString());
            }
        }
        public int BankActionCode { get { return Record.GetXmlPropertyInt("genxml/hidden/bankaction"); } set { Record.SetXmlProperty("genxml/hidden/bankaction", value.ToString()); } }
        public PaymentAction BankAction
        {
            get
            {
                if (Enum.IsDefined(typeof(PaymentAction), BankActionCode))
                {
                    return (PaymentAction)BankActionCode;
                }
                else
                {
                    return PaymentAction.None;
                }
            }
            set { Record.SetXmlProperty("genxml/hidden/bankaction", Convert.ToInt32(value).ToString()); }
        }
        /// <summary>
        /// Reset the amountpay to an empty string, so the default amount is used on a form.
        /// </summary>
        public void AmountPayReset()
        {
            Record.SetXmlPropertyDecimal("genxml/textbox/amountpay", "");
        }
        public string Name { get { return Record.GetXmlProperty("genxml/textbox/firstname") + " " + Record.GetXmlProperty("genxml/textbox/lastname"); } }
        public string LastName { get { return Record.GetXmlProperty("genxml/textbox/lastname"); } set { Record.SetXmlProperty("genxml/textbox/lastname", value); } }
        public string FirstName { get { return Record.GetXmlProperty("genxml/textbox/firstname"); } set { Record.SetXmlProperty("genxml/textbox/firstname", value); } }
        public string Email { get { return Record.GetXmlProperty("genxml/textbox/email"); } set { Record.SetXmlProperty("genxml/textbox/email", value); } }
        public string EmailMessage { get { return Record.GetXmlProperty("genxml/textbox/emailmessage"); } set { Record.SetXmlProperty("genxml/textbox/emailmessage", value); } }
        public string PaymentProvider { get { return Record.GetXmlProperty("genxml/textbox/paymentprovider"); } set { Record.SetXmlProperty("genxml/textbox/paymentprovider", value); } }
        public string Company { get { return Record.GetXmlProperty("genxml/textbox/company"); } set { Record.SetXmlProperty("genxml/textbox/company", value); } }
        public string PreferedCultureCode { get { return Record.GetXmlProperty("genxml/textbox/preferedculturecode"); } set { Record.SetXmlProperty("genxml/textbox/preferedculturecode", value); } }
        /// <summary>
        /// The Amount being paid.
        /// </summary>
        public int AmountPayCents { get { return Record.GetXmlPropertyInt("genxml/textbox/amountpay"); } set { Record.SetXmlProperty("genxml/textbox/amountpay", value.ToString()); } }
        public decimal AmountPay
        {
            get { return (PortalShop.CurrencyCentsToDollars(AmountPayCents)); }
        }
        public string AmountPayDisplay
        {
            get { return AmountPay.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        /// <summary>
        /// The total amount of the payment requested
        /// </summary>
        public int AmountCents { get { return Record.GetXmlPropertyInt("genxml/textbox/amount"); } set { Record.SetXmlProperty("genxml/textbox/amount", value.ToString()); } }
        public decimal Amount
        {
            get { return (PortalShop.CurrencyCentsToDollars(AmountCents)); }
        }
        public string AmountDisplay
        {
            get { return Amount.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        /// <summary>
        /// The amount that HAS been paid.
        /// </summary>
        public int AmountPaidCents { get { return Record.GetXmlPropertyInt("genxml/textbox/amountpaid"); } set { Record.SetXmlProperty("genxml/textbox/amountpaid", value.ToString()); } }
        public decimal AmountPaid
        {
            get { return (PortalShop.CurrencyCentsToDollars(AmountPaidCents)); }
        }
        public string AmountPaidDisplay
        {
            get { return AmountPaid.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }
        /// <summary>
        /// The amount that is left to pay. (Amount - AmountPaid)
        /// </summary>
        public int AmountDueCents { get { return (AmountCents - AmountPaidCents); } }
        public decimal AmountDue
        {
            get { return (PortalShop.CurrencyCentsToDollars(AmountDueCents)); }
        }
        public string AmountDueDisplay
        {
            get { return AmountDue.ToString("C", CultureInfo.GetCultureInfo(PortalShop.CurrencyCultureCode)); }
        }


        public string Notes { get { return Record.GetXmlProperty("genxml/textbox/notes"); } set { Record.SetXmlProperty("genxml/textbox/notes", value); } }
        public string BankMessage { get { return Record.GetXmlProperty("genxml/textbox/bankmessage"); } set { Record.SetXmlProperty("genxml/textbox/bankmessage", value); } }
        public bool IsPaymentCompleted
        {
            get {
                if (AmountDue <= 0) return true;
                return false;
            }
        }
        public bool PaymentMade
        {
            get
            {
                if (
                    Status == PaymentStatus.PaymentOK
                    || Status == PaymentStatus.PaymentNotVerified
                    || Status == PaymentStatus.Partial
                    || Status == PaymentStatus.PartialNotVerified
                    || Status == PaymentStatus.Overpaid
                    || Status == PaymentStatus.OverpaidNotVerified
                   )
                {
                    return true;
                }
                return false;
            }
        }
        public int CartId { get { return Record.GetXmlPropertyInt("genxml/textbox/cartid"); } set { Record.SetXmlProperty("genxml/textbox/cartid", value.ToString()); } }

        #endregion

    }
}
