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
using FoodJournal.Runtime;
using FoodJournal.AppModel;

namespace FoodJournal.Views
{
    public partial class Settings : Screen
    {
        public Settings()
        {
            InitializeComponent();

            DataContext = new SettingsVM();

            var fb = new ApplicationBarMenuItem(AppResources.SendFeedback);
            fb.Click += SendFeedbackClick;
            ApplicationBar.MenuItems.Add(fb);

        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void PinToStart_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            try
            {
                SessionLog.RecordMilestone("Pin To Start", AppStats.Current.SessionId.ToString());

                string tileId = new Random().Next().ToString();
                Uri mp = new Uri(@"/Views/Splash.xaml?" + "tileid=" + tileId, UriKind.Relative);

                IconicTileData tileData = new IconicTileData();
                tileData.Title = "@AppResLib.dll,-200";
                tileData.IconImage = new Uri(@"/Resources/IconTransparent.png", UriKind.Relative);
                tileData.SmallIconImage = new Uri(@"/Resources/IconTransparent.png", UriKind.Relative);
                ShellTile.Create(mp, tileData, false);

                (sender as Button).Visibility = System.Windows.Visibility.Collapsed;

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }


        void SendFeedbackClick(object sender, EventArgs e)
        {
            Navigate.ToFeedback();
        }

    }
}