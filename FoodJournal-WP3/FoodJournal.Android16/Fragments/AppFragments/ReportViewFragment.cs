using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views.InputMethods;
using FoodJournal.Resources;
using FoodJournal.Android15.Adapters;

namespace FoodJournal.Android15.Fragments.AppFragments
{
    public class ReportViewFragment : BaseViewFragment
    {
        ViewPager viewPager;
        MainPagerAdapter mainPagerAdapter;
        TabLayout tabLayout;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Title = GetString(Resource.String.Report);
            var view = inflater.Inflate(Resource.Layout.fragment_pager, container, false);
            tabLayout = Activity.FindViewById<TabLayout>(Resource.Id.tabs);
            viewPager = view.FindViewById<ViewPager>(Resource.Id.viewpager);
            tabLayout.Visibility = ViewStates.Gone;
            setupViewPager();
            Activity.FindViewById<FloatingActionButton>(Resource.Id.fab_list).Visibility = ViewStates.Gone;
            return view;
        }

        private void setupViewPager()
        {
            mainPagerAdapter = new MainPagerAdapter(ChildFragmentManager);
            int current = 0;
            var fragment = new ReportFragment();
            mainPagerAdapter.addFragment(fragment, AppResources.Report);
            viewPager.Adapter = mainPagerAdapter;
            tabLayout.SetupWithViewPager(viewPager);
            tabLayout.TabMode = TabLayout.ModeScrollable;
            viewPager.SetCurrentItem(current, false);
            InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(viewPager.WindowToken, 0);
        }
    }
}