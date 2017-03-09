using System.Windows;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Phone.Scheduler;
using System;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace FoodJournalAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {

        private static volatile bool _classInitialized;
        public const string periodicTaskName = "FoodJournalAgent";
        public const string scheduledTime = "Scheduled";

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        public static void RemoveAgent()
        {
            try
            {
                if (ScheduledActionService.Find(periodicTaskName) != null)
                    ScheduledActionService.Remove(periodicTaskName);
            }
            catch 
            { // no watson in the agent
            }
        }

        public static void SetCount(int count)
        {

            //count = (new Random()).Next(2, 12);

            if (count > 9) count = 9;

            // Get our application tile
            foreach (ShellTile tile in ShellTile.ActiveTiles)
                if (tile != null)
                {
                    // Any values on the StandardTileData that are not set will not be effected
                    tile.Update(new IconicTileData { Count = count });
                }

        }

        //public static int Period(DateTime dt)
        //{
        //    if (dt.Hour < 5) return 0;
        //    if (dt.Hour < 11) return 1;
        //    if (dt.Hour < 14) return 2;
        //    if (dt.Hour < 17) return 3;
        //    if (dt.Hour < 21) return 3;
        //    return 4;
        //}

        public static int Period(DateTime dt)
        {
            if (dt.Hour < 9) return 0;
            if (dt.Hour < 13) return 1;
            if (dt.Hour < 19) return 2;
            return 3;
        }


        // since = 9am (1), now=14:05 (3)   -> 1     now=21 (4)   -> 2
        // since = 8pm (3), now=8:am  (1+3) -> 0     now=11 (2+3) -> 1
        // since = 10pm, now= 10.05pm (0 + 3 - 
        private int GetCount(DateTime sinceTime)
        {
            DateTime late = sinceTime.Hour > 21 ? sinceTime : sinceTime.AddHours(2); // fix the 10pm bug
            int cnt = (DateTime.Now.Subtract(sinceTime).Days * 3)
                        + Period(DateTime.Now) - Period(late);
            if (cnt < 0) cnt = 0;
            //return Convert.ToInt16(DateTime.Now.Subtract(sinceTime).TotalHours + 1);
            return cnt;
        }

        protected override void OnInvoke(ScheduledTask task)
        {

            try
            {

                DateTime time;
                if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(scheduledTime, out time))
                {

                    SetCount(GetCount(time));
                    //Convert.ToInt16(new Random().NextDouble() * 10 + 1));

                }

            }
            catch
            {
                // throw; // TODO: this really should be logged
            }
            finally
            {

#if DEBUG
                ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(5));
#endif

                NotifyComplete();

            }
        }

    }
}