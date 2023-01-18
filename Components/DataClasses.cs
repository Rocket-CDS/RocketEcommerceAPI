using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RocketEcommerceAPI.Components
{
    public enum OrderStatus
    {
        Incomplete = 20,
        Cancelled = 40,
        Paid = 200,
        PaymentNotVerified = 210,
        WaitingForPayment = 70,
        WaitingForStock = 80,
        Waiting = 130,
        BeginManufactured = 90,
        Shipped = 100,
        Completed = 110,
        Archived = 120,
        Special = 140,
    }

    public enum PaymentStatus
    {
        Incomplete = 20,
        WaitingForBank = 30,
        WaitingForPayment = 220,
        PaymentOK = 50,
        PaymentNotVerified = 60,
        PaymentFailed = 150,
        Archived = 120,
        Special = 140,
        Partial = 160,
        PartialNotVerified = 170,
        Overpaid = 180,
        OverpaidNotVerified = 190
    }

    public enum PaymentAction
    {
        None = 0,
        BankPost = 10,
        BankReturn = 20
    }
    public enum ProductDataSection
    {
        Models = 0,
        Images = 1,
        Options = 2,
        Categories = 3,
        Properties = 4,
        Docs = 5,
        Links= 6,
        Html = 7,
        Extra = 8,
        SEO = 9
    }
}
