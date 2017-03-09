using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using FoodJournal.Values;
using FoodJournal.Logging;
using FoodJournal.Resources;

namespace FoodJournal.Android15.Adapters
{
	public class JournalViewPagerAdapter : FragmentStatePagerAdapter, ISwipingPagerAdapter<DateTime>
    {

		private DateTime center = DateTime.Now.Date;

		private int cnt = 800;
		private int today = 400;

		public JournalViewPagerAdapter(FragmentManager fm)
            : base(fm)
        {}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			var title = position == today ? AppResources.Today : GetItemFromId(position).ToShortDateString();
			SessionLog.Debug (string.Format ("Page Title: {0}", title));
			return new Java.Lang.String(title);
		}

		public string GetPageSubTitle(int position)
		{
			return position == today ? null : GetItemFromId(position).ToString("dddd");
		}

//		public void InsertOne() 
//		{
//			cnt += 2;
//			today += 2;
//			this.NotifyDataSetChanged ();
//		}

		public int GetIdFromItem (DateTime item)
		{
			return ((int)item.Subtract(center).TotalDays) + today;
		}

		public DateTime GetItemFromId (int id)
		{
			return center.AddDays(id-today);
		}

		public int NextItemId (bool inc, int currentid)
		{
			return inc ? (currentid < cnt - 1 ? currentid + 1 : currentid) : (currentid >0 ? currentid -1: currentid);
		}

       public override Fragment GetItem(int position)
        {
			DateTime date = GetItemFromId (position);
			#if !DEBUG1
			return JournalFragment.newInstance(date);
			#else
			return null;
			#endif
        }

        public override int Count
        {
            get
            {
                // Show 3 total pages.
				return cnt;
            }
        }

    }
}