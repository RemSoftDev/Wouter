using FoodJournal.Logging;
using FoodJournal.Extensions;
using FoodJournal.Values;
using FoodJournal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FoodJournal.Model.Data;
using System.Threading;

namespace FoodJournal.Model
{

	public struct RecentItem
	{
		public DateTime Date;
		public string Text;
		private FoodItem fooditem;
		public FoodItem FlatItem { get { if (fooditem == null) fooditem = new FoodItem (Text, true); return fooditem; } }	
		public FoodItem FullItem { get { 
				Cache.GetEntryCache(Date); // make sure the date is loaded, so that the item has at least shallow data
				fooditem=Cache.GetItem(Text);
				return fooditem; } }
	}

	public class Cache
	{

		public static readonly AutoResetEvent ISOSync = new AutoResetEvent(true);

		public const int MAXCACHEDAYS = 30;
		public const int PeriodCount = (int)Period.SnackMidnight;

		//private static bool itemCacheLoading;
		//private static bool allDaysLoaded = false;
		private static Dictionary<DateTime, Day> dateCache = new Dictionary<DateTime, Day>();
		private static Dictionary<String, FoodItem> itemCache;
		private static List<RecentItem>[] recentCache;

		public static bool AllItemsLoaded = false;

		#if DEBUG
		public static void ResetForScreenshots()
		{
			//itemCacheLoading = false;
			//allDaysLoaded = false;
			dateCache = new Dictionary<DateTime, Day>();
			itemCache = null;
			recentCache = null;

			for (int i = 0; i < 8; i++) {

				var items = Cache.GetEntryCache (DateTime.Now.AddDays(-i).Date);
				// delete all item to fix culture related fixes
				foreach (var item in items) {
					while (item.Count > 0) {
						item [0].Delete ();
					}
				}
			}
		}
		#endif

		public static List<RecentItem> GetRecentCache(Period period)
		{
			if (recentCache == null)
			{
				recentCache = new List<RecentItem>[PeriodCount + 1];
				foreach (Period p in PeriodList.All)
					recentCache[(int)p] = new List<RecentItem>();
				FoodJournalNoSQL.LoadRecent();
			}
			return recentCache[(int)period];
		}

		private struct Day
		{

			public class PeriodInfo
			{
				public DateTime LastChangeStored;
				public String Note;
				public String Time;
			}

			private DateTime date;

			public List<Entry>[] PeriodEntries;
			public Dictionary<Property, Amount>[] PeriodProperties;
			public PeriodInfo[] Periods;

			public Day(DateTime date)
			{

				this.date = date;

				// initialize storage
				PeriodProperties = new Dictionary<Property, Amount>[PeriodCount + 1];
				PeriodEntries = new List<Entry>[PeriodCount + 1];
				Periods = new PeriodInfo[PeriodCount + 1];

				for (int i = 0; i <= Cache.PeriodCount; i++)
					PeriodEntries[i] = new List<Entry>();

				#if LegacyDB
				if (!loadEntries) return;

				foreach (Entry entry in FoodJournalDB.SelectEntriesByDateRangeSorted(date, date))
				PeriodEntries[(int)entry.Period].Add(entry);
				#endif

			}

			public Amount GetPeriodPropertyValue(Period period, Property property)
			{
				Amount value = Amount.Zero;
				try
				{
					Dictionary<Property, Amount> set = PeriodProperties[(int)period];
					if (set == null)
					{
						set = new Dictionary<Property, Amount>();
						PeriodProperties[(int)period] = set;
					}
					if (!set.TryGetValue(property, out value))
					{
						// cache miss, calculate the value
						value = Amount.Zero;
						foreach (Entry entry in PeriodEntries[(int)period])
							value += entry.GetPropertyValue(property);
						set.Add(property, value);
					}
				}
				catch (Exception ex) { LittleWatson.ReportException(ex); }
				return value;
			}

			public Amount GetPropertyValue(Property property)
			{
				Amount total = Amount.Zero;
				//for (Period p = Period.Breakfast; p <= Period.Snack; p++)
				foreach (Period p in PeriodList.All)
					total += GetPeriodPropertyValue(p, property);
				return total;
			}

		}

		private static Day GetDayObject(DateTime date)
		{
			Day day;
			if (dateCache.TryGetValue(date, out day)) return day;

			try
			{

				day = new Day(date);

				Day day2;
				if (dateCache.TryGetValue(date, out day2)) return day2;

				dateCache.Add(date, day);
				FoodJournalNoSQL.LoadDay(date);

			}
			catch (Exception ex) { LittleWatson.ReportException(ex); } // we'll try again if the Day Constructor Throws (from online crash list)
			return day;
		}

		/// <summary>
		/// Gets the Entry Cache for the specified date. This method fills the cache at first invocation
		/// </summary>
		public static List<Entry>[] GetEntryCache(DateTime date)
		{
			return GetDayObject(date).PeriodEntries;
		}

		public static bool IsEntryCacheLoaded(DateTime date)
		{
			return (dateCache.ContainsKey(date));
		}

