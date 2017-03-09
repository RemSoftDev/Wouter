
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
using FoodJournal.Android15;
using Android.Support.V7.App;
using System.Threading.Tasks;

namespace FoodJournal.Android15.Activities
{
	[Activity (Label = "@string/AppTitle", MainLauncher = true, Icon = "@drawable/ic_icon", Theme = "@style/AppTheme8")]
	public class Splash : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			FoodJournal.Runtime.Navigate.SetActiveActivity (this);

			this.SetTheme (Resource.Style.AppTheme8);

			SetContentView (Resource.Layout.Splash);

           Task.Run(() =>
            {
                Task.Delay(100).Wait();
               if (!App.IsSessionInitialized && FoodJournal.Model.Data.FoodJournalDB.MigrationNeeded)
               {
                   FoodJournal.Model.Data.FoodJournalDB.Migrate();
               }

               if (App.IsSessionInitialized || App.InitSession(this))
               {
                   Intent i = new Intent(this, typeof(FoodJournal.Android15.MainActivity));
                   i.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                   StartActivity(i);
                   Finish();
               }
            });            
		}
	}
}