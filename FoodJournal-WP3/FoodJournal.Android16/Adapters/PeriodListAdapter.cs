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
using Android.Gms.Ads;

//using Google.Ads;
using FoodJournal.Logging;
using FoodJournal.Android15.Activities;
using FoodJournal.AppModel;

namespace FoodJournal.Android15.Adapters
{
	public class PeriodListAdapter : BaseExpandableListAdapter
	{
		private readonly Context _context;
		private PeriodVM vm;

		public DayPivot daypivot;

		private ObservableCollection<EntryRowVM> entrylist;
		private ObservableCollection<SearchResultVM> itemlist;

		public static List<LinearLayout> AllAds = new List<LinearLayout> ();

		private static AdView ad;

		public PeriodListAdapter (Context context, PeriodVM vm)
		{
			this.vm = vm;
			_context = context;

			vm.EntryList.CollectionChanged += ItemList_CollectionChanged;
			entrylist = vm.EntryList;

			vm.ItemList.CollectionChanged += ItemList_CollectionChanged;
			itemlist = vm.ItemList;

			vm.PropertyChanged += (sender, e) => {

				if (e.PropertyName == "EntryList") {
					if (entrylist != null)
						entrylist.CollectionChanged -= ItemList_CollectionChanged;
					entrylist = vm.EntryList;
					entrylist.CollectionChanged += ItemList_CollectionChanged;
				}


				if (e.PropertyName == "ItemList") {
					if (itemlist != null)
						itemlist.CollectionChanged -= ItemList_CollectionChanged;
					itemlist = vm.ItemList;
					itemlist.CollectionChanged += ItemList_CollectionChanged;
				}

				SessionLog.Debug (string.Format ("PropertyChanged: {0} - {1}", vm.Period, e.PropertyName));
				this.NotifyDataSetChanged ();
			};
		}

		void ItemList_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			SessionLog.Debug (string.Format ("CollectionChanged: {0}", vm.Period));
			this.NotifyDataSetChanged ();
		}

		public override Java.Lang.Object GetChild (int groupPosition, int childPosition)
		{
			return null;
		}

		public override long GetChildId (int groupPosition, int childPosition)
		{
			return childPosition + groupPosition * 1000;
		}

		public override int GetChildrenCount (int groupPosition)
		{
			switch (groupPosition) {
			case 0:
				int cnt = vm.EntryList.Count;
				return cnt == 0 ? 1 : cnt;
			case 1:
				return AppStats.Current.ShouldShowAds ? 1 : 0;
			case 2:
				return vm.ItemList.Count;
			}
			return 0;
		}

