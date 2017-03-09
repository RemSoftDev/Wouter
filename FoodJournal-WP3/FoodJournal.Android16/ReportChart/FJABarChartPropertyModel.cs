using System.Collections.Generic;
namespace FoodJournal.Android15.ReportChart
{
    public class FJABarChartPropertyModel : Java.Lang.Object
    {
        public FJABarChartPropertyModel()
        {
            Days = new List<FJABarChartDayModel>();
        }
        public FoodJournal.Values.Property Property { get; set; }
        public List<FJABarChartDayModel> Days { get; set; }

        public Android.Graphics.Color Color { get; set; }
    }
}