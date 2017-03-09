using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Common.AppModel.Logging
{
    public class LittleWatson
    {

        public static void Invoke(Action a)
        {
            try
            {
                a.Invoke();
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        internal static void ReportException(Exception ex)
        {
          /*  string caughtIn = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
            SessionLog.ReportException(ex, caughtIn);*/
        }

        internal static void ReportException(Exception ex, string extra)
        {
          /*  string caughtIn = "unknown" + extra;
            try
            {
                caughtIn = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name + ' ' + extra;
            }
            catch
            {
            }
            SessionLog.ReportException(ex, caughtIn);*/
        }
    }
}
