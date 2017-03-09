using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using FoodJournal.Model;
using FoodJournal.Runtime;
using FoodJournal.Values;
using Android.Views.Animations;
using FoodJournal.ViewModels;
using FoodJournal.Extensions;
using Android.Gms.Ads;
using FoodJournal.Logging;
using FoodJournal.Android15.Fragments.AppFragments;
using FoodJournal.AppModel;
using System.Globalization;
using System.Threading.Tasks;

namespace FoodJournal.Android15
{
    public class TodayFragment : Fragment
    {
        private RecyclerView recycler;
        private EditText editText;
        private LinearLayout linearLayoutTimeCalories;
        private LinearLayout editNotes;
        private LinearLayout advertisement;
        private LinearLayout layoutNotes;
        private TextView text_totalCalories2;
        private TextView textTotalCalories;
        private Button buttonSetTime;
        private TextView textNotes;
        private LinearLayout layoutNotesSection;
        private ImageView emptyScreenImage;
        private TextView getStarted;
        private TextView getStartedText;
       
        public Period Period;
        public PeriodVM vm;
        private TodayAdapter todayAdapter;
        private View view;

        Fragment ParentFragment;

        public TodayFragment(Fragment parent)
        {
            ParentFragment = parent;
        }

        public TodayFragment()
        {
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Bundle bundle = this.Arguments;
            DateTime date = Arguments.GetString("date").ToDateTime();
            Period = (Period)Enum.Parse(typeof(Period), Arguments.GetString("period"));
            view = inflater.Inflate(Resource.Layout.fragment_today_card, container, false);
            vm = new PeriodVM(date, Period, true);
            
            var binding = DataContext<PeriodVM>.FromView(view);
            binding.VM = vm;

            if (binding.Bindings.Count == 0)
            {
                binding.Add(Resource.Id.layout_time_calories, x => x.TimeOrTotalVisibility);
                binding.Add(Resource.Id.text_totalCalories1, x => x.TotalVisibility);
                binding.Add(Resource.Id.text_totalCalories2, x => x.TotalVisibility);
                binding.Add(Resource.Id.text_totalCalories1, x => x.TotalText);
                binding.Add(Resource.Id.text_totalCalories2, x => x.TotalValue);
                binding.Add(Resource.Id.button_pick_time, x => x.Time);
                binding.Add(Resource.Id.button_pick_time, x => x.TimeVisibility);
                binding.Add(Resource.Id.button_pick_time, (x) =>
					{
                    DateTime current;
                    if (!DateTime.TryParse(x.Time, out current)) current = DateTime.Now;
                    var tpd = new Android.App.TimePickerDialog(Activity,
                        (s, e) => { x.Time = DateTime.Now.SetTime(e.HourOfDay, e.Minute, 0).ToShortTimeString(); }
                        , current.Hour, current.Minute, true);
                    tpd.Show();
                });
                binding.Add(Resource.Id.editText_Notes, x => x.Note);
                binding.Add(Resource.Id.layout_notes, x => x.NoteVisibility);
                binding.Add(Resource.Id.btn_delete, x => { x.Note = null; });
            }

            recycler = view.FindViewById<RecyclerView>(Resource.Id.recycler);
            editText = view.FindViewById<EditText>(Resource.Id.editText_Notes);
            advertisement = view.FindViewById<LinearLayout>(Resource.Id.advertisement);
            linearLayoutTimeCalories = view.FindViewById<LinearLayout>(Resource.Id.layout_time_calories);
            editNotes = view.FindViewById<LinearLayout>(Resource.Id.inputNotes);
            ImageView cross = view.FindViewById<ImageView>(Resource.Id.btn_delete);
            textTotalCalories = view.FindViewById<TextView>(Resource.Id.text_totalCalories);
            buttonSetTime = view.FindViewById<Button>(Resource.Id.button_pick_time);
            textNotes = view.FindViewById<TextView>(Resource.Id.text_today_notes);
            layoutNotesSection = view.FindViewById<LinearLayout>(Resource.Id.layout_note_section);
            emptyScreenImage = view.FindViewById<ImageView>(Resource.Id.empty_screen);
            getStarted = view.FindViewById<TextView>(Resource.Id.get_started);
            getStartedText = view.FindViewById<TextView>(Resource.Id.get_started_text);
            text_totalCalories2 = view.FindViewById<TextView>(Resource.Id.text_totalCalories2);          
            UpdateEmptyVisibility();

            cross.Click += async delegate
            {
               
                Android.Views.InputMethods.InputMethodManager inputManager = (Android.Views.InputMethods.InputMethodManager)inflater.Context.GetSystemService(Context.InputMethodService);
                inputManager.HideSoftInputFromWindow(cross.WindowToken, 0);
            };
            setupRecycler();

            this.HasOptionsMenu = true;

#if DEBUG
            AndroidDebug.SetViewBorders(view);
#endif


            return view;

        }

