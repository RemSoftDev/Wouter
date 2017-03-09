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
using FoodJournal.AppModel.UI;
using FoodJournal.AppModel;
using FoodJournal.Runtime;
using FoodJournal.Resources;
using FoodJournal.ViewModels.Fragments;

namespace FoodJournal.ViewModels
{
    public class GoalsVM : VMBase
    {

        public ObservableCollection<GoalLineVM> Goals { get; private set; }

        public string Description { get { return UserSettings.Current.GoalDescription; } set { UserSettings.Current.GoalDescription = value; } }
        public string Date { get { return UserSettings.Current.GoalDate; } set { UserSettings.Current.GoalDate = value; } }

        public GoalsVM()
        {
            this.Goals = new ObservableCollection<GoalLineVM>();
            foreach (Property p in UserSettings.Current.SelectedProperties)
            {
                if (p.ID != "00")// avoid "None" property
                    Goals.Add(new GoalLineVM(p));
            }
        }

        public void DeleteGoal(GoalLineVM property)
        {
            Goals.Remove(property);
            UserSettings.Current.RemoveSelectedProperty(property.property);
        }

        public void AddGoal(Property property)
        {
            Goals.Add(new GoalLineVM(property));
            UserSettings.Current.AddSelectedProperty(property);
        }

        private bool HasGoal(Property goal)
        {
            foreach (var current in Goals)
                if (current.property == goal)
                    return true;
            return false;
        }

        public List<String> NewPropertyOptions
        {
            get
            {

                List<String> options = new List<String>();
                foreach (var value in Property.All())
                    if (!HasGoal(value))
                        options.Add(value.FullCapitalizedText);

                return options;
            }
        }

    }
}
