using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FoodJournal.Common.AppModel.Logging;

namespace FoodJournal.Common.AppModel
{
    public static class Platform
    {


        public static void RunSafeOnUIThread(string extra, Action a)
        {
            try
            {
#if WINDOWS_PHONE
				Deployment.Current.Dispatcher.BeginInvoke(a);
#else
               // Navigate.navigationContext.RunOnUiThread(a);
#endif
            }
            catch (Exception ex) { LittleWatson.ReportException(ex, extra); }
        }

        public static void MessageBox(string message)
        {
#if WINDOWS_PHONE
			System.Windows.MessageBox.Show(message);
#else
            try
            {
               /* new AlertDialog.Builder(Navigate.dialogContext)
                    .SetMessage(message)
                    .SetPositiveButton("Ok", (sender, args) =>
                    {
                    })
                    .SetTitle("")
                    .Create()
                    .Show();*/
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
#endif
        }


        public enum NetworkConnectionType
        {
            None,
            Slow,
            Fast
        }

        public static string NetworkConnectionTypeName;
        private static NetworkConnectionType lastType;

        public static NetworkConnectionType GetConnectionType()
        {
#if WINDOWS_PHONE
			if (NetworkConnectionTypeName == null)
			// always checking connection type on Android, because the process may still be alive
#endif
            lastType = GetConnectionTypeEx();
            return lastType;
        }

        public static NetworkConnectionType GetConnectionTypeEx()
        {

          //  if (!Thread.CurrentThread.IsBackground) throw new Exception("Can't get Connection Type on UI Thread (this can take up to 30 seconds)");

            try
            {

#if WINDOWSPHONE

                if (!NetworkInterface.GetIsNetworkAvailable()) return NetworkConnectionType.None;

                using (SessionLog.NewScope("Getting Network Type", null))
                {
                    var type = NetworkInterface.NetworkInterfaceType;
                    NetworkConnectionTypeName = type.ToString();
                    SessionLog.RecordTraceValue("NetworkType", type.ToString());
                    switch (type)
                    {
                        case (NetworkInterfaceType.Wireless80211):
                            return NetworkConnectionType.Fast;
                        case (NetworkInterfaceType.None):
                            return NetworkConnectionType.None;
                    }
                }
#endif
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            return NetworkConnectionType.Slow;
        }


    }
}
