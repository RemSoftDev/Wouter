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
//using FoodJournal.Android15;

namespace FoodJournal.ViewModels
{
    public class PeriodVM : VMBase, IAcceptsSearchResult
    {

        private const int MAXCOMMONITEMSFREE = 25;
        private const int MAXCOMMONITEMSPAID = 40;

		public readonly bool NewStyleWithoutItems;

        public readonly DateTime date;
        private Period period;
		private volatile string querySyncID;

        public Period Period { get { return period; } }

        //public PicturesVM PicturesVM { get; set; }

        public ObservableCollection<EntryRowVM> EntryList { get; private set; }
        public ObservableCollection<SearchResultVM> ItemList { get; private set; }

        public string DateText { get { return (date == DateTime.Now.Date) ? AppResources.Today : date.ToLongDateString(); } }
        public string PageTitle { get { return AppResources.PageTitle + " - " + DateText.ToUpper(); } }

		public Visibility EmptyListVisibility { get { return (EntryList.Count != 0) ? Visibility.Collapsed : Visibility.Visible; } }

		public Visibility NoteVisibility { get { return (Cache.GetPeriodNote (date, period) == null) ? Visibility.Collapsed : Visibility.Visible; } }
		public string Note { get { return Cache.GetPeriodNote (date, period); } set { Cache.SetPeriodNote (date, period, value); NotifyPropertyChanged ("NoteVisibility"); } }

