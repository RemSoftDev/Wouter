using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.Values;
using FoodJournal;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using FoodJournal.Messages;
using FoodJournal.ResourceData;
using FoodJournal.AppModel;
using FoodJournal.AppModel.UI;

namespace FoodJournal.ViewModels
{
#if false
    public class RecipesVM : VMBase
    {

        public ObservableCollection<SearchResultRecipeVM> Recipes { get; private set; }

        public Visibility NoEntriesVisibility { get { return Recipes.Count == 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public RecipesVM()
        {
            this.Recipes = new ObservableCollection<SearchResultRecipeVM>();

#if DEBUG
            Recipe newRecipe = new Recipe("The kale dish", "");
            newRecipe.CookTime = "25 min";
            newRecipe.AddIngredient(new FoodItem("Kale", ""));
            newRecipe.AddIngredient(new FoodItem("Potatoes", ""));
            newRecipe.AddIngredient(new FoodItem("Bacon", ""));
            Recipes.Add(new SearchResultRecipeVM(newRecipe));

             newRecipe = new Recipe("Lamb Biryani", "");
            newRecipe.CookTime = "45 min";
            newRecipe.AddIngredient(new FoodItem("Lamb", ""));
            newRecipe.AddIngredient(new FoodItem("Potatoes", ""));
            newRecipe.AddIngredient(new FoodItem("Peas", ""));
            newRecipe.AddIngredient(new FoodItem("Spices", ""));
            newRecipe.AddIngredient(new FoodItem("Rice", ""));
            Recipes.Add(new SearchResultRecipeVM(newRecipe));

             newRecipe = new Recipe("Mushroom risotti", "");
            newRecipe.CookTime = "45 min";
            newRecipe.AddIngredient(new FoodItem("Risotto rice", ""));
            newRecipe.AddIngredient(new FoodItem("Parmesan", ""));
            newRecipe.AddIngredient(new FoodItem("Spinach", ""));
            Recipes.Add(new SearchResultRecipeVM(newRecipe));
#endif
        }

        public void Requery()
        {
            var recipes = new ObservableCollection<SearchResultRecipeVM>();

            foreach (Recipe recipe in Cache.AllItems.Values.Where(i => i is Recipe))
                recipes.Add(new SearchResultRecipeVM(recipe));
            this.Recipes = recipes;

            NotifyPropertyChanged("Recipes");
        }


    }
#endif

}
