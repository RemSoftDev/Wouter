using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FoodJournal.Model;
using FoodJournal.Android15.Adapters;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Values;
using Android.Support.V4.App;
using Android.Views.Animations;
using Xamarin.NineOldAndroids.Animations;
using FoodJournal.Logging;
using FoodJournal.Runtime;
using FoodJournal.Android15.Activities;

namespace FoodJournal.Android15
{
	public class JournalFragmentOld : Fragment, Animator.IAnimatorListener //, IRefreshableFragment
	{

		private static Java.Lang.Object Row = 1;
		private static Java.Lang.Object Line = 2;
		private static Java.Lang.Object Space = 3;

		private float pd;
		private int offscreen;

		private JournalVM jvm;
		private JournalDayVM vm;

		private LinearLayout headers;
		private FrameLayout h1;
		private FrameLayout h2;

		private LinearLayout table;

		private List<View> animvalues;

		private Action nextAnim;

		//public static JournalDayVM lastvm;

		public JournalFragmentOld()		{}

		public static JournalFragment newInstance (DateTime date)
		{

			JournalFragment myFragment = new JournalFragment ();

			Bundle args = new Bundle ();
			args.PutString ("date", FoodJournal.Extensions.DateTimeExtensions.ToStorageStringDate (date));
			myFragment.Arguments = args;

			return myFragment;
		}

		private void MakeSureVM()
		{
			if (vm == null) {

				DateTime date = DateTime.Parse (Arguments.GetString ("date"));

				SessionLog.Debug (string.Format ("New Journal: {0}", date));

				jvm = new JournalVM ();
				vm = new JournalDayVM (jvm, date);
				//vm.Sync (vm.Date);
				//vm.StartRequery();

			}
		}

		public void ShowEmail ()
		{
			MakeSureVM ();
			vm.EmailSettingsVisibility = FoodJournal.AppModel.UI.Visibility.Visible;
		}

		public override void OnAttach (Android.App.Activity activity)
		{
			base.OnAttach (activity);
			MakeSureVM ();
			SwipingFragmentActivity<DateTime> A = activity as SwipingFragmentActivity<DateTime>;
			if (A != null)
				A.SetActiveFragment (vm.Date, this);
		}

		public override void OnDetach ()
		{
			base.OnDetach ();
		}

		public override void OnStart ()
		{
			base.OnStart ();
			MakeSureVM ();
			Sync (vm.Date);
			//activity.FindViewById<GridView>(Resource.Id.colors).LayoutAnimation.Start(); 
		}

		public void Sync (DateTime date)
		{
			MakeSureVM ();
			vm.Sync (date);
			//(Content as JournalView).UpdateHeader(VM);
			//Header = vm.DateText;
			//VM.EmailSettingsVisibility = System.Windows.Visibility.Collapsed;
			UpdateHeader (vm);
			SyncLines ();
		}

		#region IRefreshableFragment implementation

		public void Refresh ()
		{
			MakeSureVM ();
			Sync (vm.Date);
		}

		#endregion

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			try {

				MakeSureVM ();
				View view = inflater.Inflate (Resource.Layout.JournalView, container, false);

				var binding = DataContext<JournalDayVM>.FromView (view);
				binding.VM = vm;

				if (binding.Bindings.Count == 0) {

					binding.Add (Resource.Id.EmailSettings, x => x.EmailSettingsVisibility);
					binding.Add (Resource.Id.Email, x => x.Email);
					var email = view.FindViewById<EditText> (Resource.Id.Email);
					email.TextChanged += (sender, e) => {
						vm.Email = email.Text;
					};
					binding.Add (Resource.Id.SendMail, (x) => {
						Navigate.dialogContext=this.Activity;
						vm.SendEmailReport ("");

						if (vm.Email == "wouterst@hotmail.com")
							Navigate.ToBuyNowPage();

					});
				}

				animvalues = new List<View> ();

				table = view.FindViewById<LinearLayout> (Resource.Id.tableLayout2);

				headers = view.FindViewById<LinearLayout> (Resource.Id.headers);
				h1 = view.FindViewById<FrameLayout> (Resource.Id.H1);
				h2 = view.FindViewById<FrameLayout> (Resource.Id.H2);

				h1.Click += CycleProperty;

				(table as ViewGroup).SetClipChildren (false);

				pd = Resources.DisplayMetrics.Density;
				offscreen = (int)(pd * 700);

				SyncLines ();

				return view;
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
				return null;
			}
		}

