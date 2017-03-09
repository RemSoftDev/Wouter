using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using FoodJournal.Logging;
using FoodJournal.Model;
using System.Threading;
using System.Globalization;
using FoodJournal.WinPhone.Common.Resources;
using System.Diagnostics;
using FoodJournal.Values;
using FoodJournal.Extensions;
using FoodJournal.Messages;
using System.Text;
using FoodJournal.FoodJournalService;
using FoodJournal.AppModel.UI;
using FoodJournal.AppModel;

#if WINDOWS_PHONE
using System.Windows.Markup;
using System.Windows.Navigation;
using Windows.ApplicationModel.Store;
using Microsoft.Phone.Tasks;



#else
using Android.Content;
using FoodJournal.Android15;
using Android.App;
#endif

using FoodJournal.WinPhone.Common.ViewModels.Fragments;
using FoodJournal.Resources;

namespace FoodJournal.Runtime
{
    public class Navigate
    {

#if !WINDOWS_PHONE
        private class AndroidNav
        {
            public Type type;
            public Activity activity;
        }
#endif

        public static DateTime selectedDate = DateTime.Now.Date;

        private static Period _selectedPeriod = Period.none;
        public static Period selectedPeriod
        {
            get
            {
                return _selectedPeriod;
            }
            set
            {
                _selectedPeriod = value;
            }
        }


        public static FoodItem selectedItem;
        public static Entry selectedEntry;
        //public static Recipe selectedRecipe;

#if DEBUG
#if WINDOWS_PHONE
        public static Screen screenshotScreen;
#else
        public static Activity screenshotScreen;
#endif
#endif

        public static string PictureFilename;
        public static string PictureFoodId;

        public static IAcceptsNewEntry IAcceptsNewEntry;

#if WINDOWS_PHONE

        public static NavigationService navigationService;

        private const string PGDayPivot = "/Views/DayPivot.xaml";
        private const string PGSearchPage = "/Views/SearchPage.xaml";
        private const string PGEntryDetail = "/Views/EntryDetail.xaml";
        private const string PGRecipes = "/Views/Recipes.xaml";
        private const string PGRecipeDetail = "/Views/Recipe.xaml";
        private const string PGFeedback = "/Views/Feedback.xaml";
        private const string PGSettingsPage = "/Views/Settings.xaml";
        private const string PGBuyNowPage = "/Views/BuyNow.xaml";
        private const string PGLocalize = "/Views/Localization.xaml";
        private const string PGReportPage = "/Views/Report.xaml";
        private const string PGJournal = "/Views/Journal.xaml";
        private const string PGCamera = "/Views/Camera.xaml";



#else

        public static Activity navigationContext;
        public static Context dialogContext;

      //  private static AndroidNav PGDayPivot = new AndroidNav { type = typeof(Android15.Activities.DayPivot) };
      //  private static AndroidNav PGSearchPage = new AndroidNav { type = typeof(Android15.Activities.Search) };
#if DEBUG1
        private static AndroidNav PGEntryDetail = new AndroidNav { type = typeof(Android15.Activities.EntryEditActivity) };//.EntryDetail)};
#endif
        private static AndroidNav PGRecipes = new AndroidNav { type = null };
        //typeof(Android15.Activities.Recipes);
        private static AndroidNav PGRecipeDetail = new AndroidNav { type = null };
        //typeof(Android15.Activities.Recipe);
        private static AndroidNav PGFeedback = new AndroidNav { type = typeof(Android15.Activities.AboutActivity) }; // Feedback
        private static AndroidNav PGSettingsPage = new AndroidNav { type = typeof(Android15.Activities.Settings) };
        //   private static AndroidNav PGGoals = new AndroidNav { type = typeof(Android15.Activities.Goals) };
#if AMAZON
        private static AndroidNav PGBuyNowPage = new AndroidNav { type = typeof(Android15.Activities.Settings) };
#else
        private static AndroidNav PGBuyNowPage = new AndroidNav { type = typeof(Android15.Activities.BuyNow) };
#endif
        //private static AndroidNav PGLocalize = new AndroidNav { type = typeof(Android15.Activities.Localization) };
        //  private static AndroidNav PGReportPage = new AndroidNav { type = typeof(Android15.Activities.Report) };
        //   private static AndroidNav PGJournal = new AndroidNav { type = typeof(Android15.Activities.JournalActivity) }; // Journal
        private static AndroidNav PGCamera = new AndroidNav { type = null };
        //typeof(Android15.Activities.Camera);

