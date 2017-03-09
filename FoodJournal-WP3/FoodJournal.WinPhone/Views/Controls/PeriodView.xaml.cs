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
using System.Windows.Media.Animation;
using FoodJournal.Values;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{

    public partial class PeriodView : UserControl
    {

        public PeriodView()
        {
            InitializeComponent();
#if DEBUG
            DataContext = new PeriodVM(DateTime.Now.Date, Period.Breakfast);
#endif
        }

        private Period period;
        public Period Period
        {
            get { return period; }
            set
            {
                period = value;
                DataContext = new PeriodVM(Navigate.selectedDate, period);
            }
        }

        private void GotoDetails(object sender, System.Windows.Input.GestureEventArgs e)
        {

            EntryRowVM entryVM = (sender as StackPanel).DataContext as EntryRowVM;
            if (entryVM == null) return;
            entryVM.NavigateToEntryDetail();

            //object result = entry.ResultObject;

            //if (null != result)
            //{
            //    if (result is Item)
            //        Navigate.ToEntryDetails((DataContext as PeriodVM).NewEntry(result as Item));
            //    else
            //        Navigate.ToEntryDetails(result as Entry);
            //}

        }

        //private void ItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    (DataContext as PeriodVM).PlusOne((sender as Grid).DataContext as ResultVM); 
        //}

        //private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    App.TodayVM2.SyncAll();
        //}

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = false;
            (DataContext as PeriodVM).DeleteEntry(menu.DataContext as EntryRowVM);
        }

        private ContextMenu menu;
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            menu = sender as ContextMenu;
            if (menu.Items.Count == 0)
            {
                var mi = new Microsoft.Phone.Controls.MenuItem();
                mi.Header = AppResources.DeleteEntry;
                mi.Click += MenuItem_Click;
                menu.Items.Add(mi);
            }
        }

    }
}