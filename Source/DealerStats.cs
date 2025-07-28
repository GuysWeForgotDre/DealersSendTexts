#if   Il2Cpp
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.PlayerScripts;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.Product;
using ScheduleOne.Quests;

#endif
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using System;

namespace DealersSendTexts
{
    public enum EContractState { Started, Success, Failure }

    public class DealerStats
    {
        public static readonly Dictionary<string, DealerStats> Dealers = new Dictionary<string, DealerStats>();
        public static readonly HashSet<string> Completed = new HashSet<string>();

        public static readonly string   IconStarted  = " <color=blue><b>▶</b></color> ",
                                        IconSuccess  = " <color=green><b>✔</b></color> ",
                                        IconFailure  = " <color=red><b>✖</b></color> ",
                                        IconSummary  = " <color=purple><b>☰</b></color> ",
                                        IconCustomer = " <color=darkblue><b>✪</b></color> ",
                                        IconLocation = " <color=teal><b>✦</b></color> ",
                                        IconAlert    = " <color=orange><b>⁉</b></color> ";

        public Dictionary<string, string> Failures;
        public Dictionary<string, int>  LastSale;
        public Dictionary<string, int>  Products;
        public List<DealerSale>         Sales;
        public Dealer                   Dealer;

        public DealerStats(Dealer dealer) 
        {
            Dealer   = dealer;
            Sales    = new List<DealerSale>();
            LastSale = new Dictionary<string, int>();
            Failures = new Dictionary<string, string>();

            foreach (Customer cust in dealer.AssignedCustomers)
                LastSale[cust.NPC.fullName] = -1;

            Dealers.Add(dealer.fullName, this);
        }

        public static DealerStats Get(Dealer dealer)
        {
            if (!Dealers.TryGetValue(dealer.fullName, out DealerStats stats))
                stats = new DealerStats(dealer);
            return stats;
        }

        public static void ProcessContract(Contract contract, EContractState state)
        {
            if (contract is null || contract.Dealer is null) return;

            Dealer dealer     = contract.Dealer;
            DealerStats stats = Get(dealer);
            DealerSale sale   = new DealerSale(contract, stats);
            DealerPrefs prefs = DealerPrefs.Get(contract.Dealer.FirstName);

            bool showStarted  = prefs.ShowStarted.Value;
            bool showSuccess  = prefs.ShowSuccess.Value;
            bool showFailure  = prefs.ShowFailure.Value;
            bool notifyDeals  = prefs.NotifyDeals.Value;

            if (state == EContractState.Failure && Completed.TryGetValue(sale.Customer, out _))
                state = EContractState.Success;

            if (state == EContractState.Started)
                stats.ContractStarted(contract, sale, showStarted, notifyDeals);

            if (state == EContractState.Success)
                stats.ContractSuccess(contract, sale, showSuccess, notifyDeals);

            if (state == EContractState.Failure)
                stats.ContractFailure(contract, sale, showFailure, notifyDeals);
        }

        public void SendSummary()
        {
            var products  = new Dictionary<string, int>();
            var locations = new HashSet<string>();
            float payment = 0;

            DealerPrefs prefs  = DealerPrefs.Get(Dealer.FirstName);
            bool showSummary   = prefs.ShowSummary.Value;
            bool sendCustomers = prefs.SendCustomers.Value;
            bool sendLocations = prefs.SendLocations.Value;
            bool sendFailures  = prefs.SendFailures.Value;


            foreach (DealerSale sale in Sales)
            {
                payment += sale.Payment;

                if (!products.TryGetValue(sale.Product, out int amount))
                    products.Add(sale.Product, sale.Quantity);
                else
                    products[sale.Product] += sale.Quantity;

                locations.Add(sale.Location);
            }

            if (showSummary)
            {
                int count  = Sales.Count;
                string pay = MoneyManager.FormatAmount(payment, showDecimals: false, includeColor: true), sold = "";
                foreach (string product in products.Keys)
                    sold += $"\n{Util.GetName(product)} ({products[product]})";

                string summary = $"{IconSummary}DAILY SUMMARY\nDid {count} deals in {locations.Count} locations; {Failures.Count} failed.\nMade {pay} from:{sold}";
                Util.SendMessage(Dealer, summary, notify: false);
            }

            if (sendCustomers)
            {
                string customers = $"{IconCustomer}CUSTOMER LOG";
                foreach (string customer in LastSale.Keys)
                {
                    string last = LastSale[customer] == -1 ? "Never" : Util.HoursMins(Util.AbsTime() - LastSale[customer]);
                    customers += $"\n{customer}: {last}";
                }

                Util.SendMessage(Dealer, customers, notify: false);
            }

            if (sendLocations)
            {
                string locMsg = $"{IconLocation}LOCATIONS";
                foreach (string location in locations)
                    locMsg += "\n" + location;

                Util.SendMessage(Dealer, locMsg, notify: false);
            }

            if (sendFailures && Failures.Count > 0)
            {
                string failures  = $"{IconFailure}FAILURES ({Failures.Count})";
                foreach (var kvp in Failures)
                    failures += $"\n{kvp.Key} {Util.Prefix(kvp.Value)}";

                Util.SendMessage(Dealer, failures, notify: false);
            }

            Failures.Clear();
        }