		private static void resetProperties(Entry entry)
		{
			dateCache[entry.Date].PeriodProperties[(int)entry.Period] = null;
		}

		public static void OnEntryChanged(Entry entry)
		{
			if (!dateCache.ContainsKey(entry.Date)) return;
			resetProperties(entry);
		}

		public static bool ContainsEntry(Entry entry)
		{
			GetDayObject(entry.Date);
			foreach (var option in dateCache[entry.Date].PeriodEntries[(int)entry.Period])
				if (string.Compare(option.Text, entry.Text, StringComparison.CurrentCultureIgnoreCase) == 0)
					return true;
			return false;           
		}

		public static void AddEntry(Entry entry)
		{
			GetDayObject(entry.Date);
			//if (!dateCache.ContainsKey(entry.Date)) return;
			resetProperties(entry);

			var list = dateCache [entry.Date].PeriodEntries [(int)entry.Period];

			// first remove the entry if it already exists
			int i = 0;
			while (i < list.Count)
				if (string.Compare(list[i].Text, entry.Text, StringComparison.CurrentCultureIgnoreCase) == 0) list.RemoveAt(i); else i++;

			// add it
			list.Add(entry);
		}

		public static void RemoveEntry(Entry entry)
		{
			if (!dateCache.ContainsKey(entry.Date)) return;
			resetProperties(entry);
			dateCache[entry.Date].PeriodEntries[(int)entry.Period].Remove(entry);
		}

		public static Amount GetPropertyTotal(DateTime date, Property property)
		{
			return GetDayObject(date).GetPropertyValue(property);
		}

		public static Amount GetPeriodPropertyValue(DateTime date, Period period, Property property)
		{
			return GetDayObject(date).GetPeriodPropertyValue(period, property);
		}

		public static List<Period> GetDatePeriods(DateTime date)
		{
			var cache = Cache.GetEntryCache(date);
			// Show selected periods, and Periods with entries that are no longer selected
			List<Period> list = new List<Period> ();
			foreach (Period p in PeriodList.All)
				if (UserSettings.Current.Meals.Contains (p) || cache [(int)p].Count > 0)
					list.Add (p);
			return list;
		}

		public static FoodItem GetItem(string Text)
		{
			if (itemCache == null) itemCache = new Dictionary<string, FoodItem>();
			FoodItem result;
			if (itemCache.TryGetValue(Text, out result)) return result;
			result = new FoodItem(Text, true);
			itemCache.Add(Text, result);
			return result;
		}


		//GetRenamedItem is called when the text is changed on an entry.
		// if the item is used by multiple entries, split it up into 2,
		// if there is already an item in the cache by the new name, update thatone, and return it
		// otherwise, update the item's text, and return it.
		public static FoodItem GetRenamedItem(FoodItem source, string Text, Entry entry)
		{

			if (source.Text == Text)
				return source;

			Debug.Assert (source.Text != Text);
			Debug.Assert (source.NotifyEntries.Contains (entry));

			FoodItem existing = null;
			if (itemCache.TryGetValue (Text, out existing)) {
				existing.CopyDetailsFrom (source);
				source.NotifyEntries.Remove (entry);
				if (source.NotifyEntries.Count == 0)
					itemCache.Remove (source.Text);
				existing.NotifyEntries.Add (entry);
				return existing;
			}

			if (source.NotifyEntries.Count > 1)
			{
				FoodItem newitem = GetItem(Text); // adds it to the cache
				newitem.CopyDetailsFrom (source);
				source.NotifyEntries.Remove (entry);
				newitem.NotifyEntries.Add (entry);
				return newitem;
			}

			// rename existing item, in the cache and in the item
			itemCache.Remove(source.Text);
			source.Text = Text;
			itemCache.Add (Text, source);

			return source;
		}

		public static void MergeMeal(DateTime date, Period period, List<Entry> entries, DateTime modified, String Note, String Time)
		{

			var day = GetDayObject(date);
			bool MergeIn = day.Periods[(int)period] == null || day.Periods[(int)period].LastChangeStored < modified;

			Debug.WriteLine (string.Format ("Merging meal {0} {1} {2}", MergeIn, date, period));

			if (MergeIn)
			{
				day.Periods[(int)period] = new Day.PeriodInfo() { LastChangeStored = modified, Note = Note, Time = Time };
				day.PeriodEntries[(int)period] = entries;

				// TODO: would be good if we can force the current active view to refresh if we can (and needed)
			}

		}

		public static string GetPeriodTime(DateTime date, Period period)
		{
			if (!IsEntryCacheLoaded (date)) return null;
			var day = GetDayObject(date);
			if (day.Periods[(int)period] == null) return null;
			return day.Periods[(int)period].Time;
		}

		public static void SetPeriodTime(DateTime date, Period period, string time)
		{
			var day = GetDayObject(date);
			if (day.Periods[(int)period] == null)
				day.Periods[(int)period] = new Day.PeriodInfo();
			day.Periods[(int)period].Time = time;

			FoodJournalNoSQL.StartSaveDay(date, period);
		}

