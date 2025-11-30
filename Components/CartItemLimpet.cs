using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    public class CartItemLimpet
    {
        public CartItemLimpet(int portalId, SimplisityRecord cartRec, string langaugeRequired)
        {
            CultureCode = langaugeRequired;
            Record = cartRec;
            ProductId = Record.GetXmlPropertyInt("genxml/productid");

            // look for static product, created when order is placed.
            var productNodeString = Record.GetXmlNode("genxml/productdata/item");
            if (productNodeString == "")
            {
                // no static so use normal database version
                ProductData = new ProductLimpet(portalId, ProductId, CultureCode);
            }
            else
            {
                // If there is a static product in this record, use the XML
                ProductData = new ProductLimpet("<item>" + productNodeString + "</item>");
            }

            ModelId = "";
            Qty = 0;
            Key = "";
            Model = new ProductModel(ProductData.PortalId, new SimplisityInfo(), CultureCode);

            PortalShop = new PortalShopLimpet(ProductData.PortalId, CultureCode);

            Validate();
        }

        public ProductModel GetSelectedModel()
        {
            return ProductData.GetModel(ModelId);
        }
        public Dictionary<string, string> GetSelectedOptionsDict()
        {
            var rtn = new Dictionary<string, string>();
            var nodList = Record.XMLDoc.SelectNodes("genxml/options/*");
            if (nodList != null)
            {
                foreach (XmlNode nod in nodList)
                {
                    rtn.Add(nod.Name, nod.InnerText);
                }
            }
            return rtn;
        }
        public List<CartItemOptionLimpet> GetSelectedOptions()
        {
            var rtn = new List<CartItemOptionLimpet>();
            foreach (var o in GetSelectedOptionsDict())
            {
                var productOption = ProductData.GetOption(o.Key);
                var cartItemOption = new CartItemOptionLimpet(productOption, o.Key, o.Value);
                if (productOption != null) rtn.Add(cartItemOption);
            }
            return rtn;
        }
        public CartItemOptionLimpet GetOption(string optionref)
        {
            foreach (var o in GetSelectedOptionsDict())
            {
                var productOption = ProductData.GetOption(o.Key);
                var cartItemOption = new CartItemOptionLimpet(productOption, o.Key, o.Value);
                if (cartItemOption.OptionRef == optionref) return cartItemOption;
            }
            return null;
        }
        public CartItemOptionLimpet GetOption(int idx)
        {
            var lp = 1;
            foreach (var o in GetSelectedOptionsDict())
            {
                if (lp == idx)
                {
                    var productOption = ProductData.GetOption(o.Key);
                    var cartItemOption = new CartItemOptionLimpet(productOption, o.Key, o.Value);
                    return cartItemOption;
                }
                lp += 1;
            }
            return null;
        }
        public void Validate()
        {
            Valid = false;
            if (ProductData.Exists)
            {
                ModelId = Record.GetXmlProperty("genxml/modelid");
                Qty = Record.GetXmlPropertyInt("genxml/qty");
                Key = Record.GetXmlProperty("genxml/key");
                Model = ProductData.GetModel(ModelId);
                PriceCents = Model.BestPriceCents;

                int priceWithOptionsCents = PriceCents;
                foreach (var cartItemOption in GetSelectedOptions())
                {
                    priceWithOptionsCents += cartItemOption.SelectCost;
                }
                PriceWithOptionsCents = priceWithOptionsCents;
                TotalWithOptionsCents = (int)(Qty * PriceWithOptionsCents);
                TotalCents = (int)(Qty * PriceCents);

                Valid = true;
            }
        }
        public ProductLimpet ProductData { set; get; }
        public PortalShopLimpet PortalShop { get; private set; }
        public int ProductId { set; get; }
        public string ModelId { set; get; }
        public int Qty { set; get; }
        public string Key { set; get; }

        public int PriceCents { set; get; }
        public decimal Price
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceCents)); }
        }
        public string PriceDisplay
        {
            get
            {
                return PortalShop.CurrencyDisplay(Price);
            }
        }


        public int PriceWithOptionsCents { set; get; }
        public decimal PriceWithOptions
        {
            get { return (PortalShop.CurrencyCentsToDollars(PriceWithOptionsCents)); }
        }
        public string PriceWithOptionsDisplay
        {
            get
            {
                return PortalShop.CurrencyDisplay(PriceWithOptions);
            }
        }
        public int TotalWithOptionsCents { set; get; }
        public decimal TotalWithOptions
        {
            get { return (PortalShop.CurrencyCentsToDollars(TotalWithOptionsCents)); }
        }
        public string TotalWithOptionsDisplay
        {
            get
            {
                return PortalShop.CurrencyDisplay(TotalWithOptions);
            }
        }
        public int TotalCents { set; get; }
        public string TotalDisplay
        {
            get
            {
                return PortalShop.CurrencyDisplay(PortalShop.CurrencyCentsToDollars(TotalCents));
            }
        }
        public double TotalWeight
        {
            get
            {
                return Weight * Record.GetXmlPropertyDouble("genxml/qty");
            }
        }
        public double Weight
        {
            get
            {
                return Record.GetXmlPropertyDouble("genxml/weight");
            }
        }


        public bool Valid { set; get; }
        public ProductModel Model { set; get; }
        public SimplisityRecord Record { get; private set; }
        public string CultureCode { get; private set; }        
        public bool Exists { get { return ProductData.Exists; } }

    }
}
