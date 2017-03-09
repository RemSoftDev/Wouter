using System;
using FoodJournal.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FoodJournal.AppModel;

namespace FoodJournal.Android15.Activities
{
    [Android.App.Activity(Theme = "@style/AppTheme")]
    public class AboutActivity : AppCompatActivity
    {
        private BillingWrapper BillingWrapper;

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_about_info);
           
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_addItem);
            SetSupportActionBar(toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            this.FindViewById<LinearLayout>(Resource.Id.layout_about_upgrade).Click += (object sender, EventArgs e) =>
            {
                Navigate.ToBuyNowPage();
            };
            this.FindViewById<LinearLayout>(Resource.Id.layout_about_rate).Click += (object sender, EventArgs e) => { Navigate.StartReviewTask(); };
            this.FindViewById<LinearLayout>(Resource.Id.layout_about_report).Click += (object sender, EventArgs e) => { Navigate.StartEmailTask(); };
            this.FindViewById<LinearLayout>(Resource.Id.layout_about_share).Click += (object sender, EventArgs e) => { Navigate.StartShareTask(); };
            this.FindViewById<LinearLayout>(Resource.Id.layout_about_upgrade).Visibility = ViewStates.Gone;
			this.FindViewById<LinearLayout>(Resource.Id.layout_about_premiumpurchased).Visibility = AppStats.Current.InstalledProductKind != AppStats.ProductKind.Paid ? ViewStates.Gone : ViewStates.Visible;

        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home) { Finish(); return true; }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnDestroy()
        {
#if !AMAZON
            if (BillingWrapper != null)
                BillingWrapper.OnDestroy();
#endif
            base.OnDestroy();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AppStats.Current.InstalledProductKind != AppStats.ProductKind.Paid)
            {
#if !AMAZON
                BillingWrapper = new BillingWrapper(this);
                BillingWrapper.OnConnected = () =>
                {
                    this.FindViewById<LinearLayout>(Resource.Id.layout_about_upgrade).Visibility = ViewStates.Visible;
                };
                BillingWrapper.OnProductPurchased = (name) =>
                {
                    this.FindViewById<LinearLayout>(Resource.Id.layout_about_upgrade).Visibility = ViewStates.Gone;
                    this.FindViewById<LinearLayout>(Resource.Id.layout_about_premiumpurchased).Visibility = ViewStates.Visible;
                    this.FindViewById<View>(Resource.Id.view3).Visibility = ViewStates.Visible;
                    this.FindViewById<View>(Resource.Id.view2).Visibility = ViewStates.Gone;
                };
                BillingWrapper.Connect();
#endif
            }
            else
            {
                this.FindViewById<LinearLayout>(Resource.Id.layout_about_premiumpurchased).Visibility = ViewStates.Visible;
                this.FindViewById<LinearLayout>(Resource.Id.layout_about_upgrade).Visibility = ViewStates.Gone;
                this.FindViewById<View>(Resource.Id.view2).Visibility = ViewStates.Gone;
                this.FindViewById<View>(Resource.Id.view3).Visibility = ViewStates.Visible;
            }
        }
    }
}

