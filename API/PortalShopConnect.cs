using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;

namespace RocketEcommerceAPI.API
{
    public partial class StartConnect
    {
        private void ResetPortalShop(int portalId, string storeName = "")
        {
            var defaultFileMapPath = DNNrocketUtils.MapPath(_rocketInterface.TemplateRelPath).TrimEnd('\\') + "\\default_portalshop.xml";
            var defaultxml = FileUtils.ReadFile(defaultFileMapPath);
            var portalShop = new PortalShopLimpet(portalId, _sessionParams.CultureCodeEdit);

            var sitekey = portalShop.SiteKey;

            if (defaultxml != "")
            {
                var tempInfo = new SimplisityInfo();
                tempInfo.FromXmlItem(defaultxml);
                portalShop.Record.XMLData = tempInfo.XMLData;
            }

            portalShop.SiteKey = sitekey;
            portalShop.LogoRelPath = "";

            portalShop.Update();

            CacheUtils.ClearAllCache();
            CacheUtilsDNN.ClearAllCache();
            DNNrocketUtils.ClearAllCache();
        }
        private string SavePortalShop()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                var portalShop = new PortalShopLimpet(portalId, _sessionParams.CultureCodeEdit);
                if (portalShop.PortalId >= 0)
                {
                    portalShop.Save(_postInfo);
                    _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _systemkey + "setup", "True");
                    _dataObject.PortalData.Update();
                }
                _dataObject.PortalShop.ClearPortalCache();
                _dataObject.SetDataObject("portaldata", new PortalLimpet(portalId));

