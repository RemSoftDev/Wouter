using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Windows;
//using System.Windows.Data;
//using System.Windows.Navigation;
using FoodJournal.AppModel;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{
    public class DayPivotVM : VMBase
    {

        public readonly DateTime date;

        public string DateText { get { return (date == DateTime.Now.Date) ? AppResources.Today : date.ToLongDateString(); } }
        public string PageTitle { get { return DateText.ToUpper(); } }

        private string dayTotal;
        public string DayTotal { get { return dayTotal; } set { dayTotal = value; NotifyPropertyChanged("DayTotal"); } }

        public DayPivotVM(DateTime date)
        {
            this.date = date.Date;
        }

    }
}
