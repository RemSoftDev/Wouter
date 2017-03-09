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
using FoodJournal.Logging;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using System.IO;
using FoodJournal.Extensions;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;
using FoodJournal.AppModel;

namespace FoodJournal.Views
{

    public partial class DayPivot : Screen
    {

        public DayPivot()
        {
            InitializeComponent();

            try
            {

                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.Add;

                //ApplicationBar.MenuItems.Add(new MenuLink(AppResources.OpenJournal, () => { Navigate.ToJournal((DataContext as DayPivotVM).date); })); 
                //ApplicationBar.MenuItems.Add(new MenuLink(AppResources.Recipes, () => { Navigate.ToRecipes(); }));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.SettingsScreen, () => { Navigate.ToSettingsPage(); }));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.SendFeedback, () => { Navigate.ToFeedback(); }));

#if DEBUG && SCREENSHOT
                ApplicationBar.MenuItems.Add(new MenuLink("Make Screenshots", () => { ScreenshotVM.StartScreenshots(Pivot); }));
#endif

                this.DataContext = new DayPivotVM(Navigate.selectedDate);                
                SyncDayTotal();

                if (Navigate.selectedPeriod == Period.none)
                    Navigate.selectedPeriod = DateTime.Now.Period();

                int selectedPeriodId = 0;
                Pivot.Items.Clear();

                List<Period> list = UserSettings.Current.Meals;
                if (!list.Contains(Navigate.selectedPeriod))
                {
                    list = new List<Period>();
                    list.AddRange(UserSettings.Current.Meals);
                    list.Add(Navigate.selectedPeriod);
                }

                foreach (Period p in list)
                {
                    if (p == Navigate.selectedPeriod) selectedPeriodId = Pivot.Items.Count;
                    PivotItem item = new PivotItem();
                    item.Header = Strings.FromEnum(p);
                    item.Content = new PeriodView() { Period = p };
                    Pivot.Items.Add(item);
                }

                Pivot.SelectedIndex = selectedPeriodId;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void SyncDayTotal()
        {
            try
            {
                (DataContext as DayPivotVM).DayTotal = Cache.GetPropertyTotal((DataContext as DayPivotVM).date, UserSettings.Current.CurrentProperty).ToString(true).ToUpper();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        protected override void OnNavigatedLayoutUpdated()
        {

            MessageCenter.AskForReviewIfAppropriate();

            try
            {
                SyncDayTotal();
                PeriodVM periodVM = (((Pivot.SelectedItem as PivotItem).Content as PeriodView).DataContext as PeriodVM);
                periodVM.StartRequery();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }


        private ContextMenu menu;
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            menu = sender as ContextMenu;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {

                PeriodVM periodVM = (((Pivot.SelectedItem as PivotItem).Content as PeriodView).DataContext as PeriodVM);
                Navigate.selectedPeriod = periodVM.Period;
                periodVM.StartRequery();

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void Add_Click(object sender, EventArgs e)
        {
//            if (AppStats.Current.IsTrialExpired)
//                App.MessageTrialCount(true);
//            else
                //Navigate.ToSearchPage(null);
        }

        private void SelectedDateTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //App.SelectDate();
            //Navigate.ToJournal((DataContext as DayPivotVM).date);
        }

#if DEBUG && SCREENSHOT
        private void MakeScreenshots(object sender, EventArgs e)
        {
            ScreenshotVM.StartScreenshots(Pivot);
        }
#endif

    }
}