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
using FoodJournal.ViewModels;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Runtime;

namespace FoodJournal.Views
{
    public partial class Recipes : Screen
    {
        public Recipes()
        {
            InitializeComponent();

            try
            {
                
                (this.ApplicationBar.Buttons[0] as ApplicationBarIconButton).Text = AppResources.Add;

                DataContext = new RecipesVM();               
            
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        protected override void OnNavigatedLayoutUpdated()
        {
            base.OnNavigatedLayoutUpdated();

            try
            {
                (DataContext as RecipesVM).Requery();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        private void Recipe_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ((sender as StackPanel).DataContext as SearchResultRecipeVM).NavigateToRecipeDetails();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            try
            {
#if DEBUG
                var newRecipe = new Model.Recipe("new recipe",null);
                Navigate.ToRecipeDetail(newRecipe);
#endif
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }
    }
}