		private bool syncing = false;
		private void SyncLines ()
		{

			if (syncing)
				return;
			syncing = true;

			string extra = "";

			try {

				var inflater = table.Context.GetSystemService (Context.LayoutInflaterService) as LayoutInflater;
				int i = 0;

				FrameLayout nextRow = null;

				foreach (var period in vm.Periods) {

					// first, remove all next rows that are not Period Rows
					extra = "remove";
					while (i < table.ChildCount) {
						nextRow = table.GetChildAt (i) as FrameLayout;
						if (nextRow.Tag == Row)
							break;
						var value = nextRow.FindViewById<TextView> (Resource.Id.value);
						if (value != null)
							animvalues.Remove (value);
						var bar = nextRow.FindViewById<View> (Resource.Id.bar);
						if (bar != null)
							animvalues.Remove (bar);
						table.RemoveViewAt (i);
						nextRow = null;
					}

					var periodRow = nextRow;
					// if no next row, inflate a new one
					extra = "inflate";
					if (periodRow == null) {
						periodRow = inflater.Inflate (Resource.Layout.PeriodRow, null) as FrameLayout;
						periodRow.Tag = Row;
						periodRow.Click += (object sender, EventArgs e) => {
							PeriodTap (period);
						};
						(periodRow as ViewGroup).SetClipChildren (false);
						animvalues.Add (periodRow.FindViewById<TextView> (Resource.Id.value));
						animvalues.Add (periodRow.FindViewById<View> (Resource.Id.bar));
						table.AddView (periodRow, i);
					}

					// sync the row texts
					extra = "synctext";
					int screenwidth =  table.Context.Resources.DisplayMetrics.WidthPixels;
					periodRow.FindViewById<View> (Resource.Id.bar).LayoutParameters.Width= (period.BarWidth * screenwidth / JournalDayVM.GoalWidth);
					periodRow.FindViewById<TextView> (Resource.Id.text).Text = period.Text;
					periodRow.FindViewById<TextView> (Resource.Id.value).Text = period.Value;
					i++;

					foreach (var line in period.Lines) {

						nextRow = (i < table.ChildCount) ? table.GetChildAt (i)  as FrameLayout : null;

						var lineRow = nextRow;
						if (nextRow != null && nextRow.Tag != Line)
							nextRow = null;

						if (lineRow != null)
						{
							// 05/30/2015: we were getting lots of "settext" exceptions on object null (not sure why). This is the workaround:
							if (lineRow.FindViewById<TextView> (Resource.Id.text) == null) lineRow = null;
							if (lineRow.FindViewById<TextView> (Resource.Id.value) == null) lineRow = null;
						}

						// if none to re-use, inflate a new one
						extra = "inflaterow";
						if (lineRow == null) {
							lineRow = inflater.Inflate (Resource.Layout.LineRow, null) as FrameLayout;
							lineRow.Tag = Line;
							lineRow.Click += (object sender, EventArgs e) => {
								PeriodTap (period);
							};
							(lineRow as ViewGroup).SetClipChildren (false);
							animvalues.Add (lineRow.FindViewById<TextView> (Resource.Id.value));
							table.AddView (lineRow, i);
						}

						// set the texts
						extra = "settext";
						lineRow.FindViewById<TextView> (Resource.Id.text).Text = line.Text;
						lineRow.FindViewById<TextView> (Resource.Id.value).Text = line.Value;
						i++;

					}

					// add margin if next row isnt margin
					extra = "addmargin";
					nextRow = (i < table.ChildCount) ? table.GetChildAt (i)  as FrameLayout : null;
					if (nextRow == null || nextRow.Tag != Space) {
						var marginview = inflater.Inflate (Resource.Layout.PeriodRowMargin, null) as FrameLayout;
						marginview.Tag = Space;
						table.AddView (marginview, i);
					}
					i++;

				}

				// remove all subsequent rows
				extra = "removetherest";
				while (i < table.ChildCount) {
					nextRow = table.GetChildAt (i) as FrameLayout;
					var value = nextRow.FindViewById<TextView> (Resource.Id.value);
					if (value != null)
						animvalues.Remove (value);
					var bar = nextRow.FindViewById<TextView> (Resource.Id.bar);
					if (bar != null)
						animvalues.Remove (bar);
					table.RemoveViewAt (i);
				}


			} catch (Exception ex) {
				LittleWatson.ReportException (ex, extra);
			}

			syncing = false;

		}

