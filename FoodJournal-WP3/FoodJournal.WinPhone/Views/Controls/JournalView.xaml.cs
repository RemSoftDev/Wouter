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
using System.Windows.Media;
using FoodJournal.Logging;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class JournalView : UserControl
    {

        public JournalView()
        {
            InitializeComponent();

        }

        private JournalVM jvm;

        public void SetVM(JournalVM jvm, JournalDayVM vm)
        {
            this.jvm = jvm;
            UpdateHeader(vm);
            DataContext = vm;
        }

        public void UpdateHeader(JournalDayVM vm)
        {
            var header = vm.GetHeader(jvm.SelectedProperty);
            if (h1.DataContext != header) h1.DataContext = header;
            if (h2.DataContext != header) h2.DataContext = header;
        }

        private void CycleProperty(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PrepDown.Begin();
            h1.DataContext = (DataContext as JournalDayVM).GetHeader(jvm.NextProperty());
            CycleDown.Begin();

            ValuesOut.Begin();
        }

        private void CycleCompleted(object sender, EventArgs e)
        {
            h2.DataContext = h1.DataContext;
        }

        private void ValuesOutCompleted(object sender, EventArgs e)
        {
            var day = (DataContext as JournalDayVM);
            jvm.SelectedProperty = jvm.NextProperty();
            day.Sync(day.Date);
            ValuesIn.Begin();
        }

        private void PeriodTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                Navigate.selectedDate = (DataContext as JournalDayVM).Date;
                //Navigate.ToDayPivot(((sender as StackPanel).DataContext as JournalPeriodVM).Period);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void SendMailTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //(DataContext as JournalDayVM).SendEmailReport();
        }

        private void emailtextchanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                (DataContext as JournalDayVM).Email = (sender as TextBox).Text;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }
    }
}