        public static void CheckProductAlerts(Dealer dealer, string location)
        {
            DealerStats stats = Get(dealer);
            stats.RefreshInventory();

            DealerPrefs prefs  = DealerPrefs.Get(dealer.FirstName);
            bool checkProduct  = prefs.CheckProduct.Value;
            bool checkCash     = prefs.CheckCash.Value;
            bool notify        = prefs.NotifyAlerts.Value;
            float productAlert = prefs.ProductAlert.Value;
            float cashAlert    = prefs.CashAlert.Value;

            float count = 0;
            foreach (string product in stats.Products.Keys) 
                count += stats.Products[product];

            string cash = MoneyManager.FormatAmount(dealer.Cash);
            string dist = Vector3.Distance(Player.Local.PlayerBasePosition, dealer.transform.position).ToString("#.#");

            if (checkCash && dealer.Cash >= cashAlert)
                Util.SendMessage(dealer, $"{IconAlert}Cash threshold reached: {cash}.\n{count} products left; I'm {dist} meters from you {location}.", notify);

            if (checkProduct && count <= productAlert)
                Util.SendMessage(dealer, $"{IconAlert}Product threshold reached: {count}.\n{cash} cash; I'm {dist} meters from you {location}.", notify);
        }

        private void ContractStarted(Contract contract, DealerSale sale, bool send, bool notify)
        {
            if (!send) return;

            Dealer dealer  = contract.Dealer;
            string dist    = Vector3.Distance(contract.DeliveryLocation.CustomerStandPoint.transform.position, dealer.transform.position).ToString("#.#");
            string message = $"{IconStarted}{sale.Customer} at {sale.Started}: {sale.Description} for {sale.Cost}, {Util.Prefix(sale.Location)} ({dist} meters), {sale.Window}.";            
            Util.SendMessage(dealer, message, notify);
        }

        private void ContractSuccess(Contract contract, DealerSale sale, bool send, bool notify)
        {
            float extra = 0f;
            int quick   = TimeManager.AddMinutesTo24HourTime(contract.DeliveryWindow.WindowStartTime, minsToAdd: 60);
            var endTime = new GameDateTime(contract.AcceptTime.elapsedDays, quick);

            if (NetworkSingleton<TimeManager>.Instance.IsCurrentDateWithinRange(contract.AcceptTime, endTime))
            {
                extra = sale.Payment * 0.1f;
                sale.Payment += extra;
                sale.Cost = MoneyManager.FormatAmount(sale.Payment);
            }

            Completed.Remove(sale.Customer);
            LastSale[sale.Customer] = Util.AbsTime();
            Sales.Add(sale);

            if (send)
            {
                int active     = contract.Dealer.ActiveContracts.Count - 1;
                string length  = Util.HoursMins(Util.TimeDiff(sale.StartTime, Util.IntTime()));
                string bonus   = extra > 0 ? $" ({MoneyManager.FormatAmount(extra)} bonus incl.)" : "";
                string message = $"{IconSuccess}Completed deal for {sale.Customer} in {length}, made {sale.Cost}{bonus}; {active} active deals.";

                Util.SendMessage(contract.Dealer, message, notify);
            }
        }

        private void ContractFailure(Contract contract, DealerSale sale, bool send, bool notify)
        {
            Failures.Add(sale.Customer, sale.Location);

            if (send)
            {
                Dealer dealer  = contract.Dealer;
                int active     = dealer.ActiveContracts.Count - 1;
                string dist    = Vector3.Distance(contract.DeliveryLocation.CustomerStandPoint.transform.position, dealer.transform.position).ToString("#.#");
                string failure = "Failed deal: ";
                failure += (Math.Abs(Util.IntTime() - 700) < 5) ? "Went to bed too early" : "Ran out of time";
                string message = $"{IconFailure}{failure} for {sale.Customer} from {dist} meters, lost potential {sale.Cost}; {active} active deals.";

                Util.SendMessage(dealer, message, notify);
            }
        }

        private void RefreshInventory()
        {
            var slots = Dealer.GetAllSlots();
            if (slots is null || slots.Count == 0) return;

            var products = new Dictionary<string, int>();
            var slotList = new List<string>();
            foreach (ItemSlot slot in slots)
            {
                if (slot.Quantity != 0)                
                {                    
                    int count = slot.Quantity;
#if Il2Cpp
                    var instance = slot.ItemInstance.TryCast<ProductItemInstance>();
                    if (instance != null)
#elif Mono
                    if (slot.ItemInstance is ProductItemInstance instance)
#endif
                        count *= instance.Amount;

                    if (slotList.Contains(slot.ItemInstance.ID))
                    {
                        products[slot.ItemInstance.ID] += count;
                        continue;
                    }

                    slotList.Add(slot.ItemInstance.ID);
                    products.Add(slot.ItemInstance.ID, count);
                }
            }
            Products = products;
        }
    }

    public class DealerSale
    {
        public string Product;
        public string Description;
        public string Customer;
        public string Location;
        public string Window;
        public string Started;
        public string Cost;
        public float  Payment;
        public int    Quantity;
        public int    StartTime;

        public DealerSale(Contract contract, DealerStats stats)
        {
            Product     = contract.ProductList.entries[0].ProductID;
            Description = contract.ProductList.GetCommaSeperatedString();
            Customer    = contract.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown";
            Location    = contract.DeliveryLocation.LocationName;
            Window      = DealWindowInfo.GetWindow(contract.DeliveryWindow.WindowStartTime).ToString();
            Started     = TimeManager.Get12HourTime(contract.AcceptTime.time);
            Cost        = MoneyManager.FormatAmount(contract.Payment * (1 - stats.Dealer.Cut));
            Payment     = contract.Payment * (1 - stats.Dealer.Cut);
            Quantity    = contract.ProductList.GetTotalQuantity();
            StartTime   = contract.AcceptTime.time;
        }
    }
}