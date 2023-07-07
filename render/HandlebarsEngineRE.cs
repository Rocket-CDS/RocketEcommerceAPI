using DNNrocketAPI.Components;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketEcommerceAPI.Components
{
    public class HandlebarsEngineRE : HandlebarsEngine
    {
        public string ExecuteRE(string source, object model)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                RegisterREHelpers(hbs);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                throw new TemplateException("Failed to render Handlebar template : " + ex.Message, ex, model, source);
            }
        }

        public static void RegisterREHelpers(IHandlebars hbs)
        {
            RegisterProduct(hbs);
            RegisterProductTest(hbs);
            RegisterImageShow(hbs);
            RegisterImage(hbs);
            RegisterDocumentShow(hbs);
            RegisterDocument(hbs);
            RegisterLinkShow(hbs);
            RegisterLink(hbs);
            RegisterEngineUrl(hbs);
            RegisterCultureCodeEdit(hbs);
            RegisterCultureCode(hbs);
            RegisterIsInCategory(hbs);
            RegisterHasProperty(hbs);
            RegisterListUtil(hbs);
            RegisterModuleRef(hbs);
        }
        private static ProductLimpet GetProductData(JObject o)
        {
            var articleid = (string)o.SelectToken("genxml.data.genxml.column.itemid") ?? "";
            var portalid = (string)o.SelectToken("genxml.data.genxml.column.portalid") ?? "-1";
            var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
            if (!GeneralUtils.IsNumeric(articleid)) return new ProductLimpet();

            var cacheKey = portalid + "*" + articleid + "*" + cultureCode;
            var articleData = (ProductLimpet)CacheUtils.GetCache(cacheKey, "article");
            if (articleData == null)
            {
                articleData = new ProductLimpet(PortalUtils.GetPortalId(), Convert.ToInt32(articleid), cultureCode);
                CacheUtils.SetCache(cacheKey, articleData, "article");
            }
            return articleData;
        }
        private static PortalShopLimpet GetPortalShop(JObject o)
        {
            var portalid = (string)o.SelectToken("genxml.data.genxml.column.portalid") ?? "-1";
            var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";

            var cacheKey = portalid + "*portalshop*" + cultureCode;
            var portalShop = (PortalShopLimpet)CacheUtils.GetCache(cacheKey, "portal" + portalid);
            if (portalShop == null)
            {
                portalShop = new PortalShopLimpet(Convert.ToInt32(portalid), cultureCode);
                CacheUtils.SetCache(cacheKey, portalShop, "portal" + portalid);
            }
            return portalShop;
        }

        private static void RegisterEngineUrl(IHandlebars hbs)
        {
            hbs.RegisterHelper("engineurl", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCode(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecode", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterCultureCodeEdit(IHandlebars hbs)
        {
            hbs.RegisterHelper("culturecodeedit", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.culturecodeedit") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterListUtil(IHandlebars hbs)
        {
            hbs.RegisterHelper("listurl", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.pageurl") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterModuleRef(IHandlebars hbs)
        {
            hbs.RegisterHelper("moduleref", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.sessionparams.r.moduleref") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        // Get Image Data
        private static void RegisterImageShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("imagetest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetProductData(o);

                var imgidx = 0;
                if (arguments.Length >= 3) imgidx = Convert.ToInt32(arguments[2]);
                var img = articleData.GetImage(imgidx);
                var cmd = arguments[1].ToString();

                if (cmd == "isshown")
                {
                    if (!img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (img.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasheading")
                {
                    if (img.Alt != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasimage")
                {
                    if (img.RelPath != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "haslink")
                {
                    if (img.UrlText != "")
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hassummary")
                {
                    if (!String.IsNullOrWhiteSpace(img.Summary))
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }


            });
        }
        private static void RegisterImage(IHandlebars hbs)
        {
            hbs.RegisterHelper("image", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetProductData(o);

                    var imgidx = 0;
                    if (arguments.Length >= 3) imgidx = Convert.ToInt32(arguments[2]);
                    var img = articleData.GetImage(imgidx);
                    var cmd = arguments[1].ToString();

                    switch (cmd)
                    {
                        case "alt":
                            dataValue = img.Alt;
                            break;
                        case "relpath":
                            dataValue = img.RelPath;
                            break;
                        case "height":
                            dataValue = img.Height.ToString();
                            break;
                        case "width":
                            dataValue = img.Width.ToString();
                            break;
                        case "count":
                            dataValue = articleData.GetImageList().Count.ToString();
                            break;
                        case "summary":
                            dataValue = img.Summary;
                            break;
                        case "url":
                            dataValue = img.Url;
                            break;
                        case "urltext":
                            dataValue = img.UrlText;
                            break;
                        case "hidden":
                            dataValue = img.Hidden.ToString();
                            break;
                        case "thumburl":
                            var width = Convert.ToInt32(arguments[3]);
                            var height = Convert.ToInt32(arguments[4]);
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + img.RelPath + "&w=" + width + "&h=" + height;
                            break;
                        case "fieldid":
                            dataValue = img.FieldId;
                            break;
                        default:
                            dataValue = img.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = img.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;

                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterDocumentShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("doctest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetProductData(o);

                var cmd = arguments[1].ToString();
                var idx = 0;
                if (arguments.Length >= 3) idx = (int)arguments[2];
                var doc = articleData.GetDoc(idx);

                if (cmd == "isshown")
                {
                    if (!doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (doc.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterDocument(IHandlebars hbs)
        {
            hbs.RegisterHelper("document", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetProductData(o);

                    var cmd = arguments[1].ToString();
                    var idx = 0;
                    if (arguments.Length >= 3) idx = (int)arguments[2];
                    var doc = articleData.GetDoc(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = doc.DocKey;
                            break;
                        case "name":
                            dataValue = doc.Name;
                            break;
                        case "hidden":
                            dataValue = doc.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = doc.FieldId;
                            break;
                        case "relpath":
                            dataValue = doc.RelPath;
                            break;
                        case "count":
                            dataValue = articleData.GetDocList().Count.ToString();
                            break;
                        case "url":
                            var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + doc.RelPath;
                            break;
                        default:
                            dataValue = doc.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = doc.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }
                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterLinkShow(IHandlebars hbs)
        {
            hbs.RegisterHelper("linktest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetProductData(o);
                var cmd = arguments[1].ToString();
                var idx = 0;
                if (arguments.Length >= 3) idx = (int)arguments[2];
                var lnk = articleData.Getlink(idx);

                if (cmd == "isshown")
                {
                    if (!lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (lnk.Hidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }
        private static void RegisterLink(IHandlebars hbs)
        {
            hbs.RegisterHelper("link", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var articleData = GetProductData(o);
                    var cmd = arguments[1].ToString();
                    var idx = 0;
                    if (arguments.Length >= 3) idx = (int)arguments[2];
                    var lnk = articleData.Getlink(idx);

                    switch (cmd)
                    {
                        case "key":
                            dataValue = lnk.LinkKey;
                            break;
                        case "name":
                            dataValue = lnk.Name;
                            break;
                        case "hidden":
                            dataValue = lnk.Hidden.ToString().ToLower();
                            break;
                        case "fieldid":
                            dataValue = lnk.FieldId;
                            break;
                        case "count":
                            dataValue = articleData.GetLinkList().Count.ToString();
                            break;
                        case "ref":
                            dataValue = lnk.Ref;
                            break;
                        case "type":
                            dataValue = lnk.LinkType.ToString();
                            break;
                        case "target":
                            dataValue = lnk.Target;
                            break;
                        case "anchor":
                            dataValue = lnk.Anchor;
                            break;
                        case "url":
                            dataValue = lnk.Url;
                            break;
                        default:
                            dataValue = lnk.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = lnk.Info.GetXmlProperty("genxml/lang/" + cmd);
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterProductTest(IHandlebars hbs)
        {
            hbs.RegisterHelper("producttest", (writer, options, context, arguments) =>
            {
                var o = (JObject)arguments[0];
                var articleData = GetProductData(o);
                var cmd = arguments[1].ToString();

                if (cmd == "haslinks")
                {
                    if (articleData.GetLinkList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasdocs")
                {
                    if (articleData.GetDocList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasimages")
                {
                    if (articleData.GetImageList().Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "haslist")
                {
                    var listname = arguments[2].ToString();
                    if (articleData.Info.GetList(listname).Count > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "ishidden")
                {
                    if (!articleData.IsHidden)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasmodels")
                {
                    if (articleData.GetModelList().Count > 1)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hasoptions")
                {
                    if (articleData.GetOptionList().Count > 1)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }
                if (cmd == "hassaleprice")
                {
                    if (articleData.SalePriceMinimum > 0)
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
                }

            });
        }

        private static void RegisterProduct(IHandlebars hbs)
        {
            hbs.RegisterHelper("product", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    var cmd = arguments[1].ToString();
                    var productData = GetProductData(o);
                    var portalShop = GetPortalShop(o);
                    var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode");

                    switch (cmd)
                    {
                        case "productid":
                            dataValue = productData.ProductId.ToString();
                            break;
                        case "ref":
                            dataValue = productData.Ref;
                            break;
                        case "name":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.lang.genxml.textbox.productname");
                            break;
                        case "bestprice":
                            dataValue = productData.BestPriceDisplay();
                            break;
                        case "priceminimum":
                            dataValue = productData.PriceMinimumDisplay();
                            break;
                        case "pricemaximum":
                            dataValue = productData.PriceMaximumDisplay();
                            break;
                        case "salepriceminimum":
                            dataValue = productData.SalePriceMinimumDisplay();
                            break;
                        case "salepricemaximum":
                            dataValue = productData.SalePriceMaximumDisplay();
                            break;
                        case "summary":
                            dataValue = BreakOf(productData.Summary);
                            break;
                        case "html":
                            dataValue = productData.RichText;
                            break;
                        case "detailurl":
                            var url = (string)o.SelectToken("genxml.remotemodule.genxml.remote.detailpageurl" + cultureCode);
                            if (String.IsNullOrEmpty(url)) url = (string)o.SelectToken("genxml.sessionparams.r.pagedetailurl");
                            if (String.IsNullOrEmpty(url)) url = (string)o.SelectToken("genxml.sessionparams.r.pageurl");
                            url = url.TrimEnd('/') + productData.PortalShop.ProductDetailURL;
                            url = url.Replace("{productid}", productData.ProductId.ToString());
                            url = url.Replace("{productname}", GeneralUtils.UrlFriendly(productData.Name));
                            // Remove to stop duplicate content
                            //url = url.Replace("{page}", (string)o.SelectToken("genxml.sessionparams.r.page"));
                            //url = url.Replace("{pagesize}", (string)o.SelectToken("genxml.sessionparams.r.pagesize"));
                            //url = url.Replace("{catid}", (string)o.SelectToken("genxml.sessionparams.r.catid")); // use int so we always get a value (i.e. "0")

                            url = LocalUtils.TokenReplacementCultureCode(url, (string)o.SelectToken("genxml.sessionparams.r.culturecode"));
                            dataValue = url;
                            break;
                        case "modeldropdown":
                            var r = new RocketEcommerceAPITokens<SimplisityRazor >();
                            dataValue = r.ModelDropdown(productData, "modelid").ToString();
                            break;
                        case "optionsinput":
                            var r2 = new RocketEcommerceAPITokens<SimplisityRazor>();
                            dataValue = r2.OptionsInput(productData).ToString();
                            break;
                        default:
                            var displaytype = "";
                            if (arguments.Length >= 3) displaytype = arguments[2].ToString();
                            var displayculturecode = productData.CultureCode;
                            if (arguments.Length >= 4) displayculturecode = arguments[3].ToString();

                            dataValue = productData.Info.GetXmlProperty(cmd);
                            if (dataValue == "") dataValue = productData.Info.GetXmlProperty("genxml/lang/" + cmd);
                            if (displaytype == "breakof") dataValue = BreakOf(dataValue);
                            if (displaytype == "money") dataValue = CurrencyUtils.CurrencyDisplay(dataValue, displayculturecode);
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }
        private static void RegisterIsInCategory(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("IsInCategory", (writer, options, context, parameters) =>
            {
                try
                {
                    if (parameters != null && parameters.Length == 2)
                    {
                        var o = (JObject)parameters[0];
                        var articleData = GetProductData(o);
                        string categoryRef = parameters[1].ToString();
                        if (articleData.Exists)
                        {
                            if (articleData.IsInCategory(categoryRef))
                                options.Template(writer, (object)context);
                            else
                                options.Inverse(writer, (object)context);
                        }
                        else
                        {
                            options.Inverse(writer, (object)context);
                        }
                    }
                    else
                    {
                        writer.WriteSafeString("INCORRECT ARGS: {{#IsInCategoryById this ArticleId CategoryId }}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString(ex.ToString());
                }
            });
        }
        private static void RegisterHasProperty(HandlebarsDotNet.IHandlebars hbs)
        {
            hbs.RegisterHelper("HasProperty", (writer, options, context, parameters) =>
            {
                try
                {
                    if (parameters != null && parameters.Length == 2)
                    {
                        var o = (JObject)parameters[0];
                        var articleData = GetProductData(o);
                        string propRef = parameters[1].ToString();
                        if (articleData.Exists)
                        {
                            if (articleData.HasProperty(propRef))
                                options.Template(writer, (object)context);
                            else
                                options.Inverse(writer, (object)context);
                        }
                        else
                        {
                            options.Inverse(writer, (object)context);
                        }
                    }
                    else
                    {
                        writer.WriteSafeString("INCORRECT ARGS: {{#IsInCategoryById this ProductId CategoryId }}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString(ex.ToString());
                }
            });
        }

        private static string BreakOf(String strIn)
        {
            var strOut = System.Web.HttpUtility.HtmlEncode(strIn);
            strOut = strOut.Replace(Environment.NewLine, "<br/>");
            strOut = strOut.Replace("\n", "<br/>");
            strOut = strOut.Replace("\t", "&nbsp;&nbsp;&nbsp;");
            strOut = strOut.Replace("'", "&apos;");
            return strOut;
        }

    }
}
