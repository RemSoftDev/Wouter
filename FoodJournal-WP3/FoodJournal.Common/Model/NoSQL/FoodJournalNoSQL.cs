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
using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;
using FoodJournal.AppModel.Data;
using FoodJournal.Parsing;
using FoodJournal.AppModel;

namespace FoodJournal.Model
{

	/// <summary>
	/// Here's how the syncing works:
	/// 
	/// Svc A (1st phone) ->  LocalDB  ->  Cache
	/// Svc B (2nd phone)
	/// 
	/// Scenario 1. 
	/// User Changes values in A => Values in LocalDB (T1), and SvcA
	/// Then opens phone B       => Phone B Loads T0, and merges in T1
	/// Makes changes in phone B => Phone B Stores T2 => SvcB
	/// Then back to phone A     => Phone A Loads T1, then Loads T2 form Svc B, Merges T2 into Cache, Saves to LocalDB and Svc A
	/// 
	/// because SvcB has T2, T2 take precedence on conflicts
	/// now if A was offline, and makes changes, then goes online, and reads from svc B, A (T3) wins
	/// 
	/// In order to make this work, we need to know the time the day was stored in the localDB, and in the svc
	/// 
	/// Can we make it such that remote always wins? No, then the user cannot delete an entry. Unless we keep track of which changesets we've already synced?
	/// The best way to make sure the latest edit is applied is by putting a modified datetime on every entry. Otherwise, you may end up in the following situation
	/// 
	/// Changed tracked only per date
	/// Phone A    Phone B
	/// Add A, B
	///             Add C, Modify A
	/// Modify B
	///    sync from B
	/// PROBLEM: Changes to A are gone
	/// 
	/// 
	/// Remote always wins
	/// Phone A    Phone B
	/// Add A, B
	///             Add C, Modify A
	/// Modify B
	///    sync from B
	/// PROBLEM: Changes to B are gone
	/// 
	/// Solution:
	/// Keep Change DateTime on per day level, but changes on individual rows can be tracked in memory
	/// This only works if syncing always happens. So. We'll just have to track modified datetime on each row that we want to merge. If we always want to have the latest changes, AND support offline working
	/// How critical is offline sync correctness, on individual line items (entries, items?)
	/// 
	/// Most likely, folks care about it on a per meal basis. The enter a meal at a time. Then they may open up the app on another device, and make some updates.
	/// They expect all of the updates to the meal to be represented on the other device, but most likely not a mix and match strategy, where one item is updated on one device, and another one on another device
	/// As long as they can recognize the consistent state for a meal correctly represented on the device they are looking at, they are likley ok
	/// 
	/// So, Modified/Changed tracking on a per meal basis?
	/// </summary>

	public class FoodJournalNoSQL
	{

		private static List<DateTime> SaveQueue = new List<DateTime> ();
		private static bool Saving = false;

		#region StartSave

		public static bool IsSaving ()
		{
			lock (SaveQueue) {
				return Saving;
			}
		}

		public static void StartSave ()
		{

			SessionLog.StartPerformance ("Save");

			lock (SaveQueue) {
				if (Saving)
					return;
				Saving = true;
			}

			// save after 0 seconds
			BackgroundTask.Start (0, () => {

				try {
					List<DateTime> queue = new List<DateTime> ();
					lock (SaveQueue) {
						queue.AddRange (SaveQueue);
						SaveQueue.Clear ();
					}

					foreach (var dt in queue)
						SaveDay (dt);

					if (queue.Count > 0)
						SaveRecent ();

					if (!Cache.AllItemsLoaded)
						LoadItems (true, null);

					SaveItems ();
				} finally {
					lock (SaveQueue) {
						Saving = false;
					}

					SessionLog.EndPerformance("Save");

					if (SaveQueue.Count > 0)
						StartSave ();
				}

			}
			);

		}

		public static void StartSaveDayFull (DateTime date)
		{

			lock (SaveQueue) {
				if (!SaveQueue.Contains (date))
					SaveQueue.Add (date);
			}

			
			foreach (Period period in PeriodList.All)
				if (Cache.ShouldSavePeriod (date, period))
					SyncRecent (date, period);

			StartSave ();

		}

