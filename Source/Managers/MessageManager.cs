#if   Il2Cpp
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Quests;

#elif Mono
using HarmonyLib;
using System.Reflection;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.Quests;

#endif
using System;
using UnityEngine;

namespace DealersSendTexts
{
    public enum EIcon { None = -1, Started, Success, Failure, Summary, Customer, Location, ProdAlert, HurtAlert, KnockOut, HasDied, Navigate }

    public static class MessageManager
    {
        public static string Create(EContract state, Contract contract, SaleData sale)
        {
            Vector3 delivery = contract.DeliveryLocation.CustomerStandPoint.transform.position;
            Vector3 dealer   = contract.Dealer.transform.position;
            string distance  = Vector3.Distance(delivery, dealer).ToString("#.#");
            int active       = contract.Dealer.ActiveContracts.Count - 1;

            if (state == EContract.Started)
            {
                string location  = Util.Prefix(sale.Location);
                return $"{sale.Customer} at {sale.Started}: {sale.Description} for {sale.Cost}, {location} ({distance} meters), {sale.Window}.";
            }

            if (state == EContract.Success)
            {
                int quick   = TimeManager.AddMinutesTo24HourTime(contract.DeliveryWindow.WindowStartTime, minsToAdd: 60);
                var endTime = new GameDateTime(contract.AcceptTime.elapsedDays, quick);
                float extra = 0f;

                if (NetworkSingleton<TimeManager>.Instance.IsCurrentDateWithinRange(contract.AcceptTime, endTime))
                {
                    extra = sale.Payment * 0.1f;
                    sale.Payment += extra;
                    sale.Cost = MoneyManager.FormatAmount(sale.Payment);
                }

                string length = Util.HoursMins(Util.TimeDiff(sale.StartTime, Util.IntTime()));
                string bonus  = extra > 0 ? $" ({MoneyManager.FormatAmount(extra, showDecimals: false, includeColor: true)} bonus incl.)" : "";

                return $"Completed deal for {sale.Customer} in {length}, made {sale.Cost}{bonus}; {active} active deals";
            }

            if (state == EContract.Failure)
            {
                string failure = "Failed deal: ";
                int threshold  = 5;

                if (sale.Stats.IsDealerHurt(out string status))
                    failure += $"Dealer {status}";

                else if (sale.Stats.IsCustomerHurt(sale.Customer, out status))
                    failure += $"Customer {status}";

                else if (Pathing.IsStuck(sale.Stats.Dealer))
                    failure += $"Stuck {LocationManager.Describe(sale.Stats.Dealer.transform.position, noDistPrefix: "at")}";

                else if (Util.TimeDiff(contract.DeliveryWindow.WindowEndTime, Util.IntTime(), is24hour: false) < threshold)
                    failure += "Ran out of time";

                else if (Math.Abs(Util.IntTime() - 700) < threshold)
                    failure += "Went to bed too early";

                else
                    failure += "Unknown reason";

                return $"{failure} for {sale.Customer} from {distance} meters, lost potential {sale.Cost}; {active} active deals.";
            }
            return "";
        }

        public static void Send(Dealer dealer, EIcon icon, string message, bool notify)
        {
            if (dealer.MSGConversation is null) return;
            Message msg = new Message(GetIcon(icon) + message, Message.ESenderType.Other);
            dealer.MSGConversation.SendMessage(msg, notify, network: false);
        }

        public static void Icons() => Send(Dealer.AllPlayerDealers[0], EIcon.None, $"{GetIcon(EIcon.Started)} Deal Started\n{GetIcon(EIcon.Success)} Deal Success\n" +
            $"{GetIcon(EIcon.Failure)} Deal Failure\n{GetIcon(EIcon.Summary)} Daily Summary\n{GetIcon(EIcon.Customer)} Customer List\n" + 
            $"{GetIcon(EIcon.Location)} Location List\n{GetIcon(EIcon.ProdAlert)} Product Alert\n{GetIcon(EIcon.HurtAlert)} Injury Alert\n" +
            $"{GetIcon(EIcon.KnockOut)} KnockOut Alert\n{GetIcon(EIcon.HasDied)} Death Alert\n{GetIcon(EIcon.Navigate)} Navigation", false);

        private static string GetIcon(EIcon state) => state switch
        {
            EIcon.Started   => " <color=blue><b>▶</b></color> ",
            EIcon.Success   => " <color=green><b>✔</b></color> ",
            EIcon.Failure   => " <color=red><b>✖</b></color> ",
            EIcon.Summary   => " <color=brown><b>☰</b></color> ",
            EIcon.Customer  => " <color=darkblue><b>✪</b></color> ",
            EIcon.Location  => " <color=teal><b>✦</b></color> ",
            EIcon.ProdAlert => " <color=purple><b>⁉</b></color> ",
            EIcon.HurtAlert => " <color=maroon><b>‼</b></color> ",
            EIcon.KnockOut  => " <color=orange><b>⁂</b></color> ",
            EIcon.HasDied   => " <color=red><b>☠</b></color> ",
            EIcon.Navigate  => " <color=blue>∇</color> ",
            _ => "",
        };
    }
}