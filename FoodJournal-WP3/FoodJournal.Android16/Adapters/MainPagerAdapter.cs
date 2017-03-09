using System;
using System.Collections.Generic;
using Android.Support.V4.App;
using FoodJournal.Logging;

namespace FoodJournal.Android15.Adapters
{
    public class MainPagerAdapter : Android.Support.V4.App.FragmentPagerAdapter
    {
        private List<Fragment> mFragments = new List<Fragment>();
        private List<String> mFragmentTitles = new List<String>();
        FragmentManager fm;
        public MainPagerAdapter(Android.Support.V4.App.FragmentManager fm)
            : base(fm)
        {
            this.fm = fm;
        }

        public void insertFragment(int position, Fragment fragment, String title)
        {
            mFragments.Insert(position, fragment);
            mFragmentTitles.Insert(position, title);
        }

        public void addFragment(Fragment fragment, String title)
        {
            mFragments.Add(fragment);
            mFragmentTitles.Add(title);
        }

        //public void RemoveItem(int position)
        //{
        //    mFragments.RemoveAt(position);
        //    mFragmentTitles.RemoveAt(position);
        //}

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            var item = mFragments[position];
            return item;
        }

        public override int Count
        {
            get
            {
                return mFragments.Count;
            }
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(mFragmentTitles[position]);
        }

        public void RefreshPeriod(Values.Period period, bool FocusNote)
        {
			try{
	            foreach (var fragment in mFragments)
	                if (fragment is TodayFragment)
	                    if ((fragment as TodayFragment).Period == period)
	                    {
	                        (fragment as TodayFragment).Refresh();
	                        if (FocusNote)
	                            (fragment as TodayFragment).FocusNote();
	                    }
			} catch (Exception ex) {LittleWatson.ReportException (ex);}
        }
    }
}