using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using FoodJournal.Values;
using FoodJournal.Runtime;
using FoodJournal.Extensions;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;
using FoodJournal.Resources;

namespace FoodJournal.Android15.Adapters
{
	public class PeriodViewPagerAdapter : FragmentPagerAdapter, ISwipingPagerAdapter<Period>
	{

		private DateTime date;
		private Dictionary<Period, PeriodFragment> fragments = new Dictionary<Period, PeriodFragment> ();
		private List<Period> list;

		public int GetIdFromItem (Period item)
		{
			try {
				return list.IndexOf (item);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
				return 0;
			}
		}

		public Period GetItemFromId (int id)
		{
			return list [id];
		}

		public int NextItemId (bool inc, int currentid)
		{
			if (inc)
				return (currentid + 1) % this.Count;
			else
				return (currentid + this.Count - 1) % this.Count;
		}

		public PeriodViewPagerAdapter (DateTime date, FragmentManager fm)
			: base (fm)
		{

			this.date = date;

			list = UserSettings.Current.Meals;
			if (!list.Contains (Navigate.selectedPeriod)) {
				list = new List<Period> ();
				list.AddRange (UserSettings.Current.Meals);
				list.Add (Navigate.selectedPeriod);
			}

			int i = 0;
			foreach (Period p in list) {
				if (p == Navigate.selectedPeriod)
					i++;
			}

		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			SessionLog.Debug (string.Format ("Page Title: {0}", list [position]));
			return new Java.Lang.String (Strings.FromEnum (list [position]));
		}

		public string GetPageSubTitle(int position)
		{
			return date == DateTime.Now.Date ? AppResources.Today : date.ToShortDateString();
		}

		public override Fragment GetItem (int position)
		{
			Period period = list [position];

			if (fragments.ContainsKey (period))
				return fragments [period];

			SessionLog.Debug (string.Format ("New Fragment: {0}", list [position]));
			// getItem is called to instantiate the fragment for the given page.
			// Return a DummySectionFragment (defined as a static inner class
			// below) with the page number as its lone argument.
			PeriodFragment fragment = PeriodFragment.newInstance (date, list [position]);
			fragments.Add (period, fragment);

			//Bundle args = new Bundle();
			//args.putInt(DummySectionFragment.ARG_SECTION_NUMBER, position + 1);
			//fragment.setArguments(args);
			return fragment;
		}

		public override int Count {
			get {
				return list.Count;
			}
		}

	}
}