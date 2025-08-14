using System;
using System.Collections.Generic;

namespace DealersSendTexts
{
    [Serializable]
    public class DealerState
    {
        public string MostRecent = "Never";
        public int SaleCount = 0;
        public Dictionary<string, string> TodaysSales = new Dictionary<string, string>();
        public Dictionary<string, string> RecentSale  = new Dictionary<string, string>();
        public Dictionary<string, int>    Products    = new Dictionary<string, int>();
        public HashSet<FailureKey>        Failures    = new HashSet<FailureKey>();
        public List<SaleData>             DailySales  = new List<SaleData>();
        public List<SaleData>             TotalSales  = new List<SaleData>();

        public void ClearAll(bool daily = false)
        {
            DailySales .Clear();
            TodaysSales.Clear();
            Failures   .Clear();

            if (!daily)
            {
                Products  .Clear();
                RecentSale.Clear();
                TotalSales.Clear();
            }
        }
    }
}