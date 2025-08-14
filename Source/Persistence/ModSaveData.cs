#if   Il2Cpp
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Persistence.Datas;

#elif Mono
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;

#endif
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DealersSendTexts
{
    [Serializable]
    public class ModSaveData : SaveData
    {
        public const string PATH = "UserData/DealersSendTexts";
        public const string DATA = "DealersSendTexts.json";
        public const string PREFS = "Config.cfg";

        public Dictionary<string, DealerState> DealerStates = new Dictionary<string, DealerState>();
        public HashSet<string> Completed = new HashSet<string>();
        public List<Location>  Locations = new List<Location>();

        public static void SaveData(string path)
        {
            string name = GetDataFile(path);
            string dir  = Path.GetDirectoryName(name);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var saveData = new ModSaveData
            {
                DealerStates = DealerManager.Dealers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.State),
                Completed    = ContractManager.Completed,
                Locations    = LocationManager.All(),
            };

            try
            {
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                File.WriteAllText(name, json);
            }
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to save to {name}: {ex}"); }
            MelonLogger.Msg($"[DealersSendTexts] Successfully saved {name}");
            MelonLogger.Msg(Application.version);
        }

        public static void LoadData(string path)
        {
            string name = GetDataFile(path);
            try
            {
                if (!File.Exists(name)) return;

                string json  = File.ReadAllText(name);
                var saveData = JsonConvert.DeserializeObject<ModSaveData>(json);
                ContractManager.Completed = saveData.Completed;

                foreach (Location loc in saveData.Locations)
                    LocationManager.Add(loc);

                foreach (var kvp in saveData.DealerStates)
                    if (DealerManager.Dealers.TryGetValue(kvp.Key, out var stats))
                        stats.State = kvp.Value;
                    else
                        DealerManager.StateCache.Add(kvp.Key, kvp.Value);
            }
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to load from {name}: {ex}"); }
            MelonLogger.Msg($"[DealersSendTexts] Successfully loaded {name}");
        }

        public static string GetConfigFile() => Path.Combine(PATH, PREFS);
        public static string GetDataFile(string path = "") => Path.Combine(path.Length == 0 
            ? PersistentSingleton<SaveManager>.Instance.IndividualSavesContainerPath : path, DATA);
    }
}