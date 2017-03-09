using System;
using FoodJournal.Extensions;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using FoodJournal.Model;
using FoodJournal.Runtime;
using FoodJournal.WinPhone.Common.Resources;
using Android.Views.InputMethods;
using Android.Support.V7.App;
using FoodJournal.ViewModels;
using Android.Support.V7.Widget;
using FoodJournal.Resources;
using FoodJournal.Android15.Adapters;
using FoodJournal.Values;
using Android.Widget;
using Android.Preferences;
using FoodJournal.AppModel;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Text;
using Android.Graphics;
using Android.Gms.Ads;
using FoodJournal.Android15.Activities;
using Android.Content.PM;
using FoodJournal.Logging;



namespace FoodJournal.Android15.Fragments.AppFragments
{
    public class TodayViewFragment : BaseViewFragment, Android.Support.V4.View.ViewPager.IOnPageChangeListener, Android.Support.V7.View.ActionMode.ICallback
    {
        ViewPager viewPager;
        MainPagerAdapter mainPagerAdapter;
        TabLayout tabLayout;
        public PeriodDeleteVM periodDeleteVM;
        public static TodayViewFragment Instance;
        private TodayAdapter todayAdapter;
        public Period Period;
        public PeriodVM vm;
        AdView _bannerad;
        private LinearLayout advertisements;

        public DateTime Date { get; set; }

        public Period DefaultPeriod { get; set; }

        public TodayViewFragment()
        {
            Date = DateTime.Now;
            DefaultPeriod = Date.Period();
        }

        /// <summary>
        /// can show meals of given date
        /// created to be used from Journal screen
        /// </summary>
        /// <param name="date"></param>
        public TodayViewFragment(DateTime date, string dateText, Period period)
        {
            Date = date;
            Title = dateText;
            DefaultPeriod = period;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            Instance = this;
            Title = Title ?? GetString(Resource.String.Today);
            var view = inflater.Inflate(Resource.Layout.fragment_pager, container, false);
            tabLayout = Activity.FindViewById<TabLayout>(Resource.Id.tabs);
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            tabLayout.Visibility = ViewStates.Visible;

            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                Platform.RunSafeOnUIThread("TodayViewFragment.OnCreateView",() =>
                {
                    periodDeleteVM = new PeriodDeleteVM();
                    periodDeleteVM.DeleteModeChanged += DeleteModeChanged;
                    setupViewPager(DefaultPeriod);

					SessionLog.EndPerformance("Navigate");

                });
            });

            FloatingActionButton fab = Activity.FindViewById<FloatingActionButton>(Resource.Id.fab_list);
            fab.SetRippleColor(Resource.Color.ripple_material_dark);
            fab.Visibility = ViewStates.Visible;
            fab.Click += (object sender, EventArgs ea) =>
            {

                try {
                    periodDeleteVM.InDeleteMode = false;
                    actionMode = ((AppCompatActivity)Activity).StartSupportActionMode(this);
                    actionMode.Finish();
                    //  periodDeleteVM.InDeleteMode = false;
                    //GrabAdIntertitials();
                    ShowNewDialog();
                } catch (Exception ex) { LittleWatson.ReportException(ex); }
            };
            return view;
        }

#if INTERSTITIAL
        public async void GrabAdIntertitials()
        {
            bannerView();
        }
        public async Task bannerView()
        {
			if (!AppStats.Current.ShouldShowInterstitials) return;
            advertisements = Activity.FindViewById<LinearLayout>(Resource.Id.advertisement);
            Context con = advertisements.Context;
            {
                _bannerad = AdWrapper.ConstructStandardBanner(con, AdSize.SmartBanner, "ca-app-pub-3167302081266616/1445459882");
                var listener = new adlistener();
                listener.AdLoaded += () => { };
                _bannerad.AdListener = listener;
                _bannerad.CustomBuild();

                var layout = Activity.FindViewById<LinearLayout>(Resource.Id.adbox);
                layout.AddView(_bannerad);
                var FinalAd = AdWrapper.ConstructFullPageAdd(con, "ca-app-pub-3167302081266616/1445459882");
                var intlistener = new adlistener();
                intlistener.AdLoaded += () => { if (FinalAd.IsLoaded)FinalAd.Show(); };
                FinalAd.AdListener = intlistener;
                FinalAd.CustomBuild();
            }
        }
