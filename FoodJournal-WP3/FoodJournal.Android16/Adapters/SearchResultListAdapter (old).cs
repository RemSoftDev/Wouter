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

#if false
namespace FoodJournal.Android15.Adapters
{
    public class SearchResultListAdapter : BaseAdapter<SearchResultVM>
    {

        protected LayoutInflater context;
        protected ObservableCollection<SearchResultVM> results;

        public SearchResultListAdapter(LayoutInflater context, ObservableCollection<SearchResultVM> results)
            : base()
        {
            this.context = context;
            this.results = results;
            results.CollectionChanged += results_CollectionChanged;
        }

        void results_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public override SearchResultVM this[int position] { get { return results[position]; } }
        public override long GetItemId(int position) { return position; }
        public override int Count { get { return results.Count; } }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var result = results[position];

            if (position % 2 == 0)
            {
                // Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
                FrameLayout view = (convertView is FrameLayout ? convertView :
                            context.Inflate(Resource.Layout.ItemListItem, parent, false)) as FrameLayout;

                var captionAccent = view.FindViewById<TextView>(Resource.Id.captionAccent);
                var text = view.FindViewById<TextView>(Resource.Id.text);

                captionAccent.SetText(result.CaptionAccent, TextView.BufferType.Normal);
                text.SetText(result.Text, TextView.BufferType.Normal);

                return view;
            }
            else
            {

                // Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
                LinearLayout view = (convertView is LinearLayout ? convertView :
                            context.Inflate(Resource.Layout.EntryListItem, parent, false)) as LinearLayout;

                var itemText = view.FindViewById<TextView>(Resource.Id.itemText);
                var summary = view.FindViewById<TextView>(Resource.Id.summary);

                itemText.SetText(result.CaptionAccent, TextView.BufferType.Normal);
                summary.SetText(result.Text, TextView.BufferType.Normal);

                return view;
            }

        }

    }
}
#endif