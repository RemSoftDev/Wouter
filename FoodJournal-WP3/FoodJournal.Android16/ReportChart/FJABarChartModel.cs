using System;
using System.Collections.Generic;
namespace FoodJournal.Android15.ReportChart
{
    public class FJABarChartModel
    {
        public DateTime StartDate { get; set; }
        public FJABarChartModel()
        {
            Properties = new List<FJABarChartPropertyModel>();
        }
        public List<FJABarChartPropertyModel> Properties { get; set; }
    }
}