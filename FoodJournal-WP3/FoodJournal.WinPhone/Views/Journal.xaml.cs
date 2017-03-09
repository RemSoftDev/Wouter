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
using FoodJournal.Logging;
using System.Collections.ObjectModel;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.AppModel.UI;
using FoodJournal.AppModel;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{

    public class JournalPivotItem : PivotItem
    {
        public DateTime Date { get { return VM.Date; } }
        public JournalDayVM VM { get { return (Content as JournalView).DataContext as JournalDayVM; } }

        public JournalPivotItem(JournalVM JournalVM, DateTime date)
        {
            JournalDayVM vm = new JournalDayVM(JournalVM, date);
            Header = vm.DateText;
            var view = new JournalView();
            view.SetVM(JournalVM, vm);
            vm.ValuesTransform = view.ValuesTransform;
            Content = view;
        }

        public void Sync(DateTime date)
        {
            VM.Sync(date);
            (Content as JournalView).UpdateHeader(VM);
            Header = VM.DateText;
            VM.EmailSettingsVisibility = System.Windows.Visibility.Collapsed;
        }

    }

    public partial class Journal : Screen
    {

        private const int BufferSize = 7; // == bufferhalfsize

        public Journal()
        {

            InitializeComponent();

            try
            {
                //ApplicationBar.MenuItems.Add(new MenuLink(AppResources.OpenReport, () => { Navigate.ToReportPage(); }));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.Goals, () => { Navigate.ToSettingsPage(); }));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.EmailReport_ShowSettings, () =>
                                    {
                                        if (AppStats.Current.InstalledProductKind == AppStats.ProductKind.Paid)
                                            (Pivot.SelectedItem as JournalPivotItem).VM.EmailSettingsVisibility = System.Windows.Visibility.Visible;
                                        else
                                            Navigate.ToBuyNowPage();
                                    }));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.SendFeedback, () => { Navigate.ToFeedback(); }));

                var jvm = new JournalVM();
                DateTime date = Navigate.selectedDate;

                for (int i = -BufferSize; i < BufferSize; i++)
                    Pivot.Items.Add(new JournalPivotItem(jvm, date.AddDays(i)));

                DataContext = jvm;
                Pivot.SelectedIndex = BufferSize;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private JournalPivotItem SelectedItem { get { return Pivot.SelectedItem as JournalPivotItem; } }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int itemcount = Pivot.Items.Count;
            for (int i = -1; i < 2; i++)
                (Pivot.Items[(Pivot.SelectedIndex + itemcount + i) % itemcount] as JournalPivotItem).Sync(SelectedItem.Date.AddDays(i)); // also hides email

            (DataContext as JournalVM).SelectedDay = SelectedItem.VM;

        }

        private void SelectedDateTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void Add_Click(object sender, EventArgs e)
        {
            //Navigate.ToSearchPage(null);
        }

        private bool _navigatedToCalled;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _navigatedToCalled = true;
        }

        private void PhoneApplicationPage_LayoutUpdated(object sender, EventArgs e)
        {
//#if DEBUG
//            if (ScreenshotVM.InScreenshot)
//            {
//                if (ScreenshotVM.phase == ScreenshotVM.ScreenshotPhase.Journal)
//                    ScreenshotVM.ScreenshotLayoutUpdated(this);
//                return;
//            }
//#endif
            if (_navigatedToCalled)
            {
                _navigatedToCalled = false;

                try
                {
                    SelectedItem.Sync(SelectedItem.Date);
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }

            }
        }


    }
}