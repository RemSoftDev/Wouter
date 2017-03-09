
using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;

using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.AppModel;
using System.Collections.Generic;
using FoodJournal.Values;
using FoodJournal.Logging;
using FoodJournal.Messages;
using System.Threading;
using System.Globalization;

using FoodJournal.ViewModels;
using Android.App;
using FoodJournal.Runtime;
using FoodJournal.Resources;
using Android.Content;
using FoodJournal.AppModel.Data;

namespace FoodJournal.Android15
{
#if !DEBUG
	[Application(Debuggable=false, Icon = "@drawable/ic_icon")]
#else
    [Application(Debuggable = true, Icon = "@drawable/ic_icon")]
#endif
    public partial class App : Application
    {

        public static bool IsSessionInitialized = false;
        static DateTime _LastInitCalled = DateTime.MinValue;
        public static string SharedPrefFileName = "FJA_Pref";

		public static DateTime LastInitCalled
        {
            get
            {
                return _LastInitCalled;
            }
            set
            {
                _LastInitCalled = value;
            }
        }
        // used to track onstart -> onstop combination for navigation

        public App(IntPtr javaReference, Android.Runtime.JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
        public override void OnCreate()
        {
            base.OnCreate();

            InitSession(ApplicationContext);
		
        }

        public static void FinalizeSession()
        {

            if (LastInitCalled.AddSeconds(5) > DateTime.Now)
                // Start was called in the last 5 seconds; we're navigating (not pushing to the background). Lets not finalize
                return;

            LastInitCalled = DateTime.MinValue;

            //5 * 60 * 1000
            // if we're still not back in 4 seconds, finalize the session
            BackgroundTask.Start(4000, () =>
            {
                Platform.RunSafeOnUIThread("FinalizeSession", () =>
                {

					//FoodJournal.Model.Data.FoodJournalBackup.Log("FinalizeSession", null, null);

                    if (LastInitCalled != DateTime.MinValue)
                        return; // we are back before our timeout, dont finalize the session now either

                    try
                    {
                        SessionLog.Push();
                        AppStats.Reset();
                        UserSettings.Reset();
                        MessageCenter.Reset();
                    }
                    catch (Exception ex)
                    {
                        LittleWatson.ReportException(ex, "Resetting");
                    }

                    IsSessionInitialized = false; // now we are closed. next time initsession is called, we have to start a new session

                });
            });
        }

#if WINDOWS_PHONE
		public static bool InitSession ()
		




#else
        public static bool InitSession(Context ApplicationContext)
#endif
        {

            LastInitCalled = DateTime.Now;

            if (IsSessionInitialized)
                return false;


			//FoodJournal.Model.Data.FoodJournalBackup.Log("InitSession", null, null);

            IsSessionInitialized = true;

            // if we're still getting too many session logs, we could consider only proceeding here if FinalizeSession succeeded longer than 1 hour ago or something

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {

                LittleWatson.ReportException(e.ExceptionObject as Exception, "unhandledexception");
                if (e.IsTerminating)
                {
                    FinalizeSession();
                }
            };
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
            {
                SessionLog.Push();
            };

#if WINDOWS_PHONE
			if (DateTime.Now > AppStats.Current.LastUpgrade.AddMonths (3)) {
#else

            FoodJournal.Android15.AlarmService.ResetNotifications();
            InitializeLanguage();

            if (DateTime.Now > AppStats.Current.LastUpgrade.AddMonths(48))
            {
#endif

                SessionLog.RecordTrace("App Expired");

#if WINDOWS_PHONE
				if (MessageBox.Show (AppResources.ExpiredMessage, AppResources.ExpiredCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					(new Microsoft.Phone.Tasks.MarketplaceDetailTask ()).Show ();

				SessionLog.RecordTrace ("Terminating");
				Application.Current.Terminate ();
#else

                new AlertDialog.Builder(ApplicationContext)
                    .SetPositiveButton("Ok", (sender, args) =>
                    {
                        //Navigate.navigationContext = ApplicationContext;
                        Navigate.StartReviewTask();
                    })
                    .SetNegativeButton("Cancel", (sender, args) =>
                    {
                        SessionLog.RecordTrace("Terminating");
                        throw new Exception();
                    })
                    .SetMessage(AppResources.ExpiredMessage)
                    .SetTitle(AppResources.ExpiredCaption)
                    .Create()
                    .Show();

                return false;
#endif
            }

            //			UserSettings.Current.SelectedProperties = new List<Property> () {
            //				StandardProperty.Calories,
            //				StandardProperty.TotalFat,
            //				StandardProperty.Carbs,
            //				StandardProperty.Protein
            //			};

            try
            {
                // processes live tile and other settings:
                var x = UserSettings.Current;
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }

            // 9/6/15: I thikn this is not needed to be done this quickly anymore, and its a pretty heavy operation (loading SQL dependencies, etc)
            // commenting out for now

            // 9/19/15: this is messing with purchase stats, so bringing it back, but asynch for now
            BackgroundTask.Start(4000, () =>
            {
                if (AppStats.Current.SessionId == 1)
                    MessageQueue.Push(AppStats.Current);
                else
                    MessageQueue.StartPump();
            });

            SyncQueue.StartLoad();

            return true;
        }
		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);
		}
#if DEBUG
        public static void SetThreadCulture()
        {
            try
            {
                // Change locale to appForceCulture if it is not empty
                if (String.IsNullOrWhiteSpace(AppStats.appForceCulture) == false)
                {
                    // Force app globalization to follow appForceCulture
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(AppStats.appForceCulture);

                    // Force app UI culture to follow appForceCulture
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(AppStats.appForceCulture);

					Application.Context.Resources.Configuration.Locale = new Java.Util.Locale(Thread.CurrentThread.CurrentCulture.Name.ToLower().Substring(0, 2));
                    Application.Context.Resources.UpdateConfiguration(Application.Context.Resources.Configuration, Application.Context.Resources.DisplayMetrics);
                }
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }
#endif


        private static void InitializeLanguage()
        {
            try
            {

#if DEBUG
                SetThreadCulture();
#endif

                AppStats.CultureComma = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                AppStats.CultureCommaNotADot = AppStats.CultureComma != ".";

                //				#if !DEBUG
                //				RootFrame.Language = XmlLanguage.GetLanguage (AppResources.ResourceLanguage);
                //				FlowDirection flow = (FlowDirection)Enum.Parse (typeof(FlowDirection), AppResources.ResourceFlowDirection, true);
                //
                //				#if DEBUG
                //				if (String.IsNullOrWhiteSpace (AppStats.appForceCulture) == false)
                //					flow = AppStats.appForceFlow;
                //				#endif
                //
                //				RootFrame.FlowDirection = flow;
                //				#endif
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

    }
}

