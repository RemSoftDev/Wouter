using System;

//using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FoodJournal.Model;
using FoodJournal.AppModel;
using System.Collections.Generic;
using FoodJournal.Android15.Adapters;
using FoodJournal.ViewModels;
using FoodJournal.Resources;
using FoodJournal.Values;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Support.V4.App;
using FoodJournal.Logging;
using Android.Util;
using FoodJournal.Runtime;
using Java.Lang.Reflect;

namespace FoodJournal.Android15.Activities
{

	public abstract class StandardActivity : AppCompatActivity, ActionBar.ITabListener
	{

		private List<MenuLink> menuitems;
		//private bool ActionBar;

		protected abstract List<MenuLink> GetMenuItems ();
        public StandardActivity() { }
		public StandardActivity(bool ActionBar) {
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			FoodJournal.Runtime.Navigate.SetActiveActivity (this);
			return;
		}

		protected void UpdateTitle ()
		{
			//this.SupportActionBar.Title = "Unnamed page";
		}

		public void OnTabReselected (ActionBar.Tab tab, FragmentTransaction ft)
		{
		}

		public void OnTabUnselected (ActionBar.Tab tab, FragmentTransaction ft)
		{
		}

		public void OnTabSelected (ActionBar.Tab tab, FragmentTransaction ft)
		{
			try {
			} catch (Exception ex) {
				LittleWatson.ReportException (ex);
			}
		}

		public override bool OnSupportNavigateUp ()
		{
			this.Finish ();
			return base.OnSupportNavigateUp ();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{

			if (menuitems != null && item != null && item.TitleFormatted != null)
				foreach (var option in menuitems)
					if (option.Text == item.TitleFormatted.ToString())
						option.Invoke ();
			
			return base.OnOptionsItemSelected (item);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			#if DEBUG
			//if (menuitems != null)
				//throw new ArgumentOutOfRangeException ();
			#endif

			menuitems = GetMenuItems ();

			#if DEBUG
			if (ScreenshotVM.InScreenshot)
				menuitems=null;
			#endif

			if (menuitems != null) {

				//MenuAppendFlags.
//				var overflow = menu.AddSubMenu ("")
//					.SetIcon (Resource.Drawable.ic_overflow)
//					;

				foreach (MenuLink submenu in menuitems)
					menu.Add (submenu.Text);
			}

			return base.OnCreateOptionsMenu (menu);
		}

		//		public void OnTabUnselected (ActionBar.Tab tab, FragmentTransaction ft)
		//		{
		//			//ft.Detach(mFragment);
		//			//ft.Detach(mFragment);
		//			//throw new NotImplementedException();
		//		}

		protected override void OnStart ()
		{
			base.OnStart ();

			// make sure we have all needed session variables (we may be woken up after going to the background)
			// if weŕe not woken up, we may be navigating. onstop will be called on the previous activity after onstart

			Navigate.navigationContext = this;
			//App.InitSession (this);

		}

		protected override void OnStop ()
		{
			base.OnStop ();
			//App.FinalizeSession ();
		}

	}

}

