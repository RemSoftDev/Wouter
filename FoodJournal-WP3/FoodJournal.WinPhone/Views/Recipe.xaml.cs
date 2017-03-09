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
    public partial class Recipe : Screen
    {
        public Recipe()
        {
            InitializeComponent();
            try
            {

                DataContext = new RecipeDetailVM(Navigate.selectedRecipe);

                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.Add;

                if (Navigate.selectedRecipe.IsNewItem)
                {
                    ApplicationBar.Buttons.Add(
                        new MenuButton("/resources/icons/applicationbar.check.png", AppResources.AcceptNew,
                            (DataContext as RecipeDetailVM).CommitNewItem));
                }

                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.TakeAPicture, (DataContext as RecipeDetailVM).StartPicture));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.AddServingSize, (DataContext as RecipeDetailVM).AddServingSize));
                ApplicationBar.MenuItems.Add(new MenuLink(AppResources.SendFeedback, Navigate.ToFeedback));

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }



        //private void Delete_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        (DataContext as RecipeDetailVM).DeleteFromDB();
        //    }
        //    catch (Exception ex) { LittleWatson.ReportException(ex); }
        //}

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                (DataContext as RecipeDetailVM).ExpandAmounts();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                (DataContext as RecipeDetailVM).SelectIngredient((sender as ListBox).SelectedItem as IngredientVM);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            try
            {
                Navigate.ToSearchPage(DataContext as RecipeDetailVM);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }


    }
}