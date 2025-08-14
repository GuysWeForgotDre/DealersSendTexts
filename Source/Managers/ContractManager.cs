#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
using ScheduleOne.Quests;

#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DealersSendTexts
{
    public enum EContract { Started, Success, Failure }

    public static class ContractManager
    {
        public static HashSet<string> Completed = new HashSet<string>();

        public static void ProcessContract(Contract contract, EContract state)
        {
            if (contract is null || contract.Dealer is null) return;

            DealerPrefs prefs = DealerPrefs.Prefs(contract.Dealer.FirstName);
            DealerManager stats = DealerManager.GetStats(contract.Dealer);
            SaleData sale     = new SaleData(contract, stats);
            NPC customer      = contract.Customer.GetComponent<Customer>().NPC;
            Vector3 position  = contract.DeliveryLocation.CustomerStandPoint.position;

            stats.AddCustomer(customer.fullName, customer);
            LocationManager.Register(sale.Location, position);
            Location location = LocationManager.GetNearest(position);

            if (state == EContract.Failure && Completed.Contains(sale.Customer))
                state = EContract.Success;

            if (state == EContract.Started)
                ContractStarted(contract, sale, location, prefs);

            if (state == EContract.Success)
                ContractSuccess(contract, sale, location, prefs);

            if (state == EContract.Failure)
                ContractFailure(contract, sale, location, prefs);
        }

        public static void SendSummary()
        {
            var Dealers = DealerManager.Dealers;
            if (Dealers is null || Dealers.Count == 0) return;
            foreach (DealerManager stats in Dealers.Values)
                AlertManager.DailySummary(stats);
        }

        private static void ContractStarted(Contract contract, SaleData sale, Location location, DealerPrefs prefs)
        {
            location.RecordStarted();
            location.RecordCustomer(sale.Customer);
            location.RecordDealer(contract.Dealer.fullName);
            sale.Stats.State.TotalSales.Add(sale);
            sale.Stats.State.SaleCount++;

            string message = MessageManager.Create(EContract.Started, contract, sale);
            if (prefs.GetShowStarted() != EMsg.Disable)
                MessageManager.Send(contract.Dealer, EIcon.Started, message, prefs.GetShowStarted() == EMsg.Notify);
        }

        private static void ContractSuccess(Contract contract, SaleData sale, Location location, DealerPrefs prefs)
        {
            sale.Status = "Success";
            DealerState state = sale.Stats.State;
            Completed.Remove(sale.Customer);
            state.TodaysSales[sale.Customer] = state.RecentSale[sale.Customer] = state.MostRecent = Util.Time();
            state.DailySales.Add(sale);
            location.RecordSuccess();

            string message = MessageManager.Create(EContract.Success, contract, sale);
            if (prefs.GetShowSuccess() != EMsg.Disable)
                MessageManager.Send(contract.Dealer, EIcon.Success, message, prefs.GetShowSuccess() == EMsg.Notify);
            AlertManager.CheckProductAlert(contract.Dealer);
        }

        private static void ContractFailure(Contract contract, SaleData sale, Location location, DealerPrefs prefs)
        {
            sale.Status = "Failure";
            sale.Stats.State.Failures.Add(new FailureKey(sale.Customer, sale.Location));
            location.RecordFailure();

            string message = MessageManager.Create(EContract.Failure, contract, sale);
            if (prefs.GetShowFailure() != EMsg.Disable)
                MessageManager.Send(contract.Dealer, EIcon.Failure, message, prefs.GetShowFailure() == EMsg.Notify);
        }

        public static void ClearAll() => Completed.Clear();
    }
}