		public static string GetPeriodNote(DateTime date, Period period)
		{
			if (!IsEntryCacheLoaded (date)) return null;
			var day = GetDayObject(date);
			if (day.Periods[(int)period] == null) return null;
			return day.Periods[(int)period].Note;
		}

		public static void SetPeriodNote(DateTime date, Period period, string note)
		{
			var day = GetDayObject(date);
			if (day.Periods[(int)period] == null)
				day.Periods[(int)period] = new Day.PeriodInfo();
			day.Periods[(int)period].Note = note;

			FoodJournalNoSQL.StartSaveDay(date, period);
		}

		public static bool ShouldSavePeriod(DateTime date, Period period)
		{
			if (!IsEntryCacheLoaded (date)) return false;
			var day = GetDayObject(date);
			if (day.PeriodEntries [(int)period].Count > 0)
				return true;
			return day.Periods[(int)period] != null;
		}

		public static bool ContainsItem(string Text)
		{
			return (itemCache != null && itemCache.ContainsKey(Text));
		}

		public static void MergeItem(FoodItem item)
		{
			if (itemCache == null) itemCache = new Dictionary<string, FoodItem>();

			FoodItem existing = item;
			if (!itemCache.TryGetValue(item.Text, out existing))
				itemCache.Add(item.Text, item);
			else
				if (item != existing)
				{

					if (item.IsShallowCopy)
					{
						#if !DEBUG
						//todo: consider merging the serving size of shallow items; should they always exist in the item 
						//(imagine an old item with that selection, what happens to it when the serving size gets deleted later?)
						#endif

						// for shallow items cache contents take precedence
						if (existing.Culture == null) existing.Culture = item.Culture;
						if (existing.DescriptionDB == null) existing.DescriptionDB = item.DescriptionDB;
						if (existing.LastAmountDB == null) existing.LastAmountDB = item.LastAmountDB;
						if (existing.NutritionDB == null) { existing.NutritionDB = item.NutritionDB;existing.ResetNutrition (); }
						if (existing.ServingSizesDB == null) { existing.ServingSizesDB = item.ServingSizesDB; existing.ResetServingsizes(); }
						if (existing.SourceID == null) existing.SourceID = item.SourceID;

					}
					else
					{

						// deep items take precedence when merging in
						if (item.Culture != null) existing.Culture = item.Culture;
						if (item.DescriptionDB != null) existing.DescriptionDB = item.DescriptionDB;
						if (item.LastAmountDB != null) existing.LastAmountDB = item.LastAmountDB; 
						if (item.NutritionDB != null) { existing.NutritionDB = item.NutritionDB; existing.ResetNutrition (); }
						if (item.ServingSizesDB != null) { existing.ServingSizesDB = item.ServingSizesDB; existing.ResetServingsizes(); }
						if (item.SourceID != null) existing.SourceID = item.SourceID;

					}

					// if we're trying to add an item to the cache that was already there, make sure all entries now point to the existing item
					if (item.NotifyEntries.Count > 0)
						foreach (Entry entry in item.NotifyEntries.ToArray())
							entry.ResetItem();
				}

		}

		public static IEnumerable<FoodItem> AllValidItems()
		{
			if (itemCache != null)
				foreach (FoodItem result in itemCache.Values.ToList()) // tolist to protect from changing in other thread
					//                    if (result.Dates.Count > 0)
					yield return result;
		}

		#if LegacyDB

		private static Dictionary<int, FoodItem> AllItemsCache
		{
		get
		{
		if (itemCache == null) itemCache = new Dictionary<int, FoodItem>();
		return itemCache;
		}
		}

		private static int GetCacheID(FoodItemType type, int Id) { return (int)type * 10000 + Id; }
		public static FoodItem GetItemById(FoodItemType type, int ItemId) { FoodItem result = null; AllItems.TryGetValue(GetCacheID(type, ItemId), out result); return result; }

		public static Dictionary<int, FoodItem> AllItems
		{
		get
		{
		if (itemCacheLoading) return new Dictionary<int, FoodItem>(); // another thread is asking for the list while the first thread still loading from the DB. return an empty list. this may result in a user not seeing desired results. TODO: consider forcing a requery when this happens
		if (itemCache == null)
		{

		itemCacheLoading = true;
		try
		{
		var cache = AllItemsCache;
		foreach (var item in FoodJournalDB.SelectAllFoodItems())
		cache.Add(GetCacheID(FoodItemType.Food, item.GetItemId()), item);
		foreach (var item in FoodJournalDB.SelectAllRecipes())
		cache.Add(GetCacheID(FoodItemType.Recipe, item.GetItemId()), item);
		}
		catch (Exception ex) { LittleWatson.ReportException(ex); }
		itemCacheLoading = false;
		}
		return itemCache; //.ToList<Item>(); (needs to be possible to add to this list, copy can be made locally
		}
		}

		public static void AddItem(FoodItem item)
		{
		if (itemCache == null) return;
		itemCache.Add(GetCacheID(item.FoodItemType, item.GetItemId()), item);
		}
		#endif

		//public static void RemoveItem(Item item)

	}
}
