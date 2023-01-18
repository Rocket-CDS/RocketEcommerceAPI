using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RocketEcommerceAPI.Components
{
    /// <summary>
    /// The Cart Data class, this is inherited by CartLimpet and OrderLimpet
    /// </summary>
    public class CartDataClass
    {
        public CartDataClass()
        {
        }
        public string FirstName { get { return Record.GetXmlProperty("genxml/textbox/firstname"); } set { Record.SetXmlProperty("genxml/textbox/firstname", value); } }
        public string LastName { get { return Record.GetXmlProperty("genxml/textbox/lastname"); } set { Record.SetXmlProperty("genxml/textbox/lastname", value); } }
        public string Email { get { return Record.GetXmlProperty("genxml/textbox/email"); } set { Record.SetXmlProperty("genxml/textbox/email", value); } }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string Phone { get { return Record.GetXmlProperty("genxml/textbox/phone"); } set { Record.SetXmlProperty("genxml/textbox/phone", value); } }
        public string Notes { get { return Record.GetXmlProperty("genxml/textbox/notes"); } set { Record.SetXmlProperty("genxml/textbox/notes", value); } }
        public string ClientNotes { get { return Record.GetXmlProperty("genxml/textbox/clientnotes"); } set { Record.SetXmlProperty("genxml/textbox/clientnotes", value); } }
        public DateTime CollectionDateTime { get { return Record.GetXmlPropertyDate("genxml/textbox/collectionselecteddate"); } set { Record.SetXmlProperty("genxml/textbox/collectionselecteddate", value.ToString("O"), TypeCode.DateTime); } }

        public string BillFirstName { get { return Record.GetXmlProperty("genxml/billaddress/billfirstname"); } set { Record.SetXmlProperty("genxml/billaddress/billfirstname", value); } }
        public string BillLastName { get { return Record.GetXmlProperty("genxml/billaddress/billlastname"); } set { Record.SetXmlProperty("genxml/billaddress/billlastname", value); } }
        public string BillEmail { get { return Record.GetXmlProperty("genxml/billaddress/billemail"); } set { Record.SetXmlProperty("genxml/billaddress/billemail", value); } }
        public string BillFullName { get { return BillFirstName + " " + BillLastName; } }
        public string BillFullName2 { get { return BillLastName + " " + BillFirstName; } }
        public string BillPhone { get { return Record.GetXmlProperty("genxml/billaddress/billphone"); } set { Record.SetXmlProperty("genxml/billaddress/billphone", value); } }
        public string BillCompany { get { return Record.GetXmlProperty("genxml/billaddress/billcompany"); } set { Record.SetXmlProperty("genxml/billaddress/billcompany", value); } }
        public string BillUnit { get { return Record.GetXmlProperty("genxml/billaddress/billunit"); } set { Record.SetXmlProperty("genxml/billaddress/billunit", value); } }
        public string BillAddress1 { get { return Record.GetXmlProperty("genxml/billaddress/billaddress1"); } set { Record.SetXmlProperty("genxml/billaddress/billaddress1", value); } }
        public string BillAddress2 { get { return Record.GetXmlProperty("genxml/billaddress/billaddress2"); } set { Record.SetXmlProperty("genxml/billaddress/billaddress2", value); } }
        public string BillCity { get { return Record.GetXmlProperty("genxml/billaddress/billcity"); } set { Record.SetXmlProperty("genxml/billaddress/billcity", value); } }
        public string BillPostCode { get { return Record.GetXmlProperty("genxml/billaddress/billpostcode"); } set { Record.SetXmlProperty("genxml/billaddress/billpostcode", value); } }
        public string BillCountry { get { return Record.GetXmlProperty("genxml/billaddress/billcountry"); } set { Record.SetXmlProperty("genxml/billaddress/billcountry", value); } }
        public string BillRegion { get { return Record.GetXmlProperty("genxml/billaddress/billregion"); } set { Record.SetXmlProperty("genxml/billaddress/billregion", value); } }
        public string BillAddressHtml
        {
            get
            {
                var rtn = "";
                if (BillFullName != "") rtn += BillFullName + "<br/>";
                if (BillCompany != "" || BillUnit != "") rtn += BillCompany + "&nbsp;" + BillUnit + "<br/>";
                if (BillAddress1 != "") rtn += BillAddress1 + "<br/>";
                if (BillAddress2 != "") rtn += BillAddress2 + "<br/>";
                if (BillCity != "") rtn += BillCity + "<br/>";
                var countryName = DNNrocketUtils.GetCountryName(BillCountry, PortalId);
                var regionName = DNNrocketUtils.GetRegionName(BillRegion);
                if (countryName != "") rtn += countryName + "<br/>";
                if (regionName != "" || BillPostCode != "") rtn += regionName + "&nbsp;" + BillPostCode + "<br/>";
                if (BillPhone != "" || BillEmail != "") rtn += "<br/>";
                if (BillPhone != "") rtn += LocalUtils.ResourceKey("RE.tel") + ": " + BillPhone + "<br/>";
                if (BillEmail != "") rtn += LocalUtils.ResourceKey("RE.email") + ": " + BillEmail + "<br/>";
                return rtn;
            }
        }

        public string ShipFirstName { get { return Record.GetXmlProperty("genxml/shipaddress/shipfirstname"); } set { Record.SetXmlProperty("genxml/shipaddress/shipfirstname", value); } }
        public string ShipLastName { get { return Record.GetXmlProperty("genxml/shipaddress/shiplastname"); } set { Record.SetXmlProperty("genxml/shipaddress/shiplastname", value); } }
        public string ShipEmail { get { return Record.GetXmlProperty("genxml/shipaddress/shipemail"); } set { Record.SetXmlProperty("genxml/shipaddress/shipemail", value); } }
        public string ShipFullName { get { return ShipFirstName + " " + ShipLastName; } }
        public string ShipFullName2 { get { return ShipLastName + " " + ShipFirstName; } }
        public string ShipPhone { get { return Record.GetXmlProperty("genxml/shipaddress/shipphone"); } set { Record.SetXmlProperty("genxml/shipaddress/shipphone", value); } }
        public string ShipCompany { get { return Record.GetXmlProperty("genxml/shipaddress/shipcompany"); } set { Record.SetXmlProperty("genxml/shipaddress/shipcompany", value); } }
        public string ShipUnit { get { return Record.GetXmlProperty("genxml/shipaddress/shipunit"); } set { Record.SetXmlProperty("genxml/shipaddress/shipunit", value); } }
        public string ShipAddress1 { get { return Record.GetXmlProperty("genxml/shipaddress/shipaddress1"); } set { Record.SetXmlProperty("genxml/shipaddress/shipaddress1", value); } }
        public string ShipAddress2 { get { return Record.GetXmlProperty("genxml/shipaddress/shipaddress2"); } set { Record.SetXmlProperty("genxml/shipaddress/shipaddress2", value); } }
        public string ShipCity { get { return Record.GetXmlProperty("genxml/shipaddress/shipcity"); } set { Record.SetXmlProperty("genxml/shipaddress/shipcity", value); } }
        public string ShipPostCode { get { return Record.GetXmlProperty("genxml/shipaddress/shippostcode"); } set { Record.SetXmlProperty("genxml/shipaddress/shippostcode", value); } }
        public string ShipCountry { get { return Record.GetXmlProperty("genxml/shipaddress/shipcountry"); } set { Record.SetXmlProperty("genxml/shipaddress/shipcountry", value); } }
        public string ShipRegion { get { return Record.GetXmlProperty("genxml/shipaddress/shipregion"); } set { Record.SetXmlProperty("genxml/shipaddress/shipregion", value); } }
        public bool CollectInStore { get { return Record.GetXmlPropertyBool("genxml/checkbox/collectinstore"); } set { Record.SetXmlProperty("genxml/checkbox/collectinstore", value.ToString()); } }
        public bool ShipUseBillAddress { get {
                if (Record.GetXmlProperty("genxml/shipaddress/usebillingaddress") == "")
                {
                    // first loop in cart
                    return true;
                }
                else
                {
                    return Record.GetXmlPropertyBool("genxml/shipaddress/usebillingaddress");
                }
            } 
            set { Record.SetXmlProperty("genxml/shipaddress/usebillingaddress", value.ToString()); } }
        public string ShipAddressHtml
        {
            get
            {
                var rtn = "";
                if (ShipFullName != "") rtn += ShipFullName + "<br/>";
                if (ShipCompany != "" || ShipUnit != "") rtn += ShipCompany + "&nbsp;" + ShipUnit + "<br/>";
                if (ShipAddress1 != "") rtn += ShipAddress1 + "<br/>";
                if (ShipAddress2 != "") rtn += ShipAddress2 + "<br/>";
                if (ShipCity != "") rtn += ShipCity + "<br/>";
                var countryName = DNNrocketUtils.GetCountryName(ShipCountry, PortalId);
                var regionName = DNNrocketUtils.GetRegionName(ShipRegion);
                if (countryName != "") rtn += countryName + "<br/>";
                if (regionName != "" || ShipPostCode != "") rtn += regionName + "&nbsp;" + ShipPostCode + "<br/>";
                if (ShipPhone != "" || ShipEmail != "") rtn += "<br/>";
                if (ShipPhone != "") rtn += LocalUtils.ResourceKey("RE.tel") + ": " + ShipPhone + "<br/>";
                if (ShipEmail != "") rtn += LocalUtils.ResourceKey("RE.email") + ": " + ShipEmail + " <br/>";
                return rtn;
            }
        }



        public int PortalId { get { return Record.PortalId; } set { Record.PortalId = value; } }
        public string CultureCode { get; set; }
        public SimplisityRecord Record { get; set; }


    }
}