		public static void ActivateAd (LinearLayout adbox)
		{

			try{
			Context context = adbox.Context;

			if (ad != null && ad.Context != context) {
				if (ad.Parent != null)
					(ad.Parent as LinearLayout).RemoveView (PeriodListAdapter.ad);
				ad = null;
			}

			if (ad == null) {
				ad = new AdView (context);
				ad.AdSize = AdSize.SmartBanner;
				ad.AdUnitId = "ca-app-pub-3167302081266616/3848015885";
				var requestbuilder = new AdRequest.Builder ();
				ad.LoadAd (requestbuilder.Build ());
			}

			if (ad.Parent != null) {

				if (ad.Parent == adbox)
					return;

				(ad.Parent as LinearLayout).RemoveView (PeriodListAdapter.ad);
			}

			adbox.AddView (ad);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		public override View GetChildView (int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		{

			try {
				var inflater = _context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;

				switch (groupPosition) {
				case 0:

					if (vm.EntryList.Count == 0) {

						try {
							var view4 = inflater.Inflate (Resource.Layout.NothingSelected, parent, false);
							return view4;

						} catch (Exception ex) {
							var m = ex.Message;
						}

					}

					var result = vm.EntryList [childPosition];

					bool isexisting = convertView is LinearLayout;
					if (isexisting && (convertView as LinearLayout).FindViewById<TextView> (Resource.Id.itemText) == null)
						isexisting = false;

					LinearLayout view = (isexisting ? convertView :
                                inflater.Inflate (Resource.Layout.EntryListItem, parent, false)) as LinearLayout;

					var itemText = view.FindViewById<TextView> (Resource.Id.itemText);
					var summary = view.FindViewById<TextView> (Resource.Id.summary);

					SessionLog.Debug (string.Format ("ItemText: {0} = {1} ({2})", vm.Period, itemText, isexisting));

					itemText.SetText (result.ItemText, TextView.BufferType.Normal);
					summary.SetText (result.Summary, TextView.BufferType.Normal);

					return view;

				case 1:

					try {

						bool isexisting2 = convertView is LinearLayout;
						if (isexisting2 && (convertView as LinearLayout).FindViewById<Button> (Resource.Id.hidead) == null)
							isexisting2 = false;

						LinearLayout view3 = isexisting2 ? (LinearLayout)convertView : (LinearLayout)inflater.Inflate (Resource.Layout.Ad, parent, false);

						if (!isexisting2) {
							view3.FindViewById<Button> (Resource.Id.hidead).Click += (object sender, EventArgs e) => {
								FoodJournal.Runtime.Navigate.ToBuyNowPage ();
							};
						}


						// show moved to DayPivot

//						if (ad.Parent != null)
//							(ad.Parent as LinearLayout).RemoveView (ad);
//
//						(view3 as LinearLayout).AddView (ad);

						AllAds.Add (view3);
					

						if (daypivot == null) {
							daypivot = FoodJournal.Runtime.Navigate.navigationContext as DayPivot;
						}

						daypivot.SetAd (vm.Period, view3);


//						} else {
//						var requestbuilder = new AdRequest.Builder ();
//							ad.LoadAd (requestbuilder.Build ());
//						}
//
//						//var layout = FindViewById<LinearLayout>(Resource.Id.mainlayout);
//						//layout.AddView(ad);
//
//						//AdRequest re;
//
//						//AdRequest re = new AdRequest();
//						//re.
//						//view3.FindViewById<AdView>(Resource.Id.ad).LoadAd(re);
						return view3;
					
					} catch (Exception ex) {
						var m = ex.Message;
					}

					return new Button (parent.Context);
				case 2:

					var item = vm.ItemList [childPosition];
					if (convertView != null && convertView.Tag != null)
						convertView = null;
                        // Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
					FrameLayout view2 = (convertView is FrameLayout ? convertView :
                                    inflater.Inflate (Resource.Layout.ItemListItem, parent, false)) as FrameLayout;

					var captionA = item.CaptionAccent;
					if (captionA != null)
						captionA = captionA.ToUpper ();

					if (captionA != null) {
						view2 = inflater.Inflate (Resource.Layout.ItemListSection, parent, false) as FrameLayout;
						view2.Tag = "this";
					}

					var captionAccent = view2.FindViewById<TextView> (Resource.Id.captionAccent);
					var text = view2.FindViewById<TextView> (Resource.Id.text);

//					if (childPosition % 2 == 0)
//					{
//						var d = _context.Resources.GetDrawable(Resource.Drawable.divider);
//						view2.SetBackgroundDrawable(d);
//					}

					captionAccent.SetText (captionA, TextView.BufferType.Normal);
					text.SetText (item.Text, TextView.BufferType.Normal);

					return view2;

				}
			} catch (Exception ex) {
				var ex2 = ex.Message;
			}
			return convertView;
		}

		public override Java.Lang.Object GetGroup (int groupPosition)
		{
			return null;
		}

		public override long GetGroupId (int groupPosition)
		{
			return groupPosition;
		}

		public override View GetGroupView (int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		{

			View header = convertView;
			if (header == null) {
				var inflater = _context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				header = inflater.Inflate (Resource.Layout.ListGroup, null);
			}
			//header.FindViewById<TextView>(Resource.Id.DataHeader).Text = ((char)(65 + groupPosition)).ToString();

			//var view = convertView;

			//if (view == null)
			//{
			//    return new Button(parent.Context);
			//    //return new LinearLayout(parent.Context); 
			//    //var inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
			//    //view = inflater.Inflate(Resource.Layout.your_groupview, null);
			//}

			////setup groupview

			return header;
		}

		public override bool IsChildSelectable (int groupPosition, int childPosition)
		{
			return true;
		}

		public override int GroupCount {
			get { return 3; }
		}

		public override bool HasStableIds {
			get { return true; }
		}
	}
}