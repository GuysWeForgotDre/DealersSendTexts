using MelonLoader;
using System.Collections.Generic;

namespace DealersSendTexts
{
    public class DealerPrefs
    {
        public static DealerPrefs Master;
        private static bool MasterOnly;
        private static bool Initialized = false;
        private const string MasterName = "_Master";

        private static readonly Dictionary<string, DealerPrefs> _dealerSettings = new Dictionary<string, DealerPrefs>();

        private MelonPreferences_Category     DealerGroup;
        private MelonPreferences_Entry<bool>  OverrideMaster;
        private MelonPreferences_Entry<bool>  CreateDealers;

        private MelonPreferences_Entry<EMsg>  ShowStarted;
        private MelonPreferences_Entry<EMsg>  ShowSuccess;
        private MelonPreferences_Entry<EMsg>  ShowFailure;

        private MelonPreferences_Entry<bool>  ShowSummary;
        private MelonPreferences_Entry<bool>  SendCustomers;
        private MelonPreferences_Entry<bool>  SendLocations;
        private MelonPreferences_Entry<bool>  SendFailures;

        private MelonPreferences_Entry<EMsg>  CheckProduct;
        private MelonPreferences_Entry<float> ProductAlert;
        private MelonPreferences_Entry<EMsg>  CheckCash;
        private MelonPreferences_Entry<float> CashAlert;

        private MelonPreferences_Entry<EMsg>  CustomerInjury;
        private MelonPreferences_Entry<EMsg>  DealerInjury;
        private MelonPreferences_Entry<EMsg>  IsStuckAlert;
        private MelonPreferences_Entry<int>   IsStuckCount;
        private MelonPreferences_Entry<int>   IsStuckRadius;
        private MelonPreferences_Entry<EMsg>  Navigation;
        private MelonPreferences_Entry<int>   NavDeltaTime;

        public static void Initialize()
        {
            if (Initialized) return;
            Master      = AddDealer(MasterName);
            MasterOnly  = !Master.CreateDealers.Value;
            Initialized = true;
        }

        public EMsg GetShowStarted()    => GetValue(ShowStarted,    Master.ShowStarted);
        public EMsg GetShowSuccess()    => GetValue(ShowSuccess,    Master.ShowSuccess);
        public EMsg GetShowFailure()    => GetValue(ShowFailure,    Master.ShowFailure);

        public bool GetShowSummary()    => GetValue(ShowSummary,    Master.ShowSummary);
        public bool GetSendCustomers()  => GetValue(SendCustomers,  Master.SendCustomers);
        public bool GetSendLocations()  => GetValue(SendLocations,  Master.SendLocations);
        public bool GetSendFailures()   => GetValue(SendFailures,   Master.SendFailures);

        public EMsg GetCheckProduct()   => GetValue(CheckProduct,   Master.CheckProduct);
        public float GetProductAlert()  => GetValue(ProductAlert,   Master.ProductAlert);
        public EMsg GetCheckCash()      => GetValue(CheckCash,      Master.CheckCash);
        public float GetCashAlert()     => GetValue(CashAlert,      Master.CashAlert);

        public EMsg GetCustomerInjury() => GetValue(CustomerInjury, Master.CustomerInjury);
        public EMsg GetDealerInjury()   => GetValue(DealerInjury,   Master.DealerInjury);
        public EMsg GetIsStuckAlert()   => GetValue(IsStuckAlert,   Master.IsStuckAlert);
        public int  GetIsStuckCount()   => GetValue(IsStuckCount,   Master.IsStuckCount);
        public int  GetIsStuckRadius()  => GetValue(IsStuckRadius,  Master.IsStuckRadius);
        public EMsg GetNavigation()     => GetValue(Navigation,     Master.Navigation);
        public int  GetINavDeltaTime()  => GetValue(NavDeltaTime,   Master.NavDeltaTime);

