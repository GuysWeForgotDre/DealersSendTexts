#if   Il2Cpp
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Messaging;

#elif Mono
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Messaging;

#endif
using MelonLoader;
using UnityEngine;
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
        public MelonPreferences_Entry<bool>  ShowSuccess    { get; private set; }
        public MelonPreferences_Entry<bool>  ShowFailure    { get; private set; }
        public MelonPreferences_Entry<bool>  NotifyDeals    { get; private set; }
        public MelonPreferences_Entry<bool>  ShowSummary    { get; private set; }
        public MelonPreferences_Entry<bool>  SendCustomers  { get; private set; }
        public MelonPreferences_Entry<bool>  SendLocations  { get; private set; }
        public MelonPreferences_Entry<bool>  SendFailures   { get; private set; }
        public MelonPreferences_Entry<bool>  CheckProduct   { get; private set; }
        public MelonPreferences_Entry<float> ProductAlert   { get; private set; }
        public MelonPreferences_Entry<bool>  CheckCash      { get; private set; }
        public MelonPreferences_Entry<float> CashAlert      { get; private set; }
        public MelonPreferences_Entry<bool>  NotifyAlerts   { get; private set; }

        public static void AddDealer(string name)
        {
            DealerPrefs prefs   = new DealerPrefs { DealerGroup = MelonPreferences.CreateCategory("DealersSendTexts_" + name, name + " Preferences") };
            prefs.DealerGroup.SetFilePath("UserData/DealersSendTexts.cfg");

            prefs.ShowStarted   = prefs.DealerGroup.CreateEntry($"{name}01_ShowStarted",   default_value: true,   "Text: When a new deal is started");
            prefs.ShowSuccess   = prefs.DealerGroup.CreateEntry($"{name}02_ShowSucCess",   default_value: true,   "Text: When deal successfully completed");
            prefs.ShowFailure   = prefs.DealerGroup.CreateEntry($"{name}03_ShowFailure",   default_value: true,   "Text: When deal failed / timed out");
            prefs.NotifyDeals   = prefs.DealerGroup.CreateEntry($"{name}04_NotifyDeals",   default_value: false,  "Notification: From start / end deals texts");
            prefs.ShowSummary   = prefs.DealerGroup.CreateEntry($"{name}05_ShowSummary",   default_value: true,   "Daily: Summary of deals completed that day");
            prefs.SendCustomers = prefs.DealerGroup.CreateEntry($"{name}06_SendCustomers", default_value: false,  "Daily: Time since last deal, per customer");
            prefs.SendLocations = prefs.DealerGroup.CreateEntry($"{name}07_SendLocations", default_value: false,  "Daily: List of locations visited");
            prefs.SendFailures  = prefs.DealerGroup.CreateEntry($"{name}08_SendFailures",  default_value: true,   "Daily: List of failed customers / locations");
            prefs.CheckProduct  = prefs.DealerGroup.CreateEntry($"{name}09_CheckProduct",  default_value: true,   "Alert: When product count below threshold");
            prefs.ProductAlert  = prefs.DealerGroup.CreateEntry($"{name}10_ProductAlert",  default_value: 20f,    "Alert: Product count (total) to trigger alert");
            prefs.CheckCash     = prefs.DealerGroup.CreateEntry($"{name}11_CheckCashLevel", default_value: false, "Alert: When cash on hand is above threshold");
            prefs.CashAlert     = prefs.DealerGroup.CreateEntry($"{name}12_CashAlert",     default_value: 50000f, "Alert: Minimum cash amount to trigger alert");
            prefs.NotifyAlerts  = prefs.DealerGroup.CreateEntry($"{name}13_NotifyAlerts",  default_value: true,   "Notificatin: When product / cash alert sent");

            prefs.ShowStarted.Comment   = "Send a text message when a new deal is started (default true)";
            prefs.ShowSuccess.Comment   = "Send a text message when a deal is successfully completed (default true)";
            prefs.ShowFailure.Comment   = "Send a text message when a deal is failed or timed out (default true)";
            prefs.NotifyDeals.Comment   = "Receive a notification pop up for the above texts (default false)";
            prefs.ShowSummary.Comment   = "Send a summary each night of deals completed / failed and money made (default true)";
            prefs.SendCustomers.Comment = "Send a summary each night time since deal with each customer (default false)";
            prefs.SendLocations.Comment = "Send a summary each night of locations visited that day for successful deals (default false)";
            prefs.SendFailures.Comment  = "Send a summary each night of failed customers / locations (default true)";
            prefs.CheckProduct.Comment  = "Send a text message when product count (any type) dips below a threshold (default true)";
            prefs.ProductAlert.Comment  = "Threshold to send low product alert (total count of product, e.g. brick = 20) (default 20)";
            prefs.CheckCash.Comment     = "Send a text message when cash on hand is above a threshold (default false)";
            prefs.CashAlert.Comment     = "Amount of cash on hand to send cash alert (default 50,000)";
            prefs.NotifyAlerts.Comment  = "Receive a notification pop up for product / cash alerts (default true)";
            
            DealerSettings[name] = prefs;
        }

        public static DealerPrefs Get(string name)
        {
            if (!DealerSettings.TryGetValue(name, out var prefs))
            {
                AddDealer(name);
                prefs = DealerSettings[name];
            }
            return prefs;
        }
    }

    public static class Util
    {
        public static void SendMessage(Dealer dealer, string message, bool notify)
        {
            dealer.MSGConversation.SendMessage(new Message(message, Message.ESenderType.Other), notify, network: false);
        }

        public static int TimeDiff(int start, int end, bool is24hour = true)
        {
            start = is24hour ? TimeManager.GetMinSumFrom24HourTime(start) : start;
            end   = is24hour ? TimeManager.GetMinSumFrom24HourTime(end)   : end;

            int diff = end - start;
            if (diff < 0) diff += 1440;

            return diff;
        }

        public static string GetName(string input)
        {
            string output = Registry.GetItem(input)?.Name;

            if (string.IsNullOrEmpty(output)) 
                return input;
            return output;
        }

        public static string Prefix(string input)
        {
            if (input.ToLower().StartsWith("outside") 
                || input.ToLower().StartsWith("next") 
                || input.ToLower().StartsWith("under") 
                || input.ToLower().StartsWith("behind"))
                return input;

            return "at the " + input;
        }

        public static int IntTime() => NetworkSingleton<TimeManager>.Instance.CurrentTime;
        public static int AbsTime() => NetworkSingleton<TimeManager>.Instance.GetTotalMinSum();
        public static int DawnToday() => NetworkSingleton<TimeManager>.Instance.ElapsedDays * 1440;
        public static string HoursMins(int time) => ((time / 60 > 0) ? time / 60 + "hr " : "") + time % 60 + "min";
    }
}