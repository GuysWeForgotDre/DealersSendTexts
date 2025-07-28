#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Quests;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Messaging;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Quests;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;
using System.Reflection;

#endif
using HarmonyLib;
using MelonLoader;
using UnityEngine.UI;

namespace DealersSendTexts
{
    //Contracts Patches

    [HarmonyPatch(typeof(Contract), nameof(Contract.InitializeContract))]
    public class ContractInitializeContractPatch
    {
        static void Postfix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;
            DealerStats.ProcessContract(__instance, EContractState.Started);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.End))]
    public class ContractEndPatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;
            DealerStats.ProcessContract(__instance, EContractState.Failure);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.Complete))]
    public class ContractCompletePatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null) return;

            DealerStats.Completed.Add(__instance.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown");
            DealerStats.CheckProductAlerts(__instance.Dealer, Util.Prefix(__instance.DeliveryLocation.LocationName));
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

    // MSGConversation Patches

    [HarmonyPatch(typeof(MSGConversation), "RefreshPreviewText")]
    public class MSGConversationRefreshPreviewTextPatch
    {
        static void Postfix(MSGConversation __instance)
        {
            string day = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
            int count  = __instance.bubbles.Count;
#if Il2Cpp
            if (count > 1 && __instance.entryPreviewText.text.StartsWith(day))
                __instance.entryPreviewText.text = __instance.bubbles[count - 2].text;
#elif Mono
            FieldInfo field = AccessTools.Field(typeof(Text), "entryPreviewText");
            string last = "";

            if (field != null) 
                last = field.GetValue(__instance).ToString();

            if (count > 1 && last.StartsWith(day))
                field.SetValue(__instance, __instance.bubbles[count - 2].text);
#endif
        }
    }
}