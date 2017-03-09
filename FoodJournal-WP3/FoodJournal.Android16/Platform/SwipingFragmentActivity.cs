using System;

//using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
//using Android.Widget;
using Android.OS;
using FoodJournal.Model;
using FoodJournal.AppModel;
using System.Collections.Generic;
using FoodJournal.Android15.Adapters;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Values;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using FoodJournal.Logging;
using Android.Util;
using FoodJournal.Runtime;

namespace FoodJournal.Android15.Activities
{

	public abstract class SwipingFragmentActivity<T> : StandardActivity, ViewPager.IOnPageChangeListener
	{

		private ViewPager viewpager;
		private ISwipingPagerAdapter<T> ipageradapter;

		private Dictionary<T,Fragment> activefragments;

		protected abstract ISwipingPagerAdapter<T> GetPagerAdapter (FragmentManager fragmentManager);

		protected abstract void OnPageSelected (T item);

		public SwipingFragmentActivity(): base(true) {}

		protected override void OnCreate (Bundle bundle)
		{

			base.OnCreate (bundle);

			FoodJournal.Runtime.Navigate.SetActiveActivity (this);

			SetContentView (Resource.Layout.SwipingFragmentActivity);
			viewpager = FindViewById<ViewPager> (Resource.Id.viewPager1);

				SetSupportActionBar (FindViewById<Toolbar> (Resource.Id.toolbar));

			#if DEBUG
			screenshotview=viewpager;
			#endif

			ipageradapter = GetPagerAdapter (this.SupportFragmentManager);
			viewpager.Adapter = ipageradapter as PagerAdapter;
			viewpager.SetOnPageChangeListener (this);

			//return;
			ActionBar actionBar = this.SupportActionBar; //this.ActionBar;
			//actionBar.NavigationMode = ActionBarNavigationMode.Tabs;
			//actionBar.SetDisplayOptions(ActionBar.NavigationMode ActionBarDisplayOptions, ActionBarDisplayOptions.ShowCustom)
			//actionBar.set.setNavigationMode(ActionBar.NAVIGATION_MODE_TABS);
			actionBar.SetDisplayHomeAsUpEnabled (true);

			UpdateTitle ();

		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			this.MenuInflater.Inflate (Resource.Menu.main_activity_actions, menu);
			return base.OnCreateOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if (item.ItemId == Resource.Id.action_left)
				GestureLeft (null, null);
			else if (item.ItemId == Resource.Id.action_right)
				GestureRight (null, null);
			else
				return base.OnOptionsItemSelected (item);
			return true;
		}

		public void OnPageScrollStateChanged (int state)
		{
		}

		public void OnPageScrolled (int position, float positionOffset, int positionOffsetPixels)
		{
		}

		public void SetActiveFragment(T value, Fragment fragement)
		{
			if (activefragments == null)
				activefragments = new Dictionary<T, Fragment> ();

			activefragments [value] = fragement;

		}

		public Fragment SelectedFragment {
			get { return activefragments == null ? null : activefragments[SelectedItem]; }
		}

		public void OnPageSelected (int position)
		{
			IRefreshableFragment fr = SelectedFragment as IRefreshableFragment;
//			IRefreshableFragment fr = ipageradapter.GetItem (position) as IRefreshableFragment;
			if (fr != null)
				fr.Refresh ();

			this.OnPageSelected (ipageradapter.GetItemFromId (position));
			UpdateTitle ();
		}

//		public void JumpToSelectedItem(T value)
//		{
//			viewpager.SetCurrentItem (pageradapter.GetIdFromItem (value), false);
//		}

		public T SelectedItem {
			get{ return ipageradapter.GetItemFromId (viewpager.CurrentItem); }
			set { viewpager.SetCurrentItem (ipageradapter.GetIdFromItem (value), true); }
		}

		protected new void UpdateTitle ()
		{
			int position = viewpager.CurrentItem;
			this.SupportActionBar.Title = (ipageradapter as PagerAdapter).GetPageTitle (position);//.ToUpper();
			var subttitle = ipageradapter.GetPageSubTitle (position);
			this.SupportActionBar.Subtitle = subttitle==null ? null : subttitle.ToUpper();
		}

		void GestureLeft (MotionEvent first, MotionEvent second)
		{
			//animator.SetInAnimation(this, Resource.Animation.inright);
			//animator.SetOutAnimation(this, Resource.Animation.outleft);
			viewpager.SetCurrentItem (ipageradapter.NextItemId (false, viewpager.CurrentItem), true);
		}

		void GestureRight (MotionEvent first, MotionEvent second)
		{
			viewpager.SetCurrentItem (ipageradapter.NextItemId (true, viewpager.CurrentItem), true);
			//Toast.MakeText (this, "You swipe right on the ", ToastLength.Long).Show ();
		}

	}

}

