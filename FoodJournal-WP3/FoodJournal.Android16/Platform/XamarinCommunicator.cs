#if false
using System;
using Com.Example.Testlib.Adapters;
using System.Collections.Generic;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Model;
using FoodJournal.ViewModels;

namespace FoodJournal.Android15
{

	public class PeriodInfoX : PeriodInfo {
		public override void SetNote (string p0){Cache.SetPeriodNote (DateTime.Now.Date, (Values.Period)Period, p0); this.Note = p0;}
		public override void DeleteEntry (string p0)
		{
			var entries = Cache.GetEntryCache (DateTime.Now.Date);
			entries [Period].RemoveAll (e => e.Text == p0);
		}
	}

	public class EntryInfoX : EntryInfo
	{

		private EntryDetailVM vm;

		public EntryInfoX(EntryDetailVM vm){this.vm = vm;}
		public override string Text {get {return vm.Text;}set {vm.Text = value;}}

		
	}


	public class XamarinCommunicator : Java.Lang.Object, IXamarinCommunicator
	{
		public XamarinCommunicator (){}

		public System.Collections.Generic.IList<Period> GetDisplayPeriods (Java.Util.Date p0)
		{
			var result = new List<Period> ();
			foreach(var Period in UserSettings.Current.Meals)
				result.Add(new Period(){Id=(int)Period, Text = Strings.FromEnum (Period)});

			return result;
		}

		public PeriodInfo GetPeriodInfo (Java.Util.Date p0, int p1)
		{
			var p = new PeriodInfoX ();

			var entries = Cache.GetEntryCache (DateTime.Now.Date);

			p.ShowEmptyListScreen = false;
			p.ShowNote = true;
			p.ShowTime = true;
			p.ShowTotals = true;

			p.Note = "Some note";
			p.Time = "10:26 pm";
			p.TotalLabel = "Total Calories";
			p.TotalAmount = Cache.GetPeriodPropertyValue (DateTime.Now.Date, (Values.Period)p1, Values.StandardProperty.Calories).ToString (true);

			p.Entries = new List<EntryTile> ();
			foreach (var e in entries[p1]) {
				var t = new EntryTile () { Text = e.Text, Amount=e.TotalAmountText };
				t.Properties = new List<PropertyTile> ();
				foreach(var prop in UserSettings.Current.SelectedProperties)
					t.Properties.Add(new PropertyTile() {Text=prop.FullCapitalizedText, Amount = e.GetPropertyValue(prop).ValueString()});
				p.Entries.Add (t);
			}

			return p;
		}

		public EntryInfo GetEntry (Java.Util.Date p0, int p1, string p2)
		{
			EntryInfo result = new EntryInfo ();
			result.Text = p2;
			return result;
		}
	}
}

#endif