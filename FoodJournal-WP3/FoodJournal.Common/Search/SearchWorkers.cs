using FoodJournal.AppModel;
using FoodJournal.AppModel.UI;
using FoodJournal.Logging;
using FoodJournal.Model;
using FoodJournal.ResourceData;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using FoodJournal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodJournal.Resources;

namespace FoodJournal.Search
{

    #region class SearchWorker
    public abstract class SearchWorker
    {

        private SearchVM searchVM;
        private double syncId;
        private string Query;

        public bool IsDone;
        internal bool HasAsync = true;
        internal bool HasAsyncRequest = false;

        internal string[] terms;
        private int MaxHitCount = 0;

        public SearchWorker(SearchVM SearchVM)
        {
            this.searchVM = SearchVM;
            this.syncId = searchVM.SyncID;
            this.Query = searchVM.Query;
            terms = Query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in terms) MaxHitCount += term.Length;
        }

        public int HitCount(string title)
        {
            int cnt = 0;
            if (title == null) return 0;
            foreach (var term in terms)
                if (title.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) != -1) cnt += term.Length;//cnt++;
            return cnt;
        }

        public int AndResultCount { get { return searchVM.AndResultCount; } }

        internal bool IsMatch(string text) { return (HitCount(text) > 0); }
        internal bool IsMatch(FoodItem item) { return IsMatch(item.Text); }
        internal bool IsMatch(SearchResultVM item) { return IsMatch(item.Text); }

        public void StartRequery()
        {

            IsDone = false;

            string Query = searchVM.Query;
            RunQuery(Query, false);

            if (HasAsync)
                BackgroundTask.Start(100, () => { if (!ShouldAbort) RunQuery(Query, true); });

        }

        private void RunQuery(string Query, bool IsAsync)
        {
            try
            {
                var results = new List<SearchResultVM>();

                RunQueryI(Query, results, IsAsync);

                IsDone = !HasAsyncRequest;
                ReportProgress(results);

            }
            catch (Exception ex)
            {
                Logging.LittleWatson.ReportException(ex);
            }
        }

        internal abstract void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync);
        internal bool ShouldAbort { get { return syncId != searchVM.SyncID; } }
        public void ReportProgress(List<SearchResultVM> results)
        {
            if (ShouldAbort) return;
            foreach (SearchResultVM result in results)
                result.SetAccuracy(HitCount(result.Text), MaxHitCount);
            searchVM.ReportWorkerResults(results);
        }

    }

    /// <summary>
    /// New Items
    /// </summary>
    public class SearchWorkerNewItems : SearchWorker
    {

        public SearchWorkerNewItems(SearchVM SearchVM) : base(SearchVM) { HasAsync = false; }

        internal override void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync)
        {
            results.Add(new SearchResultHeaderVM(AppResources.NewItemSuffix, 1));
            results.Add(new SearchResultNewVM(Query, Query + " " + (AppResources.NewItemSuffix + "").Trim()));
            //results.Add(new SearchResultNewVM(Query + AppResources.NewRecipeSuffix, 2));
        }
    }

    /// <summary>
    /// Recent Items in the same period
    /// </summary>
    public class SearchWorkerRecent : SearchWorker
    {

        private Period Period;
        public SearchWorkerRecent(SearchVM SearchVM) : base(SearchVM) { this.Period = SearchVM.Period; }

        internal override void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync)
        {

            foreach (var item in Cache.GetRecentCache(Period))
				if (IsMatch(item.FlatItem))
                    results.Add(new SearchResultRecentVM(item));

            if (results.Count > 0)
                results.Insert(0, new SearchResultHeaderVM(Strings.FromEnum("MyRecentItems", Period.ToString()), 0));

        }
    }

    /// <summary>
    /// Common Items in the same period
    /// </summary>
    public class SearchWorkerCommon : SearchWorker
    {

        private Period Period;
        public SearchWorkerCommon(SearchVM SearchVM) : base(SearchVM) { this.Period = SearchVM.Period; }

        internal override void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync)
        {

            if (IsAsync)
            {

                var enumerator = new ResourceSearch2Enumerator(AppStats.Current.DatabaseCulture, terms, this, AppStats.Current.IncludePremiumItems);

                while (enumerator.MoveNext())
                    results.Add(enumerator.Current);

                //var enumerator2 = new ResourceRecord2Enumerator(Period);
                //while (enumerator2.MoveNext())
                //{
                //    FoodItem item = enumerator2.Current;
                //    if (!shown.Contains(item, FoodItemComparer.instance))
                //    {
                //        shown.Add(item);
                //        results.Add(new SearchResultFoodItemVM(item));
                //        i++;
                //    }
                //}

                //string firstChar = Period.ToString().Substring(0, 1);

                //foreach (var item in Cache.AllItems)
                //    if (item is FoodItem)
                //        if ((item as FoodItem).CommonMealContains(firstChar))
                //            if (IsMatch(item))
                //                results.Add(new SearchResultFoodItemVM(item));

                //if (results.Count > 0)
                //    results.Insert(0, new SearchResultHeaderVM(AppResources.CommonItems, 0));

            }

        }
    }

    /// <summary>
    /// All Items from the DB
    /// </summary>
    public class SearchWorkerDB : SearchWorker
    {

        private Period Period;
        public SearchWorkerDB(SearchVM SearchVM) : base(SearchVM) { this.Period = SearchVM.Period; }

        internal override void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync)
        {

            if (IsAsync || Cache.AllItemsLoaded)
            {

                if (!Cache.AllItemsLoaded)
					FoodJournalNoSQL.LoadItems(false, null);

                string firstChar = Period.ToString().Substring(0, 1);

                foreach (var item in Cache.AllValidItems())
                    if (item is FoodItem)
                        if (IsMatch(item))
                            results.Add(new SearchResultFoodItemVM(item));

                if (results.Count > 0)
                    results.Insert(0, new SearchResultHeaderVM(AppResources.CommonItems, 0));

            }

        }
    }

    #endregion

}
