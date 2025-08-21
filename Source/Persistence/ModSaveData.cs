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
        public const string PATHBASE = "UserData\\DealersSendTexts";
        public const string DATAFILE = "Data.json";
        public const string PREFFILE = "Config.cfg";

        private static string _activeSaveDir;
        private static string _activeSavePath;

        public static string DataPath  => Path.Combine(_activeSavePath ?? PATHBASE, DATAFILE);
        public static string PrefsPath => Path.Combine(_activeSavePath ?? PATHBASE, PREFFILE);

        public Dictionary<string, DealerState> DealerStates = new Dictionary<string, DealerState>();
        public HashSet<string> Completed = new HashSet<string>();
        public List<Location>  Locations = new List<Location>();

        public static void SaveData(string pathFromSaver)
        {
            SetActiveSave(pathFromSaver);

            MelonLogger.Msg($"Attempting to save {DataPath}");
            if (!Directory.Exists(Path.GetDirectoryName(DataPath)))
                Directory.CreateDirectory(DataPath);

            var saveData = new ModSaveData
            {
                DealerStates = DealerManager.Dealers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.State),
                Completed    = ContractManager.Completed,
                Locations    = LocationManager.All(),
            };

            MelonLogger.Msg($"Serializing:");
            try
            {
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                File.WriteAllText(DataPath, json);
            }
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to save to {DataPath}: {ex}"); }
            MelonLogger.Msg($"[DealersSendTexts] Successfully saved {DataPath}");
            MelonLogger.Msg(Application.version);
        }

        public static void LoadData(string pathFromLoader)
        {
            SetActiveSave(pathFromLoader);
            if (!File.Exists(DataPath))
            {
                DealerText.ClearAll();
                return;
            }

            try
            {
                MelonLogger.Msg($"Loading {DataPath}:");
                string json  = File.ReadAllText(DataPath);

                ModSaveData saveData = JsonConvert.DeserializeObject<ModSaveData>(json);
                ContractManager.Completed = saveData.Completed;

                foreach (Location loc in saveData.Locations)
                    LocationManager.Add(loc);

                foreach (var kvp in saveData.DealerStates)
                    if (DealerManager.Dealers.TryGetValue(kvp.Key, out var stats))
                        stats.State = kvp.Value;
                    else
                        DealerManager.StateCache.Add(kvp.Key, kvp.Value);
            }
            catch (Exception ex) { MelonLogger.Error($"[DealersSendTexts] Failed to load from {DataPath}: {ex}"); }
            MelonLogger.Msg($"[DealersSendTexts] Successfully loaded {DataPath}");
        }

        private static void SetActiveSave(string gamePath)
        {
            string path = gamePath;
            if (string.IsNullOrEmpty(path))
                path = PersistentSingleton<SaveManager>.Instance.IndividualSavesContainerPath;

            string dir = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            string leaf = string.IsNullOrEmpty(dir) ? "Unknown Save" : new DirectoryInfo(dir).Name;

            _activeSaveDir = leaf;
            _activeSavePath = Path.Combine(PATHBASE, _activeSaveDir);
            Directory.CreateDirectory(_activeSavePath);
        }
    }
}