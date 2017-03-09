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
using Android.Support.V7.Widget;
using FoodJournal.Values;
using FoodJournal.AppModel;

namespace FoodJournal.Android15.Adapters
{
	public class VMListAdapter<T> : BaseAdapter<T> where T: VMBase
	{
		protected Context context = null;
		private ObservableCollection<T> collection;
		private int itemLayoutId;
		private BindEvents bindEvents;
		private BindViewModel bindViewModel;
		public delegate void BindEvents(View view);
		public delegate void BindViewModel(DataContext<T> bindings, T vm);

		public VMListAdapter (Context Context, ObservableCollection<T> Collection, int ItemLayoutId, BindEvents BindEvents, BindViewModel BindViewModel)
			: base ()
		{			
			this.context = Context;
			this.Collection = Collection;
			this.itemLayoutId = ItemLayoutId;
			this.bindEvents = BindEvents;
			this.bindViewModel = BindViewModel;
		}

		private ObservableCollection<T> Collection {
			set {
				if (collection == value)
					return;
				if (collection != null)
					collection.CollectionChanged -= HandleCollectionChanged;
				collection = value;
				if (collection != null)
					collection.CollectionChanged += HandleCollectionChanged;
				this.NotifyDataSetChanged ();
			}
		}

		void HandleCollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.NotifyDataSetChanged ();
		}

		public override T this [int position] { get { return collection == null ? null : collection[position]; } }
		public override long GetItemId (int position){return position;}
		public override int Count { get { return collection == null ? 0 : collection.Count; } }
		public override int GetItemViewType (int position){return 1;}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;

			if (view == null)
			{ 
				view = LayoutInflater.From (parent.Context).Inflate (itemLayoutId, parent, false);
				if (bindEvents != null)
					bindEvents (view);
			}

			T vm = this [position];
			var binding = DataContext<T>.FromView (view);
			binding.VM = vm;

			if (binding.Bindings.Count == 0)
				bindViewModel (binding, vm);

			return view;
		}
	}
}