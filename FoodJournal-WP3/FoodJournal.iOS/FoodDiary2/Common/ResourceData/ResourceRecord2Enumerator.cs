using FoodJournal.Model;
using FoodJournal.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.ResourceData
{

    public class ResourceRecord2Enumerator : IEnumerator<FoodItem>
    {

        private int pos;
        private string Meal;
        private ResourceDatabase2 database;

        private FoodItem current;

        public ResourceRecord2Enumerator() { pos = 0; database = ResourceDatabase2.Default; }
        public ResourceRecord2Enumerator(Period period) { pos = 0; Meal = period.ToString().Substring(0, 1); database = ResourceDatabase2.Default; }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool boolarg) { }
        public FoodItem Current { get { return current; } }
        object System.Collections.IEnumerator.Current { get { return current; } }

        public bool MoveNext()
        {

            do
            {
                pos = database.NextRecordPos(pos);
                if (pos <= 0) return false;
                current = database.ItemFromResourceRecord(database.RecordFromPos(pos));
                if (current == null) return false;
                if (current.CommonMeal == null || current.CommonMeal == "P") return false; // done after common items are finished
            } while (current.Text == null || (Meal != null && current.CommonMeal != Meal && current.CommonMeal != "P")); // itterate untill we have a match

            return true;
        }

        public void Reset() { throw new NotImplementedException(); }
    }

}
