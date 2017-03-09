using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.AppModel;
using System.Collections.Generic;
using FoodJournal.Values;
using FoodJournal.Logging;
using FoodJournal.Messages;
using System.Threading;
using System.Globalization;
using System.Windows.Controls;
using FoodJournal.ViewModels;
using System.Xml;

namespace FoodJournal.WinPhone
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        public static bool InitSession()
        {

            bool IsExpired = false;
            try
            {
                if (AppStats.Current.Version.CompareTo(AppStats.Current.ExpiredVersion) != 1)
                    if (DateTime.Now > AppStats.Current.LastUpgrade.AddMonths(AppStats.Current.ExpiredMonths))
                        IsExpired = true;
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
                if (DateTime.Now > AppStats.Current.LastUpgrade.AddMonths(3))
                    IsExpired = true;
            }

            if (IsExpired)
            {
                SessionLog.RecordTrace("App Expired");
                if (MessageBox.Show(AppResources.ExpiredMessage, AppResources.ExpiredCaption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    (new Microsoft.Phone.Tasks.MarketplaceDetailTask()).Show();

                SessionLog.RecordTrace("Terminating");
                Application.Current.Terminate();
            }

            //UserSettings.SelectedProperties = new List<Property>() { StandardProperty.Calories, StandardProperty.TotalFat, StandardProperty.Carbs, StandardProperty.Protein };

            // processes live tile and other settings:
            var x = UserSettings.Current;

            if (AppStats.Current.SessionId == 1)
                MessageQueue.Push(AppStats.Current);
            else
                MessageQueue.StartPump();

            return true;
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SessionLog.RecordTrace("Application_Deactivated");
            SessionLog.Push();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SessionLog.RecordTrace("Application_Closing");
            SessionLog.Push();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LittleWatson.ReportException(e.Exception);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            //e.Handled = true;
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {

            if (e.ExceptionObject != null && e.ExceptionObject.StackTrace != null)
                if (e.ExceptionObject.StackTrace.Contains("WebBrowser.Fire")) // ScriptNotifyEvent or LoadCompleted"))
                {
                    if (e.ExceptionObject.Message == "Parameter count mismatch." || e.ExceptionObject.Message == "Object reference not set to an instance of an object.")
                    {
                        // ad contol cretaors thought it was a good idea to randomly crash someone else's app :o
                        // just eat this exception
                        SessionLog.RecordTraceValue("Eaten exception", e.ExceptionObject.Message);
                        e.Handled = true;
                        return;
                    }
                }

            if (e.ExceptionObject != null && e.ExceptionObject.Message != null)
                if (e.ExceptionObject.Message.StartsWith("FrameworkDispatcher.Update has not been called.")
                  || e.ExceptionObject.Message.Contains("Cannot create instance of type 'Microsoft.Phone.Controls.WebBrowser'"))
                {
                    // ad contol cretaors thought it was a good idea to randomly crash someone else's app :o
                    // just eat this exception
                    SessionLog.RecordTraceValue("Eaten exception", e.ExceptionObject.Message);
                    e.Handled = true;
                    return;
                }

            Exception exception = e.ExceptionObject;
            if ((exception is XmlException || exception is NullReferenceException) && exception.ToString().ToUpper().Contains("INNERACTIVE"))
            {
                SessionLog.RecordTraceValue("Handled Inneractive exception {0}", exception.Message);
                e.Handled = true;
                return;
            }
            else if (exception is NullReferenceException && exception.ToString().ToUpper().Contains("SOMA"))
            {
                SessionLog.RecordTraceValue("Handled Smaato null reference exception {0}", exception.Message);
                e.Handled = true;
                return;
            }
            else if ((exception is System.IO.IOException || exception is NullReferenceException) && exception.ToString().ToUpper().Contains("GOOGLE"))
            {
                SessionLog.RecordTraceValue("Handled Google exception {0}", exception.Message);
                e.Handled = true;
                return;
            }
            else if (exception is ObjectDisposedException && exception.ToString().ToUpper().Contains("MOBFOX"))
            {
                SessionLog.RecordTraceValue("Handled Mobfox exception {0}", exception.Message);
                e.Handled = true;
                return;
            }
            else if ((exception is NullReferenceException || exception is XamlParseException) && exception.ToString().ToUpper().Contains("MICROSOFT.ADVERTISING"))
            {
                SessionLog.RecordTraceValue("Handled Microsoft.Advertising exception {0}", exception.Message);
                e.Handled = true;
                return;
            }

            LittleWatson.ReportException(e.ExceptionObject);
            //if (e.ExceptionObject.StackTrace != null && e.ExceptionObject.StackTrace.Contains("AdDuplex"))
            //    e.Handled = true;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            //e.Handled = true;
        }


        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

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
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }
#endif

        private void InitializeLanguage()
        {
            try
            {

#if DEBUG
                try
                {
                    if (!String.IsNullOrWhiteSpace(AppStats.appForceCulture))
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo(AppStats.appForceCulture);
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(AppStats.appForceCulture);
                    }
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
#endif

                AppStats.CultureComma = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                AppStats.CultureCommaNotADot = AppStats.CultureComma != ".";

                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection, true);

#if DEBUG
                if (String.IsNullOrWhiteSpace(AppStats.appForceCulture) == false)
                    flow = AppStats.appForceFlow;
#endif

                RootFrame.FlowDirection = flow;
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

                throw;
            }
        }

        private void ItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var sr = (sender as Grid).DataContext as SearchResultVM;
                if (sr == null) return;
                var vm = sr.Listener;
                if (vm == null) return;
                vm.OnItemTap(sr);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

    }
}