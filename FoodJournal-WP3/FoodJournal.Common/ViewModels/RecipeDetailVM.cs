using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FoodJournal.Model;
using FoodJournal.Values;
using System.Windows;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Parsing;
using FoodJournal.Logging;
using System.Collections.Generic;
using FoodJournal.Messages;
using FoodJournal.AppModel.UI;
using FoodJournal.ViewModels.Fragments;
using FoodJournal.AppModel;
using FoodJournal.Runtime;
using FoodJournal.WinPhone.Common.ViewModels.Fragments;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels
{
#if false
    public class RecipeDetailVM : VMBase, IAcceptsSelectedAmount, IAcceptsNewEntry
    {

        private readonly Recipe recipe;

        //public PicturesVM PicturesVM { get; set; }
        public AmountCollectionVM AmountCollectionVM { get; set; }
        public ObservableCollection<IngredientVM> Ingredients { get; set; }

        public RecipeDetailVM(Recipe Recipe)
        {
            this.recipe = Recipe;
            //PicturesVM = new PicturesVM(item);
            AmountCollectionVM = new AmountCollectionVM(this);
            PopulateIngredients();
            PopulateProperties();
        }

        //public Recipe Recipe { get { return Recipe; } }

        public Visibility DescriptionVisibility { get { return !string.IsNullOrEmpty(recipe.Description) ? Visibility.Visible : Visibility.Collapsed; } }

        public string Text { get { return recipe.Text; } set { if (value != recipe.Text) recipe.Text = value; } }
        public string Description { get { return recipe.Description; } set { if (value != recipe.Description) recipe.Description = value; } }
        public string CookingTime { get { return recipe.CookTime; } set { recipe.CookTime = value; } }

        public string TotalAmount { get { return recipe.TotalAmount.ToString(true); } }

        private bool AmountExpanded;
        public Visibility SelectedAmountTextVisibility { get { return AmountExpanded ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility SelectedAmountListVisibility { get { return AmountExpanded ? Visibility.Visible : Visibility.Collapsed; } }

        public void ExpandAmounts() { AmountExpanded = true; NotifyPropertyChanged("SelectedAmountTextVisibility"); NotifyPropertyChanged("SelectedAmountListVisibility"); }

        // IAcceptsSelectedAmount
        public void SetSelectedAmountAndScale(Amount SelectedAmount, float Scale)
        {
            recipe.SetSelectedAmountAndScale(SelectedAmount, Scale);
            RefreshProperties();
        }
        public ServingSizeCollection GetServingSizeCollection() { return recipe.GetServingSizeCollection(); }
        public Amount GetNewDefaultAmount() { return recipe.GetNewDefaultAmount(); }
        public Amount GetAmountSelected() { return recipe.GetAmountSelected(); }
        public Amount GetTotalAmount() { return recipe.GetTotalAmount(); }
        public void OnAmountConversionChanged()
        {
            recipe.OnAmountConversionChanged();
            RefreshProperties();
        }


        public void SelectIngredient(IngredientVM ingredient)
        {
            foreach (var ing in Ingredients)
                ing.IsSelected = (ing == ingredient);
        }

        private void PopulateIngredients()
        {
            var ingredients = new ObservableCollection<IngredientVM>();
            foreach (var ing in recipe.GetIngredients())
                ingredients.Add(new IngredientVM(ing));
            this.Ingredients = ingredients;
            NotifyPropertyChanged("Ingredients");
        }

        //public void RefreshIngredients()
        //{
        //    foreach (var prop in properties)
        //        prop.Refresh();
        //}

        private void PopulateProperties()
        {
            var properties = new ObservableCollection<PropertyVM>();
            foreach (var prop in UserSettings.Current.SelectedProperties)
                properties.Add(new PropertyRecipeVM(prop, recipe));
            this.Properties = properties;
        }

        public void RefreshProperties()
        {
            foreach (var prop in properties)
                prop.Refresh();
        }

        private ObservableCollection<PropertyVM> properties;
        public ObservableCollection<PropertyVM> Properties
        {
            get { return properties; }
            set
            {
                if (value != properties)
                {
                    properties = value;
                    NotifyPropertyChanged("Properties");
                }
            }
        }

        public void CommitNewItem()
        {

            if (!recipe.IsNewItem) throw new ArgumentOutOfRangeException("Item is not new");

            recipe.Save();
			#if WINDOWS_PHONE
			PictureCache.Current.UpdateNewItemWithID(recipe.GetItemId());
			#endif

            MessageQueue.Push(new Messages.RecipeUpdatedMessage(recipe)); // do this in model?
			//#if !DEBUG
            Navigate.BackAfterSubmit();
			//#endif

        }

        //public void DeleteFromDB()
        //{
        //    recipe.DeleteFromDB();
        //    Navigate.Back();
        //}

        public void AddServingSize()
        {
            AmountCollectionVM.AddServingSize();
        }

        public void StartPicture()
        {
			#if WINDOWS_PHONE
            FoodItem item = recipe;
            Navigate.ToCamera(PictureCache.Current.NextFilename(item.GetItemId()), item.SourceID);
			#endif
        }


        public bool ShouldSaveNewEntry(Entry entry)
        {
            recipe.AddIngredient(entry.Item);
            PopulateIngredients();
            return false;
        }
    }
#endif

}