                return "OK";
            }
            return "Invalid Portal SiteKey";
        }
        private void DeletePortalShop()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                var PortalShop = new PortalShopLimpet(portalId, _sessionParams.CultureCodeEdit);
                PortalUtils.DeletePortal(portalId); // Delete base portal will crash install.
                DNNrocketUtils.RecycleApplicationPool();
                PortalShop.Delete();
                _userParams.TrackClear(_systemkey);
            }
        }
        public string ValidateShop()
        {
            foreach (var l in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalShop.PortalId))
            {
                var articleDataList = new ProductLimpetList(_sessionParams, _dataObject.PortalShop, _sessionParams.CultureCodeEdit, true);
                articleDataList.Validate();
            }
            DNNrocketUtils.RecycleApplicationPool();
            return "OK";
        }
        private String CountrySettings()
        {
            try
            {
                var razorTempl = _dataObject.AppThemeSystem.GetTemplate("CountrySettings.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalShop, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private String CopyLanguage()
        {
            var overwritelang = _postInfo.GetXmlPropertyBool("genxml/overwritelang");
            var sourcelanguage = _postInfo.GetXmlProperty("genxml/sourcelanguage");
            var destlanguage = _postInfo.GetXmlProperty("genxml/destlanguage");
            if (sourcelanguage != destlanguage)
            {
                var objCtrl = new DNNrocketController();

                // Products
                var articleList = objCtrl.GetList(_dataObject.PortalShop.PortalId, -1, "PRD", "", sourcelanguage, "", 0, 0, 0, 0, "RocketEcommerceAPI");
                foreach (var p in articleList)
                {
                    var prdSource = new ProductLimpet(p.PortalId, p.ItemID, sourcelanguage);
                    var prdDest = new ProductLimpet(p.PortalId, p.ItemID, destlanguage);
                    if (!prdDest.Exists || overwritelang)
                    {
                        prdSource.Info.Lang = destlanguage;
                        prdSource.Update();
                    }
                    else
                    {
                        var lp = 0;
                        if (prdDest.Name == "") prdDest.Name = prdSource.Name;
                        if (prdDest.Summary == "") prdDest.Summary = prdSource.Summary;
                        if (prdDest.RichText  == "") prdDest.RichText = prdSource.RichText;
                        foreach (var m in prdDest.GetModels())
                        {
                            if (m.Name == "") prdDest.Info.SetXmlProperty("genxml/lang/genxml/modellist/genxml[" + (lp + 1) + "]/textbox/modelname", prdSource.GetModel(lp).Name);
                            lp += 1;
                        }
                        lp = 0;
                        foreach (var i in prdDest.GetImages())
                        {
                            if (i.Alt == "") prdDest.Info.SetXmlProperty("genxml/lang/genxml/imagelist/genxml[" + (lp + 1) + "]/textbox/imagealt", prdSource.GetImage(lp).Alt);
                            lp += 1;
                        }
                        lp = 0;
                        foreach (var l in prdDest.Getlinks())
                        {
                            if (l.Name == "") prdDest.Info.SetXmlProperty("genxml/lang/genxml/linklist/genxml[" + (lp + 1) + "]/textbox/linkname", prdSource.Getlink(lp).Name);
                            lp += 1;
                        }
                        lp = 0;
                        foreach (var o in prdDest.GetOptions())
                        {
                            if (o.Name == "") prdDest.Info.SetXmlProperty("genxml/lang/genxml/optionlist/genxml[" + (lp + 1) + "]/textbox/name", prdSource.GetOption(lp).Name);
                            var lp2 = 1;
                            foreach (var ov in o.GetOptionFields())
                            {
                                var v = "";
                                if (prdSource.GetOption(lp).GetOptionField(ov.Ref) != null) v = prdSource.GetOption(lp).GetOptionField(ov.Ref).Value;
                                if (ov.Value == "") prdDest.Info.SetXmlProperty("genxml/lang/genxml/optionlist/genxml[" + (lp + 1) + "]/optionfields/genxml[" + lp2 + "]/optionsfieldvalue", v);
                                lp2 += 1;
                            }
                            lp += 1;
                        }

                        prdDest.Update();
                    }

                }
                // Categories
                var catList = objCtrl.GetList(_dataObject.PortalShop.PortalId, -1, "CAT", "", sourcelanguage, "", 0, 0, 0, 0, "RocketEcommerceAPI");
                foreach (var c in catList)
                {
                    var catSource = new CategoryLimpet(c.PortalId, c.ItemID, sourcelanguage);
                    var catDest = new CategoryLimpet(c.PortalId, c.ItemID, destlanguage);
                    if (!catDest.Exists || overwritelang)
                    {
                        catSource.Info.Lang = destlanguage;
                        catSource.Update();
                    }
                    else
                    {
                        if (catDest.Name == "") catDest.Name = catSource.Name;
                        if (catDest.Summary == "") catDest.Summary = catSource.Summary;
                        if (catDest.RichText == "") catDest.RichText = catSource.RichText;
                        if (catDest.Keywords == "") catDest.Keywords = catSource.Keywords;
                        catDest.Update();
                    }

                }

            }
            return "OK";
        }
        private String CalculateStats()
        {
            var portalStats = new PortalShopLimpetStats(_dataObject.PortalShop);
            portalStats.RunCalculation();
            return GetDashboard();
        }
        public string PayMethodSort()
        {
            var iList = new List<SimplisityRecord>();
            var l = _postInfo.GetList("tablelist");
            foreach (var p in l)
            {
                var pKey = p.GetXmlProperty("genxml/hidden/interfacekey");
                var i = _dataObject.PortalShop.Record.GetRecordListItem("paymentprovidermethod", "genxml/hidden/paymentmethodkey", pKey);
                if (i != null) iList.Add(i);
            }
            if (iList.Count > 0)
            {
                _dataObject.PortalShop.Record.RemoveRecordList("paymentprovidermethod");
                foreach (var i in iList)
                {
                    _dataObject.PortalShop.Record.AddRecordListItem("paymentprovidermethod", i);
                }
                _dataObject.PortalShop.Update();
            }
            CacheUtils.ClearAllCache();
            return "OK";
        }
        public string ShipMethodSort()
        {
            return "OK";
        }

    }

}