        public static DealerPrefs Prefs(string firstName) => MasterOnly ? Master : _dealerSettings.TryGetValue(firstName, out var prefs) ? prefs : AddDealer(firstName);
        private static DealerPrefs AddDealer(string name)
        {
            DealerPrefs prefs = new DealerPrefs { DealerGroup = MelonPreferences.CreateCategory("DealersSendTexts_" + name, name + " Preferences") };
            if (name == MasterName)
            {
                prefs.CreateDealers = prefs.DealerGroup.CreateEntry($"{name}00_CreateDealers", default_value: false, "Create settings per dealer [requires restart]");
                prefs.CreateDealers.Comment = "Create separate settings panel for each dealer. Requires restart. (\"default\" false but will not auto delete if reset)";
            }
            else
            {
                prefs.OverrideMaster = prefs.DealerGroup.CreateEntry($"{name}00_OverrideMaster", default_value: false, "[Override Master]: Use these settings instead [false]");
                prefs.OverrideMaster.Comment = "Use individual dealer profile settings instead of generic master settings (default false)";
            }

            prefs.ShowStarted    = prefs.DealerGroup.CreateEntry($"{name}01_ShowStarted",    default_value: EMsg.Silent, "Text: When a new deal is started [default silent]");
            prefs.ShowSuccess    = prefs.DealerGroup.CreateEntry($"{name}02_ShowSuccess",    default_value: EMsg.Silent, "Text: When deal successfully completed [silent]");
            prefs.ShowFailure    = prefs.DealerGroup.CreateEntry($"{name}03_ShowFailure",    default_value: EMsg.Notify, "Text: When deal failed / timed out [notify]");

            prefs.ShowSummary    = prefs.DealerGroup.CreateEntry($"{name}04_ShowSummary",    default_value: true,        "Daily: Summary of deals completed that day [true]");
            prefs.SendCustomers  = prefs.DealerGroup.CreateEntry($"{name}05_SendCustomers",  default_value: false,       "Daily: Time of today's deal, per customer [false]");
            prefs.SendLocations  = prefs.DealerGroup.CreateEntry($"{name}06_SendLocations",  default_value: false,       "Daily: List of locations visited today [false]");
            prefs.SendFailures   = prefs.DealerGroup.CreateEntry($"{name}07_SendFailures",   default_value: true,        "Daily: List of failed customers / locations [true]");

            prefs.CheckProduct   = prefs.DealerGroup.CreateEntry($"{name}08_CheckProduct",   default_value: EMsg.Silent, "Alert: When product count below threshold [silent]");
            prefs.ProductAlert   = prefs.DealerGroup.CreateEntry($"{name}09_ProductAlert",   default_value: 20f,         "--Product count (total) to trigger alert [20]");
            prefs.CheckCash      = prefs.DealerGroup.CreateEntry($"{name}10_CheckCashLevel", default_value: EMsg.Disable, "Alert: When cash is above threshold [disable]");
            prefs.CashAlert      = prefs.DealerGroup.CreateEntry($"{name}11_CashAlert",      default_value: 50000f,      "--Minimum cash amount to trigger alert [50,000]");

            prefs.CustomerInjury = prefs.DealerGroup.CreateEntry($"{name}12_CustomerInjury", default_value: EMsg.Silent, "Alert: When customer knocked out or dies [silent]");
            prefs.DealerInjury   = prefs.DealerGroup.CreateEntry($"{name}13_DealerInjury",   default_value: EMsg.Notify, "Alert: When dealer is knocked out or dies [notify]");
            prefs.IsStuckAlert   = prefs.DealerGroup.CreateEntry($"{name}14_IsStuckAlert",   default_value: EMsg.Notify, "Alert: When dealer hasn't moved in too long [notify]");
            prefs.IsStuckCount   = prefs.DealerGroup.CreateEntry($"{name}15_IsStuckCount",   default_value: 4,           "Failed move checks before sending stuck alert [4]");
            prefs.IsStuckRadius  = prefs.DealerGroup.CreateEntry($"{name}16_IsStuckRadius",  default_value: 5,           "Min. move distance to not be considered stuck [5]");
            prefs.Navigation     = prefs.DealerGroup.CreateEntry($"{name}17_Navigation",     default_value: EMsg.Notify, "Text: When new navigation target set [notify]");
            prefs.NavDeltaTime   = prefs.DealerGroup.CreateEntry($"{name}18_NavDeltaTime",   default_value: 5,           "Min. time between navigation messages (minutes) [5]");

            prefs.ShowStarted.Comment    = "Send a text message when a new deal is started (default silent)";
            prefs.ShowSuccess.Comment    = "Send a text message when a deal is successfully completed (default silent)";
            prefs.ShowFailure.Comment    = "Send a text message when a deal is failed or timed out (default notify)";

            prefs.ShowSummary.Comment    = "Send a summary each night of deals completed / failed and money made (default true)";
            prefs.SendCustomers.Comment  = "Send a summary each night time of deal with each customer (default false)";
            prefs.SendLocations.Comment  = "Send a summary each night of locations visited that day for successful deals (default false)";
            prefs.SendFailures.Comment   = "Send a summary each night of failed customers / locations (default true)";

            prefs.CheckProduct.Comment   = "Send a text message when product count (any type) dips below a threshold (default silent)";
            prefs.ProductAlert.Comment   = "Threshold to send low product alert (total count of product, e.g. brick = 20) (default 20)";
            prefs.CheckCash.Comment      = "Send a text message when cash on hand is above a threshold (default disable)";
            prefs.CashAlert.Comment      = "Amount of cash on hand to send cash alert (default 50,000)";

            prefs.CustomerInjury.Comment = "Send a text message when a customer is knocked out or is killed (default silent)";
            prefs.DealerInjury.Comment   = "Send a text message when a dealer is knocked out or is killed (default notify)";
            prefs.IsStuckAlert.Comment   = "Send a text message when dealer has not moved from one spot for too long (default notify)";
            prefs.IsStuckCount.Comment   = "Number of failed checks to send stuck alert (every 20 min) (default 4)";
            prefs.IsStuckRadius.Comment  = "Minimum distance dealer must move to not be considered stuck (default 5)";
            prefs.Navigation.Comment     = "Send a message when setting a new movement destination (default notify)";
            prefs.NavDeltaTime.Comment   = "Minimum cooldown minutes between navigation messages in minutes (default 5)";

            prefs.DealerGroup.SetFilePath(ModSaveData.PrefsPath);
            _dealerSettings[name] = prefs;
            return prefs;
        }

        public static void ClearAll()
        {
            foreach (DealerPrefs pref in _dealerSettings.Values)
            {
                pref.DealerGroup.Entries.Clear();
                pref.DealerGroup = null;
            }
            _dealerSettings.Clear();
            Initialized = false;
        }

        private T GetValue<T>(MelonPreferences_Entry<T> local, MelonPreferences_Entry<T> master) => MasterOnly || !OverrideMaster.Value ? master.Value : local.Value;
    }
}