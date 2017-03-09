using FoodJournal.AppModel;
using FoodJournal.Model.Data;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Model
{

	#region Class FoodItemComparer

	// used for sorting Items alphabetically
	public class FoodItemComparer : IComparer<FoodItem>, IEqualityComparer<FoodItem>
	{

		public static FoodItemComparer instance = new FoodItemComparer ();

		public int Compare (FoodItem x, FoodItem y)
		{
			return x.Text.CompareTo (y.Text);
		}

		public bool Equals (FoodItem x, FoodItem y)
		{
			return x.Text.Equals (y.Text);
		}

		public int GetHashCode (FoodItem obj)
		{
			return obj.Text.GetHashCode ();
		}

	}

	#endregion

	public class FoodItem : ModelObject, IServingSizeTarget, IPropertyTarget
	{

		// 100 gr
		private static Amount nutritionScale;

		public static Amount NutritionScale {
			get {
				if (!nutritionScale.IsValid)
					nutritionScale = Amount.FromGram (100);
				return nutritionScale;
			}
		}

		public string TextDB { get; set; }

		public string DescriptionDB { get; set; }

		public string Culture { get; set; }

		public string SourceID { get; set; }

		public string CommonMeal { get; set; }

		public string NutritionDB { get; set; }

		public string ServingSizesDB { get; set; }

		public string LastAmountDB { get; set; }

		public void ResetServingsizes() { _servingSizes = null; }
		public void ResetNutrition() { _values = null; }

		private ServingSizeCollection _servingSizes;
		private PropertyDictionary _values;

		public List<Entry> NotifyEntries = new List<Entry> ();

		public bool IsShallowCopy;
		// simplecopy items are instantiated from the journal day, and have a shallow set of data. Data should usually be replace with data from AllItems when merging

		public FoodItem (String Text, bool IsShallowCopy)
		{
			TextDB = Text;
			this.IsShallowCopy = IsShallowCopy;
		}

		public void CopyDetailsFrom(FoodItem source)
		{
			SourceID = source.SourceID;
			NutritionDB = source.NutritionDB;
			ServingSizesDB = source.ServingSizesDB; // TODO: Actually merge the serving sizes, rather than override
			LastAmountDB = source.LastAmountDB;
			ResetServingsizes ();
			ResetNutrition ();
		}

		// this object saves itself any time a property changed
		protected override void OnObjectPropertyChanged (string propertyName)
		{
			SaveIfNotNew ();
		}

		public bool IsNewItem { get { return !Cache.ContainsItem (Text); } }

		private bool Saving = false;

		protected override void SaveIfNotNew ()
		{
			if (!IsNewItem)
				Save ();
		}

		public override void Save ()
		{

			if (Saving)
				return;

			Saving = true;

			try {
				
				//if (IsNewItem)
				Cache.MergeItem (this);

				foreach (Entry entry in NotifyEntries)
					entry.Save (); // also resets cache, but has the added benefit that localDB is saved
				//Cache.OnEntryChanged(entry);

				//FoodJournalBackup.SaveItem(this);

			} finally {
				Saving = false;
			}
			//FoodJournalNoSQL.StartSave();
		}

		private string Trim (string value)
		{
			return value == null ? "" : value.Trim ();
		}

		public string Text {
			get { return TextDB; }
			set {
				TextDB = Trim (value);
				OnPropertyChanged ("Text");
			}
		}

		public string Description {
			get { return DescriptionDB; }
			set {
				DescriptionDB = value;
				OnPropertyChanged ("Description");
			}
		}

		public Amount LastAmount {
			get { return (Amount)LastAmountDB; }
			set {
				LastAmountDB = value.ToStorageString ();
				SaveIfNotNew ();
			}
		}
		// (dont update lastchanged: OnPropertyChanged("LastAmount"); } }
		public ServingSizeCollection ServingSizes {
			get {
				if (_servingSizes == null)
					_servingSizes = new ServingSizeCollection (this, this);
				return _servingSizes;
			}
		}

		public PropertyDictionary Values {
			get {
				if (_values == null)
					_values = new PropertyDictionary (this, this);
				return _values;
			}
		}

		public Amount NewEntryAmount {
			get {
				if (LastAmount.IsValid)
					return LastAmount;
				if (ServingSizes.FirstAmount.IsValid)
					return ServingSizes.FirstAmount;
				return Amount.DefaultAmount;
			}
		}

	}
}
