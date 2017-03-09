using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FoodJournal.Resources;

#if WINDOWS_PHONE
using Windows.ApplicationModel.Store;
#else
using Android.App;
#endif

namespace FoodJournal.AppModel
{

    public class MessageCenter
    {


		public static void Reset() {checkedForReview = DateTime.Now;}

        private static DateTime checkedForReview = DateTime.MinValue;
        public static void AskForReviewIfAppropriate()
        {
            try
            {
                if (checkedForReview.AddHours(2) > DateTime.Now) return;
                checkedForReview = DateTime.Now;

				#if WINDOWS_PHONE
                Sale sale = AppStats.Current.CurrentSale;
                if (sale != null)
                {

                    if (sale.NumberOfSessionsBetween == 0 || AppStats.Current.SessionId % sale.NumberOfSessionsBetween == 0)
                    {

                        if (MessageBox.Show(sale.SubTitle, sale.Title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            SessionLog.RecordMilestone("Sale Accepted " + sale.ID, AppStats.Current.SessionId.ToString());
                            Navigate.StartBuyProduct(sale.productId);
                        }
                        else
                        {
                            if (sale.ID != null && sale.ID.StartsWith("test"))
                                SessionLog.RecordMilestone("Sale Declined " + sale.ID, AppStats.Current.SessionId.ToString());
                            else
                                SessionLog.RecordTraceValue("Sale Declined " + sale.ID, AppStats.Current.SessionId.ToString());
                        }

                        return;

                    }
                }
				#endif

                FeedbackSettings feedbackSettings = AppStats.Current.FeedbackSettings;

                if (AppStats.Current.DaysSinceInstall < feedbackSettings.DaysTillStart) return;
                if (UserSettings.Current.SuppressAskForReviewCount > feedbackSettings.NumberOfReAttempts) return;
#if SUPPORTTRIAL
                if (AppStats.Current.InstalledProductKind == AppStats.ProductKind.Trial) return;
#endif

                if (AppStats.Current.SessionId % feedbackSettings.NumberOfSessionsBetween != 0) return;
                if (AppStats.Current.ReviewRequests >= feedbackSettings.NumberOfRequests) return;
                AppStats.Current.ReviewRequests += 1;

				#if WINDOWS_PHONE
                if (MessageBox.Show(AppResources.ReviewBody, AppResources.ReviewTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SessionLog.RecordMilestone("Review Request Accepted" + feedbackSettings.ID, AppStats.Current.SessionId.ToString());
                    Navigate.StartReviewTask();
                }
                else
                {
                    SessionLog.RecordTraceValue("Review Request Declined" + feedbackSettings.ID, AppStats.Current.SessionId.ToString());
                }
				#else

				new AlertDialog.Builder(Navigate.navigationContext)
					.SetPositiveButton("Ok", (sender, args) =>
						{
							SessionLog.RecordMilestone("Review Request Accepted" + feedbackSettings.ID, AppStats.Current.ReviewRequests.ToString());
							Navigate.StartReviewTask();
						})
					.SetNegativeButton("Cancel", (sender, args) =>
						{
							SessionLog.RecordTraceValue("Review Request Declined" + feedbackSettings.ID, AppStats.Current.ReviewRequests.ToString());
						})
					.SetMessage(AppResources.ReviewBody)
					.SetTitle(AppResources.ReviewTitle)
					.Create()
					.Show();

#endif
            }
            catch (Exception ex) { 
				LittleWatson.ReportException(ex);
			}
        }

    }

}
