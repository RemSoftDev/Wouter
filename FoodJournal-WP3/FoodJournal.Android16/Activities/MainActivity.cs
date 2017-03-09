using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using FoodJournal.ViewModels;
using FoodJournal.Runtime;
using FoodJournal.Values;
using FoodJournal.Extensions;
using Android.Support.V4.App;
using FoodJournal.Resources;
using FoodJournal.Android15.Fragments.AppFragments;
using System.Threading.Tasks;
using System.Threading;
using Android.Gms.Ads;
using Android.Widget;
using FoodJournal.AppModel;
using FoodJournal.Android15.Activities;
using FoodJournal.Logging;

namespace FoodJournal.Android15
{
    [Android.App.Activity(Label = "MainActivity", MainLauncher = false, ConfigurationChanges = Android.Content.PM.ConfigChanges.Locale | Android.Content.PM.ConfigChanges.LayoutDirection | Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, WindowSoftInputMode = SoftInput.StateHidden)]
    public class MainActivity : Android.Support.V7.App.AppCompatActivity
    {
        private Android.Support.V7.App.ActionBarDrawerToggle drawerToggle;
        private Android.Support.V4.Widget.DrawerLayout drawerLayout;
        private NavigationView navigationView;
        AdView _bannerad;
        private LinearLayout advertisements;
                
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
			SessionLog.StartPerformance("Navigate");

            if (bundle == null)
                bundle = Intent.Extras;

            if (bundle == null)
                bundle = new Bundle();

            SetContentView(Resource.Layout.activity_main);
            Navigate.navigationContext = this;
            ResourceData.ResourceDatabase2.assetManager = Assets;

            if (Navigate.selectedPeriod == Period.none)
            {
                Navigate.selectedDate = DateTime.Now.Date;
                Navigate.selectedPeriod = DateTime.Now.Period();
            }
            
            drawerLayout = this.FindViewById<Android.Support.V4.Widget.DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = this.FindViewById<NavigationView>(Resource.Id.navigation);
            
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var actionBar = this.SupportActionBar;
            actionBar.SetTitle(Resource.String.Today);
            actionBar.SetDisplayHomeAsUpEnabled(true);
            setupDrawerContent();
            SupportActionBar.SetTitle(Resource.String.Today);
            SupportFragmentManager.BeginTransaction()
                           .Replace(Resource.Id.cab_stub, new TodayViewFragment(Navigate.selectedDate, AppResources.Today, Navigate.selectedPeriod))
                           .Commit();
#if SCREENSHOT
            Navigate.selectedPeriod = Period.Breakfast;
            Navigate.screenshotScreen = this;
            Fragment fragment = null;
            var screen = Intent.GetStringExtra("Screen");
            if (screen != null)
            { 
                screen = screen.ToLower();
                if (screen == "today")
                {
                    fragment = new TodayViewFragment(Navigate.selectedDate, AppResources.Today, Navigate.selectedPeriod);
                }
                else if (screen == "journal")
                {
                    fragment = new JournalViewFragment();
                }
                else if (screen == "report")
                {
                    fragment = new ReportFragment();
                }
                else if (screen == "goal")
                {
                    fragment = new GoalViewFragment();
                }
                if (fragment != null)
                    SupportFragmentManager.BeginTransaction()
                                   .Replace(Resource.Id.cab_stub, fragment)
                                   .Commit();
            }

#endif
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            drawerToggle.SyncState();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Navigate.navigationContext = this;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Android.Views.InputMethods.InputMethodManager inputMethodManager = (Android.Views.InputMethods.InputMethodManager)this.GetSystemService(MainActivity.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
            if (drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
  					 drawerLayout.OpenDrawer(GravityCompat.Start);
                    return true;

            }
#if SCREENSHOT
            if (item != null && item.TitleFormatted != null)
            {
                if (AppResources.TakeAPicture == item.TitleFormatted.ToString())
                {
                    Task.Run(() =>
                    {
                        RunOnUiThread(() =>
                        {
                            if (!ScreenshotVM.InScreenshot)
                            {
                                //ScreenshotVM.CaptureScreens(Navigate.screenshotScreen);
                            }
                        });
                    });
                }
            }
#endif
            return base.OnOptionsItemSelected(item);
        }

