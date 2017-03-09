using FoodJournal.AppModel;
using FoodJournal.Model;
using FoodJournal.Parsing;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodJournal.Resources;

namespace FoodJournal.ViewModels.Fragments
{

    public class GoalLineVM:VMBase
    {

		public readonly Property property;

		public GoalLineVM(Property property)
        {
            this.property = property;
        }

        public string Text { get { return property.FullText; } }

		private static string GoalString(Single value)
		{
			if (Single.IsNaN(value) || value == 0) return "";//AppResources.NoGoalSet;
			return value.ToUIString();
		}

        public string Value
        {
			get { return GoalString(UserSettings.Current.GetGoal(property)); }
			set { UserSettings.Current.SetGoal(property, Floats.ParseUI(value, Single.NaN)); }
        }


    }

}
