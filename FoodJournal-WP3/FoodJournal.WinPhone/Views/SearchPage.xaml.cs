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
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{

    public partial class SearchPage : Screen
    {

        public SearchPage()
        {
            InitializeComponent();

            DataContext = new SearchVM(Navigate.selectedPeriod);

            this.Loaded += new SafeHandler(() => Query.Focus());
            this.BackKeyPress += new SafeHandler((DataContext as SearchVM).OnBackKeyPress);
            Query.TextChanged += new SafeHandler(() => (DataContext as SearchVM).Query = Query.Text);

        }


    }
}