#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;

#endif
using HarmonyLib;

namespace DealersSendTexts
{
    //Contracts Patches

    [HarmonyPatch(typeof(Contract), nameof(Contract.InitializeContract))]
    public class ContractInitializeContractPatch
    {
        static void Postfix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;
            DealerStats.ProcessContract(__instance, EContractState.Accepted);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.End))]
    public class ContractEndPatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;
            DealerStats.ProcessContract(__instance, EContractState.Failed);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.Complete))]
    public class ContractCompletePatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;

            DealerStats.CompletedDeals.Add(__instance.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown");
            DealerStats.CheckProductAlerts(__instance.Dealer, __instance.DeliveryLocation.LocationName);
        }
    }

    //Player Patches

    [HarmonyPatch(typeof(Player), "SleepStart")]
    public class PlayerSleepStartPatch
    {
        static void Prefix()
        {
            foreach (DealerStats stats in DealerStats.Dealers.Values)
            {
                stats.SendSummary();
                stats.Sales.Clear();
            }
        }
    }

    //Dealer Patches

    [HarmonyPatch(typeof(Dealer), "Start")]
    public class DealerStartPatch
    {
        static void Postfix(Dealer __instance)
        {
            //if (__instance.IsRecruited)
            DealerPrefs.AddDealer(__instance.FirstName);
        }
    }
}