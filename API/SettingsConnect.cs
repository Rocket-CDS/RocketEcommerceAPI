using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private string SaveSettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.Save(_postInfo);
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        private string DisplaySettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            return RenderSystemTemplate("ModuleSettings.cshtml");
        }
        public string ChatGptReturn()
        {
            var chatGPT = new DNNrocketAPI.Components.ChatGPT();
            var sQuestion = _postInfo.GetXmlProperty("genxml/textbox/chatgptquestion");
            var chatgpttext = chatGPT.SendMsg(sQuestion);
            _sessionParams.Set("chatgptreturn", chatgpttext);
            var razorTempl = AppThemeUtils.AppThemeRocketApi(_dataObject.PortalId).GetTemplate("ChatGptReturn.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

    }
}

