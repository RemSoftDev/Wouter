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

    public class PropertyVM:VMBase
    {
        protected PropertyVM() { }
        public void Refresh() { NotifyPropertyChanged("Value"); }
    }

    public class PropertyEntryVM : PropertyVM
    {

        private readonly Property property;
        private readonly Entry entry;
		private bool is100G;

		public PropertyEntryVM(Property property, Entry entry, bool Is100G)
        {
            this.property = property;
            this.entry = entry;
			this.is100G = Is100G;
        }

		public bool Is100G{get{ return is100G; } set { is100G = value; NotifyPropertyChanged ("Value");}}

        public string Text { get { return property.FullText; } }

        public string Value
        {
            get
            {
				if (is100G)
					return entry.GetPropertyStandard (property).ValueString ();
                if (entry.PivotScale <= 0)
                    return AppResources.EnterWeight;
                return entry.GetPropertyValue(property).ValueString();
            }
            set
            {
				if (is100G) {
					Single val = Floats.ParseUI(value, float.NaN);
					entry.Item.Values[property] = val;
				} else if (entry.PivotScale > 0)
                    if (value != entry.GetPropertyValue(property).ValueString())
                    {
                        // TODO: move this into the Model? (so summary update events are triggered?)
                        Single val = Floats.ParseUI(value, float.NaN);
                        entry.Item.Values[property] = val / entry.PivotScale;
                    }
                NotifyPropertyChanged("Value");
            }
        }


    }

#if false
    public class PropertyRecipeVM : PropertyVM
    {

        private readonly Property property;
        private readonly Recipe Recipe;

        public PropertyRecipeVM(Property property, Recipe Recipe)
        {
            this.property = property;
            this.Recipe = Recipe;
        }

        public string Text { get { return property.FullText; } }

        public string Value
        {
            get
            {
                if (Recipe.PivotScale <= 0)
                    return AppResources.enter_serving_weight;
                return Recipe.GetPropertyValue(property).ValueString();
            }
            set
            {
                if (Recipe.PivotScale > 0)
                    if (value != Recipe.GetPropertyValue(property).ValueString())
                    {
                        // TODO: move this into the Model? (so summary update events are triggered?)
                        Single val = Floats.ParseUI(value, float.NaN);
                        Recipe.Values[property] = val / Recipe.PivotScale;
                    }
                NotifyPropertyChanged("Value");
            }
        }


    }
#endif

}
