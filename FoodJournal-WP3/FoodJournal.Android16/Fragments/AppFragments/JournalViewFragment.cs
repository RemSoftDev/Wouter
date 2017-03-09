using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using FoodJournal.Runtime;
using Android.Views.InputMethods;
using FoodJournal.Android15.Adapters;
using FoodJournal.Values;
using System.Linq;
using FoodJournal.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Widget;
using FoodJournal.AppModel;
using FoodJournal.Logging;
namespace FoodJournal.Android15.Fragments.AppFragments
{
    public class JournalViewFragment : BaseViewFragment, Android.Support.V4.View.ViewPager.IOnPageChangeListener
    {
        ViewPager viewPager;
        MainPagerAdapter mainPagerAdapter;
        
        TabLayout tabLayout;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Title = GetString(Resource.String.OpenJournal);
            var view = inflater.Inflate(Resource.Layout.fragment_pager, container, false);
            tabLayout = Activity.FindViewById<TabLayout>(Resource.Id.tabs);
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            tabLayout.Visibility = ViewStates.Visible;
            Activity.FindViewById<FloatingActionButton>(Resource.Id.fab_list).Visibility = ViewStates.Gone;
            return view;
        }

        private void setupViewPager()
        {
            mainPagerAdapter = new MainPagerAdapter(ChildFragmentManager);
            int current = 0;
            List<Property> properties = UserSettings.Current.SelectedProperties;
            if (properties.Count == 0)
                properties.Add(StandardProperty.none);
            var props = UserSettings.Current.SelectedProperties;
            if (props.Count == 1 &&
                props.FirstOrDefault().ID == "00")
            {
                var prop = props.FirstOrDefault();
                var fragment = new JournalFragment();
                Bundle bundle = new Bundle();
                bundle.PutString("date", Navigate.selectedDate.ToStorageStringDate());
                bundle.PutString("property", prop.ID);
                fragment.Arguments = bundle;
                current = mainPagerAdapter.Count;
                mainPagerAdapter.addFragment(fragment, GetString(Resource.String.NoGoalsTitle));
            }
            else
            {
                foreach (var property in props.Where(a => a.ID != "00").ToList())
                {
                    var fragment = new JournalFragment();
                    Bundle bundle = new Bundle();
                    bundle.PutString("date", Navigate.selectedDate.ToStorageStringDate());
                    bundle.PutString("property", property.ID);

                    fragment.Arguments = bundle;
                    if (UserSettings.Current.CurrentProperty == property)
                        current = mainPagerAdapter.Count;
                    mainPagerAdapter.addFragment(fragment, property.TextOnly);
                }
            }

            try
            {
                if (viewPager.Adapter == null)
                    viewPager.AddOnPageChangeListener(this);
                viewPager.Adapter = mainPagerAdapter;
                tabLayout.SetupWithViewPager(viewPager);
                tabLayout.TabMode = TabLayout.ModeScrollable;
            }
            catch (Exception ex)
            {
                throw;
            }

            viewPager.SetCurrentItem(current, false);

            InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(viewPager.WindowToken, 0);

			SessionLog.EndPerformance("Navigate");

        }

        public void OnPageScrollStateChanged(int state) { }
        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels) { }
        public void OnPageSelected(int position)
        {
            if (mainPagerAdapter.GetItem(viewPager.CurrentItem) is TodayFragment)
                Navigate.selectedPeriod = (mainPagerAdapter.GetItem(viewPager.CurrentItem) as TodayFragment).Period;
        }

        public override void OnResume()
        {
            base.OnResume();
            Task.Run(() =>
            {
                Task.Delay(40).Wait();
                Platform.RunSafeOnUIThread("JournalViewFragment.OnResume",() =>
                {
                    setupViewPager();                   
                });
            });
        }
    }
}