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

namespace FoodJournal.Android15.Adapters
{
	public class TestAdapter : BaseAdapter<string>
    {

		protected Context _context = null;

		public TestAdapter(Context context)
            : base()
        {
			this._context = context;
        }


        public override string this[int position] { get { return "test"; } }
        public override long GetItemId(int position) { return position; }
        public override int Count { get { return 10; } }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			View view = convertView;
			if (view == null)
			{
				LayoutInflater inflater =  _context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				view = inflater.Inflate (Resource.Layout.Item, null);
				//view.FindViewById<

			}

//            var itemText = view.FindViewById<TextView>(Resource.Id.itemText);
//            var summary = view.FindViewById<TextView>(Resource.Id.summary);
//
//            itemText.SetText(entry.ItemText, TextView.BufferType.Normal);
//            summary.SetText(entry.Summary, TextView.BufferType.Normal);

            return view;
        }

    }
}

