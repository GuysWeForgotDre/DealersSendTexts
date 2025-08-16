#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.PlayerScripts;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;

#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DealersSendTexts
{
    public static class AlertManager
    {
        public static void DailySummary(DealerManager stats)
        {
            if (!stats.Dealer.RelationData.Unlocked) return;
            DealerPrefs prefs = DealerPrefs.Prefs(stats.Dealer.FirstName);
            DealerState state = stats.State;
            var products      = new Dictionary<string, int>();
            var locations     = new HashSet<string>();
            float payment     = 0;

            foreach (SaleData sale in state.DailySales)
            {
                payment += sale.Payment;
                if (!products.ContainsKey(sale.Product))
                    products.Add(sale.Product, sale.Quantity);
                else
                    products[sale.Product] += sale.Quantity;
                locations.Add(sale.Location);
            }

            if (prefs.GetShowSummary())
            {
                string pay  = MoneyManager.FormatAmount(payment, showDecimals: false, includeColor: true);
                string sold = "";

                foreach (string product in products.Keys)
                    sold += $"\n{Util.GetName(product)} ({products[product]})";

                string summary = $"DAILY SUMMARY\nDid {state.DailySales.Count} deals in {locations.Count} locations; {state.Failures.Count} failed.\nMade {pay} from:{sold}";
                if (stats.IsDealerHurt(out string status))
                    summary += $"\nDealer {status}";

                MessageManager.Send(stats.Dealer, EIcon.Summary, summary, notify: false);
            }

            if (prefs.GetSendCustomers())
            {
                string customers = $"CUSTOMER LOG ({state.TodaysSales.Count})";
                foreach (string customer in state.TodaysSales.Keys)
                {
                    stats.IsCustomerHurt(customer, out string status);
                    customers += $"\n{customer}: {state.TodaysSales[customer]} {status}";
                }
                MessageManager.Send(stats.Dealer, EIcon.Customer, customers, notify: false);
            }

            if (prefs.GetSendLocations())
            {
                string locMsg = $"LOCATIONS ({locations.Count})";
                foreach (string location in locations)
                    locMsg += "\n" + location;
                MessageManager.Send(stats.Dealer, EIcon.Location, locMsg, notify: false);
            }

            if (prefs.GetSendFailures() && state.Failures.Count > 0)
            {
                string failures  = $"FAILURES ({state.Failures.Count})";
                foreach (FailureKey failure in state.Failures)
                    failures += $"\n{failure.Customer} {Util.Prefix(failure.Location)}";
                MessageManager.Send(stats.Dealer, EIcon.Failure, failures, notify: false);
            }

            state.ClearAll(daily: true);
        }

        public static void NPCInjuryAlert(NPC npc, EIcon type)
        {
            string location = LocationManager.Describe(npc.transform.position, noDistPrefix: "at");
            string distance = Vector3.Distance(Player.Local.PlayerBasePosition, npc.transform.position).ToString("#.#");
            string ailment  = type == EIcon.KnockOut ? "been knocked out" : type == EIcon.HasDied ? "died" : "an unknown alert";

            if (npc is Dealer dealer)
            {
                EMsg injury    = DealerPrefs.Prefs(dealer.FirstName).GetDealerInjury();
                string message = $"I have {ailment} in {npc.Region} {location}, {distance} meters from you.";

                if (injury != EMsg.Disable)
                    MessageManager.Send(dealer, type, message, injury == EMsg.Notify);
                return;
            }

            foreach (DealerManager stats in DealerManager.Dealers.Values)
                foreach (string customer in stats.Customers.Keys)
                    if (customer == npc.fullName)
                    {
                        EMsg injury = DealerPrefs.Prefs(stats.Dealer.FirstName).GetCustomerInjury();
                        if (!stats.State.RecentSale.TryGetValue(customer, out string previous)) 
                            previous = "Never";

                        string message = $"{npc.fullName} has {ailment} in {npc.Region} {location}, {distance} meters from you. Last sale was {previous}.";
                        if (injury != EMsg.Disable)
                            MessageManager.Send(stats.Dealer, type, message, injury == EMsg.Notify);
                    }
        }

        public static void CheckProductAlert(Dealer dealer)
        {
            DealerPrefs prefs   = DealerPrefs.Prefs(dealer.FirstName);
            DealerManager stats = DealerManager.GetStats(dealer);
            stats.RefreshInventory();

            string cash     = MoneyManager.FormatAmount(dealer.Cash);
            string distance = Vector3.Distance(Player.Local.PlayerBasePosition, dealer.transform.position).ToString("#.#");
            string location = LocationManager.Describe(dealer.transform.position, noDistPrefix: "at");
            float count     = 0;

            foreach (string product in stats.State.Products.Keys) 
                count += stats.State.Products[product];

            if (prefs.GetCheckCash() != EMsg.Disable && dealer.Cash >= prefs.GetCashAlert())
                MessageManager.Send(dealer, EIcon.ProdAlert, 
                    $"Cash threshold reached: {cash}.\n{count} products left; I'm {distance} meters from you, {location}.", prefs.GetCheckCash() == EMsg.Notify);

            if (prefs.GetCheckProduct() != EMsg.Disable && count <= prefs.GetProductAlert())
                MessageManager.Send(dealer, EIcon.ProdAlert, 
                    $"Product threshold reached: {count}.\n{cash} cash; I'm {distance} meters from you, {location}.", prefs.GetCheckProduct() == EMsg.Notify);
        }
    }
}