		//protected override void OnResume()
		//{
		//    base.OnResume();

		//    //entryListView.Adapter = new EntryListAdapter(this, vm.EntryList); // todo, handle ItemList ref changes
		//    //itemListView.Adapter = new SearchResultListAdapter(this, vm.ItemList); // todo, handle ItemList ref changes

		//}

		private void PeriodTap (JournalPeriodVM period)
		{
			try {
				Navigate.selectedDate = vm.Date;
				Navigate.ToDayPivot (period.Period);
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		private void BindHeader (bool forh1, JournalViewHeaderVM header)
		{
			if (forh1) {
				h1.FindViewById<TextView> (Resource.Id.Header).Text = header.Header;
				h1.FindViewById<TextView> (Resource.Id.SubHeader).Text = header.SubHeader;
				h1.FindViewById<TextView> (Resource.Id.Value).Text = header.Value;
			} else {
				h2.FindViewById<TextView> (Resource.Id.Header).Text = header.Header;
				h2.FindViewById<TextView> (Resource.Id.SubHeader).Text = header.SubHeader;
				h2.FindViewById<TextView> (Resource.Id.Value).Text = header.Value;
			}
		}

		public void UpdateHeader (JournalDayVM vm)
		{
			var header = vm.GetHeader (jvm.SelectedProperty);
			BindHeader (true, header);
			BindHeader (false, header);
		}

		private void CycleProperty (object sender, EventArgs e)
		{
			PrepDown ();
		}

		private void CycleCompleted (object sender, EventArgs e)
		{
			BindHeader (true, vm.GetHeader (jvm.NextProperty ()));
		}

		private void ValuesOutCompleted (object sender, EventArgs e)
		{
			jvm.SelectedProperty = jvm.NextProperty ();
			Sync (vm.Date);
			ValuesIn ();
		}


		private void PrepDown ()
		{

			ObjectAnimator y1 = ObjectAnimator.OfFloat (headers, "translationY", 0, -h1.Height);
			y1.SetDuration (0);

			nextAnim = () => {
				BindHeader (true, vm.GetHeader (jvm.NextProperty ()));
				CycleDownValuesOut ();

			};
			y1.AddListener (this);
			y1.Start ();

			//headers.OffsetTopAndBottom (-headers.Top-h1.Height);
			//h1.OffsetTopAndBottom (-100);
			//h2.OffsetTopAndBottom (-100);
		}

		private void CycleDownValuesOut ()
		{
		
			ObjectAnimator y1 = ObjectAnimator.OfFloat (headers, "translationY", -h1.Height, 0);
			//y1.SetDuration (0);
			//y1.SetupStartValues
			nextAnim = () => {
				ValuesOutCompleted (null, null);
			};
			y1.AddListener (this);
			y1.Start ();

			//ObjectAnimator y2 = ObjectAnimator.OfFloat (h2, "y", 10f);
			//y2.Start ();

			foreach (var t in animvalues) {
				ObjectAnimator x = ObjectAnimator.OfFloat (t, "translationY", 0, offscreen);
				x.Start ();
			}
				
		}

		public void OnAnimationCancel (Animator p0)
		{
		}

		public void OnAnimationEnd (Animator p0)
		{
			nextAnim.Invoke ();
		}

		public void OnAnimationRepeat (Animator p0)
		{
		}

		public void OnAnimationStart (Animator p0)
		{
		}

		private void ValuesIn ()
		{
			foreach (var t in animvalues) {
				//t.OffsetTopAndBottom (-t.Top-t.Height);
				ObjectAnimator x = ObjectAnimator.OfFloat (t, "translationY", -offscreen, 0);
				x.Start ();
			}
			//headers.OffsetTopAndBottom (-headers.Top);
		}

	}
}