        private void setupDrawerContent()
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
				SessionLog.StartPerformance("Navigate");
                var menuItem = e.MenuItem;
                menuItem.SetChecked(true);
                switch (menuItem.ItemId)
                {
                    case Resource.Id.navigation_item_1:
                            SupportFragmentManager.BeginTransaction()
                           .Replace(Resource.Id.cab_stub, new TodayViewFragment())
                           .AddToBackStack(null)
                           .Commit();
                            SupportActionBar.SetTitle(Resource.String.Today);
                        break;
                    case Resource.Id.navigation_item_2:
                       
                        SupportFragmentManager.BeginTransaction()
                              .Replace(Resource.Id.cab_stub, new JournalViewFragment())
							  .AddToBackStack("JournalViewFragment")
                              .Commit();
                        //GrabAdIntertitials();
                        SupportActionBar.SetTitle(Resource.String.OpenJournal);
                        break;
                    case Resource.Id.navigation_item_3:
                        SupportFragmentManager.BeginTransaction()
                             .Replace(Resource.Id.cab_stub, new ReportFragment())
                             .AddToBackStack("ReportViewFragment")
                             .Commit();
                        //GrabAdIntertitials();
                        SupportActionBar.SetTitle(Resource.String.Report);
                        break;
                    case Resource.Id.navigation_item_4:
                        SupportFragmentManager.BeginTransaction()
                             .Replace(Resource.Id.cab_stub, new GoalViewFragment())
                             .AddToBackStack("GoalViewFragment")
                             .Commit();
                        SupportActionBar.SetTitle(Resource.String.Goals);
                        break;
                    case Resource.Id.navigation_item_5:
                        Navigate.ToSettingsPage();
                        break;
                    case Resource.Id.navigation_item_6:
                        Navigate.ToFeedback();
                        break;
                    default:
                        break;
                }
                menuItem.SetChecked(true);
                drawerLayout.CloseDrawers();
            };

            drawerToggle = new ActionBarDrawerToggleAnonymousInnerClassHelper(this, drawerLayout, Resource.String.drawer_open, Resource.String.drawer_close);
             drawerLayout.SetDrawerListener(drawerToggle);
        }

        private class ActionBarDrawerToggleAnonymousInnerClassHelper : Android.Support.V7.App.ActionBarDrawerToggle
        {
            private Android.App.Activity activity;
            public ActionBarDrawerToggleAnonymousInnerClassHelper(Android.App.Activity activity, Android.Support.V4.Widget.DrawerLayout drawerLayout, int drawer_open, int drawer_close)
                : base(activity, drawerLayout, drawer_open, drawer_close)
            { this.activity = activity; }

            public override void OnDrawerOpened(View drawerView)
            {
                    base.OnDrawerOpened(drawerView);
                    activity.InvalidateOptionsMenu();
            }

            public override void OnDrawerClosed(View drawerView)
            {
                base.OnDrawerClosed(drawerView);
                activity.InvalidateOptionsMenu();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
#if SCREENSHOT
            menu.Add(AppResources.TakeAPicture);
#endif
            return base.OnCreateOptionsMenu(menu);
        }

#if INTERSTITIAL
        public async void GrabAdIntertitials()
        {
            bannerView();
        }
        public async Task bannerView()
        {
			if (!AppStats.Current.ShouldShowInterstitials) return;

            advertisements = FindViewById<LinearLayout>(Resource.Id.advertisement);
            Context con = advertisements.Context;
            {
                _bannerad = AdWrapper.ConstructStandardBanner(con, AdSize.SmartBanner, "ca-app-pub-3167302081266616/1445459882");
                var listener = new adlistener();
                listener.AdLoaded += () => { };
                _bannerad.AdListener = listener;
                _bannerad.CustomBuild();

                var layout = FindViewById<LinearLayout>(Resource.Id.adbox);
                layout.AddView(_bannerad);
                var FinalAd = AdWrapper.ConstructFullPageAdd(con, "ca-app-pub-3167302081266616/1445459882");
                var intlistener = new adlistener();
                intlistener.AdLoaded += () => { if (FinalAd.IsLoaded)FinalAd.Show(); };
                FinalAd.AdListener = intlistener;
                FinalAd.CustomBuild();
    }
}
#endif
    }
}