		public Visibility TimeOrTotalVisibility {get { return (TotalVisibility == Visibility.Visible || TimeVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;}}

		public Visibility TimeVisibility { get { 
				//if (!String.IsNullOrEmpty(Cache.GetPeriodTime (date, period))) return Visibility.Visible;  // visible when a time filled out, regardless of settings
				if (NoEntriesVisibility == Visibility.Visible) return Visibility.Collapsed; // hidden when no entries yet (is this right?)
				if (UserSettings.Current.ShowMealTime) return Visibility.Visible; // visible if turen on
				return Visibility.Collapsed; } }
		public string Time { get { String value = Cache.GetPeriodTime (date, period); return String.IsNullOrEmpty (value) ? AppResources.SelectDate : value; } set { Cache.SetPeriodTime (date, period, value); NotifyPropertyChanged ("Time"); } }

		public Visibility TotalVisibility { get { 
				if (NoEntriesVisibility == Visibility.Visible) return Visibility.Collapsed; // hidden when no entries yet
				return UserSettings.Current.ShowTotal ? Visibility.Visible : Visibility.Collapsed; } }

		public string TotalText { get { if (!UserSettings.Current.ShowTotal) return null; return UserSettings.Current.SelectedTotal.FullCapitalizedText; } }
		public string TotalValue { get { if (!UserSettings.Current.ShowTotal) return null; return Cache.GetPeriodPropertyValue(date, period, UserSettings.Current.SelectedTotal).ValueString(); } }

		public Visibility NoEntriesVisibility { get { return EntryList.Count == 0 ? Visibility.Visible : Visibility.Collapsed; } }
		public Visibility EntriesVisibility { get { return EntryList.Count != 0 ? Visibility.Visible : Visibility.Collapsed; } }

		public void Rebind()
		{
			NotifyPropertyChanged ("TotalVisibility");
			NotifyPropertyChanged ("TimeVisibility");
			NotifyPropertyChanged ("Note");
			NotifyPropertyChanged ("NoteVisibility");
			NotifyPropertyChanged ("TotalText");
			NotifyPropertyChanged ("TotalValue");
			NotifyPropertyChanged ("TimeOrTotalVisibility");
		}

        public PeriodVM(DateTime date, Period period)
        {
			NewStyleWithoutItems = false;
            this.date = date.Date;
            this.period = period;
            this.EntryList = new ObservableCollection<EntryRowVM>();
            this.ItemList = new ObservableCollection<SearchResultVM>();
            //PicturesVM = new PicturesVM(date, period);
        }

		public PeriodVM(DateTime date, Period period, bool NewStyleWithoutItems)
		{
			if (NewStyleWithoutItems == false)
				throw new ArgumentOutOfRangeException ("NewStyleWithoutItems");
			NewStyleWithoutItems = true;
			this.date = date.Date;
			this.period = period;
			this.EntryList = new ObservableCollection<EntryRowVM>();
			SyncPeriod ();//StartRequery ();
		}

        public void StartRequery()
        {

			SessionLog.Debug (string.Format ("*** StartRequery: {0}", period));

            //App.PeriodVM = this;
            Navigate.selectedDate = date;
			#if WINDOWS_PHONE
            Navigate.selectedPeriod = period;
			#endif

			var syncID = new System.Random().NextDouble().ToString();
            querySyncID = syncID;

            try
            {
                SyncPeriod();

				if (NewStyleWithoutItems) return;

                Requery(syncID, false);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            var bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += (s, a) =>
            {
                System.Threading.Thread.Sleep(300);
                try
                {
                    if (querySyncID == syncID)
                    {
#if DEBUG && WINDOWS_PHONE
                        FoodJournal.WinPhone.App.SetThreadCulture(); // only needed for screenshots
#endif
                        Requery(syncID, true);
                    }

                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            };
            bw.RunWorkerAsync();
        }

        private bool ShouldStopQuery(string syncId) { return (querySyncID != syncId); }

        public void SyncPeriod()
        {
            try
            {
                List<Entry> list = Cache.GetEntryCache(date)[(int)period];

                foreach (Entry e in list)
                {
                    bool HadIt = false;
                    foreach (var VM in EntryList)
                        if (VM.IsForEntry(e))
                        {
                            HadIt = true;
                            VM.NotifyIfChanged();
                        }
                    if (!HadIt)
                    {
                        //EntryList.Insert(0, new EntryVM(e));
                        EntryList.Add(new EntryRowVM(e));
                        if (EntryList.Count == 1) NotifyPropertyChanged("NoEntriesVisibility");
                    }
                }

                for (int i = 0; i < EntryList.Count; )
                    if (list.Count(e => EntryList[i].IsForEntry(e)) == 0)
                    {
                        EntryList.RemoveAt(i);
                        if (EntryList.Count == 0) NotifyPropertyChanged("NoEntriesVisibility");
                    }
                    else
                        i++;

				NotifyPropertyChanged ("TotalValue");

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        private void Requery(string syncID, bool loadCaches)
        {

            using (var log = SessionLog.NewScope("Requerying" + (loadCaches ? " 2" : ""), period.ToString()))
            {

                // Breakfast <- static list of entries

                // Breakfast Items
                // All Items
                // New Item
                // USDA List?
                // - simple
                // - nutrition items
                // - generic items

                List<FoodItem> shown = new List<FoodItem>();

                List<Entry> list = Cache.GetEntryCache(date)[(int)period];

                foreach (Entry e in list)
					if (e.Item != null)
                    if (!shown.Contains(e.Item, FoodItemComparer.instance))
                        shown.Add(e.Item);

                var results = new ObservableCollection<SearchResultVM>();
                //results.Clear();

                int cnt = results.Count;

                foreach (var item in Cache.GetRecentCache(period))
					if (!shown.Contains(item.FlatItem, FoodItemComparer.instance))
                     {
                         shown.Add(item.FlatItem);
                         results.Add(new SearchResultRecentVM(item));
                     }

#if LegacyDB
                var enumerator = new Cache.PeriodEntryEnumerator(period);
                if (loadCaches) enumerator.LoadAllDays();
                enumerator.ShouldLoadDay = () =>
                        {
                            if (!loadCaches) return false;
                            if (ShouldStopQuery(syncID)) { log.SetState("Aborted"); return false; } else return true;
                        };

                while (enumerator.MoveNext())
                {
                    FoodItem item = enumerator.Current.Item;
					if (item != null)
                    if (!shown.Contains(item, FoodItemComparer.instance))
                    {
                        shown.Add(item);
                        results.Add(new SearchResultFoodItemVM(item));
                    }
                }
#endif

                //" + period.ToString().ToLower() + " 
                if (results.Count != cnt)
                    results.Insert(cnt, new SearchResultHeaderVM(Strings.FromEnum("MyRecentItems" , period.ToString()), 0));

                cnt = results.Count;

                int i = 0;
                var enumerator2 = new ResourceRecord2Enumerator(period);
                int MaxCommonItems = AppStats.Current.IncludePremiumItems ? MAXCOMMONITEMSPAID : MAXCOMMONITEMSFREE;
                while (i < MaxCommonItems && enumerator2.MoveNext())
                {
                    FoodItem item = enumerator2.Current;
                    if (!shown.Contains(item, FoodItemComparer.instance))
                    {
                        shown.Add(item);
                        results.Add(new SearchResultFoodItemVM(item));
                        i++;
                    }
                }

                if (results.Count != cnt)
                    results.Insert(cnt, new SearchResultHeaderVM(Strings.FromEnum("CommonItems", period.ToString()), 0));

                cnt = results.Count;

                if (loadCaches || Cache.AllItemsLoaded)
                {

                    if (!Cache.AllItemsLoaded)
                        if (ShouldStopQuery(syncID)) { log.SetState("Aborted"); return; }

                    if (!Cache.AllItemsLoaded)
						FoodJournalNoSQL.LoadItems(false, null);

                }

                if (loadCaches)
                {
                    //if (HasQuery) 
                    System.Threading.Thread.Sleep(500);
                    if (ShouldStopQuery(syncID)) { log.SetState("Aborted"); return; }
                }
                else
                {
                    while (results.Count > 12)
                        results.RemoveAt(12);
                }

                foreach (var r in results)
                    r.Listener = this;

                // todo: sync instead (how to deal with the removeat 12 though?
                this.ItemList = results;
                NotifyPropertyChanged("ItemList");

            }
        }

        public void OnItemTap(SearchResultVM result)
        {

            //if (AppStats.Current.IsTrialExpired)
            //{
            //    App.MessageTrialCount(true);
            //    return;
            //}

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
                for (int j = 0; j < this.ItemList.Count; j++)
                    if (ItemList[j] == result) i = j;
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            SessionLog.RecordNewEntry(item.Text, result.GetType().Name, "Recent Item", null, i);

            Entry entry = new Entry(this.date, this.period, item);
            entry.Save();

            MessageQueue.Push(new EntryUpdatedMessage(entry));

            EntryList.Insert(0, new EntryRowVM(entry));
            ItemList.Remove(result);
            if (EntryList.Count == 1) NotifyPropertyChanged("NoEntriesVisibility");

        }

        public void DeleteEntry(EntryRowVM entry)
        {
            if (entry == null) return;
            entry.DeleteEntry();
            StartRequery();
        }

    }
}
