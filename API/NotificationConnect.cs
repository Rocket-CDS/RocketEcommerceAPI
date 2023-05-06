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
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("notification.cshtml");
                var notificationData = new NotificationLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCodeEdit);
                return RenderRazorUtils.RazorDetail(razorTempl, notificationData, _dataObject.Settings, _sessionParams, true);
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
                var notificationData = new NotificationLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCodeEdit);
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
                var notificationData = new NotificationLimpet(_dataObject.PortalShop.PortalId, _sessionParams.CultureCodeEdit);
                notificationData.Delete();
                LogUtils.LogTracking("notification_reset", _systemkey);
                return NotificationEdit();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

    }
}
