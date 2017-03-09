using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.Resources;
using Windows.ApplicationModel.Store;
using System.Globalization;
using FoodJournal.AppModel;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Advertisement : UserControl
    {

        private static List<Advertisement> alladvertisements = new List<Advertisement>();

        private bool isfirstrefresh;

        public Advertisement()
        {
            InitializeComponent();

            adduplex.Visibility = Visibility.Collapsed;
            AdControl1.Visibility = Visibility.Collapsed;
            somaAdViewer.Visibility = Visibility.Collapsed;

            if (!AppStats.Current.ShouldShowAds) Hide();

            isfirstrefresh = true;

        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {

            try
            {

                if (!isfirstrefresh) return;

                isfirstrefresh = false;

                if (AppStats.Current.ShouldShowAds)
                {

                    alladvertisements.Add(this);

                    if (!AppStats.Current.IsDarkTheme)
                        CloseImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Resources/Icons/Cancel.png", UriKind.Relative));


                    if (AppStats.Current.UseAlternativeAds)
                    {

                        try
                        {
                            somaAdViewer.Countrycode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                        }
                        catch (Exception ex) { LittleWatson.ReportException(ex); }

                        somaAdViewer.Pub = 923883975;
                        somaAdViewer.Adspace = 65853176;
                        somaAdViewer.AdError += somaAdViewer_AdError;
                        somaAdViewer.NewAdAvailable += somaAdViewer_NewAdAvailable;
                        somaAdViewer.Visibility = System.Windows.Visibility.Visible;

                        somaAdViewer.StartAds();

                    }
                    else
                    {

                        AdControl1.Visibility = System.Windows.Visibility.Visible;

                        try
                        {
                            AdControl1.CountryOrRegion = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                        }
                        catch (Exception ex) { LittleWatson.ReportException(ex); }

                        AdControl1.AdRefreshed += new EventHandler(adControl1_AdRefreshed);
                        AdControl1.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(adControl1_ErrorOccurred);
                        AdControl1.IsAutoCollapseEnabled = true;

                        adduplex.AdLoadingError += adduplex_AdLoadingError;
                        adduplex.BindingValidationError += adduplex_BindingValidationError;

                    }

                    var nothing = new WebBrowser();

                }

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        void somaAdViewer_NewAdAvailable(object sender, EventArgs e)
        {
            //somaAdViewer.
        }

        void somaAdViewer_AdError(object sender, string ErrorCode, string ErrorDescription)
        {
            LittleWatson.ReportException(new Exception("SOMA: " + ErrorDescription + " - " + ErrorCode));
        }

        void adControl1_AdRefreshed(object sender, EventArgs e)
        {
            // Remove AdDuplex if Ad is refreshed
            adduplex.Visibility = Visibility.Collapsed;
        }

        void adControl1_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "No ad available." && e.Error.Message != "HTTP error status code: NotFound (404)")
                LittleWatson.ReportException(e.Error);

            // Show AdDuplex instead if the old Advertisment fails
            adduplex.Visibility = Visibility.Visible;
        }

        private void adduplex_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
        }

        private void adduplex_AdLoadingError(object sender, AdDuplex.AdLoadingErrorEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "Ad control is hidden by other control" && e.Error.Message != "The remote server returned an error: NotFound." && e.Error.Message != "Exception of type 'System.Net.WebException' was thrown.")
                LittleWatson.ReportException(e.Error);
        }

        public static void HideAll()
        {
            foreach (var ad in alladvertisements)
                ad.Hide();
        }

        public void Hide()
        {
            try
            {
                LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
                adduplex.IsEnabled = false;
                adduplex.Visibility = System.Windows.Visibility.Collapsed;

                somaAdViewer.StopAds();
                somaAdViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void CloseTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SuggestPurchase();
        }

        private void SuggestPurchase()
        {
            try
            {

                if (AppStats.Current.AdShows)
                    Navigate.ToBuyNowPage();

                //if (MessageBox.Show(AppResources.RemoveAdsText, AppResources.RemoveAdsTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                //{
                //    Navigate.StartRemoveAds();
                //}
                //else
                //    SessionLog.RecordTraceValue("RemoveAds", "Cancelled");

                //LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
                //SessionLog.RecordMessage("Advertisement closed");

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

    }
}
