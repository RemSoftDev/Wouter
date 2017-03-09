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
	public class SearchResultListAdapter : BaseAdapter<SearchResultVM>
	{

		protected Context _context = null;
		private SearchVM _container;
		private ObservableCollection<SearchResultVM> collection;


		public SearchResultListAdapter (Context context, SearchVM container)
			: base ()
		{
			_context = context;
			_container = container;
			_container.PropertyChanged += (sender, e) => {
				if (e.PropertyName == "Results")
					Collection = _container.Results;
			};
			Collection = _container.Results;
		}

		private ObservableCollection<SearchResultVM> Collection {
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

		public override SearchResultVM this [int position] { get { return _container.Results == null ? null : (SearchResultVM)_container.Results [position]; } }

		public override long GetItemId (int position)
		{
			return position;
		}

		public override int Count { get { return _container.Results == null ? 0 : _container.Results.Count; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{

			SearchResultVM vm = this [position];

			// Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
			View view = convertView;
			if (view == null) {
				LayoutInflater inflater = _context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				if (FoodJournal.AppModel.AppStats.Current.PremiumItemsLocked)
					view = inflater.Inflate (Resource.Layout.ItemListItemLockable, null);
				else
					view = inflater.Inflate (Resource.Layout.ItemListItem, null);
			}

			if (FoodJournal.AppModel.AppStats.Current.PremiumItemsLocked) {
				if (vm.IsLocked) {
					view.FindViewById<ImageView> (Resource.Id.lockicon).Visibility = ViewStates.Visible;
					view.FindViewById<TextView> (Resource.Id.text).SetTextColor (view.Context.Resources.GetColor (Resource.Color.SubtleTextColor));
				} else {
					view.FindViewById<ImageView> (Resource.Id.lockicon).Visibility = ViewStates.Gone;
					view.FindViewById<TextView> (Resource.Id.text).SetTextColor (view.Context.Resources.GetColor (Resource.Color.NormalTextColor));
				}
			}

			view.FindViewById<TextView> (Resource.Id.captionAccent).SetText (vm.CaptionAccent, TextView.BufferType.Normal);
			view.FindViewById<TextView> (Resource.Id.text).SetText (vm.Text, TextView.BufferType.Normal);

			return view;
		}
	}
}