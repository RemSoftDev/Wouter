using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace FoodJournal.Values
{
    public class SortedList<T> : List<T>
    {

        public IComparer<T> _comparer;
        public SortedList(IComparer<T> comparer) { _comparer = comparer; }

        public new void Add(T item)
        {
            int index = this.BinarySearch(item, _comparer);
            if (index >= 0)
                base.Insert(index, item);
            else
                base.Insert(~index, item);
        }

        public bool AddIfNew(T item)
        {
            int index = this.BinarySearch(item, _comparer);
            if (index < 0)
            {
                base.Insert(~index, item);
                return true;
            }
            return false;
        }
    }

}
