using FoodJournal.Model;
using FoodJournal.Search;
using FoodJournal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.ResourceData
{

    public class ResourceSearchResult2VM : SearchResultVM
    {

        private ResourceRecord2 record;

        public ResourceSearchResult2VM(ResourceRecord2 record) : base(record.Name) { this.record = record; }
        public override FoodItem MakeItem() { return ResourceDatabase2.FoodItemFromResourceRecord(record); }
		public override bool IsLocked {	get { if (record.Meal!="P") return false; return FoodJournal.AppModel.AppStats.Current.PremiumItemsLocked; }
		}

    }

    public class ResourceSearch2Enumerator : IEnumerator<ResourceSearchResult2VM>
    {

        private const int MAXRESULTS = 200;

        private string culture;
        private ResourceDatabase2 database;
        private SearchWorker searchWorker;
        private bool includePremium;
        private string[] terms;
        private int[] termpos;
        private int i;

        private ResourceRecord2 current;

        public ResourceSearch2Enumerator(string culture, string[] terms, SearchWorker searchWorker, bool includePremium)
        {
            this.culture = culture;
            this.database = ResourceDatabase2.FromCulture(culture);
            this.terms = terms;
            this.searchWorker = searchWorker;
            this.includePremium = includePremium;
            termpos = new int[terms.Length];
            for (int cnt = 0; cnt < terms.Length; cnt++) termpos[cnt] = 0;
            if (terms.Length == 0) i = MAXRESULTS;
        }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool boolarg) { }
        public ResourceSearchResult2VM Current { get { return new ResourceSearchResult2VM(current); } }
        object System.Collections.IEnumerator.Current { get { return new ResourceSearchResult2VM(current); } }

        public bool MoveNext()
        {
            if (i >= MAXRESULTS) return false;

            int pos = current.IndexPos;

            do
            {

                pos = database.NextRecordPos(pos);

                //if (pos < 0) { i = MAXRESULTS; return false; }

                int minnext = int.MaxValue;
                for (int cnt = 0; cnt < terms.Length; cnt++)
                {
                    // find the first record that contains at least one of the terms
                    if (termpos[cnt] >= 0 && termpos[cnt] < pos)
                        termpos[cnt] = database.NextHit(pos, terms[cnt]);
                    if (termpos[cnt] > 0)
                        if (termpos[cnt] < minnext)
                            minnext = termpos[cnt];
                }

                if (minnext == int.MaxValue) { i = MAXRESULTS; return false; }
                pos = minnext;

                current = database.RecordFromPos(pos);

                bool isPremiumMatch = (includePremium || current.Meal != "P");
                if (isPremiumMatch && searchWorker.IsMatch(current.Name))
                {
                    i++;
                    return true;
                }

            } while (pos > 0);

            i = MAXRESULTS;
            return false;
        }

        public void Reset() { throw new NotImplementedException(); }
    }

}