        public void FocusNote()
        {
            editText.RequestFocus();
        }

#if AMAZON
        public void GrabAd() { }
#else

        private static AdView ad;

		public async void GrabAd()
        {
            ActivateAd(advertisement);
        }

		public async Task ActivateAd(LinearLayout adbox)
        {
            try
            {
                if (!AppStats.Current.AdShows) return;

                Context context = adbox.Context;
                {
					ad = new AdView(context);
                    ad.AdSize = AdSize.SmartBanner;
#if V16
					ad.AdUnitId = "ca-app-pub-3167302081266616/8994125881";
#else
                    ad.AdUnitId = "ca-app-pub-3167302081266616/3848015885";
#endif
                    var requestbuilder = new AdRequest.Builder();
                    ad.LoadAd(requestbuilder.Build());
                }

                adbox.AddView(ad);
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }
#endif

        public void UpdateEmptyVisibility()
        {

            bool HasEntries = vm.EntriesVisibility == FoodJournal.AppModel.UI.Visibility.Visible;

            recycler.Visibility = HasEntries ? ViewStates.Visible : ViewStates.Gone;
            emptyScreenImage.Visibility = HasEntries ? ViewStates.Gone : ViewStates.Visible;
            getStarted.Visibility = HasEntries ? ViewStates.Gone : ViewStates.Visible;
            getStartedText.Visibility = HasEntries ? ViewStates.Gone : ViewStates.Visible;


            if (UserSettings.Current.ShowTotal)
            {
                Property p = UserSettings.Current.SelectedTotal;
                int i = UserSettings.Current.SelectedProperties.IndexOf(p);
                text_totalCalories2.SetTextColor(AndroidUI.GetPropertyColor(Activity, i, p));
            }

        }

        public override void OnResume()
        {           
            base.OnResume();            
            
                if (todayAdapter != null)
                    todayAdapter.NotifyDataSetChanged();

                Refresh();
               
               GrabAd();

        }


        public void Refresh()
        {
          base.OnResume();

			string extra = "a";
			try{				
				if (vm != null) {
					vm.SyncPeriod ();
					vm.Rebind ();
				}
				extra = "b";
	            if (todayAdapter != null)
	            {

	                recycler.GetItemAnimator().RemoveDuration = 250;
	                recycler.GetItemAnimator().AddDuration = 250;

	                List<Entry> current = Cache.GetEntryCache(vm.date.Date)[(int)Period];//
	                int i = 0;
	                int c = 0;

					extra = "c";

	                foreach (var e in current)
	                {
	                    while (todayAdapter.ItemCount > i && todayAdapter.getItem(i) != e)
	                    {
	                        todayAdapter.remove(i);
	                        c++;
	                    }
	                    if (todayAdapter.ItemCount <= i)
	                    {
	                        todayAdapter.add(e, i);
	                        c++;
	                    }
	                    i++;
	                }

	                while (todayAdapter.ItemCount > i)
	                {
	                    todayAdapter.remove(i);
	                    c++;
	                }

	                if (c == 0 || todayAdapter.ItemCount == 0)
	                {
	                    todayAdapter.NotifyDataSetChanged();
	                }

					extra = "d";

	                
	                UpdateEmptyVisibility();

	            }
			} catch (Exception ex) {LittleWatson.ReportException (ex, extra);}

        }
        
        private void setupRecycler()
        {
            recycler.HasFixedSize = false;
            LinearLayoutManager mLayoutManager = new LinearLayoutManager(Activity);
            recycler.SetLayoutManager(mLayoutManager);
            todayAdapter = new TodayAdapter(this.Activity, Cache.GetEntryCache(vm.date)[(int)Period], (TodayViewFragment.Instance).periodDeleteVM);
            recycler.SetAdapter(todayAdapter);

        }

    }
}