		public static void StartSaveDay (DateTime date, Period period)
		{

			lock (SaveQueue) {
				if (!SaveQueue.Contains (date))
					SaveQueue.Add (date);
			}

			
			SyncRecent (date, period);

			StartSave ();

		}

		#endregion

		#region SyncRecent

		private static void SyncRecent (DateTime date, Period period)
		{

			List<RecentItem> recent = Cache.GetRecentCache (period);
			List<Entry> PeriodEntries = Cache.GetEntryCache (date) [(int)period];

			int pe = 0;
			int r = 0;
			while (r < recent.Count && recent [r].Date >= date) {

			
				while ((r < recent.Count && pe < PeriodEntries.Count && recent [r].Date == date && recent [r].Text != PeriodEntries [pe].Text)
				                   || (pe == PeriodEntries.Count && r < recent.Count && recent [r].Date == date))
					recent.RemoveAt (r);

				if (r < recent.Count && recent [r].Date >= date) {
					if (pe < PeriodEntries.Count && recent [r].Text == PeriodEntries [pe].Text)
						pe++;
					r++;
				}

			}

			for (; pe < PeriodEntries.Count; pe++)
				recent.Insert (r, new RecentItem () { Date = date, Text = PeriodEntries [pe].Text });

		}

		#endregion

		#region SaveDay