        //private static List<AndroidNav> AllNavs = new List<AndroidNav>(){PGDayPivot,PGSearchPage,PGEntryDetail,PGRecipes,
        //                            PGRecipeDetail,PGFeedback,PGSettingsPage,PGGoals,PGBuyNowPage,PGLocalize,
        //                                PGReportPage, PGJournal,PGCamera};
        private static List<AndroidNav> AllNavs = new List<AndroidNav>(){PGEntryDetail,PGRecipes,
									PGRecipeDetail,PGFeedback,PGSettingsPage,PGBuyNowPage,
										PGCamera};
#endif

#if WINDOWS_PHONE
        private static void navigate(string url)

#else
        private static void navigate(AndroidNav page, bool removeActivityStack = false)
#endif
        {
            try
            {
#if WINDOWS_PHONE
                navigationService.Navigate(new Uri(url, UriKind.RelativeOrAbsolute));
#else
                Intent intent = new Intent(navigationContext, page.type);
                if (removeActivityStack)
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                navigationContext.StartActivity(intent);
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

#if !WINDOWS_PHONE
        public static void SetActiveActivity(Activity activity)
        {
#if DEBUG
            screenshotScreen = activity;
#endif
            foreach (AndroidNav nav in AllNavs)
                if (nav.type == activity.GetType())
                    nav.activity = activity;
        }
#endif

        private static void RemoveBackEntry()
        {
            try
            {

#if WINDOWS_PHONE
                navigationService.RemoveBackEntry();
#else
                //Android.App.
                //ActivityManager am = (ActivityManager) navigationContext.GetSystemService(Context.ActivityService);
                //am.
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

        //public static void ToDayPivot(Period period)
        //{
        //    selectedPeriod = period;
        //    navigate(PGDayPivot);
        //}

        //public static void ToDayPivot(bool FromSplash)
        //{
        //    navigate(PGDayPivot);
        //    if (FromSplash)
        //        RemoveBackEntry();
        //}

        //public static void ToSearchPage(IAcceptsNewEntry iAcceptsNewEntry)
        //{
        //    IAcceptsNewEntry = iAcceptsNewEntry;
        //    navigate(PGSearchPage);
        //}

        public static void ToEntryDetail(Entry entry)
        {
			if (entry == null || entry.Item == null)
				return;
            selectedEntry = entry;
            selectedItem = entry.Item;
            navigate(PGEntryDetail);
        }

        //public static void ToRecipes()
        //{
        //    navigate(PGRecipes);
        //}

        //public static void ToRecipeDetail(Recipe recipe)
        //{
        //    selectedRecipe = recipe;
        //    navigate(PGRecipes);
        //}

        public static void ToFeedback()
        {
//#if DEBUG && WINDOWS_PHONE
//            screenshotScreen = SessionLog.CurrentScreen;
//#endif
            navigate(PGFeedback);
        }

        public static void ToSettingsPage(bool removeActivityStack = false)
        {
            //navigate(PGSettingsPage, removeActivityStack);
            navigate(PGSettingsPage);
        }

        //public static void ToGoalsPage()
        //{
        //    navigate(PGGoals);
        //}

        public static void ToBuyNowPage()
        {
            navigate(PGBuyNowPage);
        }

        public static void ToLocalize()
        {
            //navigate(PGLocalize);
        }

        //public static void ToReportPage()
        //{
        //    navigate(PGReportPage);
        //}

        //public static void ToJournal(DateTime date, Property property)
        //{
        //    UserSettings.Current.CurrentProperty = property;
        //    selectedDate = date;
        //    navigate(PGJournal);
        //}

        //public static void ToJournal(DateTime date)
        //{
        //    selectedDate = date;
        //    navigate(PGJournal);
        //}

        public static void ToCamera(string filename, string foodId)
        {
            PictureFilename = filename;
            PictureFoodId = foodId;
            navigate(PGCamera);
        }

        public static void BackFromEntryDetail() { Back(PGEntryDetail); }
        public static void BackFromSearch() { 
           // Back(PGSearchPage); 
        }
        public static void BackFromFeedback() { Back(PGFeedback); }
        public static void BackFromBuyNow() { Back(PGBuyNowPage); }

#if WINDOWS_PHONE
        public static void BackFromOther() { Back(PGBuyNowPage); }

        private static void Back(string page)
#else
        private static void Back(AndroidNav page)
#endif
        {
#if WINDOWS_PHONE
            try { if (navigationService.CanGoBack) navigationService.GoBack(); }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
#else
            if (page.activity != null)
                page.activity.Finish();

#if DEBUG
            //else
            //Debugger.Break();
#endif
#endif
        }

        public static void BackAfterSubmit()
        {
            try
            {
#if WINDOWS_PHONE
                navigationService.RemoveBackEntry(); // Search screen
                navigationService.GoBack(); // -> Day Pivot
#else
                BackFromEntryDetail();
             //   BackFromSearch();
                //navigate (PGDayPivot);
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }



        public static void StartRemoveAds()
        {
            StartBuyProduct(AppStats.RemoveAdsProduct);
        }

        public static void StartBuyPremium()
        {
            StartBuyProduct(AppStats.PremiumProduct);
        }


        public async static void StartBuyProduct(string product)
        {
            try
            {
                SessionLog.RecordTrace("Buying " + product);

                try
                {
#if WINDOWS_PHONE
                    string receipt = await CurrentApp.RequestProductPurchaseAsync(product, false);
#endif
                }
                catch (Exception ex)
                {
                    LittleWatson.ReportException(ex);
                    SessionLog.RecordTraceValue("Buy " + product, "Failed");

                    // temporarilly suppress ads
                    //MessageBox.Show(AppResources.StoreException);
                    //AppStats.Current.TemporarySuppressAds();
                }

                AppStats.Current.CheckPurchases();

                //#if !DEBUG
                //                if (!AppStats.Current.ShouldShowAds)
                //                    Views.Advertisement.HideAll();
                //#endif

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
            Navigate.BackFromFeedback();
        }

        public static void StartReviewTask()
        {
            try
            {
                SessionLog.RecordMilestone("Rate and Review", AppStats.Current.SessionId.ToString());
                UserSettings.Current.SuppressAskForReviewCount += 1; //SuppressAskForReview = true;
#if WINDOWS_PHONE
                Microsoft.Phone.Tasks.MarketplaceReviewTask detailTask = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
                detailTask.Show();
#else
                Android.Net.Uri uri = Android.Net.Uri.Parse("market://details?id=" + navigationContext.ApplicationContext.PackageName);
                Intent goToMarket = new Intent(Intent.ActionView, uri);
                //goToMarket.AddFlags(ActivityFlags.NewTask);
                try
                {
                    navigationContext.StartActivity(goToMarket);
                }
                catch (ActivityNotFoundException)
                {
                    navigationContext.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("http://play.google.com/store/apps/details?id=" + navigationContext.ApplicationContext.PackageName)));
                }
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
            Navigate.BackFromFeedback();
        }

        public static void StartEmailTask()
        {
            try
            {
                SessionLog.RecordMilestone("Email", AppStats.Current.SessionId.ToString());
                var to = AppStats.email;
                var subject = AppResources.FeedbackSubject;

                #if AMAZON
                try { subject += " (z" + AppStats.Current.AppInstance.ToString().Substring(0, 6) + ")"; }
                #else
                #if V16
                try { subject += " (v" + AppStats.Current.AppInstance.ToString().Substring(0, 6) + ")"; }
                #else
				try { subject += " (" + AppStats.Current.AppInstance.ToString().Substring(0, 6) + ")"; }
                #endif
                #endif
                catch (Exception ex) { LittleWatson.ReportException(ex); }

#if WINDOWS_PHONE
                var task = new EmailComposeTask();
                task.To = to;
                task.Subject = subject;
                task.Show();
#else
                //Intent emailIntent = new Intent(Intent.ActionSendto, Android.Net.Uri.FromParts("mailto",to, null));
                Intent emailIntent = new Intent(Intent.ActionSendto);
                emailIntent.AddFlags(ActivityFlags.NewTask);
                Android.Net.Uri Uri = Android.Net.Uri.Parse("mailto:" + to + "?subject=" + subject);
                emailIntent.SetData(Uri);
                //emailIntent.SetType("text/plain");
                //emailIntent.PutExtra(Intent.ExtraEmail, new string[]{to});
                //emailIntent.PutExtra(Intent.ExtraSubject, subject);
                //emailIntent.PutExtra(Intent.ExtraText, AppResources.Feedback_EmailText);
                navigationContext.StartActivity(Intent.CreateChooser(emailIntent, AppResources.Feedback_Email));
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
            Navigate.BackFromFeedback();
        }


        public static void StartShareTask()
        {
            try
            {
                SessionLog.RecordMilestone("Share", AppStats.Current.SessionId.ToString());
#if WINDOWS_PHONE
                var task = new ShareLinkTask();
                task.LinkUri = new Uri("http://www.windowsphone.com/s?appid=2f44a06e-3d7c-4e11-b74d-9135949a1889", UriKind.Absolute);
                task.Title = AppResources.ShareTitle;
                task.Message = AppResources.ShareMessage;
                task.Show();
#else
                Intent sharingIntent = new Intent(Intent.ActionSend);
                sharingIntent.AddFlags(ActivityFlags.NewTask);
                sharingIntent.SetType("text/plain");
                sharingIntent.PutExtra(Intent.ExtraSubject, AppResources.ShareTitle);
                //sharingIntent.PutExtra(Intent.ExtraText, AppResources.ShareMessage);
                //sharingIntent.PutExtra(Intent.ExtraEmail, "daily.journal@outlook.com");
                navigationContext.StartActivity(Intent.CreateChooser(sharingIntent, AppResources.Feedback_Share));
#endif
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
            Navigate.BackFromFeedback();
        }

    }
}