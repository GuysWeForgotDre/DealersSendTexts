#if   Il2Cpp
using Il2CppScheduleOne.Persistence.Datas;

#elif Mono
using ScheduleOne.Persistence.Datas;

#endif
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DealersSendTexts
{
    [Serializable]
    public class ModSaveData : SaveData
    {
        public const string PATH  = "UserData/DealersSendTexts";
        public const string DATA  = "DealerData.json";
        public const string PREF  = "Config.cfg";

        public Dictionary<string, DealerState> DealerStates = new Dictionary<string, DealerState>();
        public HashSet<string> Completed = new HashSet<string>();
        public List<Location>  Locations = new List<Location>();

        public static void SaveData(string path = null)
        {
            path ??= FullPath();
            
            string dir = Path.GetDirectoryName(path);
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
                File.WriteAllText(path, json);
            }
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to save to {path}: {ex}"); }
        }

        public static void LoadData(string path = null)
        {
            try
            {
                path ??= Path.Combine(PATH, DATA);
                if (!File.Exists(path)) return;

                string json  = File.ReadAllText(path);
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
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to load from {path}: {ex}"); }

            MelonLogger.Msg("Successfully loaded data");
        }

        public static string FullPath(bool config = false) => Path.Combine(PATH, config ? PREF : DATA);
    }
}