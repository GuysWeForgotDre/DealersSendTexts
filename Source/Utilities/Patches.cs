#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.NPCs;
using static Il2CppScheduleOne.NPCs.NPCMovement;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Persistence;
using ScheduleOne.NPCs;
using static ScheduleOne.NPCs.NPCMovement;
using ScheduleOne.Quests;

#endif
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using MelonLoader;

namespace DealersSendTexts
{
    //Contracts Patches

    [HarmonyPatch(typeof(Contract), nameof(Contract.InitializeContract))]
    public class ContractInitializeContractPatch
    {
        static void Postfix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null || DealerManager.IsCartel(__instance.Dealer)) return;
            ContractManager.ProcessContract(__instance, EContract.Started);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.End))]
    public class ContractEndPatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null || DealerManager.IsCartel(__instance.Dealer)) return;
            ContractManager.ProcessContract(__instance, EContract.Failure);
        }
    }

    [HarmonyPatch(typeof(Contract), nameof(Contract.Complete))]
    public class ContractCompletePatch
    {
        static void Prefix(Contract __instance)
        {
            if (__instance is null || __instance.Dealer is null || DealerManager.IsCartel(__instance.Dealer)) return;
            ContractManager.Completed.Add(__instance.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown");
        }
    }

    //Dealer Patches

    [HarmonyPatch(typeof(Dealer), "Start")]
    public class DealerStartPatch
    {
        static void Postfix(Dealer __instance)
        {
            if (__instance is null || DealerManager.IsCartel(__instance)) return;
            DealerPrefs.Prefs(__instance.FirstName);
        }
    }

    //LoadManager Patch

    [HarmonyPatch(typeof(LoadManager), nameof(LoadManager.StartGame))]
    public class LoadManagerStartGamePatch
    {
        static void Postfix(SaveInfo info) => ModSaveData.LoadData(info.SavePath);
    }

    //NPC Patches

    [HarmonyPatch(typeof(NPC), "Start")]
    public class NPCStartPatch
    {
        static void Postfix(NPC __instance)
        {
            if (__instance.Health is null) return;

            __instance.Health.onDie       .AddListener((UnityAction)(() => AlertManager.NPCInjuryAlert(__instance, EIcon.HasDied)));
            __instance.Health.onKnockedOut.AddListener((UnityAction)(() => AlertManager.NPCInjuryAlert(__instance, EIcon.KnockOut)));
        }
    }

    //NPCMovement Patches

    [HarmonyPatch(typeof(NPCMovement))]
    public class NPCMovementSetDestinationPatch
    {
        static MethodBase TargetMethod() => AccessTools.GetDeclaredMethods(typeof(NPCMovement)).FirstOrDefault(method 
                => method.Name == "SetDestination" && method.GetParameters().Length == 5);

        static void Postfix(NPCMovement __instance, Vector3 pos, Action<WalkResult> callback, 
            bool interruptExistingCallback, float successThreshold, float cacheMaxDistSqr)
        {
            if (__instance is null) return;
#if Il2Cpp
            if (!(__instance.npc is null) && __instance.npc is Dealer dealer && !DealerManager.IsCartel(dealer))
#elif Mono
            FieldInfo field = AccessTools.Field(typeof(NPCMovement), "npc");
            if (field != null && field.GetValue(__instance) is Dealer dealer && !DealerManager.IsCartel(dealer))
#endif
            {
                EMsg nav = DealerPrefs.Prefs(dealer.FirstName).GetNavigation();
                if (nav != EMsg.Disable && DealerManager.CheckPing(dealer))
                {
                    DealerManager.SetPing(dealer, Util.AbsTime());
                    float distance  = Vector3.Distance(dealer.transform.position, pos);
                    string location = LocationManager.Describe(pos, noDistPrefix: "to");
                    string message  = $"Headed {location}, {distance:#.#} meters away.";
                    MessageManager.Send(dealer, EIcon.Navigate, message, nav == EMsg.Notify);
                }
            }
        }
    }

    //Player Patches

    [HarmonyPatch(typeof(Player), "SleepStart")]
    public class PlayerSleepStartPatch
    {
        static void Prefix() => ContractManager.SendSummary();
    }

    //SaveManager Patches

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save), new[] { typeof(string) } )]
    public class SaveManagerSavePatch
    {
        static void Prefix(string saveFolderPath) => ModSaveData.SaveData(saveFolderPath);
    }

    //TimeManager Patches

    [HarmonyPatch(typeof(TimeManager), "Update")]
    public class TimeManagerUpdatePatch
    {
        [HarmonyPostfix]
        public static void PostFix()
        {
            int time = Util.AbsTime() % 20;
            if (time == 0 && DealerManager.CheckStuck)
            {
                DealerManager.CheckStuck = false;
                foreach (DealerManager stats in DealerManager.Dealers.Values)
                    Pathing.CheckStuck(stats);
            }

            if (time == 1) 
                DealerManager.CheckStuck = true;
        }
    }
}