using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FoodJournal.Model;
using FoodJournal.AppModel;
using FoodJournal.Android15.Adapters;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Values;
using Android.Support.V4.App;
using FoodJournal.Runtime;
using FoodJournal.Logging;
using FoodJournal.Android15.Activities;

namespace FoodJournal.Android15
{
	public class PeriodFragment : Android.Support.V4.App.Fragment,  IRefreshableFragment
	{
	
		private PeriodVM vm;
		protected ListView entryListView;
		protected ListView itemListView;
		private PeriodListAdapter pda;

		private DayPivot daypivot;

		public PeriodFragment ()
		{
		}

		public static PeriodFragment newInstance (DateTime date, Period period)
		{

			PeriodFragment myFragment = new PeriodFragment ();

			Bundle args = new Bundle ();
			args.PutString ("date", FoodJournal.Extensions.DateTimeExtensions.ToStorageStringDate (date));
			args.PutString ("period", period.ToString ());
			myFragment.Arguments = args;

			return myFragment;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			try {

				DateTime date = DateTime.Parse (Arguments.GetString ("date"));
				Period period = (Period)Enum.Parse (typeof(Period), Arguments.GetString ("period"));
				vm = new PeriodVM (date, period);

				//View view = inflater.Inflate(Resource.Layout.PeriodView, container, false);
				View view = inflater.Inflate (Resource.Layout.PeriodView2, container, false);

				var binding = DataContext<PeriodVM>.FromView (view);
				binding.VM = vm;

				if (binding.Bindings.Count == 0) {
					binding.Add (Resource.Id.button2, (x) => { 

						//Com.Example.Testlib.Adapters.Xamarin.Instance = new XamarinCommunicator();
						//Intent intent = new Intent (Activity, typeof(Com.Example.Testlib.Adapters.MainActivity));
						Intent intent = new Intent (Activity, typeof(MainActivity));
						StartActivity (intent);

						//Navigate.selectedPeriod = vm.Period;
						//Navigate.ToSearchPage (null);
					});
				}

				var list = view.FindViewById<ExpandableListView> (Resource.Id.periodListView);
				//list.AddFooterView(new Button(container.Context));
				list.SetGroupIndicator (null);

				pda = new PeriodListAdapter (container.Context, vm);
				list.SetAdapter (pda);
				list.ExpandGroup (0);
				list.ExpandGroup (1);
				list.ExpandGroup (2);

				list.GroupCollapse += (object sender, ExpandableListView.GroupCollapseEventArgs e) => {
					try {
						list.ExpandGroup (e.GroupPosition);
					} catch (Exception ex) {
						LittleWatson.ReportException (ex);
					}
				};
				list.GroupClick += (object sender, ExpandableListView.GroupClickEventArgs e) => {
				};
				list.ChildClick += (object sender, ExpandableListView.ChildClickEventArgs e) => {
					try {
						if (e.GroupPosition == 0) {
							if (vm.EntryList.Count > 0)
								vm.EntryList [e.ChildPosition].NavigateToEntryDetail ();
						}
						if (e.GroupPosition == 2)
							vm.OnItemTap (vm.ItemList [e.ChildPosition]);
					} catch (Exception ex) {
						LittleWatson.ReportException (ex);
					}
				};

				return view;
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
				return null;
			}
		}

		public override void OnAttach (Android.App.Activity activity)
		{
			base.OnAttach (activity);
			this.daypivot = activity as DayPivot;
		}

		// IRefreshableFragment implementation
		public void Refresh ()
		{
			if (vm != null)
				vm.StartRequery ();
		}

		public override void OnResume ()
		{
			base.OnResume ();

			if (vm == null)
				return;

			SessionLog.Debug (string.Format ("* OnResume: {0}", vm.Period));
			vm.StartRequery ();

			//if (daypivot != null && vm != null)
			pda.daypivot = daypivot;
			//	daypivot.SetAd (vm.Period, pda.Ad);

			//pda.NotifyDataSetChanged ();
			//entryListView.Adapter = new EntryListAdapter(this, vm.EntryList); // todo, handle ItemList ref changes
			//itemListView.Adapter = new SearchResultListAdapter(this, vm.ItemList); // todo, handle ItemList ref changes

		}
	}
}