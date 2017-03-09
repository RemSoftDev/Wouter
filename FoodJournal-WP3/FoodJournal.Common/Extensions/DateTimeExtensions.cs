using FoodJournal.Logging;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Extensions
{

    public static class EnumExtensions
    {
        public static Period ParsePeriod(this string period)
        {
            return (Period)Enum.Parse(typeof(Period), period);
        }
    }

    public static class DateTimeExtensions
    {

        public static DateTime SetTime(this DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, time.Kind);
        }

        public static DateTime SetTime(this DateTime date, int Hour, int Minute, int Second)
        {
            return new DateTime(date.Year, date.Month, date.Day, Hour, Minute, Second, 0, date.Kind);
        }

        public static DateTime Combine(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, time.Kind);
        }

        public static Period Period(this DateTime dt)
        {
            try
            {
                // NOTE: Branched in Live Tile Agent
                if (UserSettings.Current.SnackMidnightEnabled)
                    if (dt.Hour < 6) return Values.Period.SnackMidnight;
                    else
                        if (dt.Hour < 6) return Values.Period.Snack;

                if (UserSettings.Current.SnackMorningEnabled)
                {
                    if (dt.Hour < 10) return Values.Period.Breakfast;
                    if (dt.Hour < 12) return Values.Period.SnackMorning;
                }
                else if (dt.Hour < 11) return Values.Period.Breakfast;

                if (UserSettings.Current.SnackEarlyAfternoonEnabled)
                {
                    if (dt.Hour < 13) return Values.Period.Lunch;
                    if (dt.Hour < 15) return Values.Period.SnackEarlyAfternoon;
                }
                else
                    if (dt.Hour < 14) return Values.Period.Lunch;

                if (UserSettings.Current.SnackAfternoonEnabled)
                {
                    if (dt.Hour < 17) return Values.Period.SnackAfternoon;
                }
                else
                    if (dt.Hour < 17) return Values.Period.Snack;

                if (UserSettings.Current.SnackEveningEnabled)
                {
                    if (dt.Hour < 20) return Values.Period.Dinner;
                    if (dt.Hour < 22) return Values.Period.SnackEvening;
                }
                else
                    if (dt.Hour < 21) return Values.Period.Dinner;

                if (UserSettings.Current.SnackMidnightEnabled)
                    return Values.Period.SnackMidnight;
                else
                    return Values.Period.Snack;

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            return Values.Period.Snack;
        }

        public static string ToStorageStringFull(this DateTime date)
        {
            return date.ToString("s");
        }

        public static string ToStorageStringDate(this DateTime date)
        {
            return string.Format("{0}-{1}-{2}", date.Year, date.Month, date.Day);
        }

        public static string ToStorageStringMonth(this DateTime date)
        {
            return string.Format("{0}-{1}", date.Year, date.Month);
        }

        /// <summary>
        /// Converts date time string to DateTime object,
        /// specific to datetime style of fja app
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value)
        {
            DateTime date = DateTime.Now;
            try
            {
                date = DateTime.Parse(value);
            }
            catch (Exception ex)
            {

            }
            return date;
        }

        public static string ToSafeShortDateString(this DateTime date)
        {
#if !DEBUG
            try
            {
                return date.ToShortDateString();
            }
            catch 
            {
#endif
            return string.Format("{0} {1} {2}", date.Year, date.Month, date.Day);
#if !DEBUG
            }
#endif
        }

    }
}
