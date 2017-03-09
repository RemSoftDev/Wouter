using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FoodJournal.AppModel.UI;
using FoodJournal.Runtime;
using FoodJournal.WinPhone;

namespace FoodJournal
{
    public partial class Splash : Screen
    {
        public Splash()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedLayoutUpdated()
        {
            base.OnNavigatedLayoutUpdated();

            //if (FoodJournal.Legacy.Model.Database.MigrationNeeded)
            //{
            //    this.Upgrading.Visibility = System.Windows.Visibility.Visible;
            //    FoodJournal.Legacy.Model.Database.Migrate();
            //}

            //if (FoodJournal.Legacy.Model.Database.DeDupeNeeded)
            //{
            //    this.Upgrading.Visibility = System.Windows.Visibility.Visible;
            //    FoodJournal.Legacy.Model.Database.DeDupe();
            //}

            if (FoodJournal.Model.Data.FoodJournalDB.MigrationNeeded)
            {
                this.Upgrading.Visibility = System.Windows.Visibility.Visible;
                FoodJournal.Model.Data.FoodJournalDB.Migrate();
            }

            Navigate.navigationService = this.NavigationService;
            //if (App.InitSession())
                //Navigate.ToDayPivot(true);
        }
    }
}