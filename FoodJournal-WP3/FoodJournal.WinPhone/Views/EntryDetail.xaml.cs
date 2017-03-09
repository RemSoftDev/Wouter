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
using FoodJournal.Runtime;
using FoodJournal.AppModel.UI;
using FoodJournal.ViewModels.Fragments;

namespace FoodJournal.Views
{
    public partial class EntryDetail : Screen
    {
        public EntryDetail()
        {
            InitializeComponent();

            try
            {

                DataContext = new EntryDetailVM(Navigate.selectedEntry);

                if (Navigate.selectedItem.IsNewItem)
                {
                    ApplicationBar.Buttons.Add(
                        new MenuButton("/resources/icons/applicationbar.check.png", AppResources.AcceptNew, 
                            (DataContext as EntryDetailVM).CommitNewItem));
                }

                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.TakeAPicture, (DataContext as EntryDetailVM).StartPicture));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.AddServingSize, (DataContext as EntryDetailVM).AddServingSize));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.SendFeedback, Navigate.ToFeedback));

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (DataContext as EntryDetailVM).DeleteFromDB();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }



    }
}