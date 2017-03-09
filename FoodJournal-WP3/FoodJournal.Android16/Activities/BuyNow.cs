using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using FoodJournal.Logging;
using FoodJournal.Resources;
using FoodJournal.AppModel;
using Android.Content;

namespace FoodJournal.Android15.Activities
{
    [Android.App.Activity(Theme = "@style/AppThemeDialog", LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
	public class BuyNow : BillingActivity
	{
		private TextView line2;

		public BuyNow () : base (true)
		{
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

            if (AppStats.Current.Culture.ToLower() == "en-us")
            {

                SetContentView(Resource.Layout.BuySubscription);
                this.FindViewById<Android.Widget.LinearLayout>(Resource.Id.buy1).Click += (s, e) => { BuyProduct("sub_monthly"); };
                this.FindViewById<Android.Widget.LinearLayout>(Resource.Id.buy2).Click += (s, e) => { BuyProduct("sub_quarterly"); };
                this.FindViewById<Android.Widget.LinearLayout>(Resource.Id.buy3).Click += (s, e) => { BuyProduct("sub_yearly"); };
                this.FindViewById<Android.Widget.TextView>(Resource.Id.dismissBuy).Click += (sender, e) => { Finish(); };

            }
            else
            { 

			    SetContentView (Resource.Layout.BuyNow);
                this.FindViewById<Android.Widget.TextView>(Resource.Id.buynow).Click += HandleClick;
                this.FindViewById<Android.Widget.TextView>(Resource.Id.dismissBuy).Click += (sender,e) => { Finish();  };
			    line2 = this.FindViewById<TextView> (Resource.Id.line2);
			    line2.Visibility = ViewStates.Gone;

            }
        }
        
        protected override void OnProductFound (string key, string text, string price)
        {
            if (line2 != null)
            {
                line2.Text = string.Format(AppResources.Price, price);
                line2.Visibility = ViewStates.Visible;
            }
        }

		protected override void OnProductPurchased(string productid)
		{
			AppStats.Current.RegisterPurchase (productid);
            Finish();
		}

		void HandleClick (object sender, EventArgs e)
		{
            BuyProduct("premium");
		}

        private void BuyProduct(string product)
        {
            try
            {
                SessionLog.RecordMilestone("Buying " + product, AppStats.Current.SessionId.ToString());
                if (_serviceConnection != null)
                {
#if DEBUG
                    _serviceConnection.BillingHandler.BuyProduct("android.test.purchased", "inapp", "");
#else
                    _serviceConnection.BillingHandler.BuyProduct (product, "inapp", "");
#endif
                }
            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }

        protected override List<MenuLink> GetMenuItems ()
		{
			return null;
		}

	}
}

