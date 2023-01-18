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
        public String NotificationEdit()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("notification.cshtml");
                var notificationData = new NotificationLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit);
                return RenderRazorUtils.RazorDetail(razorTempl, notificationData, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public String NotificationSave()
        {
            try
            {
                var notificationData = new NotificationLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit);
                notificationData.Save(_postInfo);
                return NotificationEdit();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public String NotificationReset()
        {
            try
            {
                var notificationData = new NotificationLimpet(_portalShop.PortalId, _sessionParams.CultureCodeEdit);
                notificationData.Delete();
                LogUtils.LogTracking("notification_reset", _systemData.SystemKey);
                return NotificationEdit();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

    }
}