#endif

        private async void ShowTrailPrompt ()
		{

			await Task.Delay (1000);
			if (Activity != null) {

				MessageCenter.AskForReviewIfAppropriate();

				var uniqueRunLastDate = AppStats.Current.LastTrialReminder;
				var trail = AppStats.Current.InstalledProductKind == AppStats.ProductKind.Trial;

				if (trail && DateTime.Now.Date != uniqueRunLastDate) {
					ShowTrialPrompt (); 
					//AppStats.Current.LastTrialReminder = DateTime.Now.Date;
				}
			}
		}

        private void ShowTrialPrompt()
        {
            var remainingDays = AppStats.Current.TrialDaysRemaining;

            if (AppStats.Current.DaysSinceInstall == 0)
            {
                Intent i = new Intent(this.Activity, typeof(FoodJournal.Android15.Trial));
                StartActivity(i);

            }
            else if (remainingDays > 1)
            {
                Dialog dialog = new Dialog(this.Activity, Resource.Style.FullHeightDialog);
                dialog.SetContentView(Resource.Layout.trial2);
                TextView txt = dialog.FindViewById<TextView>(Resource.Id.trialtime);

                String a = GetString(Resource.String.trial13);
                String b = string.Format("{0}", remainingDays);
                String c = a.Substring(a.IndexOf("{0}") + 3);
                a = a.Substring(0, a.IndexOf("{0}") - 1);

                SpannableStringBuilder builder = new SpannableStringBuilder();
                SpannableString black = new SpannableString(a);
                black.SetSpan(new Android.Text.Style.ForegroundColorSpan(Color.Black), 0, black.Length(), 0);
                builder.Append(black);
                SpannableString blue = new SpannableString(b);
                blue.SetSpan(new Android.Text.Style.ForegroundColorSpan(Application.Context.Resources.GetColor(Resource.Color.trialcolor)), 0, blue.Length(), 0);
                builder.Append(blue);

                SpannableString black1 = new SpannableString(c);
                black1.SetSpan(new Android.Text.Style.ForegroundColorSpan(Color.Black), 0, black1.Length(), 0);
                builder.Append(black1);
                txt.SetText(builder, Android.Widget.TextView.BufferType.Spannable);

                TextView more = dialog.FindViewById<TextView>(Resource.Id.trialtime1);
                more.Click += delegate
                {
                    dialog.Dismiss();
                    Intent i1 = new Intent(this.Activity, typeof(FoodJournal.Android15.Trial1));
                    i1.PutExtra("TrialData1", a);
                    i1.PutExtra("TrialData2", b);
                    i1.PutExtra("TrialData3", c);
                    StartActivity(i1);

                };
                TextView close = dialog.FindViewById<TextView>(Resource.Id.trialtime2);
                close.Click += delegate
                {
                    dialog.Dismiss();
                };
                dialog.Show();
            }
            else if (remainingDays == 1)
            {
                Dialog dialog = new Dialog(this.Activity, Resource.Style.FullHeightDialog);
                dialog.SetContentView(Resource.Layout.trial3);
                TextView buy = dialog.FindViewById<TextView>(Resource.Id.trialtime3);
                buy.Click += delegate
                {
                    dialog.Dismiss();
                    Navigate.ToBuyNowPage();
                };
                TextView close = dialog.FindViewById<TextView>(Resource.Id.trialtime4);
                close.Click += delegate
                {
                    dialog.Dismiss();
                };

                dialog.Show();
            }
            else
            {
                AppStats.Current.UnRegisterPurchase();
            }
        }

        private void DeleteModeChanged(object sender, bool DeleteMode)
        {
            var tabLayout = Activity.FindViewById<TabLayout>(Resource.Id.tabs);

            if (DeleteMode)
            {
                actionMode = ((AppCompatActivity)Activity).StartSupportActionMode(this);
                tabLayout.SetBackgroundColor(Resources.GetColor(Resource.Color.primaryDark));
            }
            else
            {
                tabLayout.SetBackgroundColor(Resources.GetColor(Resource.Color.accent));
                mainPagerAdapter.RefreshPeriod(periodDeleteVM.Period, false);

            }
        }

        private void setupViewPager(Period defaultPeriod)
        {
            mainPagerAdapter = new MainPagerAdapter(ChildFragmentManager);
            Navigate.selectedDate = Date.Date;
            Navigate.selectedPeriod = DefaultPeriod;
            var current = 0;

            foreach (var period in Cache.GetDatePeriods(Navigate.selectedDate))
            {
                TodayFragment fragment = new TodayFragment(this);
                Bundle bundle = new Bundle();
                bundle.PutString("date", Navigate.selectedDate.ToStorageStringDate());
                bundle.PutString("period", period.ToString());
                fragment.Period = period;
                fragment.Arguments = bundle;
                if (defaultPeriod == period)
                    current = mainPagerAdapter.Count;
                mainPagerAdapter.addFragment(fragment, Strings.FromEnum(period));
            }

            if (viewPager.Adapter == null)
                viewPager.AddOnPageChangeListener(this);

          //  viewPager.OffscreenPageLimit = 5;
            viewPager.Adapter = mainPagerAdapter;

            tabLayout.SetupWithViewPager(viewPager);
            tabLayout.TabMode = TabLayout.ModeScrollable;
            viewPager.SetCurrentItem(current, true);
            InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(viewPager.WindowToken, 0);
        }

        public void ShowNewDialog()
        {
            View dialogContent = View.Inflate(Activity, Resource.Layout.dialog, null);
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(Activity, Resource.Style.Dialog);
            builder.SetView(dialogContent);

            var dialogList = dialogContent.FindViewById<Android.Support.V7.Widget.RecyclerView>(Resource.Id.recyclerDialog);
            dialogList.HasFixedSize = false;

            LinearLayoutManager mLayoutManager = new LinearLayoutManager(Activity);
            dialogList.SetLayoutManager(mLayoutManager);

            SearchVM vm = new SearchVM(Navigate.selectedPeriod);

            var addItemDialog = dialogContent.FindViewById<Android.Widget.ImageView>(Resource.Id.addItemDialog);

            var addInputDialog = dialogContent.FindViewById<Android.Widget.EditText>(Resource.Id.addInputDialog);
            addInputDialog.TextChanged += (sender, e) =>
            {
                vm.Query = addInputDialog.Text;
            };


            builder.SetPositiveButton(GetString(Resource.String.Add_New), (a, b) =>
            {
                var svm = new SearchResultNewVM("", "");
                Navigate.selectedDate = Date.Date;
                vm.OnItemTap(svm);
                //bannerView();
            });
            var notes = Cache.GetPeriodNote(Navigate.selectedDate, Navigate.selectedPeriod);

            builder.SetNeutralButton(GetString(Resource.String.Add_Note), (a, b) =>
            {
                Navigate.selectedDate = Date.Date;
                Cache.SetPeriodNote(Date.Date, Navigate.selectedPeriod, notes ?? "");
                mainPagerAdapter.RefreshPeriod(Navigate.selectedPeriod, true);
            });

            var cardsAdapter = new SearchResultRecyclerAdapter(Activity, vm, SearchResultRecyclerAdapter.SRCType.Search);
            dialogList.SetAdapter(cardsAdapter);
            dialogList.GetItemAnimator().RemoveDuration = 120;

            var dialog = builder.Create();

            cardsAdapter.Click = (svm) =>
            {
                Navigate.selectedDate = Date.Date;
                if (!svm.IsLocked)
                {
                    dialog.Dismiss();
                }
                vm.OnItemTap(svm); 
                mainPagerAdapter.RefreshPeriod(Navigate.selectedPeriod, false);
                
            };

            dialog.Show();
        }

        public bool OnActionItemClicked(Android.Support.V7.View.ActionMode mode, IMenuItem item)
        {
            periodDeleteVM.DeleteAll();
            actionMode.Finish();
          //  mainPagerAdapter.RefreshPeriod(periodDeleteVM.Period, false);
            //  periodDeleteVM.InDeleteMode = false;            
            return true;
        }

        public bool OnCreateActionMode(Android.Support.V7.View.ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(Resource.Menu.menu_cab, menu);
            return true;
        }

        public void OnDestroyActionMode(Android.Support.V7.View.ActionMode mode)
        {
            periodDeleteVM.InDeleteMode = false;
        }

        public bool OnPrepareActionMode(Android.Support.V7.View.ActionMode mode, IMenu menu)
        {
            return true;
        }

        public void OnPageScrollStateChanged(int state)
        {
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
        }

        public void OnPageSelected(int position)
        {
            periodDeleteVM.InDeleteMode = false;
            if (mainPagerAdapter.GetItem(viewPager.CurrentItem) is TodayFragment)
                Navigate.selectedPeriod = (mainPagerAdapter.GetItem(viewPager.CurrentItem) as TodayFragment).Period;
        }

        public override void OnResume()
        {
            base.OnResume();
            Task.Run(() =>
            {
                Task.Delay(1000).Wait();
                Platform.RunSafeOnUIThread("TodayViewFragment.OnResume",() =>
                {
                  //  setupViewPager(Navigate.selectedPeriod);
                    if (Cache.GetDatePeriods(Navigate.selectedDate).Count != mainPagerAdapter.Count)
                    {
                        setupViewPager(Period.Breakfast);
                        return;
                    }
                    ShowTrailPrompt();
                });
            });
        }

        public Android.Support.V7.View.ActionMode actionMode { get; set; }

    }
}