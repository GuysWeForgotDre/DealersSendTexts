#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Product;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Product;

#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DealersSendTexts
{
    public class DealerManager
    {
        public static Dictionary<string, DealerManager> Dealers  = new Dictionary<string, DealerManager>();
        public static Dictionary<string, DealerState> StateCache = new Dictionary<string, DealerState>();
        public static bool CheckStuck;

        public Dictionary<string, NPC> Customers = new Dictionary<string, NPC>();
        public Dealer      Dealer;
        public DealerState State;
        public Vector3     Home;
        private int        LastPing = -1;

        public DealerManager(Dealer dealer)
        {
            Dealer = dealer;
            State  = StateCache.TryGetValue(dealer.fullName, out DealerState state) ? state : new DealerState();

            Home = dealer.Home.GetClosestDoor(dealer.transform?.position ?? Vector3.zero, useableOnly: true).transform.position;
            LocationManager.Register(dealer.HomeName, Home);

            foreach (Customer customer in dealer.AssignedCustomers)
            {
                string name = customer.NPC.fullName;
                if (!State.TodaysSales.ContainsKey(name))
                    State.TodaysSales[name] = "Never";
                if (!State.RecentSale.ContainsKey(name))
                    State.RecentSale[name] = "Never";
                
                AddCustomer(name, customer.NPC);
            }
        }

        public static DealerManager GetStats(Dealer dealer)
        {
            if (!Dealers.TryGetValue(dealer.fullName, out DealerManager stats))
            {
                stats = new DealerManager(dealer);
                Dealers.Add(dealer.fullName, stats);
            }
            return stats;
        }

        public void RefreshInventory()
        {
            var slots = Dealer.GetAllSlots();
            if (slots is null || slots.Count == 0) return;

            var slotList = new List<string>();
            var products = new Dictionary<string, int>();
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
            State.Products = products;
        }

        public void AddCustomer(string fullName, NPC npc)
        {
            if (!Customers.ContainsKey(fullName)) Customers.Add(fullName, npc);
        }

        public bool IsCustomerHurt(string name, out string status)
        {
            status = "";
            if (!Customers.TryGetValue(name, out NPC customer)) return false;

            if (customer.Health.IsDead)       status = $"<color=red>(Dead {customer.Health.DaysPassedSinceDeath} days)</color>";
            if (customer.Health.IsKnockedOut) status = $"<color=orange>(Knocked Out)</color>";

            return !string.IsNullOrEmpty(status);
        }

        public bool IsDealerHurt(out string status)
        {
            status = "";
            if (Dealer.Health.IsDead)       status = $"<color=red>Dead ({Dealer.Health.DaysPassedSinceDeath} days)</color>";
            if (Dealer.Health.IsKnockedOut) status = $"<color=orange>(Knocked Out)</color>";

            return !string.IsNullOrEmpty(status);
        }

        public static bool IsCartel(Dealer dealer)
        {
            if (Application.version.CompareTo("0.4.0") < 0) return false;
            Type cartelDealer = Type.GetType("CartelDealer");
            if (cartelDealer is null) return false;

            return cartelDealer.IsInstanceOfType(dealer);
        }

        public static bool CheckPing(Dealer dealer)
        {
            int navigation = DealerPrefs.Prefs(dealer.FirstName).GetINavDeltaTime();
            return navigation > -1 && Util.AbsTime() - GetStats(dealer).LastPing > navigation;
        }

        public static void SetPing(Dealer dealer, int ping) => GetStats(dealer).LastPing = ping;

        public static void ClearAll()
        {
            foreach (DealerManager stats in Dealers.Values)
            {
                stats.Customers.Clear();
                stats.State.ClearAll();
            }

            Dealers.Clear();
            StateCache.Clear();
        }

        public Vector3 DistanceToHome() => Dealer.Home.GetClosestDoor(Dealer.transform.position, useableOnly: true).AccessPoint.position;
    }
}