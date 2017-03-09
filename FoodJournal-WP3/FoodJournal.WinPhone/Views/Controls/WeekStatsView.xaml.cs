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

namespace FoodJournal.Views
{
    public partial class WeekStatsView : UserControl
    {
        public WeekStatsView()
        {
            InitializeComponent();
        }

        public Property Property
        {
            get { return (DataContext as WeekStatsVM).property; }
            set { DataContext = new WeekStatsVM(value); }
        }

    }
}
