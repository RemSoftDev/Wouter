using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FoodJournal.Model;
using System.Collections.ObjectModel;
using FoodJournal.ViewModels;
using FoodJournal.Logging;
using FoodJournal.Runtime;

namespace FoodJournal.Android15.Adapters
{
    public class EntryListAdapter : BaseAdapter<EntryRowVM>
    {

        protected LayoutInflater context = null;
        protected ObservableCollection<EntryRowVM> entries;

        public EntryListAdapter(LayoutInflater context, ObservableCollection<EntryRowVM> entries)
            : base()
        {
            this.context = context;
            this.entries = entries;
            entries.CollectionChanged += entries_CollectionChanged;
        }

        void entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public override EntryRowVM this[int position] { get { return entries[position]; } }
        public override long GetItemId(int position) { return position; }
        public override int Count { get { return entries.Count; } }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var entry = entries[position];
            LinearLayout view = (convertView ??
                        context.Inflate(Resource.Layout.EntryListItem, parent, false)) as LinearLayout;
            var itemText = view.FindViewById<TextView>(Resource.Id.itemText);
            var summary = view.FindViewById<TextView>(Resource.Id.summary);
            itemText.SetText(entry.ItemText, TextView.BufferType.Normal);
            summary.SetText(entry.Summary, TextView.BufferType.Normal);
            return view;
        }
    }
}