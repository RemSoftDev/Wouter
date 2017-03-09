
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FoodJournal.Android15
{
	[Activity (Label = "Trial")]			
	public class Trial : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView(Resource.Layout.trial);
			Button start = FindViewById<Button>(Resource.Id.start);
			start.Click += delegate {
				Finish();
			};
		}
	}
}

