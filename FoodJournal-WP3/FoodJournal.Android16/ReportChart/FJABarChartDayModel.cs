using System;
using FoodJournal.Values;
using System.Collections.Generic;

namespace FoodJournal.Android15.ReportChart
{
    public class FJABarChartDayModel
    {
        public FJABarChartDayModel()
        {
            Amounts = new Dictionary<Period, Amount>();
        }
        public DateTime Day { get; set; }
        public Dictionary<Period,Amount> Amounts { get; set; }

        public Amount TotalAmount { get; set; }
    }
}