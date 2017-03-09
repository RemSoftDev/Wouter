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
using FoodJournal.Values;
using FoodJournal.ViewModels.Fragments;

namespace FoodJournal.Android15.Adapters
{
	public class PropertyAdapter : BaseAdapter<PropertyEntryVM>
    {

		protected Context _context = null;
		private EntryDetailVM _container;

		public PropertyAdapter(Context context, EntryDetailVM container)
            : base()
        {
			_context = context;
			_container = container;
			_container.PropertyChanged += (sender, e) => {if (e.PropertyName=="Properties") this.NotifyDataSetChanged();};
        }


		public override PropertyEntryVM this[int position] { get { return (PropertyEntryVM)_container.Properties[position]; } }
        public override long GetItemId(int position) { return position; }
		public override int Count { get { return _container.Properties.Count; } }

		public View GetView2(int position, View convertView, ViewGroup parent)
		{

			PropertyEntryVM vm = this [position];

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			View view = convertView;
			if (view == null) {
				view = View.Inflate (_context, Resource.Layout.item_nutrition_dynamic_items, null);
			}

			var binding = DataContext<PropertyEntryVM>.FromView (view);
			binding.VM = vm;

			if (binding.Bindings.Count == 0)
			{
				binding.Add (Resource.Id.txt_text, x => x.Text);
				binding.Add (Resource.Id.txt_value, x => x.Value);
			}

			return view;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{

			PropertyEntryVM vm = this [position];

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			View view = convertView;
			if (view == null) {
				LayoutInflater inflater = _context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				view = inflater.Inflate (Resource.Layout.Property, null);
			}

			var binding = DataContext<PropertyEntryVM>.FromView (view);
			binding.VM = vm;

			if (binding.Bindings.Count == 0)
			{
				binding.Add (Resource.Id.txt_text, x => x.Text);
				binding.Add (Resource.Id.txt_value, x => x.Value);
			}

			return view;
		}

    }
}

