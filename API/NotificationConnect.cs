using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        public String NotificationEdit()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("notification.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.NotificationData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String NotificationSave()
        {
            _dataObject.NotificationData.Save(_postInfo);
            return NotificationEdit();
        }
        public String NotificationReset()
        {
            _dataObject.NotificationData.Delete();
            return NotificationEdit();
        }

    }
}
