using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.Messages;
using FoodJournal.Model;
using FoodJournal.ResourceData;
using FoodJournal.WinPhone.Common.Resources;
//using FoodJournal.Runtime;
using FoodJournal.Search;
//using FoodJournal.Services;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using FoodJournal.AppModel.UI;

namespace FoodJournal.ViewModels
{

	#region class SearchResultNewVM

	public class SearchResultNewVM : SearchResultVM
	{
		private string Query;

		public SearchResultNewVM(string Query, string Text) : base(Text) { this.Query = Query; AccuracyScore = 1; }

		public override FoodItem MakeItem()
		{
			return new FoodItem(Query, true);
		}
	}

	#endregion

	//- common foods, by meal
	//- recipe's
	//- all foods in the db
	//- foods in the resourcedb
	//- web services

	public class SearchVM : VMBase, IAcceptsSearchResult
	{

		public Period Period;
		public int AndResultCount;
		public ObservableCollection<SearchResultVM> Results { get; private set; }
		private SortedList<SearchResultVM> SortedResults;

		#region SyncID
		private object syncLock = new object();
		private double qSyncID;
		internal double SyncID
		{
			get { lock (syncLock) { return qSyncID; } }
			set { lock (syncLock) { qSyncID = value; } }
		}
		#endregion

		private List<SearchWorker> workers;

		public SearchVM(Period Period)
		{
			this.Period = Period;
		}

		public Visibility NoEntriesVisibility { get { return (Results == null || Results.Count == 0) ? Visibility.Visible : Visibility.Collapsed; } }

		private string Previous = null;
		private string LastQuery = "";
		private void TraceQuery(bool force, bool issuccess)
		{
			if (force)
			{
				SessionLog.RecordQuery(Query, issuccess, Previous);
			}
			else
			{
				if (Query.StartsWith(LastQuery)) { LastQuery = Query; return; }
				if (LastQuery.StartsWith(Query)) return; // backspace
				if (!string.IsNullOrEmpty(Query))
					SessionLog.RecordQuery(LastQuery, false, Previous);
			}
			if (!string.IsNullOrEmpty(LastQuery)) Previous = LastQuery;
			LastQuery = "";
		}

		public void OnBackKeyPress() { TraceQuery(true, false); }

		public void OnItemTap(SearchResultVM result)
		{

			if (result.IsLocked) {
//				Navigate.ToBuyNowPage ();
				return;
			}

			FoodItem item = null;
			try
			{
				item = result.MakeItem() as FoodItem;
			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }
			if (item == null) return;

			int i = 0;
			try
			{
				if (Results != null)
					for (int j = 0; j < Results.Count; j++)
						if (Results[j] == result) i = j;
			}
			catch (Exception ex) { LittleWatson.ReportException(ex); }

			TraceQuery(true, true);
			SessionLog.RecordNewEntry(item.Text, result.GetType().Name, Query, LastQuery, i);

//			Entry entry = new Entry(Navigate.selectedDate, Navigate.selectedPeriod, item);
//
//			if (item.IsNewItem)
//				Navigate.ToEntryDetail(entry);
//			else
//			{
//
//				if (Navigate.IAcceptsNewEntry != null)
//				if (!Navigate.IAcceptsNewEntry.ShouldSaveNewEntry(entry))
//				{
//					Navigate.BackFromSearch();
//					return;
//				}
//
//				entry.Save();
//
//				MessageQueue.Push(new EntryUpdatedMessage(entry));
//
//				//Cache.AddEntry(entry); // <-- the double entry bug!!!
//				Navigate.BackFromSearch();
//			}

		}

		private bool _IsProcessing;
		public bool IsProcessing
		{
			get { return _IsProcessing; }
			set
			{
				if (_IsProcessing != value)
				{
					_IsProcessing = value;
					NotifyPropertyChanged("IsProcessing");
				}
			}
		}

		private string _Query = "";
		public string Query
		{
			get { return _Query; }
			set
			{
				if (value != _Query)
				{
					_Query = value;
					NotifyPropertyChanged("Query");
					TraceQuery(false, false);
					StartRequery(); // todo: sink click or timeout instead for online searches?
				}
			}
		}

		public void StartRequery()
		{

			List<SearchResultVM> oldItems = new List<SearchResultVM>();
			var tempworker = new SearchWorkerCommon(this);

			if (!string.IsNullOrEmpty(Query) && Results != null)
			{
				foreach (var result in Results)
					if (!(result is SearchResultNewVM) && tempworker.IsMatch(result))
						oldItems.Add(result);
			}

			AndResultCount = 0;
			Results = new ObservableCollection<SearchResultVM>();
			SortedResults = new SortedList<SearchResultVM>(new SortedResultsComparer());

			if (oldItems.Count > 0)
				tempworker.ReportProgress(oldItems);

			if (Query != null && Query.Length > 0)
			{
				IsProcessing = true;

				qSyncID = new System.Random().NextDouble();

				workers = new List<SearchWorker>();

				workers.Add(new SearchWorkerNewItems(this));
				workers.Add(new SearchWorkerRecent(this));
				workers.Add(new SearchWorkerCommon(this)); // thisone reads resource v2
				workers.Add(new SearchWorkerDB(this));
				//if (AppResources.DatabaseCulture.ToLower() == "en")
				//{
				//    workers.Add(new SearchWorkerNutritionIx(this));
				//}

				foreach (var worker in workers)
					worker.StartRequery();

			}

			NotifyPropertyChanged("Results");
			NotifyPropertyChanged("NoEntriesVisibility");

		}

		public void CheckEndRequery()
		{
			foreach (var worker in workers)
				if (!worker.IsDone) return;

			IsProcessing = false;

		}

		internal void ReportWorkerResults(List<SearchResultVM> results)
		{
			try{
				bool hasNew = false;
				foreach (var result in results)
					if (!(result is SearchResultHeaderVM))
					if (SortedResults.AddIfNew(result))
					{
						result.Listener = this;
						hasNew = true;
						if (result.IsAndHit) AndResultCount++;
					}
				if (hasNew) SyncProgress();
			} catch(Exception ex) {}; // eat this exception, user experience for a missing result is much better than a crash
		}

		private void SyncProgress()
		{
			Platform.RunSafeOnUIThread("SyncProgress", () =>
				{
					try
					{
						for (int i = 0; i < SortedResults.Count; i++)
							if (SortedResults[i] != null)
							if (Results.Count <= i || Results[i].Text != SortedResults[i].Text)
							{
								if (Results.Count <= i)
									Results.Add(SortedResults[i]);
								else
									Results.Insert(i, SortedResults[i]);
								if (Results.Count == 1)
									NotifyPropertyChanged("NoEntriesVisibility");
							}
						//System.Threading.Thread.Sleep(50);
					}
					catch (Exception ex) { LittleWatson.ReportException(ex); }
				}
			);
		}


	}
}
