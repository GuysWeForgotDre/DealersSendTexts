#if Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.PlayerScripts;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Quests;

#endif
using System.Collections.Generic;
using UnityEngine;

namespace DealersSendTexts
{
    public enum EContractState { Accepted, Completed, Failed }

    public class DealerStats
    {
        public static readonly Dictionary<string, DealerStats>  Dealers = new Dictionary<string, DealerStats>();
        public static readonly HashSet<string>                  CompletedDeals = new HashSet<string>();

        public List<DealerSale> Sales;
        public Dealer           Dealer;
        public string           Name;
        public int              Failed;

        public DealerStats(string name, Dealer dealer) 
        {
            Name   = name;
            Dealer = dealer;
            Sales  = new List<DealerSale>();
        }

        public static void ProcessContract(Contract contract, EContractState state)
        {
            if (contract is null || contract.Dealer is null) return;

            string name     = contract.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown";
            string product  = contract.ProductList.GetCommaSeperatedString();
            string location = contract.DeliveryLocation.LocationName;
            string cost     = MoneyManager.FormatAmount(contract.Payment);
            string accepted = TimeManager.Get12HourTime(contract.AcceptTime.time);
            string window   = DealWindowInfo.GetWindow(contract.DeliveryWindow.WindowStartTime).ToString();
            string distance = Vector3.Distance(contract.DeliveryLocation.CustomerStandPoint.transform.position, contract.Dealer.transform.position).ToString("#.#");
            int num         = contract.Dealer.ActiveContracts.Count - 1;

            if (state == EContractState.Failed && CompletedDeals.TryGetValue(name, out _))
                state = EContractState.Completed;

            CompletedDeals.Remove(name);

            bool sendAccept = true, sendComplete = true, sendFailed = true, notify = false;
            if (DealerPrefs.DealerSettings.TryGetValue(contract.Dealer.FirstName, out var prefs) && !(prefs is null))
            {
                sendAccept   = prefs.ShowStarted.Value;
                sendComplete = prefs.ShowCompleted.Value;
                sendFailed   = prefs.ShowFailed.Value;
                notify       = prefs.NotifyDeals.Value;
            }

            if (state == EContractState.Accepted && sendAccept)
            {
                string icon = "<color=blue><b>▶</b></color>";
                Util.SendMessage(contract.Dealer, $"{icon}{name} at {accepted}: {product} for {cost}, {location} ({distance} meters), {window}.", notify);
            }

            if (state == EContractState.Completed && sendComplete)
            {
                int start = TimeManager.GetMinSumFrom24HourTime(contract.AcceptTime.time);
                int now   = TimeManager.GetMinSumFrom24HourTime(Util.IntTime());
                if (start > now) now += 1440;
                int len   = now - start;
                int hour  = len / 60;
                int min   = len % 60;

                string length = ((hour > 0) ? hour + "hr " : "") + min + "min";
                string icon   = $"<color=green><b>✔</b></color>";

                Util.SendMessage(contract.Dealer, $"{icon}Completed deal for {name} in {length}, made {cost}; {num} active deals.", notify);
                AddContractToDealer(contract);
            }

            if (state == EContractState.Failed && sendFailed)
            {
                Dealer dealer = contract.Dealer;
                if (Dealers.TryGetValue(dealer.fullName, out DealerStats stats))
                    stats.Failed++;
                else
                    Dealers.Add(dealer.fullName, new DealerStats(dealer.fullName, dealer));

                string icon = "<color=red><b>✖</b></color>";
                string fail = (Util.IntTime() >= contract.DeliveryWindow.WindowEndTime) ? "Timed out" : "Failed";

                Util.SendMessage(contract.Dealer, $"{icon}{fail} deal for {name} from {distance} meters, lost potential {cost}; {num} active deals.", notify);
            }
        }

        public static void AddContractToDealer(Contract contract)
        {
            if (contract is null || contract.Dealer is null) return;

            Dealer dealer = contract.Dealer;
            if (!Dealers.TryGetValue(dealer.fullName, out DealerStats stats))
            {
                stats = new DealerStats(dealer.fullName, dealer);
                Dealers.Add(dealer.fullName, stats);
            }

            DealerSale sale = new DealerSale()
            {
                Dealer      = stats,
                ProductName = contract.ProductList.GetCommaSeperatedString(),
                Location    = contract.DeliveryLocation.LocationName,
                Customer    = contract.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown",
                Window      = DealWindowInfo.GetWindow(contract.DeliveryWindow.WindowStartTime).ToString(),
                Payment     = contract.Payment,
                Quantity    = contract.ProductList.GetTotalQuantity(),
                AcceptTime  = contract.AcceptTime.time,
            };

            stats.Sales.Add(sale);
        }

        public void SendSummary()
        {
            Dictionary<string, int> products = new Dictionary<string, int>();
            HashSet<string> locations = new HashSet<string>();

            int count = Sales.Count;
            float payment = 0;

            foreach (DealerSale sale in Sales)
            {
                payment += sale.Payment;

                if (!products.TryGetValue(sale.ProductName, out int amount))
                    products.Add(sale.ProductName, sale.Quantity);
                else
                    products[sale.ProductName] += sale.Quantity;

                locations.Add(sale.Location);
            }

            int loc     = locations.Count;
            string sold = "";
            string pay  = MoneyManager.FormatAmount(payment, showDecimals: false, includeColor: true);

            foreach (string product in products.Keys)
                sold += $"\n{product} ({products[product]})";

            if (DealerPrefs.DealerSettings.TryGetValue(Dealer.FirstName, out var prefs) && !(prefs is null) && prefs.ShowSummary.Value)
            {
                string icon = "<color=purple><b>☰</b></color>";
                string message = $"{icon}DAILY SUMMARY\nDid {count} deals in {loc} locations; {Failed} failed.\nMade {pay} from:{sold}";
                Util.SendMessage(Dealer, message, false);
            }

            Failed = 0;
        }

        public static void CheckProductAlerts(Dealer dealer, string location)
        {
            bool checkProduct = true, checkCash = false, notify = true;
            float productAlert = 20, cashAlert = 50000;
            
            if (DealerPrefs.DealerSettings.TryGetValue(dealer.FirstName,out var prefs) && !(prefs is null))
            {
                checkProduct = prefs.CheckProduct.Value;
                checkCash    = prefs.CheckCash.Value;
                notify       = prefs.NotifyAlerts.Value;
                productAlert = prefs.ProductAlert.Value;
                cashAlert    = prefs.CashAlert.Value;
            }

            float product = dealer.GetTotalInventoryItemCount();
            string cash   = MoneyManager.FormatAmount(dealer.Cash);
            string dist   = Vector3.Distance(Player.Local.PlayerBasePosition, dealer.transform.position).ToString("#.#");
            string icon = "<color=orange><b>⁉</b></color>";            

            if (checkCash && dealer.Cash >= cashAlert)
                Util.SendMessage(dealer, $"{icon}Cash threshold reached: {cash}.\n{product} products left, I'm {dist} meters from you at {location}.", notify);

            if (checkProduct && product <= productAlert)
                Util.SendMessage(dealer, $"{icon}Products threshold reached: {product}.\n{cash} cash, I'm {dist} meters from you at {location}.", notify);
        }
    }

    public class DealerSale
    {
        public DealerStats  Dealer;
        public string       ProductName;
        public string       Location;
        public string       Customer;
        public string       Window;
        public float        Payment;
        public int          Quantity;
        public int          AcceptTime;
    }
}