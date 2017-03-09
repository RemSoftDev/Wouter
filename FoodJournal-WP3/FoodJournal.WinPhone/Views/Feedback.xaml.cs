using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.ViewModels;
using FoodJournal.Model;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.AppModel.UI;
using FoodJournal.AppModel;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Feedback : Screen
    {
        public Feedback()
        {
            InitializeComponent();

            this.Unlock.Visibility = AppStats.Current.InstalledProductKind != AppStats.ProductKind.Paid ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            this.RemoveAds.Visibility = Visibility.Collapsed;

        }

        protected override void OnNavigatedLayoutUpdated()
        {
            this.Translate.Visibility = AppStats.Current.HasTranslationRequests ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RateTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.StartReviewTask();
        }

        private void EmailTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.StartEmailTask();
        }

#if SCREENSHOT
        private void ScreenshotTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.StartScreenshotTask();
        }
#endif

        private void ShareTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.StartShareTask();
        }

        private void UnlockTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.ToBuyNowPage();
            //Navigate.StartUnlockTask();
        }

        private void RemoveAdsTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.StartRemoveAds();
        }

        private void TranslateTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigate.ToLocalize();
        }

    }
}