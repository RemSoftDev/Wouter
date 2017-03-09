using FoodJournal.AppModel;
using FoodJournal.AppModel.SQLite;
using FoodJournal.Model.Data;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodJournal.Logging;
using FoodJournal.Resources;

namespace FoodJournal.Model
{

	public class Entry : ModelObject
	{

		public DateTime Date { get; set; }
		public Period Period { get; set; }

		//public String Text { get; set; }
		private string text;
		public String Text { get { return text; } set { text = value; 			
				// 1/14/16 -> fixing how item text updates
				if (item == null) return;
				item = Cache.GetRenamedItem (item, value, this);
				SaveIfNotNew ();
			}}

		private FoodItem item;

		public String AmountSelectedDB { get; set; }
		public Single AmountScaleDB { get; set; }

		public bool IsNewEntry { get { return !Cache.ContainsEntry(this); } }

		public Entry(DateTime date, Period period, String item)
		{
			Date = date;
			Period = period;
			Text = item;
			AmountScaleDB = 1;
			//AmountSelectedDB = item.NewEntryAmount.ToStorageString();
			//this.item = item;
			//item.NotifyEntries.Add(this);
			//IsNewEntry = true;
		}

		public Entry(DateTime date, Period period, FoodItem item)
		{
			Date = date;
			Period = period;
			Text = item.Text;
			AmountScaleDB = 1;
			AmountSelectedDB = item.NewEntryAmount.ToStorageString();
			this.item = item;
			item.NotifyEntries.Add(this);
		}

		public override DateTime LastChanged { get { return base.LastChanged > Item.LastChanged ? base.LastChanged : Item.LastChanged; } }

		// this object saves itself any time a property changed, implemented in "SetAmountAndScale"

		protected override void SaveIfNotNew() { if (!IsNewEntry) Save(); }

		private bool saving = false;

		public override void Save ()
		{

			if (saving)
				return;

			saving = true;

			try {

				if (IsNewEntry)
					Cache.AddEntry(this);
				else
					Cache.OnEntryChanged(this);

				if (item != null) item.Save();

				//				FoodJournalBackup.SaveEntry (this);
				FoodJournalNoSQL.StartSaveDay(Date, Period);

			} finally {
				saving = false;
			}
		}

		public void Delete()
		{
			//			FoodJournalBackup.DeleteEntry(this);
			//FoodJournalDB.DeleteEntryDO(data);
			Item.NotifyEntries.Remove(this);
			Cache.RemoveEntry(this);
			FoodJournalNoSQL.StartSaveDay(Date, Period);
		}

		public void ResetItem()
		{
			if (item != null) item.NotifyEntries.Remove(this);
			item = null;
		}

		public FoodItem Item
		{
			get
			{
				if (item == null)
				{
					item = Cache.GetItem(Text);
					item.NotifyEntries.Add(this);
				}
				return item;
			}
		}

		/// <summary>
		/// EntryText contains Selected Amount
		/// </summary>
		public string EntryText { get { if (TotalAmount.IsZero) return Text; return TotalAmount.AppendItemText(Text.ToLower()); } }
		public string TotalAmountText { get { if (TotalAmount.IsZero) return null; return TotalAmount.ToString (true);}}
		public string TotalAmountTextWithGram { get { if (TotalAmount.IsZero) return null; 
				Single gram = PivotScale * 100;
				if (gram >0)
					return TotalAmount.ToString (true) + string.Format(" ({0} {1})", FoodJournal.Parsing.Floats.ToUIString(gram), AppResources.Unit_Gram);	
				return TotalAmount.ToString (true);}}
		public Amount TotalAmount { get { return AmountSelected * AmountScale; } }
		// calculates nutrition value given total amount
		public Amount GetPropertyValue(Property property) { return Amount.FromProperty(Item.Values[property] * PivotScale, property); }
		public Amount GetPropertyStandard(Property property) {return Amount.FromProperty(Item.Values [property], property);}
		public string Summary { get { var sum = NutritionSummary; return sum == null ? TotalAmount.ToString(true) : TotalAmount.ToString(true) + ": " + sum; } }
		public Amount AmountSelected { get { return (Amount)AmountSelectedDB; } set { SetAmountAndScale(value, 1); } }
		public Single AmountScale { get { return AmountScaleDB; } set { SetAmountAndScale(AmountSelected, value); } }

		// private:
		private Single pivotScale = -1;
		public Single PivotScale { get { if (pivotScale == -1) pivotScale = Item.ServingSizes.CalculateScale(TotalAmount, FoodItem.NutritionScale); return pivotScale; } }

		public void OnAmountConversionChanged() { pivotScale = -1; }

		public void SetAmountAndScale(Amount amount, Single scale)
		{

			AmountSelectedDB = amount.ToStorageString();
			AmountScaleDB = scale;

			OnAmountConversionChanged();

			SaveIfNotNew();

			Item.LastAmount = TotalAmount;

			OnPropertyChanged("AmountSelected");
			OnPropertyChanged("AmountScale");
			OnPropertyChanged("TotalAmount");
			OnPropertyChanged("EntryText");
			OnPropertyChanged("Summary");

		}

		public string NutritionSummary
		{
			get
			{
				if (TotalAmount.IsZero)
					return null;//AppResources.HowMuch;
				if (PivotScale == 0)
					return null;//AppResources.CantCalculateNutrition;
				var sb = new System.Text.StringBuilder();
				Amount amt;
				string value;
				foreach (var prop in UserSettings.Current.SelectedProperties)
				{
					amt = GetPropertyValue(prop);
					if (!amt.IsAlmostZero)
					{
						value = amt.ToString(true);
						if (sb.Length > 0) sb.Append(AppResources.CommaListSeparater);
						sb.Append(value);
					}
				}
				if (sb.Length == 0) return AppResources.NoNutritionInfo;
				return sb.ToString();
			}
		}

	}
}
