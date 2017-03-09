
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using FoodJournal.Runtime;
using FoodJournal.Resources;

namespace FoodJournal.Android15.Activities
{
	[Activity (Theme = "@style/AppTheme")]
	public class Feedback : BillingActivity
	{

		public Feedback(): base(true) {}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.Feedback);
			SetSupportActionBar (FindViewById<Android.Support.V7.Widget.Toolbar> (Resource.Id.toolbar));
			this.SupportActionBar.Title = AppResources.AppTitle;
			this.FindViewById<Button>(Resource.Id.unlock).Click += (object sender, EventArgs e) => {UnlockTap(null);};
			this.FindViewById<Button>(Resource.Id.rate).Click += (object sender, EventArgs e) => {RateTap(null);};
			this.FindViewById<Button>(Resource.Id.email).Click += (object sender, EventArgs e) => {EmailTap(null);};
			this.FindViewById<Button>(Resource.Id.share).Click += (object sender, EventArgs e) => {ShareTap(null);};
		}

		protected override void OnBillingConnected()
		{
			this.FindViewById<FrameLayout> (Resource.Id.unlockframe).Visibility = ViewStates.Visible;
		}

		private void RateTap (View view)
		{	
			Navigate.StartReviewTask();	
		}

		private void EmailTap (View view)
		{		
			Navigate.StartEmailTask();		
		}

		private void ShareTap (View view)
		{	
			Navigate.StartShareTask();		
		}

		private void UnlockTap (View view)
		{			
			DoDestroyNow = false;
			Navigate.ToBuyNowPage();
		}

		private void RemoveAdsTap (View view)
		{
		}

		private void TranslateTap (View view)
		{
		}

		protected override List<MenuLink> GetMenuItems ()
		{
			return null;
		}
	}
}