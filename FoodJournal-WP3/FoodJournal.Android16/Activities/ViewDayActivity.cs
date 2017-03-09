using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FoodJournal.Runtime;
using Android.Support.V7.App;
using FoodJournal.Android15.Fragments.AppFragments;
using FoodJournal.ViewModels;
using FoodJournal.Values;

namespace FoodJournal.Android15.Activities
{
    [Android.App.Activity(Label = "ViewDayActivity")]
    public class ViewDayActivity : AppCompatActivity
    {
        public DateTime Date { get; set; }
        public string DateText { get; set; }
        public Period DefaultPeriod { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var data = Intent.Extras;

            if (data != null && data.ContainsKey("date"))
            {
                Date = DateTime.Parse(data.GetString("date"));
            }
            if (data != null && data.ContainsKey("dateText"))
            {
                DateText = data.GetString("dateText");
            }
            if (data != null && data.ContainsKey("period"))
            {
                DefaultPeriod = (Period)data.GetInt("period");
            }
            if (DateText == null) return;

            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            var actionBar = this.SupportActionBar;
            actionBar.Title = DateText;
            actionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetTitle(Resource.String.Today);
            SupportFragmentManager.BeginTransaction()
                           .Replace(Resource.Id.cab_stub, new TodayViewFragment(Date, DateText,DefaultPeriod))
                           .Commit();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Finish();
            return base.OnOptionsItemSelected(item);
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }
        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
        }

    }
}