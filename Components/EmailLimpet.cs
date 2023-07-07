using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class EmailLimpet
    {
        private SimplisityRazor _model;
        private EmailSenderData _emailData;
        public EmailLimpet(int portalId, object modelObject, string cultureCode = "")
        {
            if (cultureCode == "") cultureCode = DNNrocketUtils.GetCurrentCulture();
            CultureCode = cultureCode;
            PortalId = portalId;
            PortalShop = new PortalShopLimpet(PortalId, CultureCode);
            SystemData = new SystemLimpet(SystemKey);
            CompanyData = new CompanyLimpet(PortalId, CultureCode);
            _model = new SimplisityRazor(modelObject);
            _emailData = new EmailSenderData();
            ErrorMessage = "";
        }
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="templateName"></param>
        /// <param name="subject"></param>
        /// <returns>true if email sent, fail is failed.  [If shop emails are off, then no email is sent. In that case, if in debug mode we still return true, in not in debug module we return false.]</returns>
        public bool SendEmail(string toEmail, string templateName, string subject = "")
        {
            var appThemeDefault = new AppThemeLimpet(PortalId, SystemData, "Default", "1.0");

            _emailData.ToEmail = toEmail;
            _emailData.FromEmail = CompanyData.FromEmail;
            _emailData.ReplyToEmail = CompanyData.ContactEmail;
            _emailData.EmailSubject = CompanyData.CompanyName + " : " + subject;
            _emailData.RazorTemplateName = templateName;
            _emailData.Model = _model;
            _emailData.Model.SetDataObject("companyData", CompanyData);
            _emailData.CultureCode = CultureCode;
            _emailData.AppTheme = appThemeDefault;

            if (appThemeDefault == null) ErrorMessage += " : Email AppTheme is NULL";

            var emailSender = new EmailSender(_emailData);

            if (PortalShop.DebugMode)
            {
                LogUtils.OutputDebugFile(SystemKey + "_" + _emailData.RazorTemplateName + "_Email.html", emailSender.RenderEmailBody());
            }

            if (PortalShop.EmailActive)
            {
                var emailsent = emailSender.SendEmail();
                ErrorMessage += " : " + emailSender.Error;
                return emailsent;
            }
            else
            {
                ErrorMessage += " : PortalShop.EmailActive is FALSE";
                return false;
            }
        }

        public string CultureCode { get; private set; }
        public string SystemKey { get { return "rocketecommerceapi"; } }
        public int PortalId { get; set; }
        public PortalShopLimpet PortalShop { get; set; }
        public SystemLimpet SystemData { get; set; }
        public CompanyLimpet CompanyData { get; set; }
        public string ErrorMessage { get; set; }

    }
}
