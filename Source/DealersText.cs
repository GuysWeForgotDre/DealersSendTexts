using MelonLoader;
using System;

namespace DealersSendTexts
{
    public enum EMsg { Notify, Silent, Disable }
    public class DealerText : MelonMod 
    {
        public const string ModName = "Dealers Send Texts";
        public const string Version = "2.0.0";
        public const string ModDesc = "Dealers text updates on deals and daily summary in Schedule One";

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName.Equals("main", StringComparison.OrdinalIgnoreCase))
                DealerPrefs.Initialize();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName.Equals("main", StringComparison.OrdinalIgnoreCase))
            { 
                ContractManager.ClearAll();
                LocationManager.ClearAll();
                DealerManager  .ClearAll();
                Pathing        .ClearAll();
            }
        }
    }
}