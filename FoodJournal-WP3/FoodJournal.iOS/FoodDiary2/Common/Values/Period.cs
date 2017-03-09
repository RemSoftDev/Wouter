using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Values
{
    public enum Period
    {
        none,
        Exercise,
        Breakfast,
        Lunch,
        Dinner,
        Snack,
        SnackMorning,
        SnackEarlyAfternoon,
        SnackAfternoon,
        SnackEvening,
        SnackMidnight
    }

    public static class PeriodList
    {
        public static List<Period> All { get { return new List<Period>() { Period.Breakfast, Period.SnackMorning, Period.Lunch, Period.SnackEarlyAfternoon, Period.SnackAfternoon, Period.Dinner, Period.SnackEvening, Period.SnackMidnight, Period.Snack }; } }
    }

}
