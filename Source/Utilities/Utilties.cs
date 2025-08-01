#if   Il2Cpp
using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;

#elif Mono
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;

#endif
using System;

namespace DealersSendTexts
{
    public static class Util
    {
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
                || input.ToLower().StartsWith("behind")
                || input.ToLower().StartsWith("in "))
                return input;

            return "at the " + input;
        }

        public static string DayDate() => $"{NetworkSingleton<TimeManager>.Instance.CurrentDay}, Day {NetworkSingleton<TimeManager>.Instance.ElapsedDays}";
        public static int IntTime()    => NetworkSingleton<TimeManager>.Instance.CurrentTime;
        public static int AbsTime()    => NetworkSingleton<TimeManager>.Instance.GetTotalMinSum();
        public static string Time()    => TimeManager.Get12HourTime(IntTime());
        public static string HoursMins(int time) => ((time / 60 > 0) ? time / 60 + "hr " : "") + time % 60 + "min";
    }
}