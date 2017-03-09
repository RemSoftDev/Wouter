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
using FoodJournal.Values;
using System.Collections.ObjectModel;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Report : Screen
    {
        public Report()
        {
            InitializeComponent();

            var st = new ApplicationBarMenuItem(AppResources.Goals);
            st.Click += SettingsClick;
            ApplicationBar.MenuItems.Add(st);

            var items = new ObservableCollection<WeekStatsVM>();
            //foreach (var prop in UserSettings.SelectedProperties)
                //items.Add(new WeekStatsVM(prop));

            list.ItemsSource = items;

        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        void SettingsClick(object sender, EventArgs e)
        {
            Navigate.ToSettingsPage();
        }

    }
}