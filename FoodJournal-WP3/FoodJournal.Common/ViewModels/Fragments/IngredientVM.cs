using FoodJournal.Model;
using FoodJournal.Values;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.Messages;
using FoodJournal.AppModel;
using FoodJournal.Runtime;
using FoodJournal.ViewModels.Fragments;
using FoodJournal.AppModel.UI;

namespace FoodJournal.ViewModels
{

#if false
    public class IngredientVM : VMBase
    {

        private Ingredient ingredient;
        public AmountCollectionVM AmountCollectionVM { get; set; }

        public IngredientVM(Ingredient ingredient)
        {
            this.ingredient = ingredient;
            AmountCollectionVM = new AmountCollectionVM(ingredient);
        }

        public string Text { get { return ingredient.Item.Text; } }
        public string Amount { get { return ingredient.TotalAmount.ToString(true); } }
        public string Summary { get { return ingredient.Summary; } }

        private bool isSelected;
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("NotSelectedVisibility"); NotifyPropertyChanged("SelectedVisibility"); } }
        public Visibility NotSelectedVisibility { get { return isSelected ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility SelectedVisibility { get { return isSelected ? Visibility.Visible : Visibility.Collapsed; } }

        //public List<object> ContextMenuItems { get { return new List<object>() { AppResources.Delete }; } }

        public bool IsForIngredient(Ingredient ingredient) { return this.ingredient == ingredient; }

        public void DeleteIngredient() { ingredient.DeleteFromDB(); }

        //public void NotifyIfChanged()
        //{

        //    if (_entryLastChanged == _entry.LastChanged) return;

        //    MessageQueue.Push(new Messages.EntryUpdatedMessage(_entry as Entry));

        //    NotifyPropertyChanged("ItemText");
        //    NotifyPropertyChanged("Summary");

        //    _entryLastChanged = _entry.LastChanged;

        //}


    }
#endif

}