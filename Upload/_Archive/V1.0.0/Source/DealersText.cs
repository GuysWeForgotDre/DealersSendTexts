#if Il2Cpp
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Messaging;

#elif Mono
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;

#endif
using MelonLoader;
using System.Collections.Generic;

namespace DealersSendTexts
{
    public class DealerText : MelonMod
    {
    }

    public class DealerPrefs
    {
        public static readonly Dictionary<string, DealerPrefs> DealerSettings = new Dictionary<string, DealerPrefs>();

        public MelonPreferences_Category     DealerGroup    { get; private set; }
        public MelonPreferences_Entry<bool>  ShowStarted    { get; private set; }
        public MelonPreferences_Entry<bool>  ShowCompleted  { get; private set; }
        public MelonPreferences_Entry<bool>  ShowFailed     { get; private set; }
        public MelonPreferences_Entry<bool>  NotifyDeals    { get; private set; }
        public MelonPreferences_Entry<bool>  ShowSummary    { get; private set; }
        public MelonPreferences_Entry<bool>  CheckProduct   { get; private set; }
        public MelonPreferences_Entry<float> ProductAlert   { get; private set; }
        public MelonPreferences_Entry<bool>  CheckCash      { get; private set; }
        public MelonPreferences_Entry<float> CashAlert      { get; private set; }
        public MelonPreferences_Entry<bool>  NotifyAlerts   { get; private set; }

        public static void AddDealer(string name)
        {
            DealerPrefs prefs   = new DealerPrefs { DealerGroup = MelonPreferences.CreateCategory("DealersSendTexts_" + name, name + " Preferences") };
            prefs.DealerGroup.SetFilePath("UserData/DealersSendTexts.cfg");

            prefs.ShowStarted   = prefs.DealerGroup.CreateEntry($"{name}01_ShowStarted",   default_value: true,   "Send text when new deal started");
            prefs.ShowCompleted = prefs.DealerGroup.CreateEntry($"{name}02_ShowCompleted", default_value: true,   "Send text when deal completed");
            prefs.ShowFailed    = prefs.DealerGroup.CreateEntry($"{name}03_ShowFailed",    default_value: true,   "Send text when deal failed / timed out");
            prefs.NotifyDeals   = prefs.DealerGroup.CreateEntry($"{name}04_NotifyDeals",   default_value: false,  "Notify on start / end deals texts");
            prefs.ShowSummary   = prefs.DealerGroup.CreateEntry($"{name}05_ShowSummary",   default_value: true,   "Send daily summary of deals each night");
            prefs.CheckProduct  = prefs.DealerGroup.CreateEntry($"{name}06_CheckProduct",  default_value: true,   "Send text when product count below threshold");
            prefs.ProductAlert  = prefs.DealerGroup.CreateEntry($"{name}07_ProductAlert",  default_value: 20f,    "Product count (total) to trigger alert");
            prefs.CheckCash     = prefs.DealerGroup.CreateEntry($"{name}08_CheckCashLevel", default_value: false, "Send text when cash is above threshold");
            prefs.CashAlert     = prefs.DealerGroup.CreateEntry($"{name}09_CashAlert",     default_value: 50000f, "Minimum cash amount to trigger alert");
            prefs.NotifyAlerts  = prefs.DealerGroup.CreateEntry($"{name}10_NotifyAlerts",  default_value: true,   "Notify when product / cash alert sent");

            prefs.ShowStarted.Comment   = "Send a text message when a new deal is started (default true)";
            prefs.ShowCompleted.Comment = "Send a text message when a deal is successfully completed (default true)";
            prefs.ShowFailed.Comment    = "Send a text message when a deal is failed or timed out (default true)";
            prefs.NotifyDeals.Comment   = "Receive a notification pop up for the above texts (default false)";
            prefs.ShowSummary.Comment   = "Send a summary each night of deals completed / failed and money made (default true)";
            prefs.CheckProduct.Comment  = "Send a text message when product count (any type) dips below a threshold (default true)";
            prefs.ProductAlert.Comment  = "Threshold to send low product alert [item count only, e.g. brick counts as 1] (default 20)";
            prefs.CheckCash.Comment     = "Send a text message when cash on hand is above a threshold (default false)";
            prefs.CashAlert.Comment     = "Amount of cash on hand to send cash alert (default 50,000)";
            prefs.NotifyAlerts.Comment  = "Receive a notification pop up for product / cash alerts (default true)";
            
            DealerSettings[name] = prefs;
        }
    }

    public static class Util
    {
        public static void SendMessage(Dealer dealer, string message, bool notify)
        {
            dealer.MSGConversation.SendMessage(new Message(message, Message.ESenderType.Other), notify, network: false);
        }

        public static int IntTime() => NetworkSingleton<TimeManager>.Instance.CurrentTime;
    }
}