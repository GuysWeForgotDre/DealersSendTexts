#if   Il2Cpp
using Il2CppScheduleOne.Economy;

#elif Mono
using ScheduleOne.Economy;

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DealersSendTexts
{
    public static class Pathing
    {
        private const int MINTIME = 5;
        private const int MAXHISTORY = 10;
        private static readonly Dictionary<string, Queue<(Vector3, int)>> PositionHistory = new Dictionary<string, Queue<(Vector3, int)>>();

        public static void RecordPosition(Dealer dealer)
        {
            if (dealer is null || dealer.ActiveContracts.Count == 0) return;
            string name = dealer.fullName;
            int current = Util.AbsTime();

            if (!PositionHistory.TryGetValue(name, out var history))
                PositionHistory[name] = history = new Queue<(Vector3, int)>();

            if (history.Count == 0 || Math.Abs(history.Peek().Item2 - current) >= MINTIME)
                history.Enqueue((dealer.transform.position, current));

            if (history.Count > MAXHISTORY)
                history.Dequeue();
        }

        public static bool IsStuck(Dealer dealer)
        {
            if (dealer is null || !PositionHistory.TryGetValue(dealer.fullName, out var history))
                return false;

            DealerPrefs prefs = DealerPrefs.Prefs(dealer.FirstName);
            int stuck = prefs.GetIsStuckCount();
            if (history.Count < stuck) return false;

            (Vector3, int)[] recent = history.Reverse().Take(stuck).ToArray();
            Vector3 anchor = recent[0].Item1;

            foreach (var (position, _) in recent)
                if (Vector3.Distance(position, anchor) > prefs.GetIsStuckRadius())
                    return false;

            return true;
        }

        public static void CheckStuck(DealerManager stats)
        {
            Dealer dealer = stats.Dealer;
            RecordPosition(dealer);
            if (!IsStuck(dealer) || stats.IsDealerHurt(out _) || dealer.ActiveContracts.Count == 0) return;

            bool notify      = DealerPrefs.Prefs(dealer.FirstName).GetIsStuckAlert() == EMsg.Notify;
            Vector3 position = dealer.transform.position;
            string location  = LocationManager.Describe(position, noDistPrefix: "at");

            string message   = $"I may be stuck in {dealer.Region} {location}. Most recent deal was {stats.State.MostRecent}.";
            MessageManager.Send(dealer, EIcon.HurtAlert, message, notify);
        }

        public static void ClearAll() => PositionHistory.Clear();
    }
}