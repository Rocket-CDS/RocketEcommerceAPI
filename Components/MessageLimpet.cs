using DNNrocketAPI.Components;
using RocketPortal.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public class MessageLimpet
    {
        public Dictionary<string, string> PassSettings;
        private PortalLimpet _portalData;
        private AppThemeSystemLimpet _appThemeSystem;
        public MessageLimpet(PortalShopLimpet portalShop, string title = "", string message = "")
        {
            Title = title;
            Message = message;
            FadeModel = false;
            SystemData = new SystemLimpet("rocketecommerceapi");
            Template = "MessageModel.cshtml";
            PassSettings = new Dictionary<string, string>();
            PortalShop = portalShop;
            _portalData = new PortalLimpet(portalShop.PortalId);
            _appThemeSystem = new AppThemeSystemLimpet(PortalUtils.GetPortalId(), SystemData.SystemKey);
        }
        #region "Info - "
        public PortalShopLimpet PortalShop { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool FadeModel { get; set; }
        public SystemLimpet SystemData { get; set; }
        public string Template { get; set; }
        public string MessageType { get; set; }
        public string AvatarUrl { get; set; }

        public string GetDisplayHelp()
        {
            AvatarUrl = _portalData.EngineUrlWithProtocol.TrimEnd('/') +  "/DesktopModules/DNNrocket/api/images/avatar2.png";
            MessageType = "help";
            return GetDisplay();
        }
        public string GetDisplayMessage()
        {
            AvatarUrl = _portalData.EngineUrlWithProtocol.TrimEnd('/') + "/DesktopModules/DNNrocket/api/images/avatar3.png";
            MessageType = "message";
            return GetDisplay();
        }
        public string GetDisplayError()
        {
            AvatarUrl = _portalData.EngineUrlWithProtocol.TrimEnd('/') + "/DesktopModules/DNNrocket/api/images/avatar6.png";
            MessageType = "error";
            return GetDisplay();
        }
        public string GetDisplayInfo()
        {
            AvatarUrl = _portalData.EngineUrlWithProtocol.TrimEnd('/') + "/DesktopModules/DNNrocket/api/images/avatar4.png";
            MessageType = "info";
            return GetDisplay();
        }
        private string GetDisplay()
        {
            var razorTempl = _appThemeSystem.GetTemplate(Template);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, this, null, null, null, true);
            if (pr.StatusCode != "00")
            {
                LogUtils.LogSystem("ERROR: MessageLimpet.cs - Invalid render of GetDisplay()");
                return "";
            }
            return pr.RenderedText;
        }

        #endregion

    }

}
