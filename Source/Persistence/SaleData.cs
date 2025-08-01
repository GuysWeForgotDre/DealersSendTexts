#if   Il2Cpp
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Quests;

#elif Mono
using ScheduleOne.Economy;
using ScheduleOne.GameTime;
using ScheduleOne.Money;
using ScheduleOne.Quests;

#endif
using Newtonsoft.Json;
using System;

namespace DealersSendTexts
{
    [Serializable]
    public class SaleData
    {
        public string Product;
        public string Description;
        public string Customer;
        public string Location;
        public string Window;
        public string Started;
        public string Cost;
        public string Status;
        public string Date;
        public float  Payment;
        public int    Quantity;
        public int    StartTime;

        [JsonIgnore]
        public DealerManager Stats;

        public SaleData() { } //required for json
        public SaleData(Contract contract, DealerManager stats)
        {
            Stats       = stats;
            Product     = contract.ProductList.entries[0].ProductID;
            Description = contract.ProductList.GetCommaSeperatedString();
            Customer    = contract.Customer.GetComponent<Customer>()?.NPC?.fullName ?? "Unknown";
            Location    = contract.DeliveryLocation.LocationName;
            Window      = DealWindowInfo.GetWindow(contract.DeliveryWindow.WindowStartTime).ToString();
            Started     = TimeManager.Get12HourTime(contract.AcceptTime.time);
            Cost        = MoneyManager.FormatAmount(contract.Payment * (1 - stats.Dealer.Cut));
            Status      = "Accepted";
            Date        = Util.DayDate();
            Payment     = contract.Payment * (1 - stats.Dealer.Cut);
            Quantity    = contract.ProductList.GetTotalQuantity();
            StartTime   = contract.AcceptTime.time;
        }
    }
}