		private static void SaveDay (DateTime date)
		{
			try {

				var d = new Serializer ("Date", date.ToStorageStringDate ());

				List<Entry>[] PeriodEntries = Cache.GetEntryCache (date);

				foreach (Period period in PeriodList.All)
					if (Cache.ShouldSavePeriod (date, period)) {
						var p = d.Add ("Period", period.ToString ());
						p.Write ("LastChanged", DateTime.Now.ToStorageStringFull ());
						p.Write ("Note", Cache.GetPeriodNote (date, period));
						p.Write ("Time", Cache.GetPeriodTime (date, period));

						foreach (Entry entry in PeriodEntries[(int)period]) {
							FoodItem item = entry.Item;

#if DEBUG
                            if (item.ServingSizes.GetEquivalent(entry.AmountSelectedDB).ToStorageString() == "")
                                System.Diagnostics.Debugger.Break();
#endif

                            p.Add ("Entry", entry.Text)
                                .Write ("Amount", entry.AmountSelectedDB)
                                .Write ("AmountScale", entry.AmountScaleDB.ToStorageString ())
                                .Write ("Grams", item.ServingSizes.GetEquivalent (entry.AmountSelectedDB).ToStorageString ())
                                .Write ("Properties", item.NutritionDB);
						}


					}

				string Xml = d.GetXML ();

				// Store in LocalDB
				string Container = date.ToStorageStringMonth ();
				string DocumentID = date.ToStorageStringDate ();
				LocalDB.WriteAsync (Container, "Day", DocumentID, Xml);

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		#endregion

		#region SaveItems

		private static void SaveItems ()
		{
			try {

				var d = new Serializer ("Items", "All");

				foreach (FoodItem item in Cache.AllValidItems()) {
					d.Add ("Item", item.Text)
					#if DEBUG
						.Write("ServingSizeCount", item.ServingSizesDB == null ? "0" : item.ServingSizesDB.Split("|".ToCharArray()).Count().ToString())
					#endif
                      .Write ("LastAmount", item.LastAmountDB)
                      .Write ("LastChanged", item.LastChanged.ToStorageStringFull ())
                      .Write ("Nutrition", item.NutritionDB)
                      .Write ("ServingSizes", item.ServingSizesDB)
                      .Write ("SourceID", item.SourceID);
				}

				string Xml = d.GetXML ();

				// Store in LocalDB
				LocalDB.WriteAsync ("Items", "Items", "All", Xml);

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		#endregion

		#region SaveRecent

		private static void SaveRecent ()
		{
			try {

				var r = new Serializer ("Recent", "All");
				var MinDate = DateTime.Now.Date.AddDays (-1 * Cache.MAXCACHEDAYS);

				// PERIODS
				foreach (Period period in PeriodList.All) {
					List<RecentItem> entries = Cache.GetRecentCache (period);
					if (entries.Count > 0) {

						var p = r.Add ("Period", period.ToString ());

						// ITEMS
						foreach (RecentItem recent in entries)
							if (recent.Date > MinDate)
								p.Add ("Item", recent.Date.ToStorageStringDate ()).Write ("Text", recent.Text);

					}

				}

				string Xml = r.GetXML ();

				// Store in LocalDB
				LocalDB.WriteAsync ("Recent", "Recent", "All", Xml);

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		#endregion

		#region LoadDay

		public static void LoadDay (DateTime date)
		{

			SessionLog.StartPerformance("LoadDay");

			try {

				string Container = date.ToStorageStringMonth ();
				string DocumentID = date.ToStorageStringDate ();
				string Xml = LocalDB.Read (Container, "Day", DocumentID);

				LoadDay (date, Xml);

				//FoodJournalBackup.Log("Load Day",date.ToStorageStringFull(),null);
				//FoodJournalBackup.VerifyDate(date);

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}

			SessionLog.EndPerformance("LoadDay");

		}

		public static void LoadDay (DateTime date, String Xml)
		{
			if (Xml != null) {
				Serializer s = Serializer.FromXML (Xml);

				foreach (var p in s.Select("Period")) {

					Period period = p.Key.ParsePeriod ();

					List<Entry> entries = new List<Entry> ();
					DateTime modified = DateTime.MinValue;
					if (p.Read ("LastChanged") != null)
						modified = DateTime.Parse (p.Read ("LastChanged"));
					String note = p.Read ("Note");
					String time = p.Read ("Time");

					foreach (var e in p.Select("Entry")) {

						var NewEntry = new Entry (date, period, e.Key);

						NewEntry.AmountSelectedDB = e.Read ("Amount");
						NewEntry.AmountScaleDB = Floats.ParseStorage (e.Read ("AmountScale"));
						NewEntry.Date = date;
						NewEntry.Period = period;

						FoodItem item = new FoodItem (e.Key, true);
						item.NutritionDB = e.Read ("Properties");
						item.ServingSizesDB = e.Read ("Amount") + "=" + e.Read ("Grams");
						Cache.MergeItem (item);
						entries.Add (NewEntry);

					}

					Cache.MergeMeal (date, period, entries, modified, note, time);

				}
			}
		}

		#endregion

		#region LoadItems

		private static volatile bool Loading = false;

		public static void LoadItems (bool Wait, String Xml)
		{

			SessionLog.StartPerformance("LoadItems");

			try {

				if (Loading)
				if (Wait)
					while (Loading) Thread.Sleep(100);
				else
					return;

				//FoodJournalBackup.Log("Load Items",null,null);

				Loading = true;

				if (Xml == null)
					Xml = LocalDB.Read ("Items", "Items", "All");

				if (Xml != null) {
					Serializer s = Serializer.FromXML (Xml);

					foreach (var i in s.Select("Item")) {

						FoodItem item = new FoodItem (i.Key, false);
						item.LastAmountDB = i.Read ("LastAmount");

						item.NutritionDB = i.Read ("Nutrition");
						item.ServingSizesDB = i.Read ("ServingSizes");
						item.SourceID = i.Read ("SourceID");

						Cache.MergeItem (item);

					}
				}

				Cache.AllItemsLoaded = true;

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			} finally {
				Loading = false;

				SessionLog.EndPerformance("LoadItems");
			}
		}

		#endregion

		#region LoadRecent

		public static void LoadRecent ()
		{
			try {

				string Xml = LocalDB.Read ("Recent", "Recent", "All");
				if (Xml == null) return;
				 
				Serializer s = Serializer.FromXML (Xml);

				var MinDate = DateTime.Now.Date.AddDays (-1 * Cache.MAXCACHEDAYS);

				// Load all entries
				foreach (var p in s.Select("Period")) {

					List<RecentItem> entries = new List<RecentItem> ();

					foreach (var item in p.Select("Item")) {
						RecentItem row = new RecentItem () { Date = item.Key.ToDateTime(), Text = item.Read ("Text") };
						if (row.Date > MinDate)
							entries.Add (row);
					}

					List<RecentItem> destination = Cache.GetRecentCache (p.Key.ParsePeriod ());
					lock (destination) {
						destination.Clear ();
						destination.AddRange (entries);
					}

				}

			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		#endregion


	}


}
