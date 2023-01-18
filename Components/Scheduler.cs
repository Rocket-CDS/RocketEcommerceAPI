using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;

namespace RocketEcommerceAPI.Components
{
    public class Scheduler : SchedulerInterface
    {
        /// <summary>
        /// This is called by DNNrocketAPI.Components.RocketScheduler
        /// It is called in a loop on portals, and then looped on systemData.
        /// </summary>
        /// <param name="systemData"></param>
        /// <param name="rocketInterface"></param>
        public override void DoWork()
        {
            var portalList = PortalUtils.GetPortals();
            foreach (var portalId in portalList)
            {
                var deletecount = 0;
                var portalShop = new PortalShopLimpet(portalId, DNNrocketUtils.GetCurrentCulture());

                if (portalShop.Active && (portalShop.SchedulerRunHours == 0 || (portalShop.LastSchedulerTime < DateTime.Now.AddHours(portalShop.SchedulerRunHours * -1))))
                {
                    var cartList = new CartLimpetList(new SimplisityInfo(), portalShop, DNNrocketUtils.GetCurrentCulture(), false);
                    // Limit the amout of days carts can exist.
                    var searchFilter = " and R1.ModifiedDate < CONVERT(DATETIME, '" + DateTime.Now.AddDays((portalShop.CartDays * -1)).ToString("yyyy-MM-dd") + "')";
                    cartList.Populate(searchFilter);
                    var cl = cartList.GetCartList();
                    foreach (var cartData in cl)
                    {
                        cartData.Delete();
                        deletecount += 1;
                    }
                    // Cart Limt to stop to many carts existsing for a portal
                    var cartList2 = new CartLimpetList(new SimplisityInfo(), portalShop, DNNrocketUtils.GetCurrentCulture(), true);
                    var cl2 = cartList2.GetCartList();
                    if (cl2.Count > portalShop.CartLimit)
                    {
                        var lp = 1;
                        foreach (var cartData in cl2)
                        {
                            if (lp > portalShop.CartLimit && cartData.Record.ModifiedDate < DateTime.Now.AddHours(-1))
                            {
                                cartData.Delete();
                                deletecount += 1;
                            }
                            lp += 1;
                        }
                    }
                    if (deletecount > 0) LogUtils.LogSystem("Scheduler - PortalId:" + portalId + " Deleted Carts:" + deletecount);

                    var portalStats = new PortalShopLimpetStats(portalShop);
                    portalStats.RunCalculation();

                    //[TODO:  Remove any payments that have been created by clients, but not processed.]

                    //[TODO: remove "UserParams" records for users that don't exist or over a time limit.  OPTIONAL]



                    portalShop.LastSchedulerTime = DateTime.Now;
                    portalShop.Update();
                }
                else
                {
                    if (portalShop.DebugMode) LogUtils.LogSystem("Scheduler not run, LastSchedulerTime: " + portalShop.LastSchedulerTime.ToString("O") + " CurrentTime: " + DateTime.Now.ToString("O"));
                }
            }
        